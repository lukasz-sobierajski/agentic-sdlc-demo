// Bramki — sprawdzenia, które NIE są modelem językowym.
// Agent może się pomylić albo napisać „zrobione”; te sprawdzenia tego nie przyjmą.
// Jeden program obsługuje i CI (tasks / paths / pr), i hooki agenta (hook-edit / hook-stop),
// żeby lista chronionych ścieżek i reguły istniały w JEDNYM miejscu.

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

Console.OutputEncoding = Encoding.UTF8;

return args.Length == 0 ? Usage() : args[0] switch
{
    "tasks" => Tasks(args.Length > 1 ? args[1] : ""),
    "paths" => Paths(args.Length > 1 ? args[1] : "origin/main"),
    "pr" => PrDescription(args.Length > 1 ? File.ReadAllText(args[1]) : Environment.GetEnvironmentVariable("PR_BODY") ?? ""),
    "hook-edit" => HookEdit(),
    "hook-stop" => HookStop(),
    _ => Usage()
};

static int Usage()
{
    Console.Error.WriteLine("Użycie: Gates tasks [gałąź] | paths [baza] | pr [plik] | hook-edit | hook-stop");
    return 64;
}

// ---------------------------------------------------------------------------------------------
// B3 — każde kryterium akceptacji zadania ma test, który je wskazuje po identyfikatorze.
// Gałąź agenta (agent/0001-…) sprawdza SWOJE zadanie; każda inna — wszystkie zadania już podjęte.
// ---------------------------------------------------------------------------------------------
static int Tasks(string branch)
{
    string root = Root();
    string tests = string.Join("\n", Directory.EnumerateFiles(Path.Combine(root, "tests"), "*.cs", SearchOption.AllDirectories)
        .Where(f => !IsBuildOutput(f)).Select(File.ReadAllText));

    Match fromBranch = Regex.Match(branch, @"^agent/(\d{4})\b");
    List<string> ids = fromBranch.Success
        ? [fromBranch.Groups[1].Value]
        : Regex.Matches(tests, @"""(\d{4})-K\d+""").Select(m => m.Groups[1].Value).Distinct().Order().ToList();

    var problems = new List<string>();
    foreach (string id in ids)
    {
        string? file = Directory.EnumerateFiles(Path.Combine(root, "tasks"), id + "-*.md").FirstOrDefault();
        if (file == null) { problems.Add($"zadanie {id}: brak pliku tasks/{id}-*.md — agent nie pracuje bez spisanego zadania"); continue; }

        string text = File.ReadAllText(file);
        foreach (string heading in new[] { "## Cel", "## Kryteria akceptacji", "## Poza zakresem" })
            if (!text.Contains(heading)) problems.Add($"zadanie {id}: brak sekcji „{heading}”");

        List<string> criteria = Regex.Matches(text, @"^- \*\*(K\d+)\*\*", RegexOptions.Multiline).Select(m => m.Groups[1].Value).ToList();
        if (criteria.Count == 0) problems.Add($"zadanie {id}: brak kryteriów w postaci „- **K1** …”");
        foreach (string k in criteria)
        {
            bool covered = tests.Contains($"\"{id}-{k}\"");
            Console.WriteLine($"  {(covered ? "✓" : "✗")} {id}-{k}");
            if (!covered) problems.Add($"zadanie {id}: kryterium {k} nie ma testu oznaczonego \"{id}-{k}\"");
        }
    }
    return Verdict("B3 kryteria zadania mają testy", problems);
}

// ---------------------------------------------------------------------------------------------
// B4 — agent nie zmienia tego, co go sprawdza: standardów, bramek, potoku ani treści zadania.
// ---------------------------------------------------------------------------------------------
static int Paths(string baseRef)
{
    (int code, string output) = Run("git", $"diff --name-only {baseRef}...HEAD", Root());
    if (code != 0) return Verdict("B4 agent nie zmienia własnych bramek", [$"git diff względem {baseRef} nie powiódł się: {output.Trim()}"]);

    List<string> touched = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(IsProtected).ToList();
    return Verdict("B4 agent nie zmienia własnych bramek",
        touched.Select(p => $"zmieniono chronioną ścieżkę: {p} — taką zmianę wnosi człowiek osobnym PR-em").ToList());
}

// ---------------------------------------------------------------------------------------------
// B5 — opis PR mówi, co zrobiono, JAK to sprawdzono i czego autor nie jest pewien.
// ---------------------------------------------------------------------------------------------
static int PrDescription(string body)
{
    var problems = new List<string>();
    string clean = Regex.Replace(body, @"<!--.*?-->", "", RegexOptions.Singleline);
    foreach (string heading in new[] { "## Zadanie", "## Co zmieniłem", "## Jak sprawdziłem", "## Czego nie jestem pewien" })
    {
        Match section = Regex.Match(clean, Regex.Escape(heading) + @"\s*\n(.*?)(?=\n## |\z)", RegexOptions.Singleline);
        if (!section.Success) problems.Add($"brak sekcji „{heading}”");
        else if (section.Groups[1].Value.Count(c => !char.IsWhiteSpace(c)) < 10) problems.Add($"sekcja „{heading}” jest pusta");
    }
    if (!Regex.IsMatch(clean, @"tasks/\d{4}-")) problems.Add("opis nie wskazuje pliku zadania (tasks/NNNN-…)");
    return Verdict("B5 opis PR", problems);
}

// ---------------------------------------------------------------------------------------------
// Hook PreToolUse — ta sama lista chronionych ścieżek co B4, ale ZANIM agent zapisze plik.
// Kod wyjścia 2 = narzędzie agenta odmawia i przekazuje mu komunikat z stderr.
// ---------------------------------------------------------------------------------------------
static int HookEdit()
{
    using JsonDocument input = JsonDocument.Parse(Console.In.ReadToEnd());
    if (!input.RootElement.TryGetProperty("tool_input", out JsonElement toolInput) ||
        !toolInput.TryGetProperty("file_path", out JsonElement filePath)) return 0;

    string relative = Path.GetRelativePath(Root(), Path.GetFullPath(filePath.GetString() ?? "")).Replace('\\', '/');
    if (!IsProtected(relative)) return 0;

    Console.Error.WriteLine($"Odmowa: {relative} jest ścieżką chronioną (standardy, bramki, potok, treść zadania). " +
                            "Jeśli zadania nie da się wykonać bez tej zmiany, napisz to w sekcji „Czego nie jestem pewien” opisu PR.");
    return 2;
}

// ---------------------------------------------------------------------------------------------
// Hook Stop — agent nie kończy tury z kodem, który się nie buduje albo nie przechodzi testów.
// ---------------------------------------------------------------------------------------------
static int HookStop()
{
    using JsonDocument input = JsonDocument.Parse(Console.In.ReadToEnd());
    // Druga próba zakończenia po odmowie: oddajemy głos człowiekowi zamiast kręcić pętlę.
    if (input.RootElement.TryGetProperty("stop_hook_active", out JsonElement active) && active.ValueKind == JsonValueKind.True) return 0;

    string testProject = Path.Combine(Root(), "tests", "Accounts.Tests");
    (int build, string buildOutput) = Run("dotnet", "build -warnaserror -nologo -v q", testProject);
    if (build != 0) return Refuse("kompilacja z ostrzeżeniami jako błędami nie przechodzi", buildOutput);

    (int test, string testOutput) = Run("dotnet", "test --no-build -nologo", testProject);
    if (test != 0) return Refuse("testy nie przechodzą", testOutput);

    string branch = Run("git", "branch --show-current", Root()).Output.Trim();
    return Tasks(branch) == 0 ? 0 : Refuse("nie każde kryterium zadania ma test", "uruchom: dotnet run --project tools/Gates -- tasks " + branch);

    static int Refuse(string why, string details)
    {
        Console.Error.WriteLine($"Nie kończ tury: {why}.\n{string.Join('\n', details.Split('\n').TakeLast(25))}");
        return 2;
    }
}

// ---------------------------------------------------------------------------------------------

static bool IsProtected(string relativePath)
{
    string[] protectedPaths = ["AGENTS.md", "CLAUDE.md", "Jenkinsfile", ".github/", ".claude/", "tools/Gates/", "tasks/", "docs/reviewer-prompt.md"];
    return protectedPaths.Any(p => p.EndsWith('/') ? relativePath.StartsWith(p, StringComparison.Ordinal) : relativePath == p);
}

static bool IsBuildOutput(string path) =>
    path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") ||
    path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}");

static int Verdict(string gate, List<string> problems)
{
    if (problems.Count == 0) { Console.WriteLine($"BRAMKA {gate} — przeszła"); return 0; }
    Console.Error.WriteLine($"BRAMKA {gate} — ODRZUCONE ({problems.Count}):");
    foreach (string p in problems) Console.Error.WriteLine("  • " + p);
    return 1;
}

static string Root()
{
    for (DirectoryInfo? dir = new(Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
        if (File.Exists(Path.Combine(dir.FullName, "AGENTS.md"))) return dir.FullName;
    throw new InvalidOperationException("Nie znaleziono korzenia repozytorium (pliku AGENTS.md).");
}

static (int Code, string Output) Run(string file, string arguments, string workingDirectory)
{
    using Process process = Process.Start(new ProcessStartInfo(file, arguments)
    {
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        StandardOutputEncoding = Encoding.UTF8,
        StandardErrorEncoding = Encoding.UTF8
    })!;
    Task<string> error = process.StandardError.ReadToEndAsync();
    string output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();
    return (process.ExitCode, output + error.Result);
}

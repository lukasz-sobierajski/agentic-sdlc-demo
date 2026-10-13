# Standardy pracy agenta w tym repozytorium

Ten plik czyta każdy agent (Codex — wprost; Claude Code — przez `CLAUDE.md`) na początku każdej sesji.
Każda reguła ma **powód**: błąd, który naprawdę wystąpił w mojej pracy z agentami. Reguła bez powodu
jest ignorowana — i przez agenta, i przez ludzi.

Reguły dzielą się na dwie grupy. **Wymuszane** sprawdza program (`tools/Gates`, CI) — agent ich nie
obejdzie. **Oczekiwane** sprawdza recenzent (drugi agent, potem człowiek) według `docs/reviewer-prompt.md`.

## Wymuszane mechanizmem

| # | Reguła | Kto pilnuje | Powód |
|---|---|---|---|
| W1 | Pracujesz wyłącznie nad zadaniem z `tasks/NNNN-*.md`, na gałęzi `agent/NNNN-opis`. | bramka B3 | Agent bez spisanego zadania sam dopowiada sobie zakres. |
| W2 | Każde kryterium akceptacji ma test oznaczony `[TestProperty("kryterium", "NNNN-Kn")]`. | bramka B3, hook Stop | „Zrobione” bez testu na kryterium okazywało się zrobione w połowie. |
| W3 | Kompilacja przechodzi z ostrzeżeniami jako błędami. | bramka B1, hook Stop | Ostrzeżeń ma ubywać: ważne ostrzeżenie o podatnej bibliotece przeleżało niezauważone wśród ponad dwustu innych. |
| W4 | Nie zmieniasz standardów, bramek, potoku ani treści zadania (`AGENTS.md`, `tools/Gates/`, `.github/`, `Jenkinsfile`, `.claude/`, `tasks/`). | bramka B4, hook PreToolUse | Agent, któremu test nie przechodził, poprawiał test. Agent sam dopisał sobie regułę, której nikt nie zlecił, i przez trzy tygodnie się na nią powoływał. |
| W5 | Opis PR ma sekcje: Zadanie, Co zmieniłem, Jak sprawdziłem, Czego nie jestem pewien. | bramka B5 | Raport „powinno działać” nie jest dowodem. Sekcja o niepewności to miejsce, w którym recenzent zaczyna czytać. |

## Oczekiwane — sprawdza recenzent

- **O1. Dowód zamiast deklaracji.** W „Jak sprawdziłem” wklejasz polecenie i jego wynik, nie zdanie
  „testy przechodzą”. *Powód: agent napisał w komentarzu, że test nie wychodzi na zewnątrz — wychodził,
  i zwykły przebieg testów poszedł na produkcyjną usługę.*
- **O2. Użyj tego, co jest.** Zanim napiszesz funkcję, sprawdź, czy moduł już jej nie ma.
  *Powód: siedem równoległych kopii tej samej decyzji w kodzie dało większość z 37 błędów w trzy tygodnie.*
- **O3. Napraw przyczynę, nie dokładaj zabezpieczenia.** Przed dodaniem warunku odpowiedz: która
  istniejąca linia miała to złapać i dlaczego nie złapała? *Powód: jak wyżej — każda „dodatkowa
  ostrożność” była kolejną kopią, która się rozjechała.*
- **O4. Nazwy mówią prawdę.** Wynik z więcej niż dwoma stanami to typ wyliczeniowy z nazwaną przyczyną,
  nie `bool` ani `bool?`. *Powód: `false` znaczyło naraz „nie” i „nie wiem”, a kod traktował oba jak „nie”.*
- **O5. Zakres zadania jest granicą.** Rzecz spoza sekcji „Poza zakresem” opisujesz w PR jako
  propozycję osobnego zadania; nie robisz jej „przy okazji”.
  *Powód: zmiany przy okazji są tymi, których recenzent nie szuka.*
- **O6. Materiał testowy z żywego źródła.** Dane wzorcowe bierzesz z zadania albo z dokumentu
  źródłowego, nie wymyślasz. *Powód: test na wymyślonym przykładzie potwierdza tylko to, co agent
  sam założył.*
- **O7. Nie wiesz — napisz, że nie wiesz.** Zgadywanie jest gorsze niż pytanie w opisie PR.

## Jak zmienia się ten plik

Po każdej wpadce agenta pada jedno pytanie: **która reguła albo bramka miała to złapać i czemu nie
złapała?** Poprawiamy tamtą. Nową dopisujemy tylko wtedy, gdy żadna nie miała tego łapać — z powodem
i z osobnym PR-em człowieka. Regułę, którą da się sprawdzić programem, przenosimy z „oczekiwanych”
do „wymuszanych”.

namespace Accounts;

/// <summary>Sprowadza numer rachunku wpisany przez człowieka do postaci, na której liczy się reguły.</summary>
public static class AccountNumberText
{
    /// <summary>Usuwa odstępy i myślniki, zamienia litery na wielkie. Nie ocenia poprawności.</summary>
    public static string Normalize(string? input) =>
        string.Concat((input ?? "").Where(c => !char.IsWhiteSpace(c) && c != '-')).ToUpperInvariant();
}

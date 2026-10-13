namespace Accounts;

/// <summary>Dlaczego napis nie jest poprawnym polskim numerem rachunku.</summary>
public enum NrbRejection
{
    None,
    Empty,
    UnsupportedCountry,
    IllegalCharacter,
    WrongLength,
    ChecksumMismatch
}

/// <summary>Wynik walidacji: przyczyna odrzucenia albo numer w postaci 26 cyfr i numer rozliczeniowy banku.</summary>
public readonly record struct NrbResult(NrbRejection Rejection, string Digits, string BankCode)
{
    public bool IsValid => Rejection == NrbRejection.None;

    internal static NrbResult Rejected(NrbRejection reason) => new(reason, "", "");
}

/// <summary>NRB — Numer Rachunku Bankowego: 2 cyfry kontrolne + 8 cyfr numeru rozliczeniowego + 16 cyfr rachunku.</summary>
public static class Nrb
{
    private const int Length = 26;
    private const string CountryAsDigits = "2521"; // P = 25, L = 21 w algorytmie IBAN

    public static NrbResult Validate(string? input)
    {
        string text = AccountNumberText.Normalize(input);
        if (text.Length == 0) return NrbResult.Rejected(NrbRejection.Empty);

        if (text.Length >= 2 && char.IsLetter(text[0]) && char.IsLetter(text[1]))
        {
            if (!text.StartsWith("PL", StringComparison.Ordinal)) return NrbResult.Rejected(NrbRejection.UnsupportedCountry);
            text = text[2..];
        }

        if (!text.All(char.IsAsciiDigit)) return NrbResult.Rejected(NrbRejection.IllegalCharacter);
        if (text.Length != Length) return NrbResult.Rejected(NrbRejection.WrongLength);
        if (Mod97(text[2..] + CountryAsDigits + text[..2]) != 1) return NrbResult.Rejected(NrbRejection.ChecksumMismatch);

        return new NrbResult(NrbRejection.None, text, text.Substring(2, 8));
    }

    private static int Mod97(string digits)
    {
        int remainder = 0;
        foreach (char c in digits) remainder = (remainder * 10 + (c - '0')) % 97;
        return remainder;
    }
}

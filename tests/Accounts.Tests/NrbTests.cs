using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Accounts.Tests;

/// <summary>Zadanie tasks/0001-walidacja-nrb.md. Numer wzorcowy pochodzi z treści zadania.</summary>
[TestClass]
public class NrbTests
{
    private const string Reference = "61109010140000071219812874";

    [TestMethod]
    [TestProperty("kryterium", "0001-K1")]
    [DataRow("61109010140000071219812874")]
    [DataRow("61 1090 1014 0000 0712 1981 2874")]
    [DataRow("61-1090-1014-0000-0712-1981-2874")]
    [DataRow("PL61 1090 1014 0000 0712 1981 2874")]
    [DataRow(" pl61109010140000071219812874 ")]
    public void Spaces_hyphens_and_PL_prefix_are_accepted(string input)
    {
        NrbResult result = Nrb.Validate(input);

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(Reference, result.Digits);
    }

    [TestMethod]
    [TestProperty("kryterium", "0001-K2")]
    [DataRow("6110901014000007121981287", NrbRejection.WrongLength)]
    [DataRow("611090101400000712198128740", NrbRejection.WrongLength)]
    [DataRow("PL61", NrbRejection.WrongLength)]
    [DataRow("6110901014000007121981287X", NrbRejection.IllegalCharacter)]
    [DataRow("61.1090.1014.0000.0712.1981.2874", NrbRejection.IllegalCharacter)]
    public void Wrong_length_and_illegal_characters_are_named(string input, NrbRejection expected)
    {
        Assert.AreEqual(expected, Nrb.Validate(input).Rejection);
    }

    [TestMethod]
    [TestProperty("kryterium", "0001-K3")]
    [DataRow("62109010140000071219812874")] // zmieniona cyfra kontrolna
    [DataRow("61109010140000071219812875")] // zmieniona ostatnia cyfra rachunku
    [DataRow("61109001140000071219812874")] // przestawione sąsiednie cyfry
    public void Checksum_mismatch_is_detected(string input)
    {
        Assert.AreEqual(NrbRejection.ChecksumMismatch, Nrb.Validate(input).Rejection);
    }

    [TestMethod]
    [TestProperty("kryterium", "0001-K4")]
    public void Every_rejection_has_its_own_reason_and_no_data()
    {
        NrbResult[] rejected =
        [
            Nrb.Validate(""),
            Nrb.Validate("DE61109010140000071219812874"),
            Nrb.Validate("6110901014000007121981287X"),
            Nrb.Validate("61"),
            Nrb.Validate("62109010140000071219812874")
        ];

        CollectionAssert.AllItemsAreUnique(rejected.Select(r => r.Rejection).ToArray());
        foreach (NrbResult r in rejected)
        {
            Assert.IsFalse(r.IsValid);
            Assert.AreNotEqual(NrbRejection.None, r.Rejection);
            Assert.AreEqual("", r.Digits);
            Assert.AreEqual("", r.BankCode);
        }
    }

    [TestMethod]
    [TestProperty("kryterium", "0001-K6")]
    [DataRow(null, NrbRejection.Empty)]
    [DataRow("", NrbRejection.Empty)]
    [DataRow("  - ", NrbRejection.Empty)]
    [DataRow("DE61109010140000071219812874", NrbRejection.UnsupportedCountry)]
    [DataRow("de61 1090 1014 0000 0712 1981 2874", NrbRejection.UnsupportedCountry)]
    public void Empty_input_and_foreign_country_are_named(string? input, NrbRejection expected)
    {
        Assert.AreEqual(expected, Nrb.Validate(input).Rejection);
    }

    [TestMethod]
    [TestProperty("kryterium", "0001-K5")]
    public void Valid_number_exposes_eight_digit_bank_code_from_positions_3_to_10()
    {
        NrbResult result = Nrb.Validate("PL61 1090 1014 0000 0712 1981 2874");

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual("10901014", result.BankCode);
        Assert.AreEqual(result.Digits.Substring(2, 8), result.BankCode);
    }
}

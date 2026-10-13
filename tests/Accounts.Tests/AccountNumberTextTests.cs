using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Accounts.Tests;

[TestClass]
public class AccountNumberTextTests
{
    [TestMethod]
    public void Normalize_removes_spaces_and_hyphens_and_uppercases()
    {
        Assert.AreEqual("PL61109010140000071219812874", AccountNumberText.Normalize(" pl61 1090-1014 0000 0712 1981 2874 "));
    }

    [TestMethod]
    public void Normalize_of_null_is_empty()
    {
        Assert.AreEqual("", AccountNumberText.Normalize(null));
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tellurian.Trains.Planning.App.Contracts.Extensions;


namespace Tellurian.Trains.Planning.App.Contract.Tests;

[TestClass]
public class StringExtensionsTest
{
    [TestMethod]
    public void IsNotAnyPartOf()
    {
        Assert.IsFalse("Erik".AnyPartOf("Kalle, Anka"));
    }
    [TestMethod]
    public void IsAnyPartOf()
    {
        Assert.IsTrue("anka".AnyPartOf("Kalle, Anka"));
    }
}

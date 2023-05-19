using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Contracts.Extensions;

namespace Tellurian.Trains.Planning.App.Server.Tests;

[TestClass]
public class PaginationTests
{
    [TestMethod]
    public void TotalPages()
    {
        var target = DriverDutyBooklet.Example;
        Assert.AreEqual(1, target.Duties.TotalPages(1));
    }

    [TestMethod]
    public void GetDriverDutyPages()
    {
        var target = DriverDutyBooklet.Example;
        var pages = target.Duties.First().GetDriverDutyPages(Array.Empty<Instruction>());
        Assert.AreEqual(4, pages.Count());
    }

    [TestMethod]
    public void GetDriverDutyPagesInBookletOrder()
    {
        var target = DriverDutyBooklet.Example;
        var pages = target.Duties.First().GetDriverDutyPagesInBookletOrder(Array.Empty<Instruction>());
        Assert.AreEqual(4, pages.Count());
    }
}

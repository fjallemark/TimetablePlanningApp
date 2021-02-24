using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Tellurian.Trains.Planning.App.Contract;

namespace Tellurian.Trains.Planning.App.Server.Tests
{
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
            var pages = target.Duties.First().GetDriverDutyPages(Array.Empty<LayoutInstruction>());
            Assert.AreEqual(4, pages.Count());
        }

        [TestMethod]
        public void GetDriverDutyPagesInBookletOrder()
        {
            var target = DriverDutyBooklet.Example;
            var pages = target.Duties.First().GetDriverDutyPagesInBookletOrder(Array.Empty<LayoutInstruction>());
            Assert.AreEqual(4, pages.Count());
        }
    }
}

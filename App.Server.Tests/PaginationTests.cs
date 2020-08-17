using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Tellurian.Trains.Planning.App.Shared;

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
            var pages = target.Duties.First().GetDriverDutyPages();
            Assert.AreEqual(4, pages.Count());
        }

        [TestMethod]
        public void GetDriverDutyPagesInBookletOrder()
        {
            var target = DriverDutyBooklet.Example;
            var pages = target.Duties.First().GetDriverDutyPagesInBookletOrder();
            Assert.AreEqual(4, pages.Count());
        }

        [TestMethod]
        public void GetDriverDutyPagesInBookletOrderFromRealResponse()
        {
            var json = File.ReadAllText("Testdata\\response.json");
            var response = JsonSerializer.Deserialize<DriverDutyBooklet>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true});
            var actual = response.Duties.GetAllDriverDutyPagesInBookletOrder();

            Assert.IsNotNull(actual);
        }
    }
}

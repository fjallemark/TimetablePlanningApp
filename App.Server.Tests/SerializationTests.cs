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
    public class SerializationTests
    {
        [TestMethod]
        public void DriverDutyBookletTests()
        {
            var target = DriverDutyBooklet.Example;
            Assert.IsNotNull(target.Duties.First().Parts.First().Train.Calls, "Missing in target");

            var json = JsonSerializer.Serialize(target);
            var actual = JsonSerializer.Deserialize<DriverDutyBooklet>(json, new JsonSerializerOptions {  MaxDepth=20});
            Assert.IsNotNull(actual.Duties.First().Parts.First().Train.Calls, "Missing in actual");
            Assert.AreEqual(target.Duties.First().Parts.First().Train.Calls.Count, actual.Duties.First().Parts.First().Train.Calls.Count);
        }

        [TestMethod]
        public void TrainSerialization()
        {
            var target = DriverDutyBookletExtensions.Train51;
            var json = JsonSerializer.Serialize(target);
            var actual = JsonSerializer.Deserialize<Train>(json, new JsonSerializerOptions { MaxDepth = 20 });
            Assert.AreEqual(target.Calls.Count, actual.Calls.Count);
        }

        [TestMethod]
        public void MyTestMethod()
        {
            var json = File.ReadAllText("Testdata\\response.json");
            var response = JsonSerializer.Deserialize<DriverDutyBooklet>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive=true, MaxDepth = 20 });
            Assert.IsTrue(response.Duties.Count > 0);
        }
    }
}

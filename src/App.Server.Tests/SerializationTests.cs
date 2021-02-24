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
    public class SerializationTests
    {
        [TestMethod]
        public void DriverDutyBookletTests()
        {
            var target = DriverDutyBooklet.Example;
            Assert.IsNotNull(target.Duties.First().Parts.First().Train.Calls, "Missing in target");

            var json = JsonSerializer.Serialize(target);
            var actual = JsonSerializer.Deserialize<DriverDutyBooklet>(json, new JsonSerializerOptions {  MaxDepth=20});
            Assert.IsNotNull(actual?.Duties.First().Parts.First().Train.Calls, "Missing in actual");
            Assert.AreEqual(target.Duties.First().Parts.First().Train.Calls.Count, actual?.Duties.First().Parts.First().Train.Calls.Count);
        }

        [TestMethod]
        public void TrainSerialization()
        {
            var target = Train.Example;
            var json = JsonSerializer.Serialize(target);
            var actual = JsonSerializer.Deserialize<Train>(json, new JsonSerializerOptions { MaxDepth = 20 });
            Assert.AreEqual(target.Calls.Count, actual?.Calls.Count);
        }
    }
}

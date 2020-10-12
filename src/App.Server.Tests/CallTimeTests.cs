using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Tellurian.Trains.Planning.App.Contract;

namespace Tellurian.Trains.Planning.App.Server.Tests
{
    [TestClass]
    public class CallTimeTests
    {
        [TestMethod]
        public void ParsesTime()
        {
            var target = new CallTime { Time = "18:30" };
            Assert.AreEqual(18.5 * 60, target.OffsetMinutes());
        }
    }
}

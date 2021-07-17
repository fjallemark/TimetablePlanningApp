using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tellurian.Trains.Planning.App.Contracts;

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

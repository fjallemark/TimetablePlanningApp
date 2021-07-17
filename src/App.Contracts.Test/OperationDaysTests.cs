using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Contract.Tests
{
    [TestClass]
    public class OperationDaysTests
    {
        [TestMethod]
        public void IsAnyOtherDays1()
        {
            const byte days = 0x03;
            Assert.IsTrue(days.IsAnyOtherDays(0x01));
        }

        [TestMethod]
        public void IsAnyOtherDays2()
        {
            const byte days = 0x03;
            Assert.IsTrue(days.IsAnyOtherDays(0x05));
        }

        [TestMethod]
        public void IsAllOtherDays1()
        {
            const byte days = 0x03;
            Assert.IsTrue(days.IsAllOtherDays(0x03));
        }

        [TestMethod]
        public void IsAllOtherDays2()
        {
            const byte days = 0x03;
            Assert.IsFalse(days.IsAllOtherDays(0x02));
        }

        [TestMethod]
        public void IsAllOtherDays3()
        {
            const byte days = 0x03;
            Assert.IsTrue(days.IsAllOtherDays(0x07));
        }
    }
}

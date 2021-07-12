using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tellurian.Trains.Planning.App.Contract;

namespace Tellurian.Trains.Planning.App.Server.Tests
{
    [TestClass]
    public class StationCallExtensionsTests
    {
        [TestMethod]
        public void ShorterSimilarTextIsNotAdded()
        {
            var target = CreateStationCall;
            target.AddArrivalNote(new Note { Text = "Detta är en text som är längre." });
            Assert.IsFalse(target.AddArrivalNote(new Note { Text = "Detta är en text." }));
        }

        [TestMethod]
        public void LongerSimilarTextIsUpdatingExistingNote()
        {
            var target = CreateStationCall;
            target.AddArrivalNote(new Note { Text = "Detta är en text." });
            Assert.IsTrue(target.AddArrivalNote(new Note { Text = "Detta är en text som är längre." }));
            Assert.AreEqual("Detta är en text som är längre.", target.Arrival?.Notes[0].Text);
        }

        private static StationCall CreateStationCall =>
            new StationCall { Arrival = new CallTime(), Departure = new CallTime() };

    }
}

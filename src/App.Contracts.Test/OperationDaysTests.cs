using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

		[TestMethod]
		[Ignore("Not ready for test.")]
		public void IsTwoDaygroups()
		{
			var dayFlags = new byte[] { 0b00011111, 0b01100000 };
			var expected = new byte[] { 0b00011111, 0b01100000 };
			CollectionAssert.AreEquivalent(expected, dayFlags.UniqueDayCombinations());
		}

		[TestMethod]
		[Ignore("Not ready for test.")]
		public void IsTwoDaygroupsFromThreeDayCombinations()
		{
			var dayFlags = new byte[] { 0b00000001, 0b00000010, 0b00000011 };
			var expected = new byte[] { 0b00000001, 0b00000010 };
			CollectionAssert.AreEquivalent(expected, dayFlags.UniqueDayCombinations());
		}

		[Ignore("Not ready for test.")]
		[TestMethod]
		public void IsThreeDaygroupsFromFourDayCombinations()
		{
			var dayFlags = new byte[] { 0b00000001, 0b00000010, 0b00000011, 0b00000111 };
			var expected = new byte[] { 0b00000001, 0b00000010, 0b00000100 };
			CollectionAssert.AreEquivalent(expected, dayFlags.UniqueDayCombinations());
		}
        [Ignore("Not ready for test.")]
		[TestMethod]
		public void IsFourDaygroupsFromFourDayCombinations()
		{
			var dayFlags = new byte[] { 0b00000111, 0b00001110, 0b00011110, 0b00001111 };
			var expected = new byte[] { 0b00000001, 0b00000110, 0b00001000, 0b00010000 };
			CollectionAssert.AreEquivalent(expected, dayFlags.UniqueDayCombinations());
		}

		[TestMethod]
		public void IsBitSet()
		{
			ReadOnlySpan<byte> values = new byte[] {1,2,4,8,16,32,64,128};
			for(var i =0; i < 8; i++) Assert.IsTrue(values[i].IsBitSet(i+1));
		}

		[TestMethod]
		public void ZeroBit()
		{
			ReadOnlySpan<byte> values = new byte[] { 1, 2, 4, 8, 16, 32, 64, 128 };
			for (var i = 0; i < 8; i++) Assert.AreEqual(0, values[i].ZeroBit(i+1) );
		}
	}
}

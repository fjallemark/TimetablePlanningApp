﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Server.Tests;

[TestClass]
public class OperationDayTests
{
    [TestInitialize]
    public void TestInitialize()
    {
    }

    [TestMethod]
    public void DailyEnglish()
    {
        English();
        const byte target = 0x7F;
        var actual = target.OperationDays();
        Assert.AreEqual("Daily", actual.FullName);
        Assert.AreEqual("Daily", actual.ShortName);
    }

    [TestMethod]
    public void OnDemandEnglish()
    {
        English();
        const byte target = 0x80;
        var actual = target.OperationDays();
        Assert.AreEqual("On demand", actual.FullName);
        Assert.AreEqual("Demand", actual.ShortName);
    }

    [TestMethod]
    public void DailySwedish()
    {
        Swedish();
        const byte target = 0x7F;
        var actual = target.OperationDays();
        Assert.AreEqual("Dagligen", actual.FullName);
        Assert.AreEqual("Dagl", actual.ShortName);
    }

    [TestMethod]
    public void MondayToFridaySwedish()
    {
        Swedish();
        const byte target = 0x1F;
        var actual = target.OperationDays();
        Assert.AreEqual("Måndag till fredag", actual.FullName);
        Assert.AreEqual("M-F", actual.ShortName);
    }

    [TestMethod]
    public void MondaySaturdayEnglish()
    {
        English();
        const byte target = 63;
        var actual = target.OperationDays();
        Assert.AreEqual("Monday to Saturday", actual.FullName);
        Assert.AreEqual("Mo-Sa", actual.ShortName);
    }

    [TestMethod]
    public void MondayWendnesdayFridaySwedish()
    {
        Swedish();
        const byte target = 0x15;
        var actual = target.OperationDays();
        Assert.AreEqual("Måndag, onsdag och fredag", actual.FullName);
        Assert.AreEqual("M,O,F", actual.ShortName);
        Assert.AreEqual("Måndag", target.FirstOperationDay().FullName);
    }

    [TestMethod]
    public void MondayToFridayAndSundaySwedish()
    {
        Swedish();
        const byte target = 95;
        var actual = target.OperationDays();
        Assert.AreEqual("Söndag till fredag", actual.FullName);
        Assert.AreEqual("S-F", actual.ShortName);
    }
    [TestMethod]
    public void MondayThursdayAndSundaySwedish()
    {
        Swedish();
        const byte target = 73;
        var actual = target.OperationDays();
        Assert.AreEqual("Måndag, torsdag och söndag", actual.FullName);
        Assert.AreEqual("M,To,S", actual.ShortName);
        Assert.AreEqual("Måndag", target.FirstOperationDay().FullName);
    }

    [TestMethod]
    public void FirstOperationDayFlagIsCorrect()
    {
        Assert.AreEqual((byte)1, ((byte)73).FirstOperationDay().Flags);
        Assert.AreEqual((byte)2, ((byte)18).FirstOperationDay().Flags);
        Assert.AreEqual((byte)4, ((byte)36).FirstOperationDay().Flags);
    }

    private static void English()
    {
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en");
        Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
    }
    private static void Swedish()
    {
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("sv");
        Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("sv");
    }
}

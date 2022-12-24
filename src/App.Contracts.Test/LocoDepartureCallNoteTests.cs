using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Contracts.Extensions;

namespace Tellurian.Trains.Planning.App.Contract.Tests;

[TestClass]
public class LocoDepartureCallNoteTests
{
    [TestMethod]
    public void DaysAreAggregated()
    {
        var notes = new[]
        {
            new LocoDepartureCallNote(1)
            {
                DepartingLoco = new Loco() { TurnusNumber = 40, OperationDaysFlags = 0b_00000111 },
            },
            new LocoDepartureCallNote(1)
            {
                DepartingLoco = new Loco() { TurnusNumber = 40, OperationDaysFlags = 0b_01111000 }
            },
            new LocoDepartureCallNote(2)
            {
                DepartingLoco = new Loco() { TurnusNumber = 40, OperationDaysFlags = 0b_01111000 }
            }
        };

        var n = notes.AggregateOperationDays().ToArray();
        Assert.AreEqual(2, n.Length);
        Assert.AreEqual(0b_01111111, n[0].DepartingLoco.OperationDaysFlags);
    }
}


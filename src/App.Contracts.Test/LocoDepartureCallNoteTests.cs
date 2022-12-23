using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Tellurian.Trains.Planning.App.Contracts;

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

        var n = notes.Aggregated().ToArray();
        Assert.AreEqual(2, n.Length);
        Assert.AreEqual(0b_01111111, n[0].DepartingLoco.OperationDaysFlags);
    }
}

public static class LocoDepartureCallNoteExtensions
{

    public static void Aggregate(this List<LocoDepartureCallNote> result, LocoDepartureCallNote n1, LocoDepartureCallNote n2)
    {
        if (n1.CallId == n2.CallId && n1.DepartingLoco.TurnusNumber == n2.DepartingLoco.TurnusNumber)
        {
            n1.DepartingLoco.OperationDaysFlags |= n2.DepartingLoco.OperationDaysFlags;
            result.Add(n1);
        }
        else
        {
            result.Add(n1);
        }
    }

    public static IEnumerable<LocoDepartureCallNote> Aggregated(this LocoDepartureCallNote[] notes)
    {
        if (notes.Length == 0) return notes;
        var result = new List<LocoDepartureCallNote>(notes.Length)
        {
            notes[0]
        };
        for (int i = 1; i < notes.Length; i++)       
        {
            if (result.Last().CallId == notes[i].CallId && result.Last().DepartingLoco.TurnusNumber == notes[i].DepartingLoco.TurnusNumber)
            {
                result.Last().DepartingLoco.OperationDaysFlags |= notes[i].DepartingLoco.OperationDaysFlags;
            }
            else
            {
                result.Add(notes[i]);
            }
        }
        return result;
    }
}




﻿using System;
using System.Collections.Generic;

namespace Tellurian.Trains.Planning.App.Contracts
{
    public sealed class DriverDutyBooklet
    {
        public string ScheduleName { get; set; } = string.Empty;
        public IEnumerable<LayoutInstruction> Instructions { get; set; } = new List<LayoutInstruction>();
        public ICollection<DriverDuty> Duties { get; set; } = Array.Empty<DriverDuty>();

        public static DriverDutyBooklet Example => new DriverDutyBooklet
        {
            ScheduleName = "Demo",
            Duties = new[]
            {
                new DriverDuty
                {
                    Operator = "Green Cargo",
                         OperationDays = ((byte)31).OperationDays(),
                         Difficulty = 2,
                         Description = "Chemicals transport",
                         Number=22,
                         StartTime = "11:40",
                         EndTime = "15:38",
                         Parts = new [] {
                             new DutyPart(Train.Example, new Loco {  OperatorName="GC", Number=52}, 22, 27)
                         }
                }
            }
        };
    }
}

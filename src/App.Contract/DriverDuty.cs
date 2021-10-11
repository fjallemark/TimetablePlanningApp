﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.App.Contracts
{
    public class DriverDuty
    {
        public string LayoutName { get; set; } = string.Empty;
        public DateTime ValidFromDate { get; set; }
        public DateTime ValidToDate { get; set; }

        public string Operator { get; set; } = string.Empty;
        public int Number { get; set; }
        public string Description { get; set; } = string.Empty;
        public OperationDays OperationDays { get; set; } = new OperationDays();
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int Difficulty { get; set; } = 1;
        public int RemoveOrder { get; set; }
        public ICollection<DutyPart> Parts { get; set; } = new List<DutyPart>();
    }

    public static class DriverDutyExtensions
    {
        public static string? StartTime(this DriverDuty me) => me.StartTime.HasValue() ? me.StartTime : me.Parts.OrderBy(p => p.StartTime()).First().StartTime();
        public static string? EndTime(this DriverDuty me) => me.EndTime.HasValue() ? me.EndTime : me.Parts.OrderBy(p => p.StartTime()).Last().EndTime();
        public static string TrainOperatingDay(this DriverDuty me, Train train) => 
            me is null ? "" : 
            me.OperationDays.Flags.IsAllOtherDays(train.OperationDaysFlags) ? "" : 
            train.OperationDaysFlags.OperationDays().ShortName;

        public static string TrainTypes(this DriverDuty me) => string.Join(", ", me.Parts.Select(p => p.Train.CategoryName).Distinct());

        public static string ValidPeriod(this DriverDuty me) =>
            string.Format(CultureInfo.CurrentCulture, Notes.ValidPeriod, me.ValidFromDate, me.ValidToDate);

    }
}

using System.Collections.Generic;
using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    internal static class DutyMapper
    {
        public static DutyBooklet AsDutyBooklet(this IDataRecord me, DutyBooklet it)
        {
            it.ScheduleName = me.GetString("LayoutName");
            it.ValidFromDate = me.GetDate("ValidFromDate");
            it.ValidToDate = me.GetDate("ValidToDate");
            return it;
        }

        public static Instruction AsInstruction(this IDataRecord me, string markdownColumnName) =>
            new()
            {
                Language = me.GetString("Language"),
                Markdown = me.GetString(markdownColumnName)
            };



        public static DriverDuty AsDuty(this IDataRecord me) =>
            new()
            {
                OperationDays = me.GetByte("DutyDays").OperationDays(),
                ValidFromDate = me.GetDate("ValidFromDate"),
                ValidToDate = me.GetDate("ValidToDate"),
                Difficulty = me.GetInt("DutyDifficulty"),
                EndTime = me.GetTime("DutyEndsTime"),
                LayoutName = me.GetString("LayoutName"),
                Description = me.GetString("DutyName"),
                Number = me.GetInt("DutyNumber").ToString(),
                Operator = me.GetString("DutyOperator"),
                RemoveOrder = me.GetInt("DutyRemoveOrder"),
                StartTime = me.GetTime("DutyStartsTime"),
                Parts = new List<DriverDutyPart>()
            };

        public static DriverDutyPart AsDutyPart(this IDataRecord me, Train train) =>
            new()
            {
                Train = train,
                FromCallId = me.GetInt("FromCallId"),
                GetLocoAtParking = me.GetBool("FromParking"),
                ToCallId = me.GetInt("ToCallId"),
                PutLocoAtParking = me.GetBool("ToParking"),
                ReverseLoco = me.GetBool("ReverseLoco"),
                TurnLoco = me.GetBool("TurnLoco")
            };

        public static StationDutyData AsStationDutyData(this IDataRecord me) =>
            new()
            {
                LayoutName = me.GetString("LayoutName"),
                StationId = me.GetInt("StationId"),
                Name = me.GetString("Name"),
                Signature = me.GetString("Signature"),
                Difficulty = me.GetInt("Difficulty"),
                EndHour = me.GetInt("EndHour"),
                StartHour = me.GetInt("StartHour"),
                ValidFromDate = me.GetDate("ValidFromDate"),
                ValidToDate = me.GetDate("ValidToDate"),
                HasCombinedInstructions = me.GetBool("HasCombinedInstructions"),
            };

        public static void AddStationInstructions(this IDataRecord me, StationDutyData it)
        {
            it.StationInstructions.Add(me.AsInstruction("StationInstructions"));
            it.ShuntingInstructions.Add(me.AsInstruction("ShuntingInstructions"));
        }
    }
}

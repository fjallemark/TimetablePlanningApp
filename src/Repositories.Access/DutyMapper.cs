using System.Collections.Generic;
using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    internal static class DutyMapper
    {
        public static DriverDutyBooklet AsDriverDutyBooklet(this IDataRecord me) =>
            new()
            {
                ScheduleName = me.GetString("LayoutName")
            };

        public static LayoutInstruction AsLayoutInstruction(this IDataRecord me) =>
            new()
            {
                Language = me.GetString("Language"),
                Markdown = me.GetString("Markdown")
            };

        public static DriverDuty AsDuty(this IDataRecord me) =>
            new()
            {
                OperationDays = me.GetByte("DutyDays").OperationDays(),
                Difficulty = me.GetInt("DutyDifficulty"),
                EndTime = me.GetTime("DutyEndsTime"),
                LayoutName = me.GetString("LayoutName"),
                Description = me.GetString("DutyName"),
                Number = me.GetInt("DutyNumber"),
                Operator = me.GetString("DutyOperator"),
                RemoveOrder = me.GetInt("DutyRemoveOrder"),
                StartTime = me.GetTime("DutyStartsTime"),
                Parts = new List<DutyPart>()
            };

        public static DutyPart AsDutyPart(this IDataRecord me, Train train) =>
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
    }
}

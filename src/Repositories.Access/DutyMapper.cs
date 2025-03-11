using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;

internal static class DutyMapper
{
    public static DutyBooklet ToDutyBooklet(this IDataRecord me, DutyBooklet it)
    {
        it.ScheduleName = me.GetString("LayoutName");
        it.ValidFromDate = me.GetDate("ValidFromDate");
        it.ValidToDate = me.GetDate("ValidToDate");
        return it;
    }

    public static Instruction ToInstruction(this IDataRecord me, string markdownColumnName) =>
        new()
        {
            Language = me.GetString("Language"),
            Markdown = me.GetString(markdownColumnName)
        };



    public static DriverDuty ToDuty(this IDataRecord me) =>
        new()
        {
            DisplayOrder = me.GetInt("DutyNumber"),
            OperationDays = me.GetByte("DutyDays").OperationDays(),
            ValidFromDate = me.GetDate("ValidFromDate"),
            ValidToDate = me.GetDate("ValidToDate"),
            Difficulty = me.GetInt("DutyDifficulty"),
            EndTime = me.GetTime("DutyEndsTime", ""),
            LayoutName = me.GetString("LayoutName"),
            Description = me.GetString("DutyName"),
            Number = me.GetInt("DutyNumber").ToString(),
            Operator = me.GetString("DutyOperator"),
            RemoveOrder = me.GetInt("DutyRemoveOrder", 0),
            StartTime = me.GetTime("DutyStartsTime", ""),
            StaffingNumber = me.GetInt("Staffing"),
            Parts = []
        };

    public static DriverDutyPart ToDutyPart(this IDataRecord me, Train train) =>
        new()
        {
            Train = train,
            FromCallId = me.GetInt("FromCallId"),
            GetLocoAtParking = me.GetBool("FromParking"),
            ToCallId = me.GetInt("ToCallId"),
            PutLocoAtParking = me.GetBool("ToParking"),
            ReverseLoco = me.GetBool("ReverseLoco"),
            TurnLoco = me.GetBool("TurnLoco"),
            IsReinforcement = me.GetBool("IsReinforcement"),
        };

    public static StationDutyData ToStationDutyData(this IDataRecord me) =>
        new()
        {
            DisplayOrder = me.GetInt("DisplayOrder"),
            LayoutName = me.GetString("LayoutName"),
            StationId = me.GetInt("StationId"),
            StationName = me.GetString("Name"),
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
        it.StationInstructions.Add(me.ToInstruction("StationInstructions"));
        it.ShuntingInstructions.Add(me.ToInstruction("ShuntingInstructions"));
    }
}

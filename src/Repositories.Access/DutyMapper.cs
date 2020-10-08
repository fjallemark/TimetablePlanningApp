using System.Collections.Generic;
using System.Data;
using System.Resources;
using Tellurian.Trains.Planning.App.Contract;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    internal static class DutyMapper
    {
        private static ResourceManager Notes => App.Contract.Resources.Notes.ResourceManager;

        public static DriverDutyBooklet AsDriverDutyBooklet(this IDataRecord me) =>
            new DriverDutyBooklet
            {
                ScheduleName = me.GetString("LayoutName"),
                InstructionsMarkdown = me.GetString("Markdown")
            };

        public static DriverDuty AsDuty(this IDataRecord me) =>
            new DriverDuty
            {
                OperationDays = me.GetByte("DutyDays").OperationDays(),
                Difficulty = me.GetInt("DutyDifficulty"),
                EndTime = me.GetTime("DutyEndsTime"),
                LayoutName = me.GetString("LayoutName"),
                Name = me.GetString("DutyName"),
                Number = me.GetInt("DutyNumber"),
                Operator = me.GetString("DutyOperator"),
                RemoveOrder = me.GetInt("DutyRemoveOrder"),
                StartTime = me.GetTime("DutyStartsTime"),
                Parts = new List<DutyPart>()
            };

        public static Train AsTrain(this IDataRecord me) =>
            new Train
            {
                OperatorName = me.GetString("TrainOperator"),
                Number = $"{me.GetString("TrainNumberPrefix")} {me.GetInt("TrainNumber")}",
                OperationDays = me.GetByte("TrainDays").OperationDays(),
                CategoryName = me.GetStringResource("TrainCategoryName", Notes),
                Instruction = me.GetString("TrainInstruction"),
                MaxNumberOfWaggons = me.GetInt("TrainMaxNumberOfWaggons"),
                MaxSpeed = me.GetInt("TrainMaxSpeed"),
                Calls = new List<StationCall>()
            };

        public static StationCall AsStationCall(this IDataRecord me, int sequenceNumber) =>
            new StationCall
            {
                Id = me.GetInt("CallId"),
                IsStop = me.GetBool("IsStop"),
                Track = me.GetString("TrackNumber"),
                SequenceNumber = sequenceNumber,
                Station = new Station
                {
                     Name= me.GetString("StationName"),
                     Signature = me.GetString("StationSignature")
                },
                Arrival = new CallTime
                {
                    IsHidden = me.GetBool("HideArrival"),
                    IsStop = me.GetBool("IsStop"),
                    Time = me.GetTime("ArrivalTime", "")
                },
                Departure = new CallTime
                {
                    IsHidden = me.GetBool("HideDeparture"),
                    IsStop = me.GetBool("IsStop"),
                    Time = me.GetTime("DepartureTime", "")
                }
            };

        public static DutyPart AsDutyPart(this IDataRecord me, Train train) =>
            new DutyPart
            {
                Train = train,
                FromCallId  = me.GetInt("FromCallId"),
                GetLocoAtParking = me.GetBool("FromParking"),
                ToCallId = me.GetInt("ToCallId"),
                PutLocoAtParking = me.GetBool("ToParking"),
                ReverseLoco = me.GetBool("ReverseLoco"),
                TurnLoco = me.GetBool("TurnLoco")
            };

        public static Loco AsLoco(this IDataRecord me) =>
            new Loco
            {
                OperatorName = me.GetString("LocoOperator"),
                Number = me.GetInt("LocoNumber"),
                OperationDays = me.GetByte("LocoDays").OperationDays()
            };
    }
}

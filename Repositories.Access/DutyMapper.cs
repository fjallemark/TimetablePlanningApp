using System.Collections.Generic;
using System.Data;
using System.Resources;
using Tellurian.Trains.Planning.App.Shared;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    internal static class DutyMapper
    {
        private static ResourceManager Notes => App.Shared.Resources.Notes.ResourceManager;

        public static DriverDuty AsDuty(this IDataRecord me) =>
            new DriverDuty
            {
                OperationDays = me.GetByte("DutyDays").OperationDays(),
                Difficulty = me.GetInt16("DutyDifficulty"),
                EndTime = me.GetTime("DutyEndsTime"),
                Name = me.GetString("DutyName"),
                Number = me.GetInt16("DutyNumber"),
                Operator = me.GetString("DutyOperator"),
                RemoveOrder = me.GetInt16("DutyRemoveOrder"),
                StartTime = me.GetTime("DutyStartsTime"),
                Parts = new List<DutyPart>()

            };

        public static Train AsTrain(this IDataRecord me) =>
            new Train
            {
                OperatorName = me.GetString("TrainOperator"),
                Number = $"{me.GetString("TrainNumberPrefix")} {me.GetInt16("TrainNumber")}",
                OperationDays = me.GetByte("TrainDays").OperationDays(),
                CategoryName = me.GetStringResource("TrainCategoryName", Notes),
                Instruction = me.GetString("TrainInstruction"),
                MaxNumberOfWaggons = me.GetInt16("TrainMaxNumberOfWaggons"),
                MaxSpeed = me.GetInt16("TrainMaxSpeed"),
                Calls = new List<StationCall>()
            };

        public static StationCall AsStationCall(this IDataRecord me, int sequenceNumber) =>
            new StationCall
            {
                Id = me.GetInt32("CallId"),
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
                FromCallId  = me.GetInt32("FromCallId"),
                GetLocoAtParking = me.GetBool("FromParking"),
                ToCallId = me.GetInt32("ToCallId"),
                PutLocoAtParking = me.GetBool("ToParking"),
                ReverseLoco = me.GetBool("ReverseLoco"),
                TurnLoco = me.GetBool("TurnLoco")
               
            };

        public static Loco AsLoco(this IDataRecord me) =>
            new Loco
            {
                OperatorName = me.GetString("LocoOperator"),
                Number = me.GetInt16("LocoNumber"),
                OperatingDays = me.GetByte("LocoDays").OperationDays()
            };
    }
}

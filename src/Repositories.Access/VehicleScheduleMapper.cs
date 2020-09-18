using System.Data;
using System.Globalization;
using Tellurian.Trains.Planning.App.Contract;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    public static class VehicleScheduleMapper
    {
        public static LocoSchedule AsLocoSchedule(this IDataRecord me) =>
            new LocoSchedule
            {
                OperationDays = me.GetByte("LocoOperationDaysFlag").OperationDays(),
                Number = me.GetInt16("LocoNumber").ToString(CultureInfo.InvariantCulture),
                Operator = me.GetString("LocoOperator"),
                Class = me.GetString("LocoClass"),
                TurnForNextDay = me.GetBoolFromInt16("TurnForNextDay"),
                Note = me.GetString("Note")
            };

        public static TrainsetSchedule AsTrainsetSchedule(this IDataRecord me) =>
            new TrainsetSchedule
            {
                OperationDays = me.GetByte("TrainsetOperationDaysFlag").OperationDays(),
                Number = me.GetInt16("TrainsetNumber").ToString(CultureInfo.InvariantCulture),
                Operator = me.GetString("TrainsetOperator"),
                Class = me.GetString("TrainsetClass"),
                TurnForNextDay = me.GetBoolFromInt16("TurnForNextDay"),
                Note = me.GetString("Note")
            };

        public static void AddTrainPart(this IDataReader me, VehicleSchedule schedule)
        {
            var trainPart = new TrainPart
            {
                TrainNumber = me.GetInt16("TrainNumber").ToString(CultureInfo.InvariantCulture),
                FromDeparture = new CallAction
                {
                    Station = new Station
                    {
                        Signature = me.GetString("DepartureStationSignature"),
                        Name = me.GetString("DepartureStationName")
                    },
                    Time = CallTime.Create( me.GetTime("DepartureTime"))
                },
                ToArrival = new CallAction
                {
                    Station = new Station
                    {
                        Signature = me.GetString("ArrivalStationSignature"),
                        Name = me.GetString("ArrivalStationName")
                    },
                    Time = CallTime.Create( me.GetTime("ArrivalTime"))
                }
            };
            schedule.TrainParts.Add(trainPart);
        }
    }
}

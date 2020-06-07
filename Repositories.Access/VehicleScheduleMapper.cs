using System.Data;
using System.Globalization;
using Tellurian.Trains.Planning.App.Shared;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    public static class VehicleScheduleMapper
    {
        public static LocoSchedule AsLocoSchedule(this IDataRecord me) =>
            new LocoSchedule
            {
                Days = me.GetString("OperatingDayShortName"),
                Number = me.GetInt16("LocoNumber").ToString(CultureInfo.InvariantCulture),
                Operator = me.GetString("LocoOperator"),
                Class = me.GetString("LocoClass"),
                TurnForNextDay = me.GetBoolean(me.GetOrdinal("TurnForNextDay"))
            };

        public static TrainsetSchedule AsTrainsetSchedule(this IDataRecord me) =>
            new TrainsetSchedule
            {
                Days = me.GetString("OperatingDayShortName"),
                Number = me.GetInt32("Number").ToString(CultureInfo.InvariantCulture),
                Operator = me.GetString("TrainsetOperator"),
                Class = me.GetString("Class"),
                TurnForNextDay = me.GetBoolean(me.GetOrdinal("TurnForNextDay"))
            };

        public static void AddTrainPart(this IDataReader me, VehicleSchedule schedule)
        {
            var trainPart = new TrainPart
            {
                TrainNumber = me.GetInt32("TrainNumber").ToString(CultureInfo.InvariantCulture),
                FromDeparture = new CallAction
                {
                    Station = new Station
                    {
                        Signature = me.GetString("DepartureStationSignature"),
                        Name = me.GetString("DepartureStationName")
                    },
                    Time = me.GetTime("DepartureTime")
                },
                ToArrival = new CallAction
                {
                    Station = new Station
                    {
                        Signature = me.GetString("ArrivalStationSignature"),
                        Name = me.GetString("ArrivalStationName")
                    },
                    Time = me.GetTime("ArrivalTime")
                }
            };
            schedule.TrainParts.Add(trainPart);
        }
    }
}

using System.Data;
using System.Globalization;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    public static class VehicleScheduleMapper
    {
        public static LocoSchedule AsLocoSchedule(this IDataRecord me) =>
            new()
            {
                OperationDays = me.GetByte("LocoOperationDaysFlag").OperationDays(),
                Number = me.GetInt("LocoNumber").ToString(CultureInfo.InvariantCulture),
                Operator = me.GetString("LocoOperator"),
                Class = me.GetString("LocoClass"),
                TurnForNextDay = me.GetBool("TurnForNextDay"),
                Note = me.GetString("Note"),
                IsRailcar = me.GetBool("IsRailcar")
            };

        public static TrainsetSchedule AsTrainsetSchedule(this IDataRecord me) =>
            new()
            {
                OperationDays = me.GetByte("TrainsetOperationDaysFlag").OperationDays(),
                Number = me.GetInt("TrainsetNumber").ToString(CultureInfo.InvariantCulture),
                Operator = me.GetString("TrainsetOperator"),
                Class = me.GetString("TrainsetClass"),
                TurnForNextDay = me.GetBool("TurnForNextDay"),
                NumberOfUnits = me.GetInt("MaxNumberOfWagons", 1),
                Note = me.GetString("Note")
            };

        public static CargoOnlySchedule AsCargoOnlySchedule(this IDataRecord me) =>
            new()
            {
                OperationDays = me.GetByte("TrainsetOperationDaysFlag").OperationDays(),
                Number = me.GetInt("TrainsetNumber").ToString(CultureInfo.InvariantCulture),
                Operator = me.GetString("TrainsetOperator"),
                Class = me.GetString("TrainsetClass"),
                TurnForNextDay = me.GetBool("TurnForNextDay"),
                NumberOfUnits = me.GetInt("MaxNumberOfWagons", 1),
                Note = me.GetString("Note")
            };


        public static void AddTrainPart(this IDataReader me, VehicleSchedule schedule)
        {
            var trainPart = new TrainPart
            {
                TrainNumber = me.GetInt("TrainNumber").ToString(CultureInfo.InvariantCulture),
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

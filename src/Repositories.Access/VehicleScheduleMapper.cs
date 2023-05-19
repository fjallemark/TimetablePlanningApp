using System.Data;
using System.Globalization;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;

public static class VehicleScheduleMapper
{
    public static LocoSchedule AsLocoSchedule(this IDataRecord me) =>
        new()
        {
            Type = "Loco",
            OperationDays = me.GetByte("LocoOperationDaysFlag").OperationDays(),
            TurnusNumber = me.GetInt("LocoNumber").ToString(CultureInfo.InvariantCulture),
            Operator = me.GetString("LocoOperator"),
            Class = me.GetString("LocoClass"),
            VehicleNumber = me.GetString("VehicleNumber"),
            TurnForNextDay = me.GetBool("TurnForNextDay"),
            Note = me.GetString("Note"),
            IsRailcar = me.GetBool("IsRailcar"),
            ReplaceOrder = me.GetInt("ReplaceOrder")
        };

     public static TrainsetSchedule AsCargoWagonSchedule(this IDataRecord me) =>
        new()
        {
            Type = "CargoWagon",
            OperationDays = me.GetByte("TrainsetOperationDaysFlag").OperationDays(),
            TurnusNumber = me.GetInt("TrainsetNumber").ToString(CultureInfo.InvariantCulture),
            Operator = me.GetString("TrainsetOperator"),
            Class = me.GetString("TrainsetClass"),
            TurnForNextDay = me.GetBool("TurnForNextDay"),
            NumberOfUnits = me.GetInt("MaxNumberOfWagons", 1),
            Note = me.GetString("Note")
        };

    public static TrainsetSchedule AsPassengerWagonSchedule(this IDataRecord me) =>
        new()
        {
            Type = "PassengerWagon",
            OperationDays = me.GetByte("TrainsetOperationDaysFlag").OperationDays(),
            TurnusNumber = me.GetInt("TrainsetNumber").ToString(CultureInfo.InvariantCulture),
            Operator = me.GetString("TrainsetOperator"),
            Class = me.GetString("TrainsetClass"),
            TurnForNextDay = me.GetBool("TurnForNextDay"),
            NumberOfUnits = me.GetInt("MaxNumberOfWagons", 1),
            Note = me.GetString("Note")
        };


    public static TrainsetSchedule AsCargoOnlySchedule(this IDataRecord me) =>
        new()
        {
            Type = "CargoOnly",
            OperationDays = me.GetByte("TrainsetOperationDaysFlag").OperationDays(),
            TurnusNumber = me.GetInt("TrainsetNumber").ToString(CultureInfo.InvariantCulture),
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
            Train = new Train
            {
                Number = me.GetInt("TrainNumber"),
                OperatorName = me.GetString("TrainOperator"),
                Color = me.GetString("TrainColor"),
                OperationDaysFlags = me.GetByte("TrainOperationDaysFlag")
            },
            FromDeparture = new CallAction()
            {
                Station = new Station
                {
                    Signature = me.GetString("DepartureStationSignature"),
                    Name = me.GetString("DepartureStationName")
                },
                Time = CallTime.Create(me.GetTime("DepartureTime")),
                AssignTime = CallTime.Create(me.GetTime("AssignTime")),
            },
            ToArrival = new CallAction
            {
                Station = new Station
                {
                    Signature = me.GetString("ArrivalStationSignature"),
                    Name = me.GetString("ArrivalStationName")
                },
                Time = CallTime.Create(me.GetTime("ArrivalTime")),
                UnassignTime = CallTime.Create(me.GetTime("UnassignTime")),
            }
        };
        schedule.TrainParts.Add(trainPart);
    }
}

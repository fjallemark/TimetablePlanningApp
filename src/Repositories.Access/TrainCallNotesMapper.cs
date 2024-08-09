using System.Data;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.Repositories.Access;

internal static class TrainCallNotesMapper
{
    public static ManualTrainCallNote ToManualTrainCallNote(this IDataRecord me) =>
        new(me.GetInt("CallId"))
        {
            Text = me.GetString("Note"),
            OperationDayFlag = me.GetByte("OperatingDayFlag"),
            DisplayOrder = me.GetInt("Row"),
            IsStationNote = me.GetBool("IsStationNote"),
            IsShuntingNote = me.GetBool("IsShuntingNote"),
            IsDriverNote = me.GetBool("IsDriverNote"),
            IsForArrival = me.GetBool("IsForArrival"),
            IsForDeparture = me.GetBool("IsForDeparture")
        };

    public static LocalizedManualTrainCallNote ToLocalizedManualTrainCallNote(this IDataRecord me) =>
        new( 
            me.GetString("LanguageCode"),
            me.GetString("Note"));

    public static TrainsetsCallNote ToTrainsetDepartureCallNote(this IDataRecord me) =>
        new TrainsetsDepartureCallNote(me.GetInt("CallId"))
        {
            IsCargoOnly = me.GetBool("IsLoadOnly"),
        };

    public static TrainsetsArrivalCallNote ToTrainsetArrivalCallNote(this IDataReader me) =>
        new(me.GetInt("CallId"))
        {
            IsCargoOnly = me.GetBool("IsLoadOnly")
        };


    public static TrainContinuationNumberCallNote ToTrainContinuationNumberCallNote(this IDataRecord me) =>
        new(me.GetInt("CallId"))
        {
            LocoOperationDaysFlag = me.GetByte("LocoOperatingDaysFlag"),
            ContinuingTrain = new OtherTrain
            {
                DestinationName = me.GetString("DestinationName"),
                OperationDayFlag = me.GetByte("DepartingTrainOperationDaysFlag"),
                TrainNumber = me.GetInt("DepartingTrainNumber"),
                CategoryPrefix = me.GetString("DepartingTrainPrefix")
            }
        };

    public static TrainMeetCallNote ToTrainMeetCallNote(this IDataRecord me) =>
        new(me.GetInt("CallId"))
        {
            OperationDayFlag = me.GetByte("TrainDaysFlag")
        };

    public static OtherTrain ToMeetingTrain(this IDataRecord me) => new()
    {
        CategoryPrefix = me.GetString("MeetTrainCategoryPrefix"),
        DestinationName = me.GetString("MeetTrainDestination"),
        TrainNumber = me.GetInt("MeetTrainNumber"),
        OperationDayFlag = me.GetByte("MeetTrainDaysFlag")
    };

    public static OtherTrainCall ToMeetingTrainCall(this IDataRecord me) => new()
    {
        CategoryPrefix = me.GetString("MeetTrainCategoryPrefix"),
        DestinationName = me.GetString("MeetTrainDestination"),
        TrainNumber = me.GetInt("MeetTrainNumber"),
        OperationDayFlag = me.GetByte("MeetTrainDaysFlag"),
        ArrivalTime = CallTime.Create(me.GetTime("ArrivalTime")),
        DepartureTime = CallTime.Create(me.GetTime("DepartureTime")),
        MeetArrivalTime = CallTime.Create(me.GetTime("MeetArrivalTime")),
        MeetDepartureTime = CallTime.Create(me.GetTime("MeetDepartureTime"))
    };

    public static LocoExchangeCallNote ToLocoExchangeCallNote(this IDataRecord me) =>
        new(me.GetInt("CallId"))
        {
            ArrivingLoco = new TrainLoco
            {
                TurnusNumber = me.GetInt("ArrivingLocoScheduleNumber"),
                OperatorName = me.GetString("ArrivingLocoOperator"),
                OperationDaysFlags = me.GetByte("ArrivingLocoOperationDaysFlag")
            },
            DepartingLoco = new TrainLoco
            {
                TurnusNumber = me.GetInt("DepartingLocoScheduleNumber"),
                OperatorName = me.GetString("DepartingLocoOperator"),
                OperationDaysFlags = me.GetByte("DepartingLocoOperationDaysFlag")
            },
            TrainInfo = new TrainInfo
            {
                CategoryName = me.GetStringResource("TrainCategory", Notes.ResourceManager),
                Prefix = me.GetString("TrainNumberPrefix"),
                Number = me.GetInt("TrainNumber"),
                OperationDaysFlags = me.GetByte("TrainOperationDaysFlag"),
                OperatorName = me.GetString("DepartingLocoOperator")
            }
        };

    public static LocoDepartureCallNote ToLocoDepartureCallNote(this IDataRecord me) =>
        new(me.GetInt("CallId"))
        {
            DepartingLoco = new TrainLoco
            {
                TurnusNumber = me.GetInt("LocoNumber"),
                OperatorName = me.GetString("LocoOperator"),
                OperationDaysFlags = me.GetByte("LocoOperationDaysFlag"),
                IsRailcar = me.GetBool("IsRailcar")
            },
            IsFromParking = me.GetBool("FromParking"),
            TrainInfo = new() { OperationDaysFlags = me.GetByte("TrainOperationDaysFlag") }
        };

    public static LocoArrivalCallNote ToLocoArrivalCallNote(this IDataRecord me) =>
        new(me.GetInt("CallId"))
        {
            ArrivingLoco = new TrainLoco
            {
                TurnusNumber = me.GetInt("LocoNumber"),
                OperatorName = me.GetString("LocoOperator"),
                OperationDaysFlags = me.GetByte("LocoOperationDaysFlag"),
                IsRailcar = me.GetBool("IsRailcar")
            },
            IsToParking = me.GetBool("ToParking"),
            CirculateLoco = me.GetBool("CirculateLoco"),
            TurnLoco = me.GetBool("TurnLoco"),
            TrainInfo = new() { OperationDaysFlags = me.GetByte("TrainOperationDaysFlag") }
        };

    public static LocoTurnOrReverseCallNote ToLocoReverseOrTurnCallNote(this IDataReader record) =>
        new(record.GetInt("CallId"))
        {
            Reverse = record.GetBool("ReverseLoco"),
            Turn = record.GetBool("TurnLoco"),
        };

    public static BlockDestinationsCallNote ToBlockDestinationsCallNote(this IDataRecord me) =>
        new(me.GetInt("CallId"))
        {

        };

    public static BlockArrivalCallNote ToBlockArrivalCallNote(this IDataRecord me) =>
        new(me.GetInt("CallId"))
        {
            ToAllDestinations = me.GetBool("ToAllDestinations"),
            AndBeyond = me.GetBool("AndBeyond"),
            AlsoSwitch = me.GetBool("AlsoSwitch"),
            OrderInTrain = me.GetInt("OrderInTrain"),
            AtShadowStation = me.GetBool("IsShadow"),
            IsTransfer = me.GetBool("IsTransfer"),
        };

    internal static BlockDestination ToBlockDestination(this IDataRecord me)
    {
        var item = new BlockDestination
        {
            StationId = me.GetInt("DestinationStationId"),
            StationName = me.GetString("DestinationStationName"),
            TransferDestinationName = me.GetString("TransferDestinationName", ""),
            ToAllDestinations = me.GetBool("ToAllDestinations"),
            AndBeyond = me.GetBool("AndBeyond"),
            AndLocalDestinations = me.GetBool("AndLocalDestinations"),
            TransferAndBeyond = me.GetBool("TransferAndBeyond"),
            OrderInTrain = me.GetInt("OrderInTrain"),
            MaxNumberOfWagons = me.GetInt("MaxNumberOfWagons"),
            DestinationCountryName = me.GetString("CountryLocalName"),
            IsInternational = me.GetBool("IsInternational"),
            IsRegion = me.GetBool("IsRegion"),
            IsCargo = me.GetBool("IsCargo", false),
            IsTrainset = me.GetBool("IsTrainset", false),
            HasCouplingNote = me.GetBool("HasCoupleNote"),
            HasUncouplingNote = me.GetBool("HasUncoupleNote"),
            ForeColor = me.GetString("ForeColor", "#000000"),
            BackColor = me.GetString("BackColor", "#FFFFFF"),
            TrainsetNumber = me.GetInt("TrainsetNumber", 0),
            TrainsetOperatorName = me.GetString("TrainsetOperatorName", ""),
            TrainsetOperationDaysFlag = me.GetByte("TrainsetOperationDaysFlag"),
            Note = me.GetString("Note", ""),

        };
        return item;
    }

    internal static Trainset ToTrainset(this IDataRecord me) =>
        new()
        {
            Class = me.GetString("Class"),
            IsCargo = me.GetBool("IsCargo"),
            HasCoupleNote = me.GetBool("HasCoupleNote"),
            HasUncoupleNote = me.GetBool("HasUncoupleNote"),
            MaxNumberOfWaggons = me.GetInt("MaxNumberOfWagons"),
            WagonTypes = me.GetString("Description"),
            Number = me.GetInt("Number"),
            OperationDaysFlag = me.GetByte("OperatingDaysFlag"),
            Operator = me.GetString("Operator"),
            PositionInTrain = me.GetInt("OrderInTrain"),
            Destination = me.GetString("Destination"),
            FinalDestination = me.GetString("FinalDestination")
        };
}

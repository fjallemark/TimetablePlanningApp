using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Tellurian.Trains.Planning.App.Contract;
using Tellurian.Trains.Planning.App.Contract.Resources;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    internal static class TrainCallNotesMapper
    {
        public static ManualTrainCallNote AsManualTrainCallNote(this IDataRecord me) =>
            new ManualTrainCallNote(me.GetInt("CallId"))
            {
                Text = me.GetString("Note"),
                DisplayOrder = me.GetInt("Row"),
                IsStationNote = me.GetBool("IsStationNote"),
                IsShuntingNote = me.GetBool("IsShuntingNote"),
                IsDriverNote = me.GetBool("IsDriverNote"),
                IsForArrival = me.GetBool("IsForArrival"),
                IsForDeparture = me.GetBool("IsForDeparture")
            };

        public static TrainsetsCallNote AsTrainsetsCallNote(this IDataRecord me, bool isForDeparture, bool isForArrival) =>
            isForDeparture ? (TrainsetsCallNote)new TrainsetsDepartureCallNote(me.GetInt("CallId")) :
            isForArrival ? new TrainsetsArrivalCallNote(me.GetInt("CallId")) :
            throw new InvalidOperationException();

        public static TrainContinuationNumberCallNote AsTrainContinuationNumberCallNote(this IDataRecord me) =>
            new TrainContinuationNumberCallNote(me.GetInt("CallId"))
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

        public static TrainMeetCallNote AsTrainMeetCallNote(this IDataRecord me) =>
            new TrainMeetCallNote(me.GetInt("CallId"))
            {
                OperationDayFlag = me.GetByte("TrainDaysFlag")
            };

        public static OtherTrain AsMeetingTrain(this IDataRecord me) => new OtherTrain
        {
            CategoryPrefix = me.GetString("MeetTrainCategoryPrefix"),
            DestinationName = me.GetString("MeetTrainDestination"),
            TrainNumber = me.GetInt("MeetTrainNumber"),
            OperationDayFlag = me.GetByte("MeetTrainDaysFlag")
        };

        public static LocoExchangeCallNote AsLocoExchangeCallNote(this IDataRecord me) =>
            new LocoExchangeCallNote(me.GetInt("CallId"))
            {
                ArrivingLoco = new Loco
                {
                    Number = me.GetInt("ArrivingLocoScheduleNumber"),
                    OperatorName = me.GetString("ArrivingLocoOperator")
                },
                DepartingLoco = new Loco
                {
                    Number = me.GetInt("DepartingLocoScheduleNumber"),
                    OperatorName = me.GetString("DepartingLocoOperator")
                },
                TrainInfo = new TrainInfo
                {
                    CategoryName = me.GetStringResource("TrainCategory", Notes.ResourceManager),
                    Number = $"{me.GetString("TrainNumberPrefix")} { me.GetInt("TrainNumber")}",
                    OperationDays = me.GetByte("TrainOperationDaysFlag").OperationDays(),
                    OperatorName = me.GetString("TrainOperator")
                }
            };

        public static LocoDepartureCallNote AsLocoDepartureCallNote(this IDataRecord me) =>
            new LocoDepartureCallNote(me.GetInt("CallId"))
            {
                DepartingLoco = new Loco
                {
                    Number = me.GetInt("LocoNumber"),
                    OperatorName = me.GetString("LocoOperator"),
                    OperationDays = me.GetByte("LocoOperationDaysFlag").OperationDays(),
                    IsRailcar = me.GetBool("IsRailcar")
                },
                IsFromParking = me.GetBool("FromParking"),
                TrainOperationDays = me.GetByte("TrainOperationDaysFlag").OperationDays()
            };

        public static LocoArrivalCallNote AsLocoArrivalCallNote(this IDataRecord me) =>
            new LocoArrivalCallNote(me.GetInt("CallId"))
            {
                ArrivingLoco = new Loco
                {
                    Number = me.GetInt("LocoNumber"),
                    OperatorName = me.GetString("LocoOperator"),
                    OperationDays = me.GetByte("LocoOperationDaysFlag").OperationDays(),
                    IsRailcar = me.GetBool("IsRailcar")
                },
                IsToParking = me.GetBool("ToParking"),
                CirculateLoco = me.GetBool("CirculateLoco"),
                TurnLoco = me.GetBool("TurnLoco"),
                TrainOperationDays = me.GetByte("TrainOperationDaysFlag").OperationDays()
            };

        public static BlockDestinationsCallNote AsBlockDestinationsCallNote(this IDataRecord me) =>
            new BlockDestinationsCallNote(me.GetInt("CallId"));

        public static BlockArrivalCallNote AsBlockArrivalCallNote(this IDataRecord me) =>
            new BlockArrivalCallNote(me.GetInt("CallId"))
            {
                StationName = me.GetString("ArrivalStationName"),
                ToAllDestinations = me.GetBool("ToAllDestinations"),
                AndBeyond = me.GetBool("AndBeyond"),
                AlsoSwitch = me.GetBool("AlsoSwitch"),
                OrderInTrain = me.GetInt("OrderInTrain")
            };

        internal static BlockDestination AsBlockDestination(this IDataRecord me) =>
            new BlockDestination
            {
                StationName = me.GetString("DestinationStationName"),
                TransferDestination = me.GetString("TransferDestinationName", ""),
                ToAllDestinations = me.GetBool("ToAllDestinations"),
                AndBeyond = me.GetBool("AndBeyond"),
                OrderInTrain = me.GetInt("OrderInTrain"),
                MaxNumberOfWagons = me.GetInt("MaxNumberOfWagons"),
                ForeColor = me.GetString("ForeColor", "#000000"),
                BackColor = me.GetString("BackColor", "#FFFFFF")
            };

        internal static NoteTrainset AsTrainset(this IDataRecord me) =>
            new NoteTrainset
            {
                Class = me.GetString("Class"),
                IsCargo = me.GetBool("IsCargo"),
                HasCoupleNote = me.GetBool("HasCoupleNote"),
                HasUncoupleNote = me.GetBool("HasUncoupleNote"),
                MaxNumberOfWaggons = me.GetInt("MaxNumberOfWagons"),
                Note = me.GetString("Description"),
                Number = me.GetInt("Number"),
                OperationDaysFlag = me.GetByte("OperatingDaysFlag"),
                Operator = me.GetString("Operator"),
                PositionInTrain = me.GetInt("OrderInTrain")
            };
    }
}

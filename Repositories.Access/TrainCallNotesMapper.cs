using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using Tellurian.Trains.Planning.App.Contract;
using Tellurian.Trains.Planning.App.Contract.Resources;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    internal static class TrainCallNotesMapper
    {
        public static ManualTrainCallNote AsManualTrainCallNote(this IDataRecord me) =>
            new ManualTrainCallNote(me.GetInt32("CallId"))
            {
                Text = me.GetString("Note"),
                DisplayOrder = me.GetInt16("Row"),
                IsStationNote = me.GetBool("IsStationNote"),
                IsShuntingNote = me.GetBool("IsShuntingNote"),
                IsDriverNote = me.GetBool("IsDriverNote"),
                IsForArrival = me.GetBool("IsForArrival"),
                IsForDeparture = me.GetBool("IsForDeparture")
            };

        public static TrainsetsCallNote AsTrainsetsCallNote(this IDataRecord me, bool isForDeparture, bool isForArrival) =>
            isForDeparture ? (TrainsetsCallNote)new TrainsetsDepartureCallNote(me.GetInt32("CallId")) :
            isForArrival ? new TrainsetsArrivalCallNote(me.GetInt32("CallId")) :
            throw new InvalidOperationException();

        public static TrainContinuationNumberCallNote AsTrainContinuationNumberCallNote(this IDataRecord me) =>
            new TrainContinuationNumberCallNote(me.GetInt32("CallId"))
            {
                LocoOperationDaysFlag = me.GetByte("LocoOperatingDaysFlag"),
                ContinuingTrain = new ContinuingTrain
                {
                    DestinationName = me.GetString("DestinationName"),
                    OperationDayFlag = me.GetByte("DepartingTrainOperationDaysFlag"),
                    TrainNumber = me.GetInt16("DepartingTrainNumber"),
                    CategoryPrefix = me.GetString("DepartingTrainPrefix")
                }
            };

        public static LocoExchangeCallNote AsLocoExchangeCallNote(this IDataRecord me) =>
            new LocoExchangeCallNote(me.GetInt32("CallId"))
            {
                ArrivingLoco = new Loco
                {
                    Number = me.GetInt16("ArrivingLocoScheduleNumber"),
                    OperatorName = me.GetString("ArrivingLocoOperator")
                },
                DepartingLoco = new Loco
                {
                    Number = me.GetInt16("DepartingLocoScheduleNumber"),
                    OperatorName = me.GetString("DepartingLocoOperator")
                },
                TrainInfo = new TrainInfo
                {
                    CategoryName = me.GetStringResource("TrainCategory", Notes.ResourceManager),
                    Number = $"{me.GetString("TrainNumberPrefix")} { me.GetInt16("TrainNumber")}",
                    OperationDays = me.GetByte("TrainOperationDaysFlag").OperationDays(),
                    OperatorName = me.GetString("TrainOperator")
                }
            };

        public static LocoDepartureCallNote AsLocoDepartureCallNote(this IDataRecord me) =>
            new LocoDepartureCallNote(me.GetInt32("CallId"))
            {
                DepartingLoco = new Loco
                {
                    Number = me.GetInt16("LocoNumber"),
                    OperatorName = me.GetString("LocoOperator"),
                    OperationDays = me.GetByte("LocoOperationDaysFlag").OperationDays(),
                },
                IsFromParking = me.GetBool("FromParking"),
                TrainOperationDays = me.GetByte("TrainOperationDaysFlag").OperationDays()
            };

        public static LocoArrivalCallNote AsLocoArrivalCallNote(this IDataRecord me) =>
            new LocoArrivalCallNote(me.GetInt32("CallId"))
            {
                ArrivingLoco = new Loco
                {
                    Number = me.GetInt16("LocoNumber"),
                    OperatorName = me.GetString("LocoOperator"),
                    OperationDays = me.GetByte("LocoOperationDaysFlag").OperationDays(),
                },
                IsToParking = me.GetBool("ToParking"),
                CirculateLoco = me.GetBool("CirculateLoco"),
                TurnLoco = me.GetBool("TurnLoco"),
                TrainOperationDays = me.GetByte("TrainOperationDaysFlag").OperationDays()
            };

        public static BlockDestinationsCallNote AsBlockDestinationsCallNote(this IDataRecord me) =>
            new BlockDestinationsCallNote(me.GetInt32("CallId"));

        public static BlockArrivalCallNote AsBlockArrivalCallNote(this IDataRecord me) =>
            new BlockArrivalCallNote(me.GetInt32("CallId"))
            {
                StationName = me.GetString("ArrivalStationName"),
                AndBeyond = me.GetBool("AndBeyond"),
                AlsoSwitch = me.GetBool("AlsoSwitch"),
                OrderInTrain = me.GetInt16("OrderInTrain")
            };

        internal static BlockDestination AsBlockDestination(this IDataRecord me) =>
            new BlockDestination
            {
                Name = me.GetString("DestinationStationName"),
                AndBeyond = me.GetBool("AndBeyond"),
                OrderInTrain = me.GetInt16("OrderInTrain")
            };

        internal static Trainset AsTrainset(this IDataRecord me) =>
            new Trainset
            {
                Class = me.GetString("Class"),
                IsCargo = me.GetBool("IsCargo"),
                HasCoupleNote = me.GetBool("HasCoupleNote"),
                HasUncoupleNote = me.GetBool("HasUncoupleNote"),
                MaxNumberOfWaggons = me.GetInt16("MaxNumberOfWaggons"),
                Note = me.GetString("Description"),
                Number = me.GetInt16("Number"),
                OperationDaysFlag = me.GetByte("OperatingDaysFlag"),
                Operator = me.GetString("Operator"),
                PositionInTrain = me.GetInt16("OrderInTrain")
            };
    }
}

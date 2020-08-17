using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using Tellurian.Trains.Planning.App.Shared;

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


        public static Trainset AsTrainset(this IDataRecord me) =>
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

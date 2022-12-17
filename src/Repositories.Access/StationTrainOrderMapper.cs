﻿using System;
using System.Collections.Generic;
using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;
internal static class StationTrainOrderMapper
{
    public static StationTrainOrder AsStationTrainOrder(this IDataRecord me) =>
        new()
        {
            Designation = me.GetString("Signature"),
            Name = me.GetString("FullName"),
            Trains = new List<StationTrain>(100)
        };

    public static StationTrain AsStationTrain(this IDataRecord me) =>
        new()
        {
            ArrivalTime = me.GetString("ArrivalTime"),
            DepartureTime = me.GetString("DepartureTime"),
            DestinationName = me.GetString("Destination"),
            OperatingDayFlag = me.GetByte("OperatingDayFlag"),
            OperatorName = me.GetString("Operator"),
            OriginName = me.GetString("Origin"),
            ProductName = me.GetString("Product"),
            SortTime = me.GetTimeAsDouble("SortTime"),
            TrackNumber = me.GetString("Designation"),
            TrainNumber = me.GetString("Number"),
            IsStop = me.GetBool("IsStop")
        };
}
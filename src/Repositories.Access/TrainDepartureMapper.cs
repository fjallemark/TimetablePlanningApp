﻿using System.Data;
using System.Resources;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;

internal static class TrainDepartureMapper
{
    private static ResourceManager Notes => App.Contracts.Resources.Notes.ResourceManager;

    public static TrainDeparture ToTrainDeparture(this IDataRecord me) =>
        new()
        {
            DepartureTime = new CallTime { IsStop = true, Time = me.GetTime("DepartureTime") },
            Loco = new TrainLoco
            {
                Class = me.GetString("LocoClass"),
                IsRailcar = me.GetBool("IsRailcar"),
                TurnusNumber = me.GetInt("LocoNumber"),
                OperationDaysFlags = me.GetByte("LocoOperationDaysFlag"),
                OperatorName = me.GetString("LocoOperatorName")
            },
            StationName = me.GetString("StationName"),
            TrackNumber = me.GetString("TrackNumber"),
            Train = new TrainInfo
            {
                CategoryResourceCode = me.GetString("TrainCategoryName"),
                CategoryName = me.GetStringResource("TrainCategoryName", Notes),
                IsCargo = me.GetBool("IsCargo"),
                IsPassenger = me.GetBool("IsPassenger"),
                Number = me.GetInt("TrainNumber"),
                OperationDaysFlags = me.GetByte("TrainOperationDaysFlag"),
                OperatorName = me.GetString("TrainOperatorName"),
                Prefix = me.GetString("TrainCategoryPrefix"),
                OverriddenDestination = me.GetString("TrainDestination",null),
            }
        };
}

﻿using System.Data;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Contracts.Extensions;

namespace Tellurian.Trains.Planning.Repositories.Access;
internal static class LayoutMapper
{
    public static Layout? ToLayout(this IDataRecord me) =>
        new ()
        {
            Id = me.GetInt("Id"),
            Name = me.GetString("Name"),
            StartHour = me.GetInt("StartHour"),
            EndHour = me.GetInt("EndHour"),
            ValidFrom = me.GetDate("ValidFromDate"),
            ValidTo = me.GetDate("ValidToDate"),
            MaxLocoDriversCount = me.GetInt("MaxLocoDriversCount"),
            TrainCategoryCountryId = me.GetInt("TrainCategoryCountry"),
            TrainCategoryYear = me.GetInt("TrainCategoryYear"),
            StartWeekdayId = me.GetInt("StartWeekday"),
            PrintShuntingLocos = me.GetBool("PrintShuntingLocos"),
            FontFamily = me.GetString("FontFamily")
                .OrElse("sans-serif")
        };
}

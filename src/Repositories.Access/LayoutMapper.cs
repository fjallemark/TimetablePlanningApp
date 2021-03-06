using System.Data;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Contracts.Extensions;

namespace Tellurian.Trains.Planning.Repositories.Access;
internal static class LayoutMapper
{
    public static Layout? AsLayout(this IDataRecord me) =>
        new ()
        {
            Name = me.GetString("Name"),
            StartHour = me.GetInt("StartHour"),
            EndHour = me.GetInt("EndHour"),
            ValidFrom = me.GetDate("ValidFromDate"),
            ValidTo = me.GetDate("ValidToDate"),
            FontFamily = me.GetString("FontFamily")
                .OrElse("'Segoe UI', Tahoma, Geneva, Verdana, sans-serif")
        };
}

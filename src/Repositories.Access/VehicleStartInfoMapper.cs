using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Resources;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.Repositories.Access;
internal static class VehicleStartInfoMapper
{
    private static readonly ResourceManager ResourceManager = Notes.ResourceManager;
    public static VehicleStartInfo ToVehicleStartInfo(this IDataRecord record) =>
        new()
        {
            Id = record.GetInt("Id"),
            Type = record.GetString("Type"),
            DccAddress = (short?)record.GetIntOrNull("Address"),
            DepartureTime = record.GetString("DepartureTime"),
            MaxNumberOfVehicles = record.GetInt("MaxNumberOfWagons"),
            OperatorSignature = record.GetString("Operator"),
            OwnerName = record.GetString("Owner"),
            StationName = record.GetString("StartStationName"),
            TrackNumber = record.GetString("DepartureTrack"),
            TrainCategoryId = record.GetInt("TrainCategoryId"),
            TrainNumber = record.GetInt("TrainNumber"),
            TurnusNumber = record.GetIntOrNull("Number"),
            VehicleClass = record.GetString("Class"),
            VehicleNumber = record.GetString("VehicleNumber"),
            VehicleType = record.GetStringResource("Type", ResourceManager),
            Note = record.GetString("Note", ""),
            ReplaceOrder = record.GetInt("ReplaceOrder", -1),
            HasFredThrottle = record.GetBool("HasFred"),
            DayFlags = record.GetByte("OperationDaysFlag"),
            IsFirstDay = record.GetBool("IsFirstDay"),
            LayoutStartWeekday = record.GetInt("StartWeekday"),
        };
}

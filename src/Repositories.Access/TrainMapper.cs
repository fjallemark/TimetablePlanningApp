using System.Collections.Generic;
using System.Data;
using System.Resources;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access
{
    internal static class TrainMapper
    {
        private static ResourceManager Notes => App.Contracts.Resources.Notes.ResourceManager;

        public static Train AsTrain(this IDataRecord me) =>
             new()
             {
                 OperatorName = me.GetString("TrainOperator"),
                 Prefix = me.GetString("TrainNumberPrefix"),
                 Number = me.GetInt("TrainNumber"),
                 OperationDaysFlags = me.GetByte("TrainDays").And(me.GetByte("DutyDays")),
                 CategoryResourceCode = me.GetString("TrainCategoryName"),
                 CategoryName = me.GetStringResource("TrainCategoryName", Notes),
                 Instruction = me.GetString("TrainInstruction"),
                 MaxNumberOfWaggons = me.GetInt("TrainMaxNumberOfWaggons", 0),
                 MaxSpeed = me.GetInt("TrainMaxSpeed"),
                 IsCargo = me.GetBool("IsCargo"),
                 IsPassenger = me.GetBool("IsPassenger"),
                 Color = me.GetString("Color"),
                 Calls = new List<StationCall>()
             };

        public static StationCall AsStationCall(this IDataRecord me, int sequenceNumber) =>
            new()
            {
                Id = me.GetInt("CallId"),
                TrackId = me.GetInt("TrackId", -1),
                IsStop = me.GetBool("IsStop"),
                TrackNumber = me.GetString("TrackNumber"),
                SequenceNumber = sequenceNumber,
                Station = new StationInfo
                {
                    Id = me.GetInt("StationId"),
                    Name = me.GetString("StationName"),
                    Signature = me.GetString("StationSignature"),
                    HasCombinedInstructions = me.GetBool("HasCombinedInstructions")
                },
                Arrival = new CallTime
                {
                    IsHidden = me.GetBool("HideArrival"),
                    IsStop = me.GetBool("IsStop"),
                    Time = me.GetTime("ArrivalTime", ""),
                },
                Departure = new CallTime
                {
                    IsHidden = me.GetBool("HideDeparture"),
                    IsStop = me.GetBool("IsStop"),
                    Time = me.GetTime("DepartureTime", "")
                }
            };

        public static TrainCategory AsTrainCategory(this IDataRecord me) =>
            new()
            {
                Id = me.GetInt("Id"),
                Name = me.GetString("Name"),
                ResourceCode = me.GetString("ResourceCode"),
                IsCargo = me.GetBool("IsCargo"),
                IsPassenger = me.GetBool("IsPassenger"),
                Prefix = me.GetString("Prefix"),
                Suffix = me.GetString("Suffix"),
                FromYear = me.GetInt("FromYear"),
                ToYear = me.GetInt("UptoYear"),
                CountryId = me.GetInt("CountryId")
            };

    }
}

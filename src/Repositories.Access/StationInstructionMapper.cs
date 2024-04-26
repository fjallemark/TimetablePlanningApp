using System.Data;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.Repositories.Access;
internal static class StationInstructionMapper
{
    public static StationInstruction ToStationInstruction(this IDataRecord record) =>
        new()
        {
            StationInstructionsMarkdown = record.GetString("StationInstructions"),
            ShuntingInstructionsMarkdown = record.GetString("ShuntingInstructions"),
            StationInfo = new()
            {
                Id = record.GetInt("LayoutStationId"),
                HasCombinedInstructions = record.GetBool("HasCombinedInstructions"),
                Name = record.GetString("StationName"),
                Signature = record.GetString("StationSignature")
            }

        };
}

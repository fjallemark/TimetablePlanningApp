using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.App.Contracts
{
    public class Loco
    {
        public string OperatorName { get; set; } = string.Empty;
        public int Number { get; set; }
        public byte OperationDaysFlags { get; set; } 
        public string Class { get; set; } = string.Empty;
        public bool IsRailcar { get; set; }
        public override string ToString() => $"{OperationDaysFlags.OperationDays()} {OperatorName} {Number}".TrimStart();
        public static Loco Empty { get; } = new Loco();
    }

    public static class LocoExtensions
    {
        public static OperationDays OperationDays(this Loco me) => me.OperationDaysFlags.OperationDays();
        public static string TypeName(this Loco me) =>
            me.IsRailcar ? Notes.Railcar.ToLowerInvariant() : Notes.Loco.ToLowerInvariant();
    }
}

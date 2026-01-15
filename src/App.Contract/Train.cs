namespace Tellurian.Trains.Planning.App.Contracts;

public static class TrainInfoExtensions
{
    public static OperationDays OperationDays(this TrainInfo me) => me.OperationDaysFlags.OperationDays();
    public static bool IsOnDemand(this TrainInfo? me) => me is not null && me.OperationDaysFlags.IsOnDemand();
    public static bool IsSame(this TrainInfo? me, TrainInfo? other) =>
        me is not null && other is not null && me.OperatorName.Equals(other.OperatorName) && me.Number == other.Number;

    public static TrainInfo MergeDays(this IEnumerable<TrainInfo> trains)
    {
        if (trains == null || trains.Count() < 1) throw new ArgumentException("Is null or empty", nameof(trains));
        if (trains.Count() == 1) return trains.First();
        return new TrainInfo()
        {
            CategoryId = trains.First().CategoryId,
            Category = trains.First().Category,
            Color = trains.First().Color,
            IsCargo = trains.First().IsCargo,
            IsPassenger = trains.First().IsPassenger,
            Number = trains.First().Number,
            OperationDaysFlags = trains.Select(x => x.OperationDaysFlags).ToArray().Aggregate(),
            OperatorName = trains.First().OperatorName,
            OverriddenDestination = trains.First().OverriddenDestination,
            OverriddenOrigin = trains.First().OverriddenOrigin,
        };
    }
}
public class TrainInfo
{
    public int Id { get; set; }
    public string Prefix => Category.Prefix;
    public string CategoryName => Category.Type;
    public int Number { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public TrainCategory Category { get; set; } = TrainCategory.Empty;
    public bool IsCargo { get; set; }
    public bool IsPassenger { get; set; }
    public byte OperationDaysFlags { get; set; }
    public string Color { get; set; } = string.Empty;

    public string Origin => this is Train t ? t.Calls.First().Station.Name : OverriddenOrigin ?? string.Empty;
    public string Destination => this is Train t ? t.Calls.Last().Station.Name : OverriddenDestination ?? string.Empty;
    public string? OverriddenDestination { get; set; }
    public string? OverriddenOrigin { get; set; }
    public override string ToString() => $"{OperatorName} {Prefix} {Number} {OperationDaysFlags.OperationDays().ShortName}";
    public override bool Equals(object? obj) => obj is Train t && t.Number == Number && t.OperationDaysFlags == OperationDaysFlags && OperatorName.Equals(t.OperatorName, StringComparison.OrdinalIgnoreCase);
    public override int GetHashCode() => HashCode.Combine(Number, OperatorName, OperationDaysFlags);
}



public class Train : TrainInfo
{
    public int MaxSpeed { get; set; }
    public int MaxNumberOfWaggons { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public IList<StationCall> Calls { get; set; } = [];
    public override string ToString() => $"{base.ToString()} {Calls.Count} calls";
    public static Train Example => new()
    {
        OperatorName = "GC",
        Category = new() { Type = "Godståg", Prefix = "G" },
        Number = 51,
        Calls =
        [
            new StationCall {
                Id = 21,
                SequenceNumber = 1,
                Station = new Station { Name="Göteborg Kville", Signature="Gkv"},
                TrackNumber = "4",
                Departure = CallTime.Create("12:10", "Medtar vagnsgrupp 653 tankvagnar."),
            },
            new StationCall {
                Id=22,
                SequenceNumber = 2,
                Station = new Station { Name="Göteborg Sävenäs", Signature="Gsv"},
                TrackNumber = "4",
                Arrival = CallTime.Create("12:19", "Gör rundgång"),
                Departure = new CallTime{ Time="12:49" },
            },
            new StationCall {
                Id=23,
                SequenceNumber = 3,
                IsStop = false,
                Station = new Station { Name="Tingstad", Signature="Tsd"},
                TrackNumber = "4",
                Arrival = new CallTime { Time="12:54", IsHidden=true},
                Departure = new CallTime{ Time="12:55" },
            },
            new StationCall {
                Id=24,
                SequenceNumber = 4,
                IsStop = true,
                Station = new Station { Name="Säve", Signature="Sve"},
                TrackNumber = "4",
                Arrival = CallTime.Create ("13:05", "Möter persontåg 3766."),
                Departure = new CallTime{ Time="13:08" },
            },
            new StationCall {
                Id=25,
                SequenceNumber = 5,
                IsStop = false,
                Station = new Station { Name="Ytterby", Signature="Yb"},
                TrackNumber = "4",
                Arrival = new CallTime { Time="13:13", IsHidden=true},
                Departure = new CallTime{ Time="13:14" },
            },
            new StationCall {
                Id = 26,
                SequenceNumber = 6,
                IsStop = false,
                Station = new Station { Name="Kode", Signature="Kde"},
                TrackNumber = "4",
                Arrival = new CallTime { Time="13:20", IsHidden=true},
                Departure = new CallTime{ Time="13:21" },
            },
            new StationCall {
                Id = 27,
                SequenceNumber = 7,
                IsStop = false,
                Station = new Station { Name="Stora Höga", Signature="Sth"},
                TrackNumber = "4",
                Arrival = new CallTime { Time="13:29", IsHidden=true},
                Departure = new CallTime{ Time="13:30" },
            },
            new StationCall {
                Id = 28,
                SequenceNumber = 8,
                IsStop = true,
                Station = new Station { Name="Stenungsund", Signature="Snu"},
                TrackNumber = "4",
                Arrival = CallTime.Create( "13:38", "Vagnarna växlas in till respektive godskund enligt fraktsedel."),
            }
        ]
    };
}

using Markdig;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Tellurian.Trains.Planning.App.Contracts.Extensions;
using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.App.Contracts;

public partial class Note
{
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public byte OperatingDaysFlag { get; set; } = 0x7F;
    public override string ToString() => Text ?? "";
    public bool IsShortNote => TextLength <= 30;
    public static Note[] SingleNote(int displayOrder, string text, byte operationDaysFlag = 0x7F) => [new Note { DisplayOrder = displayOrder, Text = text, OperatingDaysFlag = operationDaysFlag }];
    public override bool Equals(object? obj) => obj is Note other && other.Text.Equals(Text, StringComparison.OrdinalIgnoreCase);
    public override int GetHashCode() => Text.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public bool IsValidDays(byte operatingDaysFlag) => OperatingDaysFlag.And(operatingDaysFlag) > 0;
    public int TextLength => TextLengthRegEx().Replace(Text, string.Empty).Length;
    [GeneratedRegex("<.*?>")]
    private static partial Regex TextLengthRegEx();
}


public abstract class TrainCallNote(int callId)
{
    public int CallId { get; } = callId;
    public int DisplayOrder { get; set; }
    public bool IsDriverNote { get; set; }
    public bool IsStationNote { get; set; }
    public bool IsShuntingNote { get; set; }
    public bool IsForArrival { get; set; }
    public bool IsForDeparture { get; set; }
    public TrainInfo? TrainInfo { get; set; }
    public abstract IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays);

    public bool IsAnyDay(byte days, byte dutyDays = OperationDays.AllDays) =>
       dutyDays.IsOnDemand() || TrainInfo.IsOnDemand() || Days(days, dutyDays) > 0;

    public bool IsNoDay(byte days, byte dutyDays = OperationDays.AllDays) =>
        !dutyDays.IsOnDemand() && !TrainInfo.IsOnDemand() && Days(days, dutyDays) == 0;

    public bool IsAllDays(byte days, byte dutyDays = OperationDays.AllDays) =>
        dutyDays.IsOnDemand() || TrainInfo.IsOnDemand() || Days(days, dutyDays) == dutyDays;

    protected byte Days(byte days, byte dutyDays = OperationDays.AllDays) =>
        Days(days, TrainInfo!.OperationDaysFlags, dutyDays);

    private static byte Days(byte days, byte trainDays, byte dutyDays = OperationDays.AllDays)
    {
        //if (dutyDays < 127) Debugger.Break();
        return trainDays == OperationDays.OnDemand ? OperationDays.OnDemand :
        (byte)(days & trainDays & dutyDays);
    }
}

public sealed class ManualTrainCallNote : TrainCallNote
{
    public ManualTrainCallNote(int callId) : base(callId) => DisplayOrder = 0;
    public byte OperationDayFlag { get; set; } = OperationDays.AllDays;
    public string Text { get; set; } = string.Empty;
    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) =>
        IsAnyDay(OperationDayFlag, onlyDays) ? Note.SingleNote(DisplayOrderWithDays(onlyDays), GetNoteText(onlyDays)) : Enumerable.Empty<Note>();

    private readonly List<LocalizedManualTrainCallNote> LocalizedNotes = [];
    public void AddLocalizedManualTrainCallNote(LocalizedManualTrainCallNote localizedNote) => LocalizedNotes.Add(localizedNote);
    string GetNoteText(byte onlyDays)
    {
        if (LocalizedNotes.Count == 0)
        {
            return FormattedNoteText(Text, onlyDays);
        }
        else
        {
            var localizedNote = LocalizedNotes.FirstOrDefault(x => x.LanguageCode == CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            if (localizedNote is null || string.IsNullOrWhiteSpace(localizedNote.Text)) return FormattedNoteText(Text, onlyDays); ;
            return FormattedNoteText(localizedNote.Text, onlyDays);
        }
    }

    string FormattedNoteText(string text, byte onlyDays) => $"""<span style="font-weight: bold">{GetNoteDays(onlyDays)}</span>{Markdown.ToHtml(text, CreatePipeline())}""";

    private static MarkdownPipeline CreatePipeline()
    {
        var builder = new MarkdownPipelineBuilder();
        builder.ConfigureNewLine("");
        return builder.Build();
    }

    string GetNoteDays(byte onlyDays)
    {
        if (IsAllDays(OperationDayFlag, onlyDays)) return string.Empty;
        if (IsAnyDay(OperationDayFlag, onlyDays)) return $"{Days(OperationDayFlag, onlyDays).OperationDays().ShortName}: ";
        return string.Empty;
    }

    int DisplayOrderWithDays(byte onlyDays) => DisplayOrder + Days(OperationDayFlag, onlyDays);
}

public sealed record LocalizedManualTrainCallNote(string LanguageCode, string? Text);

public sealed class PassengerDepartureCallNote : TrainCallNote
{
    public PassengerDepartureCallNote(int callId) : base(callId)
    {
        DisplayOrder = -12000;
        IsForDeparture = true;
        IsDriverNote = true;
    }

    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) =>
        Note.SingleNote(DisplayOrder, Notes.PickUpPassenger, onlyDays);
}

public sealed class PassengerInterchangeCallNote : TrainCallNote
{
    public PassengerInterchangeCallNote(int callId) : base(callId)
    {
        DisplayOrder = -12000;
        IsForArrival = true;
        IsDriverNote = true;
        IsStationNote = true;
    }
    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) =>
    Note.SingleNote(DisplayOrder, Notes.LeaveOffInterchangingPassengers, onlyDays);

}
public abstract class TrainsetsCallNote(int callId) : TrainCallNote(callId)
{
    protected IList<Trainset> Trainsets { get; } = [];
    public bool IsCargoOnly { init; get; }
    public string? VehicleNumber { init; get; }
    public void AddTrainset(Trainset trainset)
    {
        Trainsets.Add(trainset);
        UpdateVisibility();
    }
    protected virtual void UpdateVisibility()
    {
        IsStationNote = Trainsets.Any(t => !t.IsCargo);
        IsShuntingNote = Trainsets.Any(t => t.IsCargo);
    }
    protected IEnumerable<Note> ToNotes(Func<Trainset, bool> criteria, byte dutyDays = OperationDays.AllDays, int displayOrder = 1000, bool compact = true)
    {
        return Merge(Trainsets.Where(t => criteria(t) && IsAnyDay(t.OperationDaysFlag, dutyDays)))
            .OrderBy(t => t.PositionInTrain).ThenBy(t => t.Number)
            .GroupBy(t => Days(t.OperationDaysFlag, dutyDays))
            .OrderByDescending(t => t.Key)
            .Select(t => new Note
            {
                DisplayOrder = displayOrder,
                Text = Text(t.Key, dutyDays, t)
            });
    }
    protected static IEnumerable<Trainset> Merge(IEnumerable<Trainset> trainsets)
    {
        return trainsets
            .GroupBy(ts => ts.Number)
            .Select(ts => new Trainset
            {
                Class = string.Join(",", ts.Where(t => t.Class.HasValue()).Select(t => t.Class)),
                Destination = ts.First().Destination,
                FinalDestination = ts.First().FinalDestination,
                PositionInTrain = ts.First().PositionInTrain,
                Operator = ts.First().Operator,
                Number = ts.First().Number,
                WagonTypes = ts.First().WagonTypes,
                OperationDaysFlag = (byte)ts.First().OperationDaysFlag,
                HasCoupleNote = ts.First().HasCoupleNote,
                HasUncoupleNote = ts.First().HasUncoupleNote,
                IsCargo = ts.First().IsCargo,
                MaxNumberOfWaggons = ts.Max(x => x.MaxNumberOfWaggons),
                WagonNumbers = string.Join(',', ts.Where(x => x.WagonNumbers.HasValue()).Select(x => x.WagonNumbers)),
            });
    }
    protected string Text(byte days, byte dutyDays, IEnumerable<Trainset> trainsets) =>
         IsAllDays(days, dutyDays) ? Text(trainsets) : $"""<span style="font-weight: bold; background-color: white;">{days.OperationDays()}</span>: {Text(trainsets)}""";

    protected abstract string Text(IEnumerable<Trainset> t);

    protected static string TrainsetTexts(IEnumerable<Trainset> trainsets) =>
        string.Join(" ", trainsets.Select(ts => TrainsetFormat(ts)));

    // TODO: Add final destination and note about exchange trainset under way.
    private static string TrainsetFormat(Trainset ts) =>
        ts.Operator.HasValue() ? $"""|<span style="font-weight: bold; background-color: white;">{ts.Operator}&nbsp;{ts.Class}&nbsp;{WagonSetOrWagon(ts).ToLowerInvariant()}&nbsp;{TrainsetIdentity(ts)}</span>:&nbsp<i>{ts.WagonTypes}</i>|  """ :
        $"""|<span style="font-weight: bold; background-color: white">{ts.Class}&nbsp;{WagonSetOrWagon(ts)}&nbsp;{TrainsetIdentity(ts)}</span>:&nbsp;<i>{ts.WagonTypes}</i>| """;

    private static string TrainsetIdentity(Trainset t) =>
        t.MaxNumberOfWaggons == 1 && t.WagonNumbers.HasValue() ? t.WagonNumbers :
        $"""{Notes.VehicleScheduleNumber.ToLowerInvariant()}&nbsp;{t.Number}""";

    private static string WagonSetOrWagon(Trainset ts) =>
        ts.MaxNumberOfWaggons == 0 ? "" :
        ts.MaxNumberOfWaggons > 1 ? Notes.Wagonset :
        Notes.Wagon;
}

public class TrainsetsDepartureCallNote : TrainsetsCallNote
{
    public TrainsetsDepartureCallNote(int callId) : base(callId)
    {
        IsForDeparture = true;
        DisplayOrder = 11000;
    }
    protected override void UpdateVisibility()
    {
        base.UpdateVisibility();
        IsDriverNote = Trainsets.Any(t => t.HasCoupleNote);
    }

    public override IEnumerable<Note> ToNotes(byte dutyDays = OperationDays.AllDays) =>
        ToNotes(t => t.HasCoupleNote, dutyDays, DisplayOrder);

    protected override string Text(IEnumerable<Trainset> trainsets) =>
        string.Format(CultureInfo.CurrentCulture, Action, TrainsetTexts(trainsets));

    private string Action => IsCargoOnly ? Notes.Load : Notes.ConnectTrainset;
}

public class TrainsetsArrivalCallNote : TrainsetsCallNote
{
    public TrainsetsArrivalCallNote(int callId) : base(callId)
    {
        IsForArrival = true;
        DisplayOrder = 12000;
    }

    protected override void UpdateVisibility()
    {
        base.UpdateVisibility();
        IsDriverNote = Trainsets.Any(t => t.HasUncoupleNote);
    }

    public override IEnumerable<Note> ToNotes(byte dutyDays = OperationDays.AllDays) => ToNotes(t => t.HasUncoupleNote, dutyDays, DisplayOrder);

    protected override string Text(IEnumerable<Trainset> trainsets) =>
        string.Format(CultureInfo.CurrentCulture, Action, TrainsetTexts(trainsets));
    private string Action => IsCargoOnly ? Notes.Unload : Notes.DisconnectTrainset;

}

public class TrainContinuationNumberCallNote : TrainCallNote
{
    public TrainContinuationNumberCallNote(int callId) : base(callId)
    {
        IsStationNote = true;
        IsDriverNote = true;
        IsForArrival = true;
        DisplayOrder = 18000;
    }
    public OtherTrain ContinuingTrain { get; set; } = new OtherTrain();

    public byte LocoOperationDaysFlag { get; set; }

    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) =>
        ContinuingTrain is null || IsNoDay(ContinuingTrain.OperationDayFlag, onlyDays) ?
        [] :
        Note.SingleNote(DisplayOrder, FormatText(ContinuingTrain.OperationDayFlag, onlyDays));


    private string FormatText(byte days, byte onlyDays) =>
        IsAllDays(days, onlyDays) ?
        ContinuingTrain.ContinuesAsTrainToDestination :
        ContinuingTrain.ContinuesDaysAsTrainToDestination;
}

public class TrainMeetCallNote : TrainCallNote
{
    public TrainMeetCallNote(int callId) : base(callId)
    {
        IsStationNote = true;
        IsDriverNote = true;
        IsForArrival = true;
        DisplayOrder = 19000;
    }
    public byte OperationDayFlag { get; set; }
    public IList<OtherTrainCall> MeetingTrains { get; } = new List<OtherTrainCall>();
    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays)
    {
        var meetingTrains = ActualMeetingTrains(onlyDays);
        if (meetingTrains.Any())
        {
            return Note.SingleNote(
                DisplayOrder + Days(MeetingTrains.First().OperationDayFlag, MeetingTrains.Last().OperationDayFlag),
                string.Format(CultureInfo.CurrentCulture, Notes.MeetsOtherTrains,
                string.Join(", ", meetingTrains.Distinct()
                .OrderBy(mt => mt.MeetArrivalTime.OffsetMinutes()).Select(mt => mt.MeetingTrainInfo((byte)(OperationDayFlag & onlyDays))))));
        }
        else
        {
            return Array.Empty<Note>();
        }
    }

    private IEnumerable<OtherTrainCall> ActualMeetingTrains(byte onlyDays) => MeetingTrains.Where(mt => (mt.OperationDayFlag & onlyDays & OperationDayFlag) > 0);
}

public class LocoTurnOrReverseCallNote : LocoArrivalCallNote
{
    public LocoTurnOrReverseCallNote(int callId) : base(callId)
    {
        IsStationNote = true;
        IsDriverNote = true;
        IsForArrival = true;
        DisplayOrder = 3000; //
    }
    public bool Turn { get; set; }
    public bool Reverse { get; set; }

    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays)
    {

        if (Turn && Reverse) { return Note.SingleNote(DisplayOrder, Notes.TurnAndReverseLoco); }
        else if (Turn) { return Note.SingleNote(DisplayOrder, Notes.TurnLoco); }
        else if (Reverse) { return Note.SingleNote(DisplayOrder, Notes.ReverseLoco); }
        else { return []; }
    }
}

public class LocoExchangeCallNote : TrainCallNote
{
    public LocoExchangeCallNote(int callId) : base(callId)
    {
        IsStationNote = true;
        IsDriverNote = true;
        IsForArrival = true;
        DisplayOrder = 6000; //
    }

    public TrainLoco ArrivingLoco { get; set; } = TrainLoco.Empty;
    public TrainLoco DepartingLoco { get; set; } = TrainLoco.Empty;
    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays)
    {
        var days = (byte)(ArrivingLoco.OperationDaysFlags & DepartingLoco.OperationDaysFlags & onlyDays);
        return days == OperationDays.AllDays ? Note.SingleNote(DisplayOrder, string.Format(CultureInfo.CurrentCulture, Notes.EngineChange, ArrivingLoco.DisplayFormat(), DepartingLoco.DisplayFormat()), days) :
            days > 0 || ArrivingLoco.OperationDaysFlags.IsOnDemand() || DepartingLoco.OperationDaysFlags.IsOnDemand() ?
            Note.SingleNote(DisplayOrder, string.Format(CultureInfo.CurrentCulture, Notes.EngineChangeDays, ArrivingLoco.DisplayFormat(), DepartingLoco.DisplayFormat(), days.OperationDays().ShortName), days) :
            [];
    }
}

public abstract class LocoCallNote : TrainCallNote
{
    protected LocoCallNote(int callId) : base(callId)
    {
        IsDriverNote = true;
        IsStationNote = true;
        UseNote = true;
    }

    public bool UseNote { get; set; }
    protected abstract string ParkingText(byte days, byte onlyDays);

    protected static string LocoText(string format, TrainLoco loco) =>
       string.Format(CultureInfo.CurrentCulture, format, loco.DisplayFormat(), loco.OperationDays());
}

public class LocoDepartureCallNote : LocoCallNote
{
    public LocoDepartureCallNote(int callId) : base(callId)
    {
        IsDriverNote = true;
        IsStationNote = true;
        IsForDeparture = true;
        DisplayOrder = 1000; //
    }

    public TrainLoco DepartingLoco { get; set; } = TrainLoco.Empty;
    public bool IsFromParking { get; set; }

    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays)
    {
        var days = (byte)(onlyDays & TrainInfo!.OperationDaysFlags);
        if (days == 0) days = OperationDays.OnDemand;
        return IsAnyDay(DepartingLoco.OperationDaysFlags, onlyDays) ?
            Note.SingleNote(DisplayOrder + Days(DepartingLoco.OperationDaysFlags, onlyDays), ParkingText(DepartingLoco.OperationDaysFlags, days)) :
            [];
    }

    protected override string ParkingText(byte days, byte onlydays) =>
        IsAllDays(days, onlydays) ?
            IsFromParking ?
                LocoText(Notes.GetLocoAtParking, DepartingLoco) :
            LocoText(Notes.UseLoco, DepartingLoco) :
       IsFromParking ?
            LocoText(Notes.GetLocoAtParkingOnDays, DepartingLoco) :
       LocoText(Notes.UseLocoAtDays, DepartingLoco);

}

public class LocoArrivalCallNote : LocoCallNote
{
    public LocoArrivalCallNote(int callId) : base(callId)
    {
        IsDriverNote = true;
        IsStationNote = true;
        IsForArrival = true;
        DisplayOrder = 2000; //
    }
    public TrainLoco ArrivingLoco { get; set; } = TrainLoco.Empty;
    public bool IsToParking { get; set; }
    public bool TurnLoco { get; set; }
    public bool CirculateLoco { get; set; }

    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays)
    {
        var result = new List<Note>();
        if (IsNoDay(ArrivingLoco.OperationDaysFlags, onlyDays)) return result;
        var parkingNote = ParkingText(ArrivingLoco.OperationDaysFlags, onlyDays);

        if (parkingNote.HasValue())
        {
            result.Add(new Note { DisplayOrder = GetDisplayOrder(onlyDays, DisplayOrder), Text = parkingNote });
        }
        return result;
    }

    protected override string ParkingText(byte days, byte onlyDays) =>
        IsToParking ?
            IsAllDays(days, onlyDays) ?
                LocoText(Notes.PutLocoAtParking, ArrivingLoco) :
                LocoText(Notes.PutLocoAtParkingAtDays, ArrivingLoco) :
        string.Empty;

    protected int GetDisplayOrder(byte onlyDays, int sortOrder) => sortOrder + Days(ArrivingLoco.OperationDaysFlags, onlyDays);
}

public class LocoCirculationNote : LocoArrivalCallNote
{
    public LocoCirculationNote(int callId) : base(callId)
    {
        DisplayOrder = 5000; //
    }
    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) =>
        IsNoDay(ArrivingLoco.OperationDaysFlags, onlyDays) ? Enumerable.Empty<Note>() :
        Note.SingleNote(GetDisplayOrder(onlyDays, DisplayOrder), CirculateText(ArrivingLoco.OperationDaysFlags, onlyDays));

    private string CirculateText(byte days, byte onlyDays) =>
        CirculateLoco ?
            IsAllDays(days, onlyDays) ? Notes.CirculateLoco :
            string.Format(Notes.CirculateLocoDays, Days(ArrivingLoco.OperationDaysFlags, onlyDays).OperationDays().ShortName) :
            string.Empty;
}

public class LocoTurnNote : LocoArrivalCallNote
{
    public LocoTurnNote(int callId) : base(callId)
    {
        DisplayOrder = 4000; //
    }
    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) =>
        IsNoDay(ArrivingLoco.OperationDaysFlags, onlyDays) ? Enumerable.Empty<Note>() :
        Note.SingleNote(GetDisplayOrder(onlyDays, DisplayOrder), Notes.TurnLoco);

}

public class BlockOriginCallNote : TrainCallNote
{
    public BlockOriginCallNote(int callId) : base(callId)
    {
        IsShuntingNote = true;
        IsDriverNote = true;
        IsForArrival = true;
        DisplayOrder = 21000; //
    }
    private readonly List<string> OriginNames = [];
    public void AddOriginName(string originName) => OriginNames.Add(originName);
    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) =>
        OriginNames.Any() ? Note.SingleNote(DisplayOrder, string.Format(CultureInfo.CurrentCulture, Notes.ConnectWagonsFrom, string.Join(", ", OriginNames))) :
        [];
}
public class BlockDestinationsCallNote : TrainCallNote
{
    public BlockDestinationsCallNote(int callId) : base(callId)
    {
        IsShuntingNote = true;
        IsDriverNote = true;
        IsForDeparture = true;
        DisplayOrder = 20000;
    }

    public IList<BlockDestinationDetailed> BlockDestinations { get; } = [];
    private string[] TransferOrigins => [.. BlockDestinations.Where(bd => bd.TransferOriginName.HasValue()).Select(bd => bd.TransferOriginName ?? string.Empty).Distinct()];

    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) =>
        TransferOrigins.Length != 0 ? Note.SingleNote(DisplayOrder, $"{string.Format(Notes.BringsWaggonsFromOriginsToDestinations, BlockDestinations.DestinationText(true), string.Join(", ", TransferOrigins))} {Notes.BringsNoWagonsFromThisStation}"):
        Note.SingleNote(DisplayOrder, string.Format(CultureInfo.CurrentCulture, Notes.BringsWaggonsToDestinations, BlockDestinations.DestinationText(true)));

}

public class BlockArrivalCallNote : TrainCallNote
{
    public BlockArrivalCallNote(int callId) : base(callId)
    {
        IsShuntingNote = true;
        IsDriverNote = true;
        IsForArrival = true;
        DisplayOrder = 21000;//
    }
    public IList<BlockDestination> BlockDestinations = [];
    public string StationName => StationNames.Count > 0 ? StationNames[0] : string.Empty;
    public IList<string> StationNames { get; } = new List<string>();
    public bool ToAllDestinations { get; set; }
    public bool IsTransfer { get; set; }
    public bool AndBeyond { get; set; }
    public bool AlsoSwitch { get; set; }
    public bool AtShadowStation { get; set; }
    public int OrderInTrain { get; set; }

    public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays)
    {
        var result = new List<Note>();
        if (AlsoSwitch)
        {
            if (AtShadowStation)
            {
                result.Add(new Note { DisplayOrder = -801, Text = $"{Notes.MoveContinuingWagonsToDepartureTracks} {Notes.MoveRestOfWagonsToTable}" });
            }
            else
            {
                if (AndBeyond)
                {
                    result.Add(new Note { DisplayOrder = -801, Text = Notes.DisconnectContinuingWagons });
                }
                result.Add(new Note { DisplayOrder = -800, Text = Notes.SwitchWagonsToCustomers });
            }
        }
        else
        {
            result.Add(new Note { DisplayOrder = IsTransfer ? -900 : -700, Text = ToString() });
        }
        return result;
    }

    public override string ToString() =>
        ToAllDestinations ?
        string.Format(CultureInfo.CurrentCulture, Notes.DisconnectWagonsToHere, Notes.AllDestinations) :
        AndBeyond ? string.Format(CultureInfo.CurrentCulture, Notes.DisconnectWagonsToHereAndFurther, BlockDestinations.ArrivalText()) :
        string.Format(CultureInfo.CurrentCulture, Notes.DisconnectWagonsToHere, BlockDestinations.ArrivalText());
}

public class BlockDestination
{
    public string StationName { get; set; } = string.Empty;
    public string ForeColor { get; set; } = "#000000";
    public string BackColor { get; set; } = "#FFFFFF";
}
public class BlockDestinationDetailed : BlockDestination
{
    public int StationId { get; set; }
    public string? TransferDestinationName { get; set; }
    public string? TransferOriginName { get; set; }
    public bool ToAllDestinations { get; set; }
    public bool AndBeyond { get; set; }
    public bool AndLocalDestinations { get; set; }
    public int OrderInTrain { get; set; }
    public int MaxNumberOfWagons { get; set; }
    public bool TransferAndBeyond { get; set; }
    public string DestinationCountryName { get; set; } = string.Empty;
    public bool IsInternational { get; set; }
    public bool IsRegion { get; set; }
    public bool IsPassenger { get; set; }
    public bool IsCargo { get; set; }
    public bool IsTrainset { get; set; }
    public int TrainsetNumber { get; set; }
    public string? TrainsetOperatorName { get; set; }
    public byte TrainsetOperationDaysFlag { get; set; }
    public string? Note { get; set; }
    public bool HasCouplingNote { get; set; }
    public bool HasUncouplingNote { get; set; }
    public override string ToString() =>
        IsTrainset && TrainsetOperationDaysFlag.IsAllDays() ? $"{TrainsetOperatorName} {Notes.Turnus} {TrainsetNumber}: {FinalDestinationStationName}" :
        IsTrainset ? $"{TrainsetOperationDaysFlag.OperationDays().ShortName}: {TrainsetOperatorName} {Notes.Turnus} {TrainsetNumber}: {FinalDestinationStationName}" :
        IsRegion ? DestinationColourized :
        ToAllDestinations ? AllDestinations :
        BeyondAndOrLocalDestinations;

    private string BeyondAndOrLocalDestinations =>
        (AndBeyond || TransferAndBeyond) && AndLocalDestinations ? string.Format(CultureInfo.CurrentCulture, Notes.LocalDestionationsAndBeyond, DestinationColourized) :
        AndLocalDestinations ? string.Format(CultureInfo.CurrentCulture, Notes.LocalDestinations, DestinationColourized) :
        AndBeyond || TransferAndBeyond ? string.Format(CultureInfo.CurrentCulture, Notes.AndBeyond, DestinationColourized) :
        DestinationColourized;

    internal string AllDestinations => UseDestinationCountry ? string.Format(Notes.DestinationInCountry, Notes.AllDestinations, DestinationCountryName) : Notes.AllDestinations;
    internal bool UseDestinationCountry => HasDestinationCountry && IsInternational;
    internal bool HasDestinationCountry => !string.IsNullOrWhiteSpace(DestinationCountryName);
    internal string FinalDestinationStationName => IsRegion ? StationName : string.IsNullOrWhiteSpace(TransferDestinationName) ? StationName : TransferDestinationName;
    internal string DestinationText => UseDestinationCountry ? string.Format(Notes.DestinationInCountry, FinalDestinationStationName, DestinationCountryName) : FinalDestinationStationName;
    internal string DestinationColourized =>
        ForeColor == "#FFFFFF" && BackColor == "#777777" ? $"""<span style="font-weight: bold; padding: 0px 2px;">{DestinationText}</span>""" :
         $"""<span style="font-weight: bold; padding: 0px 2px; color: {ForeColor}; background-color: {BackColor}">{DestinationText}</span>""";


    public override bool Equals(object? obj) => obj is BlockDestinationDetailed other && other.ToString().Equals(ToString(), StringComparison.OrdinalIgnoreCase);
    public override int GetHashCode() => ToString().GetHashCode(StringComparison.OrdinalIgnoreCase);
}

public static class BlockDestinationsExtensions
{
    public static string ArrivalText(this IEnumerable<BlockDestination> destinations) =>
        string.Join(" ", destinations.OrderBy(d => d.BackColor).Select(x => x.ArrivalText()));
    public static string ArrivalText(this BlockDestination destination) =>
        destination.ForeColor == "#FFFFFF" && destination.BackColor == "#777777" ? $"""<span style="font-weight: bold; padding: 0px 2px;">{destination.StationName}</span>""" :
         $"""<span style="font-weight: bold; padding: 0px 2px; color: {destination.ForeColor}; background-color: {destination.BackColor}">{destination.StationName}</span>""";

    public static string DestinationText(this IEnumerable<BlockDestinationDetailed> me, bool useBrackets = false) =>
        string.Join(", ", me.GroupText(useBrackets));

    public static IEnumerable<string> GroupText(this IEnumerable<BlockDestinationDetailed> me, bool useBrackets = false)
    {
        var result = new List<string>();
        var destinationGroups =
            me.OrderBy(dg => dg.OrderInTrain)
            .GroupBy(bd => bd.StationId * 100000 + bd.OrderInTrain * 1000);
        foreach (var destinationGroup in destinationGroups)
        {
            var text = new StringBuilder(200);
            var destinations = destinationGroup.OrderBy(dg => !dg.IsRegion).ThenBy(dg => dg.MaxNumberOfWagons).ToArray();
            var destinationTextsInGroup = destinations.Select(d => d.ToString()).Distinct();
            if (useBrackets) text.Append('|');
            text.Append(string.Join(" ", destinationTextsInGroup));
            //if (destinationGroup.First().AndLocalDestinations)
            //{
            //    text.Append(' ');
            //    text.Append(Notes.LocalDestinations);
            //}
            if (useBrackets) text.Append('|');
            var maxNumberOfWagons = destinations.Sum(d => d.MaxNumberOfWagons);
            if (maxNumberOfWagons > 0)
            {
                text.Append('×');
                text.Append(maxNumberOfWagons);
            }
            result.Add(text.ToString());
        }
        return result;
    }
}

public class OtherTrain
{
    public string? CategoryPrefix { get; set; }
    public int TrainNumber { get; set; }
    public byte OperationDayFlag { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    public string ContinuesAsTrainToDestination => string.Format(CultureInfo.CurrentCulture, Notes.ContinuesAsTrainToDestination, $"{CategoryPrefix} {TrainNumber}", DestinationName);
    public string ContinuesDaysAsTrainToDestination => string.Format(CultureInfo.CurrentCulture, Notes.ContinuesDaysAsTrainToDestination, $"{CategoryPrefix} {TrainNumber}", DestinationName, OperationDayFlag.OperationDays().ShortName);
    public virtual string MeetingTrainInfo(byte otherTrainDaysFlags)
    {
        byte commonDaysFlag = (byte)(OperationDayFlag & otherTrainDaysFlags);
        return commonDaysFlag == otherTrainDaysFlags ?
            $"{CategoryPrefix} {TrainNumber}" :
            $"{commonDaysFlag.OperationDays().ShortName} {CategoryPrefix} {TrainNumber}";
    }
    public override string ToString() => $"{OperationDayFlag.OperationDays().ShortName} {CategoryPrefix} {TrainNumber}";
}

public class OtherTrainCall : OtherTrain
{
    public CallTime? ArrivalTime { get; set; }
    public CallTime? DepartureTime { get; set; }
    public CallTime? MeetArrivalTime { get; set; }
    public CallTime? MeetDepartureTime { get; set; }
    public override string MeetingTrainInfo(byte otherTrainDaysFlags)
    {
        byte commonDaysFlag = (byte)(OperationDayFlag & otherTrainDaysFlags);
        var arrivalTime = ArrivalTime.OffsetMinutes() > MeetArrivalTime.OffsetMinutes() ? ArrivalTime : MeetArrivalTime;
        var departureTime = DepartureTime.OffsetMinutes() < MeetDepartureTime.OffsetMinutes() ? DepartureTime : MeetDepartureTime;
        return commonDaysFlag == 0 ? string.Empty :
            commonDaysFlag == otherTrainDaysFlags ?
            $"{CategoryPrefix} {TrainNumber} {arrivalTime}-{departureTime}" :
            $"{commonDaysFlag.OperationDays().ShortName} {CategoryPrefix} {TrainNumber} {arrivalTime}-{departureTime}";
    }
}


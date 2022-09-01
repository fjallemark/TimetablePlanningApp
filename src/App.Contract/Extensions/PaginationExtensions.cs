using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Tellurian.Trains.Planning.App.Contracts.Extensions;

public static class PaginationExtensions
{
    public static int TotalPages<T>(this IEnumerable<T> me, int itemPerPage) =>
        me is null ? 0 : me.Count() % itemPerPage == 0 ? me.Count() / itemPerPage : me.Count() / itemPerPage + 1;

    private static IEnumerable<T> Page<T>(this IEnumerable<T> me, int itemPerPage, int pageNumber) =>
        pageNumber < 1 || pageNumber > me.TotalPages(itemPerPage) ? Array.Empty<T>() :
        me.Skip((pageNumber - 1) * itemPerPage).Take(itemPerPage);

    public static IEnumerable<IEnumerable<T>> ItemsPerPage<T>(this IEnumerable<T> me, int itemsPerPage)
    {
        var totalPages = me.TotalPages(itemsPerPage);
        return Enumerable.Range(1, totalPages).Select(page => me.Page(itemsPerPage, page));
    }

    #region DriverDutyBooklet

    public static IEnumerable<DriverDutyPage> GetAllDriverDutyPagesInBookletOrder(this IEnumerable<DriverDuty> me, IEnumerable<Instruction> instructions) =>
        me.SelectMany(d => d.GetDriverDutyPagesInBookletOrder(instructions)).ToList();


    public static IEnumerable<DriverDutyPage> GetDriverDutyPagesInBookletOrder(this DriverDuty me, IEnumerable<Instruction> instructions)
    {
        var pages = me.GetDriverDutyPages(instructions).ToArray();
        var bookletPageOrder = BookletPageOrder(pages.Length);
        var result = new List<DriverDutyPage>();
        for (var i = 0; i < bookletPageOrder.Length; i++)
        {
            result.Add(pages[bookletPageOrder[i] - 1]);
        }
        return result;

    }

    public static IEnumerable<DriverDutyPage> GetDriverDutyPages(this DriverDuty me, IEnumerable<Instruction> instructions)
    {
        var pageNumber = 1;
        var result = new List<DriverDutyPage> { DriverDutyPage.Front(pageNumber++, me) };
        var instruction = instructions.LanguageOrInvariantInstruction();
        var dutyParts = me.Parts.OrderBy(p => p.StartTime()).ToArray();
        dutyParts.Last().IsLastPart = true;
        for (var i = 0; i < me.Parts.Count; i++)
        {
            result.Add(DriverDutyPage.Part(pageNumber++, me, dutyParts[i]));
        }
        result.AddRange(BlankPagesToAppend<DriverDutyPage>(result.Count, instruction is null ? 0 : 1));
        pageNumber = result.Count + 1;
        if (instruction is not null) result.Add(DriverDutyPage.Instructions(pageNumber++, instruction.Markdown, $"{Resources.Notes.Instructions} {Resources.Notes.Driver}"));
        return result;
    }

    #endregion

    #region StationDutyBooklet

    public static IEnumerable<StationDutyPage> GetAllStationDutyPagesInBookletOrder(this IEnumerable<StationDuty> me, IEnumerable<Instruction> instructions) =>
        me.SelectMany(d => d.GetStationDutyPagesInBookletOrder(instructions)).ToList();
    public static IEnumerable<StationDutyPage> GetStationDutyPagesInBookletOrder(this StationDuty me, IEnumerable<Instruction> instructions)
    {
        var pages = me.GetStationDutyPages(instructions).ToArray();
        var bookletPageOrder = BookletPageOrder(pages.Length);
        var result = new List<StationDutyPage>();
        for (var i = 0; i < bookletPageOrder.Length; i++)
        {
            result.Add(pages[bookletPageOrder[i] - 1]);
        }
        return result;
    }

    public static IEnumerable<StationDutyPage> GetStationDutyPages(this StationDuty me, IEnumerable<Instruction> instructions)
    {
        var pageNumber = 1;
        var result = new List<StationDutyPage> { StationDutyPage.Front(pageNumber++, me) };
        var instruction = instructions.LanguageOrInvariantInstruction();

        //if (instruction is not null) result.Add(StationDutyPage.Instructions(pageNumber++, instruction.Markdown));
        if (me.StationInstructions is not null && me.StationInstructions.Any(i => i.Markdown.HasValue()))
            result.Add(StationDutyPage.Instructions(pageNumber++, me.StationInstructions!.LanguageOrInvariantInstruction().Markdown, $"{Resources.Notes.Instructions} {Resources.Notes.TrainClearance}"));

        if (me.ShuntingInstructions is not null && me.ShuntingInstructions.Any(i => i.Markdown.HasValue()))
            result.Add(StationDutyPage.Instructions(pageNumber++, me.ShuntingInstructions!.LanguageOrInvariantInstruction().Markdown, $"{Resources.Notes.Instructions} {Resources.Notes.Shunting}"));

        const int maxRowsOnPage = 26;
        var callsCount = me.Calls.Count - 1;
        var usedPageRows = 0;
        var fromCallIndex = 0;
        var toCallIndex = 0;
        foreach (var call in me.Calls)
        {
            usedPageRows += call.Rows;
            if (usedPageRows > maxRowsOnPage)
            {
                result.Add(StationDutyPage.TrainCalls(pageNumber++, me.Calls.Skip(fromCallIndex).Take(toCallIndex - fromCallIndex).ToList()));
                fromCallIndex = toCallIndex;
                usedPageRows = 0;
            }
            toCallIndex++;
        }
        if (fromCallIndex < toCallIndex) result.Add(StationDutyPage.TrainCalls(pageNumber++, me.Calls.Skip(fromCallIndex).Take(toCallIndex - fromCallIndex).ToList()));
        result.AddRange(BlankPagesToAppend<StationDutyPage>(result.Count));
        return result;
    }



    #endregion

    #region Utility functions

    private static Instruction LanguageOrInvariantInstruction(this IEnumerable<Instruction> instructions)
    {
        if (instructions.Any())
        {
            var languageInstruction = instructions.SingleOrDefault(i => i.Language is not null && i.Language.Equals(CultureInfo.CurrentCulture.TwoLetterISOLanguageName));
            if (languageInstruction is null) return instructions.First();
            return languageInstruction;
        }
        return new() { Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName, Markdown = "" };
    }

    private static IEnumerable<TDutyPage> BlankPagesToAppend<TDutyPage>(int afterPageNumber, int uptoPageNumber = 0) where TDutyPage : DutyPage
    {
        var totalPages = afterPageNumber <= 4 - uptoPageNumber ? 4 - uptoPageNumber : afterPageNumber <= 8 - uptoPageNumber ? 8 - uptoPageNumber : 12 - uptoPageNumber;
        var blankPagesToAppend = totalPages - afterPageNumber;
        if (blankPagesToAppend == 0) return Array.Empty<TDutyPage>();
        return Enumerable.Range(1, blankPagesToAppend).Select(i => (TDutyPage)DutyPage.Blank<TDutyPage>(afterPageNumber + i));
    }

    private static int[] BookletPageOrder(int numberOfPages) =>
        numberOfPages switch
        {
            4 => FourPageOrder,
            8 => EightPageOrder,
            12 => TwelvePageOrder,
            _ => throw new ArgumentOutOfRangeException(nameof(numberOfPages))
        };
    private static int[] FourPageOrder => new[] { 4, 1, 2, 3 };
    private static int[] EightPageOrder => new[] { 8, 1, 2, 7, 6, 3, 4, 5 };
    private static int[] TwelvePageOrder => new[] { 12, 1, 2, 11, 10, 3, 4, 9, 8, 5, 6, 7 };

    #endregion
}

public class DutyPage
{
    protected DutyPage(int number, string? instructionsMarkdown, string? instructionsHeading = null)
    {
        Number = number;
        InstructionsMarkdown = instructionsMarkdown;
        InstructionsHeading = instructionsHeading;
    }

    protected DutyPage(int number)
    {
        Number = number;
    }

    public int Number { get; }
    public virtual bool IsBlank { get; } = false;
    public virtual bool IsFront { get; } = false;
    public virtual bool IsPart { get; } = false;
    public string? InstructionsMarkdown { get; }
    public string? InstructionsHeading { get; }
    public virtual bool IsInstructions => !string.IsNullOrWhiteSpace(InstructionsMarkdown);

    public static DutyPage Blank<TDutyPage>(int pageNumber) where TDutyPage : DutyPage =>
        typeof(TDutyPage) == typeof(StationDutyPage) ? StationDutyPage.Blank(pageNumber) : DriverDutyPage.Blank(pageNumber);
}

public sealed class DriverDutyPage : DutyPage
{
    private DriverDutyPage(int number) : base(number) { }
    private DriverDutyPage(int number, string? instructionsMarkdown, string? instructionsHeading = null) : base(number, instructionsMarkdown, instructionsHeading) { }
    private DriverDutyPage(int number, DriverDuty duty) : base(number) { Duty = duty; }
    private DriverDutyPage(int number, DriverDuty duty, DriverDutyPart part) : base(number) { Duty = duty; DutyPart = part; }
    public DriverDuty? Duty { get; }
    public DriverDutyPart? DutyPart { get; }
    public override bool IsBlank => Duty is null && DutyPart is null && !IsInstructions;
    public override bool IsFront => Duty is not null && DutyPart is null;
    public override bool IsPart => DutyPart is not null && Duty is not null;

    public static DriverDutyPage Blank(int number) => new(number);
    public static DriverDutyPage Front(int number, DriverDuty duty) => new(number, duty);
    public static DriverDutyPage Part(int number, DriverDuty duty, DriverDutyPart part) => new(number, duty, part);
    public static DriverDutyPage Instructions(int number, string? instructionsMarkdown, string? instructionsHeading = null) =>
        new(number, instructionsMarkdown, instructionsHeading);
}

public sealed class StationDutyPage : DutyPage
{
    private StationDutyPage(int number) : base(number) { }
    private StationDutyPage(int number, string? instructionsMarkdown, string? instructionsHeading) : base(number, instructionsMarkdown, instructionsHeading) { }
    public StationDuty? Duty { get; init; }

    public override bool IsBlank => Duty is null && !IsInstructions && !IsCallsPage;
    public override bool IsFront => Duty is not null;
    public override bool IsPart => IsCallsPage;
    private bool IsCallsPage { get; init; }

    public IEnumerable<StationCallWithAction> Calls { get; init; } = Enumerable.Empty<StationCallWithAction>();

    public static StationDutyPage Blank(int number) => new(number);
    public static StationDutyPage Front(int number, StationDuty duty) => new(number) { Duty = duty };
    public static StationDutyPage Instructions(int number, string? instructionsMarkdown, string? instructionsHeading = null) => new(number, instructionsMarkdown, instructionsHeading) { };
    //public static StationDutyPage TrainCalls(int number, int fromCallIndex, int toCallIndex) => new(number, fromCallIndex, toCallIndex) { };
    public static StationDutyPage TrainCalls(int number, IEnumerable<StationCallWithAction> calls) => new(number) { Calls = calls, IsCallsPage = true };

}

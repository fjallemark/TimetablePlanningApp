﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Tellurian.Trains.Planning.App.Contracts
{
    public static class PaginationExtensions
    {
        public static int TotalPages<T>(this IEnumerable<T> me, int itemPerPage) =>
            me is null ? 0 : me.Count() % itemPerPage == 0 ? me.Count() / itemPerPage : (me.Count() / itemPerPage) + 1;

        public static IEnumerable<T> Page<T>(this IEnumerable<T> me, int itemPerPage, int pageNumber) =>
            pageNumber < 1 || pageNumber > me.TotalPages(itemPerPage) ? Array.Empty<T>() :
            me.Skip((pageNumber - 1) * itemPerPage).Take(itemPerPage);

        public static IEnumerable<IEnumerable<T>> ItemsPerPage<T>(this IEnumerable<T> me, int itemsPerPage)
        {
            var totalPages = me.TotalPages(itemsPerPage);
            return Enumerable.Range(1, totalPages).Select(page => me.Page(itemsPerPage, page));
        }

        public static IEnumerable<DutyPage> GetAllDriverDutyPagesInBookletOrder(this IEnumerable<DriverDuty> me, IEnumerable<LayoutInstruction> instructions) =>
            me.SelectMany(d => d.GetDriverDutyPagesInBookletOrder(instructions)).ToList();

        public static IEnumerable<DutyPage> GetDriverDutyPagesInBookletOrder(this DriverDuty me, IEnumerable<LayoutInstruction> instructions)
        {
            var pages = me.GetDriverDutyPages(instructions).ToArray();
            var bookletPageOrder = BookletPageOrder(pages.Length);
            var result = new List<DutyPage>();
            for (int i = 0; i < bookletPageOrder.Length; i++)
            {
                result.Add(pages[bookletPageOrder[i] - 1]);
            }
            return result;
        }

        public static IEnumerable<DutyPage> GetDriverDutyPages(this DriverDuty me, IEnumerable<LayoutInstruction> instructions)
        {
            var pageNumber = 1;
            var result = new List<DutyPage>
            {
                DutyPage.Front(pageNumber++, me)
            };
            if (instructions.Any())
            {
                var languageInstruction = instructions.SingleOrDefault(i => i.Language is not null && i.Language.Equals(CultureInfo.CurrentCulture.TwoLetterISOLanguageName));
                if (languageInstruction is null)
                {
                    result.Add(DutyPage.Instructions(pageNumber++, instructions.First().Markdown));

                }
                else
                {
                    result.Add(DutyPage.Instructions(pageNumber++, languageInstruction.Markdown));
                }
            }
            var dutyParts = me.Parts.OrderBy(p => p.StartTime()).ToArray();
            dutyParts.Last().IsLastPart = true;
            for (int i = 0; i < me.Parts.Count; i++)
            {
                result.Add(DutyPage.Part(pageNumber++, me, dutyParts[i]));
            }
            result.AddRange(BlankPagesToAppend(result.Count));
            return result;
        }

        private static IEnumerable<DutyPage> BlankPagesToAppend(int afterPageNumber)
        {
            var totalPages = afterPageNumber <= 4 ? 4 : afterPageNumber <= 8 ? 8 : 12;
            var blankPagesToAppend = totalPages - afterPageNumber;
            if (blankPagesToAppend == 0) return Array.Empty<DutyPage>();
            return Enumerable.Range(1, blankPagesToAppend).Select(i => DutyPage.Blank(afterPageNumber + i));
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
    }

    public abstract class Page
    {
        protected Page(int number) { Number = number; }
        public int Number { get; }
    }

    public sealed class DutyPage : Page
    {
        private DutyPage(int number) : base(number) { }
        private DutyPage(int number, DriverDuty duty) : base(number) { Duty = duty; }
        private DutyPage(int number, DriverDuty duty, DutyPart part) : base(number) { Duty = duty; DutyPart = part; }
        private DutyPage(int number, string? markdown) : base(number) { InstructionsMarkdown = markdown; }
        public DriverDuty? Duty { get; }
        public DutyPart? DutyPart { get;}
        public string? InstructionsMarkdown { get; }
        public bool IsBlank => Duty == null && DutyPart == null && !IsInstructions;
        public bool IsFront => Duty != null && DutyPart == null;
        public bool IsPart => DutyPart != null && Duty != null;
        public bool IsInstructions => !string.IsNullOrWhiteSpace(InstructionsMarkdown);
        public static DutyPage Blank(int number) => new DutyPage(number);
        public static DutyPage Front(int number, DriverDuty duty) => new DutyPage(number, duty);
        public static DutyPage Part(int number, DriverDuty duty, DutyPart part) => new DutyPage(number, duty, part);
        public static DutyPage Instructions(int number, string? markdown) => new DutyPage(number, markdown);
    }
}
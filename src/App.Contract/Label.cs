﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Tellurian.Trains.Planning.App.Contract
{
    public class Label
    {
        public string ResourceKey { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class LanguageLabels
    {
        public string LanguageCode { get; set; } = string.Empty;
        public ICollection<Label> Labels { get; set; } = Array.Empty<Label>();
    }

    public static class LanguageLabelsExtensions
    {
        public static IDictionary<string, string> Texts(this LanguageLabels me) => me.Labels.ToDictionary(v => v.ResourceKey, v => v.Text);
        public static LanguageLabels CreateLabels(this string languageCode, IEnumerable<string> resourceKeys)
        {
            var culture = new CultureInfo(languageCode);
            var resourceManager = Resources.Notes.ResourceManager;
            return new LanguageLabels
            {
                LanguageCode = languageCode,
                Labels = resourceKeys.Select(k => new Label { ResourceKey = k, Text = resourceManager.GetString(k, culture) ?? k }).ToArray()
            };
        }

        public static string GetLabelText(this LanguageLabels me, string resourceKey) =>
            me.Labels.Single(l => l.ResourceKey == resourceKey).Text;

        public static string GetLabelText(this IEnumerable<LanguageLabels> me, string languageCode, string resourceKey) =>
            me.Single(l => l.LanguageCode == languageCode).GetLabelText(resourceKey);
    }
}

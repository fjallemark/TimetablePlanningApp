using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;

namespace Tellurian.Trains.Planning.App.Server.Tests
{
    [TestClass]
    public class LanguageTests
    {
        [TestMethod]
        public void GetsNorwegianText()
        {
            var actual = GetLanguageString("no", "SpecialFreightTrain", "Special freight train");
            Assert.AreEqual("Special freight train/Spesialgodstog", actual);
        }


        private static string GetLanguageString(string? language, string key, string? englishText = null)
        {
            if (language is null) return key;
            if (englishText is null) englishText = key;
            var r = new ResourceManager(typeof(Contract.Resources.Notes));
            var c = new CultureInfo(language);

            var v = r.GetString(key, c);
            if (v is null) return key;
            return v.Equals(englishText, StringComparison.OrdinalIgnoreCase) ? englishText : $"{englishText}/{v}";
        }
    }
}

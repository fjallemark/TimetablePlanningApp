using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Tellurian.Trains.Planning.App.Server.Tests;

[TestClass]
public class LanguageTests
{
    [TestMethod]
    public void GetsNorwegianText()
    {
        var actual = GetLanguageString("nb", "SpecialFreightTrain", "Special freight train");
        Assert.AreEqual("Special freight train/Spesialgodstog", actual);
    }

    private static string GetLanguageString(string? language, string key, string? englishText = null)
    {
        if (language is null) return key;
        if (englishText is null) englishText = key;
        var r = new ResourceManager("Tellurian.Trains.Planning.App.Contracts.Resources.Notes", Assembly.GetAssembly(typeof(Contracts.Resources.Notes))! );
        var c = new CultureInfo(language);

        var v = r.GetString(key, c);
        if (v is null) return key;
        return v.Equals(englishText, StringComparison.OrdinalIgnoreCase) ? englishText : $"{englishText}/{v}";
    }
}

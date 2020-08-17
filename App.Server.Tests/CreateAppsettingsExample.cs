using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Planning.Repositories;

namespace App.Server.Tests
{
    [TestClass]
    public class CreateAppsettingsExample
    {
        [TestMethod]
        public void CreateSettingsExample()
        {
            var target = new AppSettings();
            using var stream = new FileStream("appsettings.Example.json", FileMode.Create);
            var options = new JsonSerializerOptions() { WriteIndented = true };
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented=true });
            JsonSerializer.Serialize(writer, target, options);
        }

        public class AppSettings
        {
            public Logging Logging { get; set; } = new Logging();
            public string AllowedHosts { get; set; } = "*";
            public RepositoryOptions RepositoryOptions { get; set; } = new RepositoryOptions
            {
                //ConnectionString = @"Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=C:\\Users\\Stefan\\OneDrive\\Modellj�rnv�g\\Tr�ffar\\2020\\2020-05 G�vle\\Trafikplanering\\Timetable.accdb;Uid=Admin;Pwd=;"
                 ConnectionString = @"Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=C:\\Users\\Stefan\\OneDrive\\Modellj�rnv�g\\Tr�ffar\\2020\\2020-03 Grimsl�v\\Trafikplanering\\Timetable.accdb;Uid=Admin;Pwd=;"
            };
        }

        public class Logging
        {
            public LogLevel LogLevel { get; set; } = new LogLevel();
        }

        public class LogLevel
        {
            public string Default { get; set; } = "Information";
            public string Microsoft { get; set; } = "Warning";
            [JsonPropertyName("Microsoft.Hosting.Lifetime")]
            public string MicrosoftHostingLifetime { get; set; } = "Information";
        }
    }
}
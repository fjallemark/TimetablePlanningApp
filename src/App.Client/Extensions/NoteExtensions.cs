using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Client.Extensions;

public static class NoteExtensions
{
    public static string CssClass(this Note me) =>
        me.IsShortNote?
        "train call shortnote" :
        "train call longnote";
}

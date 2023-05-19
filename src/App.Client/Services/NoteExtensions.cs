using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Client;

public static class NoteExtensions
{
    public static string CssClass(this Note me) =>
        me.Text.Length > 40 ?
        "train call longnote" :
        "train call shortnote";
}

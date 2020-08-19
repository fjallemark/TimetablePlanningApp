using Tellurian.Trains.Planning.App.Shared;

namespace Tellurian.Trains.Planning.App.Client
{
    public static class NoteExtensions
    {
        public static string CssClass(this Note me) =>
            me.Text.Length > 40 ?
            "duty call longnote" :
            "duty call shortnote";
    }
}

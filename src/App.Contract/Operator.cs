

using System.Globalization;

namespace Tellurian.Trains.Planning.App.Contracts;
public class Operator
{
    public required string Signature { get; init; }
    public required string CountryCode { get; init; }
    public required string LanguageCode { get; init; }
}

public static class OperatorExtensions
{
    // TODO: Move hardcoded to database
    public static IEnumerable<Operator> OperatorsList =>
        [   
            new() {
                Signature = "SJ",
                CountryCode = "se",
                LanguageCode = "sv",
            },
            new(){
                Signature="TGOJ",
                CountryCode = "se",
                LanguageCode = "sv",
            },
            new(){
                Signature="GL",
                CountryCode = "se",
                LanguageCode = "sv",
            },
            new(){
                Signature="NSB",
                CountryCode = "no",
                LanguageCode = "nb-NO",
            }
        ];

    public static IDictionary<string, Operator> Operators =>
        OperatorsList.ToDictionary(o => o.Signature);

    public static CultureInfo CultureInfo(this Operator o) => new(o.LanguageCode);

    public static void SetCulture(this Operator o)
    {
        System.Globalization.CultureInfo.CurrentCulture = o.CultureInfo();
        System.Globalization.CultureInfo.CurrentUICulture = o.CultureInfo();
    }

    public static Operator? OperatorFromSignature(this string signature) =>
        OperatorsList.SingleOrDefault(o => o.Signature.Equals(signature, StringComparison.OrdinalIgnoreCase));
        
}

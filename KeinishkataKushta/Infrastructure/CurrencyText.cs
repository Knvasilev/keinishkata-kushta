using System.Globalization;

namespace KeinishkataKushta.Infrastructure;

public static class CurrencyText
{
    public const decimal LevPerEuro = 1.95583m;
    private static readonly DateTime DualDisplayEndDate = new(2026, 8, 8);

    public static bool ShowLevEquivalent()
    {
        return DateTime.Today <= DualDisplayEndDate;
    }

    public static string Euro(HttpContext context, decimal amount)
    {
        return $"{amount.ToString("N2", Culture(context))} €";
    }

    public static string LevEquivalent(HttpContext context, decimal euroAmount)
    {
        var levAmount = decimal.Round(euroAmount * LevPerEuro, 2, MidpointRounding.AwayFromZero);
        return $"{levAmount.ToString("N2", Culture(context))} лв.";
    }

    private static CultureInfo Culture(HttpContext context)
    {
        return CultureInfo.GetCultureInfo(SiteText.Lang(context) == "bg" ? "bg-BG" : "en-IE");
    }
}

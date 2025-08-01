namespace Simple_Account_Service.Application.ForFakesAndDummies;

public class Currency
{
    private static HashSet<string> AllowedCurrencies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "USD",
        "EUR",
        "RUB",
        "JPY", 
        "GBP", 
        "CHF", 
        "CNY", 
        "AUD", 
        "CAD", 
        "NZD", 
        "SEK", 
        "NOK", 
        "DKK", 
        "TRY", 
        "BRL", 
        "INR", 
        "MXN", 
        "KRW" 
    };

    public static bool IsSupported(string currencyCode) => AllowedCurrencies.Contains(currencyCode);
}
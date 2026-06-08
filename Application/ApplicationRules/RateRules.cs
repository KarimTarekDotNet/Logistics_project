namespace Application.ApplicationRules
{
    public static class RateRules
    {
        public static readonly string[] AllowedCurrencies = [ "EGP" ];

        public static bool IsValidCurrency(string currency)
            => AllowedCurrencies.Contains(currency);

        public static bool IsValidDateRange(DateTimeOffset validFrom, DateTimeOffset validTo)
            => validTo > validFrom;

        public static bool IsPositivePrice(decimal price)
            => price > 0;

        // A rate can only be activated if its validity period hasn't expired
        public static bool CanActivateRate(DateTimeOffset validTo)
            => validTo > DateTimeOffset.UtcNow;

        // A new rate is active by default only if its validity period is current
        public static bool ShouldBeActive(DateTimeOffset validFrom, DateTimeOffset validTo)
        {
            var now = DateTimeOffset.UtcNow;
            return validFrom <= now && validTo > now;
        }
    }
}

namespace Infrastructure.Helper
{
    public static class PaymentHelper
    {
        public static int ConvertToCentsFromUSDToEGY(decimal amount)
        {
            var exchangeRate = 51; // Example exchange rate from USD to EGY
            return (int)((amount * 100) * exchangeRate);
        }
    }
}

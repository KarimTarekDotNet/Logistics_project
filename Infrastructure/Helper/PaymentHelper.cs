namespace Infrastructure.Helper
{
    public static class PaymentHelper
    {
        public static int ConvertToCents(decimal amount)
        {
            return (int)(amount * 100);
        }
    }
}

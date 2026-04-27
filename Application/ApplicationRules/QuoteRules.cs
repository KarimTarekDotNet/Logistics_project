namespace Application.ApplicationRules
{
    public static class QuoteRules
    {
        public static bool IsFinalPriceConsistent(decimal finalPrice, IEnumerable<decimal> itemAmounts)
        {
            var total = itemAmounts.Sum();
            return total == 0 || finalPrice >= total;
        }

        public static bool HasItems(IEnumerable<object> items)
            => items.Any();
    }
}

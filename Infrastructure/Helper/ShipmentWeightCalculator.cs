namespace Infrastructure.Helper
{
    public static class ShipmentWeightCalculator
    {
        private const decimal CbmToKgFactor = 167m;

        public static decimal CalculateItemChargeableWeight(decimal grossWeightKg, decimal volumeCbm)
        {
            return Math.Max(grossWeightKg, volumeCbm * CbmToKgFactor);
        }

        public static decimal CalculateShipmentChargeableWeight(decimal totalGrossWeightKg, decimal totalVolumeCbm)
        {
            return Math.Max(totalGrossWeightKg, totalVolumeCbm * CbmToKgFactor);
        }
    }
}

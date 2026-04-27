namespace Application.ApplicationRules
{
    public static class PortRules
    {
        public static bool AreDistinct(Guid fromPortId, Guid toPortId)
            => fromPortId != toPortId;

        public static string FormatPortCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return code;

            code = code.Trim();

            return code.Length == 5
                ? code.Insert(2, " ")
                : code;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApplicationRules
{
    public static class CarrierRule
    {
        public static bool IsCodeMatch(string code) => !string.IsNullOrEmpty(code) &&
            System.Text.RegularExpressions.Regex.IsMatch(code, "^[A-Z]{4}$");
    }
}

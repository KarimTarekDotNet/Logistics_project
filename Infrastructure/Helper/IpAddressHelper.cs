namespace Infrastructure.Helper
{
    public static class IpAddressHelper
    {
        public static async Task<string> GetRealPublicIpAsync()
        {
            try
            {
                using var client = new HttpClient();
                // Fetch plain text IP address from a trusted provider
                string ip = await client.GetStringAsync("https://api.ipify.org");
                return ip.Trim();
            }
            catch (Exception ex)
            {
                return $"Error retrieving IP: {ex.Message}";
            }
        }
    }
}

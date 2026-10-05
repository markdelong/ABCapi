using Microsoft.Extensions.Configuration;

namespace ABC
{
    public static class ConnectionString
    {
        public static string Credentials { get; set; }
        public static string VSMCredentials { get; set; }
        public static IConfiguration Configuration { get; }
    }
}

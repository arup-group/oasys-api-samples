using Oasys.AdSec;

namespace ApiVersion
{
    public static class ApiVersion
    {
        public static void Main()
        {
            System.Console.WriteLine("This AdSec API version is " + IVersion.Api());
        }
    }
}
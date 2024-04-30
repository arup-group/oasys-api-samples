using Oasys.AdSec;

namespace ApiVersion
{
    // This is the simplest possible example.
    //
    // If you can compile and run it then you've installed the API successfully.
    public static class ApiVersion
    {
        public static void Main()
        {
            System.Console.WriteLine("The AdSec API version is " + IVersion.Api());
        }
    }
}

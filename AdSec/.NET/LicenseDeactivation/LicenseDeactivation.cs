using System;
using Oasys.AdSec;

namespace api_license_deactivate
{
    class Program
    {
        static void Main(string[] args)
        {
            var deactivation = ILicense.DeactivateLicense();

            if (deactivation.ActionStatus == true)
            {
                Console.WriteLine(
                    deactivation.ProductInformation.ProductName
                        + " v"
                        + deactivation.ProductInformation.Version
                        + " license deactivated successfully!"
                );
            }
            else
            {
                foreach (var warning in deactivation.Warnings)
                {
                    Console.WriteLine(warning.Description);
                }
            }
        }
    }
}

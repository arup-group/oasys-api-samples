using System;
using Oasys.AdSec;

namespace api_license_check
{
    class Program
    {
        static void Main(string[] args)
        {
            var activation = ILicense.ActivateLicense(
                ReadInput("Please enter your License ID: "),
                ReadInput("Please enter your Password: ")
            );
            if (activation.ActionStatus)
            {
                Console.WriteLine("\nLicense activated successfully!\n");
                Console.WriteLine("Product: " + activation.ProductInformation.ProductName);
                Console.WriteLine("Version: " + activation.ProductInformation.Version);
                Console.WriteLine("Company: " + activation.LicenseInformation.CompanyName);
                Console.WriteLine("License ID: " + activation.LicenseInformation.LicenseId);
                Console.WriteLine("Days left: " + activation.LicenseInformation.NumberOfDays);
                Console.WriteLine("Seats left: " + activation.LicenseInformation.NumberOfSeats);
                Console.WriteLine("Session ID: " + activation.LicenseInformation.SessionId);
                Console.WriteLine(
                    "Time Allocated Until Expiry: "
                        + activation.LicenseInformation.TimeAllocatedUntilExpiry
                );
            }
            else
            {
                foreach (var warning in activation.Warnings)
                {
                    Console.WriteLine(warning.Description);
                }
            }
        }

        static string ReadInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }
    }
}

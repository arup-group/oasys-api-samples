from Oasys.AdSec import ILicense

if __name__ == "__main__":
    activation = ILicense.ActivateLicense(
        input("Please enter your License ID: "), input("Please enter your Password: ")
    )

    if activation.ActionStatus == 1:
        print("\n" + "License activated successfully!" + "\n")
        print("Product:", activation.ProductInformation.ProductName)
        print("Version:", activation.ProductInformation.Version)
        print("Company:", activation.LicenseInformation.CompanyName)
        print("License ID:", activation.LicenseInformation.LicenseId)
        print("Days left:", activation.LicenseInformation.NumberOfDays)
        print("Seats left:", activation.LicenseInformation.NumberOfSeats)
        print("Session ID:", activation.LicenseInformation.SessionId)
        print("Time Allocated Until Expiry:", activation.LicenseInformation.TimeAllocatedUntilExpiry)

    else:
        for warning in activation.Warnings:
            print(warning.Description)

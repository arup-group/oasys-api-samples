import oasys.adsec
from Oasys.AdSec import ILicense

if __name__ == "__main__":

    res = ILicense.ActivateLicense("license_number","password")
    if res.ActionStatus == 1:
        print("License activated successfully!")
    else:
        print("Failed to activate license.")
                       
    print("Product Name:", res.ProductInformation.ProductName)
    print("Version:", res.ProductInformation.Version)
    
    print("Company Name:", res.LicenseInformation.CompanyName)
    print("License Id:", res.LicenseInformation.LicenseId)
    print("Number of Days left:", res.LicenseInformation.NumberOfDays)
    print("Number of Seats remaining:", res.LicenseInformation.NumberOfSeats)
    print("Session Id:", res.LicenseInformation.SessionId)
    print("Time Allocated Until Expiry:", res.LicenseInformation.TimeAllocatedUntilExpiry)
    

    for warning in res.Warnings:
        print(warning.Description)
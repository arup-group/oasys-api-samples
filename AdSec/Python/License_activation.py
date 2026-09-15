import oasys.adsec
from Oasys.AdSec import ILicense

if __name__ == "__main__":

    activation = ILicense.ActivateLicense(input("Please enter your license number: "), input("Please enter your password: "))
    
    
    if activation.ActionStatus == 1:
        print("\n" + "License activated successfully!" + "\n")
        
        print("Product Name:", activation.ProductInformation.ProductName)
        print("Version:", activation.ProductInformation.Version)
        
        print("Company Name:", activation.LicenseInformation.CompanyName)
        print("License Id:", activation.LicenseInformation.LicenseId)
        print("Number of Days left:", activation.LicenseInformation.NumberOfDays)
        print("Number of Seats remaining:", activation.LicenseInformation.NumberOfSeats)
        print("Session Id:", activation.LicenseInformation.SessionId)
        print("Time Allocated Until Expiry:", activation.LicenseInformation.TimeAllocatedUntilExpiry)
    
    else:
        for warning in activation.Warnings:
            print(warning.Description)
                       
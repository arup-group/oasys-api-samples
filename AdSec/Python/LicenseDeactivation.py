import oasys.adsec
from Oasys.AdSec import ILicense

if __name__ == "__main__":
    deactivation = ILicense.DeactivateLicense()

    if deactivation.ActionStatus == 1:
        print(
            f"{deactivation.ProductInformation.ProductName} v{deactivation.ProductInformation.Version} license deactivated successfully!"
        )

    else:
        for warning in deactivation.Warnings:
            print(warning.Description)

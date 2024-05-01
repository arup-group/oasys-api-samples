# Load the AdSec API

# Import modules
from Oasys.AdSec import IVersion

# This is the simplest possible example.
#
# If you can run this then you've installed the API successfully.
if __name__ == "__main__":
    print("This AdSec API version is " + IVersion.Api())

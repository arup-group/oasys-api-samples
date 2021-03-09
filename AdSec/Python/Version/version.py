# Load the .NET Core API
# See https://github.com/arup-group/oasys-api-samples/blob/main/AdSec/Python/api.py
from api import load_api
# Change this path to point to your AdSec API directory. e.g. load_api('C:\\Temp\\AdSecApi', 'AdSec_API')
load_api('C:\\change_this_path', 'AdSec_API')

from Oasys.AdSec import IVersion

if __name__ == '__main__':
    print('This AdSec API version is ' + IVersion.Api())

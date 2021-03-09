# Setup

## Python
These examples require [Python 3.7](https://www.python.org/downloads/) or later.
They are not intended for use with [IronPython](https://ironpython.net/). 

## Installing Python Packages
`requirements.txt` contains the Python packages for the examples.

You can install them with:
```commandline
python -m pip install -r requirements.txt
```

## Loading the API
Each script starts by loading the API. Change the call to 
`load_api()` to give it the path to your API installation.
e.g.
```python
# Load the .NET Core API
from api import load_api
load_api('C:\Temp\MyApiDirectory', 'AdSec_API')
```

 
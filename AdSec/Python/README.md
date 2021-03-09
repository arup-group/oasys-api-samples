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

## Python and Casting
The API interface is strongly typed. When you call API methods from
Python you must be sure to pass the right type of object. For example,
you cannot pass an `int` when a `double` is required.

In C# and other .NET languages you can use 'casts' to convert from
one type to another. In some cases, items are cast automatically.

This is particularly noticeable with [UnitsNet](https://github.com/angularsen/UnitsNet)
as it uses a lot of automatic casts. Consider this example in C#.
```c#
    width = UnitsNet.Length.FromMeters(3);
```
This doesn't work in Python because the argument to `FromMeters` is actually
an instance of `UnitsNet.QuantityValue`.

### PythonNetHelpers
We've provided `PythonNetHelpers.dll` to solve this problem. The
[TypeHelpers.Cast](https://github.com/arup-group/oasys-api-generator/blob/main/PythonNetHelpers/TypeHelpers.cs)
method will attempt to cast a value to a target type. It will search for and use
implicit converters if there are any. This allows `FromMeters` to be called like
this:
```python
from PythonNetHelpers import TypeHelpers
...
    width = Length.FromMeters(TypeHelpers.Cast[QuantityValue](2))
```

You can avoid the cast altogether by calling UnitsNet constructors directly. e.g.
```python
    area = UnitsNet.Area(15.0, UnitsNet.Units.AreaUnit.SquareMeter)
```
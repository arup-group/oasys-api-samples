An excel spreadsheet with VBA example of GSA COM functions.

This example demonstrates the use of the following functions
- Open GSA files using `Open(...)`
- Analyse a GSA model using `Analyze(...)`
- Delete results and analysis case using `Delete(...)`
- Print a saved view using `PrintView(...)`
- Save view to assigned format using `SaveViewToFile(...)`
- Extract data using `GwaCommand(...)`
- Renumber refrences to entities using `Renumber_Init(...)`
- Initialize output function and should be called before calling any other "Output_" functions ugin `Output_Init(...)`
- Extract output parameter of element at various intermediate locations ugin `Output_Extract(...)`
- Get the position along a 1D element as a proportion of the element length for specified position number using `Output_1DElemPos(...)`
- Close GSA COM instance using `Close(...)`

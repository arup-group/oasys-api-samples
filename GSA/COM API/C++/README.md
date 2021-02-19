Contains a minimal C++ COM sample that demonstrates initialization of the COM object and model analysis.

## ComClient_OutPut_Extract
This example uses the following functions to extract element forces at intermediate point:
- Import gsa type library file (```#import "Gsa.tlb"```)
- Create GSA COM Object (`IComAutoPtr pObj(__uuidof(ComAuto))`)
- Initialize output function and should be called before calling any other "Output_" functions (`Output_Init(...)`)
- Extract output parameter of element at various intermediate locations (`Output_Extract(...)`)

## ComClient_OutPut_Extract_Arr
This example uses the following functions to extract element forces at intermediate points in array:
- Import gsa type library file (```#import "Gsa.tlb"```)
- Create GSA COM Object (`IComAutoPtr pObj(__uuidof(ComAuto))`)
- Initialise the Output Array API for a specified case, axis, header and flags (`Output_Init_Arr(...)`)
- Extract output parameter results in array (`Output_Extract_Arr(...)`)

import sys
from pathlib import Path
from clr_loader import get_coreclr
from pythonnet import set_runtime


# load .NET runtime
def load_api(path_to_api_dlls: str, api_dll_name: str):
    api_path = Path(path_to_api_dlls)
    if not api_path.exists():
        raise ValueError(f"API directory does not exist. '{path_to_api_dlls}'")
    sys.path.append(str(api_path))
    rt = get_coreclr(runtime_config=str(api_path/f'{api_dll_name}.runtimeconfig.json'))
    set_runtime(rt)
    import clr  # Can't be imported until we've loaded the .NET Core runtime

    # add api references
    load_assembly(clr, api_path, api_dll_name)
    load_assembly(clr, api_path, 'UnitsNet')
    load_assembly(clr, api_path, 'PythonNetHelpers')


def load_assembly(clr, path: Path, assembly: str):
    dll = path / (assembly + '.dll')
    if not dll.exists():
        raise ValueError("Could not find " + assembly + " assembly at '" + str(dll) + "'")
    clr.AddReference(assembly)

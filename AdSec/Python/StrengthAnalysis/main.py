# load .NET runtime
import sys
from pathlib import Path
from clr_loader import get_coreclr
from pythonnet import set_runtime
api_path = Path(<path-to-adsec-api-dll>)
sys.path.append(str(api_path))
rt = get_coreclr(runtime_config=str(api_path/'AdSec_API.runtimeconfig.json'))
set_runtime(rt)

# add api references
import clr
clr.AddReference('AdSec_API')
clr.AddReference('UnitsNet')
clr.AddReference('PythonNetHelpers')

# import modules form namespace
from Oasys.AdSec import IAdSec, ISection, ILoad
from Oasys.AdSec.DesignCode import EN1992
from Oasys.AdSec.Materials import Concrete, Reinforcement
from Oasys.AdSec.Profile import ICircle, IPoint
from Oasys.AdSec.Reinforcement import *
import UnitsNet
from UnitsNet import QuantityValue
from PythonNetHelpers import TypeHelpers


def strength_analysis():
    # Create a circular section
    profile = ICircle.Create(0.5)
    sectionMaterial = Concrete.EN1992.Part1_1.Edition_2004.C40_50
    section = ISection.Create(profile, sectionMaterial)

    # Set the cover
    section.Reinforcement.Cover = ICover.Create(0.05)

    # Set some reinforcement
    reinforcementMaterial = Reinforcement.Steel.EN1992.Part1_1.Edition_2004.S500B
    layer = ILayerByBarCount.Create(4, reinforcementMaterial, 0.032)
    group = ILine.Create(IPoint.Create(0, 0), IPoint.Create(0.15, 0.15), layer)
    group.Layers.Add(layer)
    section.Reinforcement.Groups.Add(group)

    # Analyse the section to create a solution
    adSec = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014)
    solution = adSec.Analyse(section)

    # Calculate utilisation for a particular load
    axialForce = UnitsNet.Force.FromKilonewtons(TypeHelpers.Cast[QuantityValue](-100))
    majorAxisBending = 4e4
    minorAxisBending = 0
    load = ILoad.Create(axialForce, majorAxisBending, minorAxisBending)
    result = solution.Strength.Check(load)

    # Display utilisation as a percentage
    utilisation = round(result.LoadUtilisation * 100, 1)
    print("The utilisation is: " + str(utilisation))


# Press the green button in the gutter to run the script.
if __name__ == '__main__':
    strength_analysis()

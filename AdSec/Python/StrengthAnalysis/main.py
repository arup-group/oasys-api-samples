# Load the .NET Core API
from api import load_api
# Change this path to point to your AdSec API directory. e.g. load_api('C:\\Temp\\AdSecApi', 'AdSec_API')
load_api('C:\\change_this_path', 'AdSec_API')

# Import modules from namespace
from Oasys.AdSec import IAdSec, ISection, ILoad
from Oasys.AdSec.DesignCode import EN1992
from Oasys.AdSec.Materials import Concrete, Reinforcement
from Oasys.AdSec.Profile import ICircle, IPoint
from Oasys.AdSec.Reinforcement import ICover, ILayerByBarCount, ILine
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


if __name__ == '__main__':
    strength_analysis()

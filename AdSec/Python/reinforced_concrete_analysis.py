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
from UnitsNet import Force, Length, Moment, QuantityValue
from PythonNetHelpers import TypeHelpers


def quantity_value(value) -> QuantityValue:
    return TypeHelpers.Cast[QuantityValue](value)


def mm(distance) -> Length:
    return Length.FromMillimeters(quantity_value(distance))


def kN(force) -> Force:
    return Force.FromKilonewtons(quantity_value(force))


def kNm(force) -> Moment:
    return Moment.FromKilonewtonMeters(quantity_value(force))


def strength_analysis():
    # Create a circular section
    profile = ICircle.Create(mm(500))
    sectionMaterial = Concrete.EN1992.Part1_1.Edition_2004.C40_50
    section = ISection.Create(profile, sectionMaterial)

    # Set the cover
    section.Reinforcement.Cover = ICover.Create(mm(50))

    # Set some reinforcement
    reinforcementMaterial = Reinforcement.Steel.EN1992.Part1_1.Edition_2004.S500B
    layer = ILayerByBarCount.Create(4, reinforcementMaterial, mm(32))
    group = ILine.Create(IPoint.Create(Length.Zero, Length.Zero), IPoint.Create(mm(150), mm(150)), layer)
    group.Layers.Add(layer)
    section.Reinforcement.Groups.Add(group)

    # Analyse the section to create a solution
    adSec = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014)
    solution = adSec.Analyse(section)

    # Calculate utilisation for a particular load
    axialForce = kN(-100)
    majorAxisBending = kNm(60)
    minorAxisBending = Moment.Zero
    load = ILoad.Create(axialForce, majorAxisBending, minorAxisBending)
    strengthResult = solution.Strength.Check(load)

    # Display utilisation as a percentage
    utilisation = round(strengthResult.LoadUtilisation.Percent, 1)
    print("The utilisation is: " + str(utilisation) + "%")

    # Calculate the serviceability crack width under the same load
    serviceabilityResult = solution.Serviceability.Check(load)

    # Display the crack width in mm
    crackWidth = round(serviceabilityResult.MaximumWidthCrack.Width.Millimeters, 2)
    print("The maximum crack width is: " + str(crackWidth) + " mm")


if __name__ == '__main__':
    strength_analysis()

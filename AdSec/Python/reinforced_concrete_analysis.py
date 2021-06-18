# Load the AdSec API
import oasys.adsec

# Import modules from namespace
from Oasys.AdSec import IAdSec, ISection, ILoad
from Oasys.AdSec.DesignCode import EN1992
from Oasys.AdSec.StandardMaterials import Concrete, Reinforcement
from Oasys.Profiles import ICircleProfile, IPoint
from Oasys.AdSec.Reinforcement import IBarBundle
from Oasys.AdSec.Reinforcement.Groups import ILineGroup
from Oasys.AdSec.Reinforcement.Layers import ILayerByBarCount
from Oasys.Units import Moment, MomentUnit
from UnitsNet import Force, Length
from UnitsNet.Units import ForceUnit, LengthUnit


def strength_analysis():
    # Create a circular section
    diameter = Length(float(500), LengthUnit.Millimeter)
    profile = ICircleProfile.Create(diameter)
    sectionMaterial = Concrete.EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014.C40_50
    section = ISection.Create(profile, sectionMaterial)

    # Set some reinforcement
    reinforcementMaterial = Reinforcement.Steel.EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014.S500B
    barDiameter = Length(float(32), LengthUnit.Millimeter)
    barBundle = IBarBundle.Create(reinforcementMaterial, barDiameter)
    layer = ILayerByBarCount.Create(4, barBundle)
    position = Length(float(150), LengthUnit.Millimeter)
    group = ILineGroup.Create(IPoint.Create(Length.Zero, Length.Zero), IPoint.Create(position, position), layer)
    section.ReinforcementGroups.Add(group)

    # Analyse the section to create a solution
    adSec = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014)
    solution = adSec.Analyse(section)

    # Calculate utilisation for a particular load
    axialForce = Force(float(-100), ForceUnit.Kilonewton)
    majorAxisBending = Moment(float(60), MomentUnit.KilonewtonMeter)
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

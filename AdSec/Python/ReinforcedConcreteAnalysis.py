# Load the AdSec API
import oasys.adsec

# Import modules from namespace
from Oasys.AdSec import IAdSec, ISection, ILoad
from Oasys.AdSec.DesignCode import EN1992
from Oasys.AdSec.StandardMaterials import Concrete, Reinforcement
from Oasys.Profiles import IRectangleProfile
from Oasys.AdSec.Reinforcement import ICover, IBarBundle
from Oasys.AdSec.Reinforcement.Groups import ITemplateGroup, ILinkGroup
from Oasys.AdSec.Reinforcement.Layers import ILayerByBarCount
from Oasys.Units import Moment, MomentUnit
from UnitsNet import Force, Length
from UnitsNet.Units import ForceUnit, LengthUnit


# This example shows how to define a reinforced section and analyse it.
# The example goes on to apply a load and check the utilisation and
# maximum crack width.
#
# You might like to run the 'version.py' example first, just to check
# that the API is installed correctly.
def strength_analysis():
    # Create a rectangular section
    depth = Length(float(800), LengthUnit.Millimeter)
    width = Length(float(400), LengthUnit.Millimeter)
    profile = IRectangleProfile.Create(depth, width)
    sectionMaterial = Concrete.EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014.C40_50
    section = ISection.Create(profile, sectionMaterial)

    # Set the cover
    section.Cover = ICover.Create(Length(float(40), LengthUnit.Millimeter))

    # Set some reinforcement
    reinforcementMaterial = Reinforcement.Steel.EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014.S500B
    bar20mm = IBarBundle.Create(reinforcementMaterial, Length(float(20), LengthUnit.Millimeter))
    bar16mm = IBarBundle.Create(reinforcementMaterial, Length(float(16), LengthUnit.Millimeter))
    bar12mm = IBarBundle.Create(reinforcementMaterial, Length(float(12), LengthUnit.Millimeter))

    # Define top reinforcement
    layerForTopGroup = ILayerByBarCount.Create(4, bar16mm)
    topGroup = ITemplateGroup.Create(ITemplateGroup.Face.Top)
    topGroup.Layers.Add(layerForTopGroup)

    # Define bottom reinforcement
    layerOneForBottomGroup = ILayerByBarCount.Create(4, bar20mm)
    layerTwoForBottomGroup = ILayerByBarCount.Create(4, bar16mm)
    bottomGroup = ITemplateGroup.Create(ITemplateGroup.Face.Bottom)
    bottomGroup.Layers.Add(layerOneForBottomGroup)
    bottomGroup.Layers.Add(layerTwoForBottomGroup)

    # Define link (stirrup)
    link = ILinkGroup.Create(bar12mm)

    # Add defined reinforcement groups to section
    section.ReinforcementGroups.Add(topGroup)
    section.ReinforcementGroups.Add(bottomGroup)
    section.ReinforcementGroups.Add(link)

    # Analyse the section to create a solution
    adSec = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014)
    solution = adSec.Analyse(section)

    # Calculate utilisation for a particular load
    axialForce = Force(float(-100), ForceUnit.Kilonewton)
    majorAxisBending = Moment(float(-500), MomentUnit.KilonewtonMeter)
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

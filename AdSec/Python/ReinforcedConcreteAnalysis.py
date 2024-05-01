# Load the AdSec API

# Import modules from namespace
from Oasys.AdSec import IAdSec, ILoad, ISection
from Oasys.AdSec.DesignCode import EN1992
from Oasys.AdSec.Reinforcement import IBarBundle, ICover
from Oasys.AdSec.Reinforcement.Groups import ILinkGroup, ITemplateGroup
from Oasys.AdSec.Reinforcement.Layers import ILayerByBarCount
from Oasys.AdSec.StandardMaterials import Concrete, Reinforcement
from Oasys.Profiles import IRectangleProfile
from OasysUnits import Force, Length, Moment
from OasysUnits.Units import ForceUnit, LengthUnit, MomentUnit


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
    section_material = Concrete.EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014.C40_50
    section = ISection.Create(profile, section_material)

    # Set the cover
    section.Cover = ICover.Create(Length(float(40), LengthUnit.Millimeter))

    # Set some reinforcement
    reinforcement_material = Reinforcement.Steel.EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014.S500B
    bar20mm = IBarBundle.Create(reinforcement_material, Length(float(20), LengthUnit.Millimeter))
    bar16mm = IBarBundle.Create(reinforcement_material, Length(float(16), LengthUnit.Millimeter))
    bar12mm = IBarBundle.Create(reinforcement_material, Length(float(12), LengthUnit.Millimeter))

    # Define top reinforcement
    layer_for_top_group = ILayerByBarCount.Create(4, bar16mm)
    top_group = ITemplateGroup.Create(ITemplateGroup.Face.Top)
    top_group.Layers.Add(layer_for_top_group)

    # Define bottom reinforcement
    layer_one_for_bottom_group = ILayerByBarCount.Create(4, bar20mm)
    layer_two_for_bottom_group = ILayerByBarCount.Create(4, bar16mm)
    bottom_group = ITemplateGroup.Create(ITemplateGroup.Face.Bottom)
    bottom_group.Layers.Add(layer_one_for_bottom_group)
    bottom_group.Layers.Add(layer_two_for_bottom_group)

    # Define link (stirrup)
    link = ILinkGroup.Create(bar12mm)

    # Add defined reinforcement groups to section
    section.ReinforcementGroups.Add(top_group)
    section.ReinforcementGroups.Add(bottom_group)
    section.ReinforcementGroups.Add(link)

    # Analyse the section to create a solution
    ad_sec = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014)
    solution = ad_sec.Analyse(section)

    # Calculate utilisation for a particular load
    axial_force = Force(float(-100), ForceUnit.Kilonewton)
    major_axis_bending = Moment(float(-500), MomentUnit.KilonewtonMeter)
    minor_axis_bending = Moment.Zero
    load = ILoad.Create(axial_force, major_axis_bending, minor_axis_bending)
    strength_result = solution.Strength.Check(load)

    # Display utilisation as a percentage
    utilisation = round(strength_result.LoadUtilisation.Percent, 1)
    print("The utilisation is: " + str(utilisation) + "%")

    # Calculate the serviceability crack width under the same load
    serviceability_result = solution.Serviceability.Check(load)

    # Display the crack width in mm
    crack_width = round(serviceability_result.MaximumWidthCrack.Width.Millimeters, 2)
    print("The maximum crack width is: " + str(crack_width) + " mm")


if __name__ == "__main__":
    strength_analysis()

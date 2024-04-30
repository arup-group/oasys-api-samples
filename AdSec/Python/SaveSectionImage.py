# Load the AdSec API
from pathlib import Path

# Import modules from namespace
from Oasys.AdSec import IAdSec, ISection, ISubComponent
from Oasys.AdSec.DesignCode import IS456
from Oasys.AdSec.IO.Graphics.Section import SectionImageBuilder
from Oasys.AdSec.Reinforcement import IBarBundle, ICover
from Oasys.AdSec.Reinforcement.Groups import ILinkGroup, IPerimeterGroup
from Oasys.AdSec.Reinforcement.Layers import ILayerByBarPitch
from Oasys.AdSec.StandardMaterials import Concrete, Reinforcement
from Oasys.Profiles import IPoint, IRectangleProfile
from OasysUnits import Length
from OasysUnits.Units import LengthUnit
from reportlab.graphics import renderPM
from svglib.svglib import svg2rlg

# This example shows how to save the XML to a file to create an SVG file.
# It also shows how you can use a third party library to convert the SVG into a PNG file.
#
# You might like to run the 'ApiVersion' example first, just to check
# that the API is installed correctly.


def save_section_image():
    # We're going to create a rectangular section with a sub-component.

    # Create the rectangular section
    depth = Length(float(500), LengthUnit.Millimeter)
    width = Length(float(300), LengthUnit.Millimeter)
    profile = IRectangleProfile.Create(depth, width)
    section_material = Concrete.IS456.Edition_2000.M30
    section = ISection.Create(profile, section_material)

    # Set cover
    section.Cover = ICover.Create(Length(float(30), LengthUnit.Millimeter))

    # Assign reinforcements to the main section
    reinforcement_material = Reinforcement.Steel.IS456.Edition_2000.S415
    bar16mm = IBarBundle.Create(reinforcement_material, Length(float(16), LengthUnit.Millimeter))
    layer_pitch_125mm = ILayerByBarPitch.Create(bar16mm, Length(float(125), LengthUnit.Millimeter))
    main_reinforcement = IPerimeterGroup.Create()
    main_reinforcement.Layers.Add(layer_pitch_125mm)
    link10mm = IBarBundle.Create(reinforcement_material, Length(float(10), LengthUnit.Millimeter))
    link = ILinkGroup.Create(link10mm)
    section.ReinforcementGroups.Add(main_reinforcement)
    section.ReinforcementGroups.Add(link)

    # Create another rectangular section which we'll use as a sub-component.
    subcomponent_depth = Length(float(250), LengthUnit.Millimeter)
    subcomponent_width = Length(float(700), LengthUnit.Millimeter)
    subcomponent_profile = IRectangleProfile.Create(subcomponent_depth, subcomponent_width)
    subcomponent_section = ISection.Create(subcomponent_profile, section_material)
    subcomponent_section.Cover = ICover.Create(Length(float(30), LengthUnit.Millimeter))

    # Assign reinforcements to sub-component section
    layer_pitch_120mm = ILayerByBarPitch.Create(bar16mm, Length(float(120), LengthUnit.Millimeter))
    subcomponent_reinforcement = IPerimeterGroup.Create()
    subcomponent_reinforcement.Layers.Add(layer_pitch_120mm)
    subcomponent_link = ILinkGroup.Create(link10mm)
    subcomponent_section.ReinforcementGroups.Add(subcomponent_reinforcement)
    subcomponent_section.ReinforcementGroups.Add(subcomponent_link)

    # Add this sub-component to the main section
    y = Length(float(-200), LengthUnit.Millimeter)
    z = Length(float(125), LengthUnit.Millimeter)
    subcomponent_offset = IPoint.Create(y, z)
    subcomponent = ISubComponent.Create(subcomponent_section, subcomponent_offset)
    section.SubComponents.Add(subcomponent)

    # Flatten the section
    app = IAdSec.Create(IS456.Edition_2000)
    flattened_section = app.Flatten(section)

    # Get SVG XML body as a string
    svg_str = SectionImageBuilder(flattened_section).Svg()

    # This string can be written into SVG format file
    with Path("examplepy.svg").open(mode="w") as file:
        file.write(svg_str)

    # This string can be converted into PNG format file with a third party library like https://pypi.org/project/svglib
    drawing = svg2rlg("examplepy.svg")
    renderPM.drawToFile(drawing, "examplepy.png", fmt="PNG")


if __name__ == "__main__":
    save_section_image()

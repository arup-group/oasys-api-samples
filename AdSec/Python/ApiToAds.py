import oasys.adsec
from Oasys.AdSec import ISection, ILoad
from Oasys.AdSec.DesignCode import EN1992
from Oasys.Collections import IList
from Oasys.AdSec.StandardMaterials import Concrete
from Oasys.Profiles import ICircleProfile
from Oasys.AdSec.IO.Serialization import JsonConverter
from OasysUnits import Length, Force, Moment
from OasysUnits.Units import LengthUnit, ForceUnit, MomentUnit

# This example shows how to convert an API object into .ads file
#
# You might like to run the 'version.py' example first, just to check
# that the API is installed correctly.


def api_to_ads():
    # Create API section
    circle_profile = ICircleProfile.Create(Length(float(1000), LengthUnit.Millimeter))
    C30 = Concrete.EN1992.Part1_1.Edition_2004.NationalAnnex.GB.PD6687.Edition_2010.C30_37
    section = ISection.Create(circle_profile, C30)

    # Use JsonConverter's SectionToJson method to obtain JSON string
    converter = JsonConverter(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.PD6687.Edition_2010)
    json = converter.SectionToJson(section)

    # Save this JSON string into a file
    with open("adsec_section.ads", mode="w", encoding="utf-8") as file:
        file.write(json)

    # To save a section with load (or deformation)
    load_one = ILoad.Create(Force(float(-10), ForceUnit.Kilonewton), Moment.Zero, Moment.Zero)
    load_two = ILoad.Create(Force(float(-15), ForceUnit.Kilonewton), Moment.Zero, Moment.Zero)
    load_list = IList[ILoad].Create()
    load_list.Add(load_one)
    load_list.Add(load_two)
    json_with_loads = converter.SectionToJson(section, load_list)
    with open("adsec_section_with_loads.ads", mode="w", encoding="utf-8") as file:
        file.write(json_with_loads)


if __name__ == "__main__":
    api_to_ads()

    # Note: This feature works for Standard Materials
    # Note: Tasks are ignored

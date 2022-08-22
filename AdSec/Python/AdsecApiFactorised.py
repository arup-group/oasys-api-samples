# documentation:
# https://arup-group.github.io/oasys-combined/adsec-api/index.html

# Load the AdSec API
import oasys.adsec

# Import modules
from Oasys.AdSec import ISection, IAdSec, ILoad, IVersion
from Oasys.AdSec.DesignCode import CSA, IS456, EN1992
from Oasys.AdSec.IO.Graphics.Section import SectionImageBuilder
from Oasys.AdSec.Reinforcement import IBarBundle, ICover
from Oasys.AdSec.Reinforcement.Groups import ITemplateGroup, ILinkGroup, \
    ISingleBars, IPerimeterGroup
from Oasys.AdSec.Reinforcement.Layers import ILayerByBarCount, ILayerByBarPitch
from Oasys.AdSec.StandardMaterials import Concrete, Reinforcement
from Oasys.Profiles import IRectangleProfile, ICircleProfile, \
    IPerimeterProfile, IPoint
from Oasys.Units import Moment, MomentUnit
from UnitsNet import Length, Force
from UnitsNet.Units import LengthUnit, ForceUnit
from reportlab.graphics import renderPM
from svglib.svglib import svg2rlg

# initialise default design code settings; override with code functions
design_code = EN1992.Part1_1.Edition_2004.NationalAnnex.NoNationalAnnex
material = Concrete.EN1992.Part1_1.Edition_2004. \
    NationalAnnex.NoNationalAnnex.C32_40
bar_material = Reinforcement.Steel.EN1992.Part1_1.Edition_2004. \
    NationalAnnex.NoNationalAnnex.S500B

# initialised global variables
section = ISection
profile = 0
uls_deformation = 0
sls_deformation = 0


def code_ec2_gb():
    global material
    global bar_material
    global design_code
    design_code = EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014
    material = Concrete.EN1992.Part1_1.Edition_2004. \
        NationalAnnex.GB.Edition_2014.C32_40
    bar_material = Reinforcement.Steel.EN1992.Part1_1.Edition_2004. \
        NationalAnnex.GB.Edition_2014.S500B


def code_is456():
    global material
    global bar_material
    global design_code
    design_code = IS456.Edition_2000
    material = Concrete.IS456.Edition_2000.M25
    bar_material = Reinforcement.Steel.IS456.Edition_2000.S500


def code_csa_s6():
    global material
    global bar_material
    global design_code
    design_code = CSA.S6.Edition_2014
    material = Concrete.CSA.S6.Edition_2014.C35
    bar_material = Reinforcement.Steel.CSA.S6.Edition_2014.G30_18_400R


def new_rectangle_section(depth, width, cover=30):
    global profile
    global material
    global section
    section_depth = Length(float(depth), LengthUnit.Millimeter)
    section_width = Length(float(width), LengthUnit.Millimeter)
    profile = IRectangleProfile.Create(section_depth, section_width)
    section = ISection.Create(profile, material)
    section.Cover = ICover.Create(Length(float(cover), LengthUnit.Millimeter))


def new_circle_section(diameter, cover=30):
    global profile
    global material
    global section
    section_diameter = Length(float(diameter), LengthUnit.Millimeter)
    profile = ICircleProfile.Create(section_diameter)
    section = ISection.Create(profile, material)
    section.Cover = ICover.Create(Length(float(cover), LengthUnit.Millimeter))


def add_links(diameter):
    global bar_material
    bar_dia = Length(float(diameter), LengthUnit.Millimeter)
    rebar = ILinkGroup.Create(IBarBundle.Create(bar_material, bar_dia))
    section.ReinforcementGroups.Add(rebar)


def add_perimeter_bars(diameter, number):
    global bar_material
    bar_dia = Length(float(diameter), LengthUnit.Millimeter)
    rebar = IPerimeterGroup.Create()
    bar_bundle = IBarBundle.Create(bar_material, bar_dia)
    layer = ILayerByBarCount.Create(number, bar_bundle)
    rebar.Layers.Add(layer)
    section.ReinforcementGroups.Add(rebar)


def add_top_bars(diameter, number):
    global bar_material
    bar_dia = Length(float(diameter), LengthUnit.Millimeter)
    rebar = ITemplateGroup.Create(ITemplateGroup.Face.Top)
    bar_bundle = IBarBundle.Create(bar_material, bar_dia)
    layer = ILayerByBarCount.Create(number, bar_bundle)
    rebar.Layers.Add(layer)
    section.ReinforcementGroups.Add(rebar)


def add_bottom_bars(diameter, number):
    global bar_material
    bar_dia = Length(float(diameter), LengthUnit.Millimeter)
    rebar = ITemplateGroup.Create(ITemplateGroup.Face.Bottom)
    bar_bundle = IBarBundle.Create(bar_material, bar_dia)
    layer = ILayerByBarCount.Create(number, bar_bundle)
    rebar.Layers.Add(layer)
    section.ReinforcementGroups.Add(rebar)


def add_side_bars(diameter, spacing):
    global bar_material
    bar_dia = Length(float(diameter), LengthUnit.Millimeter)
    bar_spacing = Length(float(spacing), LengthUnit.Millimeter)
    rebar = ITemplateGroup.Create(ITemplateGroup.Face.Sides)
    bar_bundle = IBarBundle.Create(bar_material, bar_dia)
    layer = ILayerByBarPitch.Create(bar_bundle, bar_spacing)
    rebar.Layers.Add(layer)
    section.ReinforcementGroups.Add(rebar)


def section_image():
    global design_code
    global section
    flat_section = IAdSec.Create(design_code).Flatten(section)
    with open("section.svg", "w") as file:
        file.write(SectionImageBuilder(flat_section).Svg())
    drawing = svg2rlg("section.svg")
    renderPM.drawToFile(drawing, "my_section.png", fmt="PNG")


def section_analyse(force=0, moment_yy=0, moment_zz=0):
    global section
    global design_code
    global uls_deformation
    global sls_deformation

    print("\nAnalyse section")
    solution = IAdSec.Create(design_code).Analyse(section)
    strength_results = solution.Strength
    serviceability_results = solution.Serviceability
    i_load = ILoad.Create(Force(float(force), ForceUnit.Kilonewton),
                          Moment(float(moment_yy), MomentUnit.KilonewtonMeter),
                          Moment(float(moment_zz), MomentUnit.KilonewtonMeter))
    uls_deformation = strength_results.Check(i_load).Deformation
    sls_deformation = serviceability_results.Check(i_load).Deformation
    print("\nULS deformation X " + str(uls_deformation.X.MilliStrain) +
          " YY: " + str(uls_deformation.YY.PerMillimeters) +
          " zz: " + str(uls_deformation.ZZ.PerMillimeters))
    uls_load_util = strength_results.Check(i_load).LoadUtilisation
    print("ULS utilisation: " + str(round(uls_load_util.Percent, 2)) + "%")


def section_stress_strain():
    global section
    global design_code
    global uls_deformation
    global sls_deformation
    print("\n\nStress - strain at key points of component")
    flat_section = IAdSec.Create(design_code).Flatten(section)
    corner_points = IPerimeterProfile(flat_section.Profile).SolidPolygon.Points
    for points in corner_points:
        point = IPoint(points)
        y = point.Y.Millimeters
        z = point.Z.Millimeters
        strain = uls_deformation.StrainAt(point)
        stress = flat_section.Material.Strength.StressAt(strain)
        print("\nPoint: (" + str(y) + "," + str(z) + ")")
        print("ULS Stress: " + str(round(stress.Megapascals, 2)) +
              " Strain: " + str(round(strain.MilliStrain, 2)))
        strain = sls_deformation.StrainAt(point)
        stress = flat_section.Material.Strength.StressAt(strain)
        print("SLS Stress: " + str(round(stress.Megapascals, 2)) +
              " Strain: " + str(round(strain.MilliStrain, 2)))


def rebar_stress_strain():
    global section
    global design_code
    global uls_deformation
    global sls_deformation
    print("\n\nULS Stress - strain in rebar")
    flat_section = IAdSec.Create(design_code).Flatten(section)
    for bar_group in range(1, len(section.ReinforcementGroups)):
        rebar_positions = ISingleBars(
            flat_section.ReinforcementGroups[bar_group]).Positions
        for rebar in rebar_positions:
            position = IPoint(rebar)
            y = rebar.Y.Millimeters
            z = rebar.Z.Millimeters
            strain = uls_deformation.StrainAt(position)
            stress = ISingleBars(flat_section.ReinforcementGroups[1]
                                 ).BarBundle.Material.Strength.StressAt(strain)
            print("Point: (" + str(round(y, 1)) + "," + str(round(z, 1)) +
                  ") :" +
                  " Stress: " + str(round(stress.Megapascals, 1)) +
                  " Strain: " + str(round(strain.MilliStrain, 2)))


if __name__ == "__main__":
    print("\nThis AdSec API version is " + IVersion.Api())
    print("____")
    
    code_ec2_gb()

    new_rectangle_section(700, 500, 30)
    add_links(12)
    add_top_bars(12, 4)
    add_bottom_bars(32, 4)
    add_side_bars(16, 150)
    section_analyse(0, -450, 0)
    section_stress_strain()

    print("____")

    new_circle_section(450, 75)
    add_links(12)
    add_perimeter_bars(20, 8)
    section_image()
    section_analyse(-250, 100, 0)
    rebar_stress_strain()

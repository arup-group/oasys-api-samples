# importing packages
import openpyxl as op  # OPen source library that can be used for reading and writing into the excel files
from Oasys.AdSec import IAdSec, ILoad, ISection
from Oasys.AdSec.DesignCode import EN1992
from Oasys.AdSec.StandardMaterials import Steel
from Oasys.Profiles import ICircleProfile
from OasysUnits import Force, Length, Moment
from OasysUnits.Units import ForceUnit, LengthUnit, MomentUnit

# Reading Values from the excel file

try:
    workbook = op.load_workbook(filename=r"C:\Repo\oasys-api-samples\AdSec\Python\loads.xlsx")
    sheet1 = workbook["Sheet1"]
    load = [row for row in sheet1.iter_rows(min_row=2, max_col=3, values_only=True)]
    # Defining the Section Material
    section_material = Steel.EN1993.Edition_2005.S235

    # Creating adsec object
    adsec = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014)

    # Creating a Circular Section
    diameter = Length(float(80), LengthUnit.Inch)
    circle_profile = ICircleProfile.Create(diameter)
    section = ISection.Create(circle_profile, section_material)

    # circular Section Analysis
    circle_section_analyse = adsec.Analyse(section)
    x = 2
    max_utilization = 100
    for row in load:
        Force_x = Force(float(row[0]), ForceUnit.Kilonewton)
        Moment_yy = Moment(float(row[1]), MomentUnit.KilonewtonMeter)
        Moment_zz = Moment(float(row[2]), MomentUnit.KilonewtonMeter)
        circle_load = ILoad.Create(Force_x, Moment_yy, Moment_zz)
        strength_result = circle_section_analyse.Strength.Check(circle_load)
        utilisation = round(strength_result.LoadUtilisation.Percent, 1)
        uls_status = "Clear :)" if utilisation < max_utilization else "Not Clear :("
        sheet1["D" + str(x)] = utilisation
        sheet1["E" + str(x)] = uls_status
        x += 1
    workbook.save("loads.xlsx")
except FileNotFoundError:
    print("file not found")
except Exception as e:
    print("Error occured: ", e)

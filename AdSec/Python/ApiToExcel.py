#importing packages
import oasys.adsec
from OasysUnits import Force, Length, Moment
from OasysUnits.Units import LengthUnit,ForceUnit,MomentUnit
from Oasys.Profiles import ICircleProfile
from Oasys.AdSec.StandardMaterials import Steel
from Oasys.AdSec import ISection
from Oasys.AdSec import IAdSec,ILoad
from Oasys.AdSec.DesignCode import EN1992
import openpyxl as op#OPen source library that can be used for reading and writing into the excel files

#Reading Values from the excel file

try:
    workbook=op.load_workbook(filename='C:\Repo\oasys-api-samples\AdSec\Python\loads.xlsx')
    sheet1=workbook['Sheet1']
    load=[row for row in sheet1.iter_rows(min_row=2,max_col=3, values_only=True)]
    #Defining the Section Material
    sectionMaterial = Steel.EN1993.Edition_2005.S235

    #Creating adsec object
    adSec = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014)

    #Creating a Circular Section
    diameter = Length(float(80), LengthUnit.Inch)
    CircleProfile = ICircleProfile.Create(diameter)
    section = ISection.Create(CircleProfile, sectionMaterial)

    #circular Section Analysis
    circle_section_Analyse = adSec.Analyse(section)
    x=2
    for row in load:
        Force_x = Force(float(row[0]), ForceUnit.Kilonewton)
        Moment_yy = Moment(float(row[1]), MomentUnit.KilonewtonMeter)
        Moment_zz = Moment(float(row[2]), MomentUnit.KilonewtonMeter)
        circle_load = ILoad.Create(Force_x, Moment_yy, Moment_zz)
        strengthResult = circle_section_Analyse.Strength.Check(circle_load)
        utilisation = round(strengthResult.LoadUtilisation.Percent, 1)
        ULS_Status= "Clear :)" if utilisation<100 else "Not Clear :("
        sheet1['D'+str(x)]=utilisation
        sheet1['E'+str(x)]=ULS_Status
        x+=1
    workbook.save('loads.xlsx')
except FileNotFoundError:
    print("file not found")
except Exception as e:
    print("Error occured: ",e);
    


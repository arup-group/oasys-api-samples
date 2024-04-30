//Importing the necessary packages
using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Oasys.AdSec;
using OasysUnits;
using OasysUnits.Units;
using Oasys.Profiles;
using Oasys.AdSec.StandardMaterials;
using Oasys.AdSec.DesignCode;

class Program
{
    static void Main()
    {
        try
        {
            //Getting the workbook from the excel file.
            var workbook = new XLWorkbook("Path to your file..");

            var worksheet = workbook.Worksheet("Sheet1");
            // Getting the load from the excel file
            var load = worksheet.Range("A2:C" + worksheet.LastRowUsed().RowNumber()).Rows();
            // creating adsec object and defining section material
            var sectionMaterial = Steel.EN1993.Edition_2005.S235;
            var adSec = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014);
            //Creating a secion 
            var diameter = new Length(80, LengthUnit.Inch);
            var circleProfile = ICircleProfile.Create(diameter);
            var section = ISection.Create(circleProfile, sectionMaterial);
            //Analysing the section
            var circleSectionAnalysis = adSec.Analyse(section);
            // Starting from 2 as the 1st row is the heading 
            int x = 2;
            for (int i = 2; i < load.Count(); i++)
            { 
                // Getting the loads and coverting it 
                var force_x = new Force((double)worksheet.Cell("A" + i).Value, ForceUnit.Kilonewton);
                var moment_yy = new Moment((double)worksheet.Cell("B" + i).Value, MomentUnit.KilonewtonMeter);
                var moment_zz = new Moment((double)worksheet.Cell("C" + i).Value, MomentUnit.KilonewtonMeter);
                var circleLoad = ILoad.Create(force_x, moment_yy, moment_zz);
                //Getting the strength results
                var strengthResult = circleSectionAnalysis.Strength.Check(circleLoad);
                var utilisation = Math.Round(strengthResult.LoadUtilisation.Percent, 1);
                var ulsStatus = utilisation < 100 ? "Clear" : "Not Clear";
                worksheet.Cell("D" + i).Value = utilisation; 
                worksheet.Cell("E" + i).Value = ulsStatus;  
            }
            workbook.Save();

        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("File not found");
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }
}

using ClosedXML.Excel; //Open source library That can be used for reading and writing in the excel file
using System;
using System.IO;
using System.Linq;
using Oasys.AdSec;
using OasysUnits;
using OasysUnits.Units;
using Oasys.Profiles;
using Oasys.AdSec.StandardMaterials;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;

class Program
{
    static void Main()
    {
        // Part1
        // Getting the workbook from the excel file.
        var excelfilePath = CreateExcelWithRandomLoads();

        // Part2
        // Open a workbook with worksheet with name "Sheet1"
        var workbook = new XLWorkbook(excelfilePath);
        var worksheet = workbook.Worksheets.Worksheet("Sheet1");

        // creating adsec object and defining section material
        var sectionMaterial = Steel.EN1993.Edition_2005.S235;
        var adSec = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014);

        var section = CreateSection(sectionMaterial);

        //Analysing the section
        var circleSectionAnalysis = adSec.Analyse(section);

        for (int i = 1; i <= 10; i++)
        {
            var circleLoad = CreateCircleLoad(worksheet, i);

            //Getting the strength results
            var strengthResult = circleSectionAnalysis.Strength.Check(circleLoad);
            var utilisation = Math.Round(strengthResult.LoadUtilisation.Percent, 1);
            var ulsStatus = utilisation < 100 ? "Clear" : "Not Clear";
            worksheet.Cell("D" + i).Value = utilisation;
            worksheet.Cell("E" + i).Value = ulsStatus;
        }
        // excel file will be created in the temp folder
        workbook.Save();
    }

    private static string CreateExcelWithRandomLoads()
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Sheet1");

        var random = new Random();

        // adding random values into cells
        for (int i = 1; i <= 10; i++)
        {
            worksheet.Cell(i, 1).Value = random.Next(-100, 101) * 1000;
            worksheet.Cell(i, 2).Value = random.Next(-100, 101) * 1000;
            worksheet.Cell(i, 3).Value = random.Next(-100, 101) * 1000;
        }

        var filePath = Path.Combine(Path.GetTempPath(), "Output.xlsx");
        workbook.SaveAs(filePath);

        return filePath;
    }

    private static ILoad CreateCircleLoad(IXLWorksheet worksheet, int i)
    {
        // Getting the loads and coverting it
        var force_x = new Force((double)worksheet.Cell("A" + i).Value, ForceUnit.Kilonewton);
        var moment_yy = new Moment(
            (double)worksheet.Cell("B" + i).Value,
            MomentUnit.KilonewtonMeter
        );
        var moment_zz = new Moment(
            (double)worksheet.Cell("C" + i).Value,
            MomentUnit.KilonewtonMeter
        );
        var circleLoad = ILoad.Create(force_x, moment_yy, moment_zz);
        return circleLoad;
    }

    private static ISection CreateSection(ISteel sectionMaterial)
    {
        var diameter = new Length(80, LengthUnit.Inch);
        var circleProfile = ICircleProfile.Create(diameter);
        var section = ISection.Create(circleProfile, sectionMaterial);
        return section;
    }
}

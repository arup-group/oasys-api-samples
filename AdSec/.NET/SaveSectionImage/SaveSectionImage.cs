using System;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.AdSec.IO.Graphics.Section;
using OasysUnits;

namespace SaveSectionImage
{
    /// <summary>
    /// This example shows how to save the XML to a file to create an SVG file.
    /// It also shows how you can use a third party library to convert the SVG into a PNG file.
    ///
    /// You might like to run the 'ApiVersion' example first, just to check
    /// that the API is installed correctly.
    /// </summary>
    public static class SaveSectionImage
    {
        static void Main()
        {
            // We're going to create a rectangular section with a sub-component.

            // Create the rectangular section
            IProfile profile = IRectangleProfile.Create(
                Length.FromMillimeters(500),
                Length.FromMillimeters(300)
            );
            IConcrete sectionMaterial = Concrete.IS456.Edition_2000.M30;
            ISection section = ISection.Create(profile, sectionMaterial);

            // Set the cover
            section.Cover = ICover.Create(Length.FromMillimeters(30));

            // Assign reinforcements to the main section
            IReinforcement reinforcementMaterial = Reinforcement.Steel.IS456.Edition_2000.S415;
            IBarBundle bar16mm = IBarBundle.Create(
                reinforcementMaterial,
                Length.FromMillimeters(16)
            );
            ILayer layerWithPitch125mm = ILayerByBarPitch.Create(
                bar16mm,
                Length.FromMillimeters(125)
            );
            IPerimeterGroup mainSectionReinforcement = IPerimeterGroup.Create();
            mainSectionReinforcement.Layers.Add(layerWithPitch125mm);
            IBarBundle link10mm = IBarBundle.Create(
                reinforcementMaterial,
                Length.FromMillimeters(10)
            );
            ILinkGroup mainSectionLink = ILinkGroup.Create(link10mm);
            section.ReinforcementGroups.Add(mainSectionReinforcement);
            section.ReinforcementGroups.Add(mainSectionLink);

            // Create another rectangular section which we'll use as a sub-component.
            IProfile subcomponentProfile = IRectangleProfile.Create(
                Length.FromMillimeters(250),
                Length.FromMillimeters(700)
            );
            ISection subcomponentSection = ISection.Create(subcomponentProfile, sectionMaterial);
            subcomponentSection.Cover = ICover.Create(Length.FromMillimeters(30));

            // Assign reinforcements to the sub-component section
            ILayer layerWithPitch120mm = ILayerByBarPitch.Create(
                bar16mm,
                Length.FromMillimeters(120)
            );
            IPerimeterGroup subcomponentReinforcement = IPerimeterGroup.Create();
            subcomponentReinforcement.Layers.Add(layerWithPitch120mm);
            ILinkGroup subcomponentLink = ILinkGroup.Create(link10mm);
            subcomponentSection.ReinforcementGroups.Add(subcomponentReinforcement);
            subcomponentSection.ReinforcementGroups.Add(subcomponentLink);

            // Add this sub-component section to the main section
            IPoint subcomponentOffset = IPoint.Create(
                Length.FromMillimeters(-200),
                Length.FromMillimeters(125)
            );
            ISubComponent subcomponent = ISubComponent.Create(
                subcomponentSection,
                subcomponentOffset
            );
            section.SubComponents.Add(subcomponent);

            // Flatten the section
            IAdSec adsecApp = IAdSec.Create(IS456.Edition_2000);
            ISection flattenedSection = adsecApp.Flatten(section);

            // Get SVG XML body as a string
            string svgStr = new SectionImageBuilder(flattenedSection).Svg();

            // This string can be written into SVG format file
            System.IO.File.WriteAllText("example.svg", svgStr);

            // This string can be converted into PNG format file with a third party library like https://www.nuget.org/packages/Svg
            Svg.SvgDocument.FromSvg<Svg.SvgDocument>(svgStr).Draw().Save("example.png");
        }
    }
}

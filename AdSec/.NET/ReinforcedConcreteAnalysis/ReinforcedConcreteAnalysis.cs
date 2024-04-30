using System;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using OasysUnits;

namespace ReinforcedConcreteAnalysis
{
    /// <summary>
    /// This example shows how to define a reinforced section and analyse it.
    /// The example goes on to apply a load and check the utilisation and
    /// maximum crack width.
    ///
    /// You might like to run the 'ApiVersion' example first, just to check
    /// that the API is installed correctly.
    /// </summary>
    public static class ReinforcedConcreteAnalysis
    {
        public static void Main()
        {
            // Create a rectangular section
            var profile = IRectangleProfile.Create(
                Length.FromMillimeters(800),
                Length.FromMillimeters(400)
            );
            IConcrete sectionMaterial = Concrete
                .EN1992
                .Part1_1
                .Edition_2004
                .NationalAnnex
                .GB
                .Edition_2014
                .C40_50;
            var section = ISection.Create(profile, sectionMaterial);

            // Set the cover
            section.Cover = ICover.Create(Length.FromMillimeters(40));

            // Set some reinforcement
            IReinforcement reinforcementMaterial = Reinforcement
                .Steel
                .EN1992
                .Part1_1
                .Edition_2004
                .NationalAnnex
                .GB
                .Edition_2014
                .S500B;
            IBarBundle bar20mm = IBarBundle.Create(
                reinforcementMaterial,
                Length.FromMillimeters(20)
            );
            IBarBundle bar16mm = IBarBundle.Create(
                reinforcementMaterial,
                Length.FromMillimeters(16)
            );
            IBarBundle bar12mm = IBarBundle.Create(
                reinforcementMaterial,
                Length.FromMillimeters(12)
            );

            // Define top reinforcement
            ILayer topLayer = ILayerByBarCount.Create(4, bar16mm);
            ITemplateGroup topReinforcement = ITemplateGroup.Create(ITemplateGroup.Face.Top);
            topReinforcement.Layers.Add(topLayer);

            // Define bottom reinforcement
            ILayer bottomLayerOne = ILayerByBarCount.Create(4, bar20mm);
            ILayer bottomLayerTwo = ILayerByBarCount.Create(4, bar16mm);
            ITemplateGroup bottomReinforcement = ITemplateGroup.Create(ITemplateGroup.Face.Bottom);
            bottomReinforcement.Layers.Add(bottomLayerOne);
            bottomReinforcement.Layers.Add(bottomLayerTwo);

            // Define link (stirrup)
            ILinkGroup link = ILinkGroup.Create(bar12mm);

            // Add defined reinforcement groups to section
            section.ReinforcementGroups.Add(topReinforcement);
            section.ReinforcementGroups.Add(bottomReinforcement);
            section.ReinforcementGroups.Add(link);

            // Analyse the section to create a solution
            var adSec = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014);
            ISolution solution = adSec.Analyse(section);

            // Calculate utilisation for a particular load
            var axialForce = Force.FromKilonewtons(-100);
            var majorAxisBending = Moment.FromKilonewtonMeters(-500);
            var minorAxisBending = Moment.Zero;
            var load = ILoad.Create(axialForce, majorAxisBending, minorAxisBending);
            IStrengthResult strengthResult = solution.Strength.Check(load);

            // Display utilisation as a percentage
            double utilisation = Math.Round(strengthResult.LoadUtilisation.Percent, 1);
            Console.WriteLine($"The utilisation is: {utilisation}%");

            // Calculate the serviceability crack width under the same load
            IServiceabilityResult serviceabilityResult = solution.Serviceability.Check(load);

            // Display the crack width in mm
            double crackWidth = Math.Round(
                serviceabilityResult.MaximumWidthCrack.Width.Millimeters,
                2
            );
            Console.WriteLine($"The maximum crack width is: {crackWidth}mm");
        }
    }
}

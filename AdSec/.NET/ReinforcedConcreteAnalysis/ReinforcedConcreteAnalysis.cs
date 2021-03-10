using System;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Profile;
using Oasys.AdSec.Reinforcement;

namespace Samples
{
    public static class ReinforcedConcreteAnalysis
    {
        static void Main()
        {
            // Create a circular section
            ICircle profile = ICircle.Create(0.5);
            IConcrete sectionMaterial = Concrete.EN1992.Part1_1.Edition_2004.C40_50;
            ISection section = ISection.Create(profile, sectionMaterial);

            // Set the cover
            section.Reinforcement.Cover = ICover.Create(0.05);

            // Set some reinforcement
            IReinforcementMat reinforcementMaterial = Reinforcement.Steel.EN1992.Part1_1.Edition_2004.S500B;
            ILayer layer = ILayerByBarCount.Create(4, reinforcementMaterial, 0.032);
            ILayeredGroup group = ILine.Create(IPoint.Create(0, 0), IPoint.Create(0.15, 0.15), layer);
            group.Layers.Add(layer);
            section.Reinforcement.Groups.Add(group);

            // Analyse the section to create a solution
            IAdSec adSec = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014);
            ISolution solution = adSec.Analyse(section);

            // Calculate utilisation for a particular load
            var axialForce = UnitsNet.Force.FromKilonewtons(-100);
            var majorAxisBending = 60000;
            var minorAxisBending = 0;
            var load = ILoad.Create(axialForce, majorAxisBending, minorAxisBending);
            IStrengthResult strengthResult = solution.Strength.Check(load);

            // Display utilisation as a percentage
            double utilisation = Math.Round(strengthResult.LoadUtilisation * 100, 1);
            Console.WriteLine($"The utilisation is: {utilisation}%");

            // Calculate the serviceability crack width under the same load
            IServiceabilityResult serviceabilityResult = solution.Serviceability.Check(load);

            // Display the crack width in mm
            double crackWidth = Math.Round(serviceabilityResult.MaximumWidthCrack.Width * 1000, 2);
            Console.WriteLine($"The maximum crack width is: {crackWidth}mm");
        }
    }
}

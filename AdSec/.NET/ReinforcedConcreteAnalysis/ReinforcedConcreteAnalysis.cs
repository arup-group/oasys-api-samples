using System;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Profile;
using Oasys.AdSec.Reinforcement;
using UnitsNet;
using IReinforcement = Oasys.AdSec.Materials.IReinforcement;

namespace ReinforcedConcreteAnalysis
{
    public static class ReinforcedConcreteAnalysis
    {
        public static void Main()
        {
            // Create a circular section
            var profile = ICircle.Create(Length.FromMillimeters(500));
            IConcrete sectionMaterial = Concrete.EN1992.Part1_1.Edition_2004.C40_50;
            var section = ISection.Create(profile, sectionMaterial);

            // Set the cover
            section.Reinforcement.Cover = ICover.Create(Length.FromMillimeters(50));

            // Set some reinforcement
            IReinforcement reinforcementMaterial = Reinforcement.Steel.EN1992.Part1_1.Edition_2004.S500B;
            ILayer layer = ILayerByBarCount.Create(4, reinforcementMaterial, Length.FromMillimeters(32));
            ILayeredGroup group = ILine.Create(IPoint.Create(Length.Zero, Length.Zero), IPoint.Create(Length.FromMillimeters(150), Length.FromMillimeters(150)), layer);
            group.Layers.Add(layer);
            section.Reinforcement.Groups.Add(group);

            // Analyse the section to create a solution
            var adSec = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014);
            ISolution solution = adSec.Analyse(section);

            // Calculate utilisation for a particular load
            var axialForce = Force.FromKilonewtons(-100);
            var majorAxisBending = Moment.FromKilonewtonMeters(60);
            var minorAxisBending = Moment.Zero;
            var load = ILoad.Create(axialForce, majorAxisBending, minorAxisBending);
            IStrengthResult strengthResult = solution.Strength.Check(load);

            // Display utilisation as a percentage
            double utilisation = Math.Round(strengthResult.LoadUtilisation.Percent, 1);
            Console.WriteLine($"The utilisation is: {utilisation}%");

            // Calculate the serviceability crack width under the same load
            IServiceabilityResult serviceabilityResult = solution.Serviceability.Check(load);

            // Display the crack width in mm
            double crackWidth = Math.Round(serviceabilityResult.MaximumWidthCrack.Width.Millimeters, 2);
            Console.WriteLine($"The maximum crack width is: {crackWidth}mm");
        }
    }
}

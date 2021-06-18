using System;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using UnitsNet;
using Oasys.Units;

namespace SampleProgram
{
    public static class SampleProgram
    {
        private static void Main()
        {
            // Create a circular section
            var profile = ICircleProfile.Create(Length.FromMillimeters(500));
            IConcrete sectionMaterial = Concrete.EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014.C40_50;
            var section = ISection.Create(profile, sectionMaterial);

            IReinforcement reinforcementMaterial = Reinforcement.Steel.EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014.S500B;
            ILayer layer = ILayerByBarCount.Create(4, IBarBundle.Create(reinforcementMaterial, Length.FromMillimeters(32)));
            // Set some reinforcement
            IGroup group = ILineGroup.Create(
                IPoint.Create(Length.Zero, Length.Zero),
                IPoint.Create(Length.FromMillimeters(150), Length.FromMillimeters(150)),
                layer);

            section.ReinforcementGroups.Add(group);

            // Analyse the section to create a solution
            var adSec = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014);
            ISolution solution = adSec.Analyse(section);

            // define load
            var axialForce = Force.FromKilonewtons(-100);
            var majorAxisBending = Moment.FromKilonewtonMeters(60);
            var minorAxisBending = Moment.Zero;
            var load = ILoad.Create(axialForce, majorAxisBending, minorAxisBending);

            // Check and Display whether the crack width is adequate
            bool result = IsCrackWidthSatisfactory(solution, load);
            Console.WriteLine($"Crack width acceptable: {result}");
        }

        public static bool IsCrackWidthSatisfactory(ISolution solution, ILoad load)
        {
            // Calculate the serviceability crack width under the same load
            IServiceabilityResult serviceabilityResult = solution.Serviceability.Check(load);

            // Return true if the crack width is acceptable
            double crackWidth = serviceabilityResult.MaximumWidthCrack.Width.Millimeters;
            return crackWidth < Length.FromMillimeters(2).Value;
        }
    }
}

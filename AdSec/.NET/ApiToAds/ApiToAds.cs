using System;
using Oasys.AdSec;
using Oasys.AdSec.IO.Serialization;
using Oasys.Collections;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using OasysUnits;
using Oasys.AdSec.DesignCode;

namespace ApiToAds
{
    /// <summary>
    /// This example shows how to convert an API object into .ads file
    ///
    /// You might like to run the 'ApiVersion' example first, just to check
    /// that the API is installed correctly.
    /// </summary>
    public static class ApiToAds
    {
        public static void Main()
        {
            // Create API section
            IConcrete C30 = Concrete
                .EN1992
                .Part1_1
                .Edition_2004
                .NationalAnnex
                .GB
                .PD6687
                .Edition_2010
                .C30_37;
            ICircleProfile circle_profile = ICircleProfile.Create(Length.FromMillimeters(1000));
            ISection section = ISection.Create(circle_profile, C30);

            // Use JsonConverter's SectionToJson method to obtain the JSON string
            JsonConverter converter = new JsonConverter(
                EN1992.Part1_1.Edition_2004.NationalAnnex.GB.PD6687.Edition_2010
            );
            String json = converter.SectionToJson(section);

            // Save this JSON string into .ads file
            System.IO.File.WriteAllText("adsec_section.ads", json);

            // To save a section with loads (or deformation)
            Oasys.AdSec.ILoad load_one = ILoad.Create(
                Force.FromKilonewtons(-10),
                Moment.Zero,
                Moment.Zero
            );
            Oasys.AdSec.ILoad load_two = ILoad.Create(
                Force.FromKilonewtons(-15),
                Moment.Zero,
                Moment.Zero
            );
            Oasys.Collections.IList<ILoad> load_list = Oasys.Collections.IList<ILoad>.Create();
            load_list.Add(load_one);
            load_list.Add(load_two);
            String json_with_loads = converter.SectionToJson(section, load_list);
            System.IO.File.WriteAllText("adsec_section_with_loads.ads", json_with_loads);
        }
    }
}

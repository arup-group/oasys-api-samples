using System;
using Oasys.AdSec;
using Oasys.AdSec.IO.Serialization;
using Oasys.Collections;

namespace AdsToApi
{
    /// <summary>
    /// This example shows how to convert an .ads file into API objects.
    ///
    /// You might like to run the 'ApiVersion' example first, just to check
    /// that the API is installed correctly.
    /// </summary>
    public static class AdsToApi
    {
        public static void Main()
        {
            // Read the required .ads file as string
            System.String path =
                "..\\..\\..\\..\\..\\..\\..\\DocumentationOnly\\.NET\\DotNetCodeSnippets\\api2section.ads";
            System.String json = System.IO.File.ReadAllText(path);

            // Use JsonParser's Deserialize method to convert from JSON to API objects
            ParsedResult api = JsonParser.Deserialize(json);

            // Access the ISection
            System.Collections.Generic.IList<ISection> api_sections = api.Sections;
            ISection section_one = api_sections[0];

            // Access and print warnings to check for warning messages while converting JSON to API
            System.Collections.Generic.IList<IWarning> api_warnings = api.Warnings;
            for (int i = 0; i < api_warnings.Count; i++)
            {
                Console.WriteLine(api_warnings[i].Description);
            }
        }
    }
}

using CsvHelper;
using CsvHelper.Configuration;
using GsaAPI;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GsaConsoleApplication
{
    internal class BeamScheduleExporter
    {
        public static void Export(IDictionary<int, AnalysisCaseResult> results, string outputFile)
        {
            using (var writer = new StreamWriter(Path.Combine(outputFile)))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Let's export these element results in a beam schdule type format
                // these are the column headers...
                csv.WriteField("Element ID");
                csv.WriteField("Case ID");
                csv.WriteField("Fx (start)");
                csv.WriteField("Fy (start)");
                csv.WriteField("Fz (start)");
                csv.WriteField("Fx (end)");
                csv.WriteField("Fy (end)");
                csv.WriteField("Fz (end)");
                csv.NextRecord();

                // we can  use a class map to customise the fields that are written to CSV
                // here we are using Double6Map which excludes the moments as (in this example) we don't need them
                csv.Context.RegisterClassMap<Double6Map>();

                // loop through the elements writing the results to CSV
                foreach (var caseResult in results)
                {
                    foreach (var elementResult in caseResult.Value.Element1DResults("all", 2))
                    {
                        csv.WriteRecord(elementResult.Key);
                        csv.WriteRecord(caseResult.Key);
                        csv.WriteRecord(elementResult.Value.Force.First());
                        csv.WriteRecord(elementResult.Value.Force.Last());
                        csv.NextRecord();
                    }
                }
            }
        }

        private class Double6Map : ClassMap<Double6>
        {
            public Double6Map()
            {
                Map(m => m.X).Name("Fx");
                Map(m => m.Y).Name("Fy");
                Map(m => m.Z).Name("Fz");
            }
        }
    }
}

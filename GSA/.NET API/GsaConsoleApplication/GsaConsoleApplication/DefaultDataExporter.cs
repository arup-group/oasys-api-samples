using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GsaConsoleApplication
{
    internal class DefaultDataExporter
    {
        public static void Export<TData>(IEnumerable<TData> data, string outputFile)
        {
            using (var writer = new StreamWriter(Path.Combine(outputFile)))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }
        }
    }
}

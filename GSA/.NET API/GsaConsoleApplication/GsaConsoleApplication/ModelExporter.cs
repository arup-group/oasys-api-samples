using GsaAPI;
using System.IO;

namespace GsaConsoleApplication
{
    internal class ModelExporter
    {
        internal static void Export(Model model, string outputDirectory)
        {
            // Make sure the output folder exists
            Directory.CreateDirectory(outputDirectory);

            // We can use the default data exporter to export all fields in simple data types
            DefaultDataExporter.Export(model.Nodes(), Path.Combine(outputDirectory, "Nodes.csv"));
            DefaultDataExporter.Export(model.Elements(), Path.Combine(outputDirectory, "Elements.csv"));
            DefaultDataExporter.Export(model.Members(), Path.Combine(outputDirectory, "Members.csv"));

            // Analyse Task 1 in advance of exporting some results
            model.Analyse(1);

            // we can create a custom exporter for more complex data, or to customise the fields
            // here we export the force at both ends of each beam using our Element1DResultExporter
            BeamScheduleExporter.Export(model.Results(), Path.Combine(outputDirectory, "BeamSchedule.csv"));
        }
    }
}

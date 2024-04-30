using CommandLine;
using GsaAPI;

namespace GsaConsoleApplication
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // this is the entry point for the application
            // parse the command line options and call the relevant export function(s)
            Parser.Default
                .ParseArguments<CommandLineOptions>(args)
                .WithParsed(options =>
                {
                    ModelExporter.Export(new Model(options.InputFile), options.OutputDirectory);
                });
        }
    }
}

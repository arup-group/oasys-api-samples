using CommandLine;

namespace GsaConsoleApplication
{
    public class CommandLineOptions
    {
        [Option('i', "input-file", Required = true, HelpText = "Set the input GSA file.")]
        public string InputFile { get; set; }

        [Option('o', "output-directory", Required = true, HelpText = "Set the output directory.")]
        public string OutputDirectory { get; set; }
    }
}

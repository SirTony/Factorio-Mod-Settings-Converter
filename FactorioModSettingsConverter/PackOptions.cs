using CommandLine;

namespace FactorioModSettingsConverter
{
    [Verb( "pack" )]
    internal struct PackOptions
    {
        [Option(
            'i',
            "input",
            MetaValue = "FILE",
            Required = false,
            Default = null,
            HelpText = "The source JSON file to pack. Omit to read from STDIN" )]
        public string InputFile { get; set; }

        [Option( 'o', "output", MetaValue = "FILE", Required = true, HelpText = "The destination file to write to" )]
        public string OutputFile { get; set; }
    }
}

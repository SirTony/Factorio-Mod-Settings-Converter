using CommandLine;

namespace FactorioModSettingsConverter
{
    [Verb( "unpack" )]
    internal struct UnpackOptions
    {
        [Option( 'i', "input", MetaValue = "FILE", Required = true, HelpText = "The source JSON file to pack" )]
        public string InputFile { get; set; }

        [Option(
            'o',
            "output",
            MetaValue = "FILE",
            Required = false,
            Default = null,
            HelpText = "The destination file to write to. Omit to write to STDOUT" )]
        public string OutputFile { get; set; }

        [Option(
            'I',
            "indented",
            MetaValue = "BOOL",
            Required = false,
            Default = true,
            HelpText = "Pretty print JSON" )]
        public bool Indented { get; set; }
    }
}

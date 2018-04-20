using System;
using System.IO;
using System.Text;
using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FactorioModSettingsConverter
{
    internal static class Program
    {
        private static int PackMain( PackOptions options )
        {
            try
            {
                string json;

                if( !String.IsNullOrWhiteSpace( options.InputFile ) )
                    json = File.ReadAllText( options.InputFile, Encoding.UTF8 );
                else
                {
                    using( var input = new StreamReader( Console.OpenStandardInput(), Encoding.UTF8 ) )
                    {
                        json = input.ReadToEnd();
                    }
                }

                using( var output = File.Open( options.OutputFile, FileMode.Create, FileAccess.Write, FileShare.None ) )
                using( var writer = new BinaryWriter( output, Encoding.UTF8 ) )
                {
                    var tree = JToken.Parse( json );
                    var version = Version.Parse( tree["version"].Value<string>() );

                    writer.Write( version );
                    writer.Write( tree["data"] );
                }
            }
            catch( Exception ex )
            {
                Console.Error.WriteLine( "Error [{0}]: {1}", ex.GetType().Name, ex.Message );
                return 1;
            }

            return 0;
        }

        private static int UnpackMain( UnpackOptions options )
        {
            try
            {
                using( var input = File.Open( options.InputFile, FileMode.Open, FileAccess.Read, FileShare.Read ) )
                using( var reader = new BinaryReader( input, Encoding.UTF8, false ) )
                {
                    var tree = new JObject
                    {
                        ["version"] = reader.ReadVersion().ToString( 4 ),
                        ["data"] = reader.ReadPropertyTree()
                    };

                    var json = tree.ToString( options.Indented ? Formatting.Indented : Formatting.None );

                    if( !String.IsNullOrWhiteSpace( options.OutputFile ) )
                        File.WriteAllText( options.OutputFile, json, Encoding.UTF8 );
                    else
                    {
                        Console.Out.WriteLine( json );
                        Console.Out.Flush();
                    }
                }
            }
            catch( Exception ex )
            {
                Console.Error.WriteLine( "Error [{0}]: {1}", ex.GetType().Name, ex.Message );
                return 1;
            }

            return 0;
        }

        private static int Main( string[] args ) => Parser
                .Default
                .ParseArguments<PackOptions, UnpackOptions>( args )
                .MapResult(
                    ( PackOptions opts ) => Program.PackMain( opts ),
                    ( UnpackOptions opts ) => Program.UnpackMain( opts ),
                    err => 1
                );
    }
}

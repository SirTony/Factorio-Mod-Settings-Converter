using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace FactorioModSettingsConverter
{
    internal static class BinaryWriterExtensions
    {
        public static void Write( this BinaryWriter writer, Version version )
        {
            writer.Write( (ushort)version.Major );
            writer.Write( (ushort)version.Minor );
            writer.Write( (ushort)version.Build );
            writer.Write( (ushort)version.Revision );
        }

        public static void WriteEnum<T>( this BinaryWriter writer, T value )
            where T : struct
        {
            var underlyingType = Enum.GetUnderlyingType( typeof( T ) );
            var writeMethod = typeof( BinaryWriter ).GetMethod(
                "Write",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                CallingConventions.HasThis,
                new[] { underlyingType },
                null
            );

            if( writeMethod?.ReturnType != typeof( void ) )
            {
                throw new MissingMethodException(
                    nameof( BinaryWriter ),
                    "Cannot find Write method corresponding to the enum's underlying type"
                );
            }

            writeMethod.Invoke( writer, new[] { Convert.ChangeType( value, underlyingType ) } );
        }

        public static void Write( this BinaryWriter writer, PropertyTreeType type )
        {
            writer.WriteEnum( type );
            writer.Write( false );
        }

        public static void WriteSpaceOptimizedUInt32( this BinaryWriter writer, uint value )
        {
            if( value < Byte.MaxValue )
                writer.Write( (byte)value );
            else
            {
                writer.Write( Byte.MaxValue );
                writer.Write( value );
            }
        }

        public static void WriteFactorioString( this BinaryWriter writer, string value )
        {
            var isEmpty = value.Length == 0;

            writer.Write( isEmpty );

            if( isEmpty ) return;

            writer.WriteSpaceOptimizedUInt32( (uint)value.Length );
            writer.Write( value.ToCharArray() );
        }

        public static void Write( this BinaryWriter writer, JToken token )
        {
            switch( token )
            {
                case JValue none when none.Type == JTokenType.Null:
                    writer.Write( PropertyTreeType.None );
                    break;

                case JValue boolean when boolean.Type == JTokenType.Boolean:
                    writer.Write( PropertyTreeType.Bool );
                    writer.Write( boolean.Value<bool>() );
                    break;

                case JValue num when ( num.Type == JTokenType.Float ) || ( num.Type == JTokenType.Integer ):
                    writer.Write( PropertyTreeType.Number );
                    writer.Write( num.Value<double>() );
                    break;

                case JValue str when str.Type == JTokenType.String:
                    writer.Write( PropertyTreeType.String );
                    writer.WriteFactorioString( str.Value<string>() );
                    break;

                case JArray arr:
                    writer.Write( PropertyTreeType.List );
                    writer.Write( (uint)arr.Count );

                    foreach( var item in arr )
                        writer.Write( item );

                    break;

                case JObject obj:
                    writer.Write( PropertyTreeType.Dictionary );
                    writer.Write( (uint)obj.Count );

                    foreach( var ( k, v ) in obj )
                    {
                        writer.WriteFactorioString( k );
                        writer.Write( v );
                    }

                    break;
            }
        }
    }
}

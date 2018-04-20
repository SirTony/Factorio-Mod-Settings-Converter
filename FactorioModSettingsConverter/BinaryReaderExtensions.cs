using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace FactorioModSettingsConverter
{
    internal static class BinaryReaderExtensions
    {
        public static Version ReadVersion( this BinaryReader reader )
            => new Version(
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16()
            );

        public static T ReadEnum<T>( this BinaryReader reader )
            where T : struct
        {
            var underlyingType = Enum.GetUnderlyingType( typeof( T ) );
            var readMethod = typeof( BinaryReader ).GetMethod(
                $"Read{underlyingType.Name}",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                CallingConventions.HasThis,
                Type.EmptyTypes,
                null
            );

            return readMethod?.ReturnType == underlyingType
                ? (T)readMethod.Invoke( reader, null )
                : throw new MissingMethodException(
                    nameof( BinaryReader ),
                    "Cannot find Read method corresponding to the enum's underlying type"
                );
        }

        public static ( PropertyTreeType, bool ) ReadPropertyTreeType( this BinaryReader reader )
            => ( reader.ReadEnum<PropertyTreeType>(), reader.ReadBoolean() );

        public static uint ReadSpaceOptimizedUInt32( this BinaryReader reader )
        {
            var len = reader.ReadByte();
            return len == Byte.MaxValue ? reader.ReadUInt32() : len;
        }

        public static string ReadFactorioString( this BinaryReader reader )
        {
            var isEmpty = reader.ReadBoolean();

            if( isEmpty ) return String.Empty;

            var len = reader.ReadSpaceOptimizedUInt32();
            return new String( reader.ReadChars( (int)len ) );
        }

        public static JToken ReadPropertyTree( this BinaryReader reader )
        {
            var ( type, _ ) = reader.ReadPropertyTreeType();

            switch( type )
            {
                case PropertyTreeType.None: return JValue.CreateNull();
                case PropertyTreeType.Bool: return new JValue( reader.ReadBoolean() );
                case PropertyTreeType.Number: return new JValue( reader.ReadDouble() );
                case PropertyTreeType.String: return JValue.CreateString( reader.ReadFactorioString() );

                case PropertyTreeType.List:
                {
                    var length = reader.ReadUInt32();
                    var arr = new JArray();

                    foreach( var _ in Enumerable.Range( 0, (int)length ) )
                        arr.Add( reader.ReadPropertyTree() );

                    return arr;
                }

                case PropertyTreeType.Dictionary:
                {
                    var length = reader.ReadUInt32();
                    var obj = new JObject();

                    foreach( var _ in Enumerable.Range( 0, (int)length ) )
                    {
                        var key = reader.ReadFactorioString();
                        obj[key] = reader.ReadPropertyTree();
                    }

                    return obj;
                }

                default: throw new NotSupportedException();
            }
        }
    }
}

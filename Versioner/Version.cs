using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Versioner
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct Version : IComparable<Version>, IEquatable<Version>
    {
        [JsonProperty("Major")]
        public UInt32 Major { get; private set; }
        [JsonProperty("Minor")]
        public UInt32 Minor { get; private set; }
        [JsonProperty("Patch")]
        public UInt32 Patch { get; private set; }

        public Version(UInt32 major, UInt32 minor = 0, UInt32 patch = 0)
            : this()
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public override string ToString()
        {
            if (Patch != 0) return Major + "." + Minor + "." + Patch;

            return Minor == 0 ? Major.ToString() : Major + "." + Minor;
        }

        public int CompareTo(Version other)
        {
            var c = Major.CompareTo(other.Major);
            if (c != 0) return c;
            c = Minor.CompareTo(other.Minor);
            if (c != 0) return c;
            c = Patch.CompareTo(other.Patch);
            return c;
        }

        public Boolean SemanticallyForwardOf(Version other)
        {
            return Major == other.Major && Minor == other.Minor && Patch >= other.Patch;
        }


        #region Equality members
        public bool Equals(Version other)
        {
            return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Version && Equals((Version) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                uint hashCode = Major;
                hashCode = (hashCode*397) ^ Minor;
                hashCode = (hashCode*397) ^ Patch;
                return (int)hashCode;
            }
        }

        public static bool operator ==(Version left, Version right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Version left, Version right)
        {
            return !left.Equals(right);
        }
        #endregion


        internal static readonly String RegexContent = @"(?<major>[0-9]+)(?:\.(?<minor>[0-9]+)(?:\.(?<patch>[0-9]+))?)?";
        private static readonly Regex ParseRegex = new Regex(RegexContent);
        public static Version Parse(String input)
        {
            try
            {
                var matches = ParseRegex.Match(input.Trim());
                var groups = matches.Groups;

                var major = groups["major"].Value;
                var minor = groups["minor"].Value;
                var patch = groups["patch"].Value;

                return new Version(UInt32.Parse(major),
                                   String.IsNullOrEmpty(minor) ? 0 : UInt32.Parse(minor),
                                   String.IsNullOrEmpty(patch) ? 0 : UInt32.Parse(patch));
            }
            catch (Exception ex)
            {
                throw new FormatException("Failed to parse Version string '" + input + "'.", ex);
            }
        }

        public static Boolean TryParse(String input, out Version output)
        {
            try
            {
                output = Parse(input);
                return true;
            }
            catch (Exception ex)
            {
                output = new Version();
                return false;
            }
        }
    }

    public class VersionStringConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) return;

            var v = (Version) value;

            writer.WriteValue(v.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool isNullable = Nullable.GetUnderlyingType(objectType) != null;
            Type t = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonSerializationException("VersionStringConverter requires a string value.");
            }

            return Version.Parse(reader.Value.ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (Version);
        }
    }
}

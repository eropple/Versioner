using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Versioner
{
    public struct Version : IComparable<Version>, IEquatable<Version>
    {
        public readonly UInt32 Major;
        public readonly UInt32 Minor;
        public readonly UInt32 Patch;

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
}

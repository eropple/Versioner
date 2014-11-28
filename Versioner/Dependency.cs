using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Versioner
{
    /// <summary>
    /// A single requirement to be provided by an `IDepending`. The composition
    /// of all Dependencies is used by `Resolver` 
    /// </summary>
    public class Dependency : IEquatable<Dependency>
    {


        public readonly String UniqueName;
        public readonly String OperatorToken;
        public readonly Version Version;

        private readonly TestDelegate _testDelegate;

        public Dependency(String uniqueName, String operatorToken, Version version)
        {
            UniqueName = uniqueName;
            OperatorToken = operatorToken;
            Version = version;

            TestDelegate d;
            if (!OperatorTable.TryGetValue(operatorToken, out d))
            {
                throw new ArgumentOutOfRangeException("Unrecognized constraint operator: " + operatorToken);
            }
            _testDelegate = d;
        }

        public override string ToString()
        {
            return String.Format("{0}, {1} {2}", UniqueName, OperatorToken, Version);
        }

        public Boolean IsSatisfiedBy(IVersioned versioned)
        {
            return versioned.UniqueName.Equals(UniqueName) && _testDelegate(this, versioned);
        }

        #region equality members
        public bool Equals(Dependency other)
        {
            return string.Equals(UniqueName, other.UniqueName) && string.Equals(OperatorToken, other.OperatorToken) && Version.Equals(other.Version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Dependency && Equals((Dependency) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (UniqueName != null ? UniqueName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (OperatorToken != null ? OperatorToken.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Version.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Dependency left, Dependency right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Dependency left, Dependency right)
        {
            return !left.Equals(right);
        }
        #endregion


        private delegate Boolean TestDelegate(Dependency c, IVersioned v);
        private static readonly Dictionary<String, TestDelegate> OperatorTable = new Dictionary<String,TestDelegate>
        {
            { "==",  (c, v) => v.Version.CompareTo(c.Version) == 0 },
            { "~>", (c, v) => v.Version.SemanticallyForwardOf(c.Version) },
            { ">",  (c, v) => v.Version.CompareTo(c.Version) > 0 },
            { ">=", (c, v) => v.Version.CompareTo(c.Version) >= 0 },
            { "<",  (c, v) => v.Version.CompareTo(c.Version) < 0 },
            { "<=", (c, v) => v.Version.CompareTo(c.Version) <= 0 }
        };
        public static IEnumerable<String> ValidOperators { get { return OperatorTable.Keys; } }


        internal static readonly String RegexContent = @"(?<name>[A-Za-z\-\._]+), (?<operator>[\<\>\=\~]{1,2}) (?<version>" +
                                                       Version.RegexContent + @")";
        private static readonly Regex ParseRegex = new Regex(RegexContent);

        public static Dependency Parse(String input)
        {
            try
            {
                var matches = ParseRegex.Match(input.Trim());
                var groups = matches.Groups;

                var name = groups["name"].Value;
                var op = groups["operator"].Value;
                var version = groups["version"].Value;

                return new Dependency(name, op, Version.Parse(version));
            }
            catch (Exception ex)
            {
                throw new FormatException("Failed to parse Version string '" + input + "'.", ex);
            }
        }

        public static Boolean TryParse(String input, out Dependency dep)
        {
            try
            {
                dep = Parse(input);
                return true;
            }
            catch (Exception)
            {
                dep = null;
                return false;
            }
        }
    }
}

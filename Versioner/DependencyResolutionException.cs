using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Versioner
{
    public class DependencyResolutionException : Exception
    {
        public IDictionary<String, ReadOnlyCollection<Dependency>> UnresolvedDependencies { get; private set; }

        private DependencyResolutionException(String message, IDictionary<String, ReadOnlyCollection<Dependency>> unresolvedDependencies)
        {
            UnresolvedDependencies = unresolvedDependencies;
        }


        internal static DependencyResolutionException FromDependencyMapping<TVersioned>(
                IDictionary<TVersioned, ReadOnlyCollection<Dependency>> deps)
            where TVersioned : IVersioned, IDepending
        {
            var named = new Dictionary<String, ReadOnlyCollection<Dependency>>(deps.Count);

            var sb = new StringBuilder();
            sb.Append("The dependency resolver failed for the following:\n");

            foreach (var kvp in deps)
            {
                sb.Append(" - ").Append(kvp.Key.UniqueName).Append("\n");
                
                foreach (var dep in kvp.Value)
                {
                    sb.Append("   - ").Append(dep).Append("\n");
                }

                named.Add(kvp.Key.UniqueName, kvp.Value);
            }

            return new DependencyResolutionException(sb.ToString(), named);
        }
    }
}

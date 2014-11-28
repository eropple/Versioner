using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Versioner
{
    /// <summary>
    /// Common operations on a collection of versioned and dependent items.
    /// </summary>
    public static class Resolver
    {
        private const Int32 RepeatCount = 20;

        /// <summary>
        /// Computes the entire set of transitive dependencies for the specified versioned
        /// items.
        /// </summary>
        /// <param name="items">
        /// All required items to be resolved.
        /// </param>
        /// <param name="availableImplicits">
        /// Any additional items that are available. For example, in the case of a software
        /// package manager, this would include packages not explicitly mentioned; if A
        /// depends on B, B (and any transitive dependencies of B) must either be in `items`
        /// or in `availableImplicits`.
        /// </param>
        /// <returns>
        /// A complete set of all values in `items`, plus any values in `availableImplicits`
        /// referenced by values in `items`.
        /// </returns>
        public static ISet<TVersioned> FindTransitiveDependencies<TVersioned>(IEnumerable<TVersioned> items,
                                                                              IEnumerable<TVersioned> availableImplicits = null)
            where TVersioned : IVersioned, IDepending
        {
            var retval = new HashSet<TVersioned>();
            var unsatisfied = new Dictionary<TVersioned, ReadOnlyCollection<Dependency>>();

            var itemList = items.ToList();
            var allItems = itemList.Concat(availableImplicits ?? Enumerable.Empty<TVersioned>()).ToDictionary(v => v.UniqueName);

            // TODO: this is a gross way to handle circularity. Implement a topological sorting solution.
            var repeatCount = new Dictionary<TVersioned, Int32>(allItems.Count);
            foreach (var item in allItems.Values) repeatCount.Add(item, 0);

            var queue = new Queue<TVersioned>(itemList);
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                var r = repeatCount[item];
                if (r == RepeatCount)
                {
                    throw new CircularDependencyException(String.Format("Transitive finder has seen '{0}' {1} times. Assuming circularity.", item, RepeatCount));
                }
                repeatCount[item] = r + 1;

                var unsatisfiedList = new List<Dependency>();
                foreach (var dep in item.Dependencies)
                {
                    TVersioned other;
                    if (!allItems.TryGetValue(dep.UniqueName, out other) || !dep.IsSatisfiedBy(other))
                    {
                        unsatisfiedList.Add(dep);
                        continue;
                    }

                    retval.Add(other);
                    queue.Enqueue(other);
                }

                if (unsatisfiedList.Count > 0) unsatisfied.Add(item, new ReadOnlyCollection<Dependency>(unsatisfiedList));
                retval.Add(item);
            }

            if (unsatisfied.Count > 0) throw DependencyResolutionException.FromDependencyMapping(unsatisfied);
            return retval;
        }

        private static void HasCircularDependenciesImpl<TVersioned>(TVersioned item,
                                                                    Dictionary<String, TVersioned> names,
                                                                    Dictionary<String, Int32> marks)
            where TVersioned : IVersioned, IDepending
        {
            var markValue = marks[item.UniqueName];
            if (markValue == 1) throw new CircularDependencyException();
            if (markValue == 2) return;

            marks[item.UniqueName] = 1; // mark temporarily

            foreach (var dep in item.Dependencies) HasCircularDependenciesImpl(names[dep.UniqueName], names, marks);

            marks[item.UniqueName] = 2;
        }

        /// <summary>
        /// Computes a list of versioned items with unsatisfied dependencies.
        /// </summary>
        /// <param name="items">
        /// All required items to be resolved.
        /// </param>
        /// <param name="availableImplicits">
        /// Any additional items that are available. For example, in the case of a software
        /// package manager, this would include packages not explicitly mentioned; if A
        /// depends on B, B (and any transitive dependencies of B) must either be in `items`
        /// or in `availableImplicits`.
        /// </param>
        /// <returns></returns>
        public static IDictionary<TVersioned, ReadOnlyCollection<Dependency>> 
            FindUnsatisfiedDependencies<TVersioned>(IEnumerable<TVersioned> items,
                                                    IEnumerable<TVersioned> availableImplicits = null)
            where TVersioned : IVersioned, IDepending
        {
            availableImplicits = availableImplicits ?? Enumerable.Empty<TVersioned>();
            var itemList = items.ToList();
            var allItems = itemList.Concat(availableImplicits).ToDictionary(v => v.UniqueName);

            return FindUnsatisfiedDependenciesImpl(itemList, allItems);
        }

        private static IDictionary<TVersioned, ReadOnlyCollection<Dependency>>
            FindUnsatisfiedDependenciesImpl<TVersioned>(IEnumerable<TVersioned> items, Dictionary<String, TVersioned> allItems)
                where TVersioned : IVersioned, IDepending
        {
            var retval = new Dictionary<TVersioned, ReadOnlyCollection<Dependency>>();

            foreach (var item in items)
            {
                var deps = item.Dependencies.Where(d => !allItems.Values.Any(v => d.IsSatisfiedBy(v))).ToList();
                if (deps.Count > 0) retval.Add(item, new ReadOnlyCollection<Dependency>(deps));
            }

            return retval;
        }

        public class CircularDependencyException : Exception
        {
            public CircularDependencyException()
            {
            }

            public CircularDependencyException(string message) : base(message)
            {
            }

            public CircularDependencyException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}

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

        /// <summary>
        /// Used when determining the order of operations when each item depends upon
        /// the loading, compilation, etc. of all its dependencies first. The returned
        /// list will be ordered in the same ordering as `items`, except that dependencies
        /// within `items` will be moved ahead of the item depending upon them and all
        /// dependencies from within `availableImplicits` will be inserted ahead of them
        /// as well.
        /// </summary>
        /// <param name="items">
        /// All required items to be resolved. They, when combined with any dependencies
        /// referred to within `availableImplicits`, must be a directed acyclic graph,
        /// otherwise Resolver.CircularDependencyException will be thrown.
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
        public static IList<TVersioned> ComputePriorityOrderWithDependencies<TVersioned>(IEnumerable<TVersioned> items,
                                                                                         IEnumerable<TVersioned> availableImplicits = null)
            where TVersioned : IVersioned, IDepending
        {
            var itemList = items as IList<TVersioned> ?? items.ToList();
            var allItems = FindTransitiveDependencies(itemList, availableImplicits).ToDictionary(i => i.UniqueName);
            var usedSet = new HashSet<TVersioned>();
            var workList = new List<TVersioned>(allItems.Count);

            foreach (var item in itemList)
            {
                ComputePriorityOrderWithDependenciesImpl(item, workList, allItems, usedSet);
            }

            return workList;
        }

        private static void ComputePriorityOrderWithDependenciesImpl<TVersioned>(TVersioned item,
                                                                                 List<TVersioned> workList,
                                                                                 Dictionary<String, TVersioned> allItems,
                                                                                 HashSet<TVersioned> used)
            where TVersioned : IVersioned, IDepending
        {
            if (used.Contains(item)) return;
            used.Add(item);

            foreach (var dep in item.Dependencies)
            {
                ComputePriorityOrderWithDependenciesImpl(allItems[dep.UniqueName], workList, allItems, used);
            }

            workList.Add(item);
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

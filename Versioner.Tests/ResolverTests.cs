using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Versioner.Tests
{
    [TestFixture]
    public class ResolverTests
    {
        [Test]
        public void SimpleTransitiveDependency()
        {
            var to1 = new TestObject("A", Version.Parse("1.0.0"), new[]
            {
                new Dependency("B", "==", Version.Parse("1.0.0")),
            });
            var to2 = new TestObject("B", Version.Parse("1.0.0"), new[]
            {
                new Dependency("C", "==", Version.Parse("1.0.0")), 
            });
            var to3 = new TestObject("C", Version.Parse("1.0.0"));
            var to4 = new TestObject("X", Version.Parse("1.0.0"));

            var deps = Resolver.FindTransitiveDependencies(new[] {to1}, new[] {to2, to3, to4}).ToList();
            Assert.Contains(to1, deps);
            Assert.Contains(to2, deps);
            Assert.Contains(to3, deps);
            Assert.IsFalse(deps.Contains(to4));
        }

        [Test]
        public void DependencyVersionFailure()
        {
            var to1 = new TestObject("A", Version.Parse("1.0.0"), new[]
            {
                new Dependency("B", "==", Version.Parse("1.0.1")),
            });
            var to2 = new TestObject("B", Version.Parse("1.0.0"));

            Assert.Throws<DependencyResolutionException>(() => Resolver.FindTransitiveDependencies(new[] {to1}, new[] {to2}));
        }

        [Test]
        public void CircularDependencyCheck()
        {
            var to1 = new TestObject("A", Version.Parse("1.0.0"), new[]
            {
                new Dependency("B", "==", Version.Parse("1.0.0")),
            });
            var to2 = new TestObject("B", Version.Parse("1.0.0"), new[]
            {
                new Dependency("C", "==", Version.Parse("1.0.0")),
            });
            var to3 = new TestObject("C", Version.Parse("1.0.0"), new[]
            {
                new Dependency("A", "==", Version.Parse("1.0.0")),
            });

            try
            {
                Resolver.FindTransitiveDependencies(new[] {to1, to2, to3});
                Assert.Fail("should have thrown CircularDependencyException");
            }
            catch (Resolver.CircularDependencyException)
            {
            }
        }

        [Test]
        public void ComputePriorityOrderWithDependencies()
        {
            var a = new TestObject("A", Version.Parse("1.0.0"), new[]
            {
                new Dependency("A1", "==", Version.Parse("1.0.0")),
            });
            var b = new TestObject("B", Version.Parse("1.0.0"), new[]
            {
                new Dependency("B1", "==", Version.Parse("1.0.0")),
                new Dependency("A2", "==", Version.Parse("1.0.0")), 
            });
            var a1 = new TestObject("A1", Version.Parse("1.0.0"), new[]
            {
                new Dependency("A2", "==", Version.Parse("1.0.0")),
            });
            var a2 = new TestObject("A2", Version.Parse("1.0.0"));
            var b1 = new TestObject("B1", Version.Parse("1.0.0"));

            var list = Resolver.ComputePriorityOrderWithDependencies(new[] { a, b }, new[] { a1, a2, b1 });

            Assert.IsEmpty(list.GroupBy(x => x)
                             .Where(g => g.Count() > 1)
                             .Select(g => g.Key), "duplicate entries found in the resolved list");

            Assert.IsTrue(list.IndexOf(a) < list.IndexOf(b), "a < b");
            Assert.IsTrue(list.IndexOf(a1) < list.IndexOf(a), "a1 < a");
            Assert.IsTrue(list.IndexOf(a2) < list.IndexOf(a1), "a2 < a1");
        }
    }
}

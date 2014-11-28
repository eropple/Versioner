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

            Assert.Throws<Resolver.CircularDependencyException>(
                () => Resolver.FindTransitiveDependencies(new[] {to1, to2, to3})
            );
        }
    }
}

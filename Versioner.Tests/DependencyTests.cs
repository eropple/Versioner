using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Versioner.Tests
{
    [TestFixture]
    public class DependencyTests
    {
        readonly TestObject _obj = new TestObject("Foo", Version.Parse("1.2.3"));

        [Test]
        public void SatisfactionTests()
        {
            Assert.IsFalse(new Dependency("Bar", "==", Version.Parse("1.2.3")).IsSatisfiedBy(_obj),
                           "different UniqueNames should fail.");

            Assert.IsTrue(new Dependency("Foo", "==", Version.Parse("1.2.3")).IsSatisfiedBy(_obj),
                          "equality should pass");
        }

        [Test]
        public void InOut()
        {
            var p1 = new Dependency("Foo", "==", new Version(1, 2, 3));
            Assert.AreEqual(p1.ToString(), Dependency.Parse(p1.ToString()).ToString());
            var p2 = new Dependency("Big.Bob", "~>", new Version(0, 1, 0));
            Assert.AreEqual(p2.ToString(), Dependency.Parse(p2.ToString()).ToString());
            var p3 = new Dependency("Some-Other_Thing", "<=", new Version(3, 12, 10));
            Assert.AreEqual(p3.ToString(), Dependency.Parse(p3.ToString()).ToString());
        }


        [Test]
        public void UniqueNames()
        {
            var c = new Dependency("Bar", "==", Version.Parse("1.2.3"));
            Assert.IsFalse(c.IsSatisfiedBy(_obj), "different UniqueNames should fail.");
        }

        [Test]
        public void Equality()
        {
            var p1 = new Dependency("Foo", "==", Version.Parse("1.2.3"));
            Assert.IsTrue(p1.IsSatisfiedBy(_obj));

            var f1 = new Dependency("Foo", "==", Version.Parse("1.2.4"));
            Assert.IsFalse(f1.IsSatisfiedBy(_obj));
        }

        [Test]
        public void SemanticForwardness()
        {
            var p1 = new Dependency("Foo", "~>", Version.Parse("1.2.0"));
            Assert.IsTrue(p1.IsSatisfiedBy(_obj));
            var p2 = new Dependency("Foo", "~>", Version.Parse("1.2.3"));
            Assert.IsTrue(p2.IsSatisfiedBy(_obj));

            var f1 = new Dependency("Foo", "~>", Version.Parse("1.2.6"));
            Assert.IsFalse(f1.IsSatisfiedBy(_obj));
        }

        [Test]
        public void LessThan()
        {
            var p1 = new Dependency("Foo", "<", Version.Parse("1.2.6"));
            Assert.IsTrue(p1.IsSatisfiedBy(_obj));

            var f1 = new Dependency("Foo", "<", Version.Parse("1.2.0"));
            Assert.IsFalse(f1.IsSatisfiedBy(_obj));
        }

        [Test]
        public void LessThanOrEquals()
        {
            var p1 = new Dependency("Foo", "<=", Version.Parse("1.2.6"));
            Assert.IsTrue(p1.IsSatisfiedBy(_obj));
            var p2 = new Dependency("Foo", "<=", Version.Parse("1.2.3"));
            Assert.IsTrue(p2.IsSatisfiedBy(_obj));

            var f1 = new Dependency("Foo", "<=", Version.Parse("1.2.0"));
            Assert.IsFalse(f1.IsSatisfiedBy(_obj));
        }

        [Test]
        public void GreaterThan()
        {
            var p1 = new Dependency("Foo", ">", Version.Parse("1.2.2"));
            Assert.IsTrue(p1.IsSatisfiedBy(_obj));

            var f1 = new Dependency("Foo", ">", Version.Parse("1.2.4"));
            Assert.IsFalse(f1.IsSatisfiedBy(_obj));
        }

        [Test]
        public void GreaterThanOrEquals()
        {
            var p1 = new Dependency("Foo", ">=", Version.Parse("1.2.2"));
            Assert.IsTrue(p1.IsSatisfiedBy(_obj));
            var p2 = new Dependency("Foo", ">=", Version.Parse("1.2.3"));
            Assert.IsTrue(p2.IsSatisfiedBy(_obj));

            var f1 = new Dependency("Foo", ">=", Version.Parse("1.2.4"));
            Assert.IsFalse(f1.IsSatisfiedBy(_obj));
        }
    }
}

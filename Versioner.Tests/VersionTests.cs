using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Versioner.Tests
{
    [TestFixture]
    public class VersionTests
    {
        [Test]
        public void CanParse()
        {
            var v1 = Version.Parse("0.0.1");
            Assert.AreEqual(1, v1.Patch);
            Assert.AreEqual(0, v1.Minor);
            Assert.AreEqual(0, v1.Major);

            var v2 = Version.Parse("0.1.2");
            Assert.AreEqual(2, v2.Patch);
            Assert.AreEqual(1, v2.Minor);
            Assert.AreEqual(0, v2.Major);

            var v3 = Version.Parse("1.2.3");
            Assert.AreEqual(3, v3.Patch);
            Assert.AreEqual(2, v3.Minor);
            Assert.AreEqual(1, v3.Major);

            var v4 = Version.Parse("1.2");
            Assert.AreEqual(2, v4.Minor);
            Assert.AreEqual(1, v4.Major);

            var v5 = Version.Parse("2");
            Assert.AreEqual(2, v5.Major);
        }

        [Test]
        public void StringConversion()
        {
            var v1 = new Version(1, 2, 3);
            Assert.AreEqual("1.2.3", v1.ToString());

            var v2 = new Version(0, 1, 2);
            Assert.AreEqual("0.1.2", v2.ToString());

            var v3 = new Version(0, 0, 1);
            Assert.AreEqual("0.0.1", v3.ToString());

            var v4 = new Version(1);
            Assert.AreEqual("1", v4.ToString());

            var v5 = new Version(1, 2);
            Assert.AreEqual("1.2", v5.ToString());
        }

        [Test]
        public void InOut()
        {
            var options = new []
            {
                new Version(1, 2, 3),
                new Version(1, 1),
                new Version(2), 
            };

            foreach (var v in options)
            {
                Assert.AreEqual(v, Version.Parse(v.ToString()));
            }
        }
    }
}

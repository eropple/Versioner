using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Versioner.Tests
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void VersionStringConverter()
        {
            var serializer = new JsonSerializer();

            var obj = new VersionTester
            {
                VersionString = new Version(1, 2, 3),
                VersionData = new Version(4, 5, 6)
            };


            var sw = new StringWriter();
            serializer.Serialize(sw, obj);


            var obj2 = serializer.Deserialize<VersionTester>(new JsonTextReader(new StringReader(sw.ToString())));

            Assert.AreEqual(obj.VersionString, obj2.VersionString, "versionstring failed");
            Assert.AreEqual(obj.VersionData, obj2.VersionData, "versiondata failed");
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class VersionTester
        {
            [JsonProperty("VersionString")]
            [JsonConverter(typeof(VersionStringConverter))]
            public Version VersionString { get; set; }

            [JsonProperty("VersionData")]
            public Version VersionData { get; set; }
        }


        [Test]
        public void DependencyStringConverter()
        {
            var serializer = new JsonSerializer();

            var obj = new DependencyTester
            {
                DependencyString = new Dependency("Foo", "==", new Version(1, 5, 1)),
                DependencyData = new Dependency("Bar", "~>", new Version(2, 1, 3))
            };


            var sw = new StringWriter();
            serializer.Serialize(sw, obj);


            var obj2 = serializer.Deserialize<DependencyTester>(new JsonTextReader(new StringReader(sw.ToString())));

            Assert.AreEqual(obj.DependencyString, obj2.DependencyString, "DependencyString failed");
            Assert.AreEqual(obj.DependencyData, obj2.DependencyData, "DependencyData failed");
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class DependencyTester
        {
            [JsonProperty("DependencyString")]
            [JsonConverter(typeof(DependencyStringConverter))]
            public Dependency DependencyString { get; set; }

            [JsonProperty("DependencyData")]
            public Dependency DependencyData { get; set; }
        }
    }
}

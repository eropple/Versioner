using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Versioner.Tests
{
    public class TestObject : IVersioned, IDepending
    {
        public TestObject(String uniqueName, Version version, IEnumerable<Dependency> constraints = null)
        {
            Dependencies = constraints ?? Enumerable.Empty<Dependency>();
            Version = version;
            UniqueName = uniqueName;
        }

        public String UniqueName { get; private set; }
        public Version Version { get; private set; }
        public IEnumerable<Dependency> Dependencies { get; private set; }
    }
}

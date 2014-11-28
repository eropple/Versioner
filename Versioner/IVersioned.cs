using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Versioner
{
    public interface IVersioned
    {
        String UniqueName { get; }
        Version Version { get; }
    }
}

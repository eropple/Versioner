using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Versioner
{
    public interface IDepending
    {
        IEnumerable<Dependency> Dependencies { get; } 
    }
}

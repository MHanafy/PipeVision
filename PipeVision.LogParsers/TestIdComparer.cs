using System;
using System.Collections.Generic;
using System.Text;
using PipeVision.Domain;

namespace PipeVision.LogParsers
{
    internal class TestNameComparer : IEqualityComparer<Domain.Test>
    {
        public bool Equals(Domain.Test x, Domain.Test y)
        {
            if (x == null) return y == null;
            return y != null && x.Name.Equals(y.Name);
        }

        public int GetHashCode(Domain.Test obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}

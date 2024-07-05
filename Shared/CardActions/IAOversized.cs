using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Shared
{
    public interface IAOversized
    {
        public int offset { get; }
        public Icon icon { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Modules.Migration.Interfaces
{
    public interface IStateNode
    {
        float Value { get; }
        bool IsValid { get; }
        IEnumerable<IStateNode> GetChilds();
    }

    public class AscendingStateComparer : IComparer<IStateNode>
    {
        public int Compare(IStateNode x, IStateNode y)
        {
            if (x == y) return 0;
            var val = (int)((y.Value - x.Value) * 1000);
            if (val == 0) return 1;
            else return val;
        }
    }
}

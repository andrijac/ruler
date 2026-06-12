using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Enums
{
    [Flags]
    public enum SaveTypes
    {
        /// <summary>
        /// Persist absolutely nothing.
        /// </summary>
        None = 0,

        /// <summary>
        /// Persist X and Y desktop coordinate positioning contexts.
        /// </summary>
        Location = 1 << 0,  // 1

        /// <summary>
        /// Persist Width and Height ruler length dimensions.
        /// </summary>
        Size = 1 << 1,      // 2

        /// <summary>
        /// Persist both location coordinate boundaries and size dimensions.
        /// </summary>
        All = Location | Size // 3 (1 + 2)
    }
}

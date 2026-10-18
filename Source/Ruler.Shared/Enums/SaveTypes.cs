using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Enums
{
    
    public enum SaveTypes
    {
        /// <summary>
        /// Persist absolutely nothing.
        /// </summary>
        None,

        /// <summary>
        /// Persist X and Y desktop coordinate positioning contexts.
        /// </summary>
        Location,

        /// <summary>
        /// Persist Width and Height ruler length dimensions.
        /// </summary>
        Size,

        /// <summary>
        /// Persist both location coordinate boundaries and size dimensions.
        /// </summary>
        All
    }
}

using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
    public interface IRulerInfoPreprocessor
    {
        IEnumerable<RulerInfo> Preprocess(IEnumerable<RulerInfo> items);
    }
}

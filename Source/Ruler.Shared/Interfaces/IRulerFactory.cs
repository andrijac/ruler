using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
    public interface IRulerFactory
    {
        RulerInfo CreateDefault();
        RulerInfo CreateFromArguments(string[] args);
        void CopyValues(RulerInfo source, RulerInfo target);
        void CopyValuesWithGUID(RulerInfo source, RulerInfo target);
        string ToParameterString(RulerInfo info);
    }
}

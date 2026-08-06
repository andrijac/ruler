using System.Collections.Generic;

using Ruler.Shared.Models;

namespace Ruler.Shared.Interfaces
{
    public interface IRulerSerializer
    {
        List<RulerInfo> DeserializeRulers(string rulersToDecrypt);
        string SerializeRulers(IEnumerable<RulerInfo> rulers);
    }
}

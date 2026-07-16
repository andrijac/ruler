using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
    public interface ISettingsService
    {
        /// <summary>
        /// Serializes a collection of RulerInfo objects into a JSON string.
        /// </summary>
        string SerializeRulers(IEnumerable<RulerInfo> rulers);

        /// <summary>
        /// Deserializes a JSON string into a list of RulerInfo objects.
        /// </summary>
        List<RulerInfo> DeserializeRulers(string json);
    }
}

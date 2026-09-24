using Newtonsoft.Json;

using Ruler.Shared.Interfaces;
using Ruler.Shared.Factories;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Ruler.Shared.Services
{
    /// <summary>
    /// Logic for converting Ruler data to and from string formats for persistence.
    /// This keeps the UI project from needing to know the details of serialization.
    /// </summary>
    public class RulerSerializer : IRulerSerializer
    {
       private readonly IRulerFactory _factory;
       
        public RulerSerializer(IRulerFactory factory)
        {
            _factory = factory;
        }

        public string SerializeRulers(IEnumerable<RulerInfo> rulers)
        {
            return JsonConvert.SerializeObject(rulers, Formatting.Indented);
        }

        public List<RulerInfo> DeserializeRulers(string rulersToDecrypt)
        {
            if (string.IsNullOrWhiteSpace(rulersToDecrypt))
            {
                // Note: If you need RulerFactory here, you can inject it via constructor
                return new List<RulerInfo>() { _factory.CreateDefault() };
            }
            try
            {
                List<RulerInfo> rulers = JsonConvert.DeserializeObject<List<RulerInfo>>(rulersToDecrypt);
                return rulers ?? new List<RulerInfo>() { _factory.CreateDefault() };
            }
            catch (JsonException ex)
            {
                Debug.WriteLine(ex);
                return new List<RulerInfo>() { _factory.CreateDefault() };
            }
        }
    }
}

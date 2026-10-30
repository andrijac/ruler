using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ruler.Shared.Services
{
    public class RulerRegistry : IRulerRegistry
    {
        private readonly Dictionary<Guid, IRuler> _rulers = new Dictionary<Guid, IRuler>();
        private readonly List<RulerInfo> _rulerstoSave = new List<RulerInfo>();
        private readonly object _lock = new object();

        public void Register(IRuler ruler)
        {
            lock (_lock)
            {
                if (ruler?.RulerData == null) return;

                // Ensure every ruler has a valid unique ID before registering
                if (ruler.RulerData.ID == Guid.Empty)
                {
                    Debug.WriteLine($"Ruler with Empty id found creating new ID");
                    ruler.RulerData.ID = Guid.NewGuid();
                }

                if (!_rulers.ContainsKey(ruler.RulerData.ID))
                {
                    Debug.WriteLine($"Ruler Registered {ruler.RulerData.ID}");
                    _rulers.Add(ruler.RulerData.ID, ruler);
                    if(!_rulerstoSave.Any(i=>i.ID == ruler.RulerData.ID))
                    {
                        _rulerstoSave.Add(ruler.RulerData);
                    }
                    Debug.WriteLine($"Number of Rulers Registered {_rulers.Count}");
                }
            }
        }

        public void Unregister(IRuler ruler)
        {
            lock (_lock)
            {
                if (_rulers.ContainsKey(ruler.RulerData.ID))
                {
                    if (!_rulerstoSave.Any(i => i.ID == ruler.RulerData.ID))
                    {
                        _rulerstoSave.Add(ruler.RulerData);
                    }
                    _rulers.Remove(ruler.RulerData.ID);
                }
            }
        }

        public IEnumerable<RulerInfo> GetRulersToSave()
        {
            lock (_lock)
            {
                return _rulerstoSave;
            }
        }
        public bool RulerExists(Guid rulerID)
        {
            bool found = false;
            lock (_lock)
            {
                if (_rulers.TryGetValue(rulerID,out var ruler))
                {
                    found = true;
                }
              
            }
            return found;
        }
        public void UpdateRulerByID(RulerInfo rulerInfo)
        {
            if(RulerExists(rulerInfo.ID))
            {
                var ruler = GetRulerById(rulerInfo.ID);
                ruler.SetRulerInfo(rulerInfo);
            }    
        }

        public IRuler GetRulerById(Guid id)
        {
            lock (_lock)
            {
                _rulers.TryGetValue(id, out var ruler);
                return ruler;
            }
        }

        public void CloseAll()
        {
            List<IRuler> toClose;
            lock (_lock)
            {
                toClose = _rulers.Values.ToList();
            }

            foreach (var ruler in toClose)
            {
                ruler.Close(); // This should ideally unregister itself via the Closed event
            }
            _rulerstoSave.Clear();
            _rulers.Clear();
        }
        public void Clear()
        {
            // Close all forms currently in memory
            foreach (var ruler in _rulers.ToList())
            {
                var form = ruler.Value as IRuler;
                form?.Close(); // This triggers your OnFormClosed logic
            }
            _rulerstoSave.Clear();
            _rulers.Clear();
        }
    }
}

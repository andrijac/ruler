using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ruler.Shared.Services
{
    public class RulerRegistry : IRulerRegistry
    {
        private readonly Dictionary<Guid, IRuler> _rulers = new Dictionary<Guid, IRuler>();
        private readonly object _lock = new object();

        public void Register(IRuler ruler)
        {
            lock (_lock)
            {
                if (!_rulers.ContainsKey(ruler.RulerData.ID))
                {
                    _rulers.Add(ruler.RulerData.ID, ruler);
                }
            }
        }

        public void Unregister(IRuler ruler)
        {
            lock (_lock)
            {
                if (_rulers.ContainsKey(ruler.RulerData.ID))
                {
                    _rulers.Remove(ruler.RulerData.ID);
                }
            }
        }

        public IEnumerable<IRuler> GetActiveRulers()
        {
            lock (_lock)
            {
                return _rulers.Values.ToList();
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
        }
        public void Clear()
        {
            // Close all forms currently in memory
            foreach (var ruler in _rulers.ToList())
            {
                var form = ruler.Value as IRuler;
                form?.Close(); // This triggers your OnFormClosed logic
            }
            _rulers.Clear();
        }
    }
}

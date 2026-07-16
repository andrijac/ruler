using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
    public interface IRulerRegistry
    {
        void Register(IRuler ruler);

        void Unregister(IRuler ruler);

        IEnumerable<IRuler> GetActiveRulers();

        // Retrieves a specific ruler by its unique ID
        IRuler GetRulerById(Guid id);

        // Closes and unregisters all active rulers
        void CloseAll();
    }
}

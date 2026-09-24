using Ruler.Shared.Models;

using System;

namespace Ruler.Shared.Interfaces
{
    public interface IMainFormFactory
    {
        IRuler Create(RulerInfo info,EventHandler handler);
    }
}

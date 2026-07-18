using Ruler.Shared.Models;

namespace Ruler.Shared.Interfaces
{
    public interface IMainFormFactory
    {
        IRuler Create(RulerInfo info);
    }
}

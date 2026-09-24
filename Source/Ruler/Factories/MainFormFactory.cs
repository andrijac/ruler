using Microsoft.Extensions.DependencyInjection;

using Ruler.Forms;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

using System;

namespace Ruler.Factories
{
    public class MainFormFactory : IMainFormFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRulerRegistry _registry;

        public MainFormFactory(IServiceProvider serviceProvider, IRulerRegistry registry)
        {
            _serviceProvider = serviceProvider;
            _registry = registry;
        }

        public IRuler Create(RulerInfo info, EventHandler handler = null)
        {
            // ActivatorUtilities automatically matches 'info' to the constructor argument 
            // in MainForm and resolves all other parameters (like IRulerFactory) 
            // from the _serviceProvider.
            var ruler = ActivatorUtilities.CreateInstance<MainForm>(_serviceProvider, info);
            if (handler != null)
            {
                if (ruler is IRuler rulers)
                {
                    rulers.Closed += handler;
                }
            }
            // Register the ruler in your shared registry immediately
            _registry.Register(ruler);

            return ruler;
        }
    }
}

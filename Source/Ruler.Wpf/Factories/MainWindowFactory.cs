using Microsoft.Extensions.DependencyInjection;

using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Wpf.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Factories
{
    public class RulerWindowFactory : IMainFormFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRulerRegistry _registry;

        public RulerWindowFactory(IServiceProvider serviceProvider, IRulerRegistry registry)
        {
            _serviceProvider = serviceProvider;
            _registry = registry;
        }

        public IRuler Create(RulerInfo info, EventHandler handler = null)
        {
            // 1. Create the WPF Window using DI. 
            // ActivatorUtilities passes 'info' as an explicit runtime argument 
            // and automatically resolves IRulerFactory and IRulerRegistry from the container.
            MainWindow ruler = ActivatorUtilities.CreateInstance<MainWindow>(_serviceProvider, info);

            // 2. Wire up the event handler if provided using the concrete window instance
            if (handler != null)
            {
                ruler.Closed += handler;
            }

            // 3. Register the ruler in your shared registry
            _registry.Register(ruler);

            return ruler; // Returns MainWindow cleanly as IRuler
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Wpf.Windows;

using System;
using System.Windows;

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
            // 1. Create the WPF Window using DI
            // 'RulerWindow' replaces your old 'MainForm'
            var ruler = ActivatorUtilities.CreateInstance<MainWindow>(_serviceProvider, info);

            // 2. Wire up the event handler if provided
            if (handler != null)
            {
                // WPF Windows use the 'Closed' event just like WinForms
                if (ruler is IRuler rulers)
                {
                    rulers.Closed += handler;
                }
            }

            // 3. Register the ruler in your shared registry
            _registry.Register(ruler);

            return ruler;
        }
    }
}

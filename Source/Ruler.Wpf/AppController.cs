using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Models;
using Ruler.Shared.Interfaces;
using System.Windows;

namespace Ruler.Wpf
{
    public class AppController
    {
        private readonly IRulerRegistry _registry;
        private readonly IPersistanceService _settings;
        private readonly IRulerFactory _rulerFactory;
        private readonly IMainFormFactory _mainFormFactory;

        public AppController(
            IRulerRegistry registry,
            IPersistanceService settings,
            IRulerFactory factory,
            IMainFormFactory mainFormFactory)
        {
            _registry = registry;
            _settings = settings;
            _rulerFactory = factory;
            _mainFormFactory = mainFormFactory;
        }

        /// <summary>
        /// Entry point for the application logic.
        /// </summary>
        public void Run(string[] args)
        {
            // 1. Handle command-line arguments
            if (args != null && args.Length > 0)
            {
                var settings = _rulerFactory.CreateFromArguments(args);
                ShowRuler(settings);
            }
            else
            {
                // 2. Load existing state ONLY if no arguments were provided
                LoadAll();
            }
        }

        private void LoadAll()
        {
            var rulers = _settings.LoadAll();
            if (rulers != null && rulers.Any())
            {
                foreach (var info in rulers)
                {
                    ShowRuler(info);
                }
            }
            else
            {
                ShowRuler(_rulerFactory.CreateDefault());
            }
        }

        private void ShowRuler(RulerInfo info)
        {
            // 1. Create the ruler (WPF Window wrapper)
            // The factory wires up the logic, we handle the event subscription here
            var ruler = _mainFormFactory.Create(info, OnWindowClosed);

            // 2. Wire up Duplicate logic
            ruler.DuplicateRequested += (sender, newInfo) =>
            {
                var copy = new RulerInfo();
                _rulerFactory.CopyValues(newInfo, copy);
                ShowRuler(copy);
            };

            ruler.Show();
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            // Cleanup logic
            if (sender is IRuler ruler)
            {
                _settings.Update(ruler.RulerData);
                _registry.Unregister(ruler);
            }

            // Check if any rulers are still active
            var active = _registry.GetActiveRulers();

            // In WPF, we use Application.Current.Shutdown()
            if (!active.Any())
            {
                Application.Current.Shutdown();
            }
        }
    }
}


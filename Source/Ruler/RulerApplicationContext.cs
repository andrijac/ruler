using Ruler.Forms;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

using System;
using System.Linq;
using System.Windows.Forms;

namespace Ruler
{
    /// <summary>
    /// Manages the application lifecycle for a multi-window environment.
    /// The process stays alive as long as at least one ruler is open.
    /// </summary>
    //public class RulerApplicationContext : ApplicationContext
    //{
    //    private static RulerApplicationContext _instance;
    //    public static List<Form> OpenForms
    //    {get; private set; }=new List<Form>();

    //    public RulerApplicationContext()
    //    {
    //        _instance = this;
    //    }
    //    public static void LoadAll()
    //    {
    //      var allrulers =  PersistenceManager.LoadAll();
    //        foreach (var ruler in allrulers)
    //        {
    //            MainForm form = new MainForm(ruler);
    //            Register(form);
    //            form.Show();
    //        }
    //    }   

    //    public static void Register(Form form)
    //    {
    //        if (_instance != null)
    //        {
    //            form.FormClosed += _instance.OnFormClosed;
    //            OpenForms.Add(form);
    //        }
    //    }

    //    private void OnFormClosed(object sender, FormClosedEventArgs e)
    //    {
    //        // If every ruler window is gone, kill the background process
    //        if (Application.OpenForms.Count == 0)
    //        {
    //           SaveAll();
    //            Application.Exit();
    //        }
    //    }
    //    public static void ClearAll()
    //    {
    //        OpenForms.Clear();
    //        SaveAll();
    //    }
    //    private static void SaveAll()
    //    {
    //        PersistenceManager.SaveAll(OpenForms);
    //    }
    //    public static void CloseAll()
    //    {
    //        var a = OpenForms.ToArray();    
    //        foreach (var form in a)
    //        {
    //            form.Close();
    //        }
    //    }

    //    public void Start(string[] args)
    //    {
    //        if (args.Length > 0)
    //        {
    //            // Scenario A: Launched with arguments (likely a Duplicate command)
    //            // Use the Shared Library to parse the string back into a RulerInfo object
    //            RulerInfo info = RulerFactory.CreateFromArguments(args);

    //            MainForm form = new MainForm(info);
    //            Register(form);
    //            form.Show();
    //        }
    //        else
    //        {
    //            // Scenario B: Normal launch
    //            LoadAll();
    //        }

    //        // Start the application loop using the Context instance
    //        Application.Run(new RulerApplicationContext());
    //    }
    //}
    public class RulerApplicationContext : ApplicationContext
    {
        private readonly IRulerRegistry _registry;
        private readonly IPersistanceService _settings;
        private readonly IRulerFactory _rulerFactory;
        private readonly IMainFormFactory _mainFormFactory;

        public RulerApplicationContext(
            IRulerRegistry registry,
            IPersistanceService settings,
            IRulerFactory factory,
            IMainFormFactory mainform)
        {
            _registry = registry;
            _settings = settings;
            _rulerFactory = factory;
            _mainFormFactory = mainform;
        }

        public void LoadAll()
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

        public void Initialize(string[] args)
        {
            // 1. Check if ANY arguments were provided
            if (args != null && args.Length > 0)
            {
                // Use your new Factory method to parse the command line
                var settings = _rulerFactory.CreateFromArguments(args);
                ShowRuler(settings);  
            }
            else
            {
              LoadAll();
            }
        }
        private void ShowRuler(RulerInfo info)
        {
            // 1. Create the ruler via the factory, passing the lifecycle handler
            // The factory handles the actual event wiring (FormClosed -> OnFormClosed)
            var ruler = _mainFormFactory.Create(info, OnFormClosed);
            System.Diagnostics.Debug.WriteLine("Attempting to subscribe...");
            // 2. Wire up the "Behavior" event (Duplication)
            // We handle this here because it involves logic only the Context should know
            ruler.DuplicateRequested += (sender, newInfo) =>
            {
                var copy = new RulerInfo();
                _rulerFactory.CopyValues(newInfo, copy);
                System.Diagnostics.Debug.WriteLine("Event fired!");
                ShowRuler(copy);
            };

            ruler.Show();
        }
        private void OnFormClosed(object sender, EventArgs e)
        {
            if (sender is IRuler ruler)
            {
                _settings.Update(ruler.RulerData);
                _registry.Unregister(ruler);
            }
            // Use the registry to check count instead of Application.OpenForms
            var active = _registry.GetActiveRulers();
            

            if (!active.Any())
            {
                Application.Exit();
            }
        }
    }
}

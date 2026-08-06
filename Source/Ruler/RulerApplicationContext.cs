using Ruler.Forms;
using Ruler.Shared.Factories;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ruler
{
    /// <summary>
    /// Manages the application lifecycle for a multi-window environment.
    /// The process stays alive as long as at least one ruler is open.
    /// </summary>
    public class RulerApplicationContext : ApplicationContext
    {
        private static RulerApplicationContext _instance;
        public static List<Form> OpenForms
        {get; private set; }=new List<Form>();

        public RulerApplicationContext()
        {
            _instance = this;
        }
        public static void LoadAll()
        {
          var allrulers =  PersistenceManager.LoadAll();
            foreach (var ruler in allrulers)
            {
                MainForm form = new MainForm(ruler);
                Register(form);
                form.Show();
            }
        }   

        public static void Register(Form form)
        {
            if (_instance != null)
            {
                form.FormClosed += _instance.OnFormClosed;
                OpenForms.Add(form);
            }
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            // If every ruler window is gone, kill the background process
            if (Application.OpenForms.Count == 0)
            {
               SaveAll();
                Application.Exit();
            }
        }
        public static void ClearAll()
        {
            OpenForms.Clear();
            SaveAll();
        }
        private static void SaveAll()
        {
            PersistenceManager.SaveAll(OpenForms);
        }
        public static void CloseAll()
        {
            var a = OpenForms.ToArray();    
            foreach (var form in a)
            {
                form.Close();
            }
        }

        public void Start(string[] args)
        {
            if (args.Length > 0)
            {
                // Scenario A: Launched with arguments (likely a Duplicate command)
                // Use the Shared Library to parse the string back into a RulerInfo object
                RulerInfo info = RulerFactory.CreateFromArguments(args);

                MainForm form = new MainForm(info);
                Register(form);
                form.Show();
            }
            else
            {
                // Scenario B: Normal launch
                LoadAll();
            }

            // Start the application loop using the Context instance
            Application.Run(new RulerApplicationContext());
        }
    }
}

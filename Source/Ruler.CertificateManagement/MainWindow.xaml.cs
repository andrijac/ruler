using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Media;

namespace Ruler.CertificateManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string MasterPassword = "SuperSecureMasterPassword123!";
        private readonly Brush SafeBackground = Brushes.Green;    // Soft green (> 5 months)
        private readonly Brush WarningBackground = Brushes.Red; // Soft red (<= 5 months / missing)

        public MainWindow()
        {
            InitializeComponent();
            CheckCertificateStatus(); // Check status on startup
        }

        private void BtnRunSetup_Click(object sender, RoutedEventArgs e)
        {
            ExecuteSetupLogic(false);
            MessageBox.Show("Selected security setup execution complete!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCopyMaster_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtMasterXml.Text))
            {
                Clipboard.SetText(TxtMasterXml.Text);
                MessageBox.Show("Master XML copied to clipboard.", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnCopyActive_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtActiveXml.Text))
            {
                Clipboard.SetText(TxtActiveXml.Text);
                MessageBox.Show("Active XML copied to clipboard.", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void BtnRecheck_Click(object sender, RoutedEventArgs e)
        {
            CheckCertificateStatus(); // Dry run check only
        }
        /// <summary>
        /// Pure inspection method: checks all certificates, updates status texts, 
        /// border colors, and XML text boxes without modifying or generating anything.
        /// </summary>
        private void CheckCertificateStatus()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                // ==========================================
                // 1. MASTER PFX STATUS CHECK
                // ==========================================
                {
                    string basePath = Path.Combine(desktopPath, "MasterPfx.pfx");
                    X509Certificate2 latestMasterCert = null;

                    if (File.Exists(basePath))
                    {
                        try { latestMasterCert = new X509Certificate2(basePath, MasterPassword); } catch { }
                    }

                    int searchIndex = 1;
                    while (true)
                    {
                        string numberedPath = Path.Combine(desktopPath, string.Format("MasterPfx_{0}.pfx", searchIndex));
                        if (!File.Exists(numberedPath)) break;

                        try
                        {
                            var cert = new X509Certificate2(numberedPath, MasterPassword);
                            if (latestMasterCert != null) latestMasterCert.Dispose();
                            latestMasterCert = cert;
                        }
                        catch { }
                        searchIndex++;
                    }

                    if (latestMasterCert != null)
                    {
                        using (var pubRsa = latestMasterCert.GetRSAPublicKey())
                        {
                            TxtMasterXml.Text = pubRsa.ToXmlString(false);
                        }

                        if (latestMasterCert.NotAfter > DateTime.UtcNow.AddMonths(5))
                        {
                            TxtMasterStatus.Text = string.Format("Status: Valid PFX found, expires {0}.", DateFormatted(latestMasterCert.NotAfter));
                            BorderMaster.Background = SafeBackground;
                        }
                        else
                        {
                            TxtMasterStatus.Text = string.Format("Status: Latest PFX expiring soon ({0}). Will generate next index on run.", DateFormatted(latestMasterCert.NotAfter));
                            BorderMaster.Background = WarningBackground;
                        }
                        latestMasterCert.Dispose();
                    }
                    else
                    {
                        TxtMasterStatus.Text = "Status: No valid Master PFX found. Will create new on run.";
                        BorderMaster.Background = WarningBackground;
                        TxtMasterXml.Text = string.Empty;
                    }
                }

                // ==========================================
                // 2. ACTIVE SIGNER STATUS CHECK
                // ==========================================
                {
                    X509Certificate2 existingActive = null;
                    int maxActiveN = -1;

                    using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                    {
                        store.Open(OpenFlags.ReadOnly);
                        var activeCerts = store.Certificates.Find(X509FindType.FindBySubjectName, "Ruler Active Signer", false);

                        foreach (var cert in activeCerts)
                        {
                            int certN = 0;
                            string subject = cert.Subject;
                            if (subject.Contains("Ruler Active Signer"))
                            {
                                string remainder = subject.Replace("CN=Ruler Active Signer", "").Trim();
                                int.TryParse(remainder, out certN);
                            }

                            if (certN > maxActiveN)
                            {
                                maxActiveN = certN;
                                if (existingActive != null) existingActive.Dispose();
                                existingActive = new X509Certificate2(cert);
                            }
                        }
                    }

                    if (existingActive != null)
                    {
                        using (var pubRsa = existingActive.GetRSAPublicKey())
                        {
                            TxtActiveXml.Text = pubRsa.ToXmlString(false);
                        }
                        File.WriteAllText("ruler_active_thumbprint.dat", existingActive.Thumbprint);

                        if (existingActive.NotAfter <= DateTime.UtcNow.AddMonths(5))
                        {
                            TxtActiveStatus.Text = string.Format("Status: Active cert expiring soon ({0}), expires {1}.", existingActive.Subject, DateFormatted(existingActive.NotAfter));
                            BorderActive.Background = WarningBackground;
                        }
                        else
                        {
                            TxtActiveStatus.Text = string.Format("Status: Found Active certificate in store ({0}), expires {1}.", existingActive.Subject, DateFormatted(existingActive.NotAfter));
                            BorderActive.Background = SafeBackground;
                        }
                        existingActive.Dispose();
                    }
                    else
                    {
                        TxtActiveStatus.Text = "Status: Active certificate not found in store. Will create on run.";
                        BorderActive.Background = WarningBackground;
                        TxtActiveXml.Text = string.Empty;
                    }
                }

                // ==========================================
                // 3. LOCAL DATA SIGNER STATUS CHECK
                // ==========================================
                {
                    X509Certificate2 existingLocal = null;
                    int maxLocalN = -1;

                    using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                    {
                        store.Open(OpenFlags.ReadOnly);
                        var localCerts = store.Certificates.Find(X509FindType.FindBySubjectName, "Ruler Local Data Signer", false);

                        foreach (var cert in localCerts)
                        {
                            int certN = 0;
                            string subject = cert.Subject;
                            if (subject.Contains("Ruler Local Data Signer"))
                            {
                                string remainder = subject.Replace("CN=Ruler Local Data Signer", "").Trim();
                                int.TryParse(remainder, out certN);
                            }

                            if (certN > maxLocalN)
                            {
                                maxLocalN = certN;
                                if (existingLocal != null) existingLocal.Dispose();
                                existingLocal = new X509Certificate2(cert);
                            }
                        }
                    }

                    if (existingLocal != null)
                    {
                        File.WriteAllText("ruler_local_thumbprint.dat", existingLocal.Thumbprint);

                        if (existingLocal.NotAfter <= DateTime.UtcNow.AddMonths(5))
                        {
                            TxtLocalStatus.Text = string.Format("Status: Local cert expiring soon ({0}), expires {1}.", existingLocal.Subject, DateFormatted(existingLocal.NotAfter));
                            BorderLocal.Background = WarningBackground;
                        }
                        else
                        {
                            TxtLocalStatus.Text = string.Format("Status: Found Local Data certificate in store ({0}), expires {1}.", existingLocal.Subject, DateFormatted(existingLocal.NotAfter));
                            BorderLocal.Background = SafeBackground;
                        }
                        existingLocal.Dispose();
                    }
                    else
                    {
                        TxtLocalStatus.Text = "Status: Local Data certificate not found in store. Will create on run.";
                        BorderLocal.Background = WarningBackground;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error checking certificate status: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ExecuteSetupLogic(bool dryRun)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                int targetMasterN = 0;

                // ==========================================
                // 1. MASTER CERTIFICATE LOGIC
                // ==========================================
                if (ChkMaster.IsChecked == true)
                {
                    string masterPfxPath = string.Empty;
                    string masterPublicKeyXml = string.Empty;
                    bool skipMasterGeneration = false;

                    string basePath = System.IO.Path.Combine(desktopPath, "MasterPfx.pfx");
                    string latestFoundPath = null;
                    X509Certificate2 latestMasterCert = null;

                    if (File.Exists(basePath))
                    {
                        try
                        {
                            var cert = new X509Certificate2(basePath, MasterPassword);
                            latestFoundPath = basePath;
                            latestMasterCert = cert;
                            targetMasterN = 0;
                        }
                        catch { }
                    }

                    int searchIndex = 1;
                    while (true)
                    {
                        string numberedPath = System.IO.Path.Combine(desktopPath, string.Format("MasterPfx_{0}.pfx", searchIndex));
                        if (!File.Exists(numberedPath)) break;

                        try
                        {
                            var cert = new X509Certificate2(numberedPath, MasterPassword);
                            if (latestMasterCert != null) latestMasterCert.Dispose();
                            latestFoundPath = numberedPath;
                            latestMasterCert = cert;
                            targetMasterN = searchIndex;
                        }
                        catch { }

                        searchIndex++;
                    }

                    if (latestMasterCert != null)
                    {
                        if (latestMasterCert.NotAfter > DateTime.UtcNow.AddMonths(5))
                        {
                            masterPfxPath = latestFoundPath;
                            using (var pubRsa = latestMasterCert.GetRSAPublicKey())
                            {
                                masterPublicKeyXml = pubRsa.ToXmlString(false);
                            }
                            skipMasterGeneration = true;
                            TxtMasterStatus.Text = string.Format("Status: Valid PFX found ({0}), expires {1}.", System.IO.Path.GetFileName(masterPfxPath), DateFormatted(latestMasterCert.NotAfter));
                            BorderMaster.Background = SafeBackground;
                            TxtMasterXml.Background = SafeBackground;
                        }
                        else
                        {
                            targetMasterN = targetMasterN + 1;
                            TxtMasterStatus.Text = string.Format("Status: Latest PFX expiring soon. Will generate MasterPfx_{0}.pfx.", targetMasterN);
                            BorderMaster.Background = WarningBackground;
                            TxtMasterXml.Background = WarningBackground;
                        }
                        latestMasterCert.Dispose();
                    }
                    else
                    {
                        targetMasterN = File.Exists(basePath) ? 1 : 0;
                        TxtMasterStatus.Text = "Status: No valid Master PFX found. Will create new.";
                        BorderMaster.Background = WarningBackground;
                        TxtMasterXml.Background = WarningBackground;
                    }

                    if (!dryRun && !skipMasterGeneration)
                    {
                        masterPfxPath = (targetMasterN == 0) ? System.IO.Path.Combine(desktopPath, "MasterPfx.pfx") : System.IO.Path.Combine(desktopPath, string.Format("MasterPfx_{0}.pfx", targetMasterN));

                        using (RSA rsaMaster = RSA.Create())
                        {
                            rsaMaster.KeySize = 4096;
                            string masterSubject = (targetMasterN == 0) ? "CN=Ruler Master Root" : string.Format("CN=Ruler Master Root {0}", targetMasterN);
                            var masterReq = new CertificateRequest(masterSubject, rsaMaster, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                            masterReq.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.KeyEncipherment, true));

                            using (X509Certificate2 masterCert = masterReq.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(99)))
                            {
                                byte[] pfxBytes = masterCert.Export(X509ContentType.Pfx, MasterPassword);
                                File.WriteAllBytes(masterPfxPath, pfxBytes);

                                using (var pubRsa = masterCert.GetRSAPublicKey())
                                {
                                    masterPublicKeyXml = pubRsa.ToXmlString(false);
                                }
                            }
                        }
                        TxtMasterStatus.Text = string.Format("Status: Generated new Master PFX at {0}", masterPfxPath);
                        BorderMaster.Background = SafeBackground;
                    }
                    TxtMasterXml.Text = masterPublicKeyXml;
                    TxtMasterXml.Background = SafeBackground;
                }
                else
                {
                    TxtMasterStatus.Text = "Status: Skipped by user selection.";
                    TxtMasterXml.Text = string.Empty;
                    BorderMaster.Background = SafeBackground;
                    TxtMasterXml.Background = SafeBackground;
                }

                // ==========================================
                // 2. ACTIVE CERTIFICATE LOGIC
                // ==========================================
                if (ChkActive.IsChecked == true)
                {
                    string activePublicKeyXml = string.Empty;
                    X509Certificate2 existingActive = null;
                    int maxActiveN = -1;

                    using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                    {
                        store.Open(OpenFlags.ReadWrite);
                        var activeCerts = store.Certificates.Find(X509FindType.FindBySubjectName, "Ruler Active Signer", false);

                        foreach (var cert in activeCerts)
                        {
                            int certN = 0;
                            string subject = cert.Subject;
                            if (subject.Contains("Ruler Active Signer"))
                            {
                                string remainder = subject.Replace("CN=Ruler Active Signer", "").Trim();
                                int.TryParse(remainder, out certN);
                            }

                            if (certN > maxActiveN)
                            {
                                maxActiveN = certN;
                                if (existingActive != null) existingActive.Dispose();
                                existingActive = new X509Certificate2(cert);
                            }
                        }

                        if (existingActive != null)
                        {
                            using (var pubRsa = existingActive.GetRSAPublicKey())
                            {
                                activePublicKeyXml = pubRsa.ToXmlString(false);
                            }
                            File.WriteAllText("ruler_active_thumbprint.dat", existingActive.Thumbprint);

                            if (existingActive.NotAfter <= DateTime.UtcNow.AddMonths(5))
                            {
                                TxtActiveStatus.Text = string.Format("Status: Active cert expiring soon ({0}), expires {1}.", existingActive.Subject, DateFormatted(existingActive.NotAfter));
                                BorderActive.Background = WarningBackground;
                                TxtActiveXml.Background = WarningBackground;
                            }
                            else
                            {
                                TxtActiveStatus.Text = string.Format("Status: Found Active certificate in store ({0}), expires {1}.", existingActive.Subject, DateFormatted(existingActive.NotAfter));
                                BorderActive.Background = SafeBackground;
                                TxtActiveXml.Background = SafeBackground;
                            }
                            existingActive.Dispose();
                        }
                        else
                        {
                            TxtActiveStatus.Text = "Status: Active certificate not found in store.";
                            BorderActive.Background = WarningBackground;
                            TxtActiveXml.Background = WarningBackground;

                            if (!dryRun)
                            {
                                int newActiveN = (targetMasterN > 0) ? targetMasterN : 0;
                                string activeSubject = (newActiveN == 0) ? "CN=Ruler Active Signer" : string.Format("CN=Ruler Active Signer {0}", newActiveN);

                                foreach (var cert in activeCerts)
                                {
                                    store.Remove(cert);
                                }

                                using (RSA rsaActive = RSA.Create())
                                {
                                    rsaActive.KeySize = 2048;
                                    var activeReq = new CertificateRequest(activeSubject, rsaActive, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                                    activeReq.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));

                                    using (X509Certificate2 activeCert = activeReq.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(2)))
                                    {
                                        store.Add(activeCert);
                                        File.WriteAllText("ruler_active_thumbprint.dat", activeCert.Thumbprint);

                                        using (var pubRsa = activeCert.GetRSAPublicKey())
                                        {
                                            activePublicKeyXml = pubRsa.ToXmlString(false);
                                        }

                                        TxtActiveStatus.Text = string.Format("Status: Generated and stored new Active certificate ({0}), expires {1}.", activeSubject, DateFormatted(activeCert.NotAfter));
                                        BorderActive.Background = SafeBackground;
                                        TxtActiveXml.Background = SafeBackground;
                                    }
                                }
                            }
                        }
                    }
                    TxtActiveXml.Text = activePublicKeyXml;
                }
                else
                {
                    TxtActiveStatus.Text = "Status: Skipped by user selection.";
                    TxtActiveXml.Text = string.Empty;
                    BorderActive.Background = SafeBackground;
                    TxtActiveXml.Background = SafeBackground;
                }

                // ==========================================
                // 3. LOCAL DATA CERTIFICATE LOGIC
                // ==========================================
                if (ChkLocal.IsChecked == true)
                {
                    X509Certificate2 existingLocal = null;
                    int maxLocalN = -1;

                    using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                    {
                        store.Open(OpenFlags.ReadWrite);
                        var localCerts = store.Certificates.Find(X509FindType.FindBySubjectName, "Ruler Local Data Signer", false);

                        foreach (var cert in localCerts)
                        {
                            int certN = 0;
                            string subject = cert.Subject;
                            if (subject.Contains("Ruler Local Data Signer"))
                            {
                                string remainder = subject.Replace("CN=Ruler Local Data Signer", "").Trim();
                                int.TryParse(remainder, out certN);
                            }

                            if (certN > maxLocalN)
                            {
                                maxLocalN = certN;
                                if (existingLocal != null) existingLocal.Dispose();
                                existingLocal = new X509Certificate2(cert);
                            }
                        }

                        if (existingLocal != null)
                        {
                            File.WriteAllText("ruler_local_thumbprint.dat", existingLocal.Thumbprint);

                            if (existingLocal.NotAfter <= DateTime.UtcNow.AddMonths(5))
                            {
                                TxtLocalStatus.Text = string.Format("Status: Local cert expiring soon ({0}), expires {1}.", existingLocal.Subject, DateFormatted(existingLocal.NotAfter));
                                BorderLocal.Background = WarningBackground;
                            }
                            else
                            {
                                TxtLocalStatus.Text = string.Format("Status: Found Local Data certificate in store ({0}), expires {1}.", existingLocal.Subject, DateFormatted(existingLocal.NotAfter));
                                BorderLocal.Background = SafeBackground;
                            }
                            existingLocal.Dispose();
                        }
                        else
                        {
                            TxtLocalStatus.Text = "Status: Local Data certificate not found in store.";
                            BorderLocal.Background = WarningBackground;

                            if (!dryRun)
                            {
                                int newLocalN = (targetMasterN > 0) ? targetMasterN : 0;
                                string localSubject = (newLocalN == 0) ? "CN=Ruler Local Data Signer" : string.Format("CN=Ruler Local Data Signer {0}", newLocalN);

                                foreach (var cert in localCerts)
                                {
                                    store.Remove(cert);
                                }

                                using (RSA rsaLocal = RSA.Create())
                                {
                                    rsaLocal.KeySize = 2048;
                                    var localReq = new CertificateRequest(localSubject, rsaLocal, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                                    localReq.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));

                                    using (X509Certificate2 localCert = localReq.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(5)))
                                    {
                                        store.Add(localCert);
                                        File.WriteAllText("ruler_local_thumbprint.dat", localCert.Thumbprint);

                                        TxtLocalStatus.Text = string.Format("Status: Generated and stored new Local certificate ({0}), expires {1}.", localSubject, DateFormatted(localCert.NotAfter));
                                        BorderLocal.Background = SafeBackground;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    TxtLocalStatus.Text = "Status: Skipped by user selection.";
                    BorderLocal.Background = SafeBackground;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error during setup processing: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string DateFormatted(DateTimeOffset date)
        {
            return date.ToString("MM-dd-yyyy");
        }
    }
}

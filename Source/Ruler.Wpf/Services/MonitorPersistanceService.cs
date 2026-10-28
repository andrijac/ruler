using Newtonsoft.Json;

using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Shared.Services;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Services
{
    public class MonitorPersistenceService : IMonitorPersistenceService
    {
        private const string FileName = "monitors.json";
        private const string SettingKey = "MonitorData"; // Legacy key from exe.config
        private const string CertSubject = "RulerLocalDataSigner";
        private readonly MonitorSerializer _settingsService;
        private readonly string _filePath;

        public MonitorPersistenceService(MonitorSerializer settings)
        {
            _settingsService = settings;
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);

            try
            {
                LocalCertificateService.GetOrInstallLocalCertificate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to ensure local signing certificate: {ex.Message}");
            }
        }

        public Dictionary<string, MonitorCalibrationProfile> LoadProfiles()
        {
            // If the new signed JSON file doesn't exist, check legacy exe.config
            if (!File.Exists(_filePath))
            {
                try
                {
                    var legacyJson = ConfigurationManager.AppSettings[SettingKey];
                    if (!string.IsNullOrWhiteSpace(legacyJson))
                    {
                        // Deserialize from legacy format
                        var migratedProfiles = _settingsService.DeserializeProfiles(legacyJson);

                        // Automatically save and sign into the new monitors.json file
                        SaveProfiles(migratedProfiles);
                        return migratedProfiles;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to migrate legacy monitor data: {ex.Message}");
                }

                return _settingsService.DeserializeProfiles(string.Empty);
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                var container = JsonConvert.DeserializeObject<SignedContainer>(json);

                if (container != null && !string.IsNullOrEmpty(container.Payload) && !string.IsNullOrEmpty(container.Signature))
                {
                    byte[] payloadBytes = Encoding.UTF8.GetBytes(container.Payload);
                    byte[] signatureBytes = Convert.FromBase64String(container.Signature);

                    if (CryptographicSigningService.VerifyData(payloadBytes, signatureBytes, CertSubject))
                    {
                        return _settingsService.DeserializeProfiles(container.Payload);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Warning: monitors.json cryptographic signature verification failed!");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load signed monitors.json: {ex.Message}");
            }

            return _settingsService.DeserializeProfiles(string.Empty);
        }

        public void SaveProfiles(Dictionary<string, MonitorCalibrationProfile> profiles)
        {
            try
            {
                string payloadJson = _settingsService.SerializeProfiles(profiles);

                byte[] payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
                byte[] signatureBytes = CryptographicSigningService.SignData(payloadBytes, CertSubject);

                var container = new SignedContainer
                {
                    Payload = payloadJson,
                    Signature = Convert.ToBase64String(signatureBytes)
                };

                string containerJson = JsonConvert.SerializeObject(container, Formatting.Indented);
                File.WriteAllText(_filePath, containerJson);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save and sign monitors.json: {ex.Message}");
            }
        }

        public void Reset()
        {
            if (File.Exists(_filePath))
            {
                try { File.Delete(_filePath); } catch { }
            }
        }
    }
}

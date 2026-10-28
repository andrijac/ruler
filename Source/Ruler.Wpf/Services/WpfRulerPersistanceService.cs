using Newtonsoft.Json;

using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Shared.Services;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Services
{
    public class WpfRulerPersistenceService : IRulerPersistanceService
    {
        private const string FileName = "rulers.json";
        private const string SettingKey = "RulerData"; // Legacy key from exe.config
        private const string CertSubject = "RulerLocalDataSigner";
        private readonly IRulerInfoPreprocessor _processor;
        private readonly IRulerSerializer _settingsService;
        private readonly IRulerRegistry _registry;
        private readonly string _filePath;

        public WpfRulerPersistenceService(
            IRulerInfoPreprocessor rulerInfoPreprocessor,
            IRulerSerializer settings,
            IRulerRegistry registry)
        {
            _processor = rulerInfoPreprocessor;
            _settingsService = settings;
            _registry = registry;
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

        public List<RulerInfo> LoadAll()
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
                        var migratedRulers = _settingsService.DeserializeRulers(legacyJson);

                        // Automatically save and sign into the new rulers.json file
                        SaveAll(migratedRulers);
                        return migratedRulers;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to migrate legacy ruler data: {ex.Message}");
                }

                return _settingsService.DeserializeRulers(string.Empty);
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
                        return _settingsService.DeserializeRulers(container.Payload);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Warning: rulers.json cryptographic signature verification failed!");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load signed rulers.json: {ex.Message}");
            }

            return _settingsService.DeserializeRulers(string.Empty);
        }

        public void SaveAll(IEnumerable<RulerInfo> rulers)
        {
            try
            {
                var rulersToSave = _processor.Preprocess(rulers);
                string payloadJson = _settingsService.SerializeRulers(rulersToSave);

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
                System.Diagnostics.Debug.WriteLine($"Failed to save and sign rulers.json: {ex.Message}");
            }
        }

        public void Update(RulerInfo info)
        {
            if (_registry.RulerExists(info.ID))
            {
                _registry.UpdateRulerByID(info);
            }
            SaveAll(_registry.GetActiveRulers().Select(r => r.RulerData).ToList());
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

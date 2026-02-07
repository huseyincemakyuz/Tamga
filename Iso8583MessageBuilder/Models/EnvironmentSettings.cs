using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Tamga.Models
{
    /// <summary>
    /// Ortam ayarlarını yöneten singleton sınıf
    /// </summary>
    public class EnvironmentSettings
    {
        #region Fields

        private const string SettingsFile = "environments.json";
        private static EnvironmentSettings _instance;

        public List<ServerEnvironment> Environments { get; set; }

        public static EnvironmentSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }

        #endregion

        #region Constructor
        private EnvironmentSettings()
        {
            Environments = new List<ServerEnvironment>();
            InitializeDefaults();
        }
        #endregion

        #region UI Initialization

        /// <summary>
        /// Varsayılan ortamları oluştur
        /// </summary>
        private void InitializeDefaults()
        {
            Environments.Add(new ServerEnvironment
            {
                Name = "Test",
                Host = "192.168.1.100",
                Port = 5000,
                TimeoutSeconds = 30,
                IsDefault = true,
                IsEnabled = true,
                Description = "Test environment"
            });

            Environments.Add(new ServerEnvironment
            {
                Name = "Debug",
                Host = "localhost",
                Port = 5001,
                TimeoutSeconds = 30,
                IsDefault = false,
                IsEnabled = true,
                Description = "Local debug server"
            });
        }

        #endregion

        /// <summary>
        /// Ayarları dosyadan yükle
        /// </summary>
        public static EnvironmentSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    var settings = JsonConvert.DeserializeObject<EnvironmentSettings>(json);

                    if (settings != null && settings.Environments != null && settings.Environments.Count > 0)
                    {
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading environments: {ex.Message}");
            }

            return new EnvironmentSettings();
        }

        /// <summary>
        /// Ayarları dosyaya kaydet
        /// </summary>
        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving environments: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Varsayılan ortamı getir
        /// </summary>
        public ServerEnvironment GetDefault()
        {
            return Environments.FirstOrDefault(e => e.IsDefault && e.IsEnabled)
                ?? Environments.FirstOrDefault(e => e.IsEnabled);
        }

        /// <summary>
        /// Aktif ortamları getir
        /// </summary>
        public List<ServerEnvironment> GetEnabled()
        {
            return Environments.Where(e => e.IsEnabled).ToList();
        }
    }
}
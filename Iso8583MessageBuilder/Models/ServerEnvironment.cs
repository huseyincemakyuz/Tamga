using System;

namespace Tamga.Models
{
    /// <summary>
    /// Ortam tanımı (Test, Debug, vb.)
    /// </summary>
    public class ServerEnvironment
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public int TimeoutSeconds { get; set; }
        public bool IsDefault { get; set; }
        public bool IsEnabled { get; set; }
        public string Description { get; set; }

        public ServerEnvironment()
        {
            Id = Guid.NewGuid().ToString();
            TimeoutSeconds = 30;
            IsDefault = false;
            IsEnabled = true;
            Description = "";
        }

        /// <summary>
        /// Dropdown'da görünecek text
        /// </summary>
        public string DisplayText => $"{Name} ({Host}:{Port})";

        public override string ToString() => DisplayText;
    }
}
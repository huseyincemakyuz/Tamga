using System;
using System.Collections.Generic;

namespace Tamga.Models
{
    public class SavedMessage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public string MTI { get; set; }
        public string HexMessage { get; set; }
        public Dictionary<int, string> Fields { get; set; }
        public List<string> Tags { get; set; }
        public string Notes { get; set; }
    }

    public class MessageStorage
    {
        public List<SavedMessage> Messages { get; set; } = new List<SavedMessage>();
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Tamga.Models
{
    public class MessageStorageManager
    {
        private static readonly string StorageFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Tamga",
                        "messages.json");

        public MessageStorageManager()
        {
            // Klasör yoksa oluştur
            var directory = Path.GetDirectoryName(StorageFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public List<SavedMessage> LoadMessages()
        {
            if (!File.Exists(StorageFilePath))
            {
                return new List<SavedMessage>();
            }

            try
            {
                var json = File.ReadAllText(StorageFilePath);
                var storage = JsonConvert.DeserializeObject<MessageStorage>(json);
                return storage?.Messages ?? new List<SavedMessage>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading messages: {ex.Message}");
            }
        }

        public void SaveMessages(List<SavedMessage> messages)
        {
            try
            {
                var storage = new MessageStorage { Messages = messages };
                var json = JsonConvert.SerializeObject(storage, Formatting.Indented);
                File.WriteAllText(StorageFilePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving messages: {ex.Message}");
            }
        }

        public void AddMessage(SavedMessage message)
        {
            var messages = LoadMessages();
            message.Id = Guid.NewGuid().ToString();
            message.Timestamp = DateTime.Now;
            messages.Insert(0, message); // En üste ekle
            SaveMessages(messages);
        }

        public void DeleteMessage(string id)
        {
            var messages = LoadMessages();
            messages.RemoveAll(m => m.Id == id);
            SaveMessages(messages);
        }

        public void UpdateMessage(SavedMessage message)
        {
            var messages = LoadMessages();
            var index = messages.FindIndex(m => m.Id == message.Id);
            if (index >= 0)
            {
                messages[index] = message;
                SaveMessages(messages);
            }
        }

        public List<SavedMessage> SearchMessages(string query)
        {
            var messages = LoadMessages();

            // Boş sorgu varsa tüm mesajları döndür
            if (string.IsNullOrWhiteSpace(query))
            {
                return messages;
            }

            // Arama yap
            return messages.Where(m =>
                (m.Name != null && m.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (m.MTI != null && m.MTI.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (m.Tags != null && m.Tags.Any(t => t != null && t.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
            ).ToList();
        }
    }
}
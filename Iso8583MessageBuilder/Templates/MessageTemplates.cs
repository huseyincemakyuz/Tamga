using System.Collections.Generic;

namespace Iso8583MessageBuilder.Templates
{
    public class MessageTemplate
    {
        public string MTI { get; set; }
        public string Name { get; set; }
        public List<int> RequiredFields { get; set; }
        public List<int> OptionalFields { get; set; }
    }

    public static class MessageTemplates
    {
        public static List<MessageTemplate> Templates = new List<MessageTemplate>
        {
            new MessageTemplate
            {
                MTI = "0200",
                Name = "Financial Transaction Request",
                RequiredFields = new List<int> { 2, 3, 4, 7, 11, 12, 13, 22, 41, 49 },
                OptionalFields = new List<int> { 14, 25, 37, 42, 43, 48, 55, 63 }
            },
            new MessageTemplate
            {
                MTI = "0210",
                Name = "Financial Transaction Response",
                RequiredFields = new List<int> { 2, 3, 4, 7, 11, 37, 39, 41 },
                OptionalFields = new List<int> { 38, 48, 63 }
            },
            new MessageTemplate
            {
                MTI = "0400",
                Name = "Reversal Request",
                RequiredFields = new List<int> { 2, 3, 4, 7, 11, 37, 41, 90 },
                OptionalFields = new List<int> { 48, 63 }
            },
            new MessageTemplate
            {
                MTI = "0800",
                Name = "Network Management Request",
                RequiredFields = new List<int> { 7, 11, 70 },
                OptionalFields = new List<int> { 48 }
            },
            new MessageTemplate
            {
                MTI = "0100",
                Name = "Authorization Request",
                RequiredFields = new List<int> { 2, 3, 4, 7, 11, 12, 13, 22, 41, 49 },
                OptionalFields = new List<int> { 14, 25, 37, 42, 48, 55 }
            }
        };
    }
}
using System;

namespace Tamga.Models
{
    /// <summary>
    /// Gateway'den gelen yanıt
    /// </summary>
    public class MessageResponse
    {
        public bool Success { get; set; }
        public string HexResponse { get; set; }
        public ParsedMessage ParsedResponse { get; set; }
        public string ErrorMessage { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public DateTime Timestamp { get; set; }

        public MessageResponse()
        {
            Timestamp = DateTime.Now;
            Success = false;
            HexResponse = "";
            ErrorMessage = "";
        }
    }
}
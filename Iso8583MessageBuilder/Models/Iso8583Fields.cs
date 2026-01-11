using System.Collections.Generic;

namespace Tamga.Models
{
    public static class Iso8583Fields
    {
        public static Dictionary<int, FieldDefinition> Fields = new Dictionary<int, FieldDefinition>
        {
            { 2, new FieldDefinition { FieldNumber = 2, Name = "PAN (Primary Account Number)", Type = FieldType.Numeric, LengthType = LengthType.LLVAR, MaxLength = 19 } },
            { 3, new FieldDefinition { FieldNumber = 3, Name = "Processing Code", Type = FieldType.Numeric, LengthType = LengthType.Fixed, MaxLength = 6 } },
            { 4, new FieldDefinition { FieldNumber = 4, Name = "Amount Transaction", Type = FieldType.Amount, LengthType = LengthType.Fixed, MaxLength = 12 } },
            { 7, new FieldDefinition { FieldNumber = 7, Name = "Transmission Date Time", Type = FieldType.Numeric, LengthType = LengthType.Fixed, MaxLength = 10 } },
            { 11, new FieldDefinition { FieldNumber = 11, Name = "STAN (System Trace Audit Number)", Type = FieldType.Numeric, LengthType = LengthType.Fixed, MaxLength = 6 } },
            { 12, new FieldDefinition { FieldNumber = 12, Name = "Local Time", Type = FieldType.Time, LengthType = LengthType.Fixed, MaxLength = 6 } },
            { 13, new FieldDefinition { FieldNumber = 13, Name = "Local Date", Type = FieldType.Date, LengthType = LengthType.Fixed, MaxLength = 4 } },
            { 14, new FieldDefinition { FieldNumber = 14, Name = "Expiration Date", Type = FieldType.Numeric, LengthType = LengthType.Fixed, MaxLength = 4 } },
            { 22, new FieldDefinition { FieldNumber = 22, Name = "POS Entry Mode", Type = FieldType.Numeric, LengthType = LengthType.Fixed, MaxLength = 3 } },
            { 25, new FieldDefinition { FieldNumber = 25, Name = "POS Condition Code", Type = FieldType.Numeric, LengthType = LengthType.Fixed, MaxLength = 2 } },
            { 32, new FieldDefinition { FieldNumber = 32, Name = "Acquiring Institution ID", Type = FieldType.Numeric, LengthType = LengthType.LLVAR, MaxLength = 11 } },
            { 37, new FieldDefinition { FieldNumber = 37, Name = "RRN (Retrieval Reference Number)", Type = FieldType.AlphaNumeric, LengthType = LengthType.Fixed, MaxLength = 12 } },
            { 38, new FieldDefinition { FieldNumber = 38, Name = "Approval Code", Type = FieldType.AlphaNumeric, LengthType = LengthType.Fixed, MaxLength = 6 } },
            { 39, new FieldDefinition { FieldNumber = 39, Name = "Response Code", Type = FieldType.AlphaNumeric, LengthType = LengthType.Fixed, MaxLength = 2 } },
            { 41, new FieldDefinition { FieldNumber = 41, Name = "Terminal ID", Type = FieldType.AlphaNumeric, LengthType = LengthType.Fixed, MaxLength = 8 } },
            { 42, new FieldDefinition { FieldNumber = 42, Name = "Merchant ID", Type = FieldType.AlphaNumeric, LengthType = LengthType.Fixed, MaxLength = 15 } },
            { 43, new FieldDefinition { FieldNumber = 43, Name = "Card Acceptor Name/Location", Type = FieldType.AlphaNumeric, LengthType = LengthType.Fixed, MaxLength = 40 } },
            { 48, new FieldDefinition { FieldNumber = 48, Name = "Additional Data", Type = FieldType.AlphaNumeric, LengthType = LengthType.LLLVAR, MaxLength = 999 } },
            { 49, new FieldDefinition { FieldNumber = 49, Name = "Currency Code", Type = FieldType.Numeric, LengthType = LengthType.Fixed, MaxLength = 3 } },
            { 52, new FieldDefinition { FieldNumber = 52, Name = "PIN Data", Type = FieldType.Binary, LengthType = LengthType.Fixed, MaxLength = 8 } },
            { 55, new FieldDefinition { FieldNumber = 55, Name = "EMV Data", Type = FieldType.Binary, LengthType = LengthType.LLLVAR, MaxLength = 999 } },            
            { 61, new FieldDefinition { FieldNumber = 61, Name = "Reserved Private", Type = FieldType.AlphaNumeric, LengthType = LengthType.LLLVAR, MaxLength = 999 } },
            { 62, new FieldDefinition { FieldNumber = 62, Name = "Reserved Private", Type = FieldType.AlphaNumeric, LengthType = LengthType.LLLVAR, MaxLength = 999 } },
            { 63, new FieldDefinition { FieldNumber = 63, Name = "Private Data", Type = FieldType.AlphaNumeric, LengthType = LengthType.LLLVAR, MaxLength = 999 } },
            { 70, new FieldDefinition { FieldNumber = 70, Name = "Network Management Code", Type = FieldType.Numeric, LengthType = LengthType.Fixed, MaxLength = 3 } },
            { 90, new FieldDefinition { FieldNumber = 90, Name = "Original Data Elements", Type = FieldType.Numeric, LengthType = LengthType.Fixed, MaxLength = 42 } },
            // F124 - Secondary bitmap için ekstra alan gerekiyor (64'ten büyük)
            { 124, new FieldDefinition { FieldNumber = 124, Name = "Info Text", Type = FieldType.AlphaNumeric, LengthType = LengthType.LLLVAR, MaxLength = 255 } },
        };
    }
}
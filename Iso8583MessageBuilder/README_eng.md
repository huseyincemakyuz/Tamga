# TAMGA - ISO 8583 Message Builder

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple)
![License](https://img.shields.io/badge/license-MIT-green)

A Windows desktop application for building, parsing, and managing ISO 8583 financial messages.

[🇹🇷 Türkçe README için tıklayın](README.tr.md)

---

## 🎯 What is ISO 8583?

ISO 8583 is the international standard for financial transaction card messages used in ATM, POS, and card payment systems. TAMGA helps you create and analyze these messages easily without manual hex calculations.

---

## ✨ Features

### 🔨 Build Tab
- **Create messages** from predefined templates (0200, 0400, 0800, etc.)
- **Add/remove fields** dynamically
- **Auto-generate values** for common fields (Date, Time, STAN, RRN)
- **Real-time hex output** with live preview
- **Color-coded display** for better readability
- **Save messages** with tags and notes

### 🔍 Parse Tab
- **Parse hex messages** into readable format
- **Field-by-field breakdown** with type information
- **Error detection** and validation warnings
- **Load to Build** - edit parsed messages
- **Save parsed messages** to history

### 📚 History Tab
- **View all saved messages** in a sortable table
- **Search and filter** by name, MTI, or tags
- **Load to Build/Parse** tabs for reuse
- **Delete** unwanted messages
- **Copy hex values** to clipboard

### 🎯 Advanced Features
- **Parse → Build workflow** - Parse a message and edit it
- **Auto-generate RRN** based on STAN (F11 → F37 dependency)
- **Template system** for quick message creation
- **JSON storage** - portable and human-readable
- **No installation required** - portable executable

---

## 🚀 Quick Start

### Download & Run
1. Download the latest release from [Releases](../../releases)
2. Extract the ZIP file
3. Run `Tamga.exe`
4. No installation required!

### System Requirements
- **OS**: Windows 7 or later
- **Framework**: .NET Framework 4.7.2 (usually pre-installed)
- **Disk Space**: ~5 MB

---

## 📖 Usage Guide

### Creating a Message

1. Open **Build** tab
2. Select message type from dropdown (e.g., "0200 - Authorization Request")
3. Required fields (marked with `*`) are automatically added
4. Fill in the required fields
5. Click **Add Field** to add optional fields
6. Use **Auto** buttons for automatic value generation:
   - **F7**: Transmission Date/Time (MMDDhhmmss)
   - **F11**: STAN - 6-digit random number
   - **F12**: Local Time (hhmmss)
   - **F13**: Local Date (MMDD)
   - **F37**: RRN (requires F11 to be filled first)
7. Click **Generate Message** to create hex output
8. Click **Save Message** to store for later use

**Example:**
```
Message Type: 0200 - Authorization Request
F2 (PAN): 4111111111111111
F3 (Processing Code): 000000
F4 (Amount): 000000010000 (100.00)
F11 (STAN): 123456 (Auto-generated)
F37 (RRN): 5023171234 (Auto-generated from F11)

Generated Hex:
0200B23A40010AA0000216411111111111111100000000000001000012345650231712345...
```

---

### Parsing a Message

1. Open **Parse** tab
2. Paste hex message into the input box
3. Click **Parse & Decode Message**
4. View the decoded fields with:
   - MTI (Message Type Indicator)
   - Primary/Secondary bitmaps
   - Field-by-field breakdown
5. **(Optional)** Click **Load to Build** to edit the parsed message
6. **(Optional)** Click **Save** to store in history

**Example Input:**
```
0200B23A40010AA0000216411111111111111100000000000001000012345650231712345...
```

**Example Output:**
```
MTI: 0200
Message Type: Authorization Request

Bitmaps:
  Primary: B23A40010AA00002

Fields: (5 fields present)
──────────────────────────────────
F002 - Primary Account Number (PAN)
     Type: n, LLVAR
     Value: 4111111111111111

F003 - Processing Code
     Type: n 6, Fixed
     Value: 000000

F004 - Amount, Transaction
     Type: n 12, Fixed
     Value: 000000010000

F011 - System Trace Audit Number (STAN)
     Type: n 6, Fixed
     Value: 123456

F037 - Retrieval Reference Number (RRN)
     Type: an 12, Fixed
     Value: 5023171234

✓ The message has been successfully parsed!
```

---

### Managing History

1. Open **History** tab
2. View all saved messages in the table
3. Use the search box to filter by name, MTI, or tags
4. Click a message to view details in the preview panel
5. Actions:
   - **Load to Build**: Open message in Build tab for editing
   - **Load to Parse**: Open message in Parse tab
   - **Delete**: Remove message from history
   - **Copy Hex**: Copy hex value to clipboard

---

## 🎓 ISO 8583 Field Reference

### Common Fields

| Field | Name | Type | Length | Example |
|-------|------|------|--------|---------|
| **F2** | PAN (Card Number) | LLVAR | max 19 | 4111111111111111 |
| **F3** | Processing Code | Fixed | 6 | 000000 |
| **F4** | Amount | Fixed | 12 | 000000010000 |
| **F7** | Transmission Date/Time | Fixed | 10 | 0129173045 |
| **F11** | STAN | Fixed | 6 | 123456 |
| **F12** | Local Time | Fixed | 6 | 173045 |
| **F13** | Local Date | Fixed | 4 | 0129 |
| **F37** | RRN | Fixed | 12 | 5023171234 |
| **F39** | Response Code | Fixed | 2 | 00 |
| **F41** | Terminal ID | Fixed | 8 | TERM0001 |
| **F42** | Merchant ID | Fixed | 15 | MERCHANT0000001 |

### Message Types (MTI)

| MTI | Description |
|-----|-------------|
| **0200** | Authorization Request |
| **0210** | Authorization Response |
| **0400** | Reversal Request |
| **0410** | Reversal Response |
| **0800** | Network Management Request |
| **0810** | Network Management Response |

---

## 🛠️ Technical Details

### Technology Stack
- **Language**: C# 7.3
- **Framework**: .NET Framework 4.7.2
- **UI**: Windows Forms
- **Storage**: JSON (Newtonsoft.Json 13.0.4)
- **Architecture**: Event-Driven, Tab Manager Pattern

### Project Structure
```
Tamga/
├── Forms/
│   ├── MainForm.cs                  # Main window (TabControl host)
│   ├── ComboBoxItem.cs              # Helper class
│   ├── AddFieldDialog.cs            # Field selection dialog
│   ├── SaveMessageDialog.cs         # Save with tags/notes
│   └── Tabs/
│       ├── BuildTabManager.cs       # Message creation logic
│       ├── ParseTabManager.cs       # Message parsing logic
│       └── HistoryTabManager.cs     # Storage management
├── Models/
│   ├── Iso8583MessageBuilder.cs     # Core message builder
│   ├── Iso8583MessageParser.cs      # Core message parser
│   ├── MessageStorageManager.cs     # JSON persistence
│   ├── FieldDefinition.cs           # Field metadata
│   ├── ParsedMessage.cs             # Parser results
│   ├── SavedMessage.cs              # Storage model
│   └── Iso8583Fields.cs             # Field definitions (F2-F128)
├── Controls/
│   └── FieldControl.cs              # Custom field input control
├── Templates/
│   └── MessageTemplates.cs          # MTI templates
└── Program.cs                       # Entry point
```

### Design Patterns
- **Tab Manager Pattern**: Each tab has a dedicated manager class
- **Event-Driven Architecture**: Loose coupling between components
- **Repository Pattern**: MessageStorageManager abstracts data access
- **Builder Pattern**: Step-by-step message construction

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

**TL;DR**: You can use this project for any purpose, including commercial use, as long as you include the original license.

# TAMGA - ISO 8583 Mesaj Oluşturucu

![Versiyon](https://img.shields.io/badge/versiyon-1.0.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple)
![Lisans](https://img.shields.io/badge/lisans-MIT-green)

ISO 8583 finansal mesajlarını oluşturmak, parse etmek ve yönetmek için Windows masaüstü uygulaması.

[🇬🇧 Click here for English README](https://github.com/huseyincemakyuz/Tamga/blob/master/README_eng.md)

![Ana Ekran](screenshots/Main_menu_1.png)

---

## 🎯 ISO 8583 Nedir?

ISO 8583, ATM, POS ve kart ödeme sistemlerinde kullanılan finansal işlem kartı mesajları için uluslararası standarttır. TAMGA, manuel hex hesaplamaları yapmadan bu mesajları kolayca oluşturmanıza ve analiz etmenize yardımcı olur.

---

## ✨ Özellikler

### 🔨 Build (Oluştur) Sekmesi
- Önceden tanımlanmış şablonlardan **mesaj oluşturma** (0200, 0400, 0800, vb.)
- **Dinamik alan ekleme/çıkarma**
- Yaygın alanlar için **otomatik değer oluşturma** (Tarih, Saat, STAN, RRN)
- Canlı önizleme ile **anlık hex çıktısı**
- Daha iyi okunabilirlik için **renkli görünüm**
- Etiket ve notlarla **mesaj kaydetme**

![Build Sekmesi](screenshots/mainmenu_buildmessagetab_2.png)

**Alan Ekleme:**

![Alan Ekleme Dialog](screenshots/AlanEkleme.png)

### 🔍 Parse (Çözümle) Sekmesi
- **Hex mesajları** okunabilir formata dönüştürme
- Tip bilgisi ile **alan bazında detay**
- **Hata tespiti** ve doğrulama uyarıları
- **Build'e Yükle** - parse edilmiş mesajları düzenleme
- **Parse edilmiş mesajları** geçmişe kaydetme

![Parse Sekmesi](screenshots/parsetab_3.png)

### 📚 History (Geçmiş) Sekmesi
- Sıralanabilir tabloda **tüm kaydedilmiş mesajları görüntüleme**
- İsim, MTI veya etiketlere göre **arama ve filtreleme**
- Yeniden kullanım için **Build/Parse sekmelerine yükleme**
- İstenmeyen mesajları **silme**
- **Hex değerlerini** panoya kopyalama

![History Sekmesi - Tek Mesaj](screenshots/historytab_4.png)

![History Sekmesi - Çoklu Mesaj](screenshots/historytab_5.png)

### 🌐 TCP/IP Entegrasyonu ⭐ YENİ!
- **Çoklu ortam yönetimi** - Test, Debug, Production ortamları
- **Gateway'lere mesaj gönderme** - Gerçek zamanlı test
- **Otomatik yanıt alma ve parse etme**
- **Ortam bazlı ayarlar** - Host, Port, Timeout
- **Kolay geçiş** - Dropdown ile ortam değiştirme
- **Klavye kısayolu** - `Ctrl+Enter` ile hızlı gönderim

![Environment Settings](screenshots/environmentsettings.png)

### 🎯 Gelişmiş Özellikler
- **Parse → Build iş akışı** - Bir mesajı parse edin ve düzenleyin
- STAN'a dayalı **otomatik RRN oluşturma** (F11 → F37 bağımlılığı)
- Hızlı mesaj oluşturma için **şablon sistemi**
- **JSON depolama** - taşınabilir ve okunabilir
- **Kurulum gerektirmez** - taşınabilir çalıştırılabilir dosya

---

## 🚀 Hızlı Başlangıç

### İndirme & Çalıştırma
1. [Releases](../../releases) sayfasından en son sürümü indirin
2. ZIP dosyasını çıkartın
3. `Tamga.exe` dosyasını çalıştırın
4. Kurulum gerektirmez!

### Sistem Gereksinimleri
- **İşletim Sistemi**: Windows 7 veya üzeri
- **Framework**: .NET Framework 4.7.2 (genellikle önceden yüklü)
- **Disk Alanı**: ~5 MB

---

## 📖 Kullanım Kılavuzu

### Mesaj Oluşturma

1. **Build** sekmesini açın
2. Açılır menüden mesaj tipini seçin (örn: "0200 - Authorization Request")
3. Gerekli alanlar (`*` ile işaretli) otomatik olarak eklenir
4. Gerekli alanları doldurun
5. İsteğe bağlı alanlar eklemek için **Add Field** tıklayın
6. Otomatik değer oluşturmak için **Auto** butonlarını kullanın:
   - **F7**: İletim Tarih/Saat (MMDDhhmmss)
   - **F11**: STAN - 6 haneli rastgele sayı
   - **F12**: Yerel Saat (hhmmss)
   - **F13**: Yerel Tarih (MMDD)
   - **F37**: RRN (önce F11'in doldurulması gerekir)
7. Hex çıktısı oluşturmak için **Generate Message** tıklayın
8. Daha sonra kullanmak üzere saklamak için **Save Message** tıklayın

**Örnek:**
```
Mesaj Tipi: 0200 - Authorization Request
F2 (PAN): 4111111111111111
F3 (Processing Code): 000000
F4 (Amount): 000000010000 (100.00)
F11 (STAN): 123456 (Otomatik oluşturuldu)
F37 (RRN): 5023171234 (F11'den otomatik oluşturuldu)

Oluşturulan Hex:
0200B23A40010AA0000216411111111111111100000000000001000012345650231712345...
```

---

### Mesaj Parse Etme

1. **Parse** sekmesini açın
2. Hex mesajı giriş kutusuna yapıştırın
3. **Parse & Decode Message** tıklayın
4. Çözümlenmiş alanları görüntüleyin:
   - MTI (Mesaj Tipi Göstergesi)
   - Birincil/İkincil bitmap'ler
   - Alan bazında detay
5. **(İsteğe bağlı)** Parse edilmiş mesajı düzenlemek için **Load to Build** tıklayın
6. **(İsteğe bağlı)** Geçmişe kaydetmek için **Save** tıklayın

**Örnek Girdi:**
```
0200B23A40010AA0000216411111111111111100000000000001000012345650231712345...
```

**Örnek Çıktı:**
```
MTI: 0200
Mesaj Tipi: Authorization Request

Bitmap'ler:
  Birincil: B23A40010AA00002

Alanlar: (5 alan mevcut)
──────────────────────────────────
F002 - Primary Account Number (PAN)
     Tip: n, LLVAR
     Değer: 4111111111111111

F003 - Processing Code
     Tip: n 6, Fixed
     Değer: 000000

F004 - Amount, Transaction
     Tip: n 12, Fixed
     Değer: 000000010000

F011 - System Trace Audit Number (STAN)
     Tip: n 6, Fixed
     Değer: 123456

F037 - Retrieval Reference Number (RRN)
     Tip: an 12, Fixed
     Değer: 5023171234

✓ Mesaj başarıyla parse edildi!
```

---

### TCP/IP ile Mesaj Gönderme ⭐ YENİ!

#### Ortam Ayarlarını Yapılandırma

1. Toolbar'daki **⚙️ Ayarlar** butonuna tıklayın
2. **➕ Add New** ile yeni ortam ekleyin
3. Ortam bilgilerini girin:
   - **Name**: Ortam adı (örn: Test, Production)
   - **Host**: Gateway IP adresi veya hostname
   - **Port**: Gateway port numarası
   - **Timeout**: Yanıt bekleme süresi (saniye cinsinden)
   - **Description**: Ortam açıklaması (opsiyonel)
4. **Default** checkbox'ı ile varsayılan ortamı seçin
5. **💾 Save** ile kaydedin

![Ortam Ayarları Dialog](screenshots/environmentsettings.png)

#### Mesaj Gönderme

1. Toolbar'daki **Environment** dropdown'dan hedef ortamı seçin
2. İstediğiniz sekmede mesajınızı hazırlayın:
   - **Build**: Yeni mesaj oluştur
   - **Parse**: Mevcut hex mesajı kullan
   - **History**: Kaydedilmiş mesajı seç
3. **📤 Send** butonuna tıklayın (veya `Ctrl+Enter` kısayolu)
4. Gönderilen mesaj ve alınan yanıt otomatik gösterilir
5. Yanıt başarılı ise otomatik parse edilir

**Not:** TCP/IP entegrasyonu, her gateway'in kendine özgü mesaj formatı olabileceğinden, kodda `MessageSender.cs` dosyasındaki TODO bölümlerini kendi gateway formatınıza göre düzenlemeniz gerekebilir.

---

### Geçmiş Yönetimi

1. **History** sekmesini açın
2. Tablodaki tüm kayıtlı mesajları görüntüleyin
3. İsim, MTI veya etiketlere göre filtrelemek için arama kutusunu kullanın
4. Önizleme panelinde detayları görmek için bir mesaja tıklayın
5. İşlemler:
   - **Load to Build**: Mesajı düzenlemek için Build sekmesinde aç
   - **Load to Parse**: Mesajı Parse sekmesinde aç
   - **Delete**: Mesajı geçmişten kaldır
   - **Copy Hex**: Hex değerini panoya kopyala

---

## ⚙️ Yapılandırma Dosyaları

### Ortam Ayarları
Ortam ayarları otomatik olarak şu konumda saklanır:
```
C:\Users\{KullanıcıAdı}\AppData\Roaming\Tamga\environments.json
```

**Örnek `environments.json`:**
```json
{
  "Environments": [
    {
      "Id": "1",
      "Name": "Test",
      "Host": "192.168.1.100",
      "Port": 5000,
      "TimeoutSeconds": 30,
      "IsDefault": true,
      "IsEnabled": true,
      "Description": "Test ortamı"
    },
    {
      "Id": "2",
      "Name": "Production",
      "Host": "10.0.0.50",
      "Port": 6000,
      "TimeoutSeconds": 60,
      "IsDefault": false,
      "IsEnabled": true,
      "Description": "Canlı ortam"
    }
  ]
}
```

### Mesaj Geçmişi
Kaydedilen mesajlar şu konumda saklanır:
```
C:\Users\{KullanıcıAdı}\AppData\Roaming\Tamga\messages.json
```

---

## 🎓 ISO 8583 Alan Referansı

### Yaygın Alanlar

| Alan | İsim | Tip | Uzunluk | Örnek |
|------|------|-----|---------|-------|
| **F2** | PAN (Kart Numarası) | LLVAR | maks 19 | 4111111111111111 |
| **F3** | İşlem Kodu | Sabit | 6 | 000000 |
| **F4** | Tutar | Sabit | 12 | 000000010000 |
| **F7** | İletim Tarih/Saat | Sabit | 10 | 0129173045 |
| **F11** | STAN | Sabit | 6 | 123456 |
| **F12** | Yerel Saat | Sabit | 6 | 173045 |
| **F13** | Yerel Tarih | Sabit | 4 | 0129 |
| **F37** | RRN | Sabit | 12 | 5023171234 |
| **F39** | Yanıt Kodu | Sabit | 2 | 00 |
| **F41** | Terminal ID | Sabit | 8 | TERM0001 |
| **F42** | Üye İşyeri ID | Sabit | 15 | MERCHANT0000001 |

### Mesaj Tipleri (MTI)

| MTI | Açıklama |
|-----|----------|
| **0200** | Yetkilendirme İsteği |
| **0210** | Yetkilendirme Yanıtı |
| **0400** | Geri Alma İsteği |
| **0410** | Geri Alma Yanıtı |
| **0800** | Ağ Yönetimi İsteği |
| **0810** | Ağ Yönetimi Yanıtı |

---

## 🛠️ Teknik Detaylar

### Teknoloji Yığını
- **Dil**: C# 7.3
- **Framework**: .NET Framework 4.7.2
- **Arayüz**: Windows Forms
- **Depolama**: JSON (Newtonsoft.Json 13.0.4)
- **Network**: System.Net.Sockets (TCP/IP)
- **Mimari**: Olay Güdümlü, Tab Manager Deseni

### Proje Yapısı
```
Tamga/
├── Forms/
│   ├── MainForm.cs                  # Ana pencere (TabControl + Toolbar)
│   ├── ComboBoxItem.cs              # Yardımcı sınıf
│   ├── AddFieldDialog.cs            # Alan seçim diyalogu
│   ├── SaveMessageDialog.cs         # Etiket/not ile kaydetme
│   ├── Dialogs/
│   │   └── EnvironmentSettingsDialog.cs  # Ortam yönetimi
│   └── Tabs/
│       ├── BuildTabManager.cs       # Mesaj oluşturma mantığı
│       ├── ParseTabManager.cs       # Mesaj parse etme mantığı
│       └── HistoryTabManager.cs     # Depolama yönetimi
├── Models/
│   ├── Iso8583MessageBuilder.cs     # Çekirdek mesaj oluşturucu
│   ├── Iso8583MessageParser.cs      # Çekirdek mesaj parser
│   ├── MessageStorageManager.cs     # JSON kalıcılığı
│   ├── MessageSender.cs             # TCP/IP gönderici
│   ├── MessageSenderHelper.cs       # UI entegrasyonu
│   ├── ServerEnvironment.cs         # Ortam modeli
│   ├── EnvironmentSettings.cs       # Ortam ayarları yönetimi
│   ├── MessageResponse.cs           # Yanıt modeli
│   ├── FieldDefinition.cs           # Alan metaverileri
│   ├── ParsedMessage.cs             # Parser sonuçları
│   ├── SavedMessage.cs              # Depolama modeli
│   └── Iso8583Fields.cs             # Alan tanımları (F2-F128)
├── Controls/
│   └── FieldControl.cs              # Özel alan girdi kontrolü
├── Templates/
│   └── MessageTemplates.cs          # MTI şablonları
└── Program.cs                       # Giriş noktası
```

### Tasarım Desenleri
- **Tab Manager Deseni**: Her sekmenin özel bir yönetici sınıfı var
- **Olay Güdümlü Mimari**: Bileşenler arası gevşek bağlantı
- **Repository Deseni**: MessageStorageManager veri erişimini soyutlar
- **Builder Deseni**: Adım adım mesaj oluşturma
- **Singleton Deseni**: EnvironmentSettings global ayar yönetimi

---

## 🔧 Geliştirici Notları

### TCP/IP Entegrasyonu Özelleştirme

`MessageSender.cs` dosyasında, gateway'inizin beklediği formata göre mesaj gönderme ve alma kodlarını özelleştirebilirsiniz:
```csharp
// TODO bölümlerini kendi formatınıza göre düzenleyin:

// Örnek Format 1: ASCII Length + Binary Message
string length = binaryMessage.Length.ToString();
await stream.WriteAsync(Encoding.ASCII.GetBytes(length));
await stream.WriteAsync(binaryMessage);

// Örnek Format 2: Binary Length (2-byte) + Binary Message
byte[] lengthBytes = BitConverter.GetBytes((short)binaryMessage.Length);
if (BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);
await stream.WriteAsync(lengthBytes);
await stream.WriteAsync(binaryMessage);
```

Yaygın gateway formatları:
- **ASCII Length + Binary Message**: `"19"` + `[0x30, 0x32, ...]`
- **Binary 2-byte Length**: `[0x00, 0x13]` + `[0x30, 0x32, ...]`
- **Binary 4-byte Length**: `[0x00, 0x00, 0x00, 0x13]` + `[0x30, ...]`

---

## 📄 Lisans

Bu proje MIT Lisansı altında lisanslanmıştır - detaylar için [LICENSE](LICENSE) dosyasına bakın.

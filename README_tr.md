# TAMGA - ISO 8583 Mesaj Oluşturucu

![Versiyon](https://img.shields.io/badge/versiyon-1.0.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple)
![Lisans](https://img.shields.io/badge/lisans-MIT-green)

ISO 8583 finansal mesajlarını oluşturmak, parse etmek ve yönetmek için Windows masaüstü uygulaması.

[🇬🇧 Click here for English README]((https://github.com/huseyincemakyuz/Tamga/blob/master/README_eng.md))

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

### 🔍 Parse (Çözümle) Sekmesi
- **Hex mesajları** okunabilir formata dönüştürme
- Tip bilgisi ile **alan bazında detay**
- **Hata tespiti** ve doğrulama uyarıları
- **Build'e Yükle** - parse edilmiş mesajları düzenleme
- **Parse edilmiş mesajları** geçmişe kaydetme

### 📚 History (Geçmiş) Sekmesi
- Sıralanabilir tabloda **tüm kaydedilmiş mesajları görüntüleme**
- İsim, MTI veya etiketlere göre **arama ve filtreleme**
- Yeniden kullanım için **Build/Parse sekmelerine yükleme**
- İstenmeyen mesajları **silme**
- **Hex değerlerini** panoya kopyalama

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
- **Mimari**: Olay Güdümlü, Tab Manager Deseni

### Proje Yapısı
```
Tamga/
├── Forms/
│   ├── MainForm.cs                  # Ana pencere (TabControl host)
│   ├── ComboBoxItem.cs              # Yardımcı sınıf
│   ├── AddFieldDialog.cs            # Alan seçim diyalogu
│   ├── SaveMessageDialog.cs         # Etiket/not ile kaydetme
│   └── Tabs/
│       ├── BuildTabManager.cs       # Mesaj oluşturma mantığı
│       ├── ParseTabManager.cs       # Mesaj parse etme mantığı
│       └── HistoryTabManager.cs     # Depolama yönetimi
├── Models/
│   ├── Iso8583MessageBuilder.cs     # Çekirdek mesaj oluşturucu
│   ├── Iso8583MessageParser.cs      # Çekirdek mesaj parser
│   ├── MessageStorageManager.cs     # JSON kalıcılığı
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

---
## 📄 Lisans

Bu proje MIT Lisansı altında lisanslanmıştır - detaylar için [LICENSE](LICENSE) dosyasına bakın.


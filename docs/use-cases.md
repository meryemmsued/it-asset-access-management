# Use Cases

## 1. Amaç

Bu doküman, IT Asset & Access Management uygulamasında kullanıcıların sistem üzerinde gerçekleştirebileceği temel işlemleri tanımlar.

Sistem; fiziksel ve dijital IT varlıklarının yönetilmesini, çalışanlara varlık zimmetlenmesini, dijital kaynaklara erişim taleplerinin oluşturulmasını ve bu taleplerin onay süreçlerinden geçirilmesini sağlar.

---

## 2. Aktörler

### Admin

Sistemin genel yönetiminden sorumludur.

Yetkileri:

* Kullanıcı oluşturma
* Kullanıcı güncelleme
* Kullanıcı pasife alma
* Rol oluşturma
* Kullanıcıya rol atama
* Rol izinlerini düzenleme
* Departman ve takım yönetimi
* Tüm varlıkları görüntüleme
* Audit log kayıtlarını görüntüleme
* Güvenlik olaylarını görüntüleme

### IT Specialist

Fiziksel ve dijital IT varlıklarının yönetiminden sorumludur.

Yetkileri:

* Asset oluşturma
* Asset bilgilerini güncelleme
* Asset durumunu değiştirme
* Fiziksel asset zimmetleme
* Zimmet iade işlemi yapma
* Asset kullanım geçmişini görüntüleme
* Dijital erişim verme
* Dijital erişimi iptal etme
* Lisans ve garanti tarihlerini takip etme

### Team Lead

Kendi ekibindeki çalışanların erişim taleplerini değerlendirir.

Yetkileri:

* Ekibindeki kullanıcıları görüntüleme
* Bekleyen erişim taleplerini görüntüleme
* Erişim talebini onaylama
* Erişim talebini reddetme
* Talebe açıklama ekleme
* Ekibindeki kullanıcıların aktif erişimlerini görüntüleme

### Developer

Yazılım geliştirme kaynaklarına erişim ihtiyacı olan kullanıcıdır.

Yetkileri:

* Kendi bilgilerini görüntüleme
* Kendisine zimmetlenen assetleri görüntüleme
* GitHub repository erişimi talep etme
* Veritabanı erişimi talep etme
* Docker Registry erişimi talep etme
* API, VPN ve diğer dijital kaynaklara erişim talep etme
* Kendi erişim taleplerini görüntüleme
* Bekleyen erişim talebini iptal etme
* Kendi aktif erişimlerini görüntüleme

### Employee

Şirket çalışanını temsil eder.

Yetkileri:

* Kendi profilini görüntüleme
* Kendisine zimmetlenen cihazları görüntüleme
* Dijital asset için erişim talebi oluşturma
* Kendi taleplerini görüntüleme
* Bildirimleri görüntüleme

### Security Officer

Güvenlik olaylarını ve kritik sistem işlemlerini takip eder.

Yetkileri:

* Başarısız giriş denemelerini görüntüleme
* Yetkisiz erişim girişimlerini görüntüleme
* Şüpheli işlemleri görüntüleme
* Güvenlik olayını çözüldü olarak işaretleme
* Audit log kayıtlarını görüntüleme
* Kullanıcıların erişim geçmişlerini inceleme

### Auditor

Sistemde gerçekleşen işlemleri denetler.

Yetkileri:

* Audit log kayıtlarını görüntüleme
* Asset geçmişini görüntüleme
* Zimmet geçmişini görüntüleme
* Erişim taleplerini görüntüleme
* Kullanıcı rol değişikliklerini görüntüleme

Auditor sistem üzerinde veri değiştiremez.

---

# 3. Kullanıcı Yönetimi Use Case'leri

## UC-01: Kullanıcı oluşturma

**Aktör:** Admin

**Ön koşullar:**

* Admin sisteme giriş yapmış olmalıdır.
* Admin, kullanıcı oluşturma yetkisine sahip olmalıdır.

**Ana akış:**

1. Admin kullanıcı yönetimi ekranını açar.
2. Yeni kullanıcı oluştur seçeneğini seçer.
3. Ad, soyad, e-posta, departman, takım ve görev bilgilerini girer.
4. Kullanıcıya bir veya daha fazla rol atar.
5. Sistem girilen bilgileri doğrular.
6. Sistem kullanıcı kaydını oluşturur.
7. İşlem audit log olarak kaydedilir.

**Alternatif akış:**

* E-posta adresi daha önce kullanılmışsa sistem hata mesajı gösterir.
* Zorunlu alanlar eksikse kullanıcı oluşturulmaz.

**Sonuç:**

* Yeni kullanıcı sisteme eklenir.

---

## UC-02: Kullanıcı güncelleme

**Aktör:** Admin

**Ana akış:**

1. Admin kullanıcı listesini açar.
2. Güncellenecek kullanıcıyı seçer.
3. Kullanıcının bilgilerini değiştirir.
4. Sistem değişiklikleri kaydeder.
5. Eski ve yeni değerler audit log olarak kaydedilir.

---

## UC-03: Kullanıcıyı pasife alma

**Aktör:** Admin

**Ana akış:**

1. Admin kullanıcıyı seçer.
2. Kullanıcıyı pasife alma işlemini başlatır.
3. Sistem kullanıcı durumunu pasif olarak günceller.
4. Kullanıcının aktif oturumları sonlandırılır.
5. İşlem audit log olarak kaydedilir.

**Sonuç:**

* Kullanıcı sisteme giriş yapamaz.
* Kullanıcıya ait geçmiş kayıtlar silinmez.

---

## UC-04: Kullanıcıya rol atama

**Aktör:** Admin

**Ana akış:**

1. Admin kullanıcıyı seçer.
2. Kullanıcıya atanacak rolü seçer.
3. Sistem kullanıcının bu role zaten sahip olup olmadığını kontrol eder.
4. Rol kullanıcıya atanır.
5. İşlem audit log olarak kaydedilir.

---

## UC-05: Kullanıcıdan rol kaldırma

**Aktör:** Admin

**Ana akış:**

1. Admin kullanıcıyı seçer.
2. Kullanıcının mevcut rollerini görüntüler.
3. Kaldırılacak rolü seçer.
4. Sistem rolü kullanıcıdan kaldırır.
5. İşlem audit log olarak kaydedilir.

---

# 4. Rol ve İzin Yönetimi Use Case'leri

## UC-06: Rol oluşturma

**Aktör:** Admin

**Ana akış:**

1. Admin rol yönetimi ekranını açar.
2. Rol adı ve açıklamasını girer.
3. Role ait izinleri seçer.
4. Sistem rolü oluşturur.
5. İşlem audit log olarak kaydedilir.

---

## UC-07: Role izin atama

**Aktör:** Admin

**Ana akış:**

1. Admin bir rol seçer.
2. Sistemdeki izinleri görüntüler.
3. Role atanacak izinleri seçer.
4. Sistem rol ve izin ilişkilerini oluşturur.
5. İşlem audit log olarak kaydedilir.

---

## UC-08: Yetkisiz işlem girişiminin engellenmesi

**Aktör:** Sistemdeki herhangi bir kullanıcı

**Ana akış:**

1. Kullanıcı yetkisi olmayan bir işlemi gerçekleştirmeye çalışır.
2. Sistem kullanıcının rol ve izinlerini kontrol eder.
3. İşlem engellenir.
4. Kullanıcıya yetkisiz işlem mesajı gösterilir.
5. Girişim güvenlik olayı olarak kaydedilir.

---

# 5. Asset Yönetimi Use Case'leri

## UC-09: Asset oluşturma

**Aktör:** IT Specialist

**Ana akış:**

1. IT Specialist asset yönetimi ekranını açar.
2. Asset kategorisini seçer.
3. Asset adını, açıklamasını, türünü ve durumunu girer.
4. Fiziksel veya dijital asset detaylarını girer.
5. Sistem bilgileri doğrular.
6. Asset kaydı oluşturulur.
7. İşlem audit log olarak kaydedilir.

---

## UC-10: Fiziksel asset oluşturma

**Aktör:** IT Specialist

**Ana akış:**

1. IT Specialist fiziksel asset kategorisi seçer.
2. Marka, model, seri numarası ve envanter numarasını girer.
3. Satın alma ve garanti tarihlerini girer.
4. Asset başlangıç durumu `Available` olarak belirlenir.
5. Sistem asset kaydını oluşturur.

**Örnek assetler:**

* Laptop
* Masaüstü bilgisayar
* Monitör
* Telefon
* Tablet
* Fiziksel sunucu
* Ağ cihazı

---

## UC-11: Dijital asset oluşturma

**Aktör:** IT Specialist

**Ana akış:**

1. IT Specialist dijital asset kategorisi seçer.
2. Kaynak adı, URL, ortam ve açıklama bilgilerini girer.
3. Kaynağın hassas olup olmadığını belirtir.
4. Varsa sona erme tarihini girer.
5. Sistem dijital asset kaydını oluşturur.

**Örnek assetler:**

* GitHub repository
* PostgreSQL veritabanı
* Docker Registry
* API
* VPN
* Yazılım lisansı
* Cloud kaynağı
* Uygulama

---

## UC-12: Asset güncelleme

**Aktör:** IT Specialist

**Ana akış:**

1. IT Specialist asseti seçer.
2. Asset bilgilerini günceller.
3. Sistem değişiklikleri kaydeder.
4. Eski ve yeni değerler audit log olarak kaydedilir.

---

## UC-13: Asset durumunu değiştirme

**Aktör:** IT Specialist

**Ana akış:**

1. IT Specialist asseti seçer.
2. Yeni durumu belirler.
3. Sistem asset durumunu günceller.
4. Eski ve yeni durum asset durum geçmişine kaydedilir.

**Olası durumlar:**

* Available
* Assigned
* InMaintenance
* Lost
* Damaged
* Retired
* Expired

---

## UC-14: Fiziksel asset zimmetleme

**Aktör:** IT Specialist

**Ön koşullar:**

* Asset fiziksel olmalıdır.
* Asset durumu `Available` olmalıdır.
* Kullanıcı aktif olmalıdır.

**Ana akış:**

1. IT Specialist zimmetlenecek asseti seçer.
2. Assetin verileceği kullanıcıyı seçer.
3. Zimmet tarihi ve varsa beklenen iade tarihini girer.
4. Sistem aktif bir zimmet kaydı oluşturur.
5. Asset durumu `Assigned` olarak güncellenir.
6. İşlem audit log olarak kaydedilir.
7. Kullanıcıya bildirim gönderilir.

---

## UC-15: Fiziksel asset iade alma

**Aktör:** IT Specialist

**Ana akış:**

1. IT Specialist aktif zimmet kaydını seçer.
2. Assetin iade durumunu ve notlarını girer.
3. Zimmet kaydına iade tarihi eklenir.
4. Zimmet durumu `Returned` olarak güncellenir.
5. Asset durumu `Available` olarak güncellenir.
6. İşlem audit log olarak kaydedilir.

**Alternatif akış:**

* Asset hasarlıysa durumu `Damaged` olarak güncellenir.
* Asset kayıpsa durumu `Lost` olarak güncellenir.

---

## UC-16: Kullanıcının kendisine zimmetlenen assetleri görüntülemesi

**Aktör:** Employee, Developer

**Ana akış:**

1. Kullanıcı sisteme giriş yapar.
2. Zimmetlerim ekranını açar.
3. Sistem kullanıcıya ait aktif zimmet kayıtlarını listeler.
4. Kullanıcı asset detaylarını görüntüler.

---

# 6. Erişim Talebi Use Case'leri

## UC-17: Erişim talebi oluşturma

**Aktör:** Employee, Developer

**Ön koşullar:**

* Kullanıcı aktif olmalıdır.
* Talep edilen asset dijital olmalıdır.
* Asset aktif durumda olmalıdır.

**Ana akış:**

1. Kullanıcı dijital asseti seçer.
2. Talep ettiği izin seviyesini belirtir.
3. Talep nedenini girer.
4. Erişim başlangıç ve bitiş tarihlerini girer.
5. Sistem talebi `Pending` durumunda oluşturur.
6. Gerekli onay adımları oluşturulur.
7. Onaylayıcılara bildirim gönderilir.
8. İşlem audit log olarak kaydedilir.

**Alternatif akış:**

* Kullanıcının aynı asset için aktif erişimi varsa sistem uyarı verir.
* Aynı asset için bekleyen talep varsa yeni talep engellenebilir.

---

## UC-18: Erişim talebini görüntüleme

**Aktör:** Employee, Developer, Team Lead, Admin

**Ana akış:**

1. Kullanıcı erişim talepleri ekranını açar.
2. Sistem kullanıcının yetkisine göre talepleri listeler.
3. Kullanıcı talep detaylarını görüntüler.

Employee ve Developer yalnızca kendi taleplerini görüntüler.

Team Lead yalnızca kendi ekibine ait talepleri görüntüler.

Admin tüm talepleri görüntüleyebilir.

---

## UC-19: Erişim talebini onaylama

**Aktör:** Team Lead, IT Specialist, Admin

**Ön koşullar:**

* Talep `Pending` durumunda olmalıdır.
* Kullanıcı ilgili onay adımında yetkili olmalıdır.

**Ana akış:**

1. Onaylayıcı bekleyen talebi açar.
2. Talep detaylarını inceler.
3. Onay seçeneğini seçer.
4. İsteğe bağlı açıklama girer.
5. Sistem onay kararını kaydeder.
6. Başka onay adımı varsa talep beklemede kalır.
7. Tüm onaylar tamamlandıysa talep `Approved` olur.
8. Kullanıcıya dijital asset erişimi tanımlanır.
9. Kullanıcıya bildirim gönderilir.
10. İşlem audit log olarak kaydedilir.

---

## UC-20: Erişim talebini reddetme

**Aktör:** Team Lead, IT Specialist, Admin

**Ana akış:**

1. Onaylayıcı talebi açar.
2. Reddet seçeneğini seçer.
3. Reddetme nedenini girer.
4. Talep `Rejected` olarak güncellenir.
5. Sonraki onay adımları iptal edilir.
6. Talep sahibine bildirim gönderilir.
7. İşlem audit log olarak kaydedilir.

---

## UC-21: Erişim talebini iptal etme

**Aktör:** Talebi oluşturan kullanıcı

**Ön koşullar:**

* Talep henüz sonuçlanmamış olmalıdır.

**Ana akış:**

1. Kullanıcı bekleyen talebi seçer.
2. Talebi iptal eder.
3. Sistem talebi `Cancelled` olarak günceller.
4. Bekleyen onay adımları iptal edilir.
5. İşlem audit log olarak kaydedilir.

---

## UC-22: Onaylanan erişimi aktifleştirme

**Aktör:** Sistem veya IT Specialist

**Ana akış:**

1. Tüm onay adımları tamamlanır.
2. Sistem bir asset erişim kaydı oluşturur.
3. Erişimin başlangıç ve bitiş tarihleri kaydedilir.
4. Erişim durumu `Active` olarak belirlenir.
5. Kullanıcıya bildirim gönderilir.
6. İşlem audit log olarak kaydedilir.

---

## UC-23: Erişimi manuel olarak iptal etme

**Aktör:** IT Specialist, Admin

**Ana akış:**

1. Yetkili kullanıcı aktif erişimi seçer.
2. İptal nedenini girer.
3. Erişim `Revoked` olarak güncellenir.
4. İptal tarihi ve işlemi yapan kullanıcı kaydedilir.
5. İlgili kullanıcıya bildirim gönderilir.
6. İşlem audit log olarak kaydedilir.

---

## UC-24: Süresi dolan erişimi otomatik kapatma

**Aktör:** Sistem

**Ana akış:**

1. Sistem belirli aralıklarla aktif erişimleri kontrol eder.
2. Bitiş tarihi geçmiş erişimleri tespit eder.
3. Erişim durumu `Expired` olarak güncellenir.
4. Kullanıcıya bildirim gönderilir.
5. İşlem audit log olarak kaydedilir.

---

# 7. Audit ve Güvenlik Use Case'leri

## UC-25: Audit log oluşturma

**Aktör:** Sistem

Aşağıdaki işlemler audit log olarak kaydedilir:

* Kullanıcı oluşturma
* Kullanıcı güncelleme
* Kullanıcı pasife alma
* Rol atama ve kaldırma
* Asset oluşturma
* Asset güncelleme
* Asset zimmetleme
* Asset iade alma
* Erişim talebi oluşturma
* Talep onaylama
* Talep reddetme
* Erişim verme
* Erişim iptal etme
* Güvenlik olayını çözme

Audit log kaydı şu bilgileri içerebilir:

* İşlemi yapan kullanıcı
* İşlem türü
* Etkilenen tablo veya nesne
* Etkilenen kayıt
* Eski değerler
* Yeni değerler
* IP adresi
* Tarih ve saat

---

## UC-26: Başarılı giriş kaydı oluşturma

**Aktör:** Sistem

**Ana akış:**

1. Kullanıcı doğru e-posta ve şifre girer.
2. Sistem kimlik bilgilerini doğrular.
3. Başarılı giriş kaydı oluşturulur.
4. Kullanıcının son giriş tarihi güncellenir.
5. JWT ve refresh token oluşturulur.

---

## UC-27: Başarısız giriş denemesini kaydetme

**Aktör:** Sistem

**Ana akış:**

1. Kullanıcı hatalı giriş bilgileri gönderir.
2. Sistem giriş isteğini reddeder.
3. Giriş denemesi başarısız olarak kaydedilir.
4. IP adresi ve hata nedeni kaydedilir.
5. Belirlenen sınır aşılmışsa güvenlik olayı oluşturulur.

---

## UC-28: Yetkisiz erişim girişimini kaydetme

**Aktör:** Sistem

**Ana akış:**

1. Kullanıcı yetkisi olmayan bir API endpointine erişmeye çalışır.
2. Sistem isteği reddeder.
3. Güvenlik olayı oluşturulur.
4. Kullanıcı, endpoint, IP adresi ve zaman bilgisi kaydedilir.

---

## UC-29: Güvenlik olayını görüntüleme

**Aktör:** Security Officer, Admin

**Ana akış:**

1. Kullanıcı güvenlik olayları ekranını açar.
2. Sistem olayları tarih, önem seviyesi ve durum bilgileriyle listeler.
3. Kullanıcı olay detaylarını görüntüler.

---

## UC-30: Güvenlik olayını çözüldü olarak işaretleme

**Aktör:** Security Officer, Admin

**Ana akış:**

1. Kullanıcı açık güvenlik olayını seçer.
2. Olayla ilgili inceleme notunu girer.
3. Olay çözüldü olarak işaretlenir.
4. Çözen kullanıcı ve çözüm tarihi kaydedilir.
5. İşlem audit log olarak kaydedilir.

---

# 8. Bildirim Use Case'leri

## UC-31: Kullanıcıya bildirim gönderme

**Aktör:** Sistem

Bildirim oluşturulabilecek durumlar:

* Yeni erişim talebi oluşturulması
* Talebin onaylanması
* Talebin reddedilmesi
* Erişim verilmesi
* Erişimin süresinin yaklaşması
* Erişimin sona ermesi
* Asset zimmetlenmesi
* Asset iadesi
* Garanti tarihinin yaklaşması
* Lisans bitiş tarihinin yaklaşması

---

## UC-32: Bildirimi okundu olarak işaretleme

**Aktör:** Sistemdeki kullanıcı

**Ana akış:**

1. Kullanıcı bildirimler ekranını açar.
2. Bir bildirimi seçer.
3. Sistem bildirimi okundu olarak işaretler.
4. Okunma tarihi kaydedilir.

---

# 9. Dashboard Use Case'leri

## UC-33: Dashboard görüntüleme

**Aktör:** Admin, IT Specialist, Team Lead, Security Officer

Dashboard kullanıcının rolüne göre farklı bilgiler gösterir.

### Admin Dashboard

* Aktif kullanıcı sayısı
* Toplam asset sayısı
* Bekleyen erişim talepleri
* Aktif erişim sayısı
* Son sistem işlemleri

### IT Dashboard

* Zimmetteki cihazlar
* Kullanılabilir cihazlar
* Bakımdaki cihazlar
* Yaklaşan garanti bitişleri
* Yaklaşan lisans bitişleri

### Team Lead Dashboard

* Ekibindeki kullanıcı sayısı
* Bekleyen ekip talepleri
* Ekibin aktif erişimleri
* Yaklaşan erişim bitişleri

### Security Dashboard

* Başarısız giriş sayısı
* Yetkisiz erişim girişimleri
* Açık güvenlik olayları
* Önem seviyesine göre güvenlik olayları
* Son kritik işlemler

---

# 10. Genel İş Kuralları

1. Bir kullanıcı birden fazla role sahip olabilir.
2. Bir rol birden fazla kullanıcıya atanabilir.
3. Bir rol birden fazla izne sahip olabilir.
4. Bir kullanıcı yalnızca yetkisi bulunan işlemleri gerçekleştirebilir.
5. Fiziksel assetler kullanıcılara zimmetlenebilir.
6. Dijital assetler için erişim talebi oluşturulabilir.
7. Aynı fiziksel asset aynı anda yalnızca bir kullanıcıya aktif olarak zimmetlenebilir.
8. Aynı kullanıcıya aynı dijital asset için birden fazla aktif erişim verilmemelidir.
9. Süresi dolan erişimler aktif olarak kullanılamaz.
10. Kritik işlemler audit log olarak kaydedilmelidir.
11. Audit log kayıtları kullanıcılar tarafından değiştirilememelidir.
12. Pasif kullanıcılar sisteme giriş yapamaz.
13. Silinen veya pasife alınan kullanıcıların geçmiş işlemleri korunmalıdır.
14. Gerçek API anahtarları, şifreler veya gizli bilgiler doğrudan veritabanında düz metin olarak tutulmamalıdır.
15. Kullanıcı şifreleri yalnızca hash biçiminde saklanmalıdır.

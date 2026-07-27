# Database Design

## 1. Amaç

Bu doküman, IT Asset & Access Management uygulamasının ilişkisel veritabanı tasarımını açıklar.

Veritabanı aşağıdaki temel ihtiyaçları karşılamak üzere tasarlanmıştır:

* Kullanıcı ve organizasyon yönetimi
* Rol ve izin yönetimi
* Fiziksel ve dijital asset yönetimi
* Fiziksel asset zimmetleme
* Dijital asset erişim yönetimi
* Çok adımlı erişim onay süreçleri
* Audit log kayıtları
* Güvenlik olayları
* Giriş denemeleri
* Bildirimler
* JWT refresh token yönetimi

Veritabanı yönetim sistemi olarak PostgreSQL kullanılacaktır.

---

# 2. Genel Tasarım İlkeleri

## 2.1 İlişkisel yapı

Veriler ilişkisel tablolar hâlinde tutulur ve tablolar arasındaki bağlantılar foreign key kullanılarak kurulur.

## 2.2 Normalizasyon

Veritabanı en az üçüncü normal forma uygun olacak şekilde tasarlanmıştır.

Amaç:

* Veri tekrarını azaltmak
* Güncelleme hatalarını önlemek
* İlişkileri açık şekilde göstermek
* Veri bütünlüğünü korumak

## 2.3 Silme yaklaşımı

Kullanıcı, asset ve rol gibi geçmiş kayıtlarla ilişkisi bulunan veriler mümkün olduğunca fiziksel olarak silinmez.

Bunun yerine:

* `is_active`
* `status`
* `revoked_at`
* `returned_at`

gibi alanlar kullanılır.

Bu yaklaşım audit ve geçmiş kayıtlarının korunmasını sağlar.

## 2.4 Tarih alanları

Temel tablolarda aşağıdaki alanlar kullanılabilir:

* `created_at`
* `updated_at`

İşleme bağlı olarak ayrıca şu alanlar bulunabilir:

* `assigned_at`
* `returned_at`
* `granted_at`
* `expires_at`
* `revoked_at`
* `decided_at`
* `resolved_at`

## 2.5 Hassas veriler

API anahtarı, parola, erişim anahtarı veya lisans anahtarı gibi hassas bilgiler doğrudan açık metin olarak saklanmamalıdır.

Veritabanında yalnızca:

* Hash değeri
* Şifreli değer
* Secret manager referansı

saklanmalıdır.

---

# 3. Tablo Grupları

Veritabanı tabloları beş ana grupta değerlendirilir:

1. Organizasyon ve kullanıcı yönetimi
2. Rol ve izin yönetimi
3. Asset yönetimi
4. Erişim yönetimi
5. Güvenlik ve audit

---

# 4. Organizasyon ve Kullanıcı Tabloları

## 4.1 departments

Şirket departmanlarını tutar.

Örnek departmanlar:

* IT
* Human Resources
* Finance
* Sales
* Operations

Temel alanlar:

* `id`
* `name`
* `description`
* `created_at`
* `updated_at`

İlişkiler:

* Bir departmanın birden fazla takımı olabilir.
* Bir departmanın birden fazla kullanıcısı olabilir.
* Bir asset bir departmana ait olabilir.

İlişki:

```text
departments 1 ---- * teams
departments 1 ---- * users
departments 1 ---- * assets
```

---

## 4.2 teams

Departmanların altında bulunan ekipleri tutar.

Örnek takımlar:

* Backend
* Frontend
* DevOps
* Cyber Security
* Mobile

Temel alanlar:

* `id`
* `department_id`
* `team_lead_user_id`
* `name`
* `description`
* `created_at`
* `updated_at`

İlişkiler:

* Her takım bir departmana bağlıdır.
* Bir takımın birden fazla kullanıcısı olabilir.
* Bir takımın bir team lead kullanıcısı olabilir.

```text
departments 1 ---- * teams
teams 1 ---- * users
users 1 ---- * teams
```

`team_lead_user_id`, `users.id` alanına foreign key olarak bağlanır.

---

## 4.3 users

Sistemdeki çalışanları ve yöneticileri tutar.

Temel alanlar:

* `id`
* `department_id`
* `team_id`
* `manager_id`
* `first_name`
* `last_name`
* `email`
* `password_hash`
* `job_title`
* `phone_number`
* `is_active`
* `last_login_at`
* `created_at`
* `updated_at`

### Manager ilişkisi

Bir kullanıcının yöneticisi başka bir kullanıcıdır.

Bu nedenle `manager_id`, yine `users.id` alanına bağlanır.

```text
users 1 ---- * users
```

Bu ilişkiye self-referencing ilişki denir.

### Rol ilişkisi

Kullanıcı tablosunda doğrudan `role` veya `role_id` tutulmaz.

Sebebi bir kullanıcının birden fazla role sahip olabilmesidir.

Kullanıcı ve rol ilişkisi `user_roles` ara tablosu üzerinden kurulur.

---

# 5. Rol ve İzin Tabloları

## 5.1 roles

Sistemdeki rolleri tutar.

Örnek roller:

* Admin
* IT Specialist
* Team Lead
* Developer
* Employee
* Security Officer
* Auditor

Temel alanlar:

* `id`
* `name`
* `description`
* `is_system_role`
* `created_at`

---

## 5.2 user_roles

Kullanıcılar ve roller arasındaki many-to-many ilişkiyi kurar.

Temel alanlar:

* `user_id`
* `role_id`
* `assigned_by_user_id`
* `assigned_at`

İlişki:

```text
users 1 ---- * user_roles
roles 1 ---- * user_roles
```

Kavramsal olarak:

```text
users * ---- * roles
```

İlişkisel veritabanında doğrudan many-to-many ilişki kurulamadığı için `user_roles` ara tablosu kullanılır.

### Primary key

`user_id` ve `role_id` birlikte composite primary key oluşturur.

```text
PRIMARY KEY (user_id, role_id)
```

Bu constraint aynı rolün aynı kullanıcıya iki kez atanmasını engeller.

---

## 5.3 permissions

Sistemde gerçekleştirilebilecek işlemleri tanımlar.

Örnek izinler:

* `User.View`
* `User.Create`
* `User.Update`
* `Role.Assign`
* `Asset.View`
* `Asset.Create`
* `Asset.Update`
* `Asset.Assign`
* `AccessRequest.Create`
* `AccessRequest.Approve`
* `AccessRequest.Reject`
* `AuditLog.View`
* `SecurityEvent.View`

Temel alanlar:

* `id`
* `name`
* `description`
* `created_at`

---

## 5.4 role_permissions

Roller ve izinler arasındaki many-to-many ilişkiyi kurar.

Temel alanlar:

* `role_id`
* `permission_id`
* `assigned_at`

İlişki:

```text
roles 1 ---- * role_permissions
permissions 1 ---- * role_permissions
```

Composite primary key:

```text
PRIMARY KEY (role_id, permission_id)
```

---

# 6. Asset Tabloları

## 6.1 asset_categories

Asset kategorilerini tutar.

Örnek kategoriler:

### Fiziksel

* Laptop
* Desktop Computer
* Monitor
* Mobile Phone
* Tablet
* Server
* Network Device

### Dijital

* GitHub Repository
* PostgreSQL Database
* Docker Registry
* API
* VPN
* Software License
* Cloud Resource
* Application

Temel alanlar:

* `id`
* `name`
* `asset_type`
* `description`
* `created_at`
* `updated_at`

`asset_type` alanı şu değerleri alabilir:

* `Physical`
* `Digital`

---

## 6.2 assets

Tüm fiziksel ve dijital assetlerin ortak bilgilerini tutar.

Temel alanlar:

* `id`
* `category_id`
* `owner_department_id`
* `created_by_user_id`
* `name`
* `description`
* `asset_type`
* `status`
* `is_active`
* `created_at`
* `updated_at`

İlişkiler:

```text
asset_categories 1 ---- * assets
departments 1 ---- * assets
users 1 ---- * assets
```

### Neden tek bir assets tablosu vardır?

Fiziksel ve dijital varlıkların ortak özellikleri vardır:

* Ad
* Açıklama
* Kategori
* Durum
* Sahip departman
* Oluşturan kullanıcı
* Oluşturulma tarihi

Bu ortak alanların tek tabloda tutulması veri tekrarını azaltır.

Fiziksel ve dijital assetlere özel bilgiler ayrı detay tablolarında tutulur.

---

## 6.3 physical_asset_details

Fiziksel assetlere özel bilgileri tutar.

Temel alanlar:

* `asset_id`
* `serial_number`
* `inventory_number`
* `brand`
* `model`
* `purchase_date`
* `purchase_price`
* `warranty_end_date`
* `location`
* `mac_address`
* `ip_address`

İlişki:

```text
assets 1 ---- 0..1 physical_asset_details
```

Bir asset fiziksel ise bir adet fiziksel detay kaydı bulunur.

`asset_id` hem primary key hem foreign key olarak kullanılır.

---

## 6.4 digital_asset_details

Dijital assetlere özel bilgileri tutar.

Temel alanlar:

* `asset_id`
* `resource_url`
* `host`
* `port`
* `environment`
* `secret_reference`
* `expiration_date`
* `is_sensitive`

İlişki:

```text
assets 1 ---- 0..1 digital_asset_details
```

Bir asset dijital ise bir adet dijital detay kaydı bulunur.

Gerçek API anahtarları ve parolalar bu tabloda açık metin olarak tutulmaz.

---

## 6.5 asset_assignments

Fiziksel assetlerin kullanıcılara zimmetlenmesini ve zimmet geçmişini tutar.

Temel alanlar:

* `id`
* `asset_id`
* `user_id`
* `assigned_by_user_id`
* `assigned_at`
* `expected_return_at`
* `returned_at`
* `status`
* `notes`

İlişkiler:

```text
assets 1 ---- * asset_assignments
users 1 ---- * asset_assignments
```

Bir asset zaman içerisinde farklı kullanıcılara zimmetlenebilir.

Fakat aynı anda yalnızca bir aktif zimmet kaydı bulunmalıdır.

Bu kontrol uygulama katmanında ve mümkünse PostgreSQL partial unique index ile sağlanabilir.

Örnek:

```sql
CREATE UNIQUE INDEX ux_active_asset_assignment
ON asset_assignments(asset_id)
WHERE status = 'Active';
```

---

## 6.6 asset_status_histories

Assetlerin durum değişikliklerini tutar.

Temel alanlar:

* `id`
* `asset_id`
* `changed_by_user_id`
* `old_status`
* `new_status`
* `description`
* `changed_at`

İlişki:

```text
assets 1 ---- * asset_status_histories
```

Örnek:

```text
Available → Assigned
Assigned → InMaintenance
InMaintenance → Available
Available → Retired
```

---

# 7. Erişim Yönetimi Tabloları

## 7.1 access_requests

Kullanıcıların dijital assetler için oluşturduğu erişim taleplerini tutar.

Temel alanlar:

* `id`
* `requester_user_id`
* `asset_id`
* `requested_permission`
* `reason`
* `requested_start_date`
* `requested_end_date`
* `status`
* `created_at`
* `completed_at`
* `cancelled_at`

İlişkiler:

```text
users 1 ---- * access_requests
assets 1 ---- * access_requests
```

Durumlar:

* Pending
* Approved
* Rejected
* Cancelled

Bir erişim talebi yalnızca dijital asset için oluşturulmalıdır.

Bu kural uygulama katmanında doğrulanır.

---

## 7.2 access_request_approvals

Erişim taleplerinin onay adımlarını tutar.

Temel alanlar:

* `id`
* `access_request_id`
* `approver_user_id`
* `approval_order`
* `decision`
* `comment`
* `decided_at`
* `created_at`

İlişkiler:

```text
access_requests 1 ---- * access_request_approvals
users 1 ---- * access_request_approvals
```

Bir erişim talebinin birden fazla onay adımı olabilir.

Örnek:

```text
1. Team Lead onayı
2. IT Specialist onayı
3. Security Officer onayı
```

`approval_order` alanı onay sırasını belirtir.

Aynı talepte aynı onay sırası bir kez bulunmalıdır.

```text
UNIQUE (access_request_id, approval_order)
```

---

## 7.3 asset_accesses

Onaylanan dijital erişimlerin gerçek durumunu tutar.

Temel alanlar:

* `id`
* `asset_id`
* `user_id`
* `access_request_id`
* `granted_by_user_id`
* `permission_level`
* `granted_at`
* `expires_at`
* `revoked_at`
* `revoked_by_user_id`
* `status`

İlişkiler:

```text
assets 1 ---- * asset_accesses
users 1 ---- * asset_accesses
access_requests 1 ---- 0..1 asset_accesses
```

Durumlar:

* Active
* Expired
* Revoked

### AccessRequests ve AssetAccesses farkı

`access_requests`, kullanıcının erişim istemesini ve talep sürecini tutar.

`asset_accesses`, kullanıcıya gerçekten verilmiş olan aktif veya geçmiş erişimleri tutar.

Örnek:

```text
AccessRequest:
Meryem PostgreSQL veritabanına Read erişimi istedi.

AssetAccess:
Meryem'e PostgreSQL veritabanı için Read erişimi verildi.
```

---

# 8. Audit ve Güvenlik Tabloları

## 8.1 audit_logs

Sistemde gerçekleşen kritik işlemleri tutar.

Temel alanlar:

* `id`
* `user_id`
* `action`
* `entity_name`
* `entity_id`
* `old_values`
* `new_values`
* `ip_address`
* `user_agent`
* `created_at`

`old_values` ve `new_values` PostgreSQL `jsonb` türünde tutulabilir.

Örnek kayıt:

```json
{
  "action": "AssetUpdated",
  "entity_name": "Asset",
  "entity_id": "42",
  "old_values": {
    "status": "Available"
  },
  "new_values": {
    "status": "Assigned"
  }
}
```

Audit log kayıtları normal kullanıcılar tarafından değiştirilememelidir.

---

## 8.2 security_events

Güvenlik açısından önemli olayları tutar.

Temel alanlar:

* `id`
* `user_id`
* `resolved_by_user_id`
* `event_type`
* `description`
* `severity`
* `ip_address`
* `is_resolved`
* `created_at`
* `resolved_at`

Örnek olay türleri:

* FailedLogin
* UnauthorizedAccess
* SuspiciousActivity
* ExpiredAccessAttempt
* MultipleFailedLogins

Önem seviyeleri:

* Low
* Medium
* High
* Critical

---

## 8.3 login_attempts

Başarılı ve başarısız giriş denemelerini tutar.

Temel alanlar:

* `id`
* `user_id`
* `email`
* `ip_address`
* `is_successful`
* `failure_reason`
* `attempted_at`

Başarısız giriş sırasında kullanıcı bulunamayabilir.

Bu nedenle `user_id` nullable olmalıdır.

---

## 8.4 refresh_tokens

JWT kimlik doğrulama sisteminde kullanılan refresh tokenları tutar.

Temel alanlar:

* `id`
* `user_id`
* `token_hash`
* `expires_at`
* `created_at`
* `revoked_at`
* `is_revoked`

Gerçek token değeri yerine token hash değeri saklanmalıdır.

İlişki:

```text
users 1 ---- * refresh_tokens
```

Bir kullanıcının farklı cihazlar veya oturumlar için birden fazla refresh tokenı olabilir.

---

## 8.5 notifications

Kullanıcı bildirimlerini tutar.

Temel alanlar:

* `id`
* `user_id`
* `title`
* `message`
* `notification_type`
* `is_read`
* `read_at`
* `created_at`

İlişki:

```text
users 1 ---- * notifications
```

---

# 9. Temel İlişki Özeti

```text
departments 1 ---- * teams
departments 1 ---- * users
teams 1 ---- * users

users 1 ---- * user_roles
roles 1 ---- * user_roles

roles 1 ---- * role_permissions
permissions 1 ---- * role_permissions

asset_categories 1 ---- * assets
departments 1 ---- * assets
users 1 ---- * assets

assets 1 ---- 0..1 physical_asset_details
assets 1 ---- 0..1 digital_asset_details

assets 1 ---- * asset_assignments
users 1 ---- * asset_assignments

assets 1 ---- * asset_status_histories

users 1 ---- * access_requests
assets 1 ---- * access_requests

access_requests 1 ---- * access_request_approvals
users 1 ---- * access_request_approvals

users 1 ---- * asset_accesses
assets 1 ---- * asset_accesses

users 1 ---- * audit_logs
users 1 ---- * security_events
users 1 ---- * login_attempts
users 1 ---- * refresh_tokens
users 1 ---- * notifications
```

---

# 10. Primary Key Kararları

Çoğu tabloda tekil `id` primary key kullanılır.

Örnek:

```text
users.id
roles.id
assets.id
access_requests.id
```

Ara tablolarda composite primary key kullanılır:

```text
user_roles:
PRIMARY KEY (user_id, role_id)

role_permissions:
PRIMARY KEY (role_id, permission_id)
```

Detay tablolarında asset kimliği primary key olarak kullanılır:

```text
physical_asset_details.asset_id
digital_asset_details.asset_id
```

---

# 11. Foreign Key Kararları

Foreign key kullanımı veri bütünlüğü sağlar.

Örnekler:

```text
users.department_id → departments.id
users.team_id → teams.id
users.manager_id → users.id

user_roles.user_id → users.id
user_roles.role_id → roles.id

assets.category_id → asset_categories.id
assets.owner_department_id → departments.id

asset_assignments.asset_id → assets.id
asset_assignments.user_id → users.id

access_requests.requester_user_id → users.id
access_requests.asset_id → assets.id
```

---

# 12. Unique Constraint Kararları

Aşağıdaki alanlar veya alan grupları unique olmalıdır:

```text
users.email
roles.name
permissions.name
departments.name
asset_categories.name

physical_asset_details.serial_number
physical_asset_details.inventory_number

user_roles(user_id, role_id)
role_permissions(role_id, permission_id)

access_request_approvals(
    access_request_id,
    approval_order
)
```

---

# 13. Index Kararları

Sık sorgulanacak foreign key ve filtre alanlarına index eklenmelidir.

Örnek indexler:

```text
users.email
users.department_id
users.team_id

assets.category_id
assets.status
assets.asset_type
assets.owner_department_id

asset_assignments.asset_id
asset_assignments.user_id
asset_assignments.status

access_requests.requester_user_id
access_requests.asset_id
access_requests.status
access_requests.created_at

asset_accesses.user_id
asset_accesses.asset_id
asset_accesses.status
asset_accesses.expires_at

audit_logs.user_id
audit_logs.entity_name
audit_logs.created_at

security_events.severity
security_events.is_resolved
security_events.created_at
```

Her alana index eklenmemelidir. Fazla index veri ekleme ve güncelleme işlemlerini yavaşlatabilir.

---

# 14. Transaction Kullanılacak Senaryolar

Birden fazla tabloyu etkileyen işlemler transaction içerisinde yapılmalıdır.

## Asset zimmetleme

Aynı transaction içerisinde:

1. `asset_assignments` kaydı oluşturulur.
2. `assets.status` değeri `Assigned` yapılır.
3. `asset_status_histories` kaydı oluşturulur.
4. `audit_logs` kaydı oluşturulur.
5. `notifications` kaydı oluşturulur.

Herhangi bir işlem başarısız olursa tüm işlemler geri alınır.

## Asset iade işlemi

Aynı transaction içerisinde:

1. Aktif zimmet kaydı güncellenir.
2. Asset durumu güncellenir.
3. Asset durum geçmişi oluşturulur.
4. Audit log oluşturulur.
5. Bildirim oluşturulur.

## Erişim talebi onaylama

Aynı transaction içerisinde:

1. Onay adımı güncellenir.
2. Son onaysa talep durumu `Approved` yapılır.
3. `asset_accesses` kaydı oluşturulur.
4. Audit log oluşturulur.
5. Bildirim oluşturulur.

---

# 15. View Önerileri

Dashboard ve raporlama işlemleri için PostgreSQL view kullanılabilir.

İlk sürümde zorunlu değildir.

Örnek viewler:

## active_asset_assignments_view

Aktif zimmetleri kullanıcı ve asset bilgileriyle birlikte gösterir.

## active_asset_accesses_view

Kullanıcıların aktif dijital erişimlerini gösterir.

## pending_access_requests_view

Bekleyen erişim taleplerini onaylayıcı bilgileriyle gösterir.

## expiring_assets_view

Garanti veya lisans bitiş tarihi yaklaşan assetleri gösterir.

---

# 16. Normalizasyon Kontrolü

## Birinci Normal Form

Her sütunda tek bir değer tutulur.

Yanlış örnek:

```text
roles = "Admin,IT,Auditor"
```

Doğru yaklaşım:

```text
user_roles
```

tablosunda her rol ayrı satır olarak tutulur.

## İkinci Normal Form

Ara tablolardaki bilgiler composite primary keyin tamamına bağlıdır.

Örneğin `user_roles` tablosundaki atama bilgileri hem kullanıcı hem rol ilişkisine aittir.

## Üçüncü Normal Form

Bir tabloda başka bir tabloya ait tekrar eden bilgiler tutulmaz.

Örneğin `users` tablosunda:

* DepartmentName
* TeamName
* RoleName

tutulmaz.

Bunların yerine:

* `department_id`
* `team_id`

ve ilişki tabloları kullanılır.

---

# 17. ASP.NET Core Identity Kararı

Authentication geliştirilirken ASP.NET Core Identity kullanılacaktır.

Identity kullanıldığında aşağıdaki tablolar hazır olarak oluşturulabilir:

* AspNetUsers
* AspNetRoles
* AspNetUserRoles
* AspNetUserClaims
* AspNetRoleClaims
* AspNetUserLogins
* AspNetUserTokens

Bu nedenle geliştirme aşamasında tasarladığımız:

* `users`
* `roles`
* `user_roles`

tabloları doğrudan Identity tablolarıyla karşılanabilir.

Ek kullanıcı alanları `ApplicationUser` sınıfına eklenebilir:

* FirstName
* LastName
* DepartmentId
* TeamId
* ManagerId
* JobTitle
* IsActive

İzin sistemi için Identity role claims veya özel `permissions` ve `role_permissions` tabloları kullanılabilir.

Bu karar backend mimarisi kurulurken kesinleştirilecektir.

---

# 18. Sonuç

Veritabanı modeli aşağıdaki temel ayrımlara göre tasarlanmıştır:

* Kullanıcılar ve roller many-to-many ilişkilidir.
* Roller ve izinler many-to-many ilişkilidir.
* Fiziksel ve dijital assetlerin ortak bilgileri `assets` tablosunda tutulur.
* Türlere özel bilgiler detay tablolarında tutulur.
* Fiziksel zimmetler `asset_assignments` tablosunda tutulur.
* Dijital erişimler `asset_accesses` tablosunda tutulur.
* Erişim talebi ve gerçek erişim birbirinden ayrılmıştır.
* Onay süreçleri ayrı tabloda tutulur.
* Kritik işlemler audit log olarak kaydedilir.
* Güvenlik olayları ayrı olarak takip edilir.
* Geçmiş kayıtlar mümkün olduğunca silinmeden korunur.

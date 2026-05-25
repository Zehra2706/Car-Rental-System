# Araç Kiralama Otomasyon Sistemi

Modern, güvenli ve katmanlı mimari prensipleriyle geliştirilmiş **ASP.NET Core MVC** tabanlı bir araç kiralama ve rezervasyon sistemidir.

---
##  Canlı Demo

Projeyi canlı olarak incelemek için aşağıdaki linki kullanabilirsiniz:

🔗 http://arackiralama-env.eba-qpyhhmb2.eu-north-1.elasticbeanstalk.com
# Proje Hakkında

---

Bu sistem kullanıcıların:

- Araçları görüntülemesini
- Filtreleme yapmasını
- Rezervasyon oluşturmasını
- Güvenli şekilde giriş yapmasını

sağlayan uçtan uca bir web uygulamasıdır.

Ayrıca admin paneli ile tüm sistem yönetilebilir.

---

#  Kimlik Doğrulama ve Güvenlik

Sistem hibrit bir güvenlik yapısı kullanır:

##  Cookie Authentication
- ASP.NET Core Cookie Authentication kullanılır
- Kullanıcı login olduğunda tarayıcıya güvenli cookie yazılır

##  Claim-Based Authorization
Kullanıcı bilgileri claim yapısında taşınır:
- UserId
- Email
- Role
- Name
- TC Kimlik No

##  Güvenlik Önlemleri
- Şifreler hashlenerek saklanır
- 3 hatalı giriş sonrası hesap kilitleme (Lockout)
- HttpOnly cookie ile XSS koruması
- HTTPS destekli Secure cookie

---

#  Session Kullanımı

- Session geçici veri saklamak için kullanılır
- Login durumu Cookie Authentication ile yönetilir
- “Beni Hatırla” sistemi 7 gün boyunca login tutar

---

# Veritabanı

- Microsoft SQL Server kullanılmıştır
- Entity Framework Core (Code First)
- Migration sistemi aktif

---

#  ER Diagram

Veritabanı ilişkileri aşağıda gösterilmiştir:

## ER Diagram

![ER Diagram](docs/ER.png)

---

#  Özellikler

| Modül | Açıklama |
|------|----------|
| Kullanıcı Sistemi | Kayıt, giriş, email doğrulama |
| Araç Yönetimi | Araç ekleme, silme, güncelleme |
| Rezervasyon Sistemi | Tarih bazlı araç kiralama |
| Admin Panel | Kullanıcı ve sistem yönetimi |
| Güvenlik | Lockout, hash şifreleme, claim auth |

---

#  Kullanılan Teknolojiler

- ASP.NET Core MVC (.NET 8/9)
- Entity Framework Core
- MS SQL Server
- Cookie Authentication
- Session Management
- HTML5, CSS3, JavaScript

---

# Kurulum

## 1. Projeyi indir

```bash
git clone https://github.com/melisa-6/Car-reservation-automation-system.git
cd Car-Rental-System

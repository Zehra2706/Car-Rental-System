# Araç Kiralama Otomasyon Sistemi (Car Reservation Automation System)

Bu proje, modern bir araç kiralama deneyimi sunan, güvenlik öncelikli ve katmanlı mimari prensiplerine uygun geliştirilmiş bir **ASP.NET Core MVC** uygulamasıdır.

---

## Proje Hakkında

Sistem; kullanıcıların araçları filtreleyip kiralayabildiği, yöneticilerin ise tüm filoyu ve kullanıcı güvenliğini kontrol edebildiği uçtan uca bir çözümdür. Projenin odak noktası, **yüksek güvenlik standartları** ve **dinamik kullanıcı deneyimi** (UX) sağlamaktır.

---

## Teknik Mimari ve Detaylar

### 1. Kimlik Doğrulama ve Yetkilendirme (Auth & Security)

Projenin kalbinde yer alan giriş sistemi, hibrit bir güvenlik yapısı kullanır:

- **JWT (JSON Web Token):** Kimlik doğrulama, tarayıcıda HttpOnly ve Secure flag'lerine sahip çerezler (cookies) içerisindeki JWT ile sağlanır. Bu, XSS saldırılarına karşı koruma sağlar.
- **Claim-Based Authorization:** Kullanıcının TC, Ehliyet No, Rol ve İsim gibi verileri Claim olarak taşınır; böylece her istekte veritabanına yük binmez.
- **Brute Force Koruması:** AccessFailedCount ve LockoutEnd parametreleri ile 5 hatalı denemede hesap 20 dakika süreyle kilitlenir.
- **Dinamik UI Kilitleme:** Hesap kilitlendiğinde giriş butonu (Client-Side) disabled duruma geçer. Kullanıcı farklı bir e-posta girdiğinde JavaScript ile buton tekrar aktifleşir.

### 2. Veri Yönetimi ve Kalıcılık

- **Entity Framework Core (ORM):** SQL Server ile iletişim için kullanılmıştır.
- **Code-First Yaklaşımı:** Veritabanı tabloları C# sınıfları (Entities) üzerinden yönetilmektedir.
- **Session Management:** JWT'ye ek olarak, uygulama içi hızlı veri erişimi için HttpContext.Session yapısı entegre edilmiştir.

---

## Sistem Özellikleri

| Modül               | Detaylar                                                   |
| :------------------ | :--------------------------------------------------------- |
| **Giriş Paneli**    | Brute force korumalı, JWT destekli güvenli giriş ekranı.   |
| **Araç Yönetimi**   | Araç ekleme, düzenleme, silme ve bakım durumu takibi.      |
| **Kiralama Akışı**  | Müsaitlik durumuna göre tarih bazlı rezervasyon oluşturma. |
| **Admin Dashboard** | Sistem istatistikleri ve kilitli hesapların yönetimi.      |

---

## Teknoloji Yığını (Tech Stack)

- **Backend:** ASP.NET Core 8.0/9.0 MVC
- **Veritabanı:** MS SQL Server
- **Güvenlik:** JWT, Anti-Forgery Tokens, Bcrypt (Password Hashing)
- **Frontend:** HTML5, CSS3, JavaScript

---

## Kurulum ve Yapılandırma

1.  **Projeyi Klonlayın:**

    ```bash
    git clone [https://github.com/melisa-6/Car-reservation-automation-system.git](https://github.com/melisa-6/Car-reservation-automation-system.git)
    cd Car-reservation-automation-system
    ```

2.  **Veritabanı Ayarları:**
    appsettings.json dosyasındaki bağlantı dizesini (ConnectionString) güncelleyin ve geçerli bir Jwt:Key (en az 32 karakter) belirleyin.

3.  **Migration Çalıştırın:**

    ```bash
    dotnet ef database update
    ```

4.  **Uygulamayı Başlatın:**
    ```bash
    dotnet run
    ```

---

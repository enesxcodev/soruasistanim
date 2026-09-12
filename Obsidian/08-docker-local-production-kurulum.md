# Docker — Local ve Production Kurulum Rehberi

> Bu not, SoruAsistanim'in Docker çalışma düzeni için kalıcı kurulum hafızasıdır.

## Mimari

```text
Tarayıcı → Traefik → frontend (Nginx) / admin (.NET) / api (.NET) / Seq
                                            api → SQL Server
                                            api → Seq (structured log)
```

- `soruasistanim.com`: React/Vite frontend.
- `admin.soruasistanim.com`: ASP.NET Core Admin.
- `api.soruasistanim.com`: ASP.NET Core API.
- `logs.soruasistanim.localhost`: Seq structured log web arayüzü (local).
- `jobs.soruasistanim.localhost/tickerq`: TickerQ background-job dashboard (local).
- SQL Server dış ağa açılmaz; yalnız Docker `backend` ağı üzerinden API erişir.

## Local Kurulum

### Gereksinimler

- Windows + WSL 2.
- Docker Desktop.
- Docker Engine/CLI ve Compose doğrulaması:

```powershell
docker --version
docker compose version
wsl --status
```

### Local alan adları

- `http://soruasistanim.localhost`
- `http://admin.soruasistanim.localhost`
- `http://api.soruasistanim.localhost`

`.localhost` otomatik olarak yerel makineye çözümlenir; hosts dosyası gerektirmez.

### Gizli local ayarlar

Proje kökünde Git'e girmeyen `.env` dosyası oluşturulur:

```env
SA_PASSWORD=<en az 8 karakter; büyük/küçük harf, rakam ve özel karakter>
JWT_KEY=<en az 32 karakterlik rastgele anahtar>
SEQ_ADMIN_PASSWORD=<Seq web arayüzü için güçlü yerel parola>
TICKERQ_USERNAME=<job dashboard kullanıcı adı>
TICKERQ_PASSWORD=<job dashboard güçlü parolası>
```

JWT anahtarı kullanıcıya verilen access token değildir. API'nin token üretmek ve doğrulamak için kullandığı sunucu sırrıdır. Yeni değer üretmek için:

```powershell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
```

### Başlatma ve kontrol

```powershell
docker compose config
docker compose up -d --build
docker compose ps
```

Canlı loglar:

```powershell
docker compose logs -f api
docker compose logs -f admin
docker compose logs -f traefik
docker compose logs -f seq
```

Yalnız değişen bir uygulamayı yeniden build etmek için:

```powershell
docker compose up -d --build api
docker compose up -d --build admin
docker compose up -d --build frontend
```

Container'ları durdurmak ve kaldırmak için:

```powershell
docker compose down
```

`down`, named volume'ları silmez; SQL Server verisi, yüklenen dosyalar ve loglar korunur. Veritabanını da sıfırlamak gerekirse ayrı ve yıkıcı komut kullanılır: `docker compose down -v`.

### Seq log arayüzü

Seq, Serilog'un yapılandırılmış event'lerini aramak ve filtrelemek için log sunucusudur. Local Compose'ta API, Docker `backend` ağı üzerinden `http://seq` adresine log yollar; tarayıcı arayüzü Traefik üzerinden aşağıdadır:

- `http://logs.soruasistanim.localhost`

İlk girişte `.env` içindeki `SEQ_ADMIN_PASSWORD` kullanılır. Seq verisi `seq_data` named volume'unda kalıcıdır. Local Compose, Seq sink'ini container içi `http://seq` adresiyle etkinleştirir; native Visual Studio akışında Serilog console ve dosya loglarına yazmaya devam eder.

### TickerQ job dashboard

TickerQ dashboard API container'ı içinde çalışır ve localde ayrı alan adına yönlendirilir:

- `http://jobs.soruasistanim.localhost/tickerq`

Bu dashboard job çalışmaları içindir; uygulama API'sinden ayrı bir container değildir. Kullanıcı adı ve parola `.env` içindeki `TICKERQ_USERNAME`/`TICKERQ_PASSWORD` değerleridir; değiştirdikten sonra `docker compose up -d --build api` çalıştırılır. Production'da dashboard için domain atamak zorunlu değildir; atanmadan önce Coolify proxy erişim kısıtı (Basic Auth/IP allowlist/VPN) uygulanır.

### Secret düzeni

- API'nin Gemini anahtarı ve SMTP uygulama parolası artık `Presentation/API/appsettings.json` içinde değildir.
- Native Visual Studio geliştirmede bunlar .NET User Secrets deposundan okunur. Yeni bilgisayarda bir kez girilir:

```powershell
dotnet user-secrets set "Gemini:ApiKey" "<anahtar>" --project Presentation/API
dotnet user-secrets set "Email:Smtp:Password" "<smtp-uygulama-parolası>" --project Presentation/API
```

- Docker local için aynı değerler gerekiyorsa `.env` içine `GEMINI_API_KEY` ve `SMTP_PASSWORD` eklenir; Compose bunları API'ye geçirir. Bu iki değer production'da yalnız Coolify Environment Variables/Secret alanında tutulur.

### Local ağ notu

Traefik, API ve Admin birden fazla Docker ağına bağlı olduğu için `edge` ağını açıkça kullanır:

```yaml
--providers.docker.network=soruasistanim-local_edge
```

Docker Engine 29 ile uyum için Traefik imajı `traefik:v3.7.13` olarak sabitlenmiştir. Eski `v3.2`, Docker provider etiketlerini okuyamadığı için tüm alan adlarında 404 üretmiştir.

### Günlük geliştirme akışı

Hızlı geliştirme ve breakpoint için Docker zorunlu değildir:

```text
API + Admin → Visual Studio F5
React       → Presentation/REACT içinde npm run dev
Veritabanı  → mevcut LocalDB
```

React'in native geliştirme API'si için Git'e girmeyen `Presentation/REACT/.env.development.local`:

```env
VITE_API_BASE_URL=https://localhost:7044/api/v1
```

Visual Studio Admin geliştirme ayarlarında hem `ApiSettings:BaseUrl` hem `ApiSettings:InternalBaseUrl`, `https://localhost:7044/api/v1` olmalıdır. CORS, hem Visual Studio originlerini hem Docker local originlerini içermelidir.

Önerilen döngü:

1. Günlük kodlama, log ve debugger için native geliştirme akışını kullan.
2. Anlamlı bir değişiklik tamamlandığında `docker compose up -d --build` ile production-benzeri entegrasyon testi yap.
3. Production'a göndermeden önce full Compose testini tekrarla.

## Production Kurulum — Coolify

> Mevcut `compose.yaml` local HTTP kurulumu içindir. Production deployment katmanı Coolify'dir; Coolify uygulama Compose'unun içinde çalışan bir servis değildir. Sunucuya bir kez kurulur, Git reposundan uygulama stack'ini build/deploy eder ve kendi Traefik proxy'siyle domain/TLS yönetir.

### Sunucu gereksinimleri

- Linux VPS (öneri: Ubuntu LTS), en az 2 vCPU, 4 GB RAM ve 40 GB SSD.
- Docker Engine + Docker Compose plugin; Coolify kurulumu için root SSH erişimi.
- Alan adı DNS kayıtları:
  - `soruasistanim.com` → sunucu IPv4 adresi
  - `www.soruasistanim.com` → sunucu IPv4 adresi (isteğe bağlı redirect)
  - `api.soruasistanim.com` → sunucu IPv4 adresi
  - `admin.soruasistanim.com` → sunucu IPv4 adresi
- Güvenlik duvarında yalnız `80/tcp` ve `443/tcp` açık olmalı. SQL Server portu açılmaz.
- Coolify'nin yönettiği Traefik + Let's Encrypt TLS kullanılmalı. Uygulama Compose'una ikinci bir Traefik eklenmez.

### Production secret kuralları

- Sunucuda ayrı bir `.env` veya secret manager kullan.
- `SA_PASSWORD`, `JWT_KEY`, Gemini/OpenAI anahtarları, SMTP uygulama parolası ve Google OAuth client ayarları kaynak kodda tutulmaz.
- `JWT_KEY` local'den farklı ve rastgele olmalı.
- Daha önce Git'e yazılmış Gemini anahtarı ve SMTP uygulama parolası sızmış kabul edilir; production öncesi sağlayıcılarından yenilenmelidir.
- Seq yalnız local geliştirme stack'inde çalışır. İlk production sürümünde ayrı Seq sunucusu yoktur; Serilog event'leri container console'una yazılır ve Coolify log ekranından izlenir.
- `TickerQ` dashboard için `TICKERQ_USERNAME` ve güçlü `TICKERQ_PASSWORD` tanımlanır. `jobs` domaini, erişim kısıtı kurulmadan Coolify'ye eklenmez.

### Production URL ayarları

```text
Frontend__BaseUrl=https://soruasistanim.com
PasswordReset__BaseUrl=https://soruasistanim.com/sifre-sifirla
Google__RedirectUri=https://api.soruasistanim.com/api/v1/Auth/google-callback
```

Frontend production build argümanı:

```text
VITE_API_BASE_URL=https://api.soruasistanim.com/api/v1
```

Admin browser tarafı için `ApiSettings__BaseUrl` public API URL'i; Admin'in server-side HttpClient'ı için `ApiSettings__InternalBaseUrl=http://api:8080/api/v1` kullanılır.

### Coolify production yerleşimi

```text
Internet → Coolify'nin Traefik proxy'si → frontend / admin / api
api → SQL Server
api → Serilog console logları → Coolify log ekranı
```

- Coolify, sunucuda ayrı platform servisidir; bu proje Compose dosyasına `coolify` servisi eklenmez.
- Coolify'de Git tabanlı Docker Compose deployment oluşturulur ve `compose.production.yaml` seçilir; production Compose'ta local `traefik` servisi ve `.localhost` etiketleri bulunmaz.
- Traefik production'da ortadan kalkmaz: local dosyadaki uygulamaya özel Traefik kalkar, yerine Coolify'nin yönettiği ortak proxy geçer. TLS ve HTTP→HTTPS yönlendirmesi bu dış proxy'de yapılır; API/Admin container'ları içeride HTTP ile konuşur.
- Production Compose'ta Seq servisi yoktur. Serilog kalır fakat Seq sink'i console sink'e çevrilir; frontend build sırasında production API URL'i kullanır.
- Coolify UI, deployment/container logları ve restart için kullanılır. Canlı log için Coolify'nin ilgili API container log ekranını aç.
- SQL Server host portu yayınlanmaz; yönetim SSMS ile VPN veya SSH tunnel üzerinden yapılır.
- API'nin Data Protection anahtarları `api_data_protection_keys` named volume'unda kalır. Container yeniden oluştuğunda kullanıcı cookie/oturumları geçersizleşmez.

### Coolify environment değişkenleri

Coolify resource içindeki Environment Variables/Secrets alanına en az şunlar girilir: `SA_PASSWORD`, `JWT_KEY`, `GEMINI_API_KEY`, `SMTP_PASSWORD`, `TICKERQ_USERNAME`, `TICKERQ_PASSWORD`.

Domainleri Coolify'de servis bazında tanımla: frontend `soruasistanim.com:80`, admin `admin.soruasistanim.com:8080`, API `api.soruasistanim.com:8080`. TickerQ için ilk canlı sürümde domain atama.

### Yedekleme

- `sqlserver_data`, `api_uploads` ve `api_data_protection_keys` kalıcı volume'lardır; sunucu disk yedeğine dahil edilmelidir.
- Ayrıca günlük SQL Server logical backup (`BACKUP DATABASE`) alıp sunucu dışındaki şifreli bir depoya gönder. Volume kopyası tek başına felaket kurtarma planı değildir.
- Geri yükleme tatbikatını canlıya çıkmadan önce ayrı bir test veritabanında yap.

### İlk production çalıştırma kontrol listesi

1. DNS kayıtlarının sunucuyu gösterdiğini doğrula.
2. Production secret'larını sunucuda oluştur.
3. Coolify'yi Ubuntu LTS sunucuya kur, Git repository'yi bağla ve production Docker Compose resource oluştur.
4. Coolify'de frontend, admin ve API için domain/port atamalarını yap; TLS/HTTPS redirect etkin olmalı.
5. Google Cloud Console'a tam production callback URL'ini ekle.
6. Frontend, Admin, API, login, dosya yükleme, şifre sıfırlama ve Gemini akışını HTTPS üzerinden test et; Coolify'de API container loglarının aktığını doğrula.
7. SQL Server volume yedeği, log saklama/rotasyon ve güncelleme prosedürü belirle.

## İlgili Dosyalar

- `compose.yaml`: local tam-stack Compose.
- `.env`: local Docker secret'ları; Git ignore.
- `.dockerignore`: Docker build context filtreleri.
- `Presentation/API/Dockerfile`: API multi-stage imajı.
- `Presentation/ADMIN/Dockerfile`: Admin multi-stage imajı.
- `Presentation/REACT/Dockerfile`: Vite build + Nginx imajı.
- `Presentation/REACT/nginx.conf`: SPA fallback yapılandırması.
- `Serilog.Sinks.Seq`: API'nin Seq sink paketi.

# SoruHavuzu — Agent Context & Proje Kuralları

## Proje
- Amaç: İlkokul öğretmenlerinin müfredata göre soru bulup seçmesini, soru seti oluşturmasını ve çıktı almasını kolaylaştırmak.
- Backend: .NET 9, SQL Server, EF Core, JWT, Clean Architecture + lite DDD.
- Frontend: React 19, Vite, Tailwind CSS, HeroUI, TanStack Query.
- API: `Presentation/API`.
- React: `Presentation/REACT`.
- Aktif admin: Razor Pages/MVC + Cuba template.

## En önemli kural — seçici context
Tüm Obsidian dosyalarını her görevde okuma; önce bu dosyadan ilgili yere yönlen.

Her görevde:
1. Bu dosyayı oku.
2. Görevin alanını belirle: API/backend, React, auth, soru seti, profil, admin vb.
3. Yalnızca o alanla doğrudan ilişkili `03-*`, `01-*` veya `02-*` notlarını oku.
4. İlişkili olmayan envanterleri tarama.
5. Notlar yalnızca yön bulmak içindir; gerçek davranışı kaynak koddan doğrula.
6. API isteği/DTO/endpoint gerekiyorsa `Presentation/API/docs/v1.json` tek gerçek kaynaktır; dosyanın tamamını yükleme, yalnızca ilgili path/operationId/DTO bölümünü ara.

Örnek:
- React soru seti değişikliği → [[03-frontend-react]] + ilgili hook/service/UI notu + gerekirse [[01-api-integration]].
- Auth değişikliği → [[03-frontend-auth-akisi]] + ilgili hook/service + [[01-api-integration]]
- Sadece React UI değişikliği → [[03-frontend-react]] + gerekiyorsa [[03-frontend-ui-komponentleri]]
- Backend repository/service değişikliği → ilgili API/backend kaynak kodu; React notlarını okuma.
- Admin UI değişikliği → [[02-admin-frontend]]; React notlarını okuma.
- React tarafında yeni bir yönlendirme route eklediğinde [[03-frontend-route-tablosu]] güncelle.

## Görev akışını seç
- Kullanıcı yalnızca inceleme, analiz veya plan istiyorsa [[05-planlama-workflow]] notunu oku. Kod yazma, test çalıştırma veya not/durum güncellemesi yapma.
- Kullanıcı kod değişikliği istiyorsa [[05-uygulama-workflow]] notunu oku.
- Plan onaylanıp uygulamaya geçilirse planlama akışını tekrar etme; doğrudan uygulama akışına geç.

## Derinlik ve aktif durum sınırı
- Notlarda: alan notu ve gerekirse yalnızca bir ilgili envanter notu oku.
- Planlama modunda: hedef dosyalar Seviye 0, doğrudan bağımlılık veya tüketicileri Seviye 1'dir. Seviye 2 ve proje-geneli tarama yapma.
- Uygulama modunda: yalnızca değişen davranışın doğrudan çağrı zincirinde gerektiği kadar derinleş. İlgisiz feature, `node_modules`, proje-geneli wildcard arama ve büyük refactor yapma.
- [[04-active-context]] varsayılan olarak okunmaz. Yalnızca kullanıcı önceki/yapılmakta olan işe atıf yapıyorsa veya aynı karar, feature, endpoint ya da migration'a bağlı bir iş varsa aç.
- İlişki belirsizse yada kullanıcı yasaklamışsa `04-active-context` okuma.

## Güvenlik
Güvenlik ayrıntılarını burada tekrar listeleme. Mevcut authentication/authorization, secret, rate-limit ve upload kurallarını kaynak kod + ilgili feature notundan doğrula. Gizli değerleri kaynak koda veya commit edilecek config'e koyma.

## İlgili Notlar
- DB Migration & Seed (MEB 4. Sınıf Matematik JSON Şeması + .NET seed servisi): [[06-db-migration-and-seed]]
- Öğretmen dashboard haftalık müfredat veri akışı: [[07-haftalik-mufredat-dashboard]]
- Manuel console ve react admin erişimli chatgpt soru üretimi [[ManuelQuestionGenerate]]

## Production ve Local Kurulum
- Docker, local geliştirme ve production sunucu gereksinimleri: [[08-docker-local-production-kurulum]]

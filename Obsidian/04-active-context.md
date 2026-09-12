# Aktif Durum ve Görev Panosu (Scratchpad)

## 1. Son Tamamlanan Görev
- [x] Production öncesi container güvenlik/operasyon hazırlığı: local `compose.yaml` korunarak ayrı `compose.production.yaml` eklendi (Coolify proxy/domain/TLS, uygulama içi Traefik yok); Gemini/SMTP sırları API appsettings'ten çıkarılıp User Secrets + Compose/Coolify environment'a taşındı; TickerQ kimlik bilgileri config/secret oldu; Data Protection anahtarları named volume'a kalıcılaştırıldı; runtime OpenAPI localhost self-call kaldırıldı; Docker/Coolify TLS redirect ayrımı yapıldı; React bağımlılıkları audit sonrası 0 açık verdi. Doğrulama: API build 0 hata, React build başarılı, iki Compose config geçerli, local API yeniden oluşturuldu.
- [x] Gemini/AI üretim dayanıklılığı (patlama önlemi): `Infrastructure/Ai/AiQuestionJsonParser.cs` eklendi (toleranslı JSON: `AllowTrailingCommas`/yorum atlama, `{...}` ayıklama, `Options` string→`{Key,Text}` normalizasyonu, boş/eksik soruları atlama). `GeminiApiService` parse hatasını retry ediyor (yeni `catch (JsonException)`), 429'da `Retry-After` okuyor (15sn üst sınır) + üstel backoff/jitter. `ManualQuestionAiProvider` aynı parser'ı kullanıyor (eski `DeserializeQuestions`/`StripCodeFence` kaldırıldı). `ManualQuestionGenerationService` timeout/istemci iptali/`JsonException`'ı `Result.Fail` ile karşılıyor (500 yok). `QuestionGeneratorService` hata sonrası `throw;` etmiyor (job "failed" gürültüsü yok; topic `FAILED` + sonraki döngüde retry). `ExceptionHandlingMiddleware` istemci iptalini (`RequestAborted`) sessiz geçiyor + `HasStarted` guard. Doğrulama: `dotnet build` 0 hata (8 görev öncesi uyarı).
- [x] React/HeroUI manuel soru üretimi: AdminRoute + admin menüsü; DB tabanlı Sınıf → Ders → Ünite → Konu zinciri ve soru sayıları; Gemini varsayılan/OpenAI placeholder; düzenlenebilir dinamik prompt; tür başına 20 veya toplam 60 soru üretme; önizleme/doğrulama ve kullanıcı onayı sonrası mevcut QuestionImportService ile import. Yeni API uçları `ManualQuestionGeneration/*` AdminOnly; 5 dakikalık otomatik akış değiştirilmedi. Doğrulama: .NET solution build 0 hata, oxlint 0 hata (6 eski uyarı), Vite build başarılı.
- [x] Katmanlı rate limiting: Global (100/dk), Login (10/5dk), Contact (5/5dk), Auth (10/1dk) — IP bazlı. Backend `RateLimiting/` (sabitler + options + extensions), GlobalLimiter + named policy (built-in, chained yok), `OnRejected` → ProblemDetails (code/message/retryAfterSeconds) + `Retry-After` header (lease `RetryAfter` meta, fallback WindowSeconds) + Warning log; `UseForwardedHeaders` (trusted proxy). React: `api/rateLimit.js`, `apiClient` ApiError code/retryAfterSeconds, `useAuthForm` + `useContactForm` 429 handling. Doğrulama: backend build 0 hata, oxlint 0 hata (6 eski uyarı), vite build 0 hata.
- [x] Home hero banner'a fade slider eklendi: `HeroSection.jsx` içinde `HERO_IMAGES` (teacher-01/02/03.webp) + `useState`/`useEffect` ile 5 sn'de `setInterval` döndürme; görseller mutlak konumda üst üste, `transition-opacity duration-700` + `opacity-100/0` crossfade; kapsayıcıya sabit `aspect-[16/9]` height (resim değişince boyut sabit). Doğrulama: oxlint 0 hata, `vite build` başarılı.
- [x] Admin hızlı soru düzenleme: `QuestionDetailModal`'a yalnızca adminin (role=2) gördüğü 3'lü düzenleme alanı eklendi (kalite tercihi, soru tipi, zorluk).
- [x] Backend: `UpdateQuestionQuickEditRequest` DTO + `IQuestionService/QuestionService.QuickEditAsync` + `PATCH /Questions/{id}/quick-edit` (`[Authorize(Policy="AdminOnly")]`).
- [x] `QuestionDto`/`QuestionMappings`/`mapQuestionDto`'ya `QualityPreference` eklendi (AI=0/TEACHER=1).
- [x] `ApiServiceExtensions`: `AdminOnly` policy (`RequireRole("Admin")`) eklendi.
- [x] Frontend: `questionService.js` + `useUpdateQuestion.js` + `apiClient.patch`; başarıda "Sorunuz güncellendi", hatada revert + toast.
- [x] Doğrulama: oxlint 0 hata, `vite build` başarılı, backend temp-output build 0 hata.
- [x] Hızlı düzenleme formu ayrı bileşene taşındı: `QuestionQuickEditForm.jsx` (lazy `useState` + `key={question.id}` remount) — default değerler artık mevcut sorudan gelir.
- [x] QuickEdit 404 teşhisi: `Result<T>.Fail` varsayılanı `BadRequest` (400) olduğundan servis 404 üretmez; 404, çalışan API sürecinin eski build'i çalıştırması (route yok) kaynaklıdır. Çözüm: API restart (kod değişikliği gerekmiyor).
- [x] React: `QuestionQuickEditForm` Select seçiminin arayüzde sabit kalma hatası düzeltildi — `handleFieldChange` artık optimistik `setEditValues` ile yeni değeri commit ediyor (hata durumunda `onError` revert korunuyor); `questionType` `Number(nextValue)` ile sayıya normalize ediliyor, erken dönüş guard'ı `String()` karşılaştırmasına çevrildi. Doğrulama: oxlint 0 hata, `vite build` başarılı.

## 2. Alınan Kritik Kararlar
- Manuel üretim geçici soru listesi döndürür; veritabanına yazma yalnız kullanıcı onayından sonra mevcut `/Questions/import` + `QuestionImportService` üzerinden yapılır. Otomatik `IQuestionGeneratorService` ve TickerQ job akışı bağımsız bırakıldı.
- AI anahtarları yalnız API sunucu ayarlarından okunur; Gemini varsayılandır, OpenAI için boş `ApiKey` ve sağlayıcı geçidi hazırlandı.
- .NET built-in `RateLimiterOptions.GlobalLimiter` önce, `EnableRateLimiting` endpoint policy'si sonra çalışır (kaynak kodla doğrulandı); bu yüzden chained/custom limiter gerekmedi → Global→(Login|Contact|Auth) doğal katmanlı.
- Partition key = `RemoteIpAddress` (ForwardedHeaders doğru yapılandırıldığında gerçek IP); X-Forwarded-For elle parse edilmez.
- `UseRateLimiter` authentication sonrasına alındı (UserId loglanabilir); `UseForwardedHeaders` pipeline'ın en başında.
- Retry-After: `OnRejectedContext.Lease` → `MetadataName.RetryAfter` (gerçek kalan süre), yoksa config `WindowSeconds`.
- Mevcut `auth` policy korundu (register/forgot/reset/google-callback için 10/1dk).
- Hızlı soru düzenleme, mevcut `UpdateQuestionRequest`'i (tam güncelleme + AdminOrTeacher) yeniden kullanmak yerine ayrı `UpdateQuestionQuickEditRequest` + `PATCH /Questions/{id}/quick-edit` (yalnız Admin) ile yapıldı — kısmi alan güncellemesi ve net yetki ayrımı.
- Cache soyutlaması Application'da (`ICacheService`), somut implementasyon Infrastructure'da (`MemoryCacheService`) — Clean Architecture korunuyor.
- Global widget'lar tek anahtar (`widgets:global`), kullanıcıya özel widget ayrı (`widgets:user:{userId}`) — invalidation temiz ve sınırlı.
- TTL: 10 dk (React `staleTime` ile uyumlu).
- Invalidation noktaları: soru create/delete, soru import, soru seti create/delete, öğretmen kaydı (email + Google).
- Genel anasayfa widget'ları `total_lessons/total_grades/total_topics/total_questions`; `total_teachers` home'da ignore; `user_question_set_count` anonimde döndürülmüyor.
- Silme onayı, HeroUI 2.7.6'da `AlertDialog` bulunmadığı için Modal tabanlı reusable `ConfirmDeleteDialog` ile yapıldı (ekstra bağımlılık yok).
- AI timeout'u model latansına (15–90sn+) bağlı olduğundan kısaltılmaz; istemci iptali (`RequestAborted`) ile AI timeout'u ayrı ele alınır.
- AI'dan dönen bozuk/şema dışı JSON "toleranslı parse + retry + graceful sonuç" ile karşılanır; servis katmanı exception fırlatıp 500/job-failed üretmez.

## 3. Şu Anki Odak ve Sıradaki Görev
- [ ] Rate limiting canlı doğrulanacak: Global aşımı → 429 GLOBAL; login 11. deneme → 429 LOGIN; contact 6. gönderim → 429 CONTACT; Retry-After + code/message/retryAfterSeconds; IP izolasyonu; nginx arkasında gerçek IP.
- [ ] Admin hızlı düzenleme canlı doğrulanacak: API restart sonrası admin token ile PATCH 200 + "Sorunuz güncellendi"; teacher/anonim → 403 (frontend panel gizli).
- [ ] Silme (faz 2): Düzenleme sayfasına (`EditQuestionSetPage`) silme butonu eklenecek; aynı `ConfirmDeleteDialog` kullanılacak.
- [ ] API yeniden başlatılıp (migration otomatik uygulanır) iletişim formu canlı doğrulanacak: form gönderimi + toast + enesxkaya@gmail.com'a mail.
- [ ] Görsel doğrulama (390/768/1280/1440 px) kullanıcı tarafından kontrol edilecek.

## 4. Dikkat Edilmesi Gereken Risk / Not
- OpenAI manuel üretimi, deployment/user-secrets üzerinden `OpenAI:ApiKey` verilene kadar bilinçli olarak yapılandırma hatası döndürür.
- Rate limiter instance-local (memory) çalışır; tek instance production başlangıcı için yeterli. Scale-out'ta sayaç paylaşılmaz → Redis/distributed limiter veya gateway/WAF gerekir.
- Production Nginx: `X-Forwarded-For` + `X-Forwarded-Proto` header'larını set etmeli; `ForwardedHeaders:KnownProxies` gerçek proxy IP'sine göre güncellenmeli (şu an 127.0.0.1/::1).
- .NET 9'da `ForwardedHeadersOptions.KnownNetworks` = `Microsoft.AspNetCore.HttpOverrides.IPNetwork` (.NET 10'da `KnownIPNetworks` + `System.Net.IPNetwork`).
- Hızlı düzenleme sonrası parent (`CreateQuestionSetPage`) `questions` state'i tazelenmez; modal tekrar açılırsa eski prop değerinden seed alır. Detay görünümü (şıklar/cevap) `question.*` prop'undan okur, `editValues`'tan değil.
- API `dotnet build`'i çalışan `SoruHavuzu.API` sürecinin DLL kilidi nedeniyle MSB3027/MSB3021 verir; doğrulama `-o <temp>` ile yapıldı. Canlı test için API durdurulup yeniden başlatılmalı.
- QuickEdit `PATCH /Questions/{id}/quick-edit` route'u doğru ve derleniyor; 404 yalnızca eski build'de görülür. `ResultStatus` map: `Fail` varsayılanı 400; "soru bulunamadı" semantiği için istenirse `Fail("...", ResultStatus.NotFound)` yapılabilir (mevcut `UpdateAsync` ile hizalı bırakıldı).
- QuestionCard davranış kararı: Detay modalı yalnızca ArrowRight ikonundan açılır; checkbox yalnızca seçim toggles, kart gövdesi tıklanabilir değildir.
- API projesinin `dotnet build`'i, çalışan `SoruHavuzu.API` sürecinin DLL kilidi nedeniyle kopyalama aşamasında hata veriyor (MSB3027/MSB3021); doğrulama için API durdurulup yeniden build alınmalı.
- `IMemoryCache.GetOrCreateAsync` eşzamanlı isteklerde factory'yi birden fazla kez çalıştırabilir (stampede); 5 COUNT sorgusu için kabul edilebilir.
- `v1.json` başlangıçta otomatik yeniden üretiliyor; `WidgetDataDto` yanıt şeması hâlâ boş (opsiyonel `[ProducesResponseType]`).
- Lint'teki 6 uyarı görev öncesinden mevcut; bu görev kaynaklı değil.
- `home_yedek/` klasörü eski anasayfayı içeriyor; doğrulama sonrası istenirse kaldırılabilir.
- İletişim formu: API süreci doğrulama için durduruldu; yeniden başlatılmalı. `AddContactMessage` + `AddEmailSentAtToContactMessage` migration'ları startup'taki `MigrateAsync` ile otomatik uygulanır.
- Bildirim TickerQ cron ile dakikada bir çalışır; e-posta en fazla ~1 dk gecikebilir (polling). TickerQ store (Redis/EF persistence) gerekmedi — mevcut cron deseniyle aynı.
- Silme hem "Soru Setlerim" hem dashboard "Son Oluşturulanlar" kartında görünür (ortak `ACTIONS` + `useQuestionSetActions`). DELETE 204 döndüğü için React'te body/result kontrolü yok; hata `apiClient` üzerinden `ApiError` ile gelir.
- `AiQuestionJsonParser` kurtarılamayan (dengesiz parantez vb.) JSON'da `JsonException` fırlatır; `GeminiApiService` retry ile, manuel üretim `Result.Fail` ile karşılar. `AiQuestionBatchResponse` artık deserialize'ta kullanılmıyor (parser `JsonDocument` ile çalışıyor) — DTO/schema geriye dönük uyumlu bırakıldı.
- Gemini AI dayanıklılığı canlıda doğrulanmalı: bozuk JSON/429/timeout senaryolarında 500 veya "Job failed" üretilmediği; topic FAILED sonrası sonraki döngüde yeniden denendiği.




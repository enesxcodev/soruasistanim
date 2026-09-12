# Kod Uygulama Akışı

Bu akış yalnızca kullanıcı kod veya kalıcı proje değişikliği istediğinde kullanılır.

1. Kapsamı ve etkilenen katmanları belirle; alan notunu ve gerekirse tek ilgili envanteri oku. `04-active-context` varsayılan olarak okunmaz.
2. Notlardaki dosya, endpoint, hook, service ve mimari bilgisini gerçek kaynak kodda doğrula.
3. API değişikliği varsa `Presentation/API/docs/v1.json` içinde yalnızca ilgili path, operationId veya DTO bölümünü doğrula; endpoint veya model uydurma.
4. Değişen davranışın doğrudan çağrı zincirini gerektiği kadar takip et. Yeni bir katmana yalnızca değişiklik için gerekli olduğu doğrulanırsa geç; ilgisiz feature, `node_modules`, proje-geneli wildcard arama ve büyük refactor yapma.
5. Kullanıcı plan talep etmiş veya kararın yönünü etkileyen belirsizlik varsa, kod yazmadan önce [[05-planlama-workflow]] biçiminde kısa plan sun.
6. Yalnızca gerekli dosyalarda değişiklik yap; mevcut mimari ve pattern'leri koru, gereksiz refactor yapma.
7. Uygun build, lint veya testi çalıştır; başarısızlıkta kapsamı büyütmeden nedeni düzelt.
8. Kalıcı mimari, endpoint, hook, service, route veya paket davranışı değiştiyse yalnızca ilgili Obsidian envanterini güncelle.
9. Kaynak kodda kalıcı değişiklik yapıldı ve doğrulama tamamlandıysa [[04-active-context]] dosyasını aşağıdaki şablona göre güncelle. Salt-okunur inceleme, planlama veya onay bekleyen işlerde güncelleme yapma.

## Genel kod kuralları
- I/O işlemlerinde async/await kullan.
- EF Core salt-okunur sorgularda uygun olduğunda `AsNoTracking()` kullan.
- DI için constructor injection kullan.
- Null kontrollerinde modern C# yaklaşımını kullan.
- UI ile domain/business logic'i gereksiz yere birbirine bağlama.
- Yeni paket eklemeden önce mevcut paketleri kontrol et.
- Küçük bir değişiklik için büyük refactor yapma.

## Dinamik hafıza şablonu
`04-active-context.md` güncellenirse bu şablonu aynen koru:

# Aktif Durum ve Görev Panosu (Scratchpad)

## 1. Son Tamamlanan Görev
- [x] <Tek cümleyle az önce biten iş ve dosya adı>
## 2. Alınan Kritik Kararlar
- <Varsa mimari veya veri tipi kararı; yoksa 'Yok'>
## 3. Şu Anki Odak ve Sıradaki Görev
- [ ] <Bir sonraki oturumda yapılması gereken ilk iş>
- [ ] <Varsa ikinci adım / Yoksa 'Kullanıcıdan yeni görev bekleniyor'>
## 4. Dikkat Edilmesi Gereken Risk / Not
- <Örn: DB migration basılmadı / API mock dönüyor; yoksa 'Temiz'>

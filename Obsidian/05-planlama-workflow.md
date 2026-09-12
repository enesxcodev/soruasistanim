# Planlama / İnceleme Akışı

Bu akış yalnızca kullanıcı inceleme, analiz, teknik plan veya öneri istediğinde kullanılır. Kod, test, migration veya Obsidian notu değiştirme.

1. İsteği tek cümleyle tanımla; etkilenen katmanları ve açık kapsam sınırlarını belirt.
2. Kök notun yönlendirdiği alan notunu ve gerekirse tek ilgili envanteri oku. İlişkisiz notları ve `04-active-context` dosyasını açma.
3. Notlardaki yalnızca plan için gerekli dosya, endpoint, hook veya mimari bilgisini kaynak kodda hedefli doğrula. Geniş proje taraması yapma.
4. API gerekiyorsa `Presentation/API/docs/v1.json` içinde yalnızca ilgili path, operationId veya DTO bölümünü ara; asla tüm dosyayı okuma.
5. Hedef dosyalar Seviye 0, doğrudan bağlantıları Seviye 1'dir. Seviye 2'ye ve proje-geneli taramaya geçme; gerekli ayrıntıyı “uygulama aşamasında doğrulanacak” diye not et.
6. Planı şu başlıklarla ver: mevcut durum, eksik/risk, önerilen çözüm, değişecek dosyalar ve doğrulama adımları.
7. Eksik bir iş kuralı kararın yönünü değiştiriyorsa kısa soru sor; aksi durumda makul varsayımı açıkça belirterek planı tamamla.

Plan tamamlanınca dur. Kullanıcı uygulama isterse [[05-uygulama-workflow]] akışına geç.

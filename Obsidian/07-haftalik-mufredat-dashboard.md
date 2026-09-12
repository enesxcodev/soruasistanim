# Haftalık Müfredat — Öğretmen Dashboard

## Veri akışı

1. Öğretmen dashboard açıldığında React, `GET /api/v1/Curriculum/active-week/topics` çağrısını yapar.
2. Endpoint, istek sahibi kullanıcının `GradeId` profil bilgisini kullanır; parametre verilmezse profil sınıfı zorunludur.
3. `CurriculumService`, `AcademicCalendarHelper` ile aktif öğretim haftasını hesaplar ve bu sınıf + hafta için `CurriculumSchedule` kayıtlarını getirir.
4. Schedule kaydı varsa konu (`TopicId`) üzerinden plan kullanılır. Kayıt yoksa profil sınıfı + öğretim haftası, müfredattaki ünite sırasına eşlenir (örn. 4. sınıf + 1. hafta => her dersteki `4.1` kodlu ünite) ve o üniteye bağlı konular getirilir.
5. Repository, konu ile bağlı `Unit` ilişkisini de yükler; yanıt her konu için `unitName` ve `unitCode` içerir.
6. Dashboard sonuçları ders bazında kartlara ayırır. Kartta önce ünite kodu ve adı, altında konu etiketleri gösterilir. Bir dersin birden çok farklı planı varsa kartta ayrılmış etiketler olarak görünür.

## Değişen parçalar

- API aktif-hafta DTO'suna `ActiveWeekTopicDto.UnitName` eklendi.
- `CurriculumScheduleRepository`, `Topic.Unit` ilişkisini aktif hafta ve schedule sorgularında eager-load eder.
- React dashboard'a `WeeklyCurriculumOverview`, sorgu hook'u ve `/Curriculum/active-week/topics` istemcisi eklendi.
- Tarih seçimi için `/Curriculum/calendar/terms` ve `/Curriculum/calendar/week` endpointleri eklendi. Hero UI takvimi doğrudan `WeeklyCurriculumOverview` içindeki kartları besler.
- Takvim `tr-TR` yereline bağlandı; ay, gün ve gezinme metinleri Türkçe gösterilir.

## Takvim ayarı

Aktif hafta, `Core/Domain/Common/AcademicTermSettings.cs` içindeki dönem başlangıcı, tatiller ve `MaxWeeks` ile hesaplanır. Yapı 2025–2026 dönemine ek olarak 14 Eylül 2026 başlangıçlı 2026–2027 dönemini içerir. 2026–2027 için belirsiz tatil tarihleri eklenmemiştir. Bu yapı mevcut schedule kayıtlarının hafta numaralarını değiştirmez; yalnızca seçilen tarihin hangi hafta olduğunu belirler.

## Tarih seçmeli takvim

“Planlanmış dersler” başlığındaki takvim ikonu Hero UI `Calendar` açar. Bugün dönem dışındaysa kullanıcı kartları hemen görebilsin diye en yakın dönem başlangıcı seçilir. Geçerli bir gün seçildiğinde aynı bölümdeki kartlar `/Curriculum/calendar/week` yanıtıyla yenilenir. Her kart ders adını, ünite kodu/başlığını ve konu etiketlerini ayrı ayrı gösterir. Tarih tatil aralığındaysa veya dönem dışındaysa gün seçilemez; schedule kaydı yoksa müfredat ünite sırası geri dönüşü çalışır.

Tatil aralığı mevcut `HolidayPeriod` kuralından türetilir: `EndDate` dahil, `WeeksToSubtract` kadar gün geriye doğru olan dönem tatildir. Bu kural, eski aktif-hafta hesabını değiştirmez; yalnızca tarih seçmeli takvimin tatil gününü açıkça tanımasını sağlar.

## Atama kapsamı

Mevcut `AppUser` modelinde kullanıcıya bağlı ders listesi yoktur; yalnızca `GradeId` bulunur. Bu nedenle kartlar, profil sınıfının aktif hafta schedule'ındaki tüm dersleri gösterir.

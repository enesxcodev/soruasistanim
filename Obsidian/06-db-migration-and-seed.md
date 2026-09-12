# DB Migration & Seed — Faz bazlı MEB müfredat pipeline'ı

> **Ayrık tasarım notu.** `db/curriculum/**` JSON dosyaları → `CurriculumSeederService` faz-bazlı pipeline (Grade → Lesson → Unit/Tema → Topic/Alt Öğrenme Alanı → SubTopic/Kazanım). **Gerçek davranışı her zaman kaynak koddan doğrula.** (bkz. `00-root-general-information.md`)

## 1. Mimari karar (2026-08 revizyonu)
- Eski tek-dosya/tek-transaction `UnitTopicSeederService` (`db/unit_topic/*.json` + `finish_` rename) **kaldırıldı**.
- Yerine faz bazlı, idempotent (rename'siz, unique index + upsert) `CurriculumSeederService` geldi.
- `Code` alanı artık **DB'de kalıcı** (Units/Topics/SubTopics; nvarchar(32), nullable) → MEB kazanım kodu izlenebilir.
- Unique index'ler: `Grades.Level`, `Lessons.Name`, `Units(GradeId,LessonId,Name)`, `Topics(GradeId,LessonId,UnitId,Name)` (filtered: UnitId NOT NULL), `SubTopics(TopicId,Name)`. Migration: `AddCurriculumCodesAndNaturalKeys`.
- **Uyarı:** Topics'teki filtered index yüzünden bu tablolara `sqlcmd` ile DELETE atarken `SET QUOTED_IDENTIFIER ON;` gerekli.

## 2. Dizin ve dosya düzeni
```
db/curriculum/
  catalog/grades.json     # Faz 0 — { grades: [ { name, level } ] }
  catalog/lessons.json    # Faz 1 — { lessons: [ { name, grades: [1,2,3] } ] } (ders×sınıf matrisi)
  units/04_matematik.units.json        # Faz 2 — { gradeLevel, lesson, units: [ { title, order, code:"4.1" } ] }
  topics/04_matematik.topics.json      # Faz 3 — topics: [ { unitCode:"4.1", title, order, code:"4.1.1" } ]
  subtopics/04_matematik.subtopics.json# Faz 4 — subTopics: [ { topicCode:"4.1.1", title, order, code:"4.1.1.1" } ]
  schema/*.schema.json    # JSON-Schema draft-07 (editor/CI doğrulaması; servis runtime şema doğrulaması YAPMAZ)
```
- Parent-child zinciri `UnitCode` / `TopicCode` ile (Id'den bağımsız, tekrar çalıştırılabilir).
- Başlıklara `"1. Ünite: "` öneki KONMAZ; sıra `order` alanındadır.
- Ders×sınıf matrisi: Türkçe/Matematik 1-4; Hayat Bilgisi 1-3; Fen Bilimleri 3-4; Sosyal Bilgiler 4. Matris dışı dosya yine işlenir ama raporda UYARI düşer.

## 3. Servis
- **Kontrat:** `Core/Application/Interfaces/ICurriculumSeederService.cs`
  ```csharp
  Task<IReadOnlyList<string>> SeedAsync(CurriculumSeedPhase? phase = null,
                                        string? directoryPath = null,
                                        CancellationToken cancellationToken = default);
  ```
  `phase` null → tüm fazlar sırayla; dönen liste rapor satırları. `CurriculumSeedPhase`: Grades=0, Lessons=1, Units=2, Topics=3, SubTopics=4.
- **Implement:** `Infrastructure/Persistence/Seed/CurriculumSeederService.cs`
  - Her **dosya kendi transaction**'ında (eski all-or-nothing değil).
  - Upsert: önce `Code` (scope: grade+lesson / topic), yoksa doğal anahtar Name ile eşleş; eksikse ekle, fark varsa güncelle (Name/Code/Order + UpdatedAt).
  - Dizin çözümleme: parametre → `Curriculum:SeedDirectory` config → repo kökü `db/curriculum` fallback.
- **DI:** `InfrastructureServiceExtensions` → `AddScoped<ICurriculumSeederService, CurriculumSeederService>()`.
- **Çağrı:** `Presentation/API/Program.cs` (MigrateAsync → DataSeeder → CurriculumSeeder; rapor satırları Console'a).

## 4. DataSeeder durumu
- Aktif: Cities/Districts/Schools/Grades/Users + Lessons (5 ders: Türkçe, Matematik, Hayat Bilgisi, Fen Bilimleri, Sosyal Bilgiler).
- Yorum satırı (placeholder): Units/Topics/CurriculumSchedules/Questions/QuestionSets — curriculum pipeline ile çakışmaması için kapalı.

## 5. Yeniden seed (temizlik)
Questions/CurriculumSchedules boşsa müfredat katmanı güvenle silinir:
```sql
SET QUOTED_IDENTIFIER ON;  -- Topics filtered index yüzünden zorunlu
BEGIN TRAN; DELETE FROM SubTopics; DELETE FROM Topics; DELETE FROM Units; COMMIT;
```
Backup: `BACKUP DATABASE SoruHavuzuDb TO DISK='...db\SoruHavuzuDb_backup.bak' WITH INIT;`

## 6. Doğrulama (2026-08-20 canlı)
- `dotnet build`: 0 hata. Migration `AddCurriculumCodesAndNaturalKeys` uygulandı.
- İlk çalıştırma: Faz2 6 ünite / Faz3 19 konu / Faz4 39 kazanım eklendi (4. Sınıf Matematik).
- İkinci çalıştırma: 0 eklendi/0 güncellendi → idempotentlik doğrulandı.
- **Faz 2 tamamlandı:** 13 ders×sınıf dosyası, 96 ünite/tema DB'de. Matris birebir (Fen yalnız 3-4, Sosyal yalnız 4, Hayat Bilgisi yalnız 1-3).
- **Faz 3-4 durumu (2026-08-20):** dolu olanlar → 4. Matematik (19 konu/39 kazanım), 1-3. Matematik (15+18+22 konu, 30+36+45 kazanım), 1-3. Hayat Bilgisi (13+16+17 konu, 26+32+34 kazanım). Türkçe 1-4, Fen 3-4, Sosyal 4 yalnız ünite düzeyinde (kullanıcı talimatıyla duraklatıldı; ana odak 4. sınıf Matematik).
- Ünite listesi doğrulama durumu: `db/curriculum/README.md`.
- DB son durum: Grades=4, Lessons=5, Units=96, Topics=120, SubTopics=242.
- **2026-08-26 tam reseseed:** QuestionSetQuestions(15)+Questions(31)+SubTopics(242)+Topics(147)+Units(96) silindi; Faz 2-3 yeniden çalıştı. Sonuç: Units=53, Topics=305 (3. ve 4. sınıf 8 dosya; 0 atlandı). Faz 4 subtopics dosyası henüz yok. 1-2. sınıf dosyaları kaldırıldı (sadece 3-4. sınıf müfredatı aktif).

## 7. İlgili dosyalar
- `db/curriculum/**` — katalog + faz dosyaları + şemalar.
- `Core/Application/DTOs/Seed/CurriculumSeedDtos.cs` — faz DTO'ları.
- `Core/Application/Interfaces/ICurriculumSeederService.cs` — kontrat + faz enum'u.
- `Infrastructure/Persistence/Seed/CurriculumSeederService.cs` — implement.
- `Infrastructure/Persistence/Configurations/SubTopicsConfiguration.cs` — yeni (Name 300, Code 32, unique index).
- `Infrastructure/Persistence/Seed/DataSeeder.cs` — yalnızca katalog dışı seed'ler.

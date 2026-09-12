# Manuel Soru Üretimi (5 Dakikada Soru Üretimi)

> Amaç: ChatGPT / Gemini gibi bir chatbot ajanının proje dosyalarına erişip soru üretmesini
> ve çıktıyı veritabanına aktarmasını kolaylaştıran planlama/dökümantasyon notu.
> Bu yöntem, 5 dakikada bir çalışan otomatik AI schedule'ından BAĞIMSIZ, elle tetiklenen yöntemdir.

## Api ile 5 dakika bir soru üretimi

### Prompt'un çalışma yapısı — `Infrastructure/Ai/GeminiApiService.cs`

`GenerateQuestionsAsync(grade, lesson, unit, topic, existingQuestions, count = 12, ct)` metodu
çağrıldığında `BuildPrompt` aşağıdaki bölümleri üretir:

1. **Rol/kişilik:** "MEB müfredatına tam hakim, deneyimli ilkokul öğretmeni".
2. **Hedef kitle:** `{grade}` düzeyindeki 8–10 yaş grubu.
3. **Bağlam:** `Ders`, `Ünite`, `Konu` parametreleri prompt içine gömülür.
4. **Görev:** "TAM {count} adet soru üret".
5. **Dil ve pedagoji kuralları:** yalın/sevimli Türkçe, günlük hayattan somut örnekler,
   ağır akademik terimlerden kaçınma.
6. **Soru dağılımı:**
   - Zorluk: Kolay / Orta / Zor dengeli.
   - Türler (karma):
     - `MultipleChoice`: A–D 4 şık, tek doğru; `CorrectAnswer` yalnızca şık harfi (A/B/C/D).
     - `FillInTheBlank`: boşluk kesinlikle `( .......... )` (10 nokta); `Options` boş `[]`;
       `CorrectAnswer` yalnızca boşluğa gelecek kelime/sayı.
     - `OpenEnded`: işlem basamaklı problem; `Options` boş `[]`; `CorrectAnswer` nihai sonuç;
       `Explanation` adım adım çözüm.
7. **repeatBlock:** `existingQuestions` doluysa, aynı/çok benzer soru üretilmemesi uyarısı
   mevcut soru kökleriyle birlikte eklenir.
8. **Çıktı formatı:** Sadece geçerli JSON (markdown/açıklama yok). Şema:
   `{ "Questions": [ { "QuestionText", "QuestionType", "Difficulty", "Options",
   "CorrectAnswer", "Explanation" } ] }`
   → Not: `Grade/Lesson/Unit/Topic` bu çıktıda ÜST SEVİYEDE yer almaz; servis bunları parametre olarak zaten bilir.

### API çağrısı davranışı
- İstek gövdesi `contents[0].parts[0].text = prompt`; `generationConfig` →
  `responseMimeType = "application/json"`, `temperature = 0.8`.
- Model sırası: `Gemini:Model` (varsayılan `gemini-3.6-flash`) + `Gemini:FallbackModels`.
- Model başına en fazla 2 deneme; 429/5xx geçici hatalarda retry (gecikmeli); timeout yakalanır.
- Yanıt parse: `candidates[0].content.parts[0].text` → code fence temizlenir → `AiQuestionBatchResponse`.

---

## Tool: QuestionImporter (`tools/QuestionImporter`)

### `db/questions_import.json` çalışma yapısı
JSON üst seviyede şu alanları içerir (chatbot çıktısı olarak da aynı şema kullanılır):

- `Grade`, `Lesson`, `Unit`, `Topic` → hedef sınıf/ders/ünite/konu **isimleri**.
  - ID gerekmez; importer bu isimleri DB'de çözümler.
  - İsimler DB kayıtlarıyla **birebir aynı** olmalıdır (boşluk + Türkçe karakterler dahil).
- `Questions[]` → her soru: `QuestionText`, `QuestionType`, `Difficulty`,
  `Options[{Key,Text}]`, `CorrectAnswer`, `Explanation`.

> Fark: GeminiApiService çıktısı sadece `Questions` içerirken, bu dosyada hedef konu
> (`Grade/Lesson/Unit/Topic`) üst seviyede de bulunur.

### Terminalden import komutu
```bash
dotnet run --project tools/QuestionImporter -- db/questions_import.json
```
Farklı bağlantı için:
```bash
dotnet run --project tools/QuestionImporter -- db/questions_import.json --connection "..."
```

### Tool çalışma mantığı (Program.cs)
1. `args[0]` = JSON dosya yolu; `--connection "..."` opsiyonel.
2. Connection string önceliği: `--connection` → `appsettings.json` → LocalDB default.
3. JSON okunup `AiQuestionImportDto`'ya deserialize edilir (case-insensitive).
4. DI kurulur (EF Core SqlServer + repository'ler + `QuestionImportService`).
5. `ImportAsync(request)` çalıştırılır; sonuç `Added / SkippedDuplicate / Failed / Errors` olarak yazdırılır.

### Alternatif: API endpoint
```
POST /api/v1/Questions/import
Authorization: Bearer <token>   (AdminOrTeacher)
Body: questions_import.json ile aynı şema
```

### Davranış / notlar
- Sorular `QualityPreference = TEACHER` olarak eklenir (otomatik AI schedule'dan ayrışır).
- Aynı konuda aynı soru metni varsa atlanır (dedupe).
- Konu `TargetQuestionCount`'a (100) ulaşınca `COMPLETED` işaretlenir; otomatik schedule o konuyu bir daha üretmez.
- İsim bulunamazsa işlem durur; mevcut seçenekler hata mesajında listelenir.

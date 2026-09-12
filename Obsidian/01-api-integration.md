# API Entegrasyonu

## Single Source of Truth
Endpoint, HTTP method, request/response DTO ve query/body alanları için:
`Presentation/API/docs/v1.json`

- Endpoint veya DTO uydurma.
- React tarafında API çağrısı gerekiyorsa önce ilgili `v1.json` kaydını doğrula.
- Sonra gerçek backend controller/service/DTO kodunu kontrol et.
- `apiClient` yalnızca feature service katmanı üzerinden kullanılmalı.

## Context Kuralı
Bu notu yalnızca API ile bağlantılı görevlerde oku. API ile ilgisi olmayan UI görevlerinde yükleme.

## Doğrulama
Not ile kaynak kod çelişirse kaynak kodu esas al ve gerekiyorsa ilgili Obsidian notunu güncelle.

# React Servis Envanteri

## apiClient
`src/api/apiClient.js` tüm HTTP isteklerinin giriş noktasıdır.
- `get`, `post`, `put`, `delete`, `upload`
- Base URL: `VITE_API_BASE_URL`
- Token: `accessToken` → Bearer header
- Hata: `ApiError`

## Servisler
- `authService.js` → register/login/forgotPassword/resetPassword/logout
- `dashboardService.js` → dashboard widget/latest set işlemleri
- `questionSetService.js` → soru filtreleme, set listeleme/detay/oluşturma/güncelleme
- `curriculumService.js` → Grades/Lessons/Topics/SubTopics
- `profileService.js` → current user/profile/photo
- `locationService.js` → Cities/Districts
- `manualQuestionGenerationService.js` → admin manuel üretim, prompt önizleme ve onaylı import

## Kurallar
- Page/component içinde doğrudan `apiClient` kullanma.
- Her feature kendi service dosyasından API çağrısı yapar.
- Endpoint ve DTO için `Presentation/API/docs/v1.json` tek gerçek kaynaktır.
- Service envanteri yalnızca ilgili API/service görevlerinde okunur.

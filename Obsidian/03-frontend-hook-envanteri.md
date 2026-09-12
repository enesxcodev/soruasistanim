# React Hook Envanteri

| Hook | Konum | Amaç |
|---|---|---|
| `useAuth` | `features/auth/context/AuthContext.jsx` | Auth state |
| `useAuthForm` | `features/auth/hooks/useAuthForm.js` | Auth form + Zod |
| `useQuestionSets` | `features/question-set/hooks/useQuestionSets.js` | Soru seti listeleme/sayfalama |
| `useQuestionSetDetail` | aynı | Soru seti detay |
| `useCreateQuestionSet` | aynı | Soru seti oluşturma mutation |
| `useUpdateQuestionSet` | aynı | Soru seti güncelleme mutation (PUT; tam questionIds değiştirme) |
| `useFetchQuestions` | `features/question-set/hooks/useQuestionPool.js` | Soru getir/değiştir |
| `useGrades` | `features/question-set/hooks/useCurriculum.js` | Sınıflar |
| `useLessons` | aynı | Dersler |
| `useTopics` | aynı | Konular |
| `useSubTopics` | aynı | Alt konular; şu an ertelenmiş |
| `useCurrentUser` | `features/profile/hooks/useCurrentUser.js` | `useAuth` wrapper |
| `useCities` | `features/profile/hooks/useLocation.js` | İller |
| `useDistricts` | aynı | İlçeler |
| `useUpdateProfile` | `features/profile/hooks/useProfileMutations.js` | Profil mutation |
| `useUploadPhoto` | aynı | Fotoğraf upload |
| `useDebounce` | `features/question-set/hooks/useDebounce.js` | Arama debounce |

## Kurallar
- API işlemleri feature hook/service mimarisi üzerinden yapılır.
- Auth state için doğrudan localStorage erişimi yapma; `useAuth` kullan.
- Yeni hook eklemeden önce mevcut hook'un genişletilip genişletilemeyeceğini kontrol et.

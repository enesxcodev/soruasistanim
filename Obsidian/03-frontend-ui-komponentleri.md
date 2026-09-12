# Paylaşılan React UI Komponentleri

| Component | Konum | Not |
|---|---|---|
| `Button` | `components/ui/Button.jsx` | primary/outline/ghost, loading |
| `Input` | `components/ui/Input.jsx` | error + password toggle |
| `Select` | `components/ui/Select.jsx` | label/error |
| `Skeleton` | `components/ui/Skeleton.jsx` | loading states |
| `DifficultyBadge` | `components/ui/DifficultyBadge.jsx` | kolay/orta/zor |
| `QuestionSetCard` | `components/ui/QuestionSetCard.jsx` | set kartı + action menu (Göster/Düzenle/…) |
| `SetQuestionsPanel` | `features/question-set/components/EditQuestionSet/SetQuestionsPanel.jsx` | edit ekranı sıralı soru listesi (X çıkar + ↑/↓ sırala) |
| `Pagination` | `components/ui/Pagination.jsx` | önceki/sonraki |
| `AuthLayout` | `features/auth/components/AuthLayout.jsx` | auth layout |
| `Divider` | `features/auth/components/Divider.jsx` | auth ayraç |
| `SocialLoginButton` | `features/auth/components/SocialLoginButton.jsx` | Google login |

## UI Kuralları
- Shared UI component'leri feature/business logic ve API çağrısı taşımaz.
- Önce mevcut component'i yeniden kullan; yalnızca gerçekten yeni bir primitive gerekiyorsa oluştur.
- Tailwind kullan; gereksiz inline style oluşturma.
- HeroUI kullanımı mevcut projedeki 2.7.6 sürümüyle uyumlu olmalı.
- Yazarak arama gereken alanlarda mevcut proje yaklaşımına uygun `Autocomplete` tercih edilir.

Bu not yalnızca UI component'i değiştiren görevlerde okunur.

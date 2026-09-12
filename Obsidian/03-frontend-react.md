# React Frontend

## Stack
React 19, Vite, Tailwind CSS v3, HeroUI 2.7.6, Lucide React, TanStack Query, react-hook-form, Zod.

## Mimari
- Feature-based yapı: `src/features/[feature]`.
- `src/components/ui` yalnızca paylaşılabilir UI bileşenleri içindir; API çağrısı veya feature-specific business state taşımaz.
- API çağrıları sayfa/component içinden doğrudan yapılmaz; feature `services/` üzerinden yapılır.
- Sunucu state'i için TanStack Query; auth state'i için `useAuth`.
- Route tanımları `src/routes/AppRoutes.jsx`.

## UI
- Mevcut tasarım dili ve mevcut shared component'leri önce kontrol et.
- Modal, Dropdown, Table, Card gibi alanlarda mevcut HeroUI kullanımını tercih et.
- Form elemanlarında mevcut `Select`, `Input`, `Button` vb. bileşenleri tekrar kullan.
- Yeni UI component'i oluşturmadan önce mevcut shared component'lerde uygun karşılık ara.

## API
Endpoint/request/response için `Presentation/API/docs/v1.json` doğrulanmadan model veya endpoint uydurma.

## Context
Bu dosyayı React görevi varsa oku. Alt envanterleri yalnızca görev onları ilgilendiriyorsa aç:
- route → [[03-frontend-route-tablosu]]
- hook → [[03-frontend-hook-envanteri]]
- service/API → [[03-frontend-servis-envanteri]]
- UI → [[03-frontend-ui-komponentleri]]
- auth → [[03-frontend-auth-akisi]]
- package → [[03-frontend-paketler]]

## Güncelleme
Yeni route/hook/service/component/package veya kalıcı mimari değişiklik olduysa yalnızca ilgili envanter notunu güncelle.

## PDF alanı
pdf oluşturma ile ilgili dosyalara burdan yönlenebilirsin => [[_pdf-export-progress]]
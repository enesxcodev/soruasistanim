# React Paket Envanteri

## Temel
- React `19.2.x`
- React Router `7.x`
- Vite `8.x`
- Tailwind CSS `3.4.x`
- HeroUI `@heroui/react 2.7.6` — sürümü serbest bırakma
- `@heroui/theme 2.4.13`
- Lucide React
- TanStack Query
- react-hook-form + Zod
- react-hot-toast

## Kural
- Yeni paket eklemeden önce mevcut bağımlılıklardan biriyle çözüm olup olmadığını kontrol et.
- Paket ekleme `npm install` üzerinden yapılır.
- `node_modules` commit edilmez.
- HeroUI `2.7.6` sürümünü Tailwind v3 uyumluluğu nedeniyle koru; yükseltme gerekiyorsa önce uyumluluk analizi yap.

Bu not yalnızca paket/dependency görevlerinde okunur.

# React Auth Akışı

- Auth state'in tek kaynağı `useAuth` / `AuthContext`.
- `ProtectedRoute`, authenticated olmayan kullanıcıyı `/giris` sayfasına yönlendirir ve `state.from` ile dönüş yolunu korur.
- Login: `authService.login` → token → AuthContext.
- Logout: token temizlenir → AuthContext sıfırlanır.
- Register: başarılı kayıt sonrası `/giris`.
- Forgot password: `POST /Auth/forgot-password`.
- Reset password: `POST /Auth/reset-password`, `email + token + newPassword`.
- Profil güncelleme sonrası kullanıcı state'i güncellenir.

## Kaynaklar
- `features/auth/context/AuthContext.jsx`
- `features/auth/services/authService.js`
- `routes/ProtectedRoute.jsx`
- `api/apiClient.js`

Gerçek endpoint/DTO için her zaman `Presentation/API/docs/v1.json` doğrulaması yap.

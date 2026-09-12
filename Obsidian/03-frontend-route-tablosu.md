# React Route Tablosu

| Path                                   | Component                                 | Auth      |
| -------------------------------------- | ----------------------------------------- | --------- |
| `/`                                    | `features/home/pages/HomePage`            | Public    |
| `/giris`                               | `features/auth/pages/LoginPage`           | Public    |
| `/kayit`                               | `features/auth/pages/RegisterPage`        | Public    |
| `/sifremi-unuttum`                     | `features/auth/pages/ForgotPasswordPage`  | Public    |
| `/sifre-sifirla`                       | `features/auth/pages/ResetPasswordPage`   | Public    |
| `/dashboard`                           | `DashboardLayout → DashboardOverviewPage` | Protected |
| `/dashboard/olustur`                   | `CreateQuestionSetPage`                   | Protected |
| `/dashboard/olustur-2`                 | `CreateQuestionSetPage2`                  | Protected |
| `/dashboard/soru-setlerim`             | `QuestionSetsPage`                        | Protected |
| `/dashboard/soru-setlerim/:id/duzenle` | `EditQuestionSetPage`                     | Protected |
| `/dashboard/profil`                    | `ProfilePage`                             | Protected |
| `/dashboard/manuel-soru-uret`          | `ManualQuestionGenerationPage`            | Admin     |

Route kaynağı: `src/routes/AppRoutes.jsx`.

Bu not yalnızca route/navigation görevi varsa okunur.

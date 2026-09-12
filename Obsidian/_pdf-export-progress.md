	# PDF Özelleştirme (Export-Edit) — Uygulama İlerlemesi

## Mimari Notlar
- **Frontend**: HeroUI 2.7.6 + Tailwind v3 + Lucide React + framer-motion
- **PDF**: `@react-pdf/renderer` ile canlı önizleme + blob indirme
- **Backend PDF**: Mevcut QuestPDF pipeline korundu (hızlı indirme için)
- **Preference**: `UserPdfPreferences` tablosu, her kullanıcı için tek kayıt (UserId unique index)
- **Layout**: export-edit sayfası DashboardLayout dışında, tam ekran genişlikte

## Kategori Bazında Ayar Sayısı
| Kategori | Ayar Sayısı |
|---|---|
| Okul Adı | 6 |
| Sınıf/Haz./Tarih | 6 |
| Soru Seti | 5 |
| Soru Görünümü | 6 |
| Sayfa Düzeni | 8 |
| **Toplam** | **31** |

## Oluşturulan Dosyalar (Backend)
- `Core/Domain/Entities/UserPdfPreference.cs`
- `Core/Application/DTOs/PdfPreferences/UserPdfPreferenceDto.cs`
- `Core/Application/Interfaces/IPdfPreferenceService.cs`
- `Core/Application/Services/PdfPreferenceService.cs`
- `Infrastructure/Persistence/Configurations/UserPdfPreferenceConfiguration.cs`
- `Presentation/API/Controllers/UserPdfPreferencesController.cs`
- Güncellenen: `AppDbContext.cs`, `ApplicationServiceExtensions.cs`

## Oluşturulan Dosyalar (Frontend)
- `features/question-set/services/pdfPreferenceService.js`
- `features/question-set/hooks/usePdfPreferences.js`
- `features/question-set/components/PdfExport/PdfDocument.jsx`
- `features/question-set/components/PdfExport/PdfHeader.jsx`
- `features/question-set/components/PdfExport/PdfQuestion.jsx`
- `features/question-set/components/PdfExport/PdfFooter.jsx`
- `features/question-set/components/PdfExport/pdfStyles.js`
- `features/question-set/components/PdfExport/CategoryList.jsx`
- `features/question-set/components/PdfExport/SettingsPanel.jsx`
- `features/question-set/components/PdfExport/SchoolSettings.jsx`
- `features/question-set/components/PdfExport/MetaSettings.jsx`
- `features/question-set/components/PdfExport/TitleSettings.jsx`
- `features/question-set/components/PdfExport/QuestionSettings.jsx`
- `features/question-set/components/PdfExport/LayoutSettings.jsx`
- `features/question-set/components/PdfExport/ColorPicker.jsx`
- `features/question-set/components/PdfExport/AlignmentPicker.jsx`
- `features/question-set/components/PdfExport/FontSizeSlider.jsx`
- `features/question-set/pages/ExportEditPage.jsx`
- Güncellenen: `AppRoutes.jsx`, `EditQuestionSetPage.jsx`

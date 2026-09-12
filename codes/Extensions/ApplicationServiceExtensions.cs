using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using SoruHavuzu.Application.Interfaces;
using SoruHavuzu.Application.Services;
using SoruHavuzu.Application.Validators;
using SoruHavuzu.Domain.Interfaces;

namespace SoruHavuzu.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IQuestionSetService, QuestionSetService>();
        services.AddScoped<ICurriculumService, CurriculumService>();
        services.AddScoped<IGradeLessonTopicService, GradeLessonTopicService>();
        services.AddScoped<ISubTopicService, SubTopicService>();
        services.AddScoped<IUnitService, UnitService>();
        services.AddScoped<ILocationLookupService, LocationLookupService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IPageService, PageService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IPdfPreferenceService, PdfPreferenceService>();

        // Soru üretim servisi (AI + konu seçim + durum yönetimi)
        services.AddScoped<IQuestionGeneratorService, QuestionGeneratorService>();

        // Chatbot'tan elle üretilen soruların içe aktarılması (isim → ID çözümleme)
        services.AddScoped<IQuestionImportService, QuestionImportService>();
        services.AddScoped<IManualQuestionGenerationService, ManualQuestionGenerationService>();

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }
}

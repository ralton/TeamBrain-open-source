using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Infrastructure.Persistence;
using TeamBrain.Infrastructure.Services;

namespace TeamBrain.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.Configure<AzureOpenAiOptions>(configuration.GetSection("AzureOpenAi"));
        services.Configure<AzureAiSearchOptions>(configuration.GetSection("AzureAiSearch"));
        services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));
        services.Configure<GitHubOptions>(configuration.GetSection("GitHub"));

        services.AddHttpClient<GitHubService>();

        services.AddSingleton(_ =>
        {
            var endpoint = configuration["AzureOpenAi:Endpoint"]!;
            return new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
        });

        services.AddSingleton(_ =>
        {
            var endpoint = configuration["AzureAiSearch:Endpoint"]!;
            var indexName = configuration["AzureAiSearch:IndexName"] ?? "teambrain-knowledge-v1";
            return new SearchClient(new Uri(endpoint), indexName, new DefaultAzureCredential());
        });

        services.AddSingleton(_ =>
        {
            var accountUri = configuration["BlobStorage:AccountUri"]!;
            return new BlobServiceClient(new Uri(accountUri), new DefaultAzureCredential());
        });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<ISecretScannerService, SecretScannerService>();
        services.AddScoped<IAiService, AzureOpenAiService>();
        services.AddScoped<ISearchService, AzureAiSearchService>();
        services.AddScoped<IServiceBusService, ServiceBusService>();

        services.AddScoped<ICodeRepositoryService, GitHubService>();
        services.AddScoped<IWorkItemService, GitHubService>();

        return services;
    }
}

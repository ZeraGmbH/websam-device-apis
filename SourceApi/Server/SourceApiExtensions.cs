using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using ZERA.WebSam.Shared;
using Microsoft.AspNetCore.Mvc;
using SourceApi.Controllers;
using ZERA.WebSam.Shared.Provider;
using ZeraDevices.Source;
using SerialPortProxy;
using MockDevices.Source;

namespace SourceApi;

/// <summary>
/// 
/// </summary>
public static class SourceApiConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public static void UseSourceApi(this SwaggerGenOptions options)
    {
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(SourceApiConfiguration).Assembly.GetName().Name}.xml"), true);
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(PhysicalPortProxy).Assembly.GetName().Name}.xml"), true);

        SwaggerModelExtender.AddType<SourceApiErrorCodes>().Register(options);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void UseSourceApi(this IEndpointRouteBuilder app)
    {
    }

    /// <summary>
    /// Add SourceApiExceptionFilter to local scope
    /// </summary>
    public static void UseSourceApi(this MvcOptions options)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public static void UseSourceApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKeyedTransient(KeyedService.AnyKey, (ctx, key) => ctx.GetRequiredKeyedService<ISerialPortConnectionFactory>(key).Connection);

        var restMock = configuration.GetValue<string>("UseSourceRestMock");

        if (restMock == "AC")
            services.AddKeyedSingleton<ISource, ACSourceMock>(SourceRestMockController.MockKey);
        else if (restMock == "DC")
            services.AddKeyedSingleton<ISource, DCSourceMock>(SourceRestMockController.MockKey);
        else
            services.AddKeyedSingleton<ISource, UnavailableSource>(SourceRestMockController.MockKey);

        services.AddKeyedTransient<IDosage>(DosageRestMockController.MockKey, (ctx, key) => ctx.GetRequiredKeyedService<ISource>(SourceRestMockController.MockKey));
    }
}
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DotNetCoreTestWebApplicationMinimal.Models;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Exceptions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Attributes;
using Sitecore.AspNetCore.SDK.RenderingEngine.Interfaces;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;

namespace DotNetCoreTestWebApplicationMinimal.Controllers;

public class HomeController : Controller
{
    private readonly SitecoreSettings? _settings;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _settings = configuration.GetSection(SitecoreSettings.Key).Get<SitecoreSettings>();
        ArgumentNullException.ThrowIfNull(_settings);
        _logger = logger;
    }

    [UseSitecoreRendering]
    public IActionResult Index(Layout model)
    {
        IActionResult result = Empty;
        ISitecoreRenderingContext? request = HttpContext.GetSitecoreRenderingContext();
        if ((request?.Response?.HasErrors ?? false) && !IsPageEditingRequest(request))
        {
            foreach (SitecoreLayoutServiceClientException error in request.Response.Errors)
            {
                switch (error)
                {
                    case ItemNotFoundSitecoreLayoutServiceClientException:
                        result = View("NotFound");
                        break;
                    default:
                        _logger.LogError(error, "{Message}", error.Message);
                        throw error;
                }
            }
        }
        else
        {
            result = View(model);
        }

        return result;
    }

    private bool IsPageEditingRequest(ISitecoreRenderingContext request)
    {
        return request.Controller?.HttpContext.Request.Path == (_settings?.EditingPath ?? string.Empty);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

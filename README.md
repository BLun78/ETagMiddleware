# ETagMiddleware
[ETag](https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.19) Middleware for [Asp.Net Core -.NetStandard 2.0](https://docs.microsoft.com/aspnet/core/)

## Inspiration
I was inspired by Mads Kristensen 
[Blog](https://madskristensen.net/blog/send-etag-headers-in-aspnet-core/). The Example from him, can you find here as 
[code](https://gist.github.com/madskristensen/36357b1df9ddbfd123162cd4201124c4).
Thanks for your artikle!

## Requirements
.NETStandard2.0

## Install
Download from [Nuget.org](https://www.nuget.org/packages/BLun.ETagMiddleware/)
```json
"dependencies": {
  "BLun.ETagMiddleware": "<version>"
}
```

## Featurs
 - BLun.ETagMiddleware for Asp.Net Core Http
    - Microsoft.AspNetCore.Http.IMiddleware
 - BLun.ETagAttribute for Asp.Net Core Mvc
    - Microsoft.AspNetCore.Mvc.Filters.IAsyncActionFilter
 - Supports 
    - Microsoft.Extensions.Logging
    - Microsoft.Extensions.DependencyInjection
    - Microsoft.Extensions.Options (Configuration)


## Usage as Middleware (Microsoft.AspNetCore.Http.IMiddleware)

The default usage are:
```c# 
using BLun.ETagMiddleware;

// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    // Required
    // Add a Middleware for each Controller Request with
    // algorithmus          = SHA1      = default
    // etag validator       = Strong    = default
    // body content length  = 40 * 1024 = default
    services.AddETag();
}

// Add "app.UseETag();" to "Configure" method in Startup.cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseStaticFiles();
    
    // Add a Middleware for each Controller Request
    // Atention: add app.UseETag after app.UseStaticFiles, the order is important for performance
    app.UseETag();

    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}
```

You can configuratione your own option prameters:

```c# 
using BLun.ETagMiddleware;

// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    // Required
    // Add ETagOption with own global configurations
    services.AddETag(new ETagOption()
    {
        // algorithmus
        // SHA1         = default
        // SHA265
        // SHA384
        // SHA512
        // MD5
        ETagAlgorithm = ETagAlgorithm.SHA265,

        // etag validator
        // Strong       = default
        // Weak
        ETagValidator = ETagValidator.Weak,

        // body content length
        // 40 * 1024    = default
        BodyMaxLength = 20 * 1024
    });
}

// Add "app.UseETag();" to "Configure" method in Startup.cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseStaticFiles();
    
    // Add a Middleware for each Controller Request
    // Atention: add app.UseETag after app.UseStaticFiles, the order is important
    app.UseETag();

    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}
```

## Usage as ETagAttribute (Microsoft.AspNetCore.Mvc.Filters.IAsyncActionFilter)

```c# 
using BLun.ETagMiddleware;

// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    // Add ETagOption with own global configurations
    services.AddETag(new ETagOption()
    {
        // algorithmus
        // SHA1         = default
        // SHA265
        // SHA384
        // SHA512
        // MD5
        ETagAlgorithm = ETagAlgorithm.SHA265,

        // etag validator
        // Strong       = default
        // Weak
        ETagValidator = ETagValidator.Weak,

        // body content length
        // 40 * 1024    = default
        BodyMaxLength = 20 * 1024
    });
}

// Add "app.UseETag();" to "Configure" method in Startup.cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseStaticFiles();

    // Add a Middleware for each Controller Request
    // Atention: add app.UseETag after app.UseStaticFiles, the order is important
    // deactivate or combine with Middlewar
    // the EtagAttributes didn't need the Middleware
    // app.UseETag();

    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}

// Can add on controller
[ETag(ETagAlgorithm = ETagAlgorithm.SHA521)]
public class HomeController : Controller
{
    // Can add on methods
    [ETag(ETagAlgorithm = ETagAlgorithm.SHA265)]
    public IActionResult Index()
    {
        return View();
    }

    [ETag(ETagValidator = ETagValidator.Weak)]
    public IActionResult About()
    {
        ViewData["Message"] = "Your application description page.";

        return View();
    }

    [ETag(ETagValidator = ETagValidator.Strong, BodyMaxLength = 30 * 1024, ETagAlgorithm = ETagAlgorithm.SHA384)]
    public IActionResult Contact()
    {
        ViewData["Message"] = "Your contact page.";

        return View();
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
```

## Unittested
The Code is unittestet.

## Vision
I would develope muche more test for it.
Add more features for Cache-Controle-Dirictives with ETag.
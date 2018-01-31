# ETagMiddleware
(ETag)[https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.19] Middleware for (Asp.Net Core -.NetStandard 2.0)[https://docs.microsoft.com/aspnet/core/]

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

## Usage
The default usage are:
```c# 
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

You can configuratione the BodyMaxLength for the Body wich would be hashed for an ETag.
The default is 40 * 1024 = 40 KB

The default usage are:
```c# 
// Add "app.UseETag();" to "Configure" method in Startup.cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseStaticFiles();
    
    // Add a Middleware for each Controller Request
    // Atention: add app.UseETag after app.UseStaticFiles, the order is important
    app.UseETag(app, new ETagOption()
        {

            // Set the BodyMaxLength to 30 KB (Http Body)
            BodyMaxLength = 30 * 1024,
            // algorithmus
            // SHA1 = default   | strong ETag
            // SHA265           | strong ETag
            // SHA384           | strong ETag
            // SHA512           | strong ETag
            // MD5              | weak ETag
            ETagAlgorithm = ETagAlgorithm.StrongSHA1
        });

    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}
```
## Unittested
The Code is unittestet.

## Vision
I would develope muche more test for it.
Add more features for Cache-Controle-Dirictives with ETag.
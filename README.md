# ETagMiddleware
ETag Middleware for Asp.Net Core 2.0

## Inspiration
I was inspired by Mads Kristensen 
[Blog](https://madskristensen.net/blog/send-etag-headers-in-aspnet-core/). The Example from him, can you find here as 
[code](https://gist.github.com/madskristensen/36357b1df9ddbfd123162cd4201124c4).
Thanks for your artikle!

## Requirements
.NETStandard2.0

## Install
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
    // Set the BodyMaxLength to 30 KB
    app.UseETag(app, new ETagOption()
        {
            BodyMaxLength = 30 * 1024
        });

    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}
```
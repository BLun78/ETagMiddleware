# ETagMiddleware
ETag Middleware for Asp.Net Core 2.0<br>
[![Build status](https://ci.appveyor.com/api/projects/status/x4k4x820ekocml5i/branch/master?svg=true)](https://ci.appveyor.com/project/BLun78/etagmiddleware/branch/master)

## Requirements
.NETStandard2.0

## Install
```json
"dependencies": {
  "BLun.ETagMiddleware": "<version>"
}
```

## Usage
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
# Suave IIS

Set of helper functions for smooth running [Suave.io](http://suave.io) web server on Internet Information Services (IIS) without common issues like problems with routing on sub-apps, etc...


## IIS Installation

### HttpPlatformHandler vs AspNetCoreModule

To host Suave.io web application, you need to use IIS module to redirect requests from IIS to your application. There are currently two modules:

* [HttpPlaformHandler](https://www.iis.net/downloads/microsoft/httpplatformhandler) - the "official and safe" module, however not updated for more than 2 years
* [AspNetCoreModule](https://github.com/aspnet/AspNetCoreModule) - fork of HttpPlatformHandler, [not supported as standalone component](https://github.com/aspnet/AspNetCoreModule/issues/117#issuecomment-311983265), but under active development with new features added

**Which one to use?**

Since Suave.IIS v2.3.0, you can use both, so it is up to you. If you can live without new features and want to something "100% production safe", then go for HttpPlatformHandler. If you rather want to live on the edge, go for AspNetCoreModule. I will do my best to keep support for both of them as long as possible.

## Web application installation

**Please note:** Currently **only Suave 2.x.x** version is supported. If you are running on older Suave, choose [Suave.IIS v1.0.0](https://www.nuget.org/packages/Suave.IIS/1.0.0).

Ok, we got IIS ready, now let`s install Nuget package:

    Install-Package Suave.IIS

or using [Paket](http://fsprojects.github.io/Paket/getting-started.html)

    nuget Suave.IIS

To start using IIS helpers in Suave, create own filter functions based on Suave.IIS.Filters & set port to configuration using `withPort` function.

```fsharp
[<EntryPoint>]
let main argv =

    // use IIS related filter functions
    let path st = Suave.IIS.Filters.path argv st
    let pathScan format = Suave.IIS.Filters.pathScan argv format
    let pathStarts st = Suave.IIS.Filters.pathStarts argv st

    // routes
    let webpart =
    	choose [
            pathStarts "/st" >=> OK "Path starts with '/st'"
            path "/test" >=> OK "Look ma! Routing on sub-app on localhost"
            path "/" >=> OK "Hello from Suave on IIS"
        ]

    // start service server
    let config = { defaultConfig with bindings=[HttpBinding.create HTTP IPAddress.Any 8083us]; } |> Suave.IIS.Configuration.withPort argv
    startWebServer config webpart
    0

```

The last thing we need for proper run on IIS is `web.config`

**Using HttpPlatformHandler**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <remove name="httpplatformhandler" />
      <add name="httpplatformhandler" path="*" verb="*" modules="httpPlatformHandler" resourceType="Unspecified"/>
    </handlers>
    <httpPlatform
        forwardWindowsAuthToken="true"
        stdoutLogEnabled="true"
        stdoutLogFile="myiiswebname.log"
        startupTimeLimit="20"
        processPath="C:\inetpub\wwwroot\myiiswebname\myiiswebname.exe"
        arguments="%HTTP_PLATFORM_PORT% &quot;myiiswebname&quot;"/>
  <!-- now running on http://localhost/myiiswebname -->
  </system.webServer>
</configuration>
```

**Using AspNetCoreModule**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModule" resourceType="Unspecified" />
    </handlers>
    <aspNetCore
	forwardWindowsAuthToken="true"
        startupTimeLimit="20"
        stdoutLogEnabled="true"
        stdoutLogFile="myiiswebname.log"
        processPath="C:\inetpub\wwwroot\myiiswebname\myiiswebname.exe"
        arguments="%ASPNETCORE_PORT% &quot;myiiswebname&quot;">
	<!-- if running on http://localhost/myiiswebname -->
    </aspNetCore>
  </system.webServer>
</configuration>
```

## IIS Application

Now create new web application on IIS:

![IIS new web app](./docs/iis_newapp.png)

Copy all your build files into `C:\inetpub\wwwroot\myiiswebname` and navigate to `http://localhost/myiiswebname`. You should see your web application output now.

## IIS Site

If you need to run Suave application as Site (on default port 80 or any other port), just **omit second parameter** in arguments attribute of httpPlatformHandler section in `web.config` file:

**Using HttpPlatformHandler**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <remove name="httpplatformhandler" />
      <add name="httpplatformhandler" path="*" verb="*" modules="httpPlatformHandler" resourceType="Unspecified"/>
    </handlers>
    <httpPlatform
        forwardWindowsAuthToken="true"
        stdoutLogEnabled="true"
        stdoutLogFile="myiiswebname.log"
        startupTimeLimit="20"
        processPath="C:\inetpub\wwwroot\myiiswebname\myiiswebname.exe"
        arguments="%HTTP_PLATFORM_PORT%"/>
  <!-- now running on http://localhost/ -->
  </system.webServer>
</configuration>
```

**Using AspNetCoreModule**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModule" resourceType="Unspecified" />
    </handlers>
    <aspNetCore
	forwardWindowsAuthToken="true"
        startupTimeLimit="20"
        stdoutLogEnabled="true"
        stdoutLogFile="myiiswebname.log"
	    processPath="C:\inetpub\wwwroot\myiiswebname\myiiswebname.exe"
        arguments="%ASPNETCORE_PORT%">
	<!-- now running on http://localhost/ -->
    </aspNetCore>
  </system.webServer>
</configuration>
```

## Suave.IIS on .NET Core

All the previous things are exactly the same (for both IIS Site & IIS Application), but there is little change in `web.config` file.

For IIS Site:

```xml
processPath="dotnet"
arguments="C:\inetpub\wwwroot\myiiswebname\myiiswebname.exe %ASPNETCORE_PORT%"
```

or IIS Application:

```xml
processPath="dotnet"
arguments="C:\inetpub\wwwroot\myiiswebname\myiiswebname.exe %ASPNETCORE_PORT% &quot;myiiswebname&quot;"
```

## Good to know

1. Maybe you didn\`t notice, but using this library, you can still run Suave locally (from Visual Studio hitting F5 or FAKE script) - it there are no command line arguments, default setup is used, so you don\`t need to change anything. Just use `withPort` and create custom filter functions based on `Suave.IIS.Configuration`.
2. Only few (three, actually :)) filter functions are wrapped in `Suave.IIS.Filters`. If you need more of them, please feel free to send PR.


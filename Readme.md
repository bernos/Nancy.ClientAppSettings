Provides a simple API that makes sharing configuration data between your Nancy application on the server and your javascript in the browser simple.

## Features

- Easily expose specific appsettings from web.config to javascript
- Push dynamically generated configuration parameters down to the client from the server at runtime

## Installation

With Nuget

```
Install-Package Nancy.ClientAppSettings
```

Normally you will want to install this package as a dependency for the Nancy.ClientAppSettings.Razor package, which adds HtmlHelper extensions to actually expose settings in your views.

```
Install-Package Nancy.ClientAppSettings.Razor
```

## Usage

### Bootstrapping

Enable Nancy.ClientAppSettings in your application bootstrappers ApplicationStartup method

```csharp
protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
{
	base.ApplicationStartup(container, pipelines);
        
	ClientAppSettings.Enable(pipelines);
}
```

The Enable method returns an instance of the ClientAppSettings object, which you can further configure in your bootstrapper. Read on...

### Accessing settings from outside of the bootstrapper

You can easily retrieve the ClientAppSettings object using the GetClientAppSettings extension method added to NancyContext. 

```csharp
var settings = Context.GetClientAppSettings();
```

### Providing access to Web.config app settings to javascript

You can easily push appsettings from your web.config file down to javascript using the fluent api. For example, in your bootstrapper...

```csharp
protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
{
	base.ApplicationStartup(container, pipelines);
        
	ClientAppSettings.Enable(pipelines).WithAppSettings("MyAppSettingOne", "MyOtherAppSetting" ...);
}
```

Or from within a route handler in a Nancy Module

```csharp
public MyModule : NancyModule {
	
	Get["/"] = _ => {

		Context.GetClientAppSettings().WithAppSettings("SomeSetting");

		//...
	};
}
```

### Setting and appending static and dynamic app settings

You can easily set a single app setting 

```csharp
Context.GetClientAppSettings().Set("MySetting", "MyValue");
```

Or append multiple settings using the Append(IDictionary<string, object> settings) method

```csharp
Context.GetClientAppSettings().Append(new Dictionary<string, object> {
	{ "SettingOne" , 1 },
	{ "SettingTwo", 2 }
});
```

You can also append dynamic values that can be calculated at runtime, using the Append(Func<NancyContext, IDictionary<string, object>> appenderFunc) overload. The appenderFunc will not be evaluated until the ClientAppSettings object is serialized, usually during view rendering

```csharp
Context.GetClientAppSettings().Append(context => {
	return new Dictionary<string, object> {
		{ "CurrentUser", context.CurrentUser } 
	};
});
```

### Outputting settings in your views 

Obviously, setting javascript variables from the server is no use if we can't output them from within our views. Currently the Razor View engine is supported via the separate Nancy.ClientAppSettings.Razor nuget package.

This package will add some HtmlHelper extensions to make it easy to output your app settings to javascript.

The simplest method is via

```csharp
@Html.RenderClientAppSettings()
```

This will render out the javasript app settings as well as enclosing `<script>` element:

```html
<script>
	var Settings = {
		"SettingOne" : "ValueOne",
		"SettingTwo" : "ValueTwo"
		//...
	};
</script>
```

By default app settings are output as a global variable named "Settings". You can easily change the name of the javascript variable using the WithVariableName() method in your bootstrapper

```csharp
protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
{
	base.ApplicationStartup(container, pipelines);
        
	ClientAppSettings.Enable(pipelines)
		.WithVariableName("AppSettings")
		.WithAppSettings("MyAppSettingOne", "MyOtherAppSetting" ...);
}
```

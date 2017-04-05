MvcIntegrationTestFramework
===========================

Integration test harness for ASP.Net MVC 5. Allows you to fully integration test an MVC web project without needing to host under IIS or similar. Allows access to both server state and client responses in a single assertion.

![Batteries included](https://raw.githubusercontent.com/i-e-b/MvcIntegrationTestFramework/master/batteries_small.png)
https://www.nuget.org/packages/MvcIntegrationTestFramework

This fork targets MVC 5, VS2015.

Everything should just work out of the box, no need for post build steps.

Usage
----------

In your test set-up start a new AppHost targeting the folder containing your MVC application:

```csharp
	this.appHost = AppHost.Simulate("MyMvcApplication");
```

Then for each test flow, start a browsing session, make your calls and assert against the results:

```csharp
	this.appHost.Start(browsingSession =>
	{
		// Request the root URL
		RequestResult result = browsingSession.Get("/welcome");

		// Check the result status
		Assert.That(result.IsSuccess);

		// Make assertions about the ActionResult
		var viewResult = (ViewResult)result.ActionExecutedContext.Result;
		Assert.AreEqual("Index", viewResult.ViewName);
		Assert.AreEqual("Welcome to ASP.NET MVC!", viewResult.ViewData["Message"]);

		// Or make assertions about the rendered HTML
		Assert.IsTrue(result.ResponseText.Contains("<!DOCTYPE html"));
	});
```

See the `MyMvcApplication.Tests` project and the `HomeControllerTests.cs` file for more examples.

Known issues
============

Simple Injector
---------------

Simple Injector's `[assembly: WebActivator.PostApplicationStartMethod(...)]` injection causes problems with the ASP.Net hosting enviroment.
You can remove the assembly level injector and call your setup from `Global.aspx` to solve this.


System.Web.Optimization
------------------------

The Bundle provider for System.Web.Optimization can cause issues when running tests.
To work around this, call `BundleTable.VirtualPathProvider = new TestVPP();` before you create your bundles when testing, with `TestVPP` defined as:

```csharp
    public class TestVPP : VirtualPathProvider
    {
        public override bool FileExists(string virtualPath)
        {
            Console.WriteLine(virtualPath);
            return true;
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            return new DummyVirtualFile(virtualPath);
        }
    }

    public class DummyVirtualFile: VirtualFile
    {
        public DummyVirtualFile(string virtualPath) : base(virtualPath)
        {
        }

        public override Stream Open()
        {
            return new MemoryStream();
        }
    }
```

This fix will hopefully be rolled into the next minor version.

[Icon via game-icones.net](http://game-icons.net/lorc/originals/batteries.html) 

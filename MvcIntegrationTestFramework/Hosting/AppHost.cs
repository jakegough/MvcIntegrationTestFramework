using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using MvcIntegrationTestFramework.Browsing;
using MvcIntegrationTestFramework.Interception;

namespace MvcIntegrationTestFramework.Hosting
{
    /// <summary>
    /// Hosts an ASP.NET application within an ASP.NET-enabled .NET appdomain
    /// and provides methods for executing test code within that appdomain
    /// </summary>
    public class AppHost
    {
        private readonly AppDomainProxy _appDomainProxy; // The gateway to the ASP.NET-enabled .NET appdomain

        private AppHost(string appPhysicalDirectory, string virtualDirectory)
        {
            _appDomainProxy = (AppDomainProxy)ApplicationHost.CreateApplicationHost(typeof(AppDomainProxy), virtualDirectory, appPhysicalDirectory);
            _appDomainProxy.RunCodeInAppDomain(() =>
            {
                InitializeApplication();
                FilterProviders.Providers.Add(new InterceptionFilterProvider());
                LastRequestData.Reset();
            });
        }

        /// <summary>
        /// Run a set of test actions in the ASP.Net application domain.
        /// BrowsingSession object is connected to the MVC project supplied to the `Simulate` method.
        /// </summary>
        public void Start(Action<BrowsingSession> testScript)
        {
            var serializableDelegate = new SerializableDelegate<Action<BrowsingSession>>(testScript);
            _appDomainProxy.RunBrowsingSessionInAppDomain(serializableDelegate);
        }

        /// <summary>
        /// Creates an instance of the AppHost so it can be used to simulate a browsing session.
        /// Use the `Start` method on the returned AppHost to communicate with the MVC host.
        /// </summary>
        /// <param name="mvcProjectDirectory">Directory containing the MVC project, relative to the solution base path</param>
        public static AppHost Simulate(string mvcProjectDirectory)
        {
            var caller = Assembly.GetCallingAssembly();
            var mvcProjectPath = GetMvcProjectPath(mvcProjectDirectory);
            if (mvcProjectPath == null)
            {
                throw new ArgumentException("The MVC Project '" + mvcProjectDirectory + "' was not found when searching from '" + AppDomain.CurrentDomain.BaseDirectory + "'");
            }
            CopyDllFiles(mvcProjectPath, caller.Location);
            return new AppHost(mvcProjectPath, "/");
        }


        private static void InitializeApplication()
        {
            BundleTable.VirtualPathProvider = new TestVPP();

            var appInstance = GetApplicationInstance();
            appInstance.PostRequestHandlerExecute += delegate
            {
                // Collect references to context objects that would otherwise be lost
                // when the request is completed
                if (LastRequestData.HttpSessionState == null)
                    LastRequestData.HttpSessionState = HttpContext.Current.Session;
                if (LastRequestData.Response == null)
                    LastRequestData.Response = HttpContext.Current.Response;
            };
            RefreshEventsList(appInstance);

            RecycleApplicationInstance(appInstance);
        }

        private static readonly MethodInfo GetApplicationInstanceMethod;
        private static readonly MethodInfo RecycleApplicationInstanceMethod;

        static AppHost()
        {
            // Get references to some MethodInfos we'll need to use later to bypass nonpublic access restrictions
            var httpApplicationFactory = typeof(HttpContext).Assembly.GetType("System.Web.HttpApplicationFactory", true);
            GetApplicationInstanceMethod = httpApplicationFactory.GetMethod("GetApplicationInstance", BindingFlags.Static | BindingFlags.NonPublic);
            RecycleApplicationInstanceMethod = httpApplicationFactory.GetMethod("RecycleApplicationInstance", BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static HttpApplication GetApplicationInstance()
        {
            var writer = new StringWriter();
            var workerRequest = new SimpleWorkerRequest("", "", writer);
            var httpContext = new HttpContext(workerRequest);
            httpContext.User = null;

            // This can fail with "BuildManager.EnsureTopLevelFilesCompiled This method cannot be called during the application's pre-start initialization phase"
            //   at System.Web.Compilation.BuildManager.EnsureTopLevelFilesCompiled()
            //at System.Web.Compilation.BuildManager.GetGlobalAsaxTypeInternal()
            //at System.Web.HttpApplicationFactory.CompileApplication()
            //at System.Web.HttpApplicationFactory.Init()
            //at System.Web.HttpApplicationFactory.EnsureInited()
            //at System.Web.HttpApplicationFactory.GetApplicationInstance(HttpContext context)

            // I've seen this with SimpleInjector's
            // [assembly: WebActivator.PostApplicationStartMethod(...)]
            // start-up code. Removing this fixes the error.

            return (HttpApplication)GetApplicationInstanceMethod.Invoke(null, new object[] { httpContext });
        }

        private static void RecycleApplicationInstance(HttpApplication appInstance)
        {
            RecycleApplicationInstanceMethod.Invoke(null, new object[] { appInstance });
        }

        private static void RefreshEventsList(HttpApplication appInstance)
        {
            var stepManagerField = typeof(HttpApplication).GetField("_stepManager", BindingFlags.NonPublic | BindingFlags.Instance);
            var resumeStepsWaitCallbackField = typeof(HttpApplication).GetField("_resumeStepsWaitCallback", BindingFlags.NonPublic | BindingFlags.Instance);

            if (stepManagerField == null || resumeStepsWaitCallbackField == null) throw new Exception("Expected fields were not present on HttpApplication type. This version of MvcTestIntegrationFramework may not be suitable for your project.");

            var stepManager = stepManagerField.GetValue(appInstance);
            var resumeStepsWaitCallback = resumeStepsWaitCallbackField.GetValue(appInstance);
            var buildStepsMethod = stepManager.GetType().GetMethod("BuildSteps", BindingFlags.NonPublic | BindingFlags.Instance);
            buildStepsMethod.Invoke(stepManager, new[] { resumeStepsWaitCallback });
        }

        /// <summary>
        /// Copy the test files into the MVC project path
        /// </summary>
        private static void CopyDllFiles(string mvcProjectPath, string callerLocation)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var callerFile = Path.GetFileName(callerLocation);

            foreach (var file in Directory.GetFiles(baseDirectory, "*.dll"))
            {
                var fileName = Path.GetFileName(file) ?? "";
                var destFile = Path.Combine(mvcProjectPath, "bin", fileName);

                if (fileName == "MvcIntegrationTestFramework.dll" // update the test assembly
                    || fileName == callerFile) // bring tests along
                {
                    if (!File.Exists(destFile) || File.GetCreationTimeUtc(destFile) != File.GetCreationTimeUtc(file))
                        File.Copy(file, destFile, true);
                }
            }
        }

        private static string GetMvcProjectPath(string mvcProjectName)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            while (baseDirectory.Contains("\\"))
            {
                baseDirectory = baseDirectory.Substring(0, baseDirectory.LastIndexOf("\\", StringComparison.Ordinal));
                var mvcPath = Path.Combine(baseDirectory, mvcProjectName);
                if (Directory.Exists(mvcPath))
                {
                    return mvcPath;
                }
            }
            return null;
        }
    }
}
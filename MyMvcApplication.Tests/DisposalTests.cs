using System;
using MvcIntegrationTestFramework.Hosting;
using NUnit.Framework;

namespace MyMvcApplication.Tests
{
    [TestFixture]
    public class DisposalTests
    {
        [Test]
        public void an_app_host_can_be_used_inside_a_using_block()
        {
            AppHost.LoadAllBinaries = false;
            using (var appHost = AppHost.Simulate("MyMvcApplication"))
            {
                appHost.Start(browsingSession =>
                {
                    browsingSession.Get("");
                });
            }
        }

        [Test]
        public void an_app_host_can_be_disposed()
        {
            AppHost.LoadAllBinaries = false;
            var appHost = AppHost.Simulate("MyMvcApplication");
            
            appHost.Start(s => {  });
            appHost.Dispose();
        }

        [Test]
        public void an_app_host_can_be_disposed_and_then_a_new_one_started()
        {
            AppHost.LoadAllBinaries = false;
            var appHost_1 = AppHost.Simulate("MyMvcApplication");
            appHost_1.Dispose();
            var appHost_2 = AppHost.Simulate("MyMvcApplication");
            appHost_2.Start(s => {  });
            appHost_2.Dispose();
        }

        [Test]
        public void there_is_an_exception_thrown_if_using_the_host_after_disposal()
        {
            AppHost.LoadAllBinaries = false;
            var appHost = AppHost.Simulate("MyMvcApplication");
            appHost.Dispose();
            try
            {
                appHost.Start(browsingSession =>
                {
                    // Request the root URL
                    var result = browsingSession.Get("");
                    Assert.IsNotNull(result);
                });
                Assert.Fail("No exception thrown");
            }
            catch (AppDomainUnloadedException ex)
            {
                Assert.That(ex.Message, Does.StartWith("Attempted to access an unloaded AppDomain"));
            }
            catch
            {
                Assert.Fail("Unexpected exception");
            }
        }

    }
}
using System;
using MvcIntegrationTestFramework.Hosting;
using NUnit.Framework;

namespace MyMvcApplication.Tests
{
    [TestFixture]
    public class MvcFindingTests
    {
        [Test]
        public void can_find_an_mvc_project_with_a_known_path()
        {
            try
            {
                var appHost = AppHost.Simulate("MyMvcApplication");
                Assert.That(appHost, Is.Not.Null);
            }
            catch (Exception e)
            {
                Assert.Fail("Creation failed: " + e.Message);
            }
        }

        [Test]
        public void attempting_to_start_a_project_with_a_path_that_cant_be_found_gives_an_exception()
        {
            try
            {
                AppHost.Simulate("ThisShouldNotExistOnYourSystem");
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.StringStarting("The MVC Project 'ThisShouldNotExistOnYourSystem' was not found when searching from '"));
                return;
            }
            Assert.Fail("AppHost did not throw an exception");
        }

    }
}
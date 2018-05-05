using System;
using System.IO;
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
            var x = Path.Combine("E:", "_what");
            var y = Directory.Exists(x);
            try
            {
		        AppHost.LoadAllBinaries = true;
                var appHost = AppHost.Simulate("MyMvcApplication");
                Assert.That(appHost, Is.Not.Null);
            }
            catch (Exception e)
            {
                Assert.Fail("Creation failed: " + e.Message);
            }
        }

        [Test]
        public void should_be_able_to_find_the_first_match_of_a_set()
        {
            // Just because TFS build is so very wrong.
            try
            {
		        AppHost.LoadAllBinaries = true;
                var appHost = AppHost.Simulate("_wrong_invalid_junk", "MyMvcApplication", "gunk_funk_ignore_me");
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
		        AppHost.LoadAllBinaries = true;
                AppHost.Simulate("ThisShouldNotExistOnYourSystem");
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Does.StartWith("The MVC Projects 'ThisShouldNotExistOnYourSystem' were not found when searching from '"));
                return;
            }
            Assert.Fail("AppHost did not throw an exception");
        }
        /*
        [Test]
        public void can_target_a_folder_directly_under_the_test_framework()
        {
            try
            {
		        AppHost.LoadAllBinaries = true;
                var appHost = AppHost.Simulate("TargetFolder");
                Assert.That(appHost, Is.Not.Null);
            }
            catch (System.Reflection.TargetInvocationException)
            {
                Assert.Pass("Folder was found. There is no project here to be run.");
            }
            catch (Exception e)
            {
                Assert.Fail("Creation failed: " + e.Message);
            }

        }*/

    }
}
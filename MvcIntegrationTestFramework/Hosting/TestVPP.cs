using System.Web.Hosting;

namespace MvcIntegrationTestFramework.Hosting
{
    public class TestVPP : VirtualPathProvider
    {
        public override bool FileExists(string virtualPath) { return true; }
        public override bool DirectoryExists(string virtualDir) { return true; }

        public override VirtualFile GetFile(string virtualPath)
        {
            return new DummyVirtualFile(virtualPath);
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            return new DummyVirtualDirectory(virtualDir);
        }
    }
}
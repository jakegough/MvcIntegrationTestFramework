using System.IO;
using System.Text;
using System.Web.Hosting;

namespace MvcIntegrationTestFramework.Hosting
{
    public class DummyVirtualFile: VirtualFile
    {
        public DummyVirtualFile(string virtualPath) : base(virtualPath)
        {
        }

        public override Stream Open()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?><bundles></bundles>"));
        }
    }
}
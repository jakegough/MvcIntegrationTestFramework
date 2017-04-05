using System.Collections;
using System.Web.Hosting;

namespace MvcIntegrationTestFramework.Hosting
{
    public class DummyVirtualDirectory : VirtualDirectory
    {
        public DummyVirtualDirectory(string virtualDir) : base(virtualDir) { }
        public override IEnumerable Directories { get { yield break; } }
        public override IEnumerable Files { get { yield break; } }
        public override IEnumerable Children { get { yield break; } }
    }
}
using System.Collections.Generic;
using System.Web.Mvc;

namespace MvcIntegrationTestFramework.Interception
{
    /// <summary>
    /// A provider used to inject `InterceptionFilter` into the aspnet MVC lifecycle
    /// </summary>
    internal class InterceptionFilterProvider : IFilterProvider
    {
        /// <inheritdoc />
        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            yield return new Filter(new InterceptionFilter(), FilterScope.Action, null);
        }
    }
}
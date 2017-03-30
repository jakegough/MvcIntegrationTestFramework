using System;
using System.Collections.Specialized;
using System.Web.Routing;

namespace MvcIntegrationTestFramework
{
    public static class NameValueCollectionConversions
    {
        public static NameValueCollection ConvertFromObject(object anonymous)
        {
            var nvc = new NameValueCollection();

            foreach (var kvp in new RouteValueDictionary(anonymous))
            {
                if (kvp.Value == null)
                {
                    throw new NullReferenceException(kvp.Key);
                }
                if (kvp.Value.GetType().Name.Contains("Anonymous"))
                {
                    var prefix = kvp.Key + ".";
                    foreach (var innerkvp in new RouteValueDictionary(kvp.Value))
                    {
                        nvc.Add(prefix + innerkvp.Key, innerkvp.Value.ToString());
                    }
                }
                else
                {
                    nvc.Add(kvp.Key, kvp.Value.ToString());
                }


            }
            return nvc;
        }
    }
}

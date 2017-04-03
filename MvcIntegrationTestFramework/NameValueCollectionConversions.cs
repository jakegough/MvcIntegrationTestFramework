using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace MvcIntegrationTestFramework
{
    public static class NameValueCollectionConversions
    {
        /// <summary>
        /// Serialise a NameValueCollection into a UTF8 byte string of URL encoded values
        /// </summary>
        public static byte[] SerialiseFormData(NameValueCollection formData)
        {
            var sb = new StringBuilder();
            foreach (string key in formData)
                sb.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(formData[key]));
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        /// <summary>
        /// Convert the field names and values from an anonymously typed object to a NaveValueCollection
        /// </summary>
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

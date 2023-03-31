using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GeoPunt.Share
{
    class gpWebClient : WebClient
    {
        public int timeout { get; set; }

        public gpWebClient(Uri uri, int millisecTimeOut)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var objWebClient = GetWebRequest(uri);
            timeout = millisecTimeOut;
        }
        public gpWebClient(Uri uri)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var objWebClient = GetWebRequest(uri);
            timeout = 5000;
        }
        public gpWebClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            timeout = 5000;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            var objWebRequest = base.GetWebRequest(uri);
            objWebRequest.Timeout = this.timeout;
            return objWebRequest;
        }
    }
}

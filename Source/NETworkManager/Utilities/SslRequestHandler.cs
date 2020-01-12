using System.Threading.Tasks;
using CefSharp.Handler;
using CefSharp;


namespace NETworkManager.Utilities
{
    public class SslRequestHandler : RequestHandler
    {
        protected override bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            Task.Run(() =>
            {

                if (!callback.IsDisposed)
                {
                    using (callback)
                    {
                        callback.Continue(true);

                        /*
                        //We'll allow the expired certificate from badssl.com
                        if (requestUrl.ToLower().Contains("https://expired.badssl.com/"))
                        {
                            callback.Continue(true);
                        }
                        else
                        {
                            callback.Continue(false);
                        }
                        */
                    }
                }
            });

            return true;
        }
    }
}
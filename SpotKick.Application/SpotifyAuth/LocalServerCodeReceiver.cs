using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotKick.Application.Exceptions;

namespace SpotKick.Application.SpotifyAuth
{
    /// <summary>
    /// OAuth 2.0 verification code receiver that runs a local server on a free port and waits for a call with the 
    /// authorization verification code.
    /// Stolen from https://github.com/googleapis/google-api-dotnet-client/blob/1dce119eb5200be57a6ff24159b895594d6bd558/Src/Support/GoogleApis.Auth.DotNet4/OAuth2/LocalServerCodeReceiver.cs
    /// </summary>
    public static class LocalServerCodeReceiver
    {
        /// <summary>Close HTML tag to return the browser so it will close itself.</summary>
        private const string ClosePageResponse =
            @"<html>
              <head><title>OAuth 2.0 Authentication Token Received</title></head>
              <body>
                Received verification code. You may now close this window.
                <script type='text/javascript'>
                  // This doesn't work on every browser.
                  window.setTimeout(function() {
                      window.open('', '_self', ''); 
                      window.close(); 
                    }, 1000);
                  if (window.opener) { window.opener.checkToken(); }
                </script>
              </body>
            </html>";

        public static async Task<string> ReceiveCodeAsync(string authorisationUrl, string redirectUrl, string state)
        {
            using var listener = new HttpListener();
            listener.Prefixes.Add(redirectUrl);
            try
            {
                listener.Start();

                var process = new Process {StartInfo = {UseShellExecute = true, FileName = authorisationUrl}};
                process.Start();

                // Wait to get the authorization code response.
                var context = await listener.GetContextAsync();
                var coll = context.Request.QueryString;

                // Write a "close" response.
                await using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.WriteLine(ClosePageResponse);
                    writer.Flush();
                }

                if (coll["state"] != state)
                    throw new SpotifyAuthException("The returned state was invalid");

                return coll["code"];
            }
            finally
            {
                Thread.Sleep(500);
                listener.Close();
            }
        }
    }
}
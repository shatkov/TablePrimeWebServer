using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace TablePrimeWebServer
{
    // Реализация сервера взята с https://codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        internal WebServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, string> method)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");

            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (prefixes == null || prefixes.Count == 0)
                throw new ArgumentException("prefixes");
            foreach (var s in prefixes)
                _listener.Prefixes.Add(s);

            _responderMethod = method ?? throw new ArgumentException("method");
            _listener.Start();
        }

        internal WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
            : this(prefixes, method) { }

        internal void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                WriteMessage("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                if (ctx == null)
                                {
                                    return;
                                }

                                var rstr = _responderMethod(ctx.Request);
                                var buf = Encoding.UTF8.GetBytes(rstr);

                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch (Exception ex)
                            {
                                WriteError(ex.Message);
                            }
                            finally
                            {
                                // always close the stream
                                ctx?.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch (Exception ex)
                {
                    WriteError(ex.Message);
                }
            });
        }
        internal void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }

        private static void WriteError(string errorMessage)
        {
            Console.WriteLine("Error: " + errorMessage);
        }

        private static void WriteMessage(string errorMessage)
        {
            Console.WriteLine(errorMessage);
        }
    }
}
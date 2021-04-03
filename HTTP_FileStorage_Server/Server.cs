using System;
using System.Net.Sockets;
using System.Net.Http;
using System.Net;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
namespace HTTP_FileStorage_Server
{
    class Server
    {
        public static bool PortInUse(int port)
        {
            bool inUse = false;
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }
        static string StorageFolder = @"D:\University\2course\4semester\labs\KSiS\laba3\HTTP_FileStorage\Storage";

        static void Main(string[] args)
        {
            Console.WriteLine("Available ip addresses:");
            List<string> adresses = new List<string>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            Console.WriteLine(ip.Address.ToString());
                            adresses.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            Console.WriteLine("Select preferable ip");
            while (true)
            {
                string message = Console.ReadLine();
                if (!adresses.Contains(message))
                {
                    Console.WriteLine("Invalid ip, repeat input");
                }
                else
                {
                    ip = message;
                    break;
                }
            }

            Console.WriteLine("Select preferable port");
            while (true)
            {
                string message = Console.ReadLine();
                if (PortInUse(Int32.Parse(message)))
                {
                    Console.WriteLine("Port in use");
                }
                else if (Int32.Parse(message) < 1024)
                {
                    Console.WriteLine("Select port greater than 1024");
                }
                else
                {
                    port = message;
                    break;
                }
            }
            Console.WriteLine("Select folder or Enter for default");
            while (true)
            {
                string message = Console.ReadLine();
                if (message == "")
                {
                    break;
                }
                else if (Directory.Exists(message))
                {
                    StorageFolder = message;
                    break;
                }
                else
                {
                    Console.WriteLine("Folder not found");
                }
            }
            Thread thread = new Thread(StartListening);
            thread.Start();
        }
        static string port;
        static string ip;
        private static void StartListening()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://"+ip+":"+port+"/");
            listener.Start();
            Console.WriteLine("Listening...");
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                HttpListenerResponse response = context.Response;
                switch (request.HttpMethod)
                {
                    case "GET":
                        PerformGET(request, response);
                        break;
                    case "PUT":
                        PerformPUT(request, response);
                        break;
                    case "POST":
                        PerformPOST(request, response);
                        break;
                    case "COPY":
                        PerformCOPY(request, response);
                        break;
                    case "MOVE":
                        PerformMOVE(request, response);
                        break;
                    case "DELETE":
                        PerformDELETE(request, response);
                        break;
                    default:
                        response.StatusCode = (int)HttpStatusCode.NotImplemented;
                        response.OutputStream.Close();
                        break;
                }
            }
        }
        static void PerformGET(HttpListenerRequest request,HttpListenerResponse response) {
            try
            {
                if (File.Exists(StorageFolder + (request.RawUrl).Replace("/", "\\")))
                {
                    byte[] file = File.ReadAllBytes(StorageFolder + (request.RawUrl).Replace("/", "\\"));
                    response.ContentLength64 = file.Length;
                    response.SendChunked = false;
                    response.OutputStream.Write(file, 0, file.Length);
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.StatusDescription = "OK";
                    response.OutputStream.Close();
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusDescription = "Not found";
                    response.OutputStream.Close();
                }
            }
            catch
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.StatusDescription = "MethodNotAllowed";
                response.OutputStream.Close();
            }
        }
        static void PerformPUT(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                if (Directory.Exists(StorageFolder + (request.RawUrl).Replace("/", "\\")))
                {

                    byte[] file;
                    using (var stream = new MemoryStream())
                    {
                        request.InputStream.CopyTo(stream);
                        file = stream.ToArray();
                    }
                    File.WriteAllBytes(StorageFolder + (request.RawUrl).Replace("/", "\\") + "\\" + request.Headers["FileName"], file);
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.StatusDescription = "OK";
                    response.OutputStream.Close();
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusDescription = "Not found";
                    response.OutputStream.Close();
                }
            }
            catch
            {

                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.StatusDescription = "MethodNotAllowed";
                response.OutputStream.Close();
            }
        }
        static void PerformPOST(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                if (Directory.Exists(StorageFolder + (request.RawUrl).Replace("/", "\\")))
                {

                    byte[] file;
                    using (var stream = new MemoryStream())
                    {
                        request.InputStream.CopyTo(stream);
                        file = stream.ToArray();
                    }
                    using (var stream = new FileStream(StorageFolder + (request.RawUrl).Replace("/", "\\") + "\\" + request.Headers["FileName"], FileMode.Append))
                    {
                        stream.Write(file, 0, file.Length);
                    }
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.StatusDescription = "OK";
                    response.OutputStream.Close();
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusDescription = "Not found";
                    response.OutputStream.Close();
                }
            }
            catch 
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.StatusDescription = "MethodNotAllowed";
                response.OutputStream.Close();
            }
        }
        static void PerformDELETE(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                if (File.Exists(StorageFolder + (request.RawUrl).Replace("/", "\\")))
                {
                    File.Delete(StorageFolder + (request.RawUrl).Replace("/", "\\"));
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.StatusDescription = "OK";
                    response.OutputStream.Close();
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusDescription = "Not found";
                    response.OutputStream.Close();
                }
            }
            catch 
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.StatusDescription = "MethodNotAllowed";
                response.OutputStream.Close();
            }
        }
        static void PerformCOPY(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                if ((File.Exists(StorageFolder + (request.RawUrl).Replace("/", "\\")))
        & (Directory.Exists(StorageFolder + "\\" + (request.Headers["Pathto"]).Replace("/", "\\"))))
                {
                    File.Copy(StorageFolder + (request.RawUrl).Replace("/", "\\"),
                        StorageFolder + "\\" + (request.Headers["Pathto"]).Replace("/", "\\") + "\\"
                        + Path.GetFileName(StorageFolder + (request.RawUrl).Replace("/", "\\"))
                        );
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.StatusDescription = "OK";
                    response.OutputStream.Close();
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusDescription = "Not found";
                    response.OutputStream.Close();
                }
            }
            catch 
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.StatusDescription = "MethodNotAllowed";
                response.OutputStream.Close();
            }
        }
        static void PerformMOVE(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                if ((File.Exists(StorageFolder + (request.RawUrl).Replace("/", "\\")))
                    & (Directory.Exists(StorageFolder + "\\" + (request.Headers["Pathto"]).Replace("/", "\\"))))
                {
                    File.Move(StorageFolder + (request.RawUrl).Replace("/", "\\"),
                        StorageFolder + "\\" + (request.Headers["Pathto"]).Replace("/", "\\") + "\\"
                        + Path.GetFileName(StorageFolder + (request.RawUrl).Replace("/", "\\"))
                        );
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.StatusDescription = "OK";
                    response.OutputStream.Close();
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusDescription = "Not found";
                    response.OutputStream.Close();
                }
            }
            catch 
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.StatusDescription = "MethodNotAllowed";
                response.OutputStream.Close();
            }
        }

    }
}

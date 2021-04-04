using System;
using System.Net.Http;
using System.Net;
using System.Xml.Linq;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using System.IO;
using System.Text;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace HTTP_FileStorage_Client
{
    class Client
    {
        static string ServerUrl = "http://127.0.0.1:8080";
        static void Main(string[] args)
        {
            Console.WriteLine("Enter server ip:");
            string address = Console.ReadLine();
            Console.WriteLine("Enter server port:");
            string port = Console.ReadLine();
            ServerUrl = "http://"+address+":"+port+"/";
            while (true)
            {
                string request = Console.ReadLine();
                string[] parameters= request.Split();
                if (parameters.Length > 0)
                {
                    switch (parameters[0])
                    {
                        case "GET":
                            if (parameters.Length == 3)
                            {
                                GET(parameters[1], parameters[2]);
                            }
                            else Console.WriteLine("Invalide parameters,syntax is GET /serverfolder/file.ext /clientfolder");
                            break;
                        case "PUT":
                            if (parameters.Length == 3)
                            {
                                PUT(parameters[1], parameters[2]);
                            }
                            else Console.WriteLine("Invalide parameters,syntax is PUT /serverfolder /clientfolder/file.ext");
                            break;
                        case "POST":
                            if (parameters.Length == 3)
                            {
                                POST(parameters[1], parameters[2]);
                            }
                            else Console.WriteLine("Invalide parameters,syntax is POST /serverfolder /clientfolder/file.ext");
                            break;
                        case "COPY":
                            if (parameters.Length == 3)
                            {
                                COPY(parameters[1], parameters[2]);
                            }
                            else Console.WriteLine("Invalide parameters,syntax is COPY /serverfolderfrom/file.ext /serverfolderto");
                            break;
                        case "MOVE":
                            if (parameters.Length == 3)
                            {
                                MOVE(parameters[1], parameters[2]);
                            }
                            else Console.WriteLine("Invalide parameters,syntax is MOVE /serverfolderfrom/file.ext /serverfolderto");
                            break;
                        case "DELETE":
                            if (parameters.Length == 2)
                            {
                                DELETE(parameters[1]);
                            }
                            else Console.WriteLine("Invalide parameters,syntax is DELETE /serverfolder/file.ext");
                            break;
                        default:
                            Console.WriteLine("Invalide command, supported commands are GET,PUT,POST,COPY,MOVE,DELETE");
                            break;
                    }
                }
            }
        }
        public static HttpResponseMessage Request(string Method, string path, Byte[] content, Dictionary<string,string> headers)
        {
            HttpResponseMessage response = null;
            try
            {
                var client = new HttpClient();
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = new HttpMethod(Method),
                    RequestUri = new Uri(ServerUrl + path),
                    Content = new ByteArrayContent(content)
                };
                foreach(KeyValuePair<string, string> entry in headers){
                    httpRequestMessage.Headers.Add(entry.Key, entry.Value);
                }
                response = client.SendAsync(httpRequestMessage).Result;
            }
            catch
            {
                Console.WriteLine("Error sending request!");
            }
            return response;
        }

        public static void GET(string PathOnServer, string PathOnClient)
        {
            HttpResponseMessage response = Request("GET", PathOnServer, new byte[0], new Dictionary<string, string>());
            if (response?.IsSuccessStatusCode ?? false)
            {
                Console.WriteLine("File GET successful");
                Byte[] file;
                try
                {
                    file = response.Content.ReadAsByteArrayAsync().Result;
                    File.WriteAllBytes(Path.GetFullPath(PathOnClient.Replace("/", "\\")) +"//"+Path.GetFileName(PathOnServer), file);
                }
                catch
                {
                    Console.WriteLine("Error writing file!");
                    return;
                }
            }
            else
            {
                Console.WriteLine("GET unsuccessful, error: "+(response?.StatusCode.ToString() ?? "response from server was not received"));
            }
        }

        public static void PUT(string PathOnServer, string PathOnClient)
        {
            Byte[] file;
            try
            {
                file = File.ReadAllBytes(PathOnClient);
            }
            catch
            {
                Console.WriteLine("File not found!");
                return;
            }
            Dictionary<string, string> headers = new Dictionary<string, string>{
                { "FileName", Path.GetFileName(PathOnClient) }
            };
            HttpResponseMessage response = Request("PUT", PathOnServer, file,headers);
            if (response?.IsSuccessStatusCode ?? false)
            {
                Console.WriteLine("File PUT successful");
            }
            else
            {
                Console.WriteLine("PUT unsuccessful, error: " + (response?.StatusCode.ToString() ?? "response from server was not received"));
            }
        }

        public static void POST(string PathOnServer, string PathOnClient)
        {
            Byte[] file;
            try
            {
                file = File.ReadAllBytes(PathOnClient);
            }
            catch
            {
                Console.WriteLine("File not found!");
                return;
            }
            Dictionary<string, string> headers = new Dictionary<string, string>{
                { "FileName", Path.GetFileName(PathOnClient) }
            };
            HttpResponseMessage response = Request("POST", PathOnServer, file,headers);
            if (response?.IsSuccessStatusCode ?? false)
            {
                Console.WriteLine("File POST successful");
            }
            else
            {
                Console.WriteLine("POST unsuccessful, error: " + (response?.StatusCode.ToString() ?? "response from server was not received"));
            }
        }

        public static void COPY(string PathFrom, string PathTo)
        {

            Dictionary<string, string> headers = new Dictionary<string, string>{
                { "Pathto", Path.GetFileName(PathTo) }
            };
            HttpResponseMessage response = Request("COPY", PathFrom, new byte[0], headers);
            if (response?.IsSuccessStatusCode ?? false)
            {
                Console.WriteLine("File COPY successful");
            }
            else
            {
                Console.WriteLine("COPY unsuccessful, error: " + (response?.StatusCode.ToString() ?? "response from server was not received"));
            }
        }

        public static void MOVE(string PathFrom, string PathTo)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>{
                { "Pathto", Path.GetFileName(PathTo) }
            };
            HttpResponseMessage response = Request("MOVE", PathFrom, new byte[0],headers);
            if (response?.IsSuccessStatusCode ?? false)
            {
                Console.WriteLine("File MOVE successful");
            }
            else
            {
                Console.WriteLine("MOVE unsuccessful, error: " + (response?.StatusCode.ToString() ?? "response from server was not received"));
            }
        }

        public static void DELETE(string PathOnServer)
        {
            HttpResponseMessage response = Request("DELETE", PathOnServer, new byte[0],new Dictionary<string, string>());
            if (response?.IsSuccessStatusCode ?? false)
            {
                Console.WriteLine("File DELETE successful");
            }
            else
            {
                Console.WriteLine("DELETE unsuccessful, error: " + (response?.StatusCode.ToString() ?? "response from server was not received"));
            }
        }
    }
}

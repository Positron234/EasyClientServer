using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;
using System.Numerics;
using System.Dynamic;
using System.Diagnostics.Eventing.Reader;

namespace EasyServer
{
    enum Status
    {
        start,
        stop,
        close
    }
    internal sealed class Server
    {
        public static Status status;
        public static HttpListener listener;
        public static string url = "http://localhost:1234/";
        public static int pageViews = 0;
        public static int requestCount = 0;
       

        static Server()
        {
            //1. HttpListener предназначен для прослушивания подключений по протоколу HTTP 
            listener = new HttpListener();
            //2. устанавливаем адрес прослушки
            listener.Prefixes.Add(url);
            //3. начинаем прослушивать входящие подключения
            listener.Start();
            status = Status.start;
        }

        public static async Task ServerWork()
        {
            if (status == Status.start)
            {
                string data = "";
                bool Saving=false;
                // получаем контекст
                var context = await listener.GetContextAsync();
                var request = context.Request;      // получаем данные запроса
                var response = context.Response;    // получаем объект для установки ответа
                var user = context.User;            // получаем данные пользователя
                                        // отправляемый в ответ код htmlвозвращает
                string json="";
                if(request.Url.PathAndQuery=="/Factory")
                {
                    switch(request.HttpMethod)
                    {
                        case "GET":
                            {
                                response.StatusCode = 200;
                                using (StreamReader fs = new StreamReader("Factories.json"))
                                {
                                    json = fs.ReadToEnd();

                                }
                                Console.WriteLine("Запрос обработан - Фабрики отправлны");
                                break;
                            }
                            case "POST":
                            {
                                if (Saving == false)
                                {
                                    Saving = true;
                                    using (var reader = new StreamReader(request.InputStream))
                                    {
                                        data = reader.ReadToEnd();
                                    }
                                    using (var writer = new StreamWriter("Factories.json", false, Encoding.UTF8))
                                    {
                                        writer.Write(data);
                                    }
                                    await Task.Delay(3500);
                                    json = "Данные успешно сохранены";
                                    Console.WriteLine("Запрос обработан - Фабрики сохранены");
                                    Saving = false;
                                }
                                else
                                {
                                    response.StatusCode=200;
                                    json = "Данные сохраняются попробуйте позже";
                                }
                                break;
                            }
                        default:
                            {
                                response.StatusCode = 500;
                                break;
                            }
                    }
                    
                }
                if (request.Url.PathAndQuery == "/Tank")
                {
                    switch (request.HttpMethod)
                    {
                        case "GET":
                            {
                                response.StatusCode = 200;
                                using (StreamReader fs = new StreamReader("Tanks.json"))
                                {
                                    json = fs.ReadToEnd();

                                }
                                Console.WriteLine("Запрос обработан - Резервуары отправлны");
                                break;
                            }
                        case "POST":
                            {
                                if (Saving == false)
                                {
                                    Saving = true;
                                using (var reader = new StreamReader(request.InputStream))
                                    {
                                        data = reader.ReadToEnd();
                                    }
                                    using (var writer = new StreamWriter("Tanks.json", false, Encoding.UTF8))
                                    {
                                        writer.Write(data);
                                    }
                                    Console.WriteLine("Запрос обработан - Резервуары сохранены");
                                    await Task.Delay(3500);
                                    json = "Данные успешно сохранены";
                                    Saving = false;
                                }
                                else
                                {
                                    response.StatusCode = 200;
                                    json = "Данные сохраняются попробуйте позже";
                                }
                                break;
                            }
                        default:
                            {
                                response.StatusCode = 500;
                                break;
                            }
                    }
                }
                if (request.Url.PathAndQuery == "/Unit")
                {
                    switch (request.HttpMethod)
                    {
                        case "GET":
                            {
                                response.StatusCode = 200;
                                using (StreamReader fs = new StreamReader("Units.json"))
                                {
                                    json = fs.ReadToEnd();

                                }
                                Console.WriteLine("Запрос обработан - Установки отправлны");
                                break;
                            }
                        case "POST":
                            {
                                if (Saving == false)
                                {
                                    Saving = true;
                                    using (var reader = new StreamReader(request.InputStream))
                                    {
                                        data = reader.ReadToEnd();
                                    }
                                    using (var writer = new StreamWriter("Units.json", false, Encoding.UTF8))
                                    {
                                        writer.Write(data);
                                    }
                                    Console.WriteLine("Запрос обработан - Установки сохранены");
                                    await Task.Delay(3500);
                                    json = "Данные успешно сохранены";
                                    Saving = false;
                                }
                                else
                                {
                                    response.StatusCode = 200;
                                    json = "Данные сохраняются попробуйте позже";
                                }
                                break;
                            }
                        default:
                            {
                                response.StatusCode = 500;
                                break;
                            }
                    }

                    
                }
                //string Data = "test message: Today is " + DateTime.Now.ToString("U");
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                // получаем поток ответа и пишем в него ответ
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                // отправляем данные
                await output.WriteAsync(buffer, 0, buffer.Length);
                await output.FlushAsync();
                output.Close();
                
            }
        }
        public static void ServerStopWork()
        {
            listener.Stop(); // останавливаем сервер
            listener.Close(); // закрываем HttpListener
            status = Status.close;
        }

    }
    internal class Program
    {
        static async Task Main(string[] args)
        {

            if (Server.status == Status.start)
            {
                Console.WriteLine("Сервер запущен. Ожидание подключений...");
                Console.WriteLine("Press ESC to stop");
                do
                {
                    while (!Console.KeyAvailable)
                    {
                        await Task.Delay(100);
                        Task task = new Task(() => _ = Server.ServerWork());
                        task.Start();
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
                Server.status = Status.stop;
                Server.ServerStopWork();
                if (Server.status == Status.close)
                    Console.WriteLine("Сервер остановлен и закрыт");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SignalRStocks
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            StartHub();
            Console.WriteLine("Waiting");
            Console.ReadLine();
        }

        private static string _baseUrl = "https://js.devexpress.com/Demos/NetCore/liveUpdateSignalRHub"; 
        private static void StartHub()
        {
            var _hubConnection = new HubConnectionBuilder()
                .WithUrl(_baseUrl).Build();
            _hubConnection.On<StockUpdate>("updateStockPrice", data =>
            {
                Console.WriteLine(data.ToJson());
            });
            try
            {
                _hubConnection.StartAsync().Wait();
                Console.WriteLine("State " +_hubConnection.State);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToJson());
                throw;
            }
            
        }
        
    }

    public class StockUpdate
    {
        public string Symbol { get; set; }
        public double Price { get; set; }
        public double Change { get; set; }
        public double Percent { get; set; }
    }
   
    
    public static class JsonExtesntions
    {
        public static string ToJson(this object obj)
        {
            try
            {
                var res = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    DateTimeZoneHandling = DateTimeZoneHandling.Local,
                    ContractResolver = new DefaultContractResolver {NamingStrategy = new CamelCaseNamingStrategy()},
                });
                return res;
            }
            catch (Exception e)
            { 
                return $"error convert to json: {obj}, excption:{e.ToJson()}";
            }
        }
        public static string ToJson(this Exception ex)
        {
            var res= new Dictionary<string, string>()
            {
                {"Type",ex.GetType().ToString()},
                {"Message",ex.Message},
                {"StackTrace",ex.StackTrace}
            };
            return res.ToJson();
        }
    }
}
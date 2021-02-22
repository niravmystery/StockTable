using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace SignalRStocks
{

    static class Program
    {
        static DataTable table = new DataTable();

        static void program(string[] args)
        {
            StartHub();
        }

        private static string _baseUrl = "https://js.devexpress.com/Demos/NetCore/liveUpdateSignalRHub";
        public static void StartHub()
        {
            var _hubConnection = new HubConnectionBuilder()
                .WithUrl(_baseUrl).Build();
            _hubConnection.On<StockUpdate>("updateStockPrice", data =>
            {
                //Console.WriteLine(data.ToJson().ToString());
                string jsonString = data.ToJson();

                StockUpdate[] arr = new StockUpdate[] { data };
                string json_data = JsonConvert.SerializeObject(arr); // this is the Newtonsoft API method

                List<StockUpdate> UserList = JsonConvert.DeserializeObject<List<StockUpdate>>(json_data);

                DataTable dtGUI = new DataTable();
                dtGUI = UserList.ToDataTable<StockUpdate>();
                MainWindow mv = new MainWindow();
                mv.GenerateTable(dtGUI);
            });
            try
            {
                _hubConnection.StartAsync().Wait();
                //Console.WriteLine("State " + _hubConnection.State);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToJson());
                throw;
            }

        }
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props =
            TypeDescriptor.GetProperties(typeof(T));
            if (table.Columns.Count != 3)
            {
                table = new DataTable();

                for (int i = 0; i < props.Count; i++)
                {
                    PropertyDescriptor prop = props[i];
                    table.Columns.Add(prop.Name, prop.PropertyType);
                }
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
    }
    public class StockUpdate
    {

        public string Symbol { get; set; }
        public double Price { get; set; }
        public double Change { get; set; }
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
                    ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
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
            var res = new Dictionary<string, string>()
            {
                {"Type",ex.GetType().ToString()},
                {"Message",ex.Message},
                {"StackTrace",ex.StackTrace}
            };
            return res.ToJson();
        }
    }
}
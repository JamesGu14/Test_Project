using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter.ImportData
{
    public class AllStocks
    {
        public string baseUrl = "http://hq.sinajs.cn/list=";
        public void GetAllStocks()
        {
            string resultText = string.Empty;

            StreamWriter sw = new StreamWriter(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + "\\App_Data\\stocks_sz.txt");

            for (var i = 0; i < 2854; i++)
            {
                string stock_code = string.Empty;
                if (i.ToString().Length == 1)
                {
                    stock_code = "sz00000" + i;
                }
                else if (i.ToString().Length == 2)
                {
                    stock_code = "sz0000" + i;
                }
                else if (i.ToString().Length == 3)
                {
                    stock_code = "sz000" + i;
                }
                else if (i.ToString().Length == 4)
                {
                    stock_code = "sz00" + i;
                }
                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.GetAsync(new Uri(baseUrl + stock_code)).Result;
                String result = response.Content.ReadAsStringAsync().Result;

                if (result.Length > 100)
                {
                    string stock_code_str = result.Substring(13, 6);
                    string stock_name = result.Substring(result.IndexOf("\"") + 1, 10);
                    stock_name = stock_name.Substring(0, stock_name.IndexOf(","));
                    sw.WriteLine(stock_code_str + " " + stock_name);

                    Console.WriteLine(stock_code_str + " " + stock_name);
                }
            }

            sw.Close();
            Console.WriteLine("All Completed");
            Console.Read();
        }
    }
}

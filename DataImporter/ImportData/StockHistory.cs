using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.DomainModels;
using Newtonsoft.Json;

namespace DataImporter
{
    public class StockHistory
    {
        private const String host = "http://ali-stock.showapi.com";
        private const String path = "/sz-sh-stock-history";
        private const String method = "GET";
        private const String appcode = "9a5e597e74eb4ed89ea42f30a8c2d4ab";

        public string Import(string stock_code, string begin, string end)
        {
            //using System.Net.Security;
            //using System.Security.Cryptography.X509Certificates;

            string querys = string.Format("begin={1}&code={0}&end={2}", stock_code, begin, end);
            string bodys = "";
            string url = host + path;
            HttpWebRequest httpRequest = null;
            HttpWebResponse httpResponse = null;

            if (0 < querys.Length)
            {
                url = url + "?" + querys;
            }

            if (host.Contains("https://"))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                httpRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
            }
            else
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(url);
            }
            httpRequest.Method = method;
            httpRequest.Headers.Add("Authorization", "APPCODE " + appcode);
            if (0 < bodys.Length)
            {
                byte[] data = Encoding.UTF8.GetBytes(bodys);
                using (Stream stream = httpRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            try
            {
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            }
            catch (WebException ex)
            {
                httpResponse = (HttpWebResponse)ex.Response;
            }

            if (httpResponse == null)
            {
                return stock_code;
            }

            Stream st = httpResponse.GetResponseStream();
            if (!httpResponse.StatusCode.ToString().StartsWith("OK"))
            {
                return "network-fail";
            }
            StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
            string resJson = reader.ReadToEnd();

            using (var dbContext = new StockTrackerEntities())
            {
                // Get stock_id
                var stock_id =
                    (from s in dbContext.stocks where s.stock_code == stock_code select s).FirstOrDefault().id;

                if (string.IsNullOrWhiteSpace(resJson))
                {
                    return stock_code;
                }

                var stockTradingDates = dbContext.stock_trading_date.ToList();

                StockHistoryModel jsonObj = JsonConvert.DeserializeObject<StockHistoryModel>(resJson);
                if (!jsonObj.showapi_res_body.list.Any())
                {
                    return stock_code;
                }

                //for (var i = jsonObj.showapi_res_body.list.Count() - 1; i > 0; i--)
                //{
                //    var stockDay = jsonObj.showapi_res_body.list[i];

                //    if (!stockTradingDates.Any(s => s.trading_date == stockDay.date))
                //    {
                //        dbContext.stock_trading_date.Add(new stock_trading_date
                //        {
                //            trading_date = stockDay.date
                //        });
                //    }
                //}
                //dbContext.SaveChanges();
                var existingStockHistories = dbContext.stock_history.Where(s => s.stock_id == stock_id).ToList();

                for (var i = jsonObj.showapi_res_body.list.Count() - 1; i >= 0; i--)
                {
                    var stockDay = jsonObj.showapi_res_body.list[i];

                    int dayId = stockTradingDates.FirstOrDefault(s => s.trading_date == stockDay.date).id;

                    if (existingStockHistories.All(e => e.trading_date != dayId))
                    {
                        var stockHistory = new stock_history
                        {
                            stock_id = stock_id,
                            close_price = stockDay.close_price,
                            diff_rate = stockDay.diff_rate,
                            max_price = stockDay.max_price,
                            min_price = stockDay.min_price,
                            open_price = stockDay.open_price,
                            stock_day = stockDay.date,
                            swing = stockDay.swing,
                            trade_num = stockDay.trade_num,
                            trade_money = stockDay.trade_money,
                            turnover = stockDay.turnover,
                            trading_date = dayId
                        };
                        dbContext.stock_history.Add(stockHistory);
                    }
                }
                dbContext.SaveChanges();
                Console.WriteLine($"{stock_code} Imported.");
            }

            return string.Empty;
        }

        public string ImportFromFile(string folderPath)
        {
            DirectoryInfo dir = new DirectoryInfo(folderPath);

            FileInfo[] files = dir.GetFiles();

            using (var dbContext = new StockTrackerEntities())
            {
                var stocks = dbContext.stocks.ToList();

                foreach (var f in files)
                {
                    string stockCode = f.Name.Split('.')[0];
                    if (!stocks.Any(s => s.stock_code == stockCode))
                    {
                        dbContext.stocks.Add(new stock
                        {
                            stock_code = stockCode,
                            stock_name = "placeholder"
                        });
                    }
                }
                dbContext.SaveChanges();
            }


            foreach (var f in files)
            {
                using (var dbContext = new StockTrackerEntities())
                {
                    StreamReader file = new StreamReader(f.FullName);
                    string line;

                    string stockCode = f.Name.Split('.')[0];
                    stock currentStock = dbContext.stocks.FirstOrDefault(s => s.stock_code == stockCode);
                    List<stock_trading_date> tradingDates = dbContext.stock_trading_date.ToList();
                    if (currentStock == null)
                    {
                        continue;
                    }

                    int stockId = currentStock.id;
                    string content = "";
                    while ((line = file.ReadLine()) != null)
                    {
                        if (!line.StartsWith("20"))
                        {
                            continue;
                        }

                        string[] split = line.Split(',');

                        var tradingDate = new DateTime(int.Parse(split[0].Split('-')[0]), int.Parse(split[0].Split('-')[1]), int.Parse(split[0].Split('-')[2]));
                        var tradindDateDb = tradingDates.FirstOrDefault(t => t.trading_date == tradingDate);

                        if (tradindDateDb == null)
                        {
                            dbContext.stock_trading_date.Add(new stock_trading_date
                            {
                                trading_date = tradingDate
                            });
                            dbContext.SaveChanges();
                            tradindDateDb = dbContext.stock_trading_date.FirstOrDefault(t => t.trading_date == tradingDate);
                        }

                        //newHistories.Add(new stock_history
                        //{
                        //    stock_id = stockId,
                        //    trading_date = tradindDateDb.id,
                        //    stock_day = tradingDate,
                        //    open_price = decimal.Parse(split[1]),
                        //    max_price = decimal.Parse(split[2]),
                        //    min_price = decimal.Parse(split[3]),
                        //    close_price = decimal.Parse(split[4]),
                        //    trade_num = long.Parse(split[5]),
                        //    trade_money = decimal.Parse(split[6]),
                        //});
                        content += $"insert into stock_history (stock_id, trading_date, stock_day, open_price, max_price, min_price, close_price, trade_num, trade_money) " +
                            $"values({stockId}, {tradindDateDb.id}, '{string.Format("{0:yyyy-MM-dd}", tradingDate)}', {decimal.Parse(split[1])}, {decimal.Parse(split[2])}, {decimal.Parse(split[3])}, {decimal.Parse(split[4])}, {decimal.Parse(split[5])}, {decimal.Parse(split[6])});\r\n";
                    }
                    WriteSqlFile(content);
                    Console.WriteLine($"{stockCode} finishes");
                }
            }

            return "";
        }
        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public void WriteSqlFile(string line)
        {

            // Write the string to a file.
            StreamWriter file = File.AppendText("c:\\log\\output.sql");
            file.WriteLine(line);

            file.Close();

        }
    }
}

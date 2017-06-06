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


        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
}

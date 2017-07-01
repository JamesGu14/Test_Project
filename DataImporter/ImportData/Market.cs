using DataAccess;
using DataAccess.DomainModels;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace DataImporter.ImportData
{
    public class Market
    {
        private const string host = "http://ali-stock.showapi.com";
        private const string path = "/indexDayHis";
        private const string method = "GET";
        private const string appcode = "9a5e597e74eb4ed89ea42f30a8c2d4ab";

        public string Import(string code, string month)
        {
            string querys = string.Format("code={0}&month={1}", code, month);
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
                return code + month;
            }

            Stream st = httpResponse.GetResponseStream();
            if (!httpResponse.StatusCode.ToString().StartsWith("OK"))
            {
                return "network-fail";
            }
            StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
            string resJson = reader.ReadToEnd();

            using(var dbContext = new StockTrackerEntities())
            {
                if (string.IsNullOrWhiteSpace(resJson))
                {
                    return code + month;
                }

                var stockTradingDates = dbContext.stock_trading_date.ToList();

                MarketIndexModel jsonObj = JsonConvert.DeserializeObject<MarketIndexModel>(resJson);
                if (!jsonObj.showapi_res_body.list.Any())
                {
                    return code + month;
                }

                for (var i = jsonObj.showapi_res_body.list.Count() - 1; i > 0; i--)
                {
                    var indexDay = jsonObj.showapi_res_body.list[i];

                    if (!stockTradingDates.Any(s => s.trading_date == indexDay.date))
                    {
                        Console.WriteLine(indexDay.date + " does not exist in stock_trading_date");
                    }
                    else
                    {
                        var dayId = stockTradingDates.FirstOrDefault(s => s.trading_date == indexDay.date).id;

                        dbContext.market_index_history.Add(new market_index_history
                        {
                            market_index_id = 2, 
                            trading_date_id = dayId,
                            min_price = indexDay.min_price,
                            trade_num = (long)indexDay.trade_num,
                            trade_money = (long)indexDay.trade_money,
                            diff_money = indexDay.diff_money,
                            close_price = indexDay.close_price,
                            open_price = indexDay.open_price,
                            max_price = indexDay.max_price,
                            diff_rate = indexDay.diff_rate,
                            date = indexDay.date
                        });
                    }
                }

                dbContext.SaveChanges();
            }
            return string.Empty;
        }

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
}

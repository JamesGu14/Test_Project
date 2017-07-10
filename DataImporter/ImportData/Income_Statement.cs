using DataAccess;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter.ImportData
{
    public class Income_Statement
    {
        public string baseUrl = "http://listxbrl.sse.com.cn/profit/showmap.do";
        public List<stock> stockList = new List<stock>();

        public Income_Statement()
        {
            using (var dbContext = new StockTrackerEntities())
            {
                stockList = dbContext.stocks.ToList();
            }
        }

        public void ImportData()
        {
            using (var httpClient = new HttpClient())
            using (var dbContext = new StockTrackerEntities())
            {
                System.Net.ServicePointManager.Expect100Continue = false;
                foreach (var stock in stockList)
                {
                    if (!stock.stock_code.StartsWith("6"))
                    {
                        continue;
                    }
                    var formContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("stock_id", stock.stock_code),
                        new KeyValuePair<string, string>("report_period_id", "5000"),
                    });

                    var jsonObj = new ResultObject();
                    try
                    {
                        var response = httpClient.PostAsync(baseUrl, formContent).Result;
                        var result = response.Content.ReadAsStringAsync().Result;
                        
                        jsonObj = JsonConvert.DeserializeObject<ResultObject>(result);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{stock.stock_code} fails");
                        continue;
                    }

                    var statementObj2016 = new stock_income_statement_history
                    {
                        stock_id = stock.id,
                        year = 2016,
                        revenue = jsonObj.rows[0].Value0 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[0].Value0.Replace(",", "")),
                        net_profit = jsonObj.rows[0].Value0 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[29].Value0.Replace(",", "")),
                        earnings_per_share = jsonObj.rows[0].Value0 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[32].Value0.Replace(",", ""))
                    };

                    var statementObj2015 = new stock_income_statement_history
                    {
                        stock_id = stock.id,
                        year = 2015,
                        revenue = jsonObj.rows[0].Value1 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[0].Value1.Replace(",", "")),
                        net_profit = jsonObj.rows[0].Value1 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[29].Value1.Replace(",", "")),
                        earnings_per_share = jsonObj.rows[0].Value1 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[32].Value1.Replace(",", ""))
                    };

                    var statementObj2014 = new stock_income_statement_history
                    {
                        stock_id = stock.id,
                        year = 2014,
                        revenue = jsonObj.rows[0].Value2 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[0].Value2.Replace(",", "")),
                        net_profit = jsonObj.rows[0].Value2 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[29].Value2.Replace(",", "")),
                        earnings_per_share = jsonObj.rows[0].Value2 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[32].Value2.Replace(",", ""))
                    };

                    var statementObj2013 = new stock_income_statement_history
                    {
                        stock_id = stock.id,
                        year = 2013,
                        revenue = jsonObj.rows[0].Value3 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[0].Value3.Replace(",", "")),
                        net_profit = jsonObj.rows[0].Value3 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[29].Value3.Replace(",", "")),
                        earnings_per_share = jsonObj.rows[0].Value3 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[32].Value3.Replace(",", ""))
                    };

                    var statementObj2012 = new stock_income_statement_history
                    {
                        stock_id = stock.id,
                        year = 2012,
                        revenue = jsonObj.rows[0].Value4 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[0].Value4.Replace(",", "")),
                        net_profit = jsonObj.rows[0].Value4 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[29].Value4.Replace(",", "")),
                        earnings_per_share = jsonObj.rows[0].Value4 == null ? (decimal?)null : decimal.Parse(jsonObj.rows[32].Value4.Replace(",", ""))
                    };

                    dbContext.stock_income_statement_history.Add(statementObj2016);
                    dbContext.stock_income_statement_history.Add(statementObj2015);
                    dbContext.stock_income_statement_history.Add(statementObj2014);
                    dbContext.stock_income_statement_history.Add(statementObj2013);
                    dbContext.stock_income_statement_history.Add(statementObj2012);
                }

                dbContext.Configuration.AutoDetectChangesEnabled = false;
                dbContext.SaveChanges();
            }
        }
    }

    public class ResultObject
    {
        public int Total { get; set; }
        public string Stock_Id { get; set; }
        public List<RowObject> rows { get; set; }
    }

    public class RowObject
    {
        public string Value0 { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string Value4 { get; set; }
    }

}

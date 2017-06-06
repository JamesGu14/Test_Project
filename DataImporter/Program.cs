using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.Objects;
using System.Linq;
using System.Security;
using Common;
using DataAccess;
using DataAccess.DomainModels;
using DataImporter.Calculations;
using DataImporter.ImportData;
using StockTracker.Calculations;

namespace DataImporter
{
    public class Program
    {
        private readonly List<StockWithStartDate> NEW_STOCKS;
        private readonly List<string> STOCK_CODES;

        public Program()
        {
            NEW_STOCKS = new List<StockWithStartDate>
            {
            };

            using (var dbContext = new StockTrackerEntities())
            {
                // && s.id > 130 && s.stock_code != "002092" && 
                STOCK_CODES = dbContext.stocks.Where(s => s.IsDelisted == false).Select(s => s.stock_code).ToList();

                
            }
        }
        public string CallImport()
        {
            var errorStock = string.Empty;
            using (var dbContext = new StockTrackerEntities())
            {
                // Insert into stock table
                NEW_STOCKS.ForEach(ns =>
                {
                    // Check if a stock is exist
                    if (!dbContext.stocks.Any(s => s.stock_code == ns.StockCode))
                    {
                        dbContext.stocks.Add(new stock
                        {
                            stock_code = ns.StockCode,
                            stock_name = ns.StockName
                        });
                        dbContext.SaveChanges();

                        DateTime now = DateTime.Now;
                        DateTime start = new DateTime(now.Year, now.Month, 1);  // 本月第一天
                        DateTime end = new DateTime(now.Year, now.Month + 1, 1).AddDays(-1);  //本月最后一天

                        int completed = 0; // 连续10个月没有值认定为未开盘
                        while (start >= new DateTime(2000, 1, 1)) //&& completed < 10
                        {
                            string resultStock = new StockHistory().Import(ns.StockCode, start.ToString("yyyy-MM-dd"),
                                end.ToString("yyyy-MM-dd")) + " ";
                            // 如果fail试5次
                            if (resultStock == "network-fail")
                            {
                                resultStock = new StockHistory().Import(ns.StockCode, start.ToString("yyyy-MM-dd"),
                                    end.ToString("yyyy-MM-dd")) + " ";

                                if (resultStock == "network-fail")
                                {
                                    resultStock = new StockHistory().Import(ns.StockCode, start.ToString("yyyy-MM-dd"),
                                        end.ToString("yyyy-MM-dd")) + " ";

                                    if (resultStock == "network-fail")
                                    {
                                        resultStock = new StockHistory().Import(ns.StockCode, start.ToString("yyyy-MM-dd"),
                                            end.ToString("yyyy-MM-dd")) + " ";

                                        if (resultStock == "network-fail")
                                        {
                                            resultStock = new StockHistory().Import(ns.StockCode, start.ToString("yyyy-MM-dd"),
                                                end.ToString("yyyy-MM-dd")) + " ";
                                        }
                                    }
                                }
                            }

                            Console.WriteLine("Imported: {2}, Start: {0}, End: {1}", start.ToString("yyyy-MM-dd"),
                                end.ToString("yyyy-MM-dd"), ns.StockCode + " " + ns.StockName);
                            end = start.AddDays(-1);
                            start = start.AddMonths(-1);
                            if (!string.IsNullOrWhiteSpace(resultStock))
                            {
                                completed++;
                                continue;
                            }
                            completed = 0;
                        }

                        Console.WriteLine(" ");
                        Console.WriteLine("Successfully imported for {0}", ns.StockCode + " " + ns.StockName);
                        Console.WriteLine("====================================================");
                    }
                    else
                    {
                        Console.WriteLine("Stock {0} has already exist", ns.StockCode + " " + ns.StockName);
                    }
                });
            }

            return errorStock;
        }

        public void CallCalculatingMACD(string stockCode, DateTime startDate, DateTime endDate)
        {
            new MACD().CalculateMACD(stockCode, startDate, endDate);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{stockCode} calculation has completed. ");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void CallCalculatingKDJ(string stockCode, DateTime startDate, DateTime endDate)
        {
            new KDJ().CalculateKDJ(stockCode, startDate, endDate);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{stockCode} calculation has completed. ");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void CallCalculateMA(string stockCode, DateTime startDate, DateTime endDate)
        {
            new MA(stockCode, startDate, endDate).CalculateMA();

            Console.WriteLine($"{stockCode} calculation has completed. ");
        }

        public void CalculateStockHistoryId()
        {
            List<int> stockIdList = new List<int>();

            int[] needArr = { 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80 };
            using (var dbContext = new StockTrackerEntities())
            {
                stockIdList = (from s in dbContext.stocks
                               where needArr.Contains(s.id)
                               select s.id).ToList();
            }

            foreach (int stock_id in stockIdList)
            {
                using (var dbContext = new StockTrackerEntities())
                {
                    dbContext.Configuration.AutoDetectChangesEnabled = false;

                    List<int> shList = (from sh in dbContext.stock_history
                                        where sh.stock_id == stock_id
                                        orderby sh.stock_day
                                        select sh.id).ToList();

                    //List<stockmacd> macdList = (from sk in dbContext.stockmacds
                    //                            where sk.stock_id == stock_id
                    //                            orderby sk.stock_day
                    //                            select sk).ToList();


                    //if (shList.Count == macdList.Count)
                    //{
                    //    Console.WriteLine($"{stock_id} matched");
                    //    for (var i = 0; i < shList.Count; i++)
                    //    {
                    //        var macd = macdList[i];
                    //        macd.stock_history_id = shList[i];
                    //        dbContext.stockmacds.AddOrUpdate(macd);
                    //        dbContext.Entry(macd).State = System.Data.EntityState.Modified;
                    //    }
                    //}
                    //else
                    //{
                    //    Console.WriteLine($"===================={stock_id} not match");
                    //}

                    List<stockma> maList = (from sk in dbContext.stockmas
                                            where sk.stock_id == stock_id
                                            orderby sk.stock_date
                                            select sk).ToList();

                    if (shList.Count == maList.Count)
                    {
                        Console.WriteLine($"{stock_id} matched");
                        for (var i = 0; i < shList.Count; i++)
                        {
                            var ma = maList[i];
                            ma.stock_history_id = shList[i];
                            dbContext.stockmas.AddOrUpdate(ma);
                            dbContext.Entry(ma).State = System.Data.EntityState.Modified;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"===================={stock_id} not match");
                    }


                    dbContext.SaveChanges();
                }
            }

        }

        #region New Logic:

        public void NewImport()
        {
            foreach (string stockCode in STOCK_CODES)
            {
                try
                {
                    // Add to stock_trading_date first
                    // new StockHistory().Import(stockCode, "2017-05-22", "2017-05-26");
                    // new StockHistory().Import("600895", "2000-01-04", "2000-02-01");


                    // 计算一只股票所有交易日的MACD
                    // new Program().CallCalculatingMACD(stockCode, new DateTime(2016, 1, 1), new DateTime(2017, 5, 26));

                    // 计算一只股票所有交易日的KDJ指标
                    // new Program().CallCalculatingKDJ(stockCode, new DateTime(2016, 1, 1), new DateTime(2017, 5, 26));

                    new Program().CallCalculateMA(stockCode, new DateTime(2016, 1, 1), new DateTime(2017, 5, 26));
                }
                catch (Exception e)
                {
                    Logger.Log($"[{stockCode}] - " + e.StackTrace);
                }
            }
        }
        #endregion

        public static void Main(string[] args)
        {
            // 获取股票列表
            //new AllStocks().GetAllStocks();

            #region Manual import stock
            var program = new Program();
            program.NewImport();
            #endregion

            #region Calculate all KDJ

            //using (var dbContext = new StockTrackerEntities())
            //{
            //    var stockIds = dbContext.stocks.Select(s => s.id).ToList();
            //    // stockIds = new List<int>() { 998 };
            //    stockIds.ForEach(i =>
            //    {
            //        new KDJ().CalculateKDJ_Enhanced(i);
            //    });
            //}
            #endregion

            //var errorStock = new Program().CallImport();
            //Console.WriteLine(errorStock.Trim());



            // new Program().CalculateStockHistoryId();

            // new MA_Trend();

            //List<string> stockCode = new List<string>
            //{
            //    "000837"
            //};

            //new BuySellPoint(stockCode, new DateTime(2016, 1, 22), new DateTime(2016, 1, 26));

            //List<string> stockCode = new List<string>
            //{
            //    "000837", "002230", "600730", "600115", "000402", "600630", "600585", "600271", "600017", "000561"
            //};

            //new BuySellPoint(stockCode, new DateTime(2015, 12, 21), new DateTime(2017, 3, 13));

            Console.WriteLine("All Completed");
            Console.Read();
        }
    }
}

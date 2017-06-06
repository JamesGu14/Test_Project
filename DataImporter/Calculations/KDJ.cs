using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using DataAccess;

namespace DataImporter.Calculations
{
    public class KDJ
    {
        public void CalculateKDJ(string stock_code, DateTime startDate, DateTime endDate)
        {
            using (var dbContext = new StockTrackerEntities())
            {
                try
                {
                    stock stock = dbContext.stocks.FirstOrDefault(s => s.stock_code == stock_code);
                    if (stock == null) return;
                    int stock_id = stock.id;

                    List<int> tradingDates =
                        dbContext.stock_trading_date.Where(st => st.trading_date >= startDate && st.trading_date <= endDate)
                            .OrderBy(st => st.trading_date)
                            .Select(st => st.id)
                            .ToList();

                    List<stock_history> stockHistories =
                        dbContext.stock_history.Where(sh => sh.stock_id == stock_id && tradingDates.Contains(sh.trading_date.Value))
                        .OrderBy(sh => sh.stock_day).ToList();
                    

                    if (!stockHistories.Any())
                    {
                        Logger.Log("stock does not exist");
                        return;
                    }

                    List<int> stockHistoriesId = stockHistories.Select(sh => sh.id).ToList();
                    List<stockkdj> stockKDJs = dbContext.stockkdjs.Where(sk => stockHistoriesId.Contains(sk.stock_history_id.Value)).ToList();
                    List<stockkdj> newKDJs = new List<stockkdj>();
                    //if (stockKDJs.Any())
                    //{
                    //    Console.ForegroundColor = ConsoleColor.Red;
                    //    Console.WriteLine($"{stock_code} KDJ is existing.");
                    //    Console.ForegroundColor = ConsoleColor.White;
                    //    Logger.Log($"{stock_code} has been calculated.");
                    //    return;
                    //}

                    // set first day KDJ
                    var allKdjs = dbContext.stockkdjs.Where(k => k.stock_id == stock_id).ToList();
                    if (!allKdjs.Any())
                    {
                        stockHistories = dbContext.stock_history.Where(sh => sh.stock_id == stock_id).ToList();
                        var stock_firstDay = stockHistories.First();

                        var firstDay_kdj = new stockkdj(); 
                        if (stock_firstDay.max_price == stock_firstDay.min_price)
                        {
                            firstDay_kdj = new stockkdj()
                            {
                                stock_id = stock_id,
                                stock_date = stock_firstDay.stock_day,
                                rsv = 0,
                                stock_history_id = stock_firstDay.id,
                                stock_history = stock_firstDay
                            };
                        }
                        else
                        {
                            firstDay_kdj = new stockkdj()
                            {
                                stock_id = stock_id,
                                stock_date = stock_firstDay.stock_day,
                                rsv = (float)((stock_firstDay.close_price - stock_firstDay.min_price) * 100 / (stock_firstDay.max_price - stock_firstDay.min_price)),
                                stock_history_id = stock_firstDay.id,
                                stock_history = stock_firstDay
                            };
                        }
                        
                        firstDay_kdj.rsv = (float)Math.Round(firstDay_kdj.rsv, 2);
                        firstDay_kdj.k = (float)Math.Round(firstDay_kdj.rsv, 2);
                        firstDay_kdj.d = (float)Math.Round(firstDay_kdj.rsv, 2);
                        firstDay_kdj.j = 3 * firstDay_kdj.d - 2 * firstDay_kdj.k;
                        newKDJs.Add(firstDay_kdj);
                        stockKDJs.Add(firstDay_kdj);
                        tradingDates = dbContext.stock_history.Where(t => t.stock_id == stock_id).Select(t => t.trading_date.Value).ToList();
                    }
                    

                    Console.WriteLine($"Start calculating ${stock_code} KDJ indicator");

                    // Start calculating
                    for (var i = 1; i < tradingDates.Count; i++)
                    {
                        var stock_day = tradingDates[i];
                        var stock_yesterday = tradingDates[i - 1];

                        var day_kdj = stockKDJs.FirstOrDefault(sk => sk.stock_history.trading_date == stock_day);
                        if (day_kdj != null)
                        {
                            // Current day has been calculated.
                            continue;
                        }
                        var day_stock = stockHistories.FirstOrDefault(sh => sh.trading_date == stock_day);
                        if (day_stock == null)
                        {
                            continue;
                        }

                        var yesterday_kdj = stockKDJs.Last();
                        

                        // 获取最近9天的最低、最高价
                        var earlyDateInt = Math.Max(0, i - 8);
                        var days = Math.Min(i + 1, 9);
                        List<stock_history> nineDayStock = stockHistories.GetRange(earlyDateInt, days);
                        decimal highest = nineDayStock.Max(h => h.max_price);
                        decimal lowest = nineDayStock.Min(l => l.min_price);

                        float rsv = ((float) (day_stock.close_price - lowest))*100/((float) (highest - lowest));
                        float k = (rsv + 2*yesterday_kdj.k)/3;
                        float d = (k + 2*yesterday_kdj.d)/3;
                        float j = (3*k - 2*d);

                        var newKdj = new stockkdj
                        {
                            stock_id = stock_id,
                            stock_date = day_stock.stock_day,
                            stock_history_id = day_stock.id,
                            stock_history = day_stock,
                            rsv = rsv,
                            k = k,
                            d = d, 
                            j = j
                        };
                        stockKDJs.Add(newKdj);
                        newKDJs.Add(newKdj);

                        if (i % 50 == 0)
                        {
                            Console.Write(".");
                        }
                    }

                    Console.WriteLine("");
                    Console.WriteLine($"{stock_code}: Start to store to database...");

                    dbContext.Configuration.AutoDetectChangesEnabled = false;

                    for (var i = 0; i < newKDJs.Count; i++)
                    {
                        var kdj = newKDJs[i];
                        if (i % 100 == 0)
                        {
                            Console.Write(".");
                        }
                        dbContext.stockkdjs.Add(kdj);
                    }
                    dbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    Logger.Log(stock_code + " " + e.StackTrace);
                    return;
                }
            }
        }

        // Start from the first day. check if calculated or not
        public void CalculateKDJ_Enhanced(int stock_id)
        {
            using (var dbContext = new StockTrackerEntities())
            {
                // Step 1: Get whole stock_history
                var stockHistoryAndKDJ = (from sh in dbContext.stock_history
                    join k in dbContext.stockkdjs on sh.id equals k.stock_history_id into temp from sk in temp.DefaultIfEmpty()
                    where sh.stock_id == stock_id
                    orderby sh.trading_date
                    select new {sh, sk }).ToList();

                if (stockHistoryAndKDJ.All(hk => hk.sk == null))
                {
                    // If all been calculated
                    return;
                }

                return;
                //try
                //{

                //    if (!stockHistories.Any())
                //    {
                //        Logger.Log("stock does not exist");
                //        return;
                //    }

                //    List<stockkdj> newKDJs = new List<stockkdj>();
                    

                //    Console.WriteLine($"Start calculating ${stock_code} KDJ indicator");

                //    // Start calculating
                //    for (var i = 1; i < tradingDates.Count; i++)
                //    {
                //        var stock_day = tradingDates[i];
                //        var stock_yesterday = tradingDates[i - 1];

                //        var day_kdj = stockKDJs.FirstOrDefault(sk => sk.stock_history.trading_date == stock_day);
                //        if (day_kdj != null)
                //        {
                //            // Current day has been calculated.
                //            Console.WriteLine($"{stock_day} day stock KDJ has been calculated.");
                //            continue;
                //        }
                //        var day_stock = stockHistories.FirstOrDefault(sh => sh.trading_date == stock_day);
                //        if (day_stock == null)
                //        {
                //            continue;
                //        }

                //        var yesterday_kdj = stockKDJs.Last();


                //        // 获取最近9天的最低、最高价
                //        var earlyDateInt = Math.Max(0, i - 8);
                //        var days = Math.Min(i + 1, 9);
                //        List<stock_history> nineDayStock = stockHistories.GetRange(stockHistories.IndexOf(day_stock) - 8, days);
                //        decimal highest = nineDayStock.Max(h => h.max_price);
                //        decimal lowest = nineDayStock.Min(l => l.min_price);

                //        float rsv = ((float)(day_stock.close_price - lowest)) * 100 / ((float)(highest - lowest));
                //        float k = (rsv + 2 * yesterday_kdj.k) / 3;
                //        float d = (k + 2 * yesterday_kdj.d) / 3;
                //        float j = (3 * k - 2 * d);

                //        var newKdj = new stockkdj
                //        {
                //            stock_id = stock_id,
                //            stock_date = day_stock.stock_day,
                //            stock_history_id = day_stock.id,
                //            stock_history = day_stock,
                //            rsv = rsv,
                //            k = k,
                //            d = d,
                //            j = j
                //        };
                //        stockKDJs.Add(newKdj);
                //        newKDJs.Add(newKdj);

                //        if (i % 50 == 0)
                //        {
                //            Console.Write(".");
                //        }
                //    }

                //    Console.WriteLine("");
                //    Console.WriteLine($"{stock_code}: Start to store to database...");

                //    dbContext.Configuration.AutoDetectChangesEnabled = false;

                //    for (var i = 0; i < newKDJs.Count; i++)
                //    {
                //        var kdj = newKDJs[i];
                //        if (i % 100 == 0)
                //        {
                //            Console.Write(".");
                //        }
                //        dbContext.stockkdjs.Add(kdj);
                //    }
                //    dbContext.SaveChanges();
                //}
                //catch (Exception e)
                //{
                //    Logger.Log(stock_code + " " + e.StackTrace);
                //    return;
                //}
            }
        }
    }
}

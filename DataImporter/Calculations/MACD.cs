using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using DataAccess;
using DataImporter;

namespace StockTracker.Calculations
{
    public class MACD
    {
        public void CalculateMACD(string stock_code, DateTime startDate, DateTime endDate)
        {
            using (var dbContext = new StockTrackerEntities())
            {
                int stock_id = (from s in dbContext.stocks
                    where s.stock_code == stock_code
                    select s).FirstOrDefault().id;

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
                    Logger.Log($"{stock_code} has no stock history");
                    return;
                }

                List<int> stockHistoriesId = stockHistories.Select(sh => sh.id).ToList();
                List<stockmacd> stockMACDs = dbContext.stockmacds.Where(sh => stockHistoriesId.Contains(sh.stock_history_id.Value)).ToList();
                List<stockmacd> newMACDs = new List<stockmacd>();

                //if (stockMACDs.Any())
                //{
                //    Console.ForegroundColor = ConsoleColor.Red;
                //    Console.WriteLine($"{stock_code} MACD is existing.");
                //    Console.ForegroundColor = ConsoleColor.White;
                //    return;
                //}

                // Set macd for the first trading day
                //var stock_firstDay = stockHistories.OrderBy(sh => sh.stock_day).FirstOrDefault();
                //stockMACDs.Add(new stockmacd
                //{
                //    ema12 = (float) stock_firstDay.close_price,
                //    ema26 = (float)stock_firstDay.close_price,
                //    diff = 0,
                //    dea = 0,
                //    macd = 0,
                //    stock_day = stock_firstDay.stock_day,
                //    stock_id = stock_firstDay.stock_id
                //});

                Console.Write($"Start Calculating {stock_code} MACD.");

                // Start calculating
                for (var i = 1; i < tradingDates.Count; i++)
                {
                    var stock_day = tradingDates[i];
                    var stock_yesterday = tradingDates[i - 1];

                    var day_macd = stockMACDs.FirstOrDefault(sm => sm.stock_history.trading_date == stock_day);

                    if (day_macd != null)
                    {
                        continue;
                    }

                    // Start calculating day MACD related indicators
                    var yesterday_macd = stockMACDs.Last();

                    var day_stock = stockHistories.FirstOrDefault(sh => sh.trading_date == stock_day);
                    if (day_stock == null)
                    {
                        continue;
                    }

                    float ema12 = (yesterday_macd.ema12*11 + (float) day_stock.close_price*2)/13;
                    float ema26 = (yesterday_macd.ema26*25 + (float) day_stock.close_price*2)/27;
                    float diff = ema12 - ema26;
                    float dea = (yesterday_macd.dea*8 + diff*2)/10;
                    float macd = 2*(diff - dea);

                    var newMacd = new stockmacd
                    {
                        stock_id = stock_id,
                        stock_history_id = day_stock.id,
                        stock_history = day_stock,
                        stock_day = day_stock.stock_day,
                        dea = (float) Math.Round(dea, 4),
                        diff = (float)Math.Round(diff, 4),
                        ema26 = (float)Math.Round(ema26, 4),
                        ema12 = (float)Math.Round(ema12, 4),
                        macd = (float)Math.Round(macd, 4)
                    };
                    newMACDs.Add(newMacd);
                    stockMACDs.Add(newMacd);

                    if (i%50 == 0)
                    {
                        Console.Write(".");
                    }
                    // Console.WriteLine($"{stock_day} MACD has been calculated.");
                }
                Console.WriteLine("");
                Console.WriteLine($"{stock_code}: Start to store to database...");

                dbContext.Configuration.AutoDetectChangesEnabled = false;

                for (var i = 0; i < newMACDs.Count; i++)
                {
                    var macd = newMACDs[i];
                    if (i % 100 == 0)
                    {
                        Console.Write(".");
                    }
                    dbContext.stockmacds.Add(macd);
                }
                dbContext.SaveChanges();
            }
        }
    }
}

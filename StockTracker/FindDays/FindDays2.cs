using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using DataAccess;
using DataAccess.DomainModels;

namespace StockTracker.FindDays
{
    public class FindDays2
    {
        public FindDays2()
        {
            using (var dbContext = new StockTrackerEntities())
            {

                var stockHistories = (from sh in dbContext.stock_history
                                      join s in dbContext.stocks on sh.stock_id equals s.id
                                      join ma in dbContext.stockmas on sh.id equals ma.stock_history_id
                                      where sh.trading_date >= 3628 && sh.trading_date <= 4160 && 
                                        s.id > 0 && s.id < 100
                                      orderby s.id, sh.trading_date
                                      select new StockHistoryAndIndicator
                                      {
                                          StockHistory = sh,
                                          Stock = s,
                                          StockMa = ma
                                      }).ToList();

                for (int i = 1; i < stockHistories.Count; i++)
                {
                    var stockYesterday = stockHistories[i - 1];
                    var stockToday = stockHistories[i];

                    if (CandleLineAnalyze.IsKanDieTunMo(stockToday, stockYesterday))
                    {
                        Console.WriteLine($"看跌吞没: [{stockToday.StockHistory.stock_day}] [{stockToday.Stock.stock_code}] Swing: (Y){stockYesterday.StockHistory.swing} (T){stockToday.StockHistory.swing}");

                        if (i + 5 < stockHistories.Count)
                        {
                            var change = (stockHistories[i + 3].StockHistory.close_price - stockToday.StockHistory.close_price)/
                                stockToday.StockHistory.close_price;
                            var c = Math.Round(change*100, 2);
                            if (c > 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            Console.WriteLine("3 天后涨跌幅：" + c);

                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }

                    if (CandleLineAnalyze.IsQingPenDaYu(stockToday, stockYesterday))
                    {
                        Console.WriteLine($"倾盆大雨: [{stockToday.StockHistory.stock_day}] [{stockToday.Stock.stock_code}] Swing: (Y){stockYesterday.StockHistory.swing} (T){stockToday.StockHistory.swing}");

                        if (i + 5 < stockHistories.Count)
                        {
                            var change = (stockHistories[i + 3].StockHistory.close_price - stockToday.StockHistory.close_price) /
                                                            stockToday.StockHistory.close_price;
                            var c = Math.Round(change * 100, 2);
                            if (c > 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            Console.WriteLine("3 天后涨跌幅：" + c);

                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using DataAccess;

namespace DataImporter.Calculations
{
    public class MA
    {
        private readonly stock _stock;
        private readonly string _stockCode;
        private readonly List<stock_history> _stockHistories;
        private readonly List<stockma> _stockMas;
        private readonly List<StockHistoryMA> _stockHistoryMas; 
         
        public MA(string stock_code, DateTime startDate, DateTime endDate)
        {
            this._stockCode = stock_code;
            using (var dbContext = new StockTrackerEntities())
            {
                _stock = dbContext.stocks.FirstOrDefault(sh => sh.stock_code == stock_code);
                if (_stock == null)
                {
                    return;
                }
                _stockHistoryMas = (from sh in dbContext.stock_history
                                   join sm in dbContext.stockmas on sh.id equals sm.stock_history_id into temp from tt in temp.DefaultIfEmpty()
                                   where sh.stock_id == _stock.id // && sh.stock_day >= startDate
                                    orderby sh.stock_day
                                   select new StockHistoryMA
                                   {
                                       stockHistory = sh,
                                       stockMa = tt
                                   }).ToList();

                _stockHistories = dbContext.stock_history.Where(sh => sh.stock_id == _stock.id).OrderBy(sh => sh.stock_day).ToList();
            }
        }

        public void CalculateMA()
        {
            if (_stockHistoryMas == null || _stockHistories == null)
            {
                Console.WriteLine($"{_stockCode} has data issue.");
                Logger.Log($"{_stockCode} has data issue.");
                return;
            }

            using (var dbContext = new StockTrackerEntities())
            {
                List<stockma> newStockMas = new List<stockma>();

                if (!dbContext.stockmas.Any(s => s.stock_id == _stock.id))
                {
                    // Calculate first day
                    var firstDay = _stockHistoryMas[0].stockHistory;
                    var firstDayMa = new stockma
                    {
                        stock_id = _stock.id,
                        stock_date = firstDay.stock_day,
                        ma5 = firstDay.close_price,
                        ma10 = firstDay.close_price,
                        ma20 = firstDay.close_price,
                        ma30 = firstDay.close_price,
                        ma60 = firstDay.close_price,
                        ma120 = firstDay.close_price,
                        ma20_dropdays = 0,
                        ma20_raisedays = 0,
                        ma30_dropdays = 0,
                        ma30_raisedays = 0,
                        stock_history_id = firstDay.id
                    };

                    _stockHistoryMas[0].stockMa = firstDayMa;
                    newStockMas.Add(firstDayMa);
                }

                for (var i = 1; i < _stockHistoryMas.Count; i++)
                {
                    var yesterdayMA = _stockHistoryMas[i - 1].stockMa;
                    

                    if (_stockHistoryMas[i].stockMa != null)
                    {
                        continue;
                    }

                    var dayStock =
                        _stockHistories.FirstOrDefault(sh => sh.trading_date == _stockHistoryMas[i].stockHistory.trading_date);

                    // MA5
                    int startIndex5D = (i - 4 >= 0 ? i - 4 : 0);
                    int count5D = (i - 4 >= 0 ? 5 : i + 1);
                    decimal ma5 = _stockHistories.GetRange(startIndex5D, count5D).Select(sh => sh.close_price).Sum()/
                                  count5D;

                    // MA10
                    int startindex10d = (i - 9 >= 0 ? i - 9 : 0);
                    int count10d = (i - 9 >= 0 ? 10 : i + 1);
                    decimal ma10 = _stockHistories.GetRange(startindex10d, count10d).Select(sh => sh.close_price).Sum() /
                                  count10d;

                    // MA20
                    int startIndex20D = (i - 19 >= 0 ? i - 19 : 0);
                    int count20D = (i - 19 >= 0 ? 20 : i + 1);
                    decimal ma20 = _stockHistories.GetRange(startIndex20D, count20D).Select(sh => sh.close_price).Sum() /
                                  count20D;

                    // MA30
                    int startIndex30D = (i - 29 >= 0 ? i - 29 : 0);
                    int count30D = (i - 29 >= 0 ? 30 : i + 1);
                    decimal ma30 = _stockHistories.GetRange(startIndex30D, count30D).Select(sh => sh.close_price).Sum() /
                                  count30D;

                    // MA60
                    int startIndex60D = (i - 59 >= 0 ? i - 59 : 0);
                    int count60D = (i - 59 >= 0 ? 60 : i + 1);
                    decimal ma60 = _stockHistories.GetRange(startIndex60D, count60D).Select(sh => sh.close_price).Sum() /
                                  count60D;

                    // MA120
                    int startIndex120D = (i - 119 >= 0 ? i - 119 : 0);
                    int count120D = (i - 119 >= 0 ? 120 : i + 1);
                    decimal ma120 = _stockHistories.GetRange(startIndex120D, count120D).Select(sh => sh.close_price).Sum() /
                                  count120D;

                    var newStockMa = new stockma
                    {
                        stock_id = _stock.id,
                        stock_date = _stockHistoryMas[i].stockHistory.stock_day,
                        ma5 = ma5,
                        ma10 = ma10,
                        ma20 = ma20,
                        ma30 = ma30,
                        ma60 = ma60,
                        ma120 = ma120,
                        stock_history_id = dayStock.id
                    };


                    if (ma20 > yesterdayMA.ma20)
                    {
                        if (yesterdayMA.ma20_raisedays >= 0 && yesterdayMA.ma20_dropdays == 0)
                        {
                            newStockMa.ma20_raisedays = yesterdayMA.ma20_raisedays + 1;
                            newStockMa.ma20_dropdays = 0;
                        }
                        else
                        {
                            newStockMa.ma20_raisedays = 1;
                            newStockMa.ma20_dropdays = 0;
                        }
                    }
                    else if (ma20 < yesterdayMA.ma20)
                    {
                        if (yesterdayMA.ma20_raisedays == 0 && yesterdayMA.ma20_dropdays >= 0)
                        {
                            newStockMa.ma20_dropdays = yesterdayMA.ma20_dropdays + 1;
                            newStockMa.ma20_raisedays = 0;
                        }
                        else
                        {
                            newStockMa.ma20_dropdays = 1;
                            newStockMa.ma20_raisedays = 0;
                        }
                    }
                    else
                    {
                        newStockMa.ma20_dropdays = yesterdayMA.ma20_dropdays;
                        newStockMa.ma20_raisedays = yesterdayMA.ma20_raisedays;
                    }

                    if (ma30 > yesterdayMA.ma30)
                    {
                        if (yesterdayMA.ma30_raisedays >= 0 && yesterdayMA.ma30_dropdays == 0)
                        {
                            newStockMa.ma30_raisedays = yesterdayMA.ma30_raisedays + 1;
                            newStockMa.ma30_dropdays = 0;
                        }
                        else
                        {
                            newStockMa.ma30_raisedays = 1;
                            newStockMa.ma30_dropdays = 0;
                        }
                    }
                    else if (ma30 < yesterdayMA.ma30)
                    {
                        if (yesterdayMA.ma30_raisedays == 0 && yesterdayMA.ma30_dropdays >= 0)
                        {
                            newStockMa.ma30_dropdays = yesterdayMA.ma30_dropdays + 1;
                            newStockMa.ma30_raisedays = 0;
                        }
                        else
                        {
                            newStockMa.ma30_dropdays = 1;
                            newStockMa.ma30_raisedays = 0;
                        }
                    }
                    else
                    {
                        newStockMa.ma30_dropdays = yesterdayMA.ma30_dropdays;
                        newStockMa.ma30_raisedays = yesterdayMA.ma30_raisedays;
                    }


                    _stockHistoryMas[i].stockMa = newStockMa;
                    newStockMas.Add(newStockMa);
                }

                dbContext.Configuration.AutoDetectChangesEnabled = false;
                newStockMas.ForEach(shm =>
                {
                    dbContext.stockmas.Add(shm);
                });
                dbContext.SaveChanges();
            }
        }

        public class StockHistoryMA
        {
            public stock_history stockHistory { get; set; }
            public stockma stockMa { get; set; }
        }
    }
}

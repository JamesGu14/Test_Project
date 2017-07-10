using DataAccess;
using DataAccess.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackingSystem.Signal
{
    public class KDJ_Golden
    {
        public List<stock> stockList;
        public List<stock_trading_date> stockTradingDates;
        public List<SHKObject> kdjHistory;

        public KDJ_Golden(List<string> stockCodeList, DateTime date)
        {
            using (var dbContext = new StockTrackerEntities())
            {
                if (stockCodeList != null && stockCodeList.Any())
                {
                    this.stockList = dbContext.stocks.Where(s => stockCodeList.Contains(s.stock_code)).ToList();
                }
                else
                {
                    this.stockList = dbContext.stocks.Where(s => !s.IsDelisted).ToList();
                }

                this.stockTradingDates = dbContext.stock_trading_date.OrderByDescending(sd => sd.trading_date)
                    .Where(sd => sd.trading_date < date).Take(3).ToList();
                this.stockTradingDates.Reverse();

                List<int> stockTradingDateId = this.stockTradingDates.Select(d => d.id).ToList();
                List<int> stockIdList = this.stockList.Select(s => s.id).ToList();
                var kdjHistory = (from s in dbContext.stocks
                                  join h in dbContext.stock_history on s.id equals h.stock_id
                                  join k in dbContext.stockkdjs on h.id equals k.stock_history_id
                                  where stockTradingDateId.Contains(h.trading_date.Value) && stockIdList.Contains(s.id)
                                  select new SHKObject
                                  {
                                      Stock = s,
                                      StockHistory = h,
                                      StockKdj = k
                                  }).ToList();
                this.kdjHistory = kdjHistory;
            }
        }

        /// <summary>
        /// Return a list of security code which matches KDJ_Golden cross
        /// </summary>
        /// <returns></returns>
        public List<stock> GetKDJ_GoldenCrossStock()
        {
            // Stores all stocks matches the criteria
            List<stock> resultList = new List<stock>();

            using (var dbContext = new StockTrackerEntities())
            {
                foreach (var stock in stockList)
                {
                    var kdjForStock = kdjHistory.Where(k => k.Stock.id == stock.id).OrderBy(k => k.StockHistory.trading_date).ToList();
                    if (kdjForStock.Count != 3)
                    {
                        continue;
                    }

                    var firstDayKdj = kdjForStock[0].StockKdj;
                    var secondDayKdj = kdjForStock[1].StockKdj;
                    var thirdDayKdj = kdjForStock[2].StockKdj;

                    if (firstDayKdj.j < firstDayKdj.k && firstDayKdj.j < firstDayKdj.d
                        && secondDayKdj.j < secondDayKdj.k && secondDayKdj.j < secondDayKdj.d
                        && thirdDayKdj.j > thirdDayKdj.k && thirdDayKdj.j > thirdDayKdj.d)
                    {
                        resultList.Add(stock);
                    }
                }
            }

            return resultList;
        }

        /// <summary>
        /// Return a list of security code which matches KDJ_Golden cross with LOW J
        /// </summary>
        /// <returns></returns>
        public List<stock> GetLowKDJ_GoldenCrossStock()
        {
            // Stores all stocks matches the criteria
            List<stock> resultList = new List<stock>();

            using (var dbContext = new StockTrackerEntities())
            {
                foreach (var stock in stockList)
                {
                    var kdjForStock = kdjHistory.Where(k => k.Stock.id == stock.id).OrderBy(k => k.StockHistory.trading_date).ToList();
                    if (kdjForStock.Count != 3)
                    {
                        continue;
                    }

                    var firstDayKdj = kdjForStock[0].StockKdj;
                    var secondDayKdj = kdjForStock[1].StockKdj;
                    var thirdDayKdj = kdjForStock[2].StockKdj;

                    if (!(firstDayKdj.j < firstDayKdj.k && firstDayKdj.j < firstDayKdj.d
                        && secondDayKdj.j < secondDayKdj.k && secondDayKdj.j < secondDayKdj.d
                        && thirdDayKdj.j > thirdDayKdj.k + 1 && thirdDayKdj.j > thirdDayKdj.d + 1))
                    {
                        continue;
                    }

                    if (thirdDayKdj.j < 50)
                    {
                        resultList.Add(stock);
                    }
                }
            }

            return resultList;
        }

        /**
         * Sell 
         */


        public List<stock> GetKDJ_DeadCrossStock()
        {
            List<stock> resultList = new List<stock>();

            using (var dbContext = new StockTrackerEntities())
            {
                foreach (var stock in stockList)
                {
                    var kdjForStock = kdjHistory.Where(k => k.Stock.id == stock.id).OrderBy(k => k.StockHistory.trading_date).ToList();
                    if (kdjForStock.Count != 3)
                    {
                        continue;
                    }

                    var secondDayKdj = kdjForStock[1].StockKdj;
                    var thirdDayKdj = kdjForStock[2].StockKdj;

                    if (secondDayKdj.j > secondDayKdj.k && secondDayKdj.j > secondDayKdj.d
                        && thirdDayKdj.j < thirdDayKdj.k && thirdDayKdj.j < thirdDayKdj.d)
                    {
                        resultList.Add(stock);
                    }
                }
                return resultList;
            }
        }
    }
}

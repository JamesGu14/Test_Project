using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace TrackingSystem
{
    public class KDJ_Golden
    {
        private List<SHKObject> _kdjHistory;
        // private DateTime _latestDate;

        /// <summary>
        /// KDJ金叉：J上穿KD线
        /// </summary>
        public KDJ_Golden()
        {
            using (var dbContext = new StockTrackerEntities())
            {
                var pastTwoDaysInt = dbContext.stock_trading_date.OrderByDescending(d => d.id).Select(d => d.id).Take(2).ToList();

                // _latestDate = dbContext.stock_trading_date.OrderBy(d => d.id).Select(d => d.trading_date).FirstOrDefault();
                // KDJ for all stocks in past 2 days.
                var kdjHistory = (from s in dbContext.stocks
                                  join h in dbContext.stock_history on s.id equals h.stock_id
                                  join k in dbContext.stockkdjs on h.id equals k.stock_history_id
                                  where pastTwoDaysInt.Contains(h.trading_date.Value) && !s.IsDelisted
                                  select new SHKObject
                                  {
                                     Stock = s,
                                     StockHistory = h,
                                     StockKdj = k
                                  }).ToList();
                _kdjHistory = kdjHistory;
            }
        }

        /// <summary>
        /// Return a list of security code which matches KDJ_Golden cross
        /// </summary>
        /// <returns></returns>
        public List<string> GetKDJ_GoldenStock()
        {
            // Stores all stocks matches the criteria
            List<string> resultList = new List<string>();

            using (var dbContext = new StockTrackerEntities())
            {
                var stockIdList = dbContext.stocks.Where(s => !s.IsDelisted).ToList();

                foreach (var stock in stockIdList)
                {
                    var kdjForStock = _kdjHistory.Where(k => k.Stock.id == stock.id).OrderBy(k => k.StockHistory.trading_date).ToList();
                    if (kdjForStock.Count != 2)
                    {
                        continue;
                    }

                    var firstDayKdj = kdjForStock[0].StockKdj;
                    var secondDayKdj = kdjForStock[1].StockKdj;

                    if (firstDayKdj.j < firstDayKdj.k && firstDayKdj.j < firstDayKdj.d 
                        && secondDayKdj.j > secondDayKdj.k && secondDayKdj.j > secondDayKdj.d)
                    {
                        resultList.Add(stock.stock_code);
                    }
                }
            }

            return resultList;
        }

        public class SHKObject
        {
            public stock Stock { get; set; }
            public stock_history StockHistory { get; set; }
            public stockkdj StockKdj { get; set; }
        }
    }
}

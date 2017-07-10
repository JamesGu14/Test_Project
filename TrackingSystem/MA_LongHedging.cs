using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.DomainModels;

namespace TrackingSystem
{
    public class MA_LongHedging
    {
        private List<SHMObject> _maHistory; 
        /// <summary>
        /// MA多头排列：连续三天，MA5>MA10>MA30
        /// </summary>
        public MA_LongHedging()
        {
            using (var dbContext = new StockTrackerEntities())
            {
                var pastThreeDaysInt = dbContext.stock_trading_date.OrderByDescending(d => d.id).Select(d => d.id).Take(4).ToList();

                var maHistory = (from s in dbContext.stocks
                    join h in dbContext.stock_history on s.id equals h.stock_id
                    join m in dbContext.stockmas on h.id equals m.stock_history_id
                    where pastThreeDaysInt.Contains(h.trading_date.Value) && !s.IsDelisted
                    select new SHMObject
                    {
                        Stock = s,
                        StockHistory = h,
                        StockMa = m
                    }).ToList();

                _maHistory = maHistory;
            }
        }

        /// <summary>
        /// Returns a list matches ma long hedging criteria
        /// </summary>
        /// <returns></returns>
        public List<string> GetMA_LongHedgingStock()
        {
            List<string> resultList = new List<string>();

            using (var dbContext = new StockTrackerEntities())
            {
                var stockIdList = dbContext.stocks.Where(s => !s.IsDelisted).ToList();

                foreach (var stock in stockIdList)
                {
                    var maForStock = _maHistory.Where(m => m.Stock.id == stock.id).OrderBy(k => k.StockHistory.trading_date).ToList();
                    if (maForStock.Count != 4)
                    {
                        continue;
                    }

                    var firstDayMa = maForStock[0].StockMa;
                    var secondDayMa = maForStock[1].StockMa;
                    var thirdDayMa = maForStock[2].StockMa;

                    if (firstDayMa.ma5 > firstDayMa.ma10 && firstDayMa.ma10 > firstDayMa.ma30
                        && secondDayMa.ma5 > secondDayMa.ma10 && secondDayMa.ma10 > secondDayMa.ma30
                        && thirdDayMa.ma5 > thirdDayMa.ma10 && thirdDayMa.ma10 > thirdDayMa.ma30)
                    {
                        resultList.Add(stock.stock_code);
                    }
                }
            }

            return resultList;
        }

        public List<string> GetMA_LongHedgingStock_JustStart()
        {
            List<string> resultList = new List<string>();

            using (var dbContext = new StockTrackerEntities())
            {
                var stockIdList = dbContext.stocks.Where(s => !s.IsDelisted).ToList();

                foreach (var stock in stockIdList)
                {
                    var maForStock = _maHistory.Where(m => m.Stock.id == stock.id).OrderBy(k => k.StockHistory.trading_date).ToList();
                    if (maForStock.Count != 4)
                    {
                        continue;
                    }

                    var preNonDay = maForStock[0].StockMa;  // This day does not match and following three days matches
                    var firstDayMa = maForStock[1].StockMa;
                    var secondDayMa = maForStock[2].StockMa;
                    var thirdDayMa = maForStock[3].StockMa;

                    if ((preNonDay.ma5 <= preNonDay.ma10 || preNonDay.ma10 <= preNonDay.ma30)
                        && firstDayMa.ma5 > firstDayMa.ma10 && firstDayMa.ma10 > firstDayMa.ma30
                        && secondDayMa.ma5 > secondDayMa.ma10 && secondDayMa.ma10 > secondDayMa.ma30
                        && thirdDayMa.ma5 > thirdDayMa.ma10 && thirdDayMa.ma10 > thirdDayMa.ma30)
                    {
                        resultList.Add(stock.stock_code);
                    }
                }
            }

            return resultList;
        }
    }
}

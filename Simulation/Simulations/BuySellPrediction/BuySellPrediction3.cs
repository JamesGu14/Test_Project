using DataAccess;
using DataAccess.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Simulations.BuySellPrediction
{
    public class BuySellPrediction3
    {
        public List<stock> stockList;
        public List<stock_trading_date> stockTradingDates;
        public List<StockFullInfoObject> stockFullInfo;

        public BuySellPrediction3(List<string> stockCodeList, DateTime date)
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
                var stockFullInfo = (from s in dbContext.stocks
                                  join h in dbContext.stock_history on s.id equals h.stock_id
                                  join k in dbContext.stockkdjs on h.id equals k.stock_history_id
                                  join ma in dbContext.stockmas on h.id equals ma.stock_history_id
                                  join macd in dbContext.stockmacds on h.id equals macd.stock_history_id
                                  where stockTradingDateId.Contains(h.trading_date.Value) && stockIdList.Contains(s.id)
                                  select new StockFullInfoObject
                                  {
                                      Stock = s,
                                      StockHistory = h,
                                      StockKdj = k,
                                      StockMa = ma,
                                      StockMacd = macd
                                  }).ToList();
                this.stockFullInfo = stockFullInfo;
            }
        }

        public List<stock> GetToBuyStocks()
        {
            List<stock> resultList = new List<stock>();

            using (var dbContext = new StockTrackerEntities())
            {
                foreach(var stock in stockList)
                {
                    var stockFullInfoForDay = stockFullInfo.Where(k => k.Stock.id == stock.id).OrderBy(k => k.StockHistory.trading_date).ToList();
                    if (stockFullInfoForDay.Count != 3)
                    {
                        continue;
                    }

                    var result = DetermineBuyOrNot(stockFullInfoForDay);
                    if (result != null)
                    {
                        resultList.Add(result);
                    }
                }
            }
        }

        private stock DetermineBuyOrNot(List<StockFullInfoObject> stockFullInfoForDay)
        {
            var firstDayMa = stockFullInfoForDay[0].StockMa;
            var secondDayMa = stockFullInfoForDay[1].StockMa;
            var thirdDayMa = stockFullInfoForDay[2].StockMa;

            var firstDayMacd = stockFullInfoForDay[0].StockMacd;
            var secondDayMacd = stockFullInfoForDay[1].StockMacd;
            var thirdDayMacd = stockFullInfoForDay[2].StockMacd;

            var firstDayKdj = stockFullInfoForDay[0].StockKdj;
            var secondDayKdj = stockFullInfoForDay[1].StockKdj;
            var thirdDayKdj = stockFullInfoForDay[2].StockKdj;


        }
    }
}

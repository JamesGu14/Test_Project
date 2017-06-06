using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.DomainModels;

namespace DataImporter.Calculations
{
    public class BuySellPoint
    {
        public BuySellPoint(List<string> stockCodeList, DateTime startDate, DateTime endDate)
        {
            using (var dbContext = new StockTrackerEntities())
            {
                int startDayId = dbContext.stock_trading_date.FirstOrDefault(st => st.trading_date >= startDate).id;
                int endDayId = dbContext.stock_trading_date.FirstOrDefault(st => st.trading_date >= endDate).id;

                List<StockHistoryAndIndicator> stockInfoList = (from s in dbContext.stocks
                    join sh in dbContext.stock_history on s.id equals sh.stock_id
                    join kdj in dbContext.stockkdjs on sh.id equals kdj.stock_history_id
                    join macd in dbContext.stockmacds on sh.id equals macd.stock_history_id
                    join ma in dbContext.stockmas on sh.id equals ma.stock_history_id
                    where
                        stockCodeList.Contains(s.stock_code) && sh.trading_date >= startDayId &&
                        sh.trading_date <= endDayId
                    select new StockHistoryAndIndicator
                    {
                        Stock = s,
                        StockHistory = sh,
                        StockKdj = kdj,
                        StockMacd = macd,
                        StockMa = ma
                    }).ToList();

                // 分类储放Stock yesterday & stock Today
                List<StockHistoryAndIndicatorsByStock> stockHistoryAndIndicatorsByStockList = new List<StockHistoryAndIndicatorsByStock>();
                foreach (var stockCode in stockCodeList)
                {
                    stockHistoryAndIndicatorsByStockList.Add(new StockHistoryAndIndicatorsByStock
                    {
                        StockId = stockInfoList.Where(sl => sl.Stock.stock_code == stockCode).Select(sl => sl.Stock.id).FirstOrDefault(),
                        StockIndicators = stockInfoList.Where(sl => sl.Stock.stock_code == stockCode).ToList()
                    });
                }

                // Start calculation points:
                List<stock_buysell_point> buySellList = new List<stock_buysell_point>();
                foreach (StockHistoryAndIndicatorsByStock stockHistoryAndIndicators in stockHistoryAndIndicatorsByStockList)
                {
                    for (int i = 1; i < stockHistoryAndIndicators.StockIndicators.Count; i ++)
                    {
                        var stockToday = stockHistoryAndIndicators.StockIndicators[i];
                        var stockYesterday = stockHistoryAndIndicators.StockIndicators[i - 1];

                        int buyPoint = CalculateBuyPoints(stockToday, stockYesterday);
                        int sellPoint = CalculateSellPoints(stockToday, stockYesterday);

                        buySellList.Add(new stock_buysell_point
                        {
                            buy_point = buyPoint,
                            sell_point = sellPoint,
                            stock_history_id = stockToday.StockHistory.id
                        });
                    }
                }

                buySellList.ForEach(bs =>
                {
                    dbContext.stock_buysell_point.Add(bs);
                });
                dbContext.Configuration.AutoDetectChangesEnabled = false;
                dbContext.SaveChanges();
            }
        }

        // 计算买的权重，0-100之间。越高越要买
        private int CalculateBuyPoints(StockHistoryAndIndicator stockToday, StockHistoryAndIndicator stockYesterday)
        {
            int point = 0;

            var todayK = stockToday.StockKdj.k;
            var todayD = stockToday.StockKdj.d;
            var todayJ = stockToday.StockKdj.j;

            var yesterdayK = stockYesterday.StockKdj.k;
            var yesterdayD = stockYesterday.StockKdj.d;
            var yesterdayJ = stockYesterday.StockKdj.j;

            // JK在50之下上穿D, D同时也是上升
            // 昨日J,K < D; 今日J,K > D;  D今 > D昨; J,D,K < 50
            if ((yesterdayJ < yesterdayD && yesterdayK < yesterdayD) &&
                (todayK > todayD && todayJ > todayD) &&
                todayD > yesterdayD &&
                yesterdayK < 50 && yesterdayJ < 50 && yesterdayD < 50)
            {
                point += 40;
                if ((todayJ + todayK - 10) > (yesterdayJ + yesterdayK))
                {
                    point += 10;
                }
                var yesterdayMax = stockYesterday.StockHistory.max_price;
                var yesterdayMA30 = stockYesterday.StockMa.ma30;
                if ((yesterdayMax - yesterdayMA30) / yesterdayMA30 > (decimal)0.1)
                {
                    point -= 30;
                }
            }

            // TODO: MACD 

            // TODO: MA20, 30?
            if (stockToday.StockMa.ma30_raisedays > 5)
            {
                point += 10;
            }

            if (stockToday.StockMa.ma20_raisedays > 5)
            {
                point += 10;
            }

            return point;
        }

        private int CalculateSellPoints(StockHistoryAndIndicator stockToday, StockHistoryAndIndicator stockYesterday)
        {
            int point = 0;

            var todayK = stockToday.StockKdj.k;
            var todayD = stockToday.StockKdj.d;
            var todayJ = stockToday.StockKdj.j;

            var yesterdayK = stockYesterday.StockKdj.k;
            var yesterdayD = stockYesterday.StockKdj.d;
            var yesterdayJ = stockYesterday.StockKdj.j;

            // 前一天是涨，J>110再卖
            // K, J 向下突破D --> Sell

            //if (stockYesterday.StockHistory.close_price >= stockYesterday.StockHistory.open_price && todayJ > 112)
            //{
            //    return true;
            //}

            //if (stockYesterday.StockHistory.close_price < stockYesterday.StockHistory.open_price && todayJ > 100)
            //{
            //    return true;
            //}

            if ((yesterdayJ > yesterdayD && yesterdayK > yesterdayD) &&
              (todayK < todayD - 2 && todayJ < todayD - 2) &&
              todayD < yesterdayD)
            {
                point += 50;
            }

            // 当天跌幅超过5% --> Sell
            if ((stockToday.StockHistory.close_price - stockYesterday.StockHistory.close_price) / stockYesterday.StockHistory.close_price < (decimal)-0.05)
            {
                point += 50;
            }

            return point;
        }

    }
}

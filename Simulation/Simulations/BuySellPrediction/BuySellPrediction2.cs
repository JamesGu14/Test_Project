using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.DomainModels;

namespace Simulation.Simulations.BuySellPrediction
{
    public class BuySellPrediction2
    {
        public StockAnalysisResult AnalyzeStock(StockHistoryAndIndicatorsByStock stock, TradeAction action)
        {
            int stockId = stock.StockId;

            // 昨天，今天和明天 3天数据 （明天不可用）
            List<StockHistoryAndIndicator> stockIndicators = stock.StockIndicators;
            if (stockIndicators.Count != 2)
            {
                return new StockAnalysisResult
                {
                    StockId = stockId,
                    TradeAction = TradeAction.Sell,
                    Point = 0
                };
            }

            StockHistoryAndIndicator stockToday = stockIndicators[1];
            StockHistoryAndIndicator stockYesterday = stockIndicators[0];

            if (action == TradeAction.Buy)
            {
                return new StockAnalysisResult
                {
                    StockId = stockId,
                    TradeAction = TradeAction.Buy,
                    Point = CalculateBuyPoints(stockToday, stockYesterday)
                };
            }
            else
            {
                return new StockAnalysisResult
                {
                    StockId = stockId,
                    TradeAction = TradeAction.Sell,
                    Point = CalculateSellPoints(stockToday, stockYesterday)
                };
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

            var todayMA5 = stockToday.StockMa.ma5;
            var todayMA10 = stockToday.StockMa.ma10;
            var todayMA20_dropDays = stockToday.StockMa.ma20_dropdays;
            var todayMA30_dropDays = stockToday.StockMa.ma30_dropdays;

            var yesterdayMA5 = stockYesterday.StockMa.ma5;
            var yesterdayMA10 = stockYesterday.StockMa.ma10;

            // 50一下KDJ金叉就买
            if (todayJ <= 60 && yesterdayK > yesterdayJ && yesterdayD > yesterdayJ
                && todayJ - 1 > todayK && todayJ - 1 > todayD)
            {
                point += 50;
            }

            // MA5 和 MA10 的差距在缩小
            if (todayMA5 - todayMA10 >= yesterdayMA5 - yesterdayMA10)
            {
                point += 15;
            }
            
            // MA5 上穿 MA10
            if (yesterdayMA5 < yesterdayMA10 && todayMA5 > todayMA10)
            {
                if (todayJ <= 60)
                {
                    point = 100;
                }
            }

            // X
            //if (todayMA30_dropDays > 5)
            //{
            //    point -= 20;
            //}
            //if (todayMA20_dropDays > 5)
            //{
            //    point -= 20;
            //}

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

            var todayMA5 = stockToday.StockMa.ma5;
            var todayMA10 = stockToday.StockMa.ma10;

            var yesterdayMA5 = stockYesterday.StockMa.ma5;
            var yesterdayMA10 = stockYesterday.StockMa.ma10;

            if (yesterdayMA5 > yesterdayMA10 && todayMA5 <= todayMA10)
            {
                point = 100;
            }

            if (todayJ > 112)
            {
                point = 100;
            }

            return point;
        }

    }
}

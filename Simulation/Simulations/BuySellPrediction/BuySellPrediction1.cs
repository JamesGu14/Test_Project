using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.DomainModels;
using Simulation.Models;

namespace Simulation.Simulations.BuySellPrediction
{
    public class BuySellPrediction1 // : IBuySellPrediction
    {
        #region old functions
        public bool PredictBuy(StockHistoryAndIndicator stockToday, StockHistoryAndIndicator stockYesterday)
        {
            var todayK = stockToday.StockKdj.k;
            var todayD = stockToday.StockKdj.d;
            var todayJ = stockToday.StockKdj.j;

            var yesterdayK = stockYesterday.StockKdj.k;
            var yesterdayD = stockYesterday.StockKdj.d;
            var yesterdayJ = stockYesterday.StockKdj.j;

            // 昨日J,K < D
            // 今日J,K > D
            // D今 > D昨
            // J,D,K < 50
            if ((yesterdayJ < yesterdayD && yesterdayK < yesterdayD) &&
                (todayK > todayD && todayJ > todayD) &&
                todayD > yesterdayD &&
                (todayJ + todayK - 10) > (yesterdayJ + yesterdayK) &&
                yesterdayK < 50 && yesterdayJ < 50 && yesterdayD < 50)
            {
                var yesterdayMax = stockYesterday.StockHistory.max_price;
                var yesterdayMA30 = stockYesterday.StockMa.ma30;
                if ((yesterdayMax - yesterdayMA30) / yesterdayMA30 > (decimal)0.1)
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        public bool PredictSell(StockHistoryAndIndicator stockToday, StockHistoryAndIndicator stockYesterday)
        {
            var todayK = stockToday.StockKdj.k;
            var todayD = stockToday.StockKdj.d;
            var todayJ = stockToday.StockKdj.j;

            var yesterdayK = stockYesterday.StockKdj.k;
            var yesterdayD = stockYesterday.StockKdj.d;
            var yesterdayJ = stockYesterday.StockKdj.j;

            // 前一天是涨，J>110再卖
            // K, J 向下突破D --> Sell
            // 当天跌幅超过5% --> Sell
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
                return true;
            }

            if (stockToday.StockHistory.diff_rate < -3)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region new predictions
        public StockAnalysisResult AnalyzeStock(StockHistoryAndIndicatorsByStock stock, TradeAction action)
        {
            int stockId = stock.StockId;

            // 昨天，今天和明天 3天数据 （明天不可用）
            List<StockHistoryAndIndicator> stockIndicators = stock.StockIndicators;
            if (stockIndicators.Count != 2) { 
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
            if ((stockToday.StockHistory.close_price - stockYesterday.StockHistory.close_price) / stockYesterday.StockHistory.close_price > (decimal)0.05)
            {
                point += 50;
            }

            return point;
        }
        
        #endregion

    }
}

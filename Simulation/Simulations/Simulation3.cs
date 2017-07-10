using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
using DataAccess.DomainModels;
using Simulation.Simulations.BuySellPrediction;

namespace Simulation.Simulations
{
    // 模拟多只股票
    public class Simulation3
    {
        public readonly BuySellPrediction2 _BuySellPrediction;
        private readonly decimal INITIAL_AMOUNT;
        private readonly List<string> _StockCodeList;
        private readonly DateTime _StartDate;
        private readonly DateTime _EndDate;
        private readonly List<DateTime> _TradingDayList;

        public Simulation3(decimal INITIAL_AMOUNT, List<string> stockCodeList, DateTime startDate, DateTime endDate)
        {
            this.INITIAL_AMOUNT = INITIAL_AMOUNT;
            this._BuySellPrediction = new BuySellPrediction2();
            this._StockCodeList = stockCodeList;
            this._StartDate = startDate;
            this._EndDate = endDate;

            using (var dbContext = new StockTrackerEntities())
            {
                // Step 1: Get StartDate && EndDate id
                var startTradingDate = dbContext.stock_trading_date.SingleOrDefault(t => t.trading_date == startDate);
                var endTradingDate = dbContext.stock_trading_date.SingleOrDefault(t => t.trading_date == endDate);
                if (startTradingDate == null || endTradingDate == null)
                {
                    throw new Exception("Invalid start or end date");
                }

                var startDateId = startTradingDate.id;

                var endDateId = endTradingDate.id;

                List<int> stockIdList = dbContext.stocks.Where(s => stockCodeList.Contains(s.stock_code)).Select(s => s.id).ToList(); 

                    // Step 2: Get selected stocks trading_days between the start and end dates

                // Step 3: Exclude the days which stock does not have
                _TradingDayList = dbContext.stock_history
                    .Where(sh => sh.trading_date >= startDateId && sh.trading_date <= endDateId && stockIdList.Contains(sh.stock_id))
                    .Select(sh => sh.stock_day).Distinct().OrderBy(sh => sh).ToList();
            }
        }

        public void RunSimulation()
        {
            var moneyBag = new MoneyBag(INITIAL_AMOUNT);

            OperationModel nextDayOperation = new OperationModel
            {
                ActionForNextDay = TradeAction.None
            };

            for (var i = 1; i < _TradingDayList.Count; i++)
            {
                var todayDate = _TradingDayList[i];
                moneyBag.TodayDate = todayDate;
                Console.Write($"[{todayDate.ToString("yyyy-MM-dd")}] ");

                // Take action from previous day
                if (nextDayOperation.ActionForNextDay == TradeAction.Buy && nextDayOperation.ToBuyStockList.Any())
                {
                    StockHistoryAndIndicator todayStockInfo = GetDayStockInfo(nextDayOperation, todayDate);
                    // take buy action
                    if (todayStockInfo == null) throw new Exception("Stock day data has issue");

                    // TODO: Now only buys first stock, later determine strategy of buying
                    moneyBag.Buy(todayStockInfo.StockHistory.open_price, todayStockInfo.Stock.id, todayDate, todayStockInfo.Stock.stock_code);
                }
                else if (nextDayOperation.ActionForNextDay == TradeAction.Sell && nextDayOperation.ToSellStockList.Any())
                {
                    StockHistoryAndIndicator todayStockInfo = GetDayStockInfo(nextDayOperation, todayDate);
                    // take Sell action
                    if (todayStockInfo == null) continue;   // 如果不存在当天数据，说明当日停盘了。
                    moneyBag.Sell(todayStockInfo.StockHistory.open_price, todayStockInfo.Stock.id, todayDate, todayStockInfo.Stock.stock_code);
                }
                else // Nothing
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(moneyBag.ToString());
                }

                // Step 2: Analyze operation for tmr buy yesterday & today's situation
                nextDayOperation = AnalyzeStockList(todayDate, moneyBag);

                //[Shares]: {moneyBag.HoldStockShares}, 
                Console.WriteLine($"[Pocket]: {moneyBag.PocketMoney}, [Stock]: {moneyBag.GetStockValue()} | Overall: {moneyBag.PocketMoney + moneyBag.GetStockValue()}");
                Console.WriteLine(" ");
            }
        }

        // Determine what to buy and percentage of money to buy for a specific date
        // 分析所有传入股票的情况
        private OperationModel AnalyzeStockList(DateTime todayDate, MoneyBag moneyBag)
        {
            List<StockHistoryAndIndicatorsByStock> stockHistoryAndIndicatorsByStockList = new List<StockHistoryAndIndicatorsByStock>();
            using (var dbContext = new StockTrackerEntities())
            {
                DateTime yesterday = _TradingDayList[_TradingDayList.IndexOf(todayDate) - 1];
                //DateTime tomorrow = _TradingDayList[_TradingDayList.IndexOf(todayDate) + 1];

                List<StockHistoryAndIndicator> stockInfoList = (from s in dbContext.stocks
                    join sh in dbContext.stock_history on s.id equals sh.stock_id
                    join kdj in dbContext.stockkdjs on sh.id equals kdj.stock_history_id
                    join macd in dbContext.stockmacds on sh.id equals macd.stock_history_id
                    join ma in dbContext.stockmas on sh.id equals ma.stock_history_id
                    where
                        _StockCodeList.Contains(s.stock_code) && sh.stock_day >= yesterday &&
                        sh.stock_day <= todayDate
                    orderby sh.stock_day
                    select new StockHistoryAndIndicator
                    {
                        Stock = s,
                        StockHistory = sh,
                        StockKdj = kdj,
                        StockMacd = macd,
                        StockMa = ma
                    }).ToList();

                // 分类储放Stock yesterday & stock Today
                foreach (var stockCode in _StockCodeList)
                {
                    stockHistoryAndIndicatorsByStockList.Add(new StockHistoryAndIndicatorsByStock
                    {
                        StockId = stockInfoList.Where(sl => sl.Stock.stock_code == stockCode).Select(sl => sl.Stock.id).FirstOrDefault(),
                        StockIndicators = stockInfoList.Where(sl => sl.Stock.stock_code == stockCode).ToList()
                    });
                }
            }

            // TODO: Predict Buy or Sell
            // Note: OperationModel's StockDayAndIndicator is the info for the next day

            // Perform predictions:
            // 循环所有股票，返回一个List, 股票，可否买，buy权重/力度，然后决定怎么买或者怎么卖
            List<StockAnalysisResult> buyResultList = new List<StockAnalysisResult>();
            List<StockAnalysisResult> sellResultList = new List<StockAnalysisResult>();
            foreach (var stockHistoryAndIndicators in stockHistoryAndIndicatorsByStockList)
            {
                buyResultList.Add(_BuySellPrediction.AnalyzeStock(stockHistoryAndIndicators, TradeAction.Buy));
                sellResultList.Add(_BuySellPrediction.AnalyzeStock(stockHistoryAndIndicators, TradeAction.Sell));
            }

            return DayActionAnalyze(moneyBag, buyResultList, sellResultList, stockHistoryAndIndicatorsByStockList);
        }

        // 根据仓位决定，具体要买几只股票，返回OperationModel
        private OperationModel DayActionAnalyze(MoneyBag moneyBag, List<StockAnalysisResult> buyResultList,
            List<StockAnalysisResult> sellResultList, List<StockHistoryAndIndicatorsByStock> stockHistoryAndIndicatorsByStockList)
        {

            // Sort out points > 60
            buyResultList = buyResultList.Where(br => br.Point >= 60).OrderBy(br => br.Point).ToList();
            sellResultList = sellResultList.Where(sr => sr.Point >= 40).OrderBy(br => br.Point).ToList();

            switch (moneyBag.StockStatus)
            {
                case SharePosition._0:
                    // TODO: Determine how many stocks to buy by the money bag value amount. 
                    if (buyResultList.Count >= 1)
                    {
                        
                        return new OperationModel
                        {
                            ActionForNextDay = TradeAction.Buy,
                            ToBuyStockList = buyResultList.Select(br => new stock
                            {
                                id = br.StockId
                            }).ToList()
                        };
                    }
                    break;

                //case SharePosition._20:
                //    break;
                //case SharePosition._40:
                //    break;
                //case SharePosition._50:
                //    break;
                //case SharePosition._60:
                //    break;
                //case SharePosition._80:
                //    break;
                case SharePosition._100:
                    int holdingStockId = moneyBag.MoneyBagStockList[0].StockId;

                    if (sellResultList.Any(sr => sr.StockId == holdingStockId))
                    {
                        return new OperationModel
                        {
                            ActionForNextDay = TradeAction.Sell,
                            ToSellStockList = sellResultList.Select(br => new stock
                            {
                                id = br.StockId
                            }).ToList()
                        };
                    }
                    break;
            }

            return new OperationModel
            {
                ActionForNextDay = TradeAction.None
            };
        }

        private StockHistoryAndIndicator GetDayStockInfo(OperationModel nextDayOperation, DateTime todayDate)
        {
            using (var dbContext = new StockTrackerEntities())
            {
                if (nextDayOperation.ActionForNextDay == TradeAction.Buy)
                {
                    var toBuyOrSellStockId = nextDayOperation.ToBuyStockList.Select(s => s.id).ToList();
                    var stockInfo = (from s in dbContext.stocks
                        join sh in dbContext.stock_history on s.id equals sh.stock_id
                        where toBuyOrSellStockId.Contains(s.id) && sh.stock_day == todayDate
                        orderby sh.stock_day
                        select new StockHistoryAndIndicator
                        {
                            Stock = s,
                            StockHistory = sh
                        }).FirstOrDefault();
                    return stockInfo;
                }
                else if (nextDayOperation.ActionForNextDay == TradeAction.Sell)
                {
                    var toBuyOrSellStockId = nextDayOperation.ToSellStockList.Select(s => s.id).ToList();
                    var stockInfo = (from s in dbContext.stocks
                            join sh in dbContext.stock_history on s.id equals sh.stock_id
                            where toBuyOrSellStockId.Contains(s.id) && sh.stock_day == todayDate
                            orderby sh.stock_day
                            select new StockHistoryAndIndicator
                            {
                                Stock = s,
                                StockHistory = sh
                            }).FirstOrDefault();
                    return stockInfo;
                }

                return null;
            }
        }
    }
}

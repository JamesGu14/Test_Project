using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation.Models;

namespace Simulation.Simulations
{
    public class Simulation2
    {
        public void RunSimulation2(int _stockId, decimal INITIAL_AMOUNT, List<StockHistoryAndIndicator> _stockHistoryAndKDJs)
        {
            var moneyBag = new MoneyBag(INITIAL_AMOUNT);

            var actionForNextDay = TradeAction.None;
            for (var i = 3; i < _stockHistoryAndKDJs.Count; i++)
            {
                if (actionForNextDay == TradeAction.Buy)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(
                        $"[{_stockHistoryAndKDJs[i].StockKdj.stock_date.ToString("yyyy-MM-dd")}] Action - {actionForNextDay} at {_stockHistoryAndKDJs[i].StockHistory.open_price} | ");
                }
                else if (actionForNextDay == TradeAction.Sell)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(
                        $"[{_stockHistoryAndKDJs[i].StockKdj.stock_date.ToString("yyyy-MM-dd")}] Action - {actionForNextDay} at {_stockHistoryAndKDJs[i].StockHistory.open_price} | ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"[{_stockHistoryAndKDJs[i].StockKdj.stock_date.ToString("yyyy-MM-dd")}] Action - {actionForNextDay} at {_stockHistoryAndKDJs[i].StockHistory.open_price} | ");
                }

                var stockCompareDay = _stockHistoryAndKDJs[i - 3];
                var stockYesterday = _stockHistoryAndKDJs[i - 1];
                var stockToday = _stockHistoryAndKDJs[i];

                var openPrice = stockToday.StockHistory.open_price;
                var closePrice = stockToday.StockHistory.close_price;

                // Perform Actions
                switch (actionForNextDay)
                {
                    case TradeAction.Buy:
                        int sharesToBuy = (int)Math.Floor(moneyBag.PocketMoney / openPrice);
                        moneyBag.PocketMoney -= sharesToBuy * openPrice;
                        moneyBag.StockMoney = sharesToBuy * openPrice;
                        moneyBag.MoneyBagStockList[0].Shares = sharesToBuy;
                        moneyBag.StockStatus = SharePosition._100;
                        break;
                    case TradeAction.Sell:
                        moneyBag.PocketMoney += moneyBag.MoneyBagStockList[0].Shares * openPrice;
                        moneyBag.StockMoney = 0;
                        moneyBag.MoneyBagStockList[0].Shares = 0;
                        moneyBag.StockStatus = SharePosition._0;

                        break;
                    case TradeAction.None:
                        moneyBag.StockMoney = moneyBag.MoneyBagStockList[0].Shares * openPrice;
                        break;
                }

                // Perform predictions:
                switch (moneyBag.StockStatus)
                {
                    case SharePosition._0:
                        // Predict B:
                        if (PredictBuy(stockToday, stockYesterday, stockCompareDay))
                        {
                            actionForNextDay = TradeAction.Buy;
                        }
                        else
                        {
                            actionForNextDay = TradeAction.None; ;
                        }

                        break;
                    case SharePosition._100:
                        // Predict S:
                        if (PredictSell(stockToday, stockYesterday, stockCompareDay))
                        {
                            actionForNextDay = TradeAction.Sell;
                        }
                        else
                        {
                            actionForNextDay = TradeAction.None;
                        }
                        break;
                }

                Console.WriteLine($"[Pocket Money]: {moneyBag.PocketMoney}, [Shares]: {moneyBag.MoneyBagStockList[0].Shares}, [Stock Value]: {moneyBag.StockMoney} | Overall: {moneyBag.PocketMoney + moneyBag.StockMoney}");
                Console.WriteLine(" ");
            }
        }

        private bool PredictBuy(StockHistoryAndIndicator stockToday, StockHistoryAndIndicator stockYesterday, StockHistoryAndIndicator stockCompareDay)
        {
            if (stockToday.StockMacd.diff - stockToday.StockMacd.dea > stockCompareDay.StockMacd.diff - stockCompareDay.StockMacd.dea)
            {
                return true;
            }

            return false;
        }

        private bool PredictSell(StockHistoryAndIndicator stockToday, StockHistoryAndIndicator stockYesterday, StockHistoryAndIndicator stockCompareDay)
        {
            if (stockToday.StockMacd.diff - stockToday.StockMacd.dea < stockCompareDay.StockMacd.diff - stockCompareDay.StockMacd.dea)
            {
                return true;
            }
            return false;
        }
    }
}

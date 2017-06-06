using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation.Models;
using Simulation.Simulations.BuySellPrediction;

namespace Simulation.Simulations
{
    public class Simulation1
    {
        public BuySellPrediction1 _BuySellPrediction1 = new BuySellPrediction1();

        public void RunSimulation1(int _stockId, decimal INITIAL_AMOUNT, List<StockHistoryAndIndicator> _stockHistoryAndKDJs)
        {
            var moneyBag = new MoneyBag(INITIAL_AMOUNT);

            var actionForNextDay = TradeAction.None;
            for (var i = 1; i < _stockHistoryAndKDJs.Count; i++)
            {
                if (actionForNextDay == TradeAction.Buy)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(
                        $"[{_stockHistoryAndKDJs[i].StockKdj.stock_date.ToString("yyyy-MM-dd")}] {actionForNextDay} at {_stockHistoryAndKDJs[i].StockHistory.open_price} (Day: {_stockHistoryAndKDJs[i].StockHistory.diff_rate}) | ");
                }
                else if (actionForNextDay == TradeAction.Sell)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(
                        $"[{_stockHistoryAndKDJs[i].StockKdj.stock_date.ToString("yyyy-MM-dd")}] {actionForNextDay} at {_stockHistoryAndKDJs[i].StockHistory.open_price} (Day: {_stockHistoryAndKDJs[i].StockHistory.diff_rate}) | ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"[{_stockHistoryAndKDJs[i].StockKdj.stock_date.ToString("yyyy-MM-dd")}] {actionForNextDay} at {_stockHistoryAndKDJs[i].StockHistory.open_price} (Day: {_stockHistoryAndKDJs[i].StockHistory.diff_rate}) | ");
                }


                var stockYesterday = _stockHistoryAndKDJs[i - 1];
                var stockToday = _stockHistoryAndKDJs[i];

                var openPrice = stockToday.StockHistory.open_price;

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
                        if (_BuySellPrediction1.PredictBuy(stockToday, stockYesterday))
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
                        if (_BuySellPrediction1.PredictSell(stockToday, stockYesterday))
                        {
                            actionForNextDay = TradeAction.Sell;
                        }
                        else
                        {
                            actionForNextDay = TradeAction.None;
                        }
                        break;
                }

                //[Shares]: {moneyBag.HoldStockShares}, 
                Console.WriteLine($"[Pocket]: {moneyBag.PocketMoney}, [Stock]: {moneyBag.StockMoney} | Overall: {moneyBag.PocketMoney + moneyBag.StockMoney}");
                Console.WriteLine(" ");
            }
        }
    }
}

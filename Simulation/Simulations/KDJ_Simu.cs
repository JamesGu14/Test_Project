using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.DomainModels;
using TrackingSystem.Signal;
using Common;

namespace Simulation.Simulations
{
    public class KDJ_Simu
    {
        private NewMoneyBag moneyBag;
        private decimal budgetForEachStock;
        private List<string> stockPool;

        public KDJ_Simu(int cash, decimal budgetForEachStock, List<string> stockPool)
        {
            this.budgetForEachStock = budgetForEachStock;
            moneyBag = new NewMoneyBag(cash, budgetForEachStock);
            this.stockPool = stockPool;
        }

        public void Simulate(DateTime startDate, DateTime endDate)
        {
            using (var dbContext = new StockTrackerEntities())
            {
                List<stock_trading_date> tradingDates = dbContext.stock_trading_date.Where(d => d.trading_date >= startDate && d.trading_date <= endDate).ToList();

                foreach (var tradingDate in tradingDates)
                {
                    var currentDate = tradingDate.trading_date;

                    #region Step 1: Decide what to sell
                    List<int> holdingStockList = moneyBag.HoldingStocks.Select(s => s.StockId).ToList();
                    List<string> holdingStockCodeList = moneyBag.HoldingStocks.Select(s => s.StockCode).ToList();

                    // If not holding any stock
                    if (holdingStockCodeList.Any())
                    {
                        List<stock> toSellList = new KDJ_Golden(holdingStockCodeList, currentDate).GetKDJ_DeadCrossStock();
                        List<int> toSellListId = toSellList.Select(s => s.id).ToList();


                        List<int> commonStockIdList = toSellListId.FindCommonList(holdingStockList);

                        if (commonStockIdList.Any())
                        {
                            List<stock_history> stockHistories = dbContext.stock_history
                                .Where(sh => commonStockIdList.Contains(sh.stock_id) && sh.trading_date == tradingDate.id).ToList();
                            toSellList = toSellList.Where(s => commonStockIdList.Contains(s.id)).ToList();
                            toSellList.ForEach(s =>
                            {
                                var stockDayInfo = stockHistories.FirstOrDefault(sh => sh.stock_id == s.id);
                                if (stockDayInfo != null)
                                {
                                    moneyBag.SellStock(s.id, stockDayInfo.open_price, currentDate, s.stock_code);
                                }
                            });
                        }
                    }

                    #endregion

                    #region Step 2: Decide what to buy
                    if (moneyBag.Cash > budgetForEachStock)
                    {
                        List<stock> toBuyList = new KDJ_Golden(stockPool, currentDate).GetLowKDJ_GoldenCrossStock();
                        List<int> toBuyListId = toBuyList.Select(s => s.id).ToList();
                        holdingStockList = moneyBag.HoldingStocks.Select(s => s.StockId).ToList();
                        int ableToBuyStockCount = (int)(moneyBag.Cash / budgetForEachStock);

                        // Exclude already hold stocks
                        toBuyListId = toBuyListId.ExcludeCommonList(holdingStockList);
                        // Randomly pick specified amount of stocks

                        if (toBuyListId.Any())
                        {
                            toBuyListId = toBuyListId.FindRandomItemsFromList(ableToBuyStockCount);
                            List<stock_history> stockHistories = dbContext.stock_history
                                .Where(sh => toBuyListId.Contains(sh.stock_id) && sh.trading_date == tradingDate.id).ToList();
                            toBuyList = toBuyList.Where(s => toBuyListId.Contains(s.id)).ToList();

                            foreach (var toBuyStock in toBuyList)
                            {
                                var stockDayInfo = stockHistories.FirstOrDefault(sh => sh.stock_id == toBuyStock.id);
                                if (stockDayInfo != null)
                                {
                                    moneyBag.BuyStock(toBuyStock.id, stockDayInfo.open_price, currentDate, toBuyStock.stock_code);
                                }
                            }
                        }
                    }
                    #endregion

                    // Print log
                    holdingStockList = moneyBag.HoldingStocks.Select(s => s.StockId).ToList();
                    List<stock_history> holdingStockHistories = dbContext.stock_history
                            .Where(sh => holdingStockList.Contains(sh.stock_id) && sh.trading_date == tradingDate.id).ToList();
                    var currentHoldingStockList = moneyBag.HoldingStocks;

                    List<HoldingStock> stockCurrentStatus = new List<HoldingStock>();
                    foreach (var s in currentHoldingStockList)
                    {
                        // This is current day value. Don't get missed by property name
                        if (holdingStockHistories.FirstOrDefault(hs => hs.stock_id == s.StockId) == null)
                        {
                            continue;
                        }
                        stockCurrentStatus.Add(new HoldingStock
                        {
                            StockId = s.StockId,
                            StockCode = s.StockCode,
                            PurchasePricePerShare = holdingStockHistories.FirstOrDefault(hs => hs.stock_id == s.StockId).close_price
                        });
                    }

                    moneyBag.PrintLog(currentDate, stockCurrentStatus);

                    Console.WriteLine(currentDate + " finished");
                }
            }
        }
    }
}

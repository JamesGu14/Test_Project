using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DomainModels
{
    public class NewMoneyBag
    {
        // Initially, total amount of money
        public decimal Cash { get; set; }

        // Budget for each stock 
        public decimal BudgetForEachStock { get; set; }

        public List<HoldingStock> HoldingStocks { get; set; }

        public List<TradingHistory> TradingHistories { get; set; }

        public NewMoneyBag(decimal cash, decimal budgetForEachStock)
        {
            // Initialize money bag
            this.Cash = cash;

            this.BudgetForEachStock = budgetForEachStock;

            HoldingStocks = new List<HoldingStock>();
            TradingHistories = new List<TradingHistory>();
        }

        // Right now assume sell all for a selected stock
        public void SellStock(int stockId, decimal pricePerShare, DateTime sellDate, string stockCode = "")
        {
            var toSellStock = HoldingStocks.FirstOrDefault(s => s.StockId == stockId);

            if (toSellStock == null)
            {
                return;
            }

            // Step 1: Add money to cash
            this.Cash += toSellStock.Shares * pricePerShare;

            // Step 2: Add history record
            this.TradingHistories.Add(new TradingHistory
            {
                Action = "S",
                ActionDate = sellDate,
                StockId = stockId,
                StockCode = stockCode,
                Shares = toSellStock.Shares
            });

            // Step 3: Remove the stock from holding stock list
            this.HoldingStocks.Remove(toSellStock);

            // Step 4: Print log
            Logger.LogPure($"Sells {stockCode} at {pricePerShare}.");
        }

        public void BuyStock(int stockId, decimal pricePerShare, DateTime buyDate, string stockCode = "")
        {
            var toBuyStock = HoldingStocks.FirstOrDefault(s => s.StockId == stockId);

            // If we are holding, then don't buy again
            if (toBuyStock != null || Cash < BudgetForEachStock)
            {
                return;
            }

            int buyingShares = (int)Math.Floor(BudgetForEachStock / (pricePerShare * 100)) * 100;
            decimal totalPrice = buyingShares * pricePerShare;

            // Step 1: Decrese cash
            this.Cash -= Math.Ceiling(totalPrice * (decimal)1.0005 * 100) / 100;  // brokrage fee

            // Step 2: Add to holding stock list
            this.HoldingStocks.Add(new HoldingStock
            {
                StockId = stockId,
                StockCode = stockCode,
                Shares = buyingShares,
                PurchasePricePerShare = pricePerShare
            });

            // Step 3: Add history
            this.TradingHistories.Add(new TradingHistory
            {
                Action = "B",
                ActionDate = buyDate,
                StockId = stockId,
                StockCode = stockCode,
                Shares = buyingShares
            });

            // Step 4: Print log
            Logger.LogPure($"Buys {stockCode} at {pricePerShare}.");
        }

        public void PrintLog(DateTime date, List<HoldingStock> holdingStockStatus)
        {
            // holdingStockStatus in here indicates the current day price of stock
            string content = string.Empty;
            content += "[" + date.ToShortDateString() + "]" + "\r\n";
            decimal stockValue = 0;
            foreach(var hs in HoldingStocks)
            {
                var currentPrice = holdingStockStatus.FirstOrDefault(s => s.StockId == hs.StockId).PurchasePricePerShare;
                var diff = Math.Round((currentPrice - hs.PurchasePricePerShare) * 100 / hs.PurchasePricePerShare, 2);
                string stockContent = hs.StockCode + " B: " + hs.PurchasePricePerShare
                    + " Now: " + currentPrice
                    + " Diff: " + diff + "% \r\n";
                content += stockContent;
                stockValue += currentPrice * hs.Shares;
            }
            content += "[Account Summary] - Cash: " + this.Cash + " Stock value: " + stockValue + " Total value: " + (this.Cash + stockValue);
            content += "\r\n\r\n";

            Logger.LogPure(content);
        }
    }

    public class HoldingStock
    {
        public int StockId { get; set; }
        public string StockCode { get; set; }
        public int Shares { get; set; }
        public decimal PurchasePricePerShare { get; set; }
    }

    public class TradingHistory
    {
        // B or S
        public string Action { get; set; }
        public DateTime ActionDate { get; set; }
        public int StockId { get; set; }
        public string StockCode { get; set; }
        public int Shares { get; set; }
    }
}

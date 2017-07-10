using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DomainModels
{
    public class MoneyBag
    {
        public DateTime TodayDate { get; set; }

        public decimal PocketMoney { get; set; }

        public List<MoneyBagStock> MoneyBagStockList { get; set; }

        public SharePosition StockStatus { get; set; }

        public MoneyBag(decimal pocketMoney)
        {
            this.PocketMoney = pocketMoney;
            this.MoneyBagStockList = new List<MoneyBagStock>();

            this.StockStatus = SharePosition._0;
        }

        #region Actions to calculate MoneyBag
        public void Buy(decimal buyPrice, int stockId, DateTime todayDate, string stockCode)
        {
            int sharesToBuy = (int)Math.Floor(PocketMoney / buyPrice);
            PocketMoney -= sharesToBuy * buyPrice;
            StockStatus = SharePosition._100;
            MoneyBagStockList.Add(new MoneyBagStock
            {
                Shares = sharesToBuy,
                StockId = stockId,
                StockCode = stockCode
            });

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(
                $"Buy {stockCode} at {buyPrice} | ");
        }

        public void Sell(decimal sellPrice, int stockId, DateTime todayDate, string stockCode)
        {
            if (MoneyBagStockList.Select(mb => mb.StockId).ToList().Contains(stockId))
            {
                PocketMoney += MoneyBagStockList[0].Shares * sellPrice;
                StockStatus = SharePosition._0;
                MoneyBagStockList.Clear();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(
                    $"[{todayDate.ToString("yyyy-MM-dd")}] Sell {stockCode} at {sellPrice} | ");
            }
        }
        #endregion

        public decimal GetStockValue()
        {
            decimal stockValue = 0;
            using (var dbContext = new StockTrackerEntities())
            {
                foreach (var moneyBagStock in MoneyBagStockList)
                {
                    var firstOrDefault = (from s in dbContext.stocks
                                          join sh in dbContext.stock_history on s.id equals sh.stock_id
                                          where sh.stock_day == TodayDate && s.id == moneyBagStock.StockId
                                          select sh).FirstOrDefault();
                    if (firstOrDefault == null)
                    {
                        throw new Exception("Data issue, losing day stock_history");
                    }

                    stockValue += firstOrDefault.close_price * moneyBagStock.Shares;
                }
            }
            return stockValue;
        }

        //public SharePosition DetermineStockStatus()
        //{
        //    int percent = 100 - (int) (PocketMoney*100/(GetStockValue() + PocketMoney));
        //    if (percent < 10)
        //    {
        //        return SharePosition._0;
        //    } else if (percent >= 10 && percent < 38)
        //    {
        //        return SharePosition._25;
        //    } else if (percent >= 30 && )
        //}

        public string ToString()
        {
            if (MoneyBagStockList.Any())
            {
                return
                    $"PocketMoney: {PocketMoney}, StockMoney: {GetStockValue()}, Shares: {MoneyBagStockList[0].Shares}, Status: {StockStatus.ToString().Replace("_", "")}";
            }
            else
            {
                return
                    $"PocketMoney: {PocketMoney}, Status: {StockStatus.ToString().Replace("_", "")}";
            }
        }
    }



    public class StockHistoryAndIndicator
    {
        public stock Stock { get; set; }
        public stock_history StockHistory { get; set; }
        public stockkdj StockKdj { get; set; }
        public stockmacd StockMacd { get; set; }
        public stockma StockMa { get; set; }
    }

    public class MoneyBagStock
    {
        public int StockId { get; set; }
        public string StockCode { get; set; }
        public int Shares { get; set; }
    }

    public enum TradeAction
    {
        Buy = 'B',
        Sell = 'S',
        None = 'N'
    }

    // 持股仓位
    public enum SharePosition
    {
        _100 = 100,
        _75 = 75,
        _50 = 50,
        _25 = 25,
        _0 = 0
    }

    // 日线趋势
    public enum MaTrend
    {
        Raise = 'R',
        Drop = 'D'
    }

    public class OperationModel
    {
        public TradeAction ActionForNextDay { get; set; }

        public List<stock> ToBuyStockList { get; set; }

        public List<stock> ToSellStockList { get; set; }
    }

    public class StockHistoryAndIndicatorsByStock
    {
        public int StockId { get; set; }
        public List<StockHistoryAndIndicator> StockIndicators { get; set; }
    }

    public class StockAnalysisResult
    {
        public int StockId { get; set; }
        public TradeAction TradeAction { get; set; }
        public int Point { get; set; }
    }
}

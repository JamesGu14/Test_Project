using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using Simulation.Simulations;

namespace Simulation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //new KDJ_Simu("000837", new DateTime(2008, 6, 20), new DateTime(2017, 3, 13));
            //new KDJ_Simu("002230", new DateTime(2015, 4, 29), new DateTime(2017, 3, 16));
            // new KDJ_Simu("600730", new DateTime(2016, 1, 19), new DateTime(2017, 3, 13));

            List<string> stockCode = new List<string>
            {
                "600017" //, "002230", "600730", "600115", "000402", "600630", "600585", "600271", "600017", "000561"
            };
            new Simulation3(100000, stockCode, new DateTime(2010, 12, 1), new DateTime(2017, 3, 13)).RunSimulation();

            //using (var dbContext = new StockTrackerEntities())
            //{
            //    dbContext.Configuration.AutoDetectChangesEnabled = false;
            //    var tradingDates = dbContext.stock_trading_date.ToList();
            //    foreach (var stockTradingDate in tradingDates)
            //    {
            //        var historyList = dbContext.stock_history.Where(sh => sh.stock_day == stockTradingDate.trading_date).ToList();
            //        historyList.ForEach(hl =>
            //        {
            //            hl.trading_date = stockTradingDate.id;
            //            dbContext.stock_history.AddOrUpdate(hl);
            //            dbContext.Entry(hl).State = System.Data.EntityState.Modified;
            //        });
            //        dbContext.SaveChanges();
            //        Console.WriteLine($"{stockTradingDate.trading_date} done");
            //    }
            //}

            Console.WriteLine("Program Completed.");
            Console.Read();
        }
    }
}

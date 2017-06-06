using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.DomainModels;
using Simulation.Models;

namespace Simulation.Simulations
{
    public class KDJ_Simu
    {
        private readonly int _stockId;
        private readonly stock _stock;
        private readonly List<StockHistoryAndIndicator> _stockHistoryAndIndicators;

        private const decimal INITIAL_AMOUNT = 10000;

        public KDJ_Simu(string stockCode, DateTime startDate, DateTime endDate)
        {
            using (var dbContext = new StockTrackerEntities())
            {
                try
                {
                    this._stock = dbContext.stocks.FirstOrDefault(db => db.stock_code == stockCode);
                    if (_stock == null)
                    {
                        return;
                    }
                    this._stockId = _stock.id;

                    this._stockHistoryAndIndicators = (from sh in dbContext.stock_history
                                                 join kdj in dbContext.stockkdjs on sh.id equals kdj.stock_history_id
                                                 join macd in dbContext.stockmacds on sh.id equals macd.stock_history_id
                                                 join ma in dbContext.stockmas on sh.id equals ma.stock_history_id
                                                 where sh.stock_id == _stockId && sh.stock_day > startDate && sh.stock_day < endDate
                                                 orderby sh.stock_day
                                                 select new StockHistoryAndIndicator
                                                 {
                                                     StockHistory = sh,
                                                     StockKdj = kdj,
                                                     StockMacd = macd,
                                                     StockMa = ma
                                                 }).ToList();

                    if (this._stockHistoryAndIndicators.Count > 100)
                    {
                        ExecuteSimulation();
                    }
                    else
                    {
                        Console.WriteLine("Seems there is data issue");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        private void ExecuteSimulation()
        {
            // new Simulation1().RunSimulation1(_stockId, INITIAL_AMOUNT, _stockHistoryAndIndicators);
        }
    }
}

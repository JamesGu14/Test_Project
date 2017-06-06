using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace Simulation.Simulations
{
    public class SimulateByPoint
    {
        public SimulateByPoint(List<string> stockCodeList, DateTime startDate, DateTime endDate)
        {
            using (var dbContext = new StockTrackerEntities())
            {
                int startDayId = dbContext.stock_trading_date.FirstOrDefault(st => st.trading_date >= startDate).id;
                int endDayId = dbContext.stock_trading_date.FirstOrDefault(st => st.trading_date >= endDate).id;

                // Step 1. Get all stockIdList
                var stockIdList =
                    (from s in dbContext.stocks where stockCodeList.Contains(s.stock_code) select s.id).ToList();

                // Step 2. Get all buy sell points with selected stock and date range

                var a = (from bs in dbContext.stock_buysell_point
                    join sh in dbContext.stock_history on bs.stock_history_id equals sh.id
                    where
                        stockIdList.Contains(sh.stock_id) && sh.trading_date >= startDayId &&
                        sh.trading_date <= endDayId
                    select new {bs, sh}).ToList();

            }
        }
    }
}

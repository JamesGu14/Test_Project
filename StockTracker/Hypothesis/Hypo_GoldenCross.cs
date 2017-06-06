using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace StockTracker.Hypothesis
{
    public class Hypo_GoldenCross
    {
        private readonly int _stock_id;
        private readonly List<StockMaKdj> _StockMaKdjList;
        private readonly List<hypothesis_result> _hypothesisResults;

        public Hypo_GoldenCross(string stock_code)
        {
            using (var dbContext = new StockTrackerEntities())
            {
                try
                {
                    var query = (from s in dbContext.stocks
                        join sh in dbContext.stock_history on s.id equals sh.stock_id
                        join sk in dbContext.stockkdjs on new {A = sh.stock_id, B = sh.stock_day} equals new {A = sk.stock_id, B = sk.stock_date}
                        join smc in dbContext.stockmacds on new { A = sh.stock_id, B = sh.stock_day } equals new { A = smc.stock_id, B = smc.stock_day }
                        where s.stock_code == stock_code
                        select new StockMaKdj
                        {
                            Stock = s,
                            StockMacd = smc,
                            StockHistory = sh,
                            StockKdj = sk
                        }
                        );

                    _StockMaKdjList = query.ToList();

                    ExecuteHypos();
                }
                catch (Exception e)
                {
                    // throw e;
                }
            }
        }

        private void ExecuteHypos()
        {
            for (var i = 1; i < _StockMaKdjList.Count; i ++)
            {
                var today = _StockMaKdjList[i];
                var yesterday = _StockMaKdjList[i - 1];

                if (yesterday.StockMacd.diff < yesterday.StockMacd.dea && today.StockMacd.diff > today.StockMacd.dea)
                {
                    Console.WriteLine(today.StockHistory.stock_day);
                }
            }
        }

        public class StockMaKdj
        {
            public stock Stock { get; set; }
            public stock_history StockHistory { get; set; }
            public stockmacd StockMacd { get; set; }
            public stockkdj StockKdj { get; set; }
        }
    }
}

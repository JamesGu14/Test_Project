using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using StockTracker.Calculations;

namespace StockTracker.FindDays
{
    public class FindDay1
    {
        public FindDay1()
        {
            using (var dbContext = new StockTrackerEntities())
            {

                var stockHistories = (from sh in dbContext.stock_history
                    join s in dbContext.stocks on sh.stock_id equals s.id
                    join ma in dbContext.stockmas on sh.id equals ma.stock_history_id
                    join macd in dbContext.stockmacds on sh.id equals macd.stock_history_id
                    join kdj in dbContext.stockkdjs on sh.id equals kdj.stock_history_id
                    join d in dbContext.stock_trading_date on sh.trading_date equals d.id
                    where sh.stock_id == 4 && sh.trading_date >= 2290 && sh.trading_date <= 4159
                    select new {sh, s, d, ma, macd, kdj}).ToList();

                foreach (var stockDay in stockHistories)
                {
                    var sh = stockDay.sh;
                    var s = stockDay.s;
                    var d = stockDay.d;
                    var ma = stockDay.ma;
                    var macd = stockDay.macd;
                    var kdj = stockDay.kdj;

                    if (sh.min_price > ma.ma5 && sh.min_price > ma.ma10 && sh.min_price > ma.ma20 &&
                        sh.min_price > ma.ma30)
                    {
                        decimal shangYingXian;
                        decimal xiaYingXian;
                        decimal body;

                        // If Raise
                        if (sh.open_price >= sh.close_price)
                        {
                            shangYingXian = sh.max_price - sh.close_price;
                            xiaYingXian = sh.open_price - sh.min_price;
                            body = sh.close_price - sh.open_price;
                        }
                        else   // If Drop
                        {
                            shangYingXian = sh.max_price - sh.open_price;
                            xiaYingXian = sh.close_price - sh.min_price;
                            body = sh.open_price - sh.close_price;
                        }

                        if (shangYingXian > 2*xiaYingXian && shangYingXian > 2*body)
                        {
                            Console.WriteLine($"Sell {s.stock_code} : {d.trading_date}");
                        }
                    }
                }
            }
        }
    }
}

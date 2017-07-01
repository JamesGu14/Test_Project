using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DomainModels
{
    public static class Extensions
    {
        public static market_index_history ConvertMarketIndexExtension(this string[] lineContent, int market_index_id, int dateId, DateTime trading_date, market_index_history lastDayRecord)
        {
            if (lineContent.Count() != 7)
            {
                Console.WriteLine("Line error: [" + lineContent + "]");
                return null;
            }

            var dayHistory = new market_index_history
            {
                market_index_id = market_index_id,
                trading_date_id = dateId,
                min_price = decimal.Parse(lineContent[3]),
                max_price = decimal.Parse(lineContent[2]),
                trade_num = (long) decimal.Parse(lineContent[5]),
                trade_money = (long) decimal.Parse(lineContent[6]),
                close_price = decimal.Parse(lineContent[4]),
                open_price = decimal.Parse(lineContent[1]),
                date = trading_date
            };

            dayHistory.diff_money = dayHistory.max_price - dayHistory.min_price;
            if (lastDayRecord != null)
            {
                dayHistory.diff_rate = (decimal) Math.Round((double)((dayHistory.close_price - lastDayRecord.close_price) * 100 / lastDayRecord.close_price), 2);
            }

            return dayHistory;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.DomainModels;

namespace Common
{
    public class CandleLineAnalyze
    {
        /// <summary>
        /// 看跌吞没
        /// </summary>
        public static bool IsKanDieTunMo(StockHistoryAndIndicator stockToday, StockHistoryAndIndicator stockYesterday)
        {
            // 非上涨形态
            if (stockToday.StockMa.ma30_raisedays < 5)
            {
                return false;
            }

            // 
            if (stockYesterday.StockHistory.min_price <= stockYesterday.StockMa.ma5 ||
                stockYesterday.StockHistory.min_price <= stockYesterday.StockMa.ma10)
            {
                return false;
            }

            // 前一根阳线实体超过3%
            if ((stockYesterday.StockHistory.close_price - stockYesterday.StockHistory.open_price)/
                stockYesterday.StockHistory.open_price < (decimal)0.03)
            {
                return false;
            }

            // 第二天trade_num需大于第一天
            if (stockToday.StockHistory.trade_num < stockYesterday.StockHistory.trade_num)
            {
                return false;
            }

            // 
            if (stockToday.StockHistory.open_price > stockToday.StockHistory.close_price &&
                stockYesterday.StockHistory.open_price < stockYesterday.StockHistory.close_price &&
                stockToday.StockHistory.open_price > stockYesterday.StockHistory.close_price &&
                stockToday.StockHistory.close_price < stockYesterday.StockHistory.open_price)
            {
                if (stockToday.StockHistory.trade_num > stockYesterday.StockHistory.trade_num)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 乌云盖顶
        /// </summary>
        public static bool IsWuYunGaiDing(StockHistoryAndIndicator stockToday, StockHistoryAndIndicator stockYesterday)
        {
            // 非上涨形态
            if (stockToday.StockMa.ma30_raisedays < 5)
            {
                return false;
            }

            // TODO: 转势信号不那么强烈

            return false;
        }

        /// <summary>
        /// 倾盆大雨
        /// </summary>
        /// <returns></returns>
        public static bool IsQingPenDaYu(StockHistoryAndIndicator stockToday, StockHistoryAndIndicator stockYesterday)
        {
            // 非上涨形态
            if (stockToday.StockMa.ma30_raisedays < 5)
            {
                return false;
            }

            // 前一根阳线实体超过3%
            if (stockYesterday.StockHistory.min_price <= stockYesterday.StockMa.ma5 ||
                stockYesterday.StockHistory.min_price <= stockYesterday.StockMa.ma10)
            {
                return false;
            }

            // 第二天trade_num需大于第一天
            if (stockToday.StockHistory.trade_num < stockYesterday.StockHistory.trade_num)
            {
                return false;
            }

            //
            if (stockToday.StockHistory.open_price > stockToday.StockHistory.close_price &&
                stockYesterday.StockHistory.open_price < stockYesterday.StockHistory.close_price &&
                stockToday.StockHistory.open_price <= stockYesterday.StockHistory.close_price &&
                stockToday.StockHistory.open_price >= stockYesterday.StockHistory.open_price &&
                stockToday.StockHistory.close_price < stockYesterday.StockHistory.open_price)
            {
                if (stockToday.StockHistory.trade_num > stockYesterday.StockHistory.trade_num)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

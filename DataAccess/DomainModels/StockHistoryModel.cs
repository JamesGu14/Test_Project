using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DomainModels
{
    public class StockHistoryModel
    {
        public int showapi_res_code { get; set; }
        public string showapi_res_error { get; set; }
        public StockHistoryBodyModel showapi_res_body { get; set; }
    }

    public class StockHistoryBodyModel
    {
        public int ret_code { get; set; }
        public List<StockHistoryDayModel> list { get; set; }
    }

    public class StockHistoryDayModel
    {
        public string stockName { get; set; }
        public decimal trade_money { get; set; }
        public decimal diff_money { get; set; }
        public decimal open_price { get; set; }
        public string code { get; set; }
        public DateTime date { get; set; }
        public string market { get; set; }
        public decimal min_price { get; set; }
        public long trade_num { get; set; }
        public float? turnover { get; set; }
        public decimal close_price { get; set; }
        public decimal max_price { get; set; }
        public float? swing { get; set; }
        public float? diff_rate { get; set; }
    }

}

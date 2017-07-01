using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DomainModels
{
    public class MarketIndexModel
    {
        public int showapi_res_code { get; set; }
        public string showapi_res_error { get; set; }
        public MarketIndexResBodyModel showapi_res_body { get; set; }
    }

    public class MarketIndexResBodyModel
    {
        public int ret_code { get; set; }
        public string month { get; set; }
        public List<MarketIndexBodyModel> list { get; set; }
    }

    public class MarketIndexBodyModel
    {
        public decimal min_price { get; set; }
        public decimal trade_num { get; set; }
        public decimal trade_money { get; set; }
        public decimal diff_money { get; set; }
        public decimal close_price { get; set; }
        public decimal open_price { get; set; }
        public decimal max_price { get; set; }
        public DateTime date { get; set; }
        public decimal diff_rate { get; set; }
    }

}

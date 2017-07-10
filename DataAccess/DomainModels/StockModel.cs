using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DomainModels
{
    public class StockModel
    {
    }

    public class StockWithStartDate
    {
        public int StartYear { get; set; }
        public int StartMonth { get; set; }
        public string StockCode { get; set; }
        public string StockName { get; set; }
    }

    public class SHKObject
    {
        public stock Stock { get; set; }
        public stock_history StockHistory { get; set; }
        public stockkdj StockKdj { get; set; }
    }

    public class StockFullInfoObject
    {
        public stock Stock { get; set; }
        public stock_history StockHistory { get; set; }
        public stockmacd StockMacd { get; set; }
        public stockma StockMa { get; set; }
        public stockkdj StockKdj { get; set; }
    }

    public class SHMObject
    {
        public stock Stock { get; set; }
        public stock_history StockHistory { get; set; }
        public stockma StockMa { get; set; }
    }

}

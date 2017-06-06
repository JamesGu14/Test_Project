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

    
}

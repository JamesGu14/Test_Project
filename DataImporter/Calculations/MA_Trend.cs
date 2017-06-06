using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace DataImporter.Calculations
{
    public class MA_Trend
    {
        private readonly List<int> _stockIdList;

        public MA_Trend()
        {
            using (var dbContext = new StockTrackerEntities())
            {
                _stockIdList = dbContext.stocks.Where(s => s.id > 1408).Select(s => s.id).ToList();

                foreach (int stockId in _stockIdList)
                {
                    var _stockMas = dbContext.stockmas.Where(s => s.stock_id == stockId).OrderBy(sm => sm.stock_date).ToList();

                    var updatedMa = CalculateMATrend(_stockMas);

                    dbContext.Configuration.AutoDetectChangesEnabled = false;

                    updatedMa.ForEach(um =>
                    {
                        dbContext.stockmas.AddOrUpdate(um);
                        dbContext.Entry(um).State = System.Data.EntityState.Modified;
                    });
                    dbContext.SaveChanges();
                    Console.WriteLine($"{stockId} completed.");
                }
            }
        }

        public List<stockma> CalculateMATrend(List<stockma> _stockMas)
        {

            List<stockma> updatedMa = new List<stockma>();

            for (var j = 1; j < _stockMas.Count; j++)
            {
                var yesterday_stockMa = _stockMas[j - 1];
                var today_stockMa = _stockMas[j];

                int yesterday_20Raise = yesterday_stockMa.ma20_raisedays.Value;
                int yesterday_30Raise = yesterday_stockMa.ma30_raisedays.Value;
                int yesterday_20Drop = yesterday_stockMa.ma20_dropdays.Value;
                int yesterday_30Drop = yesterday_stockMa.ma30_dropdays.Value;
                decimal yesterday_ma20 = yesterday_stockMa.ma20;
                decimal yesterday_ma30 = yesterday_stockMa.ma30;

                decimal today_ma20 = today_stockMa.ma20;
                decimal today_ma30 = today_stockMa.ma30;

                if (today_ma20 > yesterday_ma20)
                {
                    if (yesterday_20Raise >= 0 && yesterday_20Drop == 0)
                    {
                        today_stockMa.ma20_raisedays = yesterday_20Raise + 1;
                    }
                    else
                    {
                        today_stockMa.ma20_raisedays = 1;
                    }
                }
                else if (today_ma20 < yesterday_ma20)
                {
                    if (yesterday_20Raise >= 0 && yesterday_20Drop == 0)
                    {
                        today_stockMa.ma20_dropdays = 1;
                    }
                    else
                    {
                        today_stockMa.ma20_dropdays = yesterday_20Drop + 1;
                    }
                }
                else // today_ma20 = yesterday_ma20
                {
                    if (yesterday_20Raise > 0 && yesterday_20Drop == 0)
                    {
                        today_stockMa.ma20_raisedays = yesterday_20Raise + 1;
                    }
                    else if (yesterday_20Raise == 0 && yesterday_20Drop > 0)
                    {
                        today_stockMa.ma20_dropdays = yesterday_20Drop + 1;
                    }
                }

                if (today_ma30 > yesterday_ma30)
                {
                    if (yesterday_30Raise >= 0 && yesterday_30Drop == 0)
                    {
                        today_stockMa.ma30_raisedays = yesterday_30Raise + 1;
                    }
                    else
                    {
                        today_stockMa.ma30_raisedays = 1;
                    }
                }
                else if (today_ma30 < yesterday_ma30)
                {
                    if (yesterday_30Raise >= 0 && yesterday_30Drop == 0)
                    {
                        today_stockMa.ma30_dropdays = 1;
                    }
                    else
                    {
                        today_stockMa.ma30_dropdays = yesterday_30Drop + 1;
                    }
                }
                else // today_ma30 = yesterday_ma30
                {
                    if (yesterday_30Raise > 0 && yesterday_30Drop == 0)
                    {
                        today_stockMa.ma30_raisedays = yesterday_30Raise + 1;
                    }
                    else if (yesterday_30Raise == 0 && yesterday_30Drop > 0)
                    {
                        today_stockMa.ma30_dropdays = yesterday_30Drop + 1;
                    }
                }

                updatedMa.Add(today_stockMa);

            }

            return updatedMa;
        }
    }
}

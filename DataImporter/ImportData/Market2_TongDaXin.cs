using DataAccess;
using DataAccess.DomainModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter.ImportData
{
    public class Market2_TongDaXin
    {
        public void ImportData(string filePath)
        {
            using (var dbContext = new StockTrackerEntities())
            using (var fs = File.OpenRead(filePath))
            using (var reader = new StreamReader(fs))
            {
                var dayList = dbContext.stock_trading_date.ToList();

                market_index_history lastDayRecord = null;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var lineContent = line.Split(',');

                    if (lineContent.Count() != 7)
                    {
                        Console.WriteLine("Line error: [" + lineContent + "]");
                        continue;
                    }

                    DateTime date;
                    if (DateTime.TryParse(lineContent[0], out date))
                    {
                        var day = dayList.FirstOrDefault(d => d.trading_date == date);

                        if (day == null)
                        {
                            Console.WriteLine("Date format issue: [" + lineContent[0] + "]");
                            continue;
                        }

                        var dayHistory = lineContent.ConvertMarketIndexExtension(1, day.id, day.trading_date, lastDayRecord);

                        if (dayHistory != null)
                        {
                            dbContext.market_index_history.Add(dayHistory);
                        }
                        lastDayRecord = dayHistory;
                    }
                    else
                    {
                        Console.WriteLine("Date does not exist in DB: [" + lineContent[0] + "]");
                        continue;
                    }
                }
                dbContext.Configuration.AutoDetectChangesEnabled = false;
                dbContext.SaveChanges();
            }
        }
    }
}

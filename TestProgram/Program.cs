using DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProgram
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new Program().ParseCsv("C:\\Projects\\sh2.csv");

            Console.WriteLine("Processing all completed.");
            Console.Read();
        }

        public void ParseCsv(string filePath)
        {
            using (var dbContext = new StockTrackerEntities())
            {
                List<stock> stocks = dbContext.stocks.ToList();


                FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                StreamReader sr = new StreamReader(fs, Encoding.UTF8);

                var line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    List<string> items = LineSplitter(line).ToList();
                    
                    if (items.Count != 7)
                    {
                        Console.WriteLine("[" + line + "] not 7 columns");
                    }

                    var stock = stocks.FirstOrDefault(s => s.stock_code == items[0]);

                    if (stock == null)
                    {
                        Console.WriteLine(items[0] + " fails");
                        continue;
                    }

                    try
                    {
                        stock.stock_name = items[3];

                        var extendInfo = new stock_extend_info
                        {
                            stock_id = stock.id,
                            listed_date = DateTime.Parse(items[4]),
                            general_capital = (long) (decimal.Parse(items[5]) * 10000),
                            circulation_capital = (long)(decimal.Parse(items[6]) * 10000)
                        };

                        dbContext.stock_extend_info.Add(extendInfo);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(items[0] + " fails: " + e.Message);
                        continue;
                    }

                    //Console.OutputEncoding = Encoding.GetEncoding(936);
                    //Console.WriteLine(items[0]);
                    //Console.WriteLine(items[2]);
                    //Console.WriteLine(items[7]);
                    //Console.WriteLine(items[8]);
                    //Console.WriteLine(items[9]);
                    //Console.WriteLine(items[15]);
                    //Console.WriteLine(items[16]);
                    //Console.WriteLine(items[17]);
                    //Console.WriteLine(items[18]);
                    //Console.WriteLine(items[19]);
                }

                dbContext.SaveChanges();
            }
        }

        private IEnumerable<string> LineSplitter(string line)
        {
            int fieldStart = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ',')
                {
                    yield return line.Substring(fieldStart, i - fieldStart).Replace("\"", "").Replace(",", "");
                    fieldStart = i + 1;
                }
                if (line[i] == '"')
                    for (i++; line[i] != '"'; i++) { }
            }

            yield return line.Substring(line.LastIndexOf(",") + 1).Replace("\"", "").Replace(",", "");
        }
    }
}

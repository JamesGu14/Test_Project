using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.DomainModels;
using StockTracker.Calculations;
using StockTracker.Hypothesis;

namespace StockTracker
{
    public class Program
    {
        public readonly List<string> STOCK_LIST;
        public Program()
        {
            string filePath = "C:\\Users\\James\\Documents\\visual studio 2015\\Projects\\StockTracker\\StockTracker\\Content\\stock_codes.txt";
            STOCK_LIST = new List<string>();
            StreamReader file = new System.IO.StreamReader(filePath);
            string line = string.Empty;
            while ((line = file.ReadLine()) != null)
            {
                //这里的Line就是您要的的数据了
                STOCK_LIST.Add(line.Replace(" ", ""));
            }

            file.Close();//关闭文件读取流
        }

        public void CallMACDHypo()
        {
            this.STOCK_LIST.ForEach(sl =>
            {
                Console.WriteLine($"Start calculating {sl} hypo");
                new Hypo_MACD(sl);
                Console.WriteLine($"{sl} hypo completed");
            });
        }

        public void CallKDJHypo()
        {
            this.STOCK_LIST.ForEach(sl =>
            {
                Console.WriteLine($"Start calculating {sl} hypo");
                new Hypo_KDJ(sl);
                Console.WriteLine($"{sl} hypo completed");
            });
        }

        public void CallCandleHypo()
        {
            this.STOCK_LIST.ForEach(sl =>
            {
                Console.WriteLine($"Start calculating {sl} hypo");
                new Hypo_Candle(sl);
                Console.WriteLine($"{sl} hypo completed");
            });
        }

        public void GetGoldenCross()
        {
            
        }

        public static void Main(string[] args)
        {
            //new Program().CallMACDHypo();
            //new Program().CallKDJHypo();
            //new Program().CallCandleHypo();

            // new Hypo_GoldenCross("000837");


            Console.WriteLine("");
            Console.WriteLine("ProgramExecuting completed");
            Console.Read();
        }
    }
}

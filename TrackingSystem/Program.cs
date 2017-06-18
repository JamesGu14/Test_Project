using System;
using System.Linq;
using DataAccess;

namespace TrackingSystem
{
    public class Program
    {
        public void KDJ_Golden()
        {
            var resultList = new KDJ_Golden().GetKDJ_GoldenStock();
            Console.WriteLine($"{resultList.Count} stocks matches KDJ Golden");
            resultList.ForEach(Console.WriteLine);
        }

        public void MA_LongHedging()
        {
            // var resultList = new MA_LongHedging().GetMA_LongHedgingStock();
            var resultList = new MA_LongHedging().GetMA_LongHedgingStock_JustStart();
            Console.WriteLine($"{resultList.Count} stocks matches MA Long Hedging");
            resultList.ForEach(Console.WriteLine);
        }

        public static void Main(string[] args)
        {
            // TODO: 参照大智慧选股宝
            // --- A.技术指标
            // A1 KDJ金叉
            // new Program().KDJ_Golden();

            // A2 均线多头排列
            new Program().MA_LongHedging();

            // A3 MACD底背离


            // A4 黄金坑底部掘金 ？？
            // 
            // --- B.K线形态
            // B1 突破在即
            // B2 空中加油
            // B3 平台整理
            // B4 底部红三兵
            // B5 早晨之星

            // --- C.资金流向
            // C1 资金连续流入
            // C2 长线资金关注
            // C3 短线资金关注

            // --- D.财务指标
            // D1 盈利能力强
            // D2 价值低估
            // D3 高成长性

            Console.WriteLine("All Completed");
            Console.Read();
        }
    }
}

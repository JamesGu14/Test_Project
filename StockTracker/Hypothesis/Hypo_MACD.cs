using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using DataImporter;

namespace StockTracker.Hypothesis
{
    public class Hypo_MACD
    {
        private readonly int _stock_id;
        private readonly Common _common;
        private readonly stock _stock;
        private readonly List<stock_history> _stockHistory;
        private readonly List<stockmacd> _stockMacd;

        public Hypo_MACD(string stock_code)
        {
            _common = new Common();
            using (var dbContext = new StockTrackerEntities())
            {
                try
                {
                    this._stock = dbContext.stocks.FirstOrDefault(db => db.stock_code == stock_code);
                    this._stock_id = _stock.id;
                    this._stockHistory = dbContext.stock_history.Where(db => db.stock_id == _stock.id).OrderBy(sh => sh.stock_day).ToList();
                    this._stockMacd = dbContext.stockmacds.Where(db => db.stock_id == _stock.id).OrderBy(sh => sh.stock_day).ToList();

                    ExecuteHypos();
                }
                catch (Exception e)
                {
                    // throw e;
                }
            }
        }

        /// <summary>
        ///  Execute hypos and write into db
        /// </summary>
        private void ExecuteHypos()
        {
            List<hypothesis_result> hypoResultList = new List<hypothesis_result>();
            hypoResultList.Add(Hypo1());
            hypoResultList.Add(Hypo2());
            hypoResultList.Add(Hypo3());
            hypoResultList.Add(Hypo4());
            hypoResultList.Add(Hypo5());
            hypoResultList.Add(Hypo6());
            hypoResultList.Add(Hypo7());
            hypoResultList.Add(Hypo8());
            hypoResultList.AddRange(Hypo9());

            _common.SaveHypos(hypoResultList);
        }

        #region 预测上涨
        // Hypothesis 1: 
        // Day 2 MACD > Day 1 MACD ==> Day 3 raise
        private hypothesis_result Hypo1()
        {
            float correct = 0;
            float incorrect = 0;
            for (var i = 2; i < _stockHistory.Count - 1; i++)
            {
                var stock_day2 = _stockHistory[i];
                var stock_day2_macd = _stockMacd[i].macd;

                var stock_day1_macd = _stockMacd[i - 1].macd;

                var stock_day3 = _stockHistory[i + 1];
                if (stock_day2_macd > stock_day1_macd)
                {
                    // 预测上涨 - 今日收盘＞昨日收盘
                    if (stock_day3.close_price > stock_day2.close_price)
                    {
                        // 预测正确
                        correct ++;
                    }
                    else
                    {
                        incorrect ++;
                    }
                }

            }

            double successRate = Math.Round(correct/(incorrect + correct) * 100, 2);

            return new hypothesis_result
            {
                stock_id = _stock_id,
                comment = "预测Day 2 MACD > Day 1 MACD ==> Day 3 raise ",
                fail = (int) incorrect,
                success = (int) correct,
                hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                percentage = (float) successRate
            };
        }

        // Hypothesis 2: 
        // Day 3 MACD >= Day 2 MACD >= Day 1 MACD ==> Day 4 raise
        private hypothesis_result Hypo2()
        {
            double correct = 0;
            double incorrect = 0;
            for (var i = 3; i < _stockHistory.Count - 2; i++)
            {
                var stock_day3 = _stockHistory[i];
                var stock_day3_macd = _stockMacd[i].macd;

                var stock_day2_macd = _stockMacd[i - 1].macd;

                var stock_day1_macd = _stockMacd[i - 2].macd;

                var stock_day4 = _stockHistory[i + 1];
                if (stock_day3_macd > stock_day2_macd && stock_day2_macd > stock_day1_macd)
                {
                    // 预测上涨 - 今日收盘＞昨日收盘
                    if (stock_day4.close_price > stock_day3.close_price)
                    {
                        // 预测正确
                        correct++;
                    }
                    else
                    {
                        incorrect++;
                    }
                }

            }
            double successRate = Math.Round(correct / (incorrect + correct) * 100, 2);

            return new hypothesis_result
            {
                stock_id = _stock_id,
                comment = "Day 3 MACD >= Day 2 MACD >= Day 1 MACD ==> Day 4 raise",
                fail = (int)incorrect,
                success = (int)correct,
                hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                percentage = (float)successRate
            };
        }

        // Hypothesis 3: 
        // Day 1 MACD <= Day 2 MACD <= 0 < Day 3 MACD < Day 4 MACD ==> Day 5 raise
        private hypothesis_result Hypo3()
        {
            double correct = 0;
            double incorrect = 0;
            for (var i = 4; i < _stockHistory.Count - 3; i++)
            {
                var stock_day4 = _stockHistory[i];
                var stock_day4_macd = _stockMacd[i].macd;

                var stock_day3_macd = _stockMacd[i - 1].macd;

                var stock_day2_macd = _stockMacd[i - 2].macd;

                var stock_day1_macd = _stockMacd[i - 3].macd;

                var stock_day5 = _stockHistory[i + 1];
                if (stock_day1_macd < stock_day2_macd && stock_day2_macd < 0 &&
                    0 < stock_day3_macd && stock_day3_macd < stock_day4_macd)
                {
                    // 预测上涨 - 今日收盘＞昨日收盘
                    if (stock_day5.close_price > stock_day4.close_price)
                    {
                        // 预测正确
                        correct++;
                    }
                    else
                    {
                        incorrect++;
                    }
                }

            }
            double successRate = Math.Round(correct / (incorrect + correct) * 100, 2);

            return new hypothesis_result
            {
                stock_id = _stock_id,
                comment = "Day 1 MACD <= Day 2 MACD <= 0 < Day 3 MACD < Day 4 MACD ==> Day 5 raise ",
                fail = (int)incorrect,
                success = (int)correct,
                hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                percentage = (float)successRate
            };
        }

        // Hypothesis 4: 
        // Day 1 MACD <= Day 2 MACD <= 0 < Day 3 MACD < Day 4 MACD ==> 之后三天会涨
        private hypothesis_result Hypo4()
        {
            double correct = 0;
            double incorrect = 0;
            for (var i = 4; i < _stockHistory.Count - 5; i++)
            {
                var stock_day4 = _stockHistory[i];
                var stock_day4_macd = _stockMacd[i].macd;

                var stock_day3_macd = _stockMacd[i - 1].macd;

                var stock_day2_macd = _stockMacd[i - 2].macd;

                var stock_day1_macd = _stockMacd[i - 3].macd;

                var stock_day7 = _stockHistory[i + 3];
                if (stock_day1_macd < stock_day2_macd && stock_day2_macd < 0 &&
                    0 < stock_day3_macd && stock_day3_macd < stock_day4_macd)
                {
                    // 预测上涨 - 今日收盘＞昨日收盘
                    if (stock_day7.close_price > stock_day4.close_price)
                    {
                        // 预测正确
                        correct++;
                    }
                    else
                    {
                        incorrect++;
                    }
                    //Console.WriteLine("变化率： {0}", (stock_day7.close_price / stock_day4.close_price) - 1);
                }

            }
            double successRate = Math.Round(correct / (incorrect + correct) * 100, 2);

            return new hypothesis_result
            {
                stock_id = _stock_id,
                comment = "Day 1 MACD <= Day 2 MACD <= 0 < Day 3 MACD < Day 4 MACD ==> 之后三天会涨",
                fail = (int)incorrect,
                success = (int)correct,
                hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                percentage = (float)successRate
            };
        }

        // Hypothesis 5: 
        // Day 1 MACD < Day 2 MACD < Day 3 MACD < Day 4 MACD <= 0 ==> 之后三天会涨
        private hypothesis_result Hypo5()
        {
            double correct = 0;
            double incorrect = 0;
            for (var i = 4; i < _stockHistory.Count - 4; i++)
            {
                var stock_day4 = _stockHistory[i];
                var stock_day4_macd = _stockMacd[i].macd;

                var stock_day3_macd = _stockMacd[i - 1].macd;

                var stock_day2_macd = _stockMacd[i - 2].macd;

                var stock_day1_macd = _stockMacd[i - 3].macd;

                var stock_day7 = _stockHistory[i + 3];
                if (stock_day1_macd < stock_day2_macd && stock_day2_macd < stock_day3_macd 
                    && stock_day3_macd < stock_day4_macd && stock_day4_macd < 0)
                {
                    // 预测上涨 - 今日收盘＞昨日收盘
                    if (stock_day7.close_price > stock_day4.close_price)
                    {
                        // 预测正确
                        correct++;
                    }
                    else
                    {
                        incorrect++;
                    }
                    // Console.WriteLine("变化率： {0}", (stock_day7.close_price / stock_day4.close_price) - 1);
                }

            }
            double successRate = Math.Round(correct / (incorrect + correct) * 100, 2);

            return new hypothesis_result
            {
                stock_id = _stock_id,
                comment = "Day 1 MACD < Day 2 MACD < Day 3 MACD < Day 4 MACD <= 0 ==> 之后三天会涨",
                fail = (int)incorrect,
                success = (int)correct,
                hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                percentage = (float)successRate
            };
        }

        // Hypothesis 6: 
        // Day 1 MACD < Day 2 MACD < Day 3 MACD < Day 4 MACD <= 0 ==> 之后五天会涨
        private hypothesis_result Hypo6()
        {
            double correct = 0;
            double incorrect = 0;
            for (var i = 4; i < _stockHistory.Count - 6; i++)
            {
                var stock_day4 = _stockHistory[i];
                var stock_day4_macd = _stockMacd[i].macd;

                var stock_day3_macd = _stockMacd[i - 1].macd;

                var stock_day2_macd = _stockMacd[i - 2].macd;

                var stock_day1_macd = _stockMacd[i - 3].macd;

                var stock_day9 = _stockHistory[i + 5];
                if (stock_day1_macd < stock_day2_macd && stock_day2_macd < stock_day3_macd
                    && stock_day3_macd < stock_day4_macd && stock_day4_macd < 0)
                {
                    // 预测上涨 - 今日收盘＞昨日收盘
                    if (stock_day9.close_price > stock_day4.close_price)
                    {
                        // 预测正确
                        correct++;
                    }
                    else
                    {
                        incorrect++;
                    }
                    // Console.WriteLine("变化率： {0}", (stock_day7.close_price / stock_day4.close_price) - 1);
                }

            }
            double successRate = Math.Round(correct / (incorrect + correct) * 100, 2);

            return new hypothesis_result
            {
                stock_id = _stock_id,
                comment = "Day 1 MACD < Day 2 MACD < Day 3 MACD < Day 4 MACD <= 0 ==> 之后五天会涨 ",
                fail = (int)incorrect,
                success = (int)correct,
                hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                percentage = (float)successRate
            };
        }

        // Hypothesis 5: 
        // Day 1 MACD < Day 2 MACD < Day 3 MACD < Day 4 MACD < Day 5 MACD <= 0 ==> 之后三天会涨
        private hypothesis_result Hypo7()
        {
            double correct = 0;
            double incorrect = 0;
            for (var i = 5; i < _stockHistory.Count - 4; i++)
            {
                var stock_day5_macd = _stockMacd[i].macd;

                var stock_day4 = _stockHistory[i - 1];
                var stock_day4_macd = _stockMacd[i - 1].macd;

                var stock_day3_macd = _stockMacd[i - 2].macd;

                var stock_day2_macd = _stockMacd[i - 3].macd;

                var stock_day1_macd = _stockMacd[i - 4].macd;

                var stock_day8 = _stockHistory[i + 3];
                if (stock_day1_macd < stock_day2_macd && stock_day2_macd < stock_day3_macd
                    && stock_day3_macd < stock_day4_macd && stock_day4_macd < stock_day5_macd && stock_day5_macd <= 0)
                {
                    // 预测上涨 - 今日收盘＞昨日收盘
                    if (stock_day8.close_price > stock_day4.close_price)
                    {
                        // 预测正确
                        correct++;
                    }
                    else
                    {
                        incorrect++;
                    }
                    // Console.WriteLine("变化率： {0}", (stock_day7.close_price / stock_day4.close_price) - 1);
                }

            }
            double successRate = Math.Round(correct / (incorrect + correct) * 100, 2);

            return new hypothesis_result
            {
                stock_id = _stock_id,
                comment = "Day 1 MACD < Day 2 MACD < Day 3 MACD < Day 4 MACD < Day 5 MACD <= 0 ==> 之后三天会涨 ",
                fail = (int)incorrect,
                success = (int)correct,
                hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                percentage = (float)successRate
            };
        }

        // Hypothesis 6: 
        // Day 1 MACD <= Day 2 MACD <= Day 3 MACD < Day 4 MACD < Day 5 MACD <= 0 ==> 之后五天会涨
        private hypothesis_result Hypo8()
        {
            double correct = 0;
            double incorrect = 0;
            for (var i = 4; i < _stockHistory.Count - 6; i++)
            {
                var stock_day5_macd = _stockMacd[i].macd;

                var stock_day4 = _stockHistory[i - 1];
                var stock_day4_macd = _stockMacd[i - 1].macd;

                var stock_day3_macd = _stockMacd[i - 2].macd;

                var stock_day2_macd = _stockMacd[i - 3].macd;

                var stock_day1_macd = _stockMacd[i - 4].macd;

                var stock_day9 = _stockHistory[i + 5];
                if (stock_day1_macd < stock_day2_macd && stock_day2_macd < stock_day3_macd
                    && stock_day3_macd < stock_day4_macd && stock_day4_macd < stock_day5_macd && stock_day5_macd <= 0)
                {
                    // 预测上涨 - 今日收盘＞昨日收盘
                    if (stock_day9.close_price > stock_day4.close_price)
                    {
                        // 预测正确
                        correct++;
                    }
                    else
                    {
                        incorrect++;
                    }
                    // Console.WriteLine("变化率： {0}", (stock_day7.close_price / stock_day4.close_price) - 1);
                }

            }
            double successRate = Math.Round(correct / (incorrect + correct) * 100, 2);

            return new hypothesis_result
            {
                stock_id = _stock_id,
                comment = "Day 1 MACD <= Day 2 MACD <= Day 3 MACD < Day 4 MACD < Day 5 MACD <= 0 ==> 之后五天会涨 ",
                fail = (int)incorrect,
                success = (int)correct,
                hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                percentage = (float)successRate
            };
        }

        // Hypothesis 9
        private List<hypothesis_result> Hypo9()
        {
            double correct1 = 0;
            double incorrect1 = 0;

            double correct2 = 0;
            double incorrect2 = 0;

            double correct3 = 0;
            double incorrect3 = 0;

            for (var i = 1; i < _stockHistory.Count - 1; i++)
            {
                var stock_day2 = _stockHistory[i];
                var stock_day2_macd = _stockMacd[i].macd;

                var stock_day1_macd = _stockMacd[i - 1].macd;

                var stock_day3 = _stockHistory[i + 1];

                // 9.1
                if (stock_day2_macd - stock_day1_macd > 0.02)
                {
                    // 预测上涨 - 今日收盘＞昨日收盘
                    if (stock_day3.close_price > stock_day2.close_price)
                    {
                        // 预测正确
                        correct1++;
                    }
                    else
                    {
                        incorrect1++;
                    }
                    // Console.WriteLine("变化率： {0}", (stock_day7.close_price / stock_day4.close_price) - 1);
                }

                // 9.2
                if (i + 3 < _stockHistory.Count)
                {
                    var stock_day5 = _stockHistory[i + 3];
                    if (stock_day5.close_price > stock_day2.close_price)
                    {
                        // 预测正确
                        correct2++;
                    }
                    else
                    {
                        incorrect2++;
                    }
                }

                // 9.3
                if (i + 5 < _stockHistory.Count)
                {
                    var stock_day7 = _stockHistory[i + 5];
                    if (stock_day7.close_price > stock_day2.close_price)
                    {
                        // 预测正确
                        correct3++;
                    }
                    else
                    {
                        incorrect3++;
                    }
                }

            }
            double successRate = Math.Round(correct1 / (incorrect1 + correct1) * 100, 2);
            double successRate2 = Math.Round(correct2 / (incorrect2 + correct2) * 100, 2);
            double successRate3 = Math.Round(correct3 / (incorrect3 + correct3) * 100, 2);

            return new List<hypothesis_result>()
            {
                new hypothesis_result()
                {
                    stock_id = _stock_id,
                    comment = "Day 1 MACD + 0.02 < Day 2 MACD ==> Day 3会涨 ",
                    fail = (int) incorrect1,
                    success = (int) correct1,
                    hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                    percentage = (float) successRate
                },
                new hypothesis_result()
                {
                    stock_id = _stock_id,
                    comment = "Day 1 MACD + 0.02 < Day 2 MACD ==> Day 5会涨 ",
                    fail = (int) incorrect2,
                    success = (int) correct2,
                    hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                    percentage = (float) successRate2
                },
                new hypothesis_result()
                {
                    stock_id = _stock_id,
                    comment = "Day 1 MACD + 0.02 < Day 2 MACD ==> Day 7会涨 ",
                    fail = (int) incorrect3,
                    success = (int) correct3,
                    hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                    percentage = (float) successRate3
                }
            };
        }

        #endregion

        // 预测下跌
        // Hypothesis 1:
        // 
    }
}

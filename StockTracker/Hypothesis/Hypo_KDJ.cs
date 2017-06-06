using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace StockTracker.Hypothesis
{
    public class Hypo_KDJ
    {
        private readonly int _stock_id;
        private readonly stock _stock;
        private readonly List<stock_history> _stockHistory;
        private readonly List<stockkdj> _stockKdjs;
        private readonly List<hypothesis_result> _hypothesisResults; 
        private readonly Common _common;

        public Hypo_KDJ(string stock_code)
        {
            _common = new Common();
            using (var dbContext = new StockTrackerEntities())
            {
                try
                {
                    this._stock = dbContext.stocks.FirstOrDefault(db => db.stock_code == stock_code);
                    this._stock_id = _stock.id;
                    this._stockHistory = dbContext.stock_history.Where(db => db.stock_id == _stock.id).OrderBy(sh => sh.stock_day).ToList();
                    this._stockKdjs = dbContext.stockkdjs.Where(db => db.stock_id == _stock.id).ToList();

                    if (_stockHistory.Count != _stockKdjs.Count)
                    {
                        Console.WriteLine($"{stock_code} KDJ count does not equal to Stock_History Count.");
                        return;
                    }

                    this._hypothesisResults = dbContext.hypothesis_result.Where(hs => hs.stock_id == _stock_id).ToList();

                    ExecuteHypos();
                }
                catch (Exception e)
                {
                    // throw e;
                }
            }
        }

        private void ExecuteHypos()
        {
            List<hypothesis_result> hypoResultList = new List<hypothesis_result>();

            hypoResultList.Add(Hypo101());
            hypoResultList.Add(Hypo102());
            hypoResultList.Add(Hypo103());

            _common.SaveHypos(hypoResultList);
        }

        #region 预测上涨

        // D2 > D1 && K1 < D1 && J1 < D1 && K2 > D2 && J2 > D2 && J2,D2,K2 < 50
        // Day3 Raise
        private hypothesis_result Hypo101()
        {
            // Check duplicate
            var hypo_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
            if (_hypothesisResults.Any(hr => hr.hypo == hypo_name))
            {
                Console.WriteLine($"{hypo_name} has been calculated.");
                return null;
            }
            
            int correct = 0;
            int incorrect = 0;

            for (var i = 1; i < _stockHistory.Count - 2; i++)
            {
                var stock_today = _stockHistory[i];
                var stock_day3 = _stockHistory[i + 1];

                var kdj_today = _stockKdjs[i];
                var kdj_yesterday = _stockKdjs[i - 1];

                var d1 = kdj_yesterday.d;
                var k1 = kdj_yesterday.k;
                var j1 = kdj_yesterday.j;

                var d2 = kdj_today.d;
                var k2 = kdj_today.k;
                var j2 = kdj_today.j;

                if (d2 > d1 && k1 < d1 && j1 < d1 && k2 > d2 && j2 > d2 && j2 < 50 && d2 < 50 && k2 < 50)
                {
                    if (stock_day3.close_price > stock_today.close_price)
                    {
                        correct ++;
                    }
                    else
                    {
                        incorrect ++;
                    }
                }
            }

            double successRate = Math.Round(((double)correct/(correct + incorrect))*100, 2);

            Console.WriteLine("[" + _stock_id + "] Hypo 101: " + successRate);
            return new hypothesis_result
            {
                stock_id = _stock_id,
                comment = "D2 > D1 && K1 < D1 && J1 < D1 && K2 > D2 && J2 > D2 && J2,D2,K2 < 50 => Day3 Raise",
                fail = incorrect,
                success = correct,
                hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                percentage = (float) successRate
            };
        }

        // D2 > D1 && K1 < D1 && J1 < D1 && K2 > D2 && J2 > D2 && J,D,K < 50
        // Day5 > Day2
        private hypothesis_result Hypo102()
        {
            // Check duplicate
            var hypo_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
            if (_hypothesisResults.Any(hr => hr.hypo == hypo_name))
            {
                Console.WriteLine($"{hypo_name} has been calculated.");
                return null;
            }

            int correct = 0;
            int incorrect = 0;

            for (var i = 1; i < _stockHistory.Count - 4; i++)
            {
                var stock_today = _stockHistory[i];
                var stock_day5 = _stockHistory[i + 3];

                var kdj_today = _stockKdjs[i];
                var kdj_yesterday = _stockKdjs[i - 1];

                var d1 = kdj_yesterday.d;
                var k1 = kdj_yesterday.k;
                var j1 = kdj_yesterday.j;

                var d2 = kdj_today.d;
                var k2 = kdj_today.k;
                var j2 = kdj_today.j;

                if (d2 > d1 && k1 < d1 && j1 < d1 && k2 > d2 && j2 > d2 && j2 < 50 && d2 < 50 && k2 < 50)
                {
                    if (stock_day5.close_price > stock_today.close_price)
                    {
                        correct++;
                    }
                    else
                    {
                        incorrect++;
                    }
                }
            }

            double successRate = Math.Round(((double)correct / (correct + incorrect)) * 100, 2);

            Console.WriteLine($"[{_stock_id}] {hypo_name}: {successRate}");
            return new hypothesis_result
            {
                stock_id = _stock_id,
                comment = "D2 > D1 && K1 < D1 && J1 < D1 && K2 > D2 && J2 > D2 && J2,D2,K2 < 50 => Day5 Raise",
                fail = incorrect,
                success = correct,
                hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                percentage = (float)successRate
            };
        }

        // D2 > D1 && K1 < D1 && J1 < D1 && K2 > D2 && J2 > D2 && J,D,K < 50
        // Day7 > Day2
        private hypothesis_result Hypo103()
        {
            // Check duplicate
            var hypo_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
            if (_hypothesisResults.Any(hr => hr.hypo == hypo_name))
            {
                Console.WriteLine($"{hypo_name} has been calculated.");
                return null;
            }

            int correct = 0;
            int incorrect = 0;

            for (var i = 1; i < _stockHistory.Count - 6; i++)
            {
                var stock_today = _stockHistory[i];
                var stock_day7 = _stockHistory[i + 5];

                var kdj_today = _stockKdjs[i];
                var kdj_yesterday = _stockKdjs[i - 1];

                var d1 = kdj_yesterday.d;
                var k1 = kdj_yesterday.k;
                var j1 = kdj_yesterday.j;

                var d2 = kdj_today.d;
                var k2 = kdj_today.k;
                var j2 = kdj_today.j;

                if (d2 > d1 && k1 < d1 && j1 < d1 && k2 > d2 && j2 > d2 && j2 < 50 && d2 < 50 && k2 < 50)
                {
                    if (stock_day7.close_price > stock_today.close_price)
                    {
                        correct++;
                    }
                    else
                    {
                        incorrect++;
                    }
                }
            }

            double successRate = Math.Round(((double)correct / (correct + incorrect)) * 100, 2);

            Console.WriteLine("[" + _stock_id + "] Hypo 103: " + successRate);
            return new hypothesis_result
            {
                stock_id = _stock_id,
                comment = "D2 > D1 && K1 < D1 && J1 < D1 && K2 > D2 && J2 > D2 && J2,D2,K2 < 50 => Day7 Raise",
                fail = incorrect,
                success = correct,
                hypo = System.Reflection.MethodBase.GetCurrentMethod().Name,
                percentage = (float)successRate
            };
        }

        #endregion
    }
}

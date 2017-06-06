using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace StockTracker.Hypothesis
{
    public class Hypo_Candle
    {
        private readonly int _stock_id;
        private readonly stock _stock;
        private readonly List<stock_history> _stockHistory;
        private readonly List<hypothesis_result> _hypothesisResults;
        private readonly List<stockma> _stockMas; 
        private readonly Common _common;

        public Hypo_Candle(string stock_code)
        {
            _common = new Common();
            using (var dbContext = new StockTrackerEntities())
            {
                try
                {
                    this._stock = dbContext.stocks.FirstOrDefault(db => db.stock_code == stock_code);
                    this._stock_id = _stock.id;
                    this._stockHistory = dbContext.stock_history.Where(db => db.stock_id == _stock.id).OrderBy(sh => sh.stock_day).ToList();

                    this._hypothesisResults = dbContext.hypothesis_result.Where(hs => hs.stock_id == _stock_id).ToList();
                    this._stockMas = dbContext.stockmas.Where(sm => sm.stock_id == _stock_id).OrderBy(sh => sh.stock_date).ToList();


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

            //hypoResultList.Add(Hypo201());
            //hypoResultList.Add(Hypo202());
            //hypoResultList.Add(Hypo203());

            //hypoResultList.Add(Hypo204());
            //hypoResultList.Add(Hypo205());
            //hypoResultList.Add(Hypo206());

            hypoResultList.Add(Hypo207());
            hypoResultList.Add(Hypo208());
            hypoResultList.Add(Hypo209());

            _common.SaveHypos(hypoResultList);
        }

        #region 预测上涨

        // 不考虑成交量
        // 锤子线
        // 股价超过锤子线后可短线买入
        // Hypo: 锤子线第二天如果收盘高于锤子天，之后一天上涨
        private hypothesis_result Hypo201()
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

            for (var i = 0; i < _stockHistory.Count - 2; i ++)
            {
                var stock_today = _stockHistory[i];
                var stock_tmr = _stockHistory[i + 1];
                var stock_day3 = _stockHistory[i + 2];

                var open_price = stock_today.open_price;
                var close_price = stock_today.close_price;
                var max_price = stock_today.max_price;
                var min_price = stock_today.min_price;
                if (close_price == max_price && (open_price - min_price) >= 2 * (close_price - open_price)
                    && stock_tmr.close_price > stock_today.close_price) 
                {
                    if (stock_day3.close_price > stock_tmr.close_price)
                    {
                        correct ++;
                    }
                    else
                    {
                        incorrect ++;
                    }
                }
            }

            double successRate = Math.Round(((double)correct / (correct + incorrect)) * 100, 2);

            string comment = "锤子线第二天如果收盘高于锤子天，之后一天上涨";
            return _common.GenerateOutput(successRate, _stock_id, hypo_name, incorrect, correct, comment);
        }

        // Hypo: 锤子线第二天如果收盘高于锤子天，之后3天上涨
        private hypothesis_result Hypo202()
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

            for (var i = 0; i < _stockHistory.Count - 4; i++)
            {
                var stock_today = _stockHistory[i];
                var stock_tmr = _stockHistory[i + 1];
                var stock_day5 = _stockHistory[i + 4];

                var open_price = stock_today.open_price;
                var close_price = stock_today.close_price;
                var max_price = stock_today.max_price;
                var min_price = stock_today.min_price;
                if (close_price == max_price && (open_price - min_price) >= 2 * (close_price - open_price)
                    && stock_tmr.close_price > stock_today.close_price)
                {
                    if (stock_day5.close_price > stock_tmr.close_price)
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

            string comment = "锤子线第二天如果收盘高于锤子天，之后3天上涨";
            return _common.GenerateOutput(successRate, _stock_id, hypo_name, incorrect, correct, comment);
        }

        // Hypo: 锤子线第二天如果收盘高于锤子天，之后5天上涨
        private hypothesis_result Hypo203()
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

            for (var i = 0; i < _stockHistory.Count - 6; i++)
            {
                var stock_today = _stockHistory[i];
                var stock_tmr = _stockHistory[i + 1];
                var stock_day5 = _stockHistory[i + 6];

                var open_price = stock_today.open_price;
                var close_price = stock_today.close_price;
                var max_price = stock_today.max_price;
                var min_price = stock_today.min_price;
                if (close_price == max_price && (open_price - min_price) >= 2 * (close_price - open_price)
                    && stock_tmr.close_price > stock_today.close_price)
                {
                    if (stock_day5.close_price > stock_tmr.close_price)
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

            string comment = "锤子线第二天如果收盘高于锤子天，之后5天上涨";
            return _common.GenerateOutput(successRate, _stock_id, hypo_name, incorrect, correct, comment);
        }

        // Hypo：启明星，之后1天上涨
        private hypothesis_result Hypo204()
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

            for (var i = 1; i < _stockHistory.Count - 2; i ++)
            {
                var stock_today = _stockHistory[i];
                var stock_yesterday = _stockHistory[i - 1];
                var stock_tmr = _stockHistory[i + 1];

                var stock_day4 = _stockHistory[i + 2];

                if (stock_yesterday.close_price < stock_yesterday.open_price &&
                    (double)Math.Abs(stock_today.close_price - stock_today.open_price) < (double)stock_today.close_price * 0.005 &&
                    stock_tmr.close_price > stock_tmr.open_price)
                {
                    if (stock_day4.close_price > stock_tmr.close_price)
                    {
                        correct ++;
                    }
                    else
                    {
                        incorrect ++;
                    }
                }
            }

            double successRate = Math.Round(((double)correct / (correct + incorrect)) * 100, 2);

            string comment = "启明星，之后1天上涨";
            return _common.GenerateOutput(successRate, _stock_id, hypo_name, incorrect, correct, comment);
        }

        // Hypo：启明星，之后3天上涨
        private hypothesis_result Hypo205()
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
                var stock_yesterday = _stockHistory[i - 1];
                var stock_tmr = _stockHistory[i + 1];

                var stock_day6 = _stockHistory[i + 4];

                if (stock_yesterday.close_price < stock_yesterday.open_price &&
                    (double)Math.Abs(stock_today.close_price - stock_today.open_price) < (double)stock_today.close_price * 0.005 &&
                    stock_tmr.close_price > stock_tmr.open_price)
                {
                    if (stock_day6.close_price > stock_tmr.close_price)
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

            string comment = "启明星，之后3天上涨";
            return _common.GenerateOutput(successRate, _stock_id, hypo_name, incorrect, correct, comment);
        }

        // Hypo：启明星，之后5天上涨
        private hypothesis_result Hypo206()
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

            for (var i = 1; i < _stockHistory.Count - 7; i++)
            {
                var stock_today = _stockHistory[i];
                var stock_yesterday = _stockHistory[i - 1];
                var stock_tmr = _stockHistory[i + 1];

                var stock_day7 = _stockHistory[i + 2];

                if (stock_yesterday.close_price < stock_yesterday.open_price &&
                    (double)Math.Abs(stock_today.close_price - stock_today.open_price) < (double)stock_today.close_price * 0.005 &&
                    stock_tmr.close_price > stock_tmr.open_price)
                {
                    if (stock_day7.close_price > stock_tmr.close_price)
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

            string comment = "启明星，之后5天上涨";
            return _common.GenerateOutput(successRate, _stock_id, hypo_name, incorrect, correct, comment);
        }

        #region 旭日东升形态
        private hypothesis_result Hypo207()
        {
            var hypo_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
            if (_hypothesisResults.Any(hr => hr.hypo == hypo_name))
            {
                Console.WriteLine($"{hypo_name} has been calculated.");
                return null;
            }

            int correct = 0;
            int incorrect = 0;

            for (var i = 1; i < _stockHistory.Count - 1; i++)
            {
                var stock_today = _stockHistory[i];
                var stock_yesterday = _stockHistory[i - 1];
                var stock_tmr = _stockHistory[i + 1];
                var stock_today_ma = _stockMas[i];

                // MA30 向下趋势
                // yesterday 是中、大阴线，today开盘高于yesterday收盘，收盘高于yesterday开盘，中、大阳线
                if (stock_yesterday.diff_rate < -2 && stock_today.diff_rate > 2 && stock_today.max_price < stock_today_ma.ma30 &&
                    IsCandleRed(stock_today.open_price, stock_today.close_price) && !IsCandleRed(stock_yesterday.open_price, stock_yesterday.close_price) &&
                    stock_today.close_price > stock_yesterday.open_price)  //stock_today.open_price > stock_yesterday.close_price && 
                {
                    if (stock_tmr.close_price > stock_today.close_price)
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

            string comment = "曙光初现后，之后1天上涨";
            return _common.GenerateOutput(successRate, _stock_id, hypo_name, incorrect, correct, comment);
        }

        private hypothesis_result Hypo208()
        {
            var hypo_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
            if (_hypothesisResults.Any(hr => hr.hypo == hypo_name))
            {
                Console.WriteLine($"{hypo_name} has been calculated.");
                return null;
            }

            int correct = 0;
            int incorrect = 0;

            for (var i = 1; i < _stockHistory.Count - 3; i++)
            {
                var stock_today = _stockHistory[i];
                var stock_yesterday = _stockHistory[i - 1];
                var stock_day3 = _stockHistory[i + 3];
                var stock_today_ma = _stockMas[i];

                // MA30 向下趋势
                // yesterday 是中、大阴线，today开盘高于yesterday收盘，收盘高于yesterday开盘，中、大阳线
                if (stock_yesterday.diff_rate < -3 && stock_today.diff_rate > 3 && stock_today.max_price < stock_today_ma.ma30 &&
                    IsCandleRed(stock_today.open_price, stock_today.close_price) && !IsCandleRed(stock_yesterday.open_price, stock_yesterday.close_price) &&
                    stock_today.close_price > stock_yesterday.open_price) //stock_today.open_price > stock_yesterday.close_price && 
                {
                    if (stock_day3.close_price > stock_today.close_price)
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

            string comment = "曙光初现后，之后3天上涨";
            return _common.GenerateOutput(successRate, _stock_id, hypo_name, incorrect, correct, comment);
        }

        private hypothesis_result Hypo209()
        {
            var hypo_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
            if (_hypothesisResults.Any(hr => hr.hypo == hypo_name))
            {
                Console.WriteLine($"{hypo_name} has been calculated.");
                return null;
            }

            int correct = 0;
            int incorrect = 0;

            for (var i = 1; i < _stockHistory.Count - 5; i++)
            {
                var stock_today = _stockHistory[i];
                var stock_yesterday = _stockHistory[i - 1];
                var stock_day5 = _stockHistory[i + 5];
                var stock_today_ma = _stockMas[i];

                // MA30 向下趋势
                // yesterday 是中、大阴线，today开盘高于yesterday收盘，收盘高于yesterday开盘，中、大阳线
                if (stock_yesterday.diff_rate < -3 && stock_today.diff_rate > 3 && stock_today.max_price < stock_today_ma.ma30 &&
                    IsCandleRed(stock_today.open_price, stock_today.close_price) && !IsCandleRed(stock_yesterday.open_price, stock_yesterday.close_price) &&
                    stock_today.close_price > stock_yesterday.open_price) //stock_today.open_price > stock_yesterday.close_price && 
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

            string comment = "曙光初现后，之后5天上涨";
            return _common.GenerateOutput(successRate, _stock_id, hypo_name, incorrect, correct, comment);
        }
        #endregion

        // 考虑成交量
        #endregion

        private bool IsCandleRed(decimal todayOpenPrice, decimal todayClosePrice)
        {
            if (todayClosePrice > todayOpenPrice)
            {
                return true;
            }
            return false;
        }
    }
}

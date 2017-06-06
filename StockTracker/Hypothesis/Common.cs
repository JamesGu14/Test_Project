using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace StockTracker.Hypothesis
{
    public class Common
    {
        public void SaveHypos(List<hypothesis_result> hypoResultList)
        {
            using (var dbContext = new StockTrackerEntities())
            {
                dbContext.Configuration.AutoDetectChangesEnabled = false;

                hypoResultList.ForEach(hr =>
                {
                    dbContext.hypothesis_result.Add(hr);
                });
                dbContext.SaveChanges();
            }
        }

        public hypothesis_result GenerateOutput(double successRate, int stockId, string hypoName,
            int incorrect, int correct, string comment)
        {
            Console.WriteLine($"[{stockId}] {hypoName}: {successRate}, happens: {incorrect + correct}");
            return new hypothesis_result
            {
                stock_id = stockId,
                comment = comment,
                fail = incorrect,
                success = correct,
                hypo = hypoName,
                percentage = (float)successRate
            };
        }
    }
}

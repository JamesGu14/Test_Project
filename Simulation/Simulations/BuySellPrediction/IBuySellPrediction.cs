using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DataAccess.DomainModels;
using Simulation.Models;

namespace Simulation.Simulations.BuySellPrediction
{
    public interface IBuySellPrediction
    {
        bool PredictBuy(StockHistoryAndIndicator stockToday, StockHistoryAndIndicator stockYesterday);

        bool PredictSell(StockHistoryAndIndicator stockToday, StockHistoryAndIndicator stockYesterday);

        StockAnalysisResult AnalyzeStock(StockHistoryAndIndicatorsByStock stock, TradeAction action);

    }
}

using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;

namespace IPro.Indicators
{
    [Indicator("SFisher")]
    public class SFisher : Indicator
    {
        [Series("Result")]
        public IIndicatorSeries Result { get; set; } 

        protected override void Calculate(int index)
        {
            // calculate
        }
    }
}
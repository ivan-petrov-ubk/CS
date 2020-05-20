using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;

namespace IPro.Samples
{
    [Indicator("Sample Money Flow Index", LevelsAllowed = true)]
    [IndicatorLevel(20, Name = "LR_Min")]
    [IndicatorLevel(80, Name = "LR_Max")]
    public class SampleMoneyFlowIndex : Indicator
    {
        [Parameter("LR_Period", MinValue = 1, DefaultValue = 14)]
        public int Period { get; set; }

        [Series("LR_Main", Color = Color.LightSeaGreen)]
        public IIndicatorSeries MainSeries { get; set; }

        protected override int TotalPeriod
        {
            get { return Period; }
        }

        protected override void Calculate(int index)
        {
            var positiveMf = 0.0;
            var negativeMf = 0.0;
            var currentTp = GetPrice(Bars[index], PriceMode.Typical);

            for (var k = index - 1; k >= index - Period; k--)
            {
                var previousTp = GetPrice(Bars[k], PriceMode.Typical);
                if (currentTp > previousTp)
                {
                    positiveMf += Bars[k].TickCount*currentTp;
                }
                else if (currentTp < previousTp)
                {
                    negativeMf += Bars[k].TickCount * currentTp;
                }

                currentTp = previousTp;
            }

            if (Math.Abs(negativeMf - 0.0) > double.Epsilon)
            {
                MainSeries[index] = 100.0 - 100.0/(1.0 + positiveMf/negativeMf);
            }
            else
            {
                MainSeries[index] = 100;
            }
        }
    }
}

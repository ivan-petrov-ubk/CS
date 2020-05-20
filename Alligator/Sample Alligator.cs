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
    [Indicator("Sample Alligator", Location = IndicatorLocation.Chart, LevelsAllowed = false)]
    public class SampleAlligator : Indicator
    {
        [Parameter("LR_Jaws_Period", DefaultValue = 13, MinValue = 1)]
        public int JawsPeriod { get; set; }

        [Parameter("LR_Jaws_Shift", DefaultValue = 8, MinValue = -1000, MaxValue = 1000)]
        public int JawsShift { get; set; }

        [Parameter("LR_Teeth_Period", DefaultValue = 8, MinValue = 1)]
        public int TeethPeriod { get; set; }

        [Parameter("LR_Teeth_Shift", DefaultValue = 5, MinValue = -1000, MaxValue = 1000)]
        public int TeethShift { get; set; }

        [Parameter("LR_Lips_Period", DefaultValue = 5, MinValue = 1)]
        public int LipsPeriod { get; set; }

        [Parameter("LR_Lips_Shift", DefaultValue = 3, MinValue = -1000, MaxValue = 1000)]
        public int LipsShift { get; set; }

        [Parameter("LR_Method", DefaultValue = MaMethods.Smma)]
        public MaMethods Method { get; set; }

        [Parameter("LR_Price_Mode", DefaultValue = PriceMode.Median)]
        public PriceMode PriceMode { get; set; }

        [Series("LR_Jaws", Color = Color.Blue)]
        public IIndicatorSeries JawsSeries { get; set; }

        [Series("LR_Teeth", Color = Color.Red)]
        public IIndicatorSeries TeethSeries { get; set; }

        [Series("LR_Lips", Color = Color.Lime)]
        public IIndicatorSeries LipsSeries { get; set; }

        protected override int TotalPeriod
        {
            get { return Math.Max(JawsPeriod, Math.Max(TeethPeriod, LipsPeriod)); }
        }

        protected override void Calculate(int index)
        {
            JawsSeries[index + JawsShift] = Series.Ma(index, JawsPeriod, PriceMode, Method);
            TeethSeries[index + TeethShift] = Series.Ma(index, TeethPeriod, PriceMode, Method);
            LipsSeries[index + + LipsShift] = Series.Ma(index, LipsPeriod, PriceMode, Method);
        }
    }
}

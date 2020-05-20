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
    [Indicator("Sample Fisher Transform Oscillator", LevelsAllowed = true)]
    public sealed class SampleFisherTransformOscillator : Indicator
    {
        [Parameter("LR_Period", DefaultValue = 18, MinValue = 1)]
        public int Period { get; set; }

        [Parameter("LR_Ma1Period", DefaultValue = 9, MinValue = 1)]
        public int Ma1Period { get; set; }

        [Parameter("LR_Ma1Method", DefaultValue = MaMethods.Sma)]
        public MaMethods Ma1Method { get; set; }

        [Parameter("LR_Ma2Period", DefaultValue = 24, MinValue = 1)]
        public int Ma2Period { get; set; }

        [Parameter("LR_Ma2Method", DefaultValue = MaMethods.Sma)]
        public MaMethods Ma2Method { get; set; }

        [Series("LR_Up", Color = Color.LightSeaGreen, Style = SeriesStyle.Histogram)]
        public IIndicatorSeries UpSeries { get; set; }

        [Series("LR_Down", Color = Color.DarkGoldenrod, Style = SeriesStyle.Histogram)]
        public IIndicatorSeries DownSeries { get; set; }

        [Series("LR_Fisher", Color = Color.Gray)]
        public IIndicatorSeries FisherSeries { get; set; }

        [Series("LR_Ma1", Color = Color.Blue)]
        public IIndicatorSeries Ma1Series { get; set; }

        [Series("LR_Ma2", Color = Color.Red)]
        public IIndicatorSeries Ma2Series { get; set; }
        
		private ISeries<double> _values;

        protected override int TotalPeriod
        {
            get { return Period; }
        }

        protected override bool DependsOnPreviousData
        {
            get { return true; }
        }

        protected override void Init()
        {
            _values = CreateSeries<double>();
        }

        protected override void Calculate(int index)
        {
            var max = GetPrice(Bars[Series.Highest(index, Period, PriceMode.High)], PriceMode.High);
            var min = GetPrice(Bars[Series.Lowest(index, Period, PriceMode.Low)], PriceMode.Low);

            var price = GetPrice(Bars[index], PriceMode.Close);

            var val = 0.33 * 2 * ((price - min) / (max - min) - 0.5) + 0.67 * ValueOrDefault(_values[index - 1]);
            val = Math.Min(Math.Max(val, -0.999), 0.999);

            var fish = 0.5 * Math.Log((1.0 + val) / (1.0 - val)) + 0.5 * ValueOrDefault(FisherSeries[index - 1]);

            _values[index] = val;
            FisherSeries[index] = ValueOrDefault(fish);

            if (fish >= 0)
            {
                UpSeries[index] = fish;
                DownSeries[index] = double.NaN;
            }
            else if (fish < 0)
            {
                DownSeries[index] = fish;
                UpSeries[index] = double.NaN;
            }

            Ma1Series[index] = ValueOrDefault(Series.Ma(FisherSeries, index, Ma1Period, Ma1Method));
            Ma2Series[index] = ValueOrDefault(Series.Ma(Ma1Series, index, Ma2Period, Ma2Method));
        }
	}
}

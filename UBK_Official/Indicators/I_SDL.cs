using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Indicators.Standard;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using IPro.Model.Client.MarketData;

namespace IPro.Indicators
{
    [Indicator("SDL",  Location = IndicatorLocation.Chart)]
    public class SDL : Indicator
    {
        [Parameter("Period:", DefaultValue = 20, MinValue = 2)]
		public int period { get; set; }

		[Parameter("Method:", DefaultValue = MaMethods.Sma)]
		public MaMethods method { get; set; }
		
		[Parameter("Price:", DefaultValue = PriceMode.Close)]
		public PriceMode price { get; set; }
	
		[Series("ExtMapBuffer",	Color = Color.Blue, Style = SeriesStyle.ContinuousLine, Width = 1)]
        public IIndicatorSeries ExtMapBuffer { get; set; } 

		[Series("Uptrend",	Color = Color.SkyBlue, Style = SeriesStyle.Line, Width = 3)]
        public IIndicatorSeries Uptrend { get; set; } 

		[Series("Dntrend", Color = Color.LimeGreen, Style = SeriesStyle.Line, Width = 3)]
        public IIndicatorSeries Dntrend { get; set; } 

        ISeries<double> vect;
		ISeries<double> trend;
		
		private MovingAverage ma1;
		private MovingAverage ma2;

		private static int p = 0;
		
		protected override void Init()
		{
			vect = CreateSeries<double>();
			trend = CreateSeries<double>();
			p = (int)Math.Sqrt(period);
			
			ma1 = GetInvisibleIndicator<MovingAverage>(
												Instrument.Id,
												Timeframe,
													period,
														0,
															method,
																price	
											 );
			
			ma2 = GetInvisibleIndicator<MovingAverage>(
												Instrument.Id,
												Timeframe,
													(int) period / 2 ,
														0,
															method,
																price	
											 );
		}
		
		protected override void Calculate(int index)
        {
			p = (int)Math.Sqrt(period);
			
			vect[index] = 2 * ma2.SeriesMa[index] - ma1.SeriesMa[index];
			ExtMapBuffer[index] = Series.Sma(vect, index, p);	
			
			trend[index] = trend[index - 1];
			if (ExtMapBuffer[index] > ExtMapBuffer[index - 1]) 
				trend[index] =  1;
			if (ExtMapBuffer[index] < ExtMapBuffer[index - 1]) 
				trend[index] = -1;
			
			if (trend[index] > 0)
				{
					Uptrend[index] = ExtMapBuffer[index];
					
					if (trend[index - 1] > 0) 
						Uptrend[index - 1] = ExtMapBuffer[index - 1];
					
					Dntrend[index] = double.NaN;
				}
				else if (trend[index] < 0)
				{
					Dntrend[index] = ExtMapBuffer[index];
					if (trend[index - 1] < 0) Dntrend[index - 1] = ExtMapBuffer[index - 1];
					Uptrend[index] = double.NaN;
				}              
			
        }
    }
}
using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Indicators.Standard;
using IPro.Model.Client.MarketData;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using System.Collections.Generic;

namespace IPro.Indicators
{
    [Indicator("AltrTrend", Location = IndicatorLocation.SeparateWindow)]
    public class AltrTrend : Indicator
    {

		[Parameter("K", DefaultValue = 30)]
		public int K { get; set; }
		
		[Parameter("Kperiod", DefaultValue = 150)]
		public int Kperiod { get; set; }
		
		[Parameter("ADX Period", DefaultValue = 14)]
		public int PerADX { get; set; }

		[Parameter("ADX PriceMode", DefaultValue = PriceMode.Close)]
		public PriceMode pmADX { get; set; }
		
		[Parameter("Count Bars", DefaultValue = 200)]
		public int CountBars { get; set; }

		[Series("val1", Style = SeriesStyle.Histogram, Color = Color.Blue, Width = 2)]
        public IIndicatorSeries val1 { get; set; } 

		[Series("val2", Style = SeriesStyle.Histogram, Color = Color.Crimson, Width = 2)]
        public IIndicatorSeries val2 { get; set; } 
        
		[Series("val3", Style = SeriesStyle.Histogram, Color = Color.DarkBlue, Width = 4)]
        public IIndicatorSeries val3 { get; set; } 
        
		[Series("val4", Style = SeriesStyle.Histogram, Color = Color.Red, Width = 4)]
        public IIndicatorSeries val4 { get; set; } 

		private AverageDirectionalMovement ADX;
		
		protected override void Init()
		{
			ADX = GetInvisibleIndicator<AverageDirectionalMovement>(
																	Instrument.Id,
																	Timeframe,
																		PerADX,
																			pmADX
																	);
		}
		
        protected override void Calculate(int FromIndex, int ToIndex)
        {
			int i, shift;
   			
			int i1, i2;
   			
			double Range, AvgRange, smin, smax, SsMax, SsMin, SSP, price;

   			if(ToIndex <= (PerADX + 1)) return;

			if(ToIndex <= Bars.Range.To - 3 * CountBars) return;
			
			if(FromIndex <= Bars.Range.To - CountBars) return;
			
			
			for (shift = (ToIndex - (CountBars + PerADX) - 1); shift <= ToIndex; shift++) 
			{ 

				double mADX = ADX.SeriesMain[shift];
				mADX =	(mADX.Equals(double.NaN)? 100.0 : mADX) / Instrument.Point;
				
				SSP = Math.Ceiling(((double) Kperiod) / (mADX==0.0? 10000.0: mADX)); 
				
				Range = 0.0;
				AvgRange = 0.0;
				
				for ( i1 = shift; i1 >= (shift - (int)SSP); i1--)
				{
					AvgRange = AvgRange + Math.Abs(Bars[i1].Close - Bars[i1].Close);
				}
				Range = AvgRange / (SSP + 1.0);

				SsMax = Bars[shift].High; 
				SsMin = Bars[shift].Low; 
   				
				for ( i2 = shift; i2 >= (shift - (int)SSP + 1); i2--)
        		{
         			price = Bars[i2].High;
         			if(SsMax < price) 
					{
						SsMax = price;
					}
         			
					price = Bars[i2].Low;
         			if(SsMin >= price)  
					{
						SsMin = price;
					}
        		}
 
				smin = SsMin + (SsMax - SsMin) * ((double)K) / 100.0; 
				smax = SsMax - (SsMax - SsMin) * ((double)K) / 100.0; 
				
				val1[shift] = double.NaN;
				val2[shift] = double.NaN;
				val3[shift] = double.NaN;
				val4[shift] = double.NaN;
				
				if (Bars[shift].Close < smin)
				{
					val1[shift] = Bars[shift].Low; 
					val2[shift] = Bars[shift].High;
				}
				if (Bars[shift].Close > smax)
				{
					val3[shift] = Bars[shift].High;
					val4[shift] = Bars[shift].Low;
				}

			}
        }
    }
}
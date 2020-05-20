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
    [Indicator("WoodieCCI", Location = IndicatorLocation.SeparateWindow)]
    public class WoodieCCI : Indicator
    {

		[Parameter("A_period", DefaultValue = 14)]
		public int A_period { get; set; }
		
		[Parameter("B_period", DefaultValue = 6)]
		public int B_period { get; set; }
		
		[Parameter("num_bars", DefaultValue = 250)]
		public int num_bars { get; set; }

		[Parameter("PriceMode", DefaultValue = PriceMode.Weighted)]
		public PriceMode _priceMode { get; set; }
		

		
		[Series("SlowWoodieCCI", Color = Color.Lime, Style = SeriesStyle.ContinuousLine, Width = 1)]
		public IIndicatorSeries SlowWoodieCCI { get; set; }

		[Series("HistWoodieCCI", Color = Color.DarkGreen, Style = SeriesStyle.Histogram, Width = 2)]
		public IIndicatorSeries HistWoodieCCI { get; set; }

		[Series("FastWoodieCCI", Color = Color.DeepSkyBlue, Style = SeriesStyle.ContinuousLine, Width = 1)]
        public IIndicatorSeries FastWoodieCCI { get; set; } 


		
		public static string commodt = "nonono";
		
		private Period _period;
		
		private CommodityChannelIndex CCIindSlow; 
		private CommodityChannelIndex CCIindFast; 
		
			
			
			
		protected override void Init()
		{
			
			_period = Timeframe;	
			commodt = Instrument.Name;
			
			CCIindSlow = GetIndicator<CommodityChannelIndex>(
												Instrument.Id,
												_period,
													A_period,
														_priceMode
															);
			
			CCIindFast = GetIndicator<CommodityChannelIndex>(
												Instrument.Id,
												_period,
													B_period,
														_priceMode
															);
	
		}
		
		
		protected override void Calculate(int FromIndex, int ToIndex)
        {
            // calculate
//---- TODO: add your code here

			for (int shift = FromIndex; shift <= ToIndex; shift++)
   			{
         		FastWoodieCCI[shift] = CCIindSlow.MainSeries[shift];
         		SlowWoodieCCI[shift] = CCIindFast.MainSeries[shift];
         		HistWoodieCCI[shift] = CCIindFast.MainSeries[shift];
   			};
//----
		}
    
		protected override void Deinit()
		{
			//CCIindSlow.Dispose();
			//CCIindFast.Dispose();
		}
	}
}
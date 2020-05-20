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
    [Indicator("angAZa", Location = IndicatorLocation.SeparateWindow)]
    public class angAZa : Indicator
    {
		[Parameter("ki", DefaultValue = 4.0)]
		public double ki { get;set; }
		
		[Parameter("Bars Number", DefaultValue = 200)]
		public int BarsNumber { get; set; }
		
		[Series("azaH", Color = Color.Blue, Width = 3, Style =  SeriesStyle.Histogram)]
        public IIndicatorSeries azaH { get; set; } 

		[Series("azaL", Color = Color.DarkGreen, Width = 3, Style =  SeriesStyle.Histogram)]
        public IIndicatorSeries azaL { get; set; } 

		[Series("a0", Color = Color.Black, Width = 1)]
        public IIndicatorSeries a0 { get; set; } 

		[Series("za", Color = Color.Black, Width = 0)]
        public IIndicatorSeries za{ get; set; } 

		[Series("za2", Color = Color.Black, Width = 0)]
        public IIndicatorSeries za2 { get; set; } 
	
		private double	z, z2;
		
		protected override void Calculate(int FromIndex, int ToIndex)
        {
			if(ToIndex < Bars.Range.To - BarsNumber) return;

			int i; 
			double xpoint = Instrument.Point;

   			for(i = ToIndex - BarsNumber; i <= ToIndex; i++) 
     		{ 
				if(
					((Bars[i].Close > z) && (Bars[i].Close > Bars[i - 1].Close))
					||
					((Bars[i].Close < z) && (Bars[i].Close < Bars[i - 1].Close))
				  ) 
           		{
					z = za[i - 1] + (Bars[i].Close - za[i - 1]) / ki;
				}
				if(
					((Bars[i].Close > z2) && (Bars[i].Close < Bars[i - 1].Close))
					||
					((Bars[i].Close < z2) && (Bars[i].Close > Bars[i - 1].Close))
				  ) 
           		{	
					z2 = za2[i - 1] + (Bars[i].Close - za2[i - 1]) / ki;
				}

				if(i < (Bars.Range.To - BarsNumber + 5))
           		{
             		z = Bars[i].Close; 
             		z2 = z;
           		}

				za[i] = z;  
       			
				za2[i] = z2;
       			
				azaH[i] = ((z - z2) / xpoint) > 0.0? ((z - z2) / xpoint) : double.NaN;
       			azaL[i] = ((z - z2) / xpoint) < 0.0? ((z - z2) / xpoint) : double.NaN;
				
				a0[i] = 0.00000001;
     		}
		}
    }
}
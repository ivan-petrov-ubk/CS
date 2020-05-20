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
    [Indicator("NRTR", Location = IndicatorLocation.Chart)]
    public class NRTR : Indicator
    {

		[Parameter("AveragePeriod", DefaultValue = 10)]
		public int AveragePeriod { get; set; }
		
		[Parameter("CountBars", DefaultValue = 300)]
		public int CountBars { get; set; }

		
		[Series("value1", Color = Color.DeepSkyBlue, Style = SeriesStyle.Chars, CharCode = CharCode.Lozenge, Width = 1)]
        public IIndicatorSeries value1 { get; set; } 
		
		[Series("value2", Color = Color.Lime, Style = SeriesStyle.Chars, CharCode = CharCode.Lozenge, Width = 1)]
		public IIndicatorSeries value2 { get; set; }

        protected override void Calculate(int FromIndex, int ToIndex)
        {
            // calculate
           	
			int i; 
   			double value = 0.0;
   			double trend = 0.0, dK = 0.0, AvgRange = 0.0, price = 0.0;
			
			//----
   			if(Bars.Range.To <= AveragePeriod) return;

   			AvgRange=0;
   			
			i= ToIndex-1;
			for (; i >= ToIndex - AveragePeriod; i--)  
				AvgRange+= Math.Abs(Bars[i].High-Bars[i].Low);  
   			
			if (Instrument.Name == "USDJPY" || Instrument.Name == "GBPJPY" || Instrument.Name == "EURJPY")
   			{
				dK = (AvgRange/AveragePeriod)/100;
			}
   			else 
			{
				dK = AvgRange/AveragePeriod;
			}

   			if (Bars[ToIndex - CountBars + 1].Close > Bars[ToIndex - CountBars + 1].Open)
   			{
   				value1[ToIndex - CountBars + 1] = Bars[ToIndex - CountBars + 1].Close * (1 - dK);
   				trend = 1.0; 
				value2[ToIndex - CountBars + 1] = double.NaN;
   			}
   			if (Bars[ToIndex - CountBars + 1].Close < Bars[ToIndex - CountBars + 1].Open)  
   			{
   				value2[ToIndex - CountBars + 1] = Bars[ToIndex - CountBars + 1].Close * (1 + dK);
   				trend = -1.0; 
				value1[ToIndex - CountBars + 1] = double.NaN;
   			}
			//----
   			
			i = ToIndex - CountBars + 1;
   			while( i <= ToIndex)
     		{
    			value1[i] = double.NaN;
				value2[i] = double.NaN;
				
    			if (trend >= 0)
       			{
       				if (Bars[i].Close > price) 
						price = Bars[i].Close;
       				value = price * (1 - dK);
       				if (Bars[i].Close < value)
          			{
          				price = Bars[i].Close;
          				value = price * (1 + dK);
          				trend = -1.0;
          			}
       			} 
    			else
       			{ 
      				if (trend <= 0)
        			{
       					if (Bars[i].Close < price) 
							price = Bars[i].Close;
       					value = price * (1 + dK);
       					if (Bars[i].Close > value) 
          				{
          					price = Bars[i].Close;
          					value = price * (1 - dK);
          					trend = 1.0;
          				}
        			}
       			}
      			if (trend ==  1.0)  { value1[i] = value;  value2[i]=double.NaN;}
      			if (trend == -1.0)  { value2[i] = value;  value1[i]=double.NaN;}

      			i++;
     		}

		}
    }
}
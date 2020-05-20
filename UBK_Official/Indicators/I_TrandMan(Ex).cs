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
    [Indicator("TrandMan_H", Location = IndicatorLocation.SeparateWindow)]
    public class TrandMan_H : Indicator
    {

		[Parameter("TM_Period_1", DefaultValue = 7)]
		public int TM_Period_1 { get; set; }
		
		[Parameter("TM_Period_2", DefaultValue = 7)]
		public int TM_Period_2 { get; set; }
		
		[Parameter("TM_Shift_1", DefaultValue = 2)]
		public int TM_Shift_1 { get; set; }
		
		[Parameter("TM_Shift_2", DefaultValue = 2)]
		public int TM_Shift_2 { get; set; }

		[Parameter("TM_NP", DefaultValue = 2)]
		public int TM_NP { get; set; }

		private static int Max_Period;
		private static int Max_Shift; 

		//double SpanB_Buffer[];
		[Series("SpanB_Buffer", Color = Color.Blue, Style = SeriesStyle.Histogram, Width = 5 /*, CharCode = CharCode.Circle */)]
		public IIndicatorSeries SpanB_Buffer {get;set;}

		//double SpanA_Buffer[];
		[Series("SpanA_Buffer", Color = Color.Silver, Style = SeriesStyle.Histogram, Width = 4 /*, CharCode = CharCode.Circle*/)]
		public IIndicatorSeries SpanA_Buffer {get;set;}
		

		private static int a_begin;
		private static double xdiff = 0.0;
		
		protected override void Init()
		{
			Max_Shift  = Math.Max(TM_Shift_1,TM_Shift_2);
			Max_Period = Math.Max(TM_Period_1,TM_Period_2);
			
			xdiff = TM_NP * Instrument.Point * Instrument.Point;
		}
		
		
        protected override void Calculate(int FromIndex, int ToIndex)
        {
            // calculate
			int    i; 

   			i = FromIndex;

			while(i<=ToIndex)
   			{
      
      			double M1, M1b;
      			double M2, M2b;
      
      			M1  = MiddlePrice(i,				TM_Period_1);
      			M1b = MiddlePrice(i - TM_Shift_1,	TM_Period_1); 
      			M2  = MiddlePrice(i,				TM_Period_2);
      			M2b = MiddlePrice(i - TM_Shift_2,	TM_Period_2); 
     
      			//
      			double diff1 = (M1 - M1b); // up or down on short term;
      			double diff2 = (M2 - M2b); // up or down on longer term.
      			// each has three choices, hence six possibilities.
      
      			if (diff1 * diff2 <= -xdiff) //  0.0 
				{ // opposite signs
         			SpanA_Buffer[i]  = (M1 + M2) * 0.5; 
         			SpanB_Buffer[i]  = SpanA_Buffer[i]; 
      			} 
				//else 
				
      			if (diff1 * diff2 > xdiff) //  0.0 
				{
         			SpanA_Buffer[i]  =  (M1 + M2) * 0.5; 
         			SpanB_Buffer[i]  =  (M1b + M2b) * 0.5;
      			}
   				//   Comment(SpanB_Buffer[i],"\n",SpanA_Buffer[i] );
      
      			i++;
   			}

        }
		protected double MiddlePrice(int nback, int shift) 
		{
  			double H = Bars[HIGHEST(nback, shift, PriceMode.High)].High;
  			double L = Bars[LOWEST( nback, shift, PriceMode.Low )].Low;
  			return 0.5 * (H + L); 
    	}
		
		protected int HIGHEST (int ToIndex, int p, PriceMode PM)
		{
			int ci = 0;
			double Price = double.MinValue;
			
			for( int i = ToIndex - p; i <= ToIndex; i++)
			{
				if(getprice(Bars[i], PM) > Price)
				{	
					ci = i;
					Price = getprice(Bars[i], PM);
				}
			}
			
			return ci;
		}

		
		protected int LOWEST (int ToIndex, int p, PriceMode PM)
		{
			int ci = 0;
			double Price = double.MaxValue;
			
			for( int i = ToIndex - p; i <= ToIndex; i++)
			{
				if(getprice(Bars[i], PM) < Price)
				{	
					ci = i;
					Price = getprice(Bars[i], PM);
				}
			}
			
			return ci;
		}

		
		protected double getprice(Bar bar, PriceMode PM)
		{
			int xPM = (int)PM;
			switch(xPM)
			{
				case (int)PriceMode.Close :
					
					return bar.Close;
				
				case (int)PriceMode.High :
					
					return bar.High;
				
				case (int)PriceMode.Low :
				
					return bar.Low;
				
				case (int)PriceMode.Open :
				
					return bar.Open;
				
				case (int)PriceMode.Median :
				
					return (bar.High + bar.Low) * 0.5;
				
				case (int)PriceMode.Typical :
				
					return (bar.High + bar.Low + bar.Close) / 3.0 ;
				
				case (int)PriceMode.Weighted :
				
					return (bar.High + bar.Low + bar.Close + bar.Close) * 0.25;
				
				default:
					
					return bar.Close;
			}
			//return 0.0;
		}
	
	
	}
	
}
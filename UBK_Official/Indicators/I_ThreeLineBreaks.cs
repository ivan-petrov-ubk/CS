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

	[Indicator("ThreeLineBreaks", Location = IndicatorLocation.SeparateWindow)]
    public class ThreeLineBreaks : Indicator
    {

		[Parameter("LineBreaks", DefaultValue = 3)]
		public int LineBreaks { get;set; }
		
		[Parameter("CountBars", DefaultValue = 300)]
		public int CountBars { get;set; }
		

		[Series("HB", Color = Color.LimeGreen, Style = SeriesStyle.Histogram, Width = 4)]
        public IIndicatorSeries HB { get; set; } 

		[Series("LB", Color = Color.DarkBlue, Style = SeriesStyle.Histogram, Width = 4)]
        public IIndicatorSeries LB { get; set; } 


		private static double VALUE1 = 0.0;
		private static double VALUE2 = 0.0;
		
		private static int Swing = 1;
		private static int OLDSwing = 0;

		
        protected override void Calculate(int ToIndex) ///*int FromIndex,*/
        {
            // calculate

			int i = 0; 
			int shift = 0;

			if(ToIndex < CountBars) return;
			
			//---- TODO: add your code here
			
			i = ToIndex - CountBars; // (CountBars-counted_bars);//

			for(shift = i; shift <= ToIndex; shift++)
			{

				OLDSwing = Swing;

				VALUE1 = Bars[HIGHEST(shift - 1, LineBreaks, PriceMode.High)].High;
				VALUE2 = Bars[LOWEST (shift - 1, LineBreaks, PriceMode.Low )].Low ;
				
				if ((OLDSwing == 1)  &&  (Bars[shift].Low  < VALUE2)) 
				{
					Swing = -1;
				}
				
				if ((OLDSwing == -1) &&  (Bars[shift].High > VALUE1)) 
				{
					Swing = 1;
				}

				if (Swing == 1) 
				{ 
					HB[shift] = Bars[shift].High; 
					LB[shift] = Bars[shift].Low; 
				}

				if (Swing == -1)
				{ 
					HB[shift] = Bars[shift].Low; 
					LB[shift] = Bars[shift].High; 
				}
 			//----
			}
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
			return 0.0;
		}

    }
}
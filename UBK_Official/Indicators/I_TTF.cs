using System;
//using System.Threading;
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
    [Indicator("TTFInd", Location = IndicatorLocation.SeparateWindow)]
    public class TTFInd : Indicator
    {

		[Parameter("TTFbars", DefaultValue = 8, MinValue = 3)]
		public int TTFbars { get;set; } 	//15 = default number of bars for computation

		[Parameter("TopLine", DefaultValue = 75, MinValue = 10)]
		public int TopLine { get;set;}

		[Parameter("BottomLine", DefaultValue = -75, MaxValue = -10)]
		public int BottomLine { get;set; }

		[Parameter("t3_period", DefaultValue = 3, MinValue = 2)]
		public int t3_period { get;set; }
		
		[Parameter("b", DefaultValue = 0.7, MinValue = 0.0)]
		public double b { get;set; }

//		[Parameter("DebugMode :", DefaultValue = true)]
//		public bool DebugMode { get;set; }
		

		[Series("MainSeries", Color = Color.Blue, Style = SeriesStyle.Line, Width = 2)]
        public IIndicatorSeries MS { get; set; } 

        [Series("SignalSeries", Color = Color.LightSeaGreen,  Style = SeriesStyle.Histogram, Width = 4)]
        public IIndicatorSeries SSU { get; set; } 

        [Series("SignalSeries", Color = Color.LightSalmon,  Style = SeriesStyle.Histogram, Width = 4)]
        public IIndicatorSeries SSD { get; set; } 

		private static double b2 = 0.0;
		private static double b3 = 0.0;
		
		private static double c1 = 0.0;
		private static double c2 = 0.0;
		private static double c3 = 0.0;
		private static double c4 = 0.0;

		private static double r  = 0.0;
	
		private static double w1 = 0.0;
		private static double w2 = 0.0;
		
		private  double e1 = 0.0;
		private  double e2 = 0.0;
		private  double e3 = 0.0;
		private  double e4 = 0.0;
		private  double e5 = 0.0;
		private  double e6 = 0.0;
		
		private static double  HighestHighRecent = 0.0;
		private static double  HighestHighOlder = 0.0;
		private static double  LowestLowRecent = 0.0;
		private static double  LowestLowOlder = 0.0;
		
		private static double  BuyPower  = 0.0;
		private static double  SellPower = 0.0;
		private static double  ttf = 0.0;


		
		protected override void Init()
		{
/*
			e1 = 0.0;
			e2 = 0.0;
			e3 = 0.0;
			e4 = 0.0;
			e5 = 0.0;
			e6 = 0.0;
*/
			b2 = b * b;
   			b3 = b2 * b;
   			c1 = -b3;
   			c2 = (3 * (b2 + b3));
   			c3 = -3 * (2 * b2 + b + b3);
   			c4 = (1 + 3 * b + b3 + 3 * b2);

   			r = (double)t3_period;

   			if (r < 1) r = 1;
   			r = 1 + 0.5 * (r - 1);
   			w1 = 2 / (r + 1);
   			w2 = 1 - w1;
		
		}
		
		protected override void Calculate(int FromIndex, int ToIndex)
        {
            // calculate
			int  i = FromIndex; 										
/*   			
			if(ToIndex < (2 * TTFbars + 2)) return;
*/			
				////*			
			for(int j = 0; j <= (2 * TTFbars + 2); j++)
			{
				if(Bars[ToIndex - j].Close.Equals(double.NaN)) return;
			}
				////*/			
//----
//---- <<initial zero>>
/*   			
			if((ToIndex - FromIndex) > 0)
     		{
	      		for(i = ToIndex; i >= FromIndex; i--) 
				{
					MS[i] = double.NaN;
				}
     			for(i = ToIndex; i >= FromIndex; i--) 
				{
					//SS[i] = double.NaN;
					SSU[i] = double.NaN;
					SSD[i] = double.NaN;
				}
     		}
*/
//---- %K line
   			
			// i = FromIndex; 			
			
			while(i <= ToIndex)
     		{
				
				if(e1.Equals(double.NaN)) e1 = 0.0;
				if(e2.Equals(double.NaN)) e2 = 0.0;
				if(e3.Equals(double.NaN)) e3 = 0.0;
				if(e4.Equals(double.NaN)) e4 = 0.0;
				if(e5.Equals(double.NaN)) e5 = 0.0;
				if(e6.Equals(double.NaN)) e6 = 0.0;

				HighestHighRecent = Bars[HIGHEST(i, 		  TTFbars, PriceMode.High)].High; 	
				HighestHighOlder  = Bars[HIGHEST(i - TTFbars, TTFbars, PriceMode.High)].High;
				
				LowestLowRecent   = Bars[LOWEST( i, 		  TTFbars, PriceMode.Low )].Low;
				LowestLowOlder    = Bars[LOWEST( i - TTFbars, TTFbars, PriceMode.Low )].Low;

			
      			BuyPower          = HighestHighRecent - LowestLowOlder;
      			SellPower         = HighestHighOlder - LowestLowRecent;
				
						//XXPrint("BuyPower = {0}; SellPower = {0};",BuyPower,SellPower);
				
      			ttf = (BuyPower - SellPower) / (0.5 * (BuyPower + SellPower)) * 100.0;
				
						//XXPrint("TTF = {0}; {1};{2};{3};{4}", ttf,w1,w2,e1,e2);

				
//				if(e1.Equals(double.NaN) || e2.Equals(double.NaN) || e3.Equals(double.NaN) 
//					|| e4.Equals(double.NaN) || e5.Equals(double.NaN) || e6.Equals(double.NaN)) continue;
				
				e1 = w1 * ttf + w2 * e1;
				e2 = w1 * e1  + w2 * e2;
				e3 = w1 * e2  + w2 * e3;
				e4 = w1 * e3  + w2 * e4;
				e5 = w1 * e4  + w2 * e5;
				e6 = w1 * e5  + w2 * e6;

      			ttf = c1 * e6 + c2 * e5 + c3 * e4 + c4 * e3;
						
						//XXPrint("TTF = {0}; {1};{2};{3};{4};;", ttf,w1,w2,e1,e2);

      			MS[i] = ttf;
				
//--- Levels ...
				
				if(i != 0)
				{	
					if (ttf >= 0.0) 
         			{
						//SS[i] = (double)TopLine;
						SSU[i] = (double)TopLine;
						//XXPrint("-<<-  Up  :{0}| {1}",SS[ToIndex - i], i);
					}
      				else
         			{	
						//SS[i] = (double)BottomLine;
						SSD[i] = (double)BottomLine;
						//XXPrint("->>- Down :{0}| {1}",SS[ToIndex - i], i);
					}
				}
				else
				{
					//SS[i] = SS[i - 1];
					SSU[i] = SSU[i - 1];
					SSD[i] = SSD[i - 1];
				}
				
				i++;
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
	
		protected void XXPrint(string xxformat, params object[] parameters)
		{
			bool DebugMode = true;
			if(DebugMode)
			{
				Print(xxformat,parameters);
			}	
		}
		
    }
}
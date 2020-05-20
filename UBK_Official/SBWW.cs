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
    [Indicator("StrngBr", Location = IndicatorLocation.Chart)]
 
	public class StrngBr : Indicator
    {

		[Parameter("Number of Bar :", DefaultValue = 5, MinValue = 2, MaxValue = 200)]
		public int NUM { get;set; }

		[Parameter("Tail/Body :", DefaultValue = 0.25, MinValue = 0.00, MaxValue = 10.00)]
		public double Coef { get;set; }

		[Parameter("Shift of Point :", DefaultValue = 55, MinValue = -1000, MaxValue = 2000)]
		public int SHIFT { get;set; }
		

		[Series("RUp", Color = Color.DarkBlue,  Style = SeriesStyle.Chars, CharCode = CharCode.ArrowUp)]
    	public IIndicatorSeries RUp { get; set; } 
		
		[Series("RDown", Color = Color.SeaGreen, Style = SeriesStyle.Chars, CharCode = CharCode.ArrowDown)]
		public IIndicatorSeries RDown { get;set; }


		
		
		protected override void Init()
		{
		}
		
		
        protected override void Calculate(int FromIndex, int ToIndex)
        {
            // calculate
			
			for(int i = ToIndex; i >= FromIndex; i--)
			{
				if(i == Bars.Range.To) continue;
				
				bool found = false;
				int sign = 0;
				double body = 0.0;
				double Htail = 0.0;
				double Ltail = 0.0;
				
				if(Bars[i].Close > Bars[i].Open)
				{
						sign = 1;
						body  = Bars[i].Close - Bars[i].Open;
						Htail = Bars[i].High  - Bars[i].Close;
						Ltail = Bars[i].Open  - Bars[i].Low;
				}
				else
					if(Bars[i].Close < Bars[i].Open) 
					{
						sign = -1;
						body  = Bars[i].Open  - Bars[i].Close;
						Htail = Bars[i].High  - Bars[i].Open;
						Ltail = Bars[i].Close - Bars[i].Low;
					}
				else
					{
						sign = 0;
						body = 0.0;
						Htail = Bars[i].High  - Bars[i].Open;
						Ltail = Bars[i].Close - Bars[i].Low;
					}

				if(body < Instrument.Point) 
				{
					continue;
				}
				
				for(int shift = 1; (shift <= NUM) && ((i - shift) >= 0); shift++)
				{	
					if(body <= (Bars[i - shift].High - Bars[i - shift].Low))
					{	
						found = true;
						break;
					}
				}
				if(found) 
				{
					continue;
				}

				RUp[i] = double.NaN;
				RDown[i] = double.NaN;

				if((sign == 1) && ((Htail / body) < Coef))
				{
					if(Bars[i].Close > Bars[i - 1].High)
					{
						RUp[i]   = Bars[i].Low  - SHIFT * Instrument.Point;
					}
				}
				
				if((sign == -1) && ((Ltail / body) < Coef))
				{
					if(Bars[i].Close < Bars[i - 1].Low)
					{	
						RDown[i] = Bars[i].High + SHIFT * Instrument.Point ;
					}
				}
				
			}
		}
		
		

		protected override void  Deinit()
		{
		}
		
	}
}
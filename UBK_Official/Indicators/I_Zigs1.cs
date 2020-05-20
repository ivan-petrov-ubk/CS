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
using System.IO;

namespace IPro.Indicators
{
    [Indicator("Zigs", Location = IndicatorLocation.Chart)]
    public class Zigs : Indicator
    {
        [Series("MZ", Color = Color.Blue, Style = SeriesStyle.ContinuousLine, Width = 1)]
        public IIndicatorSeries MZ { get; set; } 

		private static bool White = false;
		private static bool Black = false;
		
		private static int LastPoint = 0;
		
		
		protected override void Calculate(int FromIndex, int ToIndex)
        {
            // calculate
			
			for(int i = FromIndex; i <= ToIndex; i++) 
			{

				if(Bars[i].Close > Bars[i].Open)
				{
					White = true;
					Black = false;
				}	
				else
				if(Bars[i].Close < Bars[i].Open)	
				{
					White = false;
					Black = true;
				}
				else
				{
					continue;
				}

				if(LastPoint == 1)
				{
					if(White)
					{	
						continue;
					}
					else
					{
						LastPoint = -1;
						MZ[i - 1] =  (Bars[i - 1].Close > Bars[i - 1].Open) ? Bars[i - 1].Close : Bars[i - 1].Open;
					}		
				}	
				else
				{
					if(Black)
					{
						continue;	
					}
					else
					{
						LastPoint = 1;
						MZ[i - 1] =  (Bars[i - 1].Close < Bars[i - 1].Open) ? Bars[i - 1].Close : Bars[i - 1].Open;
					}	
				}
			}
        }
    }
}
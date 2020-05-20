using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using IPro.Model.Programming.Indicators.Standard;
using IPro.Model.Client.MarketData;
using System.Collections.Generic;
using System.IO;

namespace IPro.TradeSystems
{
    [TradeSystem("MAX-MIN-DAY")]
    public class MAXMINDAY : TradeSystem
    {
		private double maxd,mind;
		public DateTime DTime,tmaxd,tmind;
		public double H1,L1;
		private int dy=0;
		
        protected override void Init()
        {

        }        

        protected override void NewBar()
        {   H1 =  Bars[Bars.Range.To-1].High;
			L1 =  Bars[Bars.Range.To-1].Low;
			DTime = Bars[Bars.Range.To-1].Time;
			
			
			if(dy!=DTime.DayOfYear) 
			{   dy=DTime.DayOfYear;
				var i=0;
					maxd=0.0;
					mind=10000.0;
				
				do { i++; 

					
				 if( Bars[Bars.Range.To-i].Time.Hour<24 &&  
					 Bars[Bars.Range.To-i].Time.Hour>0 && 
					 Bars[Bars.Range.To-i].Time.DayOfYear==(dy-1) ) {
					
						 if(maxd<Bars[Bars.Range.To-i].High) { 
							              maxd=Bars[Bars.Range.To-i].High; 
							 			  tmaxd=Bars[Bars.Range.To-i].Time;
						 }
						 if(mind>Bars[Bars.Range.To-i].Low) { 
							 mind=Bars[Bars.Range.To-i].Low; 
                			  tmind=Bars[Bars.Range.To-i].Time;
						 }
							 
						 Print("{0} - {1}  {2} != {3}  -- High={4} Low={5}   ",i,Bars[Bars.Range.To-i].Time,Bars[Bars.Range.To-i].Time.DayOfYear,dy,Bars[Bars.Range.To-i].High,Bars[Bars.Range.To-i].Low);
						 
					 }
				} while(Bars[Bars.Range.To-i].Time.DayOfYear!=dy-2 && i<1000);
				Print("max={0} ({1}) min={2} ({3}) ======================================================================",maxd,tmaxd,mind,tmind);
			}
			
			
			
        }
    }
}
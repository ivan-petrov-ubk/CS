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
    [TradeSystem("ТС Пробой M5")] //copy of "ТС Пробой Н1"
    public class ТС_Пробой_Н1 : TradeSystem
    {   public double H1,L1,O1,C1,H2,L2,O2,C2,O3,C3;
		public HorizontalLine Line1,Line2,Line3;
		public DateTime DTime; // Время
				private ZigZag _wprInd3;
		
        protected override void Init()
        {
			 Line1 = Tools.Create<HorizontalLine>();
			 Line2 = Tools.Create<HorizontalLine>();
			_wprInd3= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd3.ExtDepth=3;
        }        

        protected override void NewBar()
        {	_wprInd3.ReInit();
			O1 = Bars[Bars.Range.To-1].Open;
			C1 = Bars[Bars.Range.To-1].Close;
			DTime = Bars[Bars.Range.To-1].Time;
          if ( DTime.Hour==00 && DTime.Minute==00 ) 
          { 	
         		var highestIndex = Series.Highest(Bars.Range.To, 288, PriceMode.High);
     			var highestPrice = Bars[highestIndex].High;
			     	H1 = highestPrice;
		    	var lowestIndex  = Series.Lowest(Bars.Range.To, 288, PriceMode.Low);
			    var lowestPrice = Bars[lowestIndex].Low;
			     	L1 = lowestPrice;
			  		
			  		Line1.Price = H1;
			  		Line1.Text = string.Format("{0}",Math.Round((H1-L1)*100000,0));
					Line2.Price = L1;
		  }	    
        }
    }
}
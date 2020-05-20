using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using IPro.Model.Client.MarketData;
using IPro.Model.Programming.Indicators.Standard;
using System.Collections.Generic;

namespace IPro.TradeSystems
{
    [TradeSystem("London_New")] 
    public class London1 : TradeSystem
    {
        // Simple parameter example
        private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public DateTime DTime; // Время
		
		public double x;
		public int xp;
		
        protected override void Init()
        {        }        

        protected override void NewQuote()
        {}
        
        protected override void NewBar()
        {
	 
			DTime = Bars[Bars.Range.To-1].Time;
			// Event occurs on every new bar
          if ( DTime.Hour==18 && DTime.Minute==0 ) 
          { 
				var highestIndex = Series.Highest(Bars.Range.To, 9, PriceMode.High);
     			var highestPrice = Bars[highestIndex].High;
		    	var lowestIndex = Series.Lowest(Bars.Range.To, 9, PriceMode.Low);
     			var lowestPrice = Bars[lowestIndex].Low;
			  			 
			   x=Math.Round(((highestPrice-lowestPrice)/2),5);
		       xp=(int)(x*100000);
			  
			  if(xp<400) {
			  		   var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, Instrument.Bid-x, 0, Stops.InPips(xp+50,2*xp), null, null, null);
						 if (result.IsSuccessful)  posGuidBuy=result.Position.Id; 
			   var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  Instrument.Ask+x, 0, Stops.InPips(xp+50,2*xp), null, null, null);
						 if (result1.IsSuccessful)  posGuidSell=result1.Position.Id; 
			  }
		  }
					 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) 
			     {	posGuidBuy=Guid.Empty; }
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) 
				{ posGuidSell=Guid.Empty;  }

		  if ( DTime.Hour==9 && DTime.Minute==00 ) 
		  {    
			    if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			       					{   var res = Trade.CancelPendingPosition(posGuidBuy);
			     	   					if (res.IsSuccessful) { posGuidBuy = Guid.Empty; }
				   					}
				if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
				   				{  var result2=Trade.CloseMarketPosition(posGuidBuy); 
				   				   if (result2.IsSuccessful) {  posGuidBuy = result2.Position.Id; }  
			       				}		
								
				if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			       					{   var res1 = Trade.CancelPendingPosition(posGuidSell);
			     	   					if (res1.IsSuccessful) { posGuidSell = Guid.Empty; }
				   					}
				if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
				   				{  var result3=Trade.CloseMarketPosition(posGuidSell); 
				   				   if (result3.IsSuccessful) {  posGuidBuy = result3.Position.Id; }  
			       				}
		  }

				if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
				{
					var res = Trade.CancelPendingPosition(posGuidBuy);
			     	if (res.IsSuccessful) posGuidBuy = Guid.Empty; 
				}

				if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
				{
					var res = Trade.CancelPendingPosition(posGuidSell);
			     	if (res.IsSuccessful)  posGuidSell = Guid.Empty; 
				}
				
        }
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
    }
}
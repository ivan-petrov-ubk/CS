using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;

namespace IPro.TradeSystems
{
    [TradeSystem("Trade2")]
    public class Trade2 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		private bool firstBar = true;
		private Guid posGuid;
		int i=1;


        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        { i++;
            // Event occurs on every new bar
	//    if (firstBar)
    //{
    //    var result = Trade.BuyStop(Instrument.Id, 0.1, Bars[Bars.Range.To-1].High+0.002);
      //  if (result.IsSuccessful) posGuid = result.Position.Id;
       // firstBar = false;
    //}
	
			//Print("{0} - Ask={1} Bid={2} - {3}",Bars[Bars.Range.To-1],Instrument.Ask,Instrument.Bid,Bars[Bars.Range.To-1].Time);
			
	if(firstBar) {firstBar = false;
		  Print("Ask={0}  Point={1} PriceScale={2}",Instrument.Ask,Instrument.Point,Instrument.PriceScale);
		
		 var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellStop, 0.1, Instrument.Ask-0.001, 0, Stops.InPrice(Instrument.Bid+0.002), null, null, null);
		if (result.IsSuccessful) posGuid = result.Position.Id;  
	}
	if ( Trade.GetPosition(posGuid).State==PositionState.Closed ) 
	{
		 var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellStop, 0.1, Instrument.Ask-0.001, 0, Stops.InPrice(Instrument.Bid+0.002), null, null, null);
		if (result.IsSuccessful) posGuid = result.Position.Id;  
		
	}

	Print("State={0} -- {1}",Trade.GetPosition(posGuid).State,Bars[Bars.Range.To-1].Time);
	//if (i==20)
    //{
		//var res = Trade.UpdatePendingPosition(posGuid, 0.1, Bars[Bars.Range.To-1].High+0.002, null, null);
		//var res = Trade.UpdateMarketPosition(posGuid, Bars[Bars.Range.To-1].Low-0.003, null, "Added stoploss and takeprofit");
	//}
	//if (i>30)
    //{
      //  if (posGuid==Guid.Empty) return;
      //  var res = Trade.CancelPendingPosition(posGuid);
     //   if (res.IsSuccessful) posGuid = Guid.Empty; 
    //}
	
	//if (i>20) {
      // if (posGuid!=Guid.Empty) {
        //var res = Trade.CloseMarketPosition(posGuid);
        //if (res.IsSuccessful) posGuid = Guid.Empty;
	   //} }
	
// var pos = Trade.GetPosition(posGuid);
  //        var state = pos.State;
	//Print("State={0}",state);
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
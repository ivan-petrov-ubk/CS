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
    [TradeSystem("TC_M1")]
    public class TC_M1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "ТС - M1")]
        public string CommentText { get; set; }
		[Parameter("TakeProfit :", DefaultValue = 2000)]
        public int TP { get; set; }
        [Parameter("StopLoss :", DefaultValue = 200)]
        public int SL { get; set; }
        [Parameter("Шаг SL :", DefaultValue = 0)]
        public int sSL { get; set; }
		[Parameter("Trailing SL :", DefaultValue = 100)]
        public int TSL { get; set; }
	    [Parameter("Trailing SL :", DefaultValue = 200)]
        public int DS { get; set; }
		
		
		private Guid posGuid=Guid.Empty;
		public double C1,O10,x;
		public int ind=0;
	

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
        {
            // Event occurs on every new bar
			O10=Bars[Bars.Range.To-10].Open;
			C1=Bars[Bars.Range.To-1].Close;
			if(Math.Abs(Math.Round((O10-C1)*100000,0))>200) 
				Print("{0} - {1}",Math.Abs(Math.Round((O10-C1)*100000,0)),Bars[Bars.Range.To-10].Time);
			
//=== КОРЕКЦИЯ =====================================================================================================================							 
			if (posGuid!=Guid.Empty && Trade.GetPosition(posGuid).State==PositionState.Closed) posGuid=Guid.Empty; 
//==================================================================================================================================	
			
			
			if(Instrument.Name.EndsWith("JPY")) x=0.2; else x=0.002;		
			
			if (C1-O10>x) { 
				if (posGuid==Guid.Empty) {
				var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 1.0, Instrument.Bid, -1, Stops.InPips(200,1000), null, null);	
			    if (result1.IsSuccessful)  posGuid = result1.Position.Id; }
			}
			
			if (O10-C1>x) { 
				if (posGuid==Guid.Empty) {
		          var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell,  1.0, Instrument.Ask, -1, Stops.InPips(200,1000), null, null);	
				  if (result.IsSuccessful)  posGuid = result.Position.Id;   } 
			}

			
		
			if(DS>0 && ind==200 && posGuid!=Guid.Empty) {      
				var res = Trade.CloseMarketPosition(posGuid);
        		if (res.IsSuccessful) posGuid = Guid.Empty;}
			
			if (posGuid!=Guid.Empty) ind++; else ind=0;
			
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
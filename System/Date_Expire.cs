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
    [TradeSystem("Date_Expire")]
    public class Date_Expire : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		public DateTime Data1;
		public bool first; 

        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			first=true;
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			 Data1=Bars[Bars.Range.To-1].Time;
			Print("TimeOfDay={0} Data1.Date={1} Data1.Kind={2} Data1.AddHours={3}",Data1.TimeOfDay,Data1.Date,Data1.Kind,Data1.AddHours(16));
			if (first) 
			{ first=false;
				var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  Bars[Bars.Range.To-1].High+0.002, 0, null, Data1.AddHours(3), null, null);
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
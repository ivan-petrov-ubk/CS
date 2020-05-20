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
    [TradeSystem("Graph-Fibo")] //copy of "Color"
    public class Color2 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		public bool FirstBar;
		public LineDashStyle Styl;
		
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			FirstBar=true;
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			if (FirstBar) 
			{
				
         var toolFibonacci = Tools.Create<Fibo>();
         toolFibonacci.Point1= new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].Low);
         toolFibonacci.Point2= new ChartPoint(Bars[Bars.Range.To-10].Time, Bars[Bars.Range.To-10].High);
				
				
				FirstBar=false;
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
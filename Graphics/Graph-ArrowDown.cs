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
    [TradeSystem("Graph-ArrowDown")]  //copy of "Color2"
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
				
				 var toolArrowDown = Tools.Create<ArrowDown>();
     				toolArrowDown.Color = Color.Red;
					toolArrowDown.Label=Bars[Bars.Range.To-1].Low.ToString();
				
     				toolArrowDown.Label="Label1";
					toolArrowDown.Width=1;
				         toolArrowDown.Point=new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].Low-0.004);
				
				Print("toolArrowDown.Id1={0}",toolArrowDown.Id);       // c980df27-6586-4b8e-bfc4-e7a5b3f59ec9
				Print("toolArrowDown.Id2={0}",toolArrowDown.Color); // False
				Print("toolArrowDown.Id3={0}",toolArrowDown.Point);    //  
				Print("toolArrowDown.Id5={0}",toolArrowDown.Type);    // 
				     //
				
				
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
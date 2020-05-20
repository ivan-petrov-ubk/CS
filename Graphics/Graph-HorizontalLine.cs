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
    [TradeSystem("Graph-HorizontalLine")]  //copy of "HorizontalLine"
    public class Color : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		public bool FirstBar;

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
				
				  var toolHorizLine=Tools.Create<HorizontalLine>();
     				toolHorizLine.Price = Bars[Bars.Range.To-1].Low;
					toolHorizLine.Label=Bars[Bars.Range.To-1].Low.ToString();
     				toolHorizLine.Text="Last bar Low price = "+Bars[Bars.Range.To-1].Low;
				
				Print("toolHorizLine.Id1={0}",toolHorizLine.Id);       // c980df27-6586-4b8e-bfc4-e7a5b3f59ec9
				Print("toolHorizLine.Id2={0}",toolHorizLine.IsLocked); // False
				Print("toolHorizLine.Id3={0}",toolHorizLine.Label);    //  
				Print("toolHorizLine.Id4={0}",toolHorizLine.Order);    // 0
				Print("toolHorizLine.Id5={0}",toolHorizLine.Price);    // 
				Print("toolHorizLine.Id6={0}",toolHorizLine.SectionId);// 
				Print("toolHorizLine.Id7={0}",toolHorizLine.Type);     //
				
				
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
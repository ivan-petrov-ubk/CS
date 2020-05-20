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
    [TradeSystem("Graph-VerticalLine")] //copy of "Color"
    public class Color : TradeSystem
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
				
				  var toolVerticalLine=Tools.Create<VerticalLine>();            // Создание обьекта  ВЕРТИКАЛЬНАЯ ЛИНИЯ
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;         // Задать время - где будет прорисована линия       
					toolVerticalLine.Label=Bars[Bars.Range.To-1].Low.ToString(); // 
				
     				toolVerticalLine.Label="Label1";
					toolVerticalLine.Width=5;
					toolVerticalLine.Style=LineDashStyle.LongDash;
				    toolVerticalLine.Color=Color.LightSeaGreen;
				    
				
				Print("toolVerticalLine.Id1={0}",toolVerticalLine.Id);       // c980df27-6586-4b8e-bfc4-e7a5b3f59ec9
				Print("toolVerticalLine.Id2={0}",toolVerticalLine.Color); // False
				Print("toolVerticalLine.Id3={0}",toolVerticalLine.Style);    //  
				Print("toolVerticalLine.Id4={0}",toolVerticalLine.Time);    // 0
				Print("toolVerticalLine.Id5={0}",toolVerticalLine.Type);    // 
				Print("toolVerticalLine.Id6={0}",toolVerticalLine.Width);//
				Print("toolVerticalLine.Id6={0}",toolVerticalLine.Label);
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
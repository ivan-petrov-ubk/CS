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
    [TradeSystem("Stat1")]
    public class Stat1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		double H1,L1,O1,C1; 
		double H2,L2,O2,C2;
		public DateTime DTime1,DTime2;
		public int ind1,ind2,ind3,ind4;

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
						// Значения текущего Бара
			H1 = Bars[Bars.Range.To-1].High;
			L1 = Bars[Bars.Range.To-1].Low;
			O1 = Bars[Bars.Range.To-1].Open;
			C1 = Bars[Bars.Range.To-1].Close;
			DTime1 = Bars[Bars.Range.To-1].Time;

						// Значения пред Бара
			H2 = Bars[Bars.Range.To-2].High;
			L2 = Bars[Bars.Range.To-2].Low;
			O2 = Bars[Bars.Range.To-2].Open;
			C2 = Bars[Bars.Range.To-2].Close;
			DTime2 = Bars[Bars.Range.To-2].Time;
			
			/* // количество свечей 
			if(O1>C1 && O2>C2) ind1++;  // 26-28-21-27 За полгода.
			if(O1>C1 && O2<C2) ind2++;
			if(O1<C1 && O2<C2) ind3++;
			if(O1<C1 && O2>C2) ind4++; */
			
			if(H1>H2 && L1>L2 && O2<C2 && O1<C1 ) ind1++;
			if(H1<H2 && L1<L2 && O2>C2 && O1>C1 ) ind2++;
			
			//if(H1<H2 && L1<L2 && O2>C2 && O1>C1 ) ind2++;
			ind3=0; ind4=0;
		Print("{0} {1} {2} {3} -- {4} ",ind1,ind2,ind3,ind4,DTime1);
 
            // Event occurs on every new bar
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
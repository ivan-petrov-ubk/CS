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
    [TradeSystem("uroven1")] //copy of "Ritcher1"
    public class Ritcher1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
        // Simple parameter example
        private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public DateTime DTime; // Время
		public HorizontalLine Line1,Line2,Line3;
		public double H1,L1,O1,C1,H2,L2,O2,C2,O3,C3,H3,L3;
		public ArrowDown ad;
		public ArrowUp au;
		public int tr=0;
		private VerticalLine vlR,vlB;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			 Line1 = Tools.Create<HorizontalLine>();
			 Line2 = Tools.Create<HorizontalLine>();
			 Line3 = Tools.Create<HorizontalLine>();
			 ad = Tools.Create<ArrowDown>();
			 au = Tools.Create<ArrowUp>();
        }        

        
        protected override void NewBar()
        {   Print("Instrument.Spread= {0}",Instrument.Spread);
			if (true) return;
			Print("Return - FALSE!");
			O1 = Bars[Bars.Range.To-1].Open;
			C1 = Bars[Bars.Range.To-1].Close;
			H1 = Bars[Bars.Range.To-1].High;
			L1 = Bars[Bars.Range.To-1].Low;
			// Значения пред Бара
			O2 = Bars[Bars.Range.To-2].Open;
			C2 = Bars[Bars.Range.To-2].Close;
			H2 = Bars[Bars.Range.To-2].High;
			L2 = Bars[Bars.Range.To-2].Low;

			O3 = Bars[Bars.Range.To-3].Open;
			C3 = Bars[Bars.Range.To-3].Close;			
			H3 = Bars[Bars.Range.To-3].High;
			L3 = Bars[Bars.Range.To-3].Low;
			
			DTime = Bars[Bars.Range.To-1].Time;

			if(Math.Abs(H3-H2)<0.00005 && Math.Abs(H3-H1)<0.00005)
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Red;
			}
			
			if(Math.Abs(L3-L2)<0.00005 && Math.Abs(L3-L1)<0.00005)
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Blue;
			}
        }
 
    }
}
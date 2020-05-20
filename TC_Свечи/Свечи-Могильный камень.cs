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
    [TradeSystem("Свечи-Могильный камень")]   //copy of "Свечи-Пинцет"
    public class E1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		[Parameter("Delta=", DefaultValue = 2)]
        public int SP { get; set; }
		public DateTime T1,T2;
		public double H1,H2,O1,O2,C1,C2,L1,L2,V1,V2,maxL;
		public double kor;

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
 			O1=Bars[Bars.Range.To-1].Open;      O2=Bars[Bars.Range.To-2].Open;
			C1=Bars[Bars.Range.To-1].Close;		C2=Bars[Bars.Range.To-2].Close;
			H1=Bars[Bars.Range.To-1].High;		H2=Bars[Bars.Range.To-2].High;
			L1=Bars[Bars.Range.To-1].Low;		L2=Bars[Bars.Range.To-2].Low;
			V1=Bars[Bars.Range.To-1].Volume;	V2=Bars[Bars.Range.To-2].Volume;
			T1=Bars[Bars.Range.To-1].Time;		T2=Bars[Bars.Range.To-2].Time;

				
//			if ( O1==C1 && O1==H1 ) 
			if(Instrument.Name.EndsWith("JPY")) kor=0.001*SP; else kor=0.00001*SP;	
			if ( Math.Abs(O1-C1)<kor && (O1==L1 || C1==L1) )
		  {
				var vl=Tools.Create<VerticalLine>();
               		vl.Time=Bars[Bars.Range.To-1].Time;
			   		vl.Color=Color.Red;			  
		  }
		  				
//			if ( O1==C1 && O1==L1 ) 
			if ( Math.Abs(O1-C1)<kor && (O1==H1 || C1==H1) )
		{
				var vl=Tools.Create<VerticalLine>();
               		vl.Time=Bars[Bars.Range.To-1].Time;
			   		vl.Color=Color.Blue;			  
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
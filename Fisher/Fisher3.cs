using System;
using System.Collections.Generic; 
using System.Text;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using IPro.Model.Programming.Indicators.Standard;


namespace IPro.TradeSystems
{
   
	class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello, World!");
        }
    }
    [TradeSystem("TFisher")]
    public class TFisher : TradeSystem
    {
		
        // Simple parameter example
		
        // [Parameter("Текст", DefaultValue = 2*10)]
		[Parameter("Lot", MinValue = 0.01, IsLot = true, Postfix = "Postfix example")]
        public double Lot { get; set; }

		[Parameter("PeriodK", DefaultValue = 5)]
        public int PeriodK { get; set; }

		[Parameter("PeriodD", DefaultValue = 3)]
        public int PeriodD { get; set; }

		[Parameter("Slowdown", DefaultValue = 3)]
        
		// System.Console.Write('H'); 
		public string CommentText { get; set; }
        
		private FisherTransformOscillator _ftoInd;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
			//_ftoInd = GetIndicator(Instrument.Id, Timeframe);
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
        }        

		 
		
        protected override void NewQuote()
        {
            // Event occurs on every new quote
			
		}
        private int countBar=0;
        protected override void NewBar()
        {
            // Event occurs on every new bar
			//Print("NewBar-{0}",Bars.Range.To-1);
//		Print("NB-{0} - {1} - {2}",Bars[Bars.Range.To-1].High,Bars[Bars.Range.To-1].Low,Bars[Bars.Range.To-1].Time);
			 countBar++;
    if (countBar==1)
    {
   //      var toolHandUp = Tools.Create();
   //      toolHandUp.Point=new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High);
    }
			 //Print("Fisher transform oscillator down series value: {0}", _ftoInd.DownSeries[Bars.Range.To-1]);
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

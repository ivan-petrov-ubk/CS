
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
    [TradeSystem("Fisher_Stohastic")]
    public class Fisher_Stohastic : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		public FisherTransformOscillator _ftoInd;
		 private StochasticOscillator _stoInd;
		double sL,mL,sF;

        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			_stoInd= GetIndicator<StochasticOscillator>(Instrument.Id, Timeframe);
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			//Print("{0} | {1} | {2} | {3} | {4} | {5} | {6}", _stoInd.SignalLine[Bars.Range.To-1],_stoInd.MainLine[Bars.Range.To-1],Bars[Bars.Range.To-1].Time);
            sL= _stoInd.SignalLine[Bars.Range.To-1];
			mL= _stoInd.MainLine[Bars.Range.To-1];
			sF = _ftoInd.FisherSeries[Bars.Range.To-1];
			
			
			if(sL<30 && sF>-0.3) Red();
			if(sL>70 && sF<0.3) Blue();
		}
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
		private void Red()
		{
		    var vline = Tools.Create<VerticalLine>();
            vline.Color=Color.Red;
		    vline.Time=Bars[Bars.Range.To-1].Time;	
		}
		
		private void Blue()
		{
		    var vline = Tools.Create<VerticalLine>();
            vline.Color=Color.Blue;
		    vline.Time=Bars[Bars.Range.To-1].Time;	
		}
		
    }
}
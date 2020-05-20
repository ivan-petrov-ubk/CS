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
    [TradeSystem("Fisher+Stohastic v1")]
    public class FisherStohastic_v1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		 public FisherTransformOscillator _ftoInd;
		 private StochasticOscillator _stoInd;
				private VerticalLine vlR,vlB,vlG,vlY;
		double sL,mL,sF,ma1F,ma2F,ma1F2,sF2;


        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
        	_stoInd = GetIndicator<StochasticOscillator>(Instrument.Id, Timeframe);
			_ftoInd = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
			vlR = Tools.Create<VerticalLine>();
			vlR.Color=Color.Red;
			vlB = Tools.Create<VerticalLine>();
			vlB.Color=Color.Blue;
		}        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			  sL = _stoInd.SignalLine[Bars.Range.To-1];
			  mL = _stoInd.MainLine[Bars.Range.To-1];
			  sF = _ftoInd.FisherSeries[Bars.Range.To-1];
			 sF2 = _ftoInd.FisherSeries[Bars.Range.To-2];			
			ma1F = _ftoInd.Ma1Series[Bars.Range.To-1];
			ma2F = _ftoInd.Ma2Series[Bars.Range.To-1];
		   ma1F2 = _ftoInd.Ma1Series[Bars.Range.To-2];
			

			//Print("{0} | {1} | {2} | {3} | {4} | {5} |", sL,mL,sF,ma1F,ma2F,Bars[Bars.Range.To-1].Time);
			if(sL>80 && sF<-0.5) vlR.Time=Bars[Bars.Range.To-1].Time;
			if(sL<20 && sF>0.5)  vlB.Time=Bars[Bars.Range.To-1].Time;
			
			if(sF>0 && sF<ma1F && sF2>ma1F2)  vlR.Time=Bars[Bars.Range.To-1].Time;
			if(sF<0 && sF>ma1F && sF2<ma1F2)  vlB.Time=Bars[Bars.Range.To-1].Time;
			
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
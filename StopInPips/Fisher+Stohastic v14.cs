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
		private Guid posGuidBuy =Guid.Empty;
		private Guid posGuidSell=Guid.Empty;


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
			if(sL>78 && sF<-0.1) { 
				vlR.Time=Bars[Bars.Range.To-1].Time;
							if(posGuidSell==Guid.Empty) {
				var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask,-1,Stops.InPips(200,null),null);
				if (result.IsSuccessful) posGuidSell=result.Position.Id; }
				  
			}
			
			if(sL<22 && sF>0.1) { vlB.Time=Bars[Bars.Range.To-1].Time;
				
				if(posGuidBuy==Guid.Empty) {  vlR.Time=Bars[Bars.Range.To-1].Time;
				var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid,-1,Stops.InPips(200,null),null);
				if (result.IsSuccessful) posGuidBuy=result.Position.Id; }	
				
							
			}
			
			if(sF2>ma1F2 && sF<ma1F)  { 		
				if(posGuidBuy!=Guid.Empty) {
			    var result =Trade.CloseMarketPosition(posGuidBuy);
			    if (result.IsSuccessful)   posGuidBuy = Guid.Empty;}
				}
			
			if(sF<0)  { 		
				if(posGuidBuy!=Guid.Empty) {
			    var result =Trade.CloseMarketPosition(posGuidBuy);
			    if (result.IsSuccessful)   posGuidBuy = Guid.Empty;}
				}
			
			if(sF2<ma1F2 && sF>ma1F)  
			{if(posGuidSell!=Guid.Empty) {
			    var result =Trade.CloseMarketPosition(posGuidSell);
			    if (result.IsSuccessful)   posGuidSell = Guid.Empty;}
			}
			if(sF>0)  
			{if(posGuidSell!=Guid.Empty) {
			    var result =Trade.CloseMarketPosition(posGuidSell);
			    if (result.IsSuccessful)   posGuidSell = Guid.Empty;}
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
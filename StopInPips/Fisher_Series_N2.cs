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
    [TradeSystem("Fisher_Series")]
    public class Fisher_Series : TradeSystem
    {
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		public FisherTransformOscillator _ftoInd;
		public double sF=0,sF1=0;
		private int _lastIndexM15 = -1;
		public Period periodM15;
		public ISeries<Bar> _barM15,_barH1;
		public double CloseM15,OpenM15;
		public DateTime timeM15;
		public FisherTransformOscillator _ftoInd1;
		
		private ZigZag _wprInd;
		
        protected override void Init()
        {
			// M15
			periodM15 = new Period(PeriodType.Minute, 15);
			_barM15 = GetCustomSeries(Instrument.Id,Period.H1);
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Period.H1); 
			_ftoInd1 = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=3;
			
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
			if (_lastIndexM15 < _barM15.Range.To-1) {
     		_lastIndexM15 = _barM15.Range.To - 1;
    		// CloseM15 = _barM15[_lastIndexM15].Close;
			// OpenM15 = _barM15[_lastIndexM15].Open;		
	 		timeM15 = _barM15[_lastIndexM15].Time;    
			 sF = _ftoInd.FisherSeries[_lastIndexM15];
			sF1 = _ftoInd.FisherSeries[_lastIndexM15-1];	
			} 

        }
        
        protected override void NewBar()
        {   _wprInd.ReInit();
			//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
            // Event occurs on every new bar
			// Print("{0} sFTime={1} sF={2}",Bars[Bars.Range.To-1].Time,timeM15,sF);
			if(sF1<0 && sF>0) {  // ВВЕРХ
				var vr1=Tools.Create<VerticalLine>(); vr1.Color=Color.Red;  vr1.Time=Bars[Bars.Range.To-1].Time; vr1.Width=1;
				Print("ВЕРХ - {0} -- {1} {2}",Bars[Bars.Range.To-1].Time,Math.Round(_ftoInd1.FisherSeries[Bars.Range.To-2],3),Math.Round(_ftoInd1.FisherSeries[Bars.Range.To-1],3));
				if ( _ftoInd1.FisherSeries[Bars.Range.To-2]<0 && _ftoInd1.FisherSeries[Bars.Range.To-1]>0) 
			  {	Print("----------------- OK ------------------");var vr3=Tools.Create<VerticalLine>(); vr3.Color=Color.Olive;  vr3.Time=Bars[Bars.Range.To-1].Time; vr3.Width=3; 
			  		if(posGuidBuy==Guid.Empty){  	
								 var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1, Stops.InPips(200,100), null, null);
								 if (result3.IsSuccessful)  posGuidBuy=result3.Position.Id;
							} 
						}
			  }
			
			if(sF1>0 && sF<0) {  //ВНИЗ
				var vr2=Tools.Create<VerticalLine>(); vr2.Color=Color.Blue;  vr2.Time=Bars[Bars.Range.To-1].Time; vr2.Width=1;
				Print("ВНИЗ - {0} -- {1} {2}",Bars[Bars.Range.To-1].Time,Math.Round(_ftoInd1.FisherSeries[Bars.Range.To-2],3),Math.Round(_ftoInd1.FisherSeries[Bars.Range.To-1],3));
					if ( _ftoInd1.FisherSeries[Bars.Range.To-2]>0 && _ftoInd1.FisherSeries[Bars.Range.To-1]<0) 
					{Print("----------------- OK ------------------");var vr=Tools.Create<VerticalLine>(); vr.Color=Color.IndianRed; vr.Time=Bars[Bars.Range.To-1].Time; vr.Width=3;
						if(posGuidSell==Guid.Empty)
						      {
								   var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid, -1,Stops.InPips(200,100), null, null);
								  if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;  
							  }
						 
					
					}
			}
			 
			  
			
        }
    }
}
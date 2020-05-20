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
		public FisherTransformOscillator _ftoInd;
		public double sF=0,sF1=0;
		private int _lastIndexM15 = -1;
		public Period periodM15;
		public ISeries<Bar> _barM15;
		public double CloseM15,OpenM15;
		public DateTime timeM15;
		
        protected override void Init()
        {
			// M15
			periodM15 = new Period(PeriodType.Minute, 15);
			_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, periodM15); 
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
        {
            // Event occurs on every new bar
			// Print("{0} sFTime={1} sF={2}",Bars[Bars.Range.To-1].Time,timeM15,sF);
			if(sF>0 && sF1<0) {var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red;  vr.Time=Bars[Bars.Range.To-1].Time; vr.Width=2;}
			if(sF<0 && sF1>0) {var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-1].Time; vr.Width=2;}
			
        }
    }
}
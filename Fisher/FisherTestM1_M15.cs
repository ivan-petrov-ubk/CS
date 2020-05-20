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
    [TradeSystem("FisherTestM1_M15")]          //copy of "Lo7F"
    public class ZZ_Ex1 : TradeSystem
    {
		public DateTime tmM15;
		public FisherTransformOscillator _ftoInd,_ftoIndM15;
		public ISeries<Bar> _barM15;
		private int _lastIndexM15 = -1;
		public Period periodM15;	
		public double Fs;
	
		
       protected override void Init()
        {  
			periodM15 = new Period(PeriodType.Minute, 15);
			_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			_ftoIndM15   = GetIndicator<FisherTransformOscillator>(Instrument.Id, periodM15); 
			// _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
        }      

        protected override void NewQuote()
        {
			if (_lastIndexM15 < _barM15.Range.To-1) {     		    	
					Fs =_ftoIndM15.FisherSeries[_barM15.Range.To-1];
					tmM15 = _barM15[_barM15.Range.To-1].Time;
					_lastIndexM15 = _barM15.Range.To-1;  
				} 
		}
//===============================================================================================================================
        protected override void NewBar()
        {
			Print("{0} - Fs0={1}",Bars[Bars.Range.To-1].Time,Fs);
			// Print("{0} - Fs0={1}",Bars[Bars.Range.To-1].Time,_ftoInd.FisherSeries[Bars.Range.To-1]);

		}
	}
}

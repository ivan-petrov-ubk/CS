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
    [TradeSystem("Fisher3v")]           //copy of "FisherTestM1_M15"
    public class ZZ_Ex1 : TradeSystem
    {
		public DateTime tmM15;
		public FisherTransformOscillator _ftoInd,_ftoIndM15;
		public ISeries<Bar> _barM15;
		private int _lastIndexM15 = -1;
		public Period periodM15;	
		public double Fs,Fs1,Fm,Fm1;
		private int ku,kd;
		private bool  FsU, FsD;
		
       protected override void Init()
        {  
			periodM15 = new Period(PeriodType.Minute, 15);
			_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			_ftoIndM15   = GetIndicator<FisherTransformOscillator>(Instrument.Id, periodM15); 
			 _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
        }      

        protected override void NewQuote()
        {
			if (_lastIndexM15 < _barM15.Range.To-1) {     	
				Fs1=Fs; Fm1=Fm;
					Fs =_ftoIndM15.FisherSeries[_barM15.Range.To-1];
				    Fm =_ftoIndM15.Ma1Series[_barM15.Range.To-1];
				
				// isFMU isFMD - синяя линия пересекла линию фишера
					if ( Fm1<Fs1 && Fm>Fs && Fs>0)  ku++;
					if ( Fm1>Fs1 && Fm<Fs && Fs<0)  kd++;
					if ( Fs1>0 && Fs<0 ) kd=0;
					if ( Fs1<0 && Fs>0 ) ku=0; 			
					tmM15 = _barM15[_barM15.Range.To-1].Time;
					_lastIndexM15 = _barM15.Range.To-1;  
				} 
		}
//===============================================================================================================================
        protected override void NewBar()
        {
			// Print("{0} - Fs0={1}",Bars[Bars.Range.To-1].Time,Fs);
			// Print("{0} - Fs0={1}",Bars[Bars.Range.To-1].Time,_ftoInd.FisherSeries[Bars.Range.To-1]);
//====== Определяем пересечение нуля фишером========================================================================================
	if(_ftoInd.FisherSeries[Bars.Range.To-2]<0 && _ftoInd.FisherSeries[Bars.Range.To-1]>0) FsU=true; else FsU=false;
	if(_ftoInd.FisherSeries[Bars.Range.To-2]>0 && _ftoInd.FisherSeries[Bars.Range.To-1]<0) FsD=true; else FsD=false;

	if(kd>=3 && ku<=2 && Fs1>0 && Fs<0) { var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Red; vl1.Width=3; }
	if(ku>=3 && kd<=2 && Fs1<0 && Fs>0) { var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Blue;vl1.Width=3; }
	
	 Print("{0} - ku={1} kd={2} Fs={3} Fs1={4} Fm={5} Fm1={6}",Bars[Bars.Range.To-1].Time,ku,kd,Math.Round(Fs,3),Math.Round(Fs1,3),Math.Round(Fm,3),Math.Round(Fm1,3));
		}
	}
}

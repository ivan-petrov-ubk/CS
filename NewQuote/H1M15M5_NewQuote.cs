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
    [TradeSystem("H1M15M5")]
    public class H1M15M5 : TradeSystem
    {
		[Parameter("Fisher H4=", DefaultValue = true)]
        public bool F4U { get; set; }
		[Parameter("Fisher H1=", DefaultValue = true)]
        public bool F1U { get; set; }
		[Parameter("Fisher M15=", DefaultValue = true)]
        public bool F15U { get; set; }		
		
		public DateTime tmM15,tmH1,tmH4;
		public ISeries<Bar> _barH4,_barH1,_barM15;
		public Period periodM15,periodH4;		
		private int _lastIndexM15 = -1,_lastIndexH1 = -1,_lastIndexH4 = -1;	
		public FisherTransformOscillator _ftoIndH4,_ftoIndH1,_ftoIndM15,_ftoInd;
		public double Fs,Fs1,Fh,Fh1,F4,F41;
		//public bool F4U,F1U,F15U;
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		private string trueLogPath = "";
		public string LogFileName = @"Fisher2";
		
        protected override void Init()
        {	trueLogPath=PathToLogFile+"\\"+LogFileName+".LOG";
			periodM15 = new Period(PeriodType.Minute, 15);
			periodH4 = new Period(PeriodType.Hour, 4);
			_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			_barH1 = GetCustomSeries(Instrument.Id,Period.H1);
			_barH4 = GetCustomSeries(Instrument.Id,periodH4);
			_ftoIndH4   = GetIndicator<FisherTransformOscillator>(Instrument.Id, periodH4);
			_ftoIndH1   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Period.H1);
			_ftoIndM15   = GetIndicator<FisherTransformOscillator>(Instrument.Id, periodM15);
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);

        }        

        protected override void NewQuote()
        {
				if (_lastIndexM15 < _barM15.Range.To-1) {     		    	
					Fs=_ftoIndM15.FisherSeries[_barM15.Range.To];
					Fs1=_ftoIndM15.FisherSeries[_barM15.Range.To-1];
					if(_lastIndexM15>0 && Fs>Fs1) F15U=true; else F15U=false;
					tmM15=_barM15[_barM15.Range.To-1].Time;
					_lastIndexM15 = _barM15.Range.To-1;  
				}
				if (_lastIndexH1 < _barH1.Range.To-1) {
					if(_lastIndexH1>0 && _ftoIndH1.FisherSeries[_barH1.Range.To]>_ftoIndH1.FisherSeries[_barH1.Range.To-1]) F1U=true; else F1U=false;
     		    	_lastIndexH1 = _barH1.Range.To-1;  
					tmH1=_barH1[_barH1.Range.To-1].Time;
				}
				if (_lastIndexH4 < _barH4.Range.To-1) {
				if(_lastIndexH4>0 && _ftoIndH4.FisherSeries[_barH4.Range.To]>_ftoIndH4.FisherSeries[_barH4.Range.To-1]) F4U=true; else F4U=false;	
     		    	_lastIndexH4 = _barH4.Range.To-1;  					
					tmH4=_barH4[_barH4.Range.To-1].Time;
				}

        }
        
        protected override void NewBar()
        {
            //XXPrint("{0} : {1} : {2} ::: ", tmH1,Fh1,tmH1,Fh1,tmH1,Fh);
			
			if(F4U && F1U && F15U) {var vr2=Tools.Create<VerticalLine>(); vr2.Color=Color.HotPink; vr2.Time=Bars[Bars.Range.To-1].Time;vr2.Width=1;
			
			if ( _ftoInd.FisherSeries[Bars.Range.To-1]>0 &&  _ftoInd.FisherSeries[Bars.Range.To-2]<0 ) {
		   var vline = Tools.Create<VerticalLine>();
           vline.Color=Color.Red;
		   vline.Time=Bars[Bars.Range.To-1].Time;
				vline.Width=4;
			}
			}
			if(!F4U && !F1U && !F15U) {var vr2=Tools.Create<VerticalLine>(); vr2.Color=Color.DarkMagenta; vr2.Time=Bars[Bars.Range.To-1].Time;vr2.Width=1;
			
			if ( _ftoInd.FisherSeries[Bars.Range.To-1]<0 &&  _ftoInd.FisherSeries[Bars.Range.To-2]>0 ) {
		   var vline = Tools.Create<VerticalLine>();
           vline.Color=Color.Blue;
		   vline.Time=Bars[Bars.Range.To-1].Time;
				vline.Width=4;
			}
		    }
			// Event occurs on every new bar
			//if(Fs>0 && Fs1<0) {var vr4=Tools.Create<VerticalLine>(); vr4.Color=Color.Red; vr4.Time=tmM15;}
			//if(Fs<0 && Fs1>0) {var vr3=Tools.Create<VerticalLine>(); vr3.Color=Color.Blue; vr3.Time=tmM15;}
			
			//if(Fh>0 && Fh1<0) {var vr2=Tools.Create<VerticalLine>(); vr2.Color=Color.DarkOrange; vr2.Time=tmH1;vr2.Width=3;}
			//if(Fh<0 && Fh1>0) {var vr1=Tools.Create<VerticalLine>(); vr1.Color=Color.DarkBlue; vr1.Time=tmH1;vr1.Width=3;} 
			
        }
 		protected void XXPrint(string xxformat, params object[] parameters)
		{   var logString=string.Format(xxformat,parameters)+Environment.NewLine;
			File.AppendAllText(trueLogPath, logString);   }       
    }
}
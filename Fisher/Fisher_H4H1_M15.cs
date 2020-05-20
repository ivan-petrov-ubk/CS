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
    [TradeSystem("Fisher_H4H1_M15")]   //copy of "ZZ_Ex4_Fractal"
    public class ZZ_Ex1 : TradeSystem
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
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2;
		private double zztb2,zzts2;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2,zzu6=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,FU,FD;
		private int zzi1,zzi2,zzi3,zzi4,zzi5,zzi6,V=0;

				private bool PrD=true,PrU=true,isF,isFU=true,isFD=false,isFMU=true,isFMD=false,isFM2U=true,isFM2D=false;
		
        protected override void Init()
        {
			periodM15 = new Period(PeriodType.Minute, 15);
//			periodH4 = new Period(PeriodType.Hour, 4);
			_barM15 = GetCustomSeries(Instrument.Id,periodM15);
//			_barH1 = GetCustomSeries(Instrument.Id,Period.H1);
//			_barH4 = GetCustomSeries(Instrument.Id,periodH4);
//			_ftoIndH4   = GetIndicator<FisherTransformOscillator>(Instrument.Id, periodH4);
//			_ftoIndH1   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Period.H1);
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
/*				if (_lastIndexH1 < _barH1.Range.To-1) {
					if(_lastIndexH1>0 && _ftoIndH1.FisherSeries[_barH1.Range.To]>_ftoIndH1.FisherSeries[_barH1.Range.To-1]) F1U=true; else F1U=false;
     		    	_lastIndexH1 = _barH1.Range.To-1;  
					tmH1=_barH1[_barH1.Range.To-1].Time;
				}
				if (_lastIndexH4 < _barH4.Range.To-1) {
				if(_lastIndexH4>0 && _ftoIndH4.FisherSeries[_barH4.Range.To]>_ftoIndH4.FisherSeries[_barH4.Range.To-1]) F4U=true; else F4U=false;	
     		    	_lastIndexH4 = _barH4.Range.To-1;  					
					tmH4=_barH4[_barH4.Range.To-1].Time;
				} */

        }
//===============================================================================================================================
        protected override void NewBar()
        {
 			
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
//=== Закрытие всех ордеров если пятница 16:00 (19:00 Kiev) ===========================================================================
          if ( Bars[Bars.Range.To-1].Time.DayOfWeek==DayOfWeek.Friday && Bars[Bars.Range.To-1].Time.Hour==16 ) 
		  {  
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
			
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
		  }
//======================================================================================================================================		  
		  	//Значение счетчика прохода фишера через 0  FU - вверх  FD - вниз
		    if ( _ftoInd.FisherSeries[Bars.Range.To-2]<0  &&  _ftoInd.FisherSeries[Bars.Range.To-1]>0) {FU=Bars.Range.To; isFU=true;} else isFU=false;
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>0  &&  _ftoInd.FisherSeries[Bars.Range.To-1]<0) {FD=Bars.Range.To; isFD=true;} else isFD=false;
			// isFMU isFMD - синяя линия пересекла линию фишера
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]<_ftoInd.Ma1Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma1Series[Bars.Range.To-1]) isFMU=true; else isFMU=false;
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>_ftoInd.Ma1Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma1Series[Bars.Range.To-1]) isFMD=true; else isFMD=false;
			// isFM2U isFM2D - красная линия пересекла линию фишера
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]<_ftoInd.Ma2Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma2Series[Bars.Range.To-1]) isFM2U=true; else isFM2U=false;
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>_ftoInd.Ma2Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma2Series[Bars.Range.To-1]) isFM2D=true; else isFM2D=false;

			// Если красная пересекла линию - закрыть все активные ордера
			if(isFMU) {if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res3 = Trade.CloseMarketPosition(posGuidSell); if (res3.IsSuccessful) posGuidSell = Guid.Empty;}	}
			if(isFMD) {if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty;}		}
			
			// Если фишер пересек 0 линию - закрыть все ордера
			if(isFD)  
			{
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			{var res2 = Trade.CancelPendingPosition(posGuidBuy); if (res2.IsSuccessful) posGuidBuy = Guid.Empty;}
			}
		    if(isFU)  
		    {
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res3 = Trade.CloseMarketPosition(posGuidSell); if (res3.IsSuccessful) posGuidSell = Guid.Empty;}	
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			{var res4 = Trade.CancelPendingPosition(posGuidSell); if (res4.IsSuccessful) posGuidSell = Guid.Empty;}	
				
			}           
			
//======================================================================================================================================
			Print("{0} - {1} - {2}>{3} - {4}",Bars[Bars.Range.To-1].Time,_barM15[_barM15.Range.To-1].Time,Fs,Fs1,F15U);
			if(F15U) { //var vr2=Tools.Create<VerticalLine>(); vr2.Color=Color.HotPink; vr2.Time=Bars[Bars.Range.To-1].Time;vr2.Width=1;
			
			if ( _ftoInd.FisherSeries[Bars.Range.To-1]>0 &&  _ftoInd.FisherSeries[Bars.Range.To-2]<0 ) {
		   var vline = Tools.Create<VerticalLine>();
           vline.Color=Color.Blue;
		   vline.Time=Bars[Bars.Range.To-1].Time;
				vline.Width=4;
				if(posGuidBuy==Guid.Empty){  	
								var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid, -1, Stops.InPips(200,200), null, null);
								if (result3.IsSuccessful)  posGuidBuy=result3.Position.Id;  } 
			}
			}
			if(!F15U) {//var vr2=Tools.Create<VerticalLine>(); vr2.Color=Color.DarkMagenta; vr2.Time=Bars[Bars.Range.To-1].Time;vr2.Width=1;
			
			if ( _ftoInd.FisherSeries[Bars.Range.To-1]<0 &&  _ftoInd.FisherSeries[Bars.Range.To-2]>0 ) {
		   var vline = Tools.Create<VerticalLine>();
           vline.Color=Color.Red;
		   vline.Time=Bars[Bars.Range.To-1].Time;
				vline.Width=4;
						 if(posGuidSell==Guid.Empty){
								var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1,Stops.InPips(200,200), null, null);
								  if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;  } 
			}
		    }
			
			
			
//===============================================================================================================================        
    }
}
}
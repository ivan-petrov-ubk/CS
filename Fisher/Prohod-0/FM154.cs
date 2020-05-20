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
    [TradeSystem("FM15")]    //copy of "Fisher_H4H1_M15"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Fisher M15=", DefaultValue = true)]
        public bool F15U { get; set; }		
		
		public DateTime tmM15,tmH1,tmH4;
		public ISeries<Bar> _barM15;
		public Period periodM15;		
		private int _lastIndexM15 = -1;	
		public FisherTransformOscillator _ftoIndM15,_ftoInd;
		public double Fs,Fs1,Fa,Fa1,F4,F41;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2;
		private double zztb2,zzts2;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2,zzu6=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,FU,FD;
		private int zzi1,zzi2,zzi3,zzi4,zzi5,zzi6,V=0;
		private AverageDirectionalMovement _admInd;

				private bool torg,PrD=true,PrU=true,isF,isFU=true,isFD=false,isFMU=true,isFMD=false,isFM2U=true,isFM2D=false,F15D=true;
		
        protected override void Init()
        {
			periodM15 = new Period(PeriodType.Minute, 60);
			_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			_ftoIndM15   = GetIndicator<FisherTransformOscillator>(Instrument.Id, periodM15);
			_admInd = GetIndicator<AverageDirectionalMovement>(Instrument.Id, periodM15);
 
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
        }        
		

        protected override void NewQuote()
        {
				if (_lastIndexM15 < _barM15.Range.To-1) {     		    	
					Fs=_ftoIndM15.FisherSeries[_barM15.Range.To];
					Fs1=_ftoIndM15.FisherSeries[_barM15.Range.To-1];
					
					Fa =_admInd.SeriesMain[_barM15.Range.To-1];
					
					
					if(_lastIndexM15>0 && Fs>Fs1 && Fa<0.3) F15U=true; else F15U=false;
					if(_lastIndexM15>0 && Fs<Fs1 && Fa<0.3) F15D=true; else F15D=false;
					
					Print("{0} - {1} {2} - {3}",Math.Round(Fa,3),F15U,F15D,Math.Round(Instrument.Spread*100000,0));

					tmM15=_barM15[_barM15.Range.To-1].Time;
					_lastIndexM15 = _barM15.Range.To-1;  
				}


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
			
			
			if(F15U) {
			if ( _ftoInd.FisherSeries[Bars.Range.To-1]>0 &&  _ftoInd.FisherSeries[Bars.Range.To-2]<0 ) {
		   var vline = Tools.Create<VerticalLine>();
           vline.Color=Color.Blue;
		   vline.Time=Bars[Bars.Range.To-1].Time;
				vline.Width=4;
				if(posGuidBuy==Guid.Empty){  	
								var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid, -1, Stops.InPips(200,null), null, null);
								if (result3.IsSuccessful)  posGuidBuy=result3.Position.Id;  } 
			}
			}
			
			if(F15D) {
			if ( _ftoInd.FisherSeries[Bars.Range.To-1]<0 &&  _ftoInd.FisherSeries[Bars.Range.To-2]>0 ) {
		   var vline = Tools.Create<VerticalLine>();
           vline.Color=Color.Red;
		   vline.Time=Bars[Bars.Range.To-1].Time;
				vline.Width=4;
						 if(posGuidSell==Guid.Empty){
								var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1,Stops.InPips(200,null), null, null);
								  if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;  } 
			}
		    }
			
			
			
//===============================================================================================================================        
    }
}
}
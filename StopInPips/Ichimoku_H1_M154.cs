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
    [TradeSystem("Ichimoku_H1_M15")]      //copy of "Ichimoku_D1H4H1"
    public class ZZ_Ex1 : TradeSystem
    {

		public DateTime tmD1,tmH1,tmH4;
		public ISeries<Bar> _barH4,_barH1,_barD1;
		public Period periodD1,periodH4;		
		private int _lastIndexD1 = -1,_lastIndexH4 = -1,_lastIndexH1 = -1;	
		private Ichimoku _ichInd,_ichIndD1,_ichIndH4,_ichIndH1;
		public FisherTransformOscillator _ftoInd;
		public double Is,Is1,Ih,Ih1,I4,I41;
		public bool FDU,FDD,F4U,F4D,F1U,F1D;
		public double Fs,Fs1,Fh,Fh1,F4,F41;
		private int FU,FD;
		private bool PrD=true,PrU=true,isF,isFU=true,isFD=false,isFMU=true,isFMD=false,isFM2U=true,isFM2D=false;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double tank1,senA1,senB1,kij1;
		private double tank2,senA2,senB2,kij2;
		
        protected override void Init()
        {
//			periodH4 = new Period(PeriodType.Hour, 4);
//			_barH4 = GetCustomSeries(Instrument.Id,periodH4);			
//			_ichIndH4   = GetIndicator<Ichimoku>(Instrument.Id, periodH4);

			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			_ichInd   = GetIndicator<Ichimoku>(Instrument.Id, Timeframe);

			_barH1 = GetCustomSeries(Instrument.Id,Period.H1);
			_ichIndH1   = GetIndicator<Ichimoku>(Instrument.Id, Period.H1); 
			
//			periodD1 = new Period(PeriodType.Day, 1);
//			_barD1 = GetCustomSeries(Instrument.Id,periodD1);
//			_ichIndD1   = GetIndicator<Ichimoku>(Instrument.Id, periodD1);
        }        
		

        protected override void NewQuote()
        {
				 if (_lastIndexH1 < _barH1.Range.To-1) {
					if(_lastIndexH1>0 && 
						 _ichIndH1.TankanSenSeries[_barH1.Range.To-1]>_ichIndH1.KijunSenSeries[_barH1.Range.To-1]) F1U=true; else F1U=false;
					if(_lastIndexH1>0 && 
						 _ichIndH1.TankanSenSeries[_barH1.Range.To-1]<_ichIndH1.KijunSenSeries[_barH1.Range.To-1]) F1D=true; else F1D=false;
					
     		    	_lastIndexH1 = _barH1.Range.To-1;  
				} 
/*				if (_lastIndexH4 < _barH4.Range.To-1) {
					if(_lastIndexH4>0 && 
						 _ichIndH4.TankanSenSeries[_barH4.Range.To-1]>_ichIndH4.KijunSenSeries[_barH4.Range.To-1]) F4U=true; else F4U=false;
					if(_lastIndexH4>0 && 
						 _ichIndH4.TankanSenSeries[_barH4.Range.To-1]<_ichIndH1.KijunSenSeries[_barH4.Range.To-1]) F4D=true; else F4D=false;
					_lastIndexH4 = _barH4.Range.To-1;  					
				}  
				if (_lastIndexD1 < _barD1.Range.To-1) {     		    	
					if(_lastIndexD1>0 && 
						 _ichIndD1.TankanSenSeries[_barD1.Range.To-1]>=_ichIndD1.KijunSenSeries[_barD1.Range.To-1]) FDU=true; else FDU=false;
					if(_lastIndexD1>0 && 
						 _ichIndD1.TankanSenSeries[_barD1.Range.To-1]<=_ichIndD1.KijunSenSeries[_barD1.Range.To-1]) FDD=true; else FDD=false;

					_lastIndexD1 = _barD1.Range.To-1;  
				}  */

        }
//===============================================================================================================================
        protected override void NewBar()
        {   
			_ichInd.ReInit();
 			tank1=_ichInd.TankanSenSeries[Bars.Range.To-1];
			senA1=_ichInd.SenkouSpanASeries[Bars.Range.To-1];
			senB1=_ichInd.SenkouSpanBSeries[Bars.Range.To-1];
			 kij1=_ichInd.KijunSenSeries[Bars.Range.To-1];
		
			tank2=_ichInd.TankanSenSeries[Bars.Range.To-2];
			 kij2=_ichInd.KijunSenSeries[Bars.Range.To-2];
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
/*		  	//Значение счетчика прохода фишера через 0  FU - вверх  FD - вниз
		    if ( _ftoInd.FisherSeries[Bars.Range.To-2]<0  &&  _ftoInd.FisherSeries[Bars.Range.To-1]>0) {FU=Bars.Range.To; isFU=true;} else isFU=false;
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>0  &&  _ftoInd.FisherSeries[Bars.Range.To-1]<0) {FD=Bars.Range.To; isFD=true;} else isFD=false;
			// isFMU isFMD - синяя линия пересекла линию фишера
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]<_ftoInd.Ma1Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma1Series[Bars.Range.To-1]) isFMU=true; else isFMU=false;
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>_ftoInd.Ma1Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma1Series[Bars.Range.To-1]) isFMD=true; else isFMD=false;
*/			// isFM2U isFM2D - красная линия пересекла линию фишера
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]<_ftoInd.Ma2Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma2Series[Bars.Range.To-1]) isFM2U=true; else isFM2U=false;
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>_ftoInd.Ma2Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma2Series[Bars.Range.To-1]) isFM2D=true; else isFM2D=false;

			// Золотое или Мертвое пересечение - красная пересекает синюю
		    if(tank1>kij1 && kij2>=tank2) isFU=true; else isFU=false;
			if(tank1<kij1 && kij2<=tank2) isFD=true; else isFD=false;	
			// Цена ВЫШЕ/НИЖЕ Senkou Span B
			if(Bars[Bars.Range.To-1].Close>senB1) isFMU=true; else isFMU=false;				
			if(Bars[Bars.Range.To-1].Close<senB1) isFMD=true; else isFMD=false; 
	  
		  
		  
			// Если красная пересекла линию - закрыть все активные ордера
			/*
			if(isFMU) {if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res3 = Trade.CloseMarketPosition(posGuidSell); if (res3.IsSuccessful) posGuidSell = Guid.Empty;}	}
			if(isFMD) {if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty;}		}
			*/
			// Если фишер пересек 0 линию - закрыть все ордера
			if(isFM2U)  
			{
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty;}
			}
		    if(isFM2D)  
		    {
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res3 = Trade.CloseMarketPosition(posGuidSell); if (res3.IsSuccessful) posGuidSell = Guid.Empty;}	
		
			}           
			
//======================================================================================================================================
			//Print("{0} - {1} - {2}>{3} - {4}",Bars[Bars.Range.To-1].Time,_barM15[_barM15.Range.To-1].Time,Fs,Fs1,F15U);
			
			
	Print("{0} - {1} {2} {3} - {4} {5} {6}",Bars[Bars.Range.To-1].Time,isFU,isFMU,FDU,isFD,isFMD,FDD)	;	
	if ( isFU && isFMU && F1U && _ftoInd.FisherSeries[Bars.Range.To-1]>0) {
		   var vline = Tools.Create<VerticalLine>();
           vline.Color=Color.Red;
		   vline.Time=Bars[Bars.Range.To-1].Time;
				vline.Width=4;
						 if(posGuidSell==Guid.Empty){
								var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1,Stops.InPips(200,null), null, null);
								  if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;  } 
			}
		   
			
	if ( isFD && isFMD && F1D && _ftoInd.FisherSeries[Bars.Range.To-1]<0) {
		   var vline = Tools.Create<VerticalLine>();
           vline.Color=Color.Blue;
		   vline.Time=Bars[Bars.Range.To-1].Time;
				vline.Width=4;
				if(posGuidBuy==Guid.Empty){  	
								var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid, -1, Stops.InPips(200,null), null, null);
								if (result3.IsSuccessful)  posGuidBuy=result3.Position.Id;  } 
			}
			
			
//===============================================================================================================================        
    }
}
}
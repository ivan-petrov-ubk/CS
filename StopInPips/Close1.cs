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
    [TradeSystem("Close1")]   //copy of "ZZ_Ex4_Fractal"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("- Buy", DefaultValue = false)]
        public bool isBuy { get; set; }		
		[Parameter("- Sell", DefaultValue = false)]
        public bool isSell { get; set; }		
		
		private ZigZag _wprInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2;
		private double zztb2,zzts2;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2,zzu6=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,FU,FD;
		private int zzi1,zzi2,zzi3,zzi4,zzi5,zzi6,V=0;
		public FisherTransformOscillator _ftoInd;
		private bool isFirst,i; 
		private bool PrD=true,PrU=true,isF,isFU=true,isFD=false,isFMU=true,isFMD=false,isFM2U=true,isFM2D=false;
		
        protected override void Init()
        {	isFirst=true;
			_ftoInd = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
        }    
		
			protected override void NewQuote()
       				 {		
						if(isFirst && isSell) {	isFirst=false;	 
			                     var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid, -1,Stops.InPips(200,600), null, null);
								 if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;  
						}
						if(isFirst && isBuy) {  isFirst=false;
								var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Ask, -1, Stops.InPips(200,600), null, null);
								 if (result3.IsSuccessful)  posGuidBuy=result3.Position.Id;
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
/*						if(isFirst && isSell) {	isFirst=false;	 
			                     var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid, -1,Stops.InPips(200,600), null, null);
								 if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;  
						}
						if(isFirst && isBuy) {  isFirst=false;
								var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Ask, -1, Stops.InPips(200,600), null, null);
								 if (result3.IsSuccessful)  posGuidBuy=result3.Position.Id;
						}
*/
//===============================================================================================================================        
	   }
	}
}
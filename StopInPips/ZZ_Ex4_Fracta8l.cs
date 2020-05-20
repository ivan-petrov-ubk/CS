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
    [TradeSystem("ZZ_Ex4")]  //copy of "ZZ_Ex3"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("ExtDepth:", DefaultValue = 5)]
        public int ED { get; set; }		
		
		private ZigZag _wprInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2;
		private double zztb2,zzts2;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2,zzu6=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,FU,FD;
		private int zzi1,zzi2,zzi3,zzi4,zzi5,zzi6,V=0;
		public FisherTransformOscillator _ftoInd;
				private bool PrD=true,PrU=true,isF,isFU=true,isFD=false,isFMU=true,isFMD=false,isFM2U=true,isFM2D=false;
		
        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
			_ftoInd = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
        }        
//===============================================================================================================================
        protected override void NewBar()
        {
 			_wprInd.ReInit();
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
		  	if ( _ftoInd.FisherSeries[Bars.Range.To-2]<0  &&  _ftoInd.FisherSeries[Bars.Range.To-1]>0) {FU=Bars.Range.To; isFU=true;} else isFU=false;
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>0  &&  _ftoInd.FisherSeries[Bars.Range.To-1]<0) {FD=Bars.Range.To; isFD=true;} else isFD=false;

			if ( _ftoInd.FisherSeries[Bars.Range.To-2]<_ftoInd.Ma1Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma1Series[Bars.Range.To-1]) isFMU=true; else isFMU=false;
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>_ftoInd.Ma1Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma1Series[Bars.Range.To-1]) isFMD=true; else isFMD=false;
			
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]<_ftoInd.Ma2Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma2Series[Bars.Range.To-1]) isFM2U=true; else isFM2U=false;
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>_ftoInd.Ma2Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma2Series[Bars.Range.To-1]) isFM2D=true; else isFM2D=false;

			
			if(isFM2U) {if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res3 = Trade.CloseMarketPosition(posGuidSell); if (res3.IsSuccessful) posGuidSell = Guid.Empty;}	}
			if(isFM2D) {if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty;}}
			
			
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
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{  
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
				 
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ
					zzd6=zzd5;zzd5=zzd4;zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu6=zzu5;zzu5=zzu4;zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
					if(zzd2>=FU && zzd2>=FD && zzd3<=FD)	
						{
						 var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[zzd2].Time; vr.Width=3;
						 if(posGuidSell==Guid.Empty){
							var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellStop, 0.1,  zzu2, 0, Stops.InPips(200,null), null, null, null);
						      if (result1.IsSuccessful) posGuidSell=result1.Position.Id; else 
						      {
								//var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid, -1,Stops.InPips(200,100), null, null);
								  var result2=Trade.Sell(Instrument.Id, 0.1); 
								  if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;  
							  }
						       } 
					    }
				}				
				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
					zzd6=zzd5;zzd5=zzd4;zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu6=zzu5;zzu5=zzu4;zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
					if(zzd2>=FD && zzd2>=FU && zzd3<=FU)
					{var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[zzd2].Time; vr.Width=3;
						if(posGuidBuy==Guid.Empty){  	
						var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyStop, 0.1, zzu2, 0, Stops.InPips(200,null), null, null, null);	
						     if (result.IsSuccessful)  posGuidBuy=result.Position.Id; else 
						     {
								 //var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1, Stops.InPips(200,100), null, null);
								var result3=Trade.Buy(Instrument.Id, 0.1); 
								 if (result3.IsSuccessful)  posGuidBuy=result3.Position.Id;
							 }
							} 
						}
				}
				

				
			}
        }
//===============================================================================================================================        
    }
}
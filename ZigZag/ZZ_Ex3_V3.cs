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
    [TradeSystem("ZZ_Ex3")] //copy of "ZZ_Ex1"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("ExtDepth:", DefaultValue = 5)]
        public int ED { get; set; }		
		
		private ZigZag _wprInd;
		public FisherTransformOscillator _ftoInd;
		private StochasticOscillator _stoInd;

		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2;
		private double zztb2,zzts2;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5,tp1,sl1;
		private int zzi1,zzi2,zzi3,zzi4,zzi5,V=0,ci;
		
		private bool isFU=true,isFD=false;
		
        protected override void Init()
        {
			_ftoInd = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
		//	_stoInd= GetIndicator<StochasticOscillator>(Instrument.Id, Timeframe);			
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=6;
        }        
//===============================================================================================================================
        protected override void NewBar()
        {
 			_wprInd.ReInit();
			_ftoInd.ReInit();
		ci = Bars.Range.To - 1;

//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed)   posGuidBuy=Guid.Empty;   
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
			
//==========  Значение счетчика прохода фишера через 0  FU - вверх  FD - вниз
		    if ( _ftoInd.FisherSeries[Bars.Range.To-2]<0  &&  _ftoInd.FisherSeries[Bars.Range.To-1]>0)   isFU=true;  else isFU=false;
 			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>0  &&  _ftoInd.FisherSeries[Bars.Range.To-1]<0)   isFD=true;  else isFD=false;			
/*			
//========  isFM2U isFM2D - красная линия пересекла линию фишера
			// Sell stop <0
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]<_ftoInd.Ma2Series[Bars.Range.To-2] &&  
				 _ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma2Series[Bars.Range.To-1]) isFU=true; else isFU=false;
			//  Buy stop >0
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>_ftoInd.Ma2Series[Bars.Range.To-2] &&  
				 _ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma2Series[Bars.Range.To-1]) isFD=true; else isFD=false;
*/			
//========== Если фишер пересек линию - закрыть все ордера  =================================================================================
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
//========== Если появился пик зигзага  ================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    // Значения 3 значений - для определения направления
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
				//======================= ПИК - ВЕШИНА ВВЕРХУ 
				if(zz2<zz3 && zz2<zz1)  
				{   // Берем 4 вершины - zzd-индекс zzu-значение
					//var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[zzi2].Time;
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
			    	// Print("ВВЕРХ {0} 1={1} 2={2} 3={3} 4={4}",Bars[zzd1].Time,zzu1,zzu2,zzu3,zzu4);
					//===== ВВЕРХ 2 импульса - на втором ^^ отложка лимит 
					//if(zzu1>zzu2 && zzu3>zzu2 && zzu1>zzu3 && zzu2>zzu4 && zzu3>zzu4) 
					if(zzu1>zzu3 && zzu2>zzu4 && zzu3>zzu5) 
						{var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[zzd2].Time;
						 if(posGuidSell==Guid.Empty){
							sl1=(int)(Math.Round(Math.Abs(zzu3-zzu2),5)*100000)-(int)(Math.Round(Math.Abs(zzu1-Bars[ci].Close),5)*100000);
							//sl1=(int)(Math.Round(Math.Abs(zzu1-Bars[ci].Close),5)*100000);
							 
							var result1 = Trade.OpenPendingPosition(Instrument.Id, 
								                                    ExecutionRule.SellLimit, 0.1,  
								                                    zzu2-Instrument.Spread-0.0002, 0, 
								                                    Stops.InPips(sl1,50), null, null, null);
							 // var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  zzu1+Instrument.Spread-0.0002, 0, Stops.InPips(200,null), null, null, null);
						        if (result1.IsSuccessful)  { posGuidSell=result1.Position.Id; zzts2=zzu3; } 
						       } 
						 }
				}	
				
				//======================  ПИК - ВЕРШИНА ВВЕРХУ
				if(zz2>zz3 && zz2>zz1)  
				{ // Берем 4 вершины - zzd-индекс zzu-значение 
					//var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[zzi2].Time;
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2; 
					
					// Print("НИЗ {0} 1={1} 2={2} 3={3} 4={4}",Bars[zzd1].Time,zzu1,zzu2,zzu3,zzu4);	
					//===== ВНИЗ 2 импульса - на втором ^^ отложка лимит 
					// if(zzu4>zzu2 && zzu3>zzu1 && zzu2>zzu3 && zzu4>zzu3) 
					if(zzu4>zzu2 && zzu5>zzu3 && zzu3>zzu1) 
						{var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[zzd2].Time;
						 if(posGuidBuy==Guid.Empty){  	
						sl1=(int)(Math.Round(Math.Abs(zzu3-zzu2),5)*100000);	
							 sl1=(int)(Math.Round(Math.Abs(zzu3-zzu2),5)*100000)-(int)(Math.Round(Math.Abs(zzu1-Bars[ci].Close),5)*100000);
						 var result = Trade.OpenPendingPosition(Instrument.Id, 
								                                ExecutionRule.BuyLimit, 0.1, 
								                                zzu2+Instrument.Spread+0.0002, 0,
								 								Stops.InPips(sl1,100), null, null, null);	
							 
						     if (result.IsSuccessful) { posGuidBuy=result.Position.Id; zztb2=zzu3;}
							} 
						}
				}
				
//=========  Если стоит лимитка Sell/Buy и цена пересекла низ/верх 2 пика - закрыть лимитку 				
				if (posGuidSell!=Guid.Empty && 
					Trade.GetPosition(posGuidSell).State==PositionState.Pending && 
					Bars[Bars.Range.To-1].Low<zzts2) 
				{
					var res = Trade.CancelPendingPosition(posGuidSell);
			     	   			if (res.IsSuccessful)  posGuidSell = Guid.Empty; 
				}
				if (posGuidBuy!=Guid.Empty && 
					Trade.GetPosition(posGuidBuy).State==PositionState.Pending && 
					Bars[Bars.Range.To-1].High>zztb2) 
				{
					var res = Trade.CancelPendingPosition(posGuidBuy);
			     	   			if (res.IsSuccessful)  posGuidBuy = Guid.Empty; 
				}
				

				
			}
        }
//===============================================================================================================================        
    }
}
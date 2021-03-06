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
    [TradeSystem("ZZ_Ex3_v2")]  //copy of "ZZ_Ex3"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("ExtDepth:", DefaultValue = 5)]
        public int ED { get; set; }		
		
		private ZigZag _wprInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2;
		private double zztb2,zzts2;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2;
		private int zzd1,zzd2,zzd3,zzd4,nu,nd;
		private int zzi1,zzi2,zzi3,zzi4,V=0;
		
        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
        }        
//===============================================================================================================================
        protected override void NewBar()
        {
 			//_wprInd.ReInit();
			/*Print("1-{0} 2-{1} 3-{2} ",
			     _wprInd.MainIndicatorSeries[Bars.Range.To-1],
			     _wprInd.MainIndicatorSeries[Bars.Range.To-2],
		     	 _wprInd.MainIndicatorSeries[Bars.Range.To-3]); */
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed)   posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-2]>0) 
			{  
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-2];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-2;
				 Print("ZZ {0} 1={1} 2={2} 3={3} ",Bars[zzi1].Time,zz1,zz2,zz3);		 
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ
					zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
			    	Print("ВВЕРХ {0} 1={1} 2={2} 3={3} 4={4}",Bars[zzd1].Time,zzu1,zzu2,zzu3,zzu4);		 
					if(zzu1>zzu2 && zzu3>zzu2 && zzu1>zzu3 && zzu2>zzu4 && zzu3>zzu4) // ВВЕРХУ
						{var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[zzd1].Time;
						
						 if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active)
						 { nd++; if(nd>2) {
								 var result2=Trade.UpdateMarketPosition(posGuidSell, zzu1 ,null, null); 
							          if (result2.IsSuccessful) posGuidSell = Guid.Empty;
							 } }
						
						 if(posGuidSell==Guid.Empty){ nd=0;
							var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  zzu1+Instrument.Spread-0.0002, 0, Stops.InPips(200,100), null, null, null);
						        if (result1.IsSuccessful)  { posGuidSell=result1.Position.Id; zzts2=zzu2; } 
						       } 
						 }
				}				
				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
					zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;
					zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2; 
					Print("НИЗ {0} 1={1} 2={2} 3={3} 4={4}",Bars[zzd1].Time,zzu1,zzu2,zzu3,zzu4);			
					if(zzu4>zzu2 && zzu3>zzu1 && zzu2>zzu3 && zzu4>zzu3) //  ВНИЗУ
						{
							var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[zzd1].Time;
						if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active)
						 {nu++; if(nu>2) {
								 var result2=Trade.UpdateMarketPosition(posGuidBuy, zzu1 ,null, null); 
							          if (result2.IsSuccessful) posGuidBuy = Guid.Empty;
							 }}
						 if(posGuidBuy==Guid.Empty){  	nu=0;
						 var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, zzu1-Instrument.Spread+0.0002, 0, Stops.InPips(200,100), null, null, null);	
						     if (result.IsSuccessful) { posGuidBuy=result.Position.Id; zztb2=zzu2;}
							} 
						}
				}
				
				if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending && Bars[Bars.Range.To-1].Low<zzts2) 
				{
					var res = Trade.CancelPendingPosition(posGuidSell);
			     	   			if (res.IsSuccessful)  posGuidSell = Guid.Empty; 
				}
				if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending && Bars[Bars.Range.To-1].High>zztb2) 
				{
					var res = Trade.CancelPendingPosition(posGuidBuy);
			     	   			if (res.IsSuccessful)  posGuidBuy = Guid.Empty; 
				}
				

				
			}
        }
//===============================================================================================================================        
    }
}
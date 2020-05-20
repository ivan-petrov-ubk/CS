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
		[Parameter("ExtDepth:", DefaultValue = 15)]
        public int ED { get; set; }		
		[Parameter("NKZ:", DefaultValue = 462)]
        public int NKZ { get; set; }	
		[Parameter("SL:", DefaultValue = 200)]
        public int SL { get; set; }	
		[Parameter("TP:", DefaultValue = 100)]
        public int TP { get; set; }			
		
		private ZigZag _wprInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		private double zz1=2,zz2=2,zz3=2;
		
        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			// _wprInd.ExtDepth=ED;
        }        
//===============================================================================================================================
        protected override void NewBar()
        {
 			_wprInd.ReInit();
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{  
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				
				 
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ
					if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending){
						//Print("Buy {0} - Price={1} - SL={2} - TP={3}",Bars[Bars.Range.To-1].Time,zz2-(NKZ*Instrument.Point)+Instrument.Spread,zz2-((NKZ-SL)*Instrument.Point),zz2-((NKZ+TP)*Instrument.Point));
					var result3 = Trade.UpdatePendingPosition(posGuidBuy, 0.1, zz2-(NKZ*Instrument.Point)+Instrument.Spread,zz2-((NKZ+SL)*Instrument.Point)+Instrument.Spread,zz2-((NKZ-TP)*Instrument.Point)+Instrument.Spread); 
				       	if (result3.IsSuccessful) posGuidBuy = result3.Position.Id;	 
						 }
					
						 if(posGuidBuy==Guid.Empty){  	
							 
						 var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, zz2-(NKZ*Instrument.Point)+Instrument.Spread, 0, Stops.InPips(SL,TP), null, null, null);	
						     if (result.IsSuccessful)  posGuidBuy=result.Position.Id; 				}

				}				
				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
				
					 if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending){
						// Print("Sell {0} - Price={1} - SL={2} - TP={3}",Bars[Bars.Range.To-1].Time,zz2+(NKZ*Instrument.Point)-Instrument.Spread,zz2+((NKZ+SL)*Instrument.Point)+Instrument.Spread,zz2+((NKZ-TP)*Instrument.Point));
					 			 var result2=Trade.UpdatePendingPosition(posGuidSell,  0.1,  zz2+(NKZ*Instrument.Point)-Instrument.Spread, zz2+((NKZ+SL)*Instrument.Point)-Instrument.Spread,zz2+((NKZ-TP)*Instrument.Point)-Instrument.Spread); 
							          if (result2.IsSuccessful) posGuidSell = result2.Position.Id;
					 }
					
					 if(posGuidSell==Guid.Empty){
							var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  zz2+(NKZ*Instrument.Point)-Instrument.Spread, 0, Stops.InPips(SL,TP), null, null, null);
						        if (result1.IsSuccessful)  posGuidSell=result1.Position.Id; }
					 
				}
	
			}
        }
//===============================================================================================================================        
    }
}
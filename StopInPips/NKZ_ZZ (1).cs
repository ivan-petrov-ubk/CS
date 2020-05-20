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
    [TradeSystem("NKZ_ZZ (1)")]  //copy of "NKZ_ZZ"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("ExtDepth:", DefaultValue = 15)]
        public int ED { get; set; }		
		[Parameter("NKZ:", DefaultValue = 462)]
        public int NKZ { get; set; }	
		[Parameter("SL:", DefaultValue = 200)]
        public int SL { get; set; }	
		[Parameter("TP:", DefaultValue = 200)]
        public int TP { get; set; }			
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public double H1,L1;
		private double zz1=2,zz2=2,zz3=2;
		public DateTime DTime; // Время		
		
        protected override void Init()
        {
			
        }        
//===============================================================================================================================
        protected override void NewBar()
        {    DTime = Bars[Bars.Range.To-1].Time;
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
// Максимальне та мінімальне знячення за день - на H1 - періоді..
			if ( DTime.Hour==23 && DTime.Minute==00 ) 
          { 	
         		var highestIndex = Series.Highest(Bars.Range.To, 24, PriceMode.High);
     			var highestPrice = Bars[highestIndex].High;
			     	H1 = highestPrice;
		    	var lowestIndex  = Series.Lowest(Bars.Range.To, 24, PriceMode.Low);
			    var lowestPrice = Bars[lowestIndex].Low;
			     	L1 = lowestPrice;
// ВВЕРХУ =================================================================================================================
					if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending){
						//Print("Buy {0} - Price={1} - SL={2} - TP={3}",Bars[Bars.Range.To-1].Time,zz2-(NKZ*Instrument.Point),zz2-((NKZ-SL)*Instrument.Point),zz2-((NKZ+TP)*Instrument.Point));
					var result3 = Trade.UpdatePendingPosition(posGuidBuy, 0.1, H1-(NKZ*Instrument.Point)+Instrument.Spread,H1-((NKZ-SL)*Instrument.Point),H1-((NKZ+TP)*Instrument.Point)); 
				       	if (result3.IsSuccessful) posGuidBuy = result3.Position.Id;	 
						 }
					
						 if(posGuidBuy==Guid.Empty){ 
						var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, H1-(NKZ*Instrument.Point)+Instrument.Spread, 0, Stops.InPips(SL,TP), null, null, null);	
						     if (result.IsSuccessful)  posGuidBuy=result.Position.Id; 	}
// ВНИЗУ =================================================================================================================
				
					 if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending){
						 //Print("Sell {0} - Price={1} - SL={2} - TP={3}",Bars[Bars.Range.To-1].Time,zz2+(NKZ*Instrument.Point),zz2+((NKZ+SL)*Instrument.Point),zz2+((NKZ-TP)*Instrument.Point));
					 			 var result2=Trade.UpdatePendingPosition(posGuidSell,  0.1,  L1+(NKZ*Instrument.Point)-Instrument.Spread, L1+((NKZ+SL)*Instrument.Point),L1+((NKZ-TP)*Instrument.Point)); 
							          if (result2.IsSuccessful) posGuidSell = result2.Position.Id;
					 }
					
					 if(posGuidSell==Guid.Empty){
							var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  L1+(NKZ*Instrument.Point)-Instrument.Spread, 0, Stops.InPips(SL,TP), null, null, null);
						        if (result1.IsSuccessful)  posGuidSell=result1.Position.Id; }
					 
						 }
			
		}
    }
}
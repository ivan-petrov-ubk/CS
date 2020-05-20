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
    [TradeSystem("PosTest")]          //copy of "LoV2"
    public class ZZ_Ex1 : TradeSystem
    {
	
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private IPosition[] posActiveMineB;
		private IPosition[] posActiveMineS;				
		public int ci=0,frac=7;
		public DateTime DTime; // Время
				private int mgS,mgB;

       protected override void Init()
        { 	

			
if (Instrument.Name == "EURUSD") {  mgB=101; mgS=201; }
if (Instrument.Name == "GBPUSD") {  mgB=102; mgS=202; }
if (Instrument.Name == "AUDUSD") {  mgB=103; mgS=203; }
if (Instrument.Name == "NZDUSD") {  mgB=104; mgS=204; }
if (Instrument.Name == "USDJPY") {  mgB=105; mgS=205; }
if (Instrument.Name == "USDCAD") {  mgB=106; mgS=206; }
if (Instrument.Name == "USDCHF") {  mgB=107; mgS=207; }
if (Instrument.Name == "AUDJPY") {  mgB=108; mgS=208; }
if (Instrument.Name == "AUDNZD") {  mgB=109; mgS=209; }
if (Instrument.Name == "CHFJPY") {  mgB=110; mgS=210; }
if (Instrument.Name == "EURAUD") {  mgB=111; mgS=211; }
if (Instrument.Name == "AUDCAD") {  mgB=112; mgS=212; }
if (Instrument.Name == "EURCAD") {  mgB=113; mgS=213; }
if (Instrument.Name == "EURCHF") {  mgB=114; mgS=214; }
if (Instrument.Name == "EURGBP") {  mgB=115; mgS=215; }
if (Instrument.Name == "EURJPY") {  mgB=116; mgS=216; }
if (Instrument.Name == "GBPCHF") {  mgB=117; mgS=217; }
if (Instrument.Name == "GBPJPY") {  mgB=118; mgS=218; }			
			
        }          
		
		
//===============================================================================================================================
        protected override void NewBar()
        {
			ci = Bars.Range.To - 1;
//====================================================================================================================================
			posActiveMineB = Trade.GetActivePositions(mgB, true);
			if(posActiveMineB!=null && posActiveMineB.Length>0 && posGuidBuy!=posActiveMineB[0].Id) posGuidBuy=posActiveMineB[0].Id; 
			posActiveMineS = Trade.GetActivePositions(mgS, true);
			if(posActiveMineS!=null && posActiveMineS.Length>0 && posGuidSell!=posActiveMineS[0].Id) posGuidSell=posActiveMineS[0].Id; 			
//=== КОРЕКЦИЯ =======================================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) { posGuidBuy=Guid.Empty;  }  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) { posGuidSell=Guid.Empty;  } 

//==== Торги =========================================================================================================================
	 

	/*				if (posGuidBuy==Guid.Empty) { 
						     var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								             Stops.InPips(200,null),null,mgB);
						     if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id;
						} 
			   			
					if (posGuidSell==Guid.Empty) {	
							 var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                 Stops.InPips(200,null),null,mgS); 
						     if (result207.IsSuccessful)  posGuidSell=result207.Position.Id;
						}
	 
*/
				if (posGuidBuy!=Guid.Empty) 
					{Print("BUY - Id={0} InstrumentId={1} MagicNumber={2} Number={3}",
						Trade.GetPosition(posGuidBuy).Id,
						Trade.GetPosition(posGuidBuy).InstrumentId,
						Trade.GetPosition(posGuidBuy).MagicNumber,
						Trade.GetPosition(posGuidBuy).Number);}
				if (posGuidSell!=Guid.Empty) 
					{Print("SELL - Id={0} InstrumentId={1} MagicNumber={2} Number={3}",
						Trade.GetPosition(posGuidSell).Id,
						Trade.GetPosition(posGuidSell).InstrumentId,
						Trade.GetPosition(posGuidSell).MagicNumber,
						Trade.GetPosition(posGuidSell).Number);}
		

			// TrailActiveOrders();
		
	 }	
	protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { var tr = Trade.UpdateMarketPosition(posGuidBuy,	  getSL(1),null," - update TP,SL"); }
		  if(posGuidSell!=Guid.Empty) { var tr = Trade.UpdateMarketPosition(posGuidSell,  getSL(0),null," - update TP,SL");  }
		} 	
			protected double getSL(int type)
		{
			switch(type)
			{   case 0: {   double MAX = double.MinValue;
							for(int i = 0; i < frac; i++)
							{ if(Bars[ci - i].High > MAX)
									MAX = Bars[ci - i].High; 
							}	
							return Math.Round(MAX+Instrument.Spread, Instrument.PriceScale);
						}
				case 1: {   double MIN = double.MaxValue;
							for(int i = 0; i < frac; i++)
							{  if(Bars[ci - i].Low < MIN)
									MIN = Bars[ci - i].Low; 
							}	
							return Math.Round(MIN-Instrument.Spread, Instrument.PriceScale);
						}
				default:  break;
			}
			return 0.0;
		}	
    }
}

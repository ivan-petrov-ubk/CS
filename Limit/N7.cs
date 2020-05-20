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
    [TradeSystem("NKZ_ZZ (2)")]  //copy of "NKZ_ZZ"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Цена NKZ",  DefaultValue = PriceMode.Close)]
        public PriceMode AppliedPrice { get; set; }
		[Parameter("ExtDepth:", DefaultValue = 15)]
        public int ED { get; set; }		
		[Parameter("SL:", DefaultValue = 200)]
        public int SL { get; set; }	
		[Parameter("TP:", DefaultValue = 100)]
        public int TP { get; set; }			
		
		private ZigZag _wprInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private Guid posGuidBuy2=Guid.Empty;
		private Guid posGuidSell2=Guid.Empty;		
		private double  zzu1,zzu2,zzd1,zzd2;
		private double zz1=2,zz2=2,zz3=2;
		private int NKZ,mgS,mgB;
		
        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
if (Instrument.Name == "EURUSD") { NKZ=462;  mgB=101; mgS=201; }
if (Instrument.Name == "GBPUSD") { NKZ=792;  mgB=102; mgS=202; }
if (Instrument.Name == "AUDUSD") { NKZ=343;  mgB=103; mgS=203; }
if (Instrument.Name == "NZDUSD") { NKZ=357;  mgB=104; mgS=204; }
if (Instrument.Name == "USDJPY") { NKZ=501;  mgB=105; mgS=205; }
if (Instrument.Name == "USDCAD") { NKZ=500;  mgB=106; mgS=206; }
if (Instrument.Name == "USDCHF") { NKZ=557;  mgB=107; mgS=207; }
if (Instrument.Name == "AUDJPY") { NKZ=550;  mgB=108; mgS=208; }
if (Instrument.Name == "AUDNZD") { NKZ=412;  mgB=109; mgS=209; }
if (Instrument.Name == "CHFJPY") { NKZ=1430; mgB=110; mgS=210; }
if (Instrument.Name == "EURAUD") { NKZ=682;  mgB=111; mgS=211; }
if (Instrument.Name == "AUDCAD") { NKZ=357;  mgB=112; mgS=212; }
if (Instrument.Name == "EURCAD") { NKZ=762;  mgB=113; mgS=213; }
if (Instrument.Name == "EURCHF") { NKZ=539;  mgB=114; mgS=214; }
if (Instrument.Name == "EURGBP") { NKZ=484;  mgB=115; mgS=215; }
if (Instrument.Name == "EURJPY") { NKZ=715;  mgB=116; mgS=216; }
if (Instrument.Name == "GBPCHF") { NKZ=924;  mgB=117; mgS=217; }
if (Instrument.Name == "GBPJPY") { NKZ=1045; mgB=118; mgS=218; }

			var posActiveMineB = Trade.GetActivePositions(mgB, true);
			if(posActiveMineB!=null && posActiveMineB.Length>0) posGuidBuy=posActiveMineB[0].Id; 
			var posActiveMineS = Trade.GetActivePositions(mgS, true);
			if(posActiveMineS!=null && posActiveMineS.Length>0) posGuidSell=posActiveMineS[0].Id; 

			var posPendingMineB = Trade.GetPendingPositions(mgB, true);
			if(posPendingMineB!=null && posPendingMineB.Length>0) posGuidBuy=posPendingMineB[0].Id; 
			var posPendingMineS = Trade.GetPendingPositions(mgS, true);
			if(posPendingMineS!=null && posPendingMineS.Length>0) posGuidSell=posPendingMineS[0].Id;
			
Print("Start - {0} - NKZ={1} mgB={2} mgS={3}",Instrument.Name,NKZ,mgB,mgS);


        }        
//===============================================================================================================================
        protected override void NewBar()
        {
 			_wprInd.ReInit();
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
			if (posGuidBuy2!=Guid.Empty && Trade.GetPosition(posGuidBuy2).State==PositionState.Closed) posGuidBuy2=Guid.Empty;   
		    if (posGuidSell2!=Guid.Empty && Trade.GetPosition(posGuidSell2).State==PositionState.Closed) posGuidSell2=Guid.Empty;  
			
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{  
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				
/// NKZ 1\4 ======================================================================================================				 
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ
					zzu2=zzu1; zzu1=zz2;
					if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending){
					     var result3 = Trade.UpdatePendingPosition(posGuidBuy, 0.1, 
							      zz2-(NKZ*Instrument.Point)+Instrument.Spread,
							      zz2-((NKZ+SL)*Instrument.Point)+Instrument.Spread,
							      zz2-((NKZ-TP)*Instrument.Point)+Instrument.Spread); 
				       	     if (result3.IsSuccessful) posGuidBuy = result3.Position.Id;	 }
					
				    if(posGuidBuy==Guid.Empty){
						  var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, 
							           zz2-(NKZ*Instrument.Point)+Instrument.Spread, 0, 
							           Stops.InPips(SL,TP), null, null, mgB);	
						     if (result.IsSuccessful)  posGuidBuy=result.Position.Id; }
/// NKZ 1\2 ==============================================================================================================
					if(posGuidBuy2!=Guid.Empty && Trade.GetPosition(posGuidBuy2).State==PositionState.Pending){
					     var result32 = Trade.UpdatePendingPosition(posGuidBuy2, 0.1, 
							            zz2-(NKZ*2*Instrument.Point)+Instrument.Spread,
							            zz2-(((NKZ*2)+SL)*Instrument.Point)+Instrument.Spread,
							            zz2-(((NKZ*2)-TP)*Instrument.Point)+Instrument.Spread); 
				       	     if (result32.IsSuccessful) posGuidBuy2 = result32.Position.Id;	 }
					
				    if(posGuidBuy2==Guid.Empty){
						  var result22 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, 
							           zz2-(NKZ*2*Instrument.Point)+Instrument.Spread, 0, 
							           Stops.InPips(SL,TP), null, null, mgS);	
						     if (result22.IsSuccessful)  posGuidBuy2=result22.Position.Id; }
				
				}				
				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
					zzd2=zzd1; zzd1=zz2;
/// NKZ 1\4 ======================================================================================================					
					 if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending){
			 			 var result2=Trade.UpdatePendingPosition(posGuidSell,  0.1,  zz2+(NKZ*Instrument.Point)-Instrument.Spread, zz2+((NKZ+SL)*Instrument.Point)-Instrument.Spread,zz2+((NKZ-TP)*Instrument.Point)-Instrument.Spread); 
							          if (result2.IsSuccessful) posGuidSell = result2.Position.Id;  }
					
					 if(posGuidSell==Guid.Empty){
							var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  zz2+(NKZ*Instrument.Point)-Instrument.Spread, 0, Stops.InPips(SL,TP), null, "NKZ4", null);
						        if (result1.IsSuccessful)  posGuidSell=result1.Position.Id; }
/// NKZ 1\2 ==============================================================================================================
					 if(posGuidSell2!=Guid.Empty && Trade.GetPosition(posGuidSell2).State==PositionState.Pending){
			 			 var result21=Trade.UpdatePendingPosition(posGuidSell2,  0.1,  zz2+(NKZ*2*Instrument.Point)-Instrument.Spread, zz2+(((NKZ*2)+SL)*Instrument.Point)-Instrument.Spread,zz2+(((NKZ*2)-TP)*Instrument.Point)-Instrument.Spread); 
							          if (result21.IsSuccessful) posGuidSell2 = result21.Position.Id;  }
					
					 if(posGuidSell2==Guid.Empty){
							var result12 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  zz2+(NKZ*2*Instrument.Point)-Instrument.Spread, 0, Stops.InPips(SL,TP), null, "NKZ2", null);
						        if (result12.IsSuccessful)  posGuidSell2=result12.Position.Id; }

					 
				}
	
			}
        }
//===============================================================================================================================        
    }
}
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
    [TradeSystem("Lo777")]         //copy of "Lo1"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Версия", DefaultValue = "06.06.2018")]
        public string CommentText { get; set; }
		
		[Parameter("StopLoss", DefaultValue = 250)]
        public int SL1 { get; set; }		
		
		[Parameter("Torg", DefaultValue = false)]
        public bool  torg { get; set; }
   			
		[Parameter("Buy", DefaultValue = false)]
        public bool  Buy7 { get; set; }	
		
		[Parameter("Sell", DefaultValue = false)]
        public bool  Sell7 { get; set; }


		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private IPosition[] posActiveMineB;
		private IPosition[] posActiveMineS;		
		public FisherTransformOscillator _ftoInd;
		public DateTime DTime; // Время
		private int mgS,mgB,ci = 0;
		private double dlt;

	
       protected override void Init()
        { 	
//==== Маркировка позиций =======================================================================================			
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
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;

//=========  Рисуем линию  начала торгов Европы =============================================================================			
			if ( DTime.Hour==7 && DTime.Minute==00 ) 
			{  var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Aqua; vl1.Width=3;	}
			if ( DTime.Hour==12 && DTime.Minute==00 ) 
			{  var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.DarkCyan; vl1.Width=3;	}				

//====== Перехват позиций при паузе. (наверное лишнее - Проверить!!!)==============================================================================================================================
			posActiveMineB = Trade.GetActivePositions(mgB, true);
			if(posActiveMineB!=null && posActiveMineB.Length>0 && posGuidBuy!=posActiveMineB[0].Id) posGuidBuy=posActiveMineB[0].Id; 
			posActiveMineS = Trade.GetActivePositions(mgS, true);
			if(posActiveMineS!=null && posActiveMineS.Length>0 && posGuidSell!=posActiveMineS[0].Id) posGuidSell=posActiveMineS[0].Id; 
		
//=== КОРЕКЦИЯ =======================================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) { posGuidBuy=Guid.Empty;  }  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) { posGuidSell=Guid.Empty;  } 
				

//==== Торги =========================================================================================================================
	 if ( DTime.Hour==6 && DTime.Minute==59 ) {
					if (posGuidBuy==Guid.Empty && Buy7 && torg) { torg=false;		
						     var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								             Stops.InPips(SL1,null),null,mgB);
						     if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id;
							 
							
						} 
			   			
					if (posGuidSell==Guid.Empty && Sell7 && torg) {	torg=false; 	
							 var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                 Stops.InPips(SL1,null),null,mgS); 
						     if (result207.IsSuccessful)  posGuidSell=result207.Position.Id;
							
							 
						}
	 }
//----- Сбор логов --------------------------------------------------------------------------------------------------------------			
					
		}		
//===== Функции =================================================================================================================   
    }
}

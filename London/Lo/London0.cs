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
    [TradeSystem("London0")]     //copy of "London7-15"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("StopLoss:", DefaultValue = 250)]
        public int SL1 { get; set; }		
		

		[Parameter("Buy:", DefaultValue = false)]
        public bool  Buy7 { get; set; }		
		[Parameter("Sell:", DefaultValue = false)]
        public bool  Sell7 { get; set; }		
		
		
		[Parameter("Fractal :", DefaultValue = 7, MinValue = 2, MaxValue = 200)]
		public int frac { get;set; }	
		
		[Parameter("Пауза :", DefaultValue = 7, MinValue = 2, MaxValue = 200)]
		public int kf { get;set; }			
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private Guid posActiveMineB=Guid.Empty;
		private Guid posActiveMineS=Guid.Empty;		
		public DateTime DTime,zzt1,zzt2,Upt; // Время
		private bool Down1,u1,torg,frTop,torg7,torg12;
		private int k7,k12,k;
		private double frUp,frDown,frU1,frU2,frD1,frD2,sr1;
		private int mgS,mgB;
		private double zz1=2,zz2=2,zz3=2,sl1,sl2;
		
	
		public static int ci = 0;
		
       protected override void Init()
        { k=0;
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
			
			var posActiveMineB = Trade.GetActivePositions(mgB, true);
			if(posActiveMineB!=null && posActiveMineB.Length>0 && posGuidBuy!=posActiveMineB[0].Id) posGuidBuy=posActiveMineB[0].Id; 
			var posActiveMineS = Trade.GetActivePositions(mgS, true);
			if(posActiveMineS!=null && posActiveMineS.Length>0 && posGuidSell!=posActiveMineS[0].Id) posGuidSell=posActiveMineS[0].Id; 
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) { posGuidBuy=Guid.Empty; k7=0; }  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) { posGuidSell=Guid.Empty; k7=0; }  
	
//=== Трелинг  ===========================================================================================================	
			k++; if(k>kf) TrailActiveOrders();
//====================================================================================================================================
		 
					if (posGuidBuy==Guid.Empty && Buy7) {	k=0;			
						var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
												Stops.InPips(SL1,null), null,  mgB);
							if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id;
						} 
			   			
					if (posGuidSell==Guid.Empty && Sell7) {	 k=0;			
							 var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                   Stops.InPips(SL1,null),null,   mgS); 
						if (result207.IsSuccessful)  posGuidSell=result207.Position.Id;
						}
		}		
//===============================================================================================================================   
		protected void TrailActiveOrders()
		{		

		  if(posGuidBuy!=Guid.Empty)  { var tr = Trade.UpdateMarketPosition(posGuidBuy,	getSL(1),null," - update TP,SL"); }
		  if(posGuidSell!=Guid.Empty) { var tr = Trade.UpdateMarketPosition(posGuidSell,  getSL(0),null," - update TP,SL"); } 		  
		}	
		
		protected double getSL(int type)
		{
			switch(type)
			{
				case 0:
						{
							double MAX = double.MinValue;
							for(int i = 0; i < frac; i++)
							{
								if(Bars[ci - i].High > MAX)
									MAX = Bars[ci - i].High; 
							}	
							return Math.Round(MAX, Instrument.PriceScale);
						}
				case 1:
						{
							double MIN = double.MaxValue;
							for(int i = 0; i < frac; i++)
							{
								if(Bars[ci - i].Low < MIN)
									MIN = Bars[ci - i].Low; 
							}	
							return Math.Round(MIN, Instrument.PriceScale);
						}
				default: 
					break;
			}
			return 0.0;
		}
    }
}
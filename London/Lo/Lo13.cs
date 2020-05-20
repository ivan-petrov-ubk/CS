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
    [TradeSystem("Lo1")]        //copy of "Lo0"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("StopLoss:", DefaultValue = 250)]
        public int SL1 { get; set; }		
		
		[Parameter("Torg:", DefaultValue = false)]
        public bool  torg { get; set; }			

		[Parameter("Buy:", DefaultValue = false)]
        public bool  Buy7 { get; set; }	
		
		[Parameter("Sell:", DefaultValue = false)]
        public bool  Sell7 { get; set; }		
		
		[Parameter("Fractal :", DefaultValue = 7, MinValue = 2, MaxValue = 200)]
		public int frac { get;set; }	
		
		[Parameter("Пауза :", DefaultValue = 7, MinValue = 2, MaxValue = 200)]
		public int kf { get;set; }		

		[Parameter("Fisher MA1:", DefaultValue = false)]
        public bool  Fs1 { get; set; }
		
		[Parameter("Fisher 0:", DefaultValue = false)]
        public bool  Fs0 { get; set; }
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private IPosition[] posActiveMineB;
		private IPosition[] posActiveMineS;		
		public FisherTransformOscillator _ftoInd;
		public DateTime DTime,zzt1,zzt2,Upt; // Время
		private bool FsU,FsD;
		private int mgS,mgB,k,ci = 0;
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		
		private string trueLogPath = "";		
		
       protected override void Init()
        { k=0;FsU=false;FsD=false;
			Print("{0} Start INIT ",Bars[Bars.Range.To-1].Time);
			InitLogFile(); 
			 _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
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
//=======  Определение разворота ====================================================================================================			
if(Fs1) {	
	if(_ftoInd.FisherSeries[Bars.Range.To-1]> _ftoInd.Ma1Series[Bars.Range.To-1]) FsU=true; else FsU=false;
	if(_ftoInd.FisherSeries[Bars.Range.To-1]< _ftoInd.Ma1Series[Bars.Range.To-1]) FsD=true; else FsD=false;
}
if(Fs0) {	
	if(_ftoInd.FisherSeries[Bars.Range.To-2]<0 && _ftoInd.FisherSeries[Bars.Range.To-1]>0) FsU=true; else FsU=false;
	if(_ftoInd.FisherSeries[Bars.Range.To-2]>0 && _ftoInd.FisherSeries[Bars.Range.To-1]<0) FsD=true; else FsD=false;
}
if(!Fs1 && !Fs0) { FsU=Buy7; FsD=Sell7; }
//====================================================================================================================================
			posActiveMineB = Trade.GetActivePositions(mgB, true);
			if(posActiveMineB!=null && posActiveMineB.Length>0 && posGuidBuy!=posActiveMineB[0].Id) posGuidBuy=posActiveMineB[0].Id; 
			posActiveMineS = Trade.GetActivePositions(mgS, true);
			if(posActiveMineS!=null && posActiveMineS.Length>0 && posGuidSell!=posActiveMineS[0].Id) posGuidSell=posActiveMineS[0].Id; 
		
//=== КОРЕКЦИЯ =======================================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) { posGuidBuy=Guid.Empty; k=0; }  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) { posGuidSell=Guid.Empty; k=0; } 
				
//=== Трелинг  =======================================================================================================================	
			if(posGuidBuy!=Guid.Empty || posGuidSell!=Guid.Empty) k++; 
			if ( k>kf ) TrailActiveOrders();
//====================================================================================================================================
	 
					if (posGuidBuy==Guid.Empty && Buy7 && torg && FsU) { torg=false;	k=0;	
						     var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								             Stops.InPips(SL1,null),null,mgB);
						     if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id;
							 XXPrint("{0} BUY START posGuidBuy={1} ",Bars[ci].Time,posGuidBuy.ToString());		
						} 
			   			
					if (posGuidSell==Guid.Empty && Sell7 && torg && FsD) {	torg=false; k=0;		
							 var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                 Stops.InPips(SL1,null),null,mgS); 
						     if (result207.IsSuccessful)  posGuidSell=result207.Position.Id;
							 XXPrint("{0} SELL SATRT posGuidSell={1}",Bars[ci].Time,posGuidSell.ToString());		
						}
//------------------------------------------------------------------------------------------------------------------------------------			
					
	if(posGuidBuy!=Guid.Empty)  XXPrint("{0} ----- k={1} kf={2} Buy7={3},Sell7={4} Fs1={5} Fs0={6} FsU={7} FsD={8}  PS={9} torg={11} {10}",Bars[ci].Time,k,kf, Buy7,Sell7,Fs1,Fs0,FsU,FsD,posActiveMineS.Length,posGuidBuy.ToString(),torg);
	if(posGuidSell!=Guid.Empty)	XXPrint("{0} ----- k={1} kf={2} Buy7={3},Sell7={4} Fs1={5} Fs0={6} FsU={7} FsD={8}  PS={9} torg={11} {10}",Bars[ci].Time,k,kf, Buy7,Sell7,Fs1,Fs0,FsU,FsD,posActiveMineS.Length,posGuidSell.ToString(),torg);
	if(posGuidSell==Guid.Empty && posGuidBuy==Guid.Empty)
		                        XXPrint("{0} ----- k={1} kf={2} Buy7={3},Sell7={4} Fs1={5} Fs0={6} FsU={7} FsD={8} torg={10} PS={9}",Bars[ci].Time,k,kf, Buy7,Sell7,Fs1,Fs0,FsU,FsD,posActiveMineS.Length,torg);
		}		
//===============================================================================================================================   
		protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { var tr = Trade.UpdateMarketPosition(posGuidBuy,	  getSL(1),null," - update TP,SL"); 
		  XXPrint("{0} BUY  UPDATE FsU={2} FsD={3}posGuidBuy={1}",Bars[ci].Time,posGuidBuy.ToString() ,FsU,FsD);
		  }
		  if(posGuidSell!=Guid.Empty) { var tr = Trade.UpdateMarketPosition(posGuidSell,  getSL(0),null," - update TP,SL"); 
		  XXPrint("{0} SELL UPDATE FsU={2} FsD={3} posGuidSell={1}",Bars[ci].Time,posGuidSell.ToString(),FsU,FsD); }
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
				protected void InitLogFile()
		{
			trueLogPath=PathToLogFile+"\\"+Instrument.Name.ToString()+".log";
		}
		
		protected void XXPrint(string xxformat, params object[] parameters)
		{
				var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);
		}
    }
}

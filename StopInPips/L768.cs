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
    [TradeSystem("L768")]         //copy of "Lo1"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Версия", DefaultValue = "07.06.2018 Для М1 по фракталах")]
        public string CommentText { get; set; }
		
		[Parameter("StopLoss", DefaultValue = 250)]
        public int SL1 { get; set; }		
		
		//[Parameter("Torg", DefaultValue = false)]
        //public bool  torg { get; set; }			

		//[Parameter("Buy", DefaultValue = false)]
        //public bool  Buy7 { get; set; }	
		
		//[Parameter("Sell", DefaultValue = false)]
        //public bool  Sell7 { get; set; }		
		
		[Parameter("Fractal", DefaultValue = 7)]
		public int frac { get;set; }	
		[Parameter("Отступ Stop :", DefaultValue = 30)]
		public int dl { get;set; }			
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private IPosition[] posActiveMineB;
		private IPosition[] posActiveMineS;		
		public FisherTransformOscillator _ftoInd;
		public DateTime DTime; // Время
		private bool FsU,FsD,first,Buy7,Sell7,torg;
		private int mgS,mgB,ci = 0,k;
		private double dlt,F1,F2,F3;
		public Fractals _frInd;	
		private double frUp,frDown,frUp0,frDown0;		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		private string trueLogPath = "";	

		
       protected override void Init()
        { 	FsU=false;FsD=false; k=0;
			Buy7=false; Sell7=false; torg=false;
			Print("{0} Start INIT ",Bars[Bars.Range.To-1].Time);
			InitLogFile(); 
			dlt=dl*Instrument.Point; 
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
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
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			_frInd.Range = frac; 
			first=false; 
        }            
//===============================================================================================================================
        protected override void NewBar()
        {
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
			F1=_ftoInd.FisherSeries[Bars.Range.To-1];
			F2=_ftoInd.FisherSeries[Bars.Range.To-2];
			F3=_ftoInd.FisherSeries[Bars.Range.To-3];
//=========  Рисуем линию  начала торгов Европы =============================================================================			
			if ( DTime.Hour==7 && DTime.Minute==00 ) 
			{  var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Aqua; vl1.Width=2;	}			
//====  Fractal ====================================================================================
			  if (_frInd.TopSeries[Bars.Range.To-frac-2]>0) 		frUp=Bars[Bars.Range.To-frac-2].High; 
			  if (_frInd.BottomSeries[Bars.Range.To-frac-2]>0)      frDown=Bars[Bars.Range.To-frac-2].Low; 			
		
//====== Перехват позиций при паузе. (наверное лишнее - Проверить!!!)==============================================================================================================================
			posActiveMineB = Trade.GetActivePositions(mgB, true);
			if(posActiveMineB!=null && posActiveMineB.Length>0 && posGuidBuy!=posActiveMineB[0].Id) posGuidBuy=posActiveMineB[0].Id; 
			posActiveMineS = Trade.GetActivePositions(mgS, true);
			if(posActiveMineS!=null && posActiveMineS.Length>0 && posGuidSell!=posActiveMineS[0].Id) posGuidSell=posActiveMineS[0].Id; 
		
//=== КОРЕКЦИЯ =======================================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) { posGuidBuy=Guid.Empty;  }  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) { posGuidSell=Guid.Empty;  } 

			if ( DTime.Hour<6 ) { torg=true; first=false; } 

//=== Трелинг  =======================================================================================================================	
			if(!first && posGuidBuy!=Guid.Empty && _frInd.BottomSeries[Bars.Range.To-frac-2]>0) first=true;
			if(!first && posGuidSell!=Guid.Empty && _frInd.TopSeries[Bars.Range.To-frac-2]>0)   first=true;
			if(first) TrailActiveOrders();
			//Print("{0} frac={1}  {2}   {3}",DTime,frac,_frInd.BottomSeries[Bars.Range.To-frac-2],_frInd.TopSeries[Bars.Range.To-frac-2]);
//=============================================================================================================================
 	 if ( DTime.Hour==6 && DTime.Minute==59 ) {    
//=== Определение   тренда =======================================================================================================================	
			//if(F1>F2 && F2>F3) Sell7=true;
			//if(F1<F2 && F2<F3) Buy7=true;	
			if(F1>0 && F2>0 && F3>0) Sell7=true;
			if(F1<0 && F2<0 && F3<0) Buy7=true;	
			
			Print("{0} k={1}-{2}",DTime,F1>F2,F2>F3);
//==== Торги =========================================================================================================================
					if (posGuidBuy==Guid.Empty && Buy7 && torg) { torg=false;	k=0;	
						     var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								             Stops.InPips(SL1,null),null,mgB);
						     if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id;
						} 
			   			
					if (posGuidSell==Guid.Empty && Sell7 && torg) {	torg=false; k=0;	
							 var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                 Stops.InPips(SL1,null),null,mgS); 
						     if (result207.IsSuccessful)  posGuidSell=result207.Position.Id;
						}
	 }
	//if(posGuidBuy!=Guid.Empty || posGuidSell!=Guid.Empty)  k++;		
	//if(posGuidBuy!=Guid.Empty && k>50)  { var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty; k=0;}			
	//if(posGuidSell!=Guid.Empty && k>50)  { var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty; k=0;}		
    //Print("{0} k={1}",DTime,k);
	}		
//===== Функции =================================================================================================================   
		protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { 
			  var tr = Trade.UpdateMarketPosition(posGuidBuy,	  getSLfr(1),null,DTime.ToString()); 
		  
		  }
		  if(posGuidSell!=Guid.Empty) { 
			  var tr = Trade.UpdateMarketPosition(posGuidSell,  getSLfr(0),null,DTime.ToString()); 
		  }
		} 		  
			
	    protected double getSLfr(int type)
	    {
			switch(type)
			{   case 0: { return Math.Round(frUp+dlt+Instrument.Spread, Instrument.PriceScale); 
						    Print("{0} frUp={1}-{2}",DTime,frUp);
				var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-frac-2].Time; vl1.Color=Color.Red;
			            }
				case 1: { return Math.Round(frDown-dlt-Instrument.Spread, Instrument.PriceScale); 
										    Print("{0} frUp={1}-{2}",DTime,frDown);
				var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-frac-2].Time; vl1.Color=Color.Blue;  }
			default:  break;
						}
			return 0.0;		
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
							return Math.Round(MAX+dlt+Instrument.Spread, Instrument.PriceScale);
						}
				case 1:
						{
							double MIN = double.MaxValue;
							for(int i = 0; i < frac; i++)
							{
								if(Bars[ci - i].Low < MIN)
									MIN = Bars[ci - i].Low; 
							}	
							return Math.Round(MIN-dlt-Instrument.Spread, Instrument.PriceScale);
						}
				default: 
					break;
			}
			return 0.0;
		}
		protected void InitLogFile()
		{trueLogPath=PathToLogFile+"\\"+Instrument.Name.ToString()+".log";}
		
		protected void XXPrint(string xxformat, params object[] parameters)
		{
				var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);
		}
    }
}

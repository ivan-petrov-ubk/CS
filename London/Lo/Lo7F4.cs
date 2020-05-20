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
    [TradeSystem("Lo7F")]         //copy of "Lo1"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("StopLoss:", DefaultValue = 200)]
        public int SL1 { get; set; }		
	
		[Parameter("Fractal :", DefaultValue = 40, MinValue = 2, MaxValue = 200)]
		public int frac { get;set; }	
		
		[Parameter("Пауза :", DefaultValue = 20, MinValue = 0, MaxValue = 200)]
		public int kf { get;set; }	
		
		[Parameter("Пауза Stop :", DefaultValue = 10, MinValue = 0, MaxValue = 200)]
		public int ks { get;set; }	
		
		[Parameter("Отступ Stop :", DefaultValue = 30, MinValue = 0, MaxValue = 200)]
		public int dl { get;set; }			
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private IPosition[] posActiveMineB;
		private IPosition[] posActiveMineS;	
		public DateTime tmM15;
		public FisherTransformOscillator _ftoInd,_ftoIndM15;
		public DateTime DTime,zzt1,zzt2,Upt; // Время
		private bool FsU,FsD;
		private int mgS,mgB,k,ci = 0;
		public ISeries<Bar> _barM15;
		private int _lastIndexM15 = -1;
		public Period periodM15;	
		public double Fs2,Fs1,min1,max1,max2,min2,dlt;
		private bool F15U,F15D,torg;
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		
		private string trueLogPath = "";		
		
       protected override void Init()
        {   k=0;FsU=false;FsD=false; 
			dlt=dl*Instrument.Point; 
			Print("{0} Start INIT dlt={1}",Bars[Bars.Range.To-1].Time,dlt);
			InitLogFile(); 
			periodM15 = new Period(PeriodType.Minute, 15);
			_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			 _ftoIndM15   = GetIndicator<FisherTransformOscillator>(Instrument.Id, periodM15); 
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

        protected override void NewQuote()
        {
				if (_lastIndexM15 < _barM15.Range.To-1) {     		    	
					Fs2=Fs1;
					Fs1=_ftoIndM15.FisherSeries[_barM15.Range.To-1]; // Серия фишера 
					if(_lastIndexM15>0 && Fs1>Fs2) F15U=true; else F15U=false;
					tmM15 = _barM15[_barM15.Range.To-1].Time;
					_lastIndexM15 = _barM15.Range.To-1;  
				}
		}
//===============================================================================================================================
        protected override void NewBar()
        {
			DTime = Bars[Bars.Range.To-1].Time;
			        ci = Bars.Range.To - 1;
			if ( DTime.Hour<7 ) torg=true; // Начинаем новый торговый день
			
//===== Перехват открытых позиций  ============================================================================================================
			posActiveMineB = Trade.GetActivePositions(mgB, true);
			if(posActiveMineB!=null && posActiveMineB.Length>0 && posGuidBuy!=posActiveMineB[0].Id) posGuidBuy=posActiveMineB[0].Id; 
			posActiveMineS = Trade.GetActivePositions(mgS, true);
			if(posActiveMineS!=null && posActiveMineS.Length>0 && posGuidSell!=posActiveMineS[0].Id) posGuidSell=posActiveMineS[0].Id; 
//=== КОРЕКЦИЯ =======================================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) { posGuidBuy=Guid.Empty; k=0; }  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) { posGuidSell=Guid.Empty; k=0; } 
			if(posGuidBuy!=Guid.Empty || posGuidSell!=Guid.Empty) k++; 
//====  Рисуем вертикальную линию - начало трейлинга =========================================================
			if (k==kf) { var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Red ;}
//=== Трейлинг открытых позиций  =====================================================================================	
			if ( k>kf ) TrailActiveOrders();
//====== Определяем пересечение нуля фишером========================================================================================
	if(_ftoInd.FisherSeries[Bars.Range.To-2]<0 && _ftoInd.FisherSeries[Bars.Range.To-1]>0) FsU=true; else FsU=false;
	if(_ftoInd.FisherSeries[Bars.Range.To-2]>0 && _ftoInd.FisherSeries[Bars.Range.To-1]<0) FsD=true; else FsD=false;

//=========  Рисуем линию  начала торгов =============================================================================			
			if ( DTime.Hour==7 && DTime.Minute==00 ) 
			{  k=0;  var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Aqua; vl1.Width=3;	}
//========  Печать статистики переменных (для отладки)==========================================
//			Print("{0} -  FsU={4} FsD={5} F15U={6}  SL1={7} k={8} --  tmM15={1} Fs1={2} Fs2={3}",DTime,tmM15,Fs1,Fs2,FsU,FsD,F15U,SL1,k);			
//====== Открытие позиций =============================================================================================================
			if ( DTime.Hour==7 ) {	
					if (posGuidBuy==Guid.Empty && !F15U && FsU && torg) { torg=false;	k=0;	
						     var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								             Stops.InPips(SL1,null),null,mgB);
						     if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id;
							 //XXPrint("{0} BUY START posGuidBuy={1} ",Bars[ci].Time,posGuidBuy.ToString());		
						} 
			   			
					if (posGuidSell==Guid.Empty && F15U && FsD && torg) { torg=false; k=0;		
							 var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                 Stops.InPips(SL1,null),null,mgS); 
						     if (result207.IsSuccessful)  posGuidSell=result207.Position.Id;
							 //XXPrint("{0} SELL SATRT posGuidSell={1}",Bars[ci].Time,posGuidSell.ToString());		
						}
					}
					}		
//==== Трелинг позиции ===========================================================================================================================   
		protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { var tr = Trade.UpdateMarketPosition(posGuidBuy,	  getSL(1),null," - update TP,SL"); 
//		  Print("{0} BUY  UPDATE FsU={2} FsD={3} min1={4} min2={5} posGuidBuy={1}",Bars[ci].Time,posGuidBuy.ToString() ,FsU,FsD,min1,min2);
		  }
		  if(posGuidSell!=Guid.Empty) { var tr = Trade.UpdateMarketPosition(posGuidSell,  getSL(0),null," - update TP,SL"); 
//		  Print("{0} SELL UPDATE FsU={2} FsD={3} max1={4} max2={5} posGuidSell={1}",Bars[ci].Time,posGuidSell.ToString(),FsU,FsD,max1,max2); 
		  }
		} 		  
			
//=====  Определяем стоп ============================
		protected double getSL(int type)
		{
			switch(type)
			{
				case 0:
						{
							double MAX = double.MinValue;
							for(int i = ks; i < frac; i++)
							{
								if(Bars[ci - i].High > MAX)
									MAX = Bars[ci - i].High; 
							}	
							return Math.Round(MAX+dlt+Instrument.Spread, Instrument.PriceScale);
						}
				case 1:
						{
							double MIN = double.MaxValue;
							for(int i = ks; i < frac; i++)
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
//======    Печать в файл =============================
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

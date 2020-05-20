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
    [TradeSystem("LoV41")]          //copy of "LoV4"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("StopLoss:", DefaultValue = 200)]
        public int SL1 { get; set; }	
		[Parameter("Отступ Stop :", DefaultValue = 20)]
		public int dl { get;set; }	
		[Parameter("Fractal", DefaultValue = 7)]
		public int frac { get;set; }
		[Parameter("Fisher Sig", DefaultValue = false)]
        public bool FU { get; set; }		
		
		private double zz1=2,zz2=2,zz3=2;
		private double m1F,m5F;		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private IPosition[] posActiveMineB;
		private IPosition[] posActiveMineS;				
		public FisherTransformOscillator _ftoInd,_ftoIndM1;
		public DateTime DTime; // Время
		private bool torg=true,tr;
		public bool isFU,isFD,isFMU,isFMD,isFM2U,isFM2D;
		//private int mgS,mgB,
		private int ci = 0, k=0;
		//private double dlt;
		//public Fractals _frInd;	
		//private double frUp,frDown,frUp0,frDown0;		
		private double dlt,OP,CP,TPB,TPS;	
				private int mgS,mgB;
		public ISeries<Bar> _barM1;
				private int _lastIndexM1 = -1;
		public DateTime tmM1,tmM5,t1,t2,t3,t4,t5,t6,t7,t8,t9,t10;
		
       protected override void Init()
        { 	

			tr=false;
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
			_barM1 = GetCustomSeries(Instrument.Id,Period.M1);
			_ftoIndM1   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Period.M1);
			
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
          {	if (_lastIndexM1 < _barM1.Range.To-1) {
     		    _lastIndexM1 = _barM1.Range.To - 1;
			    m1F = _ftoIndM1.FisherSeries[_lastIndexM1];
			    tmM1=_barM1[_barM1.Range.To - 1].Time;
 		 
		       }		  
		  }		
	
//===============================================================================================================================
        protected override void NewBar()
        {
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
			dlt=dl*Instrument.Point; 
//=========  Рисуем линию  начала торгов Европы =============================================================================			
			if ( DTime.Hour==7 && DTime.Minute==00 ) 
			{  var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Aqua; vl1.Width=3;	}
			    m5F = _ftoInd.FisherSeries[Bars.Range.To-1];
//======================================================================================================================================		  
		  	//Значение счетчика прохода фишера через 0  FU - вверх  FD - вниз
		    if ( _ftoInd.FisherSeries[Bars.Range.To-2]<0  &&  _ftoInd.FisherSeries[Bars.Range.To-1]>0) isFU=true; else isFU=false;
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>0  &&  _ftoInd.FisherSeries[Bars.Range.To-1]<0) isFD=true; else isFD=false;
			// isFMU isFMD - синяя линия пересекла линию фишера
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]<_ftoInd.Ma1Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma1Series[Bars.Range.To-1]) isFMU=true; else isFMU=false;
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>_ftoInd.Ma1Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma1Series[Bars.Range.To-1]) isFMD=true; else isFMD=false;
			// isFM2U isFM2D - красная линия пересекла линию фишера
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]<_ftoInd.Ma2Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma2Series[Bars.Range.To-1]) isFM2U=true; else isFM2U=false;
			if ( _ftoInd.FisherSeries[Bars.Range.To-2]>_ftoInd.Ma2Series[Bars.Range.To-2] &&  _ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma2Series[Bars.Range.To-1]) isFM2D=true; else isFM2D=false;
			
//====================================================================================================================================
			posActiveMineB = Trade.GetActivePositions(mgB, true);
			if(posActiveMineB!=null && posActiveMineB.Length>0 && posGuidBuy!=posActiveMineB[0].Id) posGuidBuy=posActiveMineB[0].Id; 
			posActiveMineS = Trade.GetActivePositions(mgS, true);
			if(posActiveMineS!=null && posActiveMineS.Length>0 && posGuidSell!=posActiveMineS[0].Id) posGuidSell=posActiveMineS[0].Id; 			
//=== КОРЕКЦИЯ =======================================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) {  
				OP=Trade.GetPosition(posGuidBuy).OpenPrice;
				CP=Trade.GetPosition(posGuidBuy).ClosePrice.Value;
				TPB=Math.Round((CP-OP)*Instrument.LotSize-Instrument.Spread,0)-16;
			  	Print("Корекция {0} - Open={1} Close={2} TPB={3}",DTime,OP,CP,TPB); 
				    posGuidBuy=Guid.Empty;  }  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) {  
				OP=Trade.GetPosition(posGuidSell).OpenPrice;
				CP=Trade.GetPosition(posGuidSell).ClosePrice.Value;
				TPS=Math.Round((OP-CP)*Instrument.LotSize+Instrument.Spread,0)-16;
			  	Print("Корекция {0} - Open={1} Close={2} TPS={3}",DTime,OP,CP,TPS); 
				posGuidSell=Guid.Empty;  } 
//==== Торги =========================================================================================================================
			 if ( DTime.Hour<6 ) { torg=true; k=0; tr=false; TPB=0.0; TPS=0.0;}  
//==== Торги =========================================================================================================================
		 if ( DTime.Hour==7 && DTime.Minute==5 ) {  Print("{0} - {1}  {2} - {3}",DTime,m5F,tmM1,m1F);
					if (posGuidBuy==Guid.Empty &&  m5F<0 && m1F<0) { 		
						     var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								             Stops.InPips(SL1,null),null,null);
						     if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id;
						} 
			   			
					if (posGuidSell==Guid.Empty && m5F>0 && m1F>0) {
							 var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                 Stops.InPips(SL1,null),null,null); 
						     if (result207.IsSuccessful)  posGuidSell=result207.Position.Id;
						}
	 }
//===================================================================================================================	 
// Если 15:00 по Киеву - закрыть все ордера
	 if ( DTime.Hour>10 &&  DTime.Minute>30 )  
			{  	if (posGuidBuy!=Guid.Empty) 
					{   
					OP=Trade.GetPosition(posGuidBuy).OpenPrice;
					CP=Trade.GetPosition(posGuidBuy).ClosePrice.Value;
					TPB=Math.Round((CP-OP)*Instrument.LotSize-Instrument.Spread,0)-16;
			  		Print("12:00 {0} - Open={1} Close={2} TPB={3}",DTime,OP,CP,TPB); 
						
					var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty;}
					
				if (posGuidSell!=Guid.Empty) 
					{   
						OP=Trade.GetPosition(posGuidSell).OpenPrice;
						CP=Trade.GetPosition(posGuidSell).ClosePrice.Value;
						TPS=Math.Round((OP-CP)*Instrument.LotSize+Instrument.Spread,0)-16;
			  			Print("12:00 {0} - Open={1} Close={2} TPS={3}",DTime,OP,CP,TPS); 
						var res2 = Trade.CloseMarketPosition(posGuidSell); if (res2.IsSuccessful) posGuidSell = Guid.Empty;}
			}
//===================================================================================================================				
			 if ( DTime.Hour==12 ) {  Print("12:05 {0} - TPB={1} TPS={2} {3} {4}",DTime,TPB,TPS,posGuidBuy,posGuidSell);		
				if (posGuidBuy==Guid.Empty && TPB<-100) {
						var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
						Stops.InPips(SL1,null),null,null);
						if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id;
					}
				if(posGuidSell==Guid.Empty && TPS<-100) {
						var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 	Stops.InPips(SL1,null),null,null); 
						if (result207.IsSuccessful)  posGuidSell=result207.Position.Id;
					}	
			}
//===================================================================================================================				
// Если 15:00 по Киеву - закрыть все ордера
	 if ( DTime.Hour==20 )  
			{  	if (posGuidBuy!=Guid.Empty) 
					{ var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty; }
					
				if (posGuidSell!=Guid.Empty) 
					{ var res2 = Trade.CloseMarketPosition(posGuidSell); if (res2.IsSuccessful) posGuidSell = Guid.Empty; }
			}	
//===================================================================================================================				
// Если фишер пересек красную линию - закрыть все ордера
			if(FU) {
				if (posGuidBuy!=Guid.Empty && isFM2D) 
					{ TPB=Trade.GetPosition(posGuidBuy).Pips; Print("Fisher {0} - TPB={2}",DTime,TPB);
						var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty;}
				if (posGuidSell!=Guid.Empty && isFM2U) 
					{ TPS=Trade.GetPosition(posGuidSell).Pips; Print("Fisher {0} - TPS={2}",DTime,TPS);
						var res2 = Trade.CloseMarketPosition(posGuidSell); if (res2.IsSuccessful) posGuidSell = Guid.Empty;}
			}
			
//===================================================================================================================				
			
		if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).Pips>50) tr=true;
		if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).Pips>50) tr=true;
			if(tr)  TrailActiveOrders();
		
	 }	
//===================================================================================================================	
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
							return Math.Round(MAX+dlt+Instrument.Spread, Instrument.PriceScale);
						}
				case 1: {   double MIN = double.MaxValue;
							for(int i = 0; i < frac; i++)
							{  if(Bars[ci - i].Low < MIN)
									MIN = Bars[ci - i].Low; 
							}	
							return Math.Round(MIN-dlt-Instrument.Spread, Instrument.PriceScale);
						}
				default:  break;
			}
			return 0.0;
		}	
    }
}

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
    [TradeSystem("TC Пробой M5_V4")]   //copy of "TC Пробой M5_V3"
    public class TC_Пробой_M5 : TradeSystem
    {
		

		public double H1,L1,O1,C1,H2,L2,O2,C2,O3,C3,U1,D1;
		private int _lastIndexH1 = -1;
		public ISeries<Bar> _barH1;
		public bool F1U,LogD,LogU,torg,isFMU,isFMD,isPrU,isPrD;
		public FisherTransformOscillator _ftoIndH1,_ftoInd;
		private StochasticOscillator _stoIndH1;
		public HorizontalLine Line1,Line2;
		public DateTime DTime;
		public int Dt;
		public double zkU,zkD,TPC,TPU,TPD,SLD,SLU;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		
        protected override void Init()
        {   torg=false;LogU=true; LogD=true; U1=100;D1=-1;
			Line1 = Tools.Create<HorizontalLine>();
			Line2 = Tools.Create<HorizontalLine>();
			_stoIndH1= GetIndicator<StochasticOscillator>(Instrument.Id, Timeframe);
			_stoIndH1.Slowing=3;
			_stoIndH1.PeriodD=3;
			_stoIndH1.PeriodK=14; // Основна
		}        

      
        protected override void NewBar()
        {		O1 = Bars[Bars.Range.To-1].Open;
				C1 = Bars[Bars.Range.To-1].Close;
				H1 =  Bars[Bars.Range.To-1].High;
				L1 =  Bars[Bars.Range.To-1].Low;
				DTime = Bars[Bars.Range.To-1].Time;
			
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) { posGuidBuy=Guid.Empty; LogU=false;}  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) {posGuidSell=Guid.Empty; LogD=false;}
//=== Закрытие всех ордеров если пятница 16:00 (19:00 Kiev) ===========================================================================
          if ( Bars[Bars.Range.To-1].Time.DayOfWeek==DayOfWeek.Friday && Bars[Bars.Range.To-1].Time.Hour==16 ) 
		  {  
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
			
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
		  }

		  
//======================================================================================================================================			
		//Print("{0} - torg={1} LogU={2} LogD={3} Hour={4} Min={5}",DTime,torg,LogU,LogD,DTime.Hour,DTime.Minute);
        // Максимальне та мінімальне знячення за день - на H1 - періоді..
		if ( DTime.Hour==23 && DTime.Minute==00 ) 
          {   
			  LogU=true; LogD=true; torg=false;
         		var highestIndex = Series.Highest(Bars.Range.To, 24, PriceMode.High);
     			var highestPrice = Bars[highestIndex].High;
			     	U1 = highestPrice;
		    	var lowestIndex  = Series.Lowest(Bars.Range.To, 24, PriceMode.Low);
			    var lowestPrice = Bars[lowestIndex].Low;
			     	D1 = lowestPrice;
			        // Різниця в пунктах між MAX-MIN
			  		Dt=(int)Math.Round((U1-D1)*100000,0);
			  		// Малюємо горизонтальну лінію на рівнях
			  		Line1.Price = U1;
			  		Line1.Text = string.Format("{0}",Dt);
					Line2.Price = D1;

		  		TPU=Math.Round(U1+(U1-D1)*0.2353,5);
			    TPD=Math.Round(D1-(U1-D1)*0.2353,5); 
			  
			    
							  }   
		  
		  if ( DTime.Hour==04 && DTime.Minute==00 && LogU && LogD) 
		  {  SLU=Math.Round(U1-0.002,5); SLD=Math.Round(D1+0.002,5);
			  torg=true;
var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyStop, 0.1, U1, 0, Stops.InPrice(SLU,TPU), null, null, null);
						 if (result.IsSuccessful)  posGuidBuy=result.Position.Id; 
var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellStop, 0.1,  D1, 0, Stops.InPrice(SLD,TPD), null, null, null);
						 if (result1.IsSuccessful)  posGuidSell=result1.Position.Id; 

		  }
		  if ( DTime.Hour==20 && DTime.Minute==00 ) 
		  {  torg=false;
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
			
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
          }

		  
//======================================================================================================================================		  
           if(C1>U1 && O1<U1)  zkU=Math.Abs((C1-U1)*100/(C1-O1));
		   if(O1>D1 && C1<D1) zkD=Math.Abs((D1-C1)*100/(C1-O1)); 
           if(C1<U1) zkU=0; if(O1>U1) zkU=100; 
		   if(C1>D1) zkD=0; if(O1<D1) zkD=100; 
			  //Print("zkU={0} zkD={1} {2} {3}",zkU,zkD,TPU,TPD);
//======================================================================================================================================
		  if (LogU && C1>U1) {  LogU=false; Print("1 - {0}",_stoIndH1.SignalLine[Bars.Range.To-1]>80.0);
			  if(_stoIndH1.SignalLine[Bars.Range.To-1]>80.0) { Print("2");
			zkU=Math.Abs((C1-U1)*100/(C1-O1));  
			  Print("{0} zkU={1}",DTime,zkU);   
		  
			if(torg && posGuidBuy==Guid.Empty && zkU>20){
				SLD=Math.Round(Bars[Bars.Range.To-1].Close-0.002,5);
				//var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Ask,-1,Stops.InPips(200,TPU),null);
				//var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Ask,-1,Stops.InPrice(SLU,TPU),null);
				//if (result.IsSuccessful) posGuidBuy=result.Position.Id;
			} 
						 			  
			 var vline = Tools.Create<VerticalLine>();
           	 vline.Color=Color.Red; vline.Width=4;
		     vline.Time=Bars[Bars.Range.To-2].Time;	
			}}
//======================================================================================================================================		  
		  if (LogD &&  C1<D1) {	LogD=false; Print("3 - {0}",_stoIndH1.SignalLine[Bars.Range.To-1]<20.0);
			  if(_stoIndH1.SignalLine[Bars.Range.To-1]<20.0) { Print("2");
			  zkD=Math.Abs((D1-C1)*100/(C1-O1));
			  Print("{0} zkD={1}",DTime,zkD);  
			  	 
			
			  if(torg && posGuidSell==Guid.Empty && zkD>20){ 
				  SLD=Math.Round(Bars[Bars.Range.To-1].Close+0.002,5);
		   		//var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid,-1,Stops.InPips(200,TPU),null);
				//  var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid,-1,Stops.InPrice(SLD,TPD),null);
				//if (result.IsSuccessful) posGuidSell=result.Position.Id; 
				  }
			  
			 var vline = Tools.Create<VerticalLine>();
           	 vline.Color=Color.Blue; vline.Width=4;
		     vline.Time=Bars[Bars.Range.To-2].Time;
			  }}
		  
		  
			  Print("{0} - torg={1} LogU={2} LogD={3} Hour={4} Min={5} TPU={6} TPD={7} Stoh={8}",DTime,torg,LogU,LogD,DTime.Hour,DTime.Minute,TPU,TPD,_stoIndH1.SignalLine[Bars.Range.To-1]);	
	  
		  
//======================================================================================================================================		  
		/*	if (_ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma1Series[Bars.Range.To-1] && 
				                   _ftoInd.FisherSeries[Bars.Range.To]>_ftoInd.Ma1Series[Bars.Range.To]) isFMU=true; else isFMU=false;
			if (_ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma1Series[Bars.Range.To-1] &&  
				                   _ftoInd.FisherSeries[Bars.Range.To]<_ftoInd.Ma1Series[Bars.Range.To]) isFMD=true; else isFMD=false;
			
		  
		  if(isFMU)  
			{   if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
					{var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty;}  }
		    
			if(isFMD)  
		    {  if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			       {var res3 = Trade.CloseMarketPosition(posGuidSell); if (res3.IsSuccessful) posGuidSell = Guid.Empty;}  }  		  	 
		  	*/

        }
    }
}

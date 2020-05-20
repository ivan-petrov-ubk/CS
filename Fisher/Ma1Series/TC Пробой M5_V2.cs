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
    [TradeSystem("TC Пробой M5_V2")] //copy of "TC Пробой M5"
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
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		
        protected override void Init()
        {   torg=false;LogU=false; LogD=false; U1=100;D1=-1;
			 isPrU=false;isPrD=false; 
			_barH1 = GetCustomSeries(Instrument.Id,Period.H1);
			Line1 = Tools.Create<HorizontalLine>();
			Line2 = Tools.Create<HorizontalLine>();
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			_ftoIndH1 = GetIndicator<FisherTransformOscillator>(Instrument.Id, Period.H1);
			_stoIndH1= GetIndicator<StochasticOscillator>(Instrument.Id, Period.H1);
			_stoIndH1.Slowing=3;
			_stoIndH1.PeriodD=3;
			_stoIndH1.PeriodK=14; // Основна
		}        

        protected override void NewQuote()
        {
            if (_lastIndexH1 < _barH1.Range.To-1) {
			 //if(_lastIndexH1>0 && _ftoIndH1.FisherSeries[_barH1.Range.To]>_ftoIndH1.FisherSeries[_barH1.Range.To-1]) F1U=true; else F1U=false;
			 // isFMU isFMD - синяя линия пересекла линию фишера
			if (_lastIndexH1>0 &&  _ftoIndH1.FisherSeries[_barH1.Range.To-1]<_ftoIndH1.Ma1Series[_barH1.Range.To-1] && 
				                   _ftoIndH1.FisherSeries[_barH1.Range.To]>_ftoIndH1.Ma1Series[_barH1.Range.To]) isFMU=true; else isFMU=false;
			if (_lastIndexH1>0 &&  _ftoIndH1.FisherSeries[_barH1.Range.To-1]>_ftoIndH1.Ma1Series[_barH1.Range.To-1] &&  
				                   _ftoIndH1.FisherSeries[_barH1.Range.To]<_ftoIndH1.Ma1Series[_barH1.Range.To]) isFMD=true; else isFMD=false;
			_lastIndexH1 = _barH1.Range.To-1; 
			
			if(_stoIndH1.SignalLine[_barH1.Range.To-1]>80 && _barH1[_barH1.Range.To-1].Close>U1) isPrU=true;
			if(_stoIndH1.SignalLine[_barH1.Range.To-1]<20 && _barH1[_barH1.Range.To-1].Close<D1) isPrD=true; 
			
			Print("{0} - {1} {2} - {3} {4} - {5}",_barH1[_barH1.Range.To-1].Time,_ftoIndH1.FisherSeries[_barH1.Range.To-1],_ftoIndH1.Ma1Series[_barH1.Range.To-1],isFMU,isFMD,_stoIndH1.SignalLine[_barH1.Range.To]);	
		   }
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
		if ( DTime.Hour==23 && DTime.Minute==55 ) 
          {   //Print("{0} - torg={1} LogU={2} LogD={3} Hour={4} Min={5}",DTime,torg,LogU,LogD,DTime.Hour,DTime.Minute);	
			  torg=true;
         		var highestIndex = Series.Highest(Bars.Range.To, 288, PriceMode.High);
     			var highestPrice = Bars[highestIndex].High;
			     	U1 = highestPrice;
		    	var lowestIndex  = Series.Lowest(Bars.Range.To, 288, PriceMode.Low);
			    var lowestPrice = Bars[lowestIndex].Low;
			     	D1 = lowestPrice;
			        // Різниця в пунктах між MAX-MIN
			  		Dt=(int)Math.Round((U1-D1)*100000,0);
			  		// Малюємо горизонтальну лінію на рівнях
			  		Line1.Price = U1;
			  		Line1.Text = string.Format("{0}",Dt);
					Line2.Price = D1;
		  }   
//======================================================================================================================================
		  if(!LogU && H1>U1) LogU=true; 
		  if(!LogD && H1<D1) LogD=true;
//======================================================================================================================================
		  if (torg && isPrU && _ftoInd.FisherSeries[Bars.Range.To-1]>0 &&  _ftoInd.FisherSeries[Bars.Range.To-2]<0 ) {		   
				isPrU=false;
			if(Bars[Bars.Range.To-1].Open>U1 && posGuidBuy==Guid.Empty){ torg=false;
				var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Ask,-1,Stops.InPips(200,null),null);
				if (result.IsSuccessful) posGuidBuy=result.Position.Id; 
						 			  
			 var vline = Tools.Create<VerticalLine>();
           	 vline.Color=Color.Red; vline.Width=4;
		     vline.Time=Bars[Bars.Range.To-1].Time;	}}
//======================================================================================================================================		  
		  if (torg && isPrD && _ftoInd.FisherSeries[Bars.Range.To-1]<0 &&  _ftoInd.FisherSeries[Bars.Range.To-2]>0 ) {			 
			  	isPrD=false;  
			 if(Bars[Bars.Range.To-1].Open<D1 && posGuidSell==Guid.Empty){ torg=false;
		   		var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid,-1,Stops.InPips(200,null),null);
				if (result.IsSuccessful) posGuidSell=result.Position.Id; 
						 				  
			 var vline = Tools.Create<VerticalLine>();
           	 vline.Color=Color.Blue; vline.Width=4;
		     vline.Time=Bars[Bars.Range.To-1].Time;}}
//======================================================================================================================================		  
			if(isFMU)  
			{   if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
					{var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty;}  }
		    
			if(isFMD)  
		    {  if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			       {var res3 = Trade.CloseMarketPosition(posGuidSell); if (res3.IsSuccessful) posGuidSell = Guid.Empty;}  }  		  	 
		  		  
        }
    }
}

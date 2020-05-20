using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using IPro.Model.Client.MarketData;
using IPro.Model.Programming.Indicators.Standard;
using System.Collections.Generic;

namespace IPro.TradeSystems
{
    [TradeSystem("Аллигатор+Фрактал 15")]
    public class АллигаторФрактал_15 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		public double TP=0.0003;
		private Guid _positionGuid=Guid.Empty;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
        // Simple parameter example
		public Alligator _allInd;
		public Fractals _frInd;
		double BarH,BarL,BarC; 		
		double aGuba5, aZub5, aChelust5;   // Челюсть Синяя
		double aGuba, aZub, aChelust;
		public DateTime DTime;		
		public double frSU=0,frSD=0;
		
				// Fractal
		double frUpH = 0.0;   // Значение текущего верхнего Fractal
		double frUpL = 0.0;    // Значение Low - свечи с верхним фрактклом
        double frUpC = 0.0;
		double frDownH = 0.0;    // Значение High - свечи с нижним фракталом
		double frDownL = 0.0;  // Значение текущего нижнего Fractal
		double frDownC = 0.0;
       // 1. Фрактал выше/ниже зубов Alligator
		double fr_all_Up;     // Цена последней свечи с фракталом - полностью выше Аллигатора
		double fr_all_Down;   // Цена последней свечи с фракталом - полностью ниже Аллигатора		
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			// Вставить индикатор Alligator
			_allInd = GetIndicator<Alligator>(Instrument.Id, Timeframe);
			// Вставить индикатор Fractals
		    _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
// =================================================================================================================================				 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) 
			     {  posGuidBuy=Guid.Empty; Print("Buy - Закрыто по StopLoss (Корекция) - {0} ",DTime);}

			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) 
				 {  posGuidSell=Guid.Empty;  Print("Sell - Закрыто по StopLoss (Корекция) - {0} ",DTime); }
// =================================================================================================================================					 
           			// Значения текущего Бара
			BarH = Bars[Bars.Range.To-1].High;
			BarL = Bars[Bars.Range.To-1].Low;
			BarC = Bars[Bars.Range.To-1].Close;
			DTime = Bars[Bars.Range.To-1].Time;
			
			//  frUp frDown - Истина если появился НОВЫЙ фрактал Вверх/Вниз
			frSU=_frInd.TopSeries[Bars.Range.To-5];
			frSD=_frInd.BottomSeries[Bars.Range.To-5];	
			// Значения Alligator около фрактала
			aGuba5 = _allInd.LipsSeries[Bars.Range.To-5];
            aZub5 = _allInd.TeethSeries[Bars.Range.To-5];
			aChelust5 =  _allInd.JawsSeries[Bars.Range.To-5];
						// Значене Alligator у цены
			aGuba = _allInd.LipsSeries[Bars.Range.To];      //З
            aZub = _allInd.TeethSeries[Bars.Range.To];      //К
			aChelust =  _allInd.JawsSeries[Bars.Range.To];  //С

			// =================================================================================================================================				 
     		// Срабатывает - когда появился новый фрактал - frUp frDown=true!
			// Запоминаем значения Свечи бара-фрактала(frUpH) и время (frUp_Time)
			  if (frSU>0)    {  frUpH=Bars[Bars.Range.To-5].High; 
				                frUpL=Bars[Bars.Range.To-5].Low;
				                frUpC=Bars[Bars.Range.To-5].Close; }
			  
			  if (frSD>0)    { frDownL=Bars[Bars.Range.To-5].Low; 
				               frDownH=Bars[Bars.Range.To-5].High; 
				               frDownC=Bars[Bars.Range.To-5].Close; }

//===============================================================================================================================			
     		// Появился новый фрактал ВНИЗ  и Свеча Fractalа НИЖЕ  Alligatora не касается Alligatorа 
			// ВЕРХ Бар-Фрактала НИЖЕ Alligator  - Назначаем рабочим (fr_all_Down) для SellLimit
			if (frSD>0) { if(frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5)  fr_all_Down=frDownL; else fr_all_Down=0; }
            if (frSU>0) { if(frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5)  fr_all_Up=frUpH; else fr_all_Up=0; }

//===============================================================================================================================
			// Появился новый фрактал ВВЕРХ и открыта позиция Sell - переосим стоп 
		   if (frSU>0 && posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active)  
		   { var res3=Trade.UpdateMarketPosition(posGuidSell, frUpH, frDownL-0.00100, null); 
		   if (res3.IsSuccessful)  posGuidSell=res3.Position.Id;  }
//===============================================================================================================================
			// Появился новый фрактал ВВЕРХ и открыта позиция Sell - переосим стоп 
		   if (frSD>0 && posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active)  
		   { var res4=Trade.UpdateMarketPosition(posGuidBuy, frDownL, frUpH+0.00100, null); 
			   if (res4.IsSuccessful)  posGuidBuy=res4.Position.Id; }
//=============================================================================================================================/==		   			
			// Отскок
		    if(fr_all_Up>aChelust && BarH>fr_all_Up+TP && BarC<fr_all_Up && posGuidSell==Guid.Empty && posGuidBuy==Guid.Empty) 
		    {   //var result=Trade.Sell(Instrument.Id, 0.1);
				//var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Ask,-1,Stops.InPips(200,100),null);
				//if (result.IsSuccessful) posGuidBuy=result.Position.Id; Print("1"); 
				var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask,-1,Stops.InPips(200,100),null);
				if (result.IsSuccessful) posGuidSell=result.Position.Id; Print("1"); 
			}	
			
			if(fr_all_Down>0 && fr_all_Down<aChelust && BarL<fr_all_Down-TP && BarC>fr_all_Down && posGuidBuy==Guid.Empty && posGuidSell==Guid.Empty) 
			{//var result=Trade.Buy(Instrument.Id, 0.1); 
				//var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid,-1,Stops.InPips(200,100),null);
				//if (result.IsSuccessful) posGuidSell=result.Position.Id;  Print("2");
				var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid,-1,Stops.InPips(200,100),null);
				if (result.IsSuccessful) posGuidBuy=result.Position.Id;  Print("2");
			
			}		
			// Пробой	
		    
			if(fr_all_Up>aChelust && BarH>fr_all_Up+TP && BarC>fr_all_Up+TP && posGuidBuy==Guid.Empty  && posGuidSell==Guid.Empty)  
			{   //var result=Trade.Buy(Instrument.Id, 0.1);
				//var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid,-1,Stops.InPips(200,100),null);
				//if (result.IsSuccessful) posGuidSell=result.Position.Id;  Print("3");
				var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid,-1,Stops.InPips(200,100),null);
				if (result.IsSuccessful) posGuidBuy=result.Position.Id;  Print("3");
			
			}	
			
			if(fr_all_Down>0 && fr_all_Down<aChelust && BarL<fr_all_Down-TP && BarC<fr_all_Down-TP && posGuidSell==Guid.Empty  && posGuidBuy==Guid.Empty) 
			{   //var result=Trade.Sell(Instrument.Id, 0.1);
				//var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Ask,-1,Stops.InPips(200,100),null);
				//if (result.IsSuccessful) posGuidBuy=result.Position.Id;  Print("4");}	
				var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask,-1,Stops.InPips(200,100),null);
				if (result.IsSuccessful) posGuidSell=result.Position.Id;  Print("4");}	

        }
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
    }
}
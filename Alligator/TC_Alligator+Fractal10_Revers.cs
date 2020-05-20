// Автор: Рымарь Виктор
// Skype: rymar_victor
//  
//  Торговая система скопирована с видео :
//  https://www.youtube.com/watch?v=un9gck_8q04#t=1172
// https://www.youtube.com/watch?v=cUfVK_l09nE
//
//  Торговая система "Alligator + Fractals"
//1. Если Линии Alligator переплетены - сигнал Torg1
//2. Берутся фракталы - которые не касаются Alligator fr_all_Up
//3. Новый  фрактал в ТУ ЖЕ СТОРОНУ  - анулирует предыдущий или становится активным
//4. Позиция открывается при пересечении линии АКТИВНОГО фрактала + Спред + отступ

//    1. Периоды активности Аллигатора делятся на 4 следующих:
 //   Аллигатор просыпается - бары с разных сторон нулевой линии окрашены в разные цвета.
 //   Аллигатор ест - зеленые бары с обеих сторон нулевой линии.
 //   Аллигатор насыщается - во время "еды" с одной из сторон появляется красный бар.
 //   Аллигатор спит - бары с обеих сторон красного цвета.
//  2. Торгуем с 6:00 до 14:00
// 3. При срабатывнии - торговля заканчивается.

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
    [TradeSystem("TC_Alligator+Fractal10_Revers")]       //copy of "TC_Alligator+Fractal10"
    public class Alligator1 : TradeSystem
    {
		public double SL=0.003;
		public double TP=0.0003;
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
        // Simple parameter example
		public Alligator _allInd;
		public Fractals _frInd;
				
		double aGuba5, aZub5, aChelust5;   // Челюсть Синяя
		double aGuba1, aZub1, aChelust1;
		double aGuba;  //  Губы Зеленая
		double aZub;    // Зубы  Красная
		double aChelust;   // Челюсть Синяя
		
		double BarH,BarL,BarC; 
				
		// Fractal
		double frUpH = 0.0;   // Значение текущего верхнего Fractal
		double frUpL = 0.0;    // Значение Low - свечи с верхним фрактклом
        double frUpC = 0.0;
		double frDownH = 0.0;    // Значение High - свечи с нижним фракталом
		double frDownL = 0.0;  // Значение текущего нижнего Fractal
		double frDownC = 0.0;
		
		public DateTime frUp_Time; // Время текущего фрактала вверху
		public DateTime frDown_Time; // Время текущего фрактала внизу
		
        // 1. Фрактал выше/ниже зубов Alligator
		double fr_all_Up;     // Цена последней свечи с фракталом - полностью выше Аллигатора
		double fr_all_Down;   // Цена последней свечи с фракталом - полностью ниже Аллигатора
		bool fr_all_Down_L, fr_all_Up_L;
		public bool frUp=false;
		public bool frDown=true;
		bool allSpit;
		public DateTime fr_all_Up_Time;  // Время последней свечи с фракталом - полностью выше Аллигатора
		public DateTime fr_all_Down_Time; // Время последней свечи с фракталом - полностью ниже Аллигатора
		public DateTime DTime;
				
		// Линии показывают АКТИВНЫЕ точки
		private VerticalLine vlR,vlB,vlG,vlY;
		private HorizontalLine hline;
		private HorizontalLine lline;
		
			
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
			// Вставить индикатор Alligator
			_allInd = GetIndicator<Alligator>(Instrument.Id, Timeframe);
			// Вставить индикатор Fractals
		    _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			
			hline = Tools.Create<HorizontalLine>();
			lline = Tools.Create<HorizontalLine>();
			vlR = Tools.Create<VerticalLine>();
			vlR.Color=Color.Red;
			vlB = Tools.Create<VerticalLine>();
			vlB.Color=Color.Blue;
			vlY = Tools.Create<VerticalLine>();
			vlY.Color=Color.Yellow;
			vlG = Tools.Create<VerticalLine>();
			vlG.Color=Color.DarkGreen;

        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        { 
		
			// Значения текущего Бара
			BarH = Bars[Bars.Range.To-1].High;
			BarL = Bars[Bars.Range.To-1].Low;
			BarC = Bars[Bars.Range.To-1].Close;
			DTime = Bars[Bars.Range.To-1].Time;

			// Значения Alligator около фрактала
			aGuba5 = _allInd.LipsSeries[Bars.Range.To-5];
            aZub5 = _allInd.TeethSeries[Bars.Range.To-5];
			aChelust5 =  _allInd.JawsSeries[Bars.Range.To-5];
			
			// Значене Alligator у цены
			aGuba = _allInd.LipsSeries[Bars.Range.To];      //З
            aZub = _allInd.TeethSeries[Bars.Range.To];      //К
			aChelust =  _allInd.JawsSeries[Bars.Range.To];  //С


			//  frUp frDown - Истина если появился НОВЫЙ фрактал Вверх/Вниз
			if(_frInd.TopSeries[Bars.Range.To-5]>0) { frUp=true; } else { frUp=false; }
			if(_frInd.BottomSeries[Bars.Range.To-5]>0) { frDown=true; } else { frDown=false; }

// ===  SET BARS FRACTAL ==============================================================================================================================				 
     		// Срабатывает - когда появился новый фрактал - frUp frDown=true!
			// Запоминаем значения Свечи бара-фрактала(frUpH) и время (frUp_Time)
			  if (frUp)    {    //Print("Появился фрактал ВВЕРХ - {0}",Bars[Bars.Range.To-5].Time);
				                frUpH=Bars[Bars.Range.To-5].High; 
				                frUpL=Bars[Bars.Range.To-5].Low;
				                frUpC=Bars[Bars.Range.To-5].Close;
				                frUp_Time = Bars[Bars.Range.To-5].Time;   }
			  
			  if (frDown)   {   //Print("Появился фрактал ВНИЗ - {0}",Bars[Bars.Range.To-5].Time);
				                frDownL=Bars[Bars.Range.To-5].Low; 
				                frDownH=Bars[Bars.Range.To-5].High; 
				                frDownC=Bars[Bars.Range.To-5].Close; 
				                frDown_Time = Bars[Bars.Range.To-5].Time;  }
              	  
//====== RUN FRACTAL BUY =========================================================================================================================		  
		      // Появился новый фрактал ВВЕРХ  и Свеча Fractalа ВЫШЕ Alligatora не касается Alligatorа 
			  // низ Бар-Фрактала выше Alligator  - Назначаем рабочим (fr_all_Up) для Buy
			  // fr_all_Up_L - если появился рабочий фрактал Buy - true
			  if (frUp) { // Появился новый фрактал ВВЕРХ
				  if(frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5) //Низ Бар-Фрактала выше Alligator 
				   {  Red();  vlB.Time=Bars[Bars.Range.To-1000].Time;
			          fr_all_Up=frUpH; fr_all_Up_Time=frUp_Time;  fr_all_Up_L=true; 
					  Print("02 Фрактал ВВЕРХ и выше Алигатора! - {0} ",Bars[Bars.Range.To-1].Time);  
				   }
			      else 
//===== CANCEL BAY  ==========================================================================================================================					   
			      // Если появился новый фрактал  ВВЕРХ и касается Алигатора - отменяем "рабочий"
			       {  fr_all_Up_L=false; vlR.Time=Bars[Bars.Range.To-1000].Time;
				      Print("03 Фрактал ВВЕРХ но касается Алигатора! - {0}",Bars[Bars.Range.To-1].Time); 
			          if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
				        var res = Trade.CancelPendingPosition(posGuidSell); 
				        if (res.IsSuccessful) { posGuidSell=Guid.Empty;
				        Print("03-1 Появился новый ВВЕРХ фрактал и НЕ подходит под рабочий - отменяем пред. BuyStop - CancelPending - {0} ",DTime);}} 
			       }      }
			  
//==== RUN FRACTAL SELL ===========================================================================================================================			
     		// Появился новый фрактал ВНИЗ  и Свеча Fractalа НИЖЕ  Alligatora не касается Alligatorа 
			// ВЕРХ Бар-Фрактала НИЖЕ Alligator  - Назначаем рабочим (fr_all_Down) для SellLimit
			if (frDown) {  //  Появился новый фрактал ВНИЗ
				if(frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5) 
				   { Blue();  vlR.Time=Bars[Bars.Range.To-1000].Time;
					 fr_all_Down=frDownL;  fr_all_Down_Time=frDown_Time; fr_all_Down_L=true;
					 Print("04 Фрактал НИЖЕ Алигатора! - {0}",Bars[Bars.Range.To-1].Time); 
				   } 
			    else  
//===== CANCEL SELL ==========================================================================================================================
					   // Если появился новый фрактал  ВНИЗ  и касается Алигатора - отменяем "рабочий"	
			       {  fr_all_Down_L=false;   vlB.Time=Bars[Bars.Range.To-1000].Time;
				      Print("05 Фрактал ВНИЗ и касается Алигатора! - {0}",Bars[Bars.Range.To-1].Time); 
			          if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) {
				           var res = Trade.CancelPendingPosition(posGuidBuy);
				           if (res.IsSuccessful) { posGuidBuy = Guid.Empty;
				           Print("05-2 появился новый фр-рабочий ВНИЗ  и не рабочий отменяем отложеный Sell CancelPending! - {0}",DTime);}}
			       }  	}
//==== UPDATE  BAY ===========================================================================================================================
			// Появился новый фрактал ВВЕРХ и назначен рабочим и есть отложеный ордер  
			// меняем цену открытия и ставим TP - Низ/Верх противоположного фрактала
			
			if ( frUp && fr_all_Up_L ) {
				 // Ордер SellLimit-ВНИЗ - Цена открытия=High раб фрактала
				if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending)
				   {   var result = Trade.UpdatePendingPosition(posGuidSell, 0.1, fr_all_Up+SL,fr_all_Down, null); 
				       if (result.IsSuccessful) { posGuidSell = result.Position.Id;
 			           Print("06 UpdatePending - !появился новый фр-рабочий - переносим цену и StopLoss TP -  {0}",fr_all_Up,fr_all_Down,DTime);}
				   }
				   
            
				   //======= CANCELL SELL ========================================================================================================================			
			    if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			       { var res = Trade.CancelPendingPosition(posGuidBuy);
			     	 if (res.IsSuccessful) { posGuidBuy = Guid.Empty;
				     Print("06-2 появился новый фр-рабочий ВВЕРХ и рабочий отменяем отложеный Sell CancelPending - {0}",DTime);}}
			                              }
			
//=== CANCEL BUY ============================================================================================================================			
			// Если появился новый фрактал ВВЕРХ и не подходит под рабочий и есть отрытый отложеный ордер - отменяем ордер Buy
			if(frUp && !fr_all_Up_L) { 
				if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
				 var res = Trade.CancelPendingPosition(posGuidSell); 
				 if (res.IsSuccessful) { posGuidSell=Guid.Empty;
				Print("07 Появился новый ВВЕРХ фрактал и не подходит под рабочий - отменяем пред. BuyStop - CancelPending - {0} ",DTime);}
                              }}
			 	
//==== CANCEL & STOP BAY===========================================================================================================================			
		    // Появился новый фрактал ВВЕРХ  и назначен рабочим и нет отложек- ставим отложеный ордер 
		    //  и при этом нет открытых ордеров  (posGuidBuy)
			// $$== ТОРГУЕМ!!!! ==$$  открываем BuyStop - Цена=High раб. ВВЕРХ фрактала SL=200p TP=Низ против. фрактала
			
		    if(frUp && fr_all_Up_L && posGuidSell==Guid.Empty) 	
			{	 // Закрываем SellStop
			    if (posGuidBuy!=Guid.Empty){
				     // Отменяем отложеный SellStop
	                 if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) { 
		                   var res = Trade.CancelPendingPosition(posGuidBuy);   
					       Print("12 Отменяем SellStop бо появился раб-фрактал  - {0} ",DTime);
				 	       if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
					 // Отменяем активный Sell
             	     if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) { 
						   var res = Trade.CloseMarketPosition(posGuidBuy);
                  	       if (res.IsSuccessful) posGuidBuy = Guid.Empty; }
						  }
//== SELLIMIT START ================================================================================						  
var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1, fr_all_Up+TP, 0, Stops.InPrice(fr_all_Up+SL,fr_all_Down), null, null, null);
				
				if (result.IsSuccessful) { posGuidSell = result.Position.Id;
					Print("09 SellLimit - {0} - {1} - {2}",fr_all_Up,fr_all_Down,DTime);} else 
					{   
						//var result2=Trade.Sell(Instrument.Id, 0.1);
						//if (result2.IsSuccessful) { posGuidSell = result2.Position.Id; 
					    Print("09 SellLimit  ОШИБКА - {0}",DTime);}     
			}
					
//====== UPDATE SELL =========================================================================================================================
			// Появился новый фрактал ВНИЗ  и назначен рабочим но есть отложеный или активный ВНИЗ ордер  
			// переносим цену открытия и ставим Stop - Низ/Верх противоположного фрактала
			if(frDown && fr_all_Down_L &&  posGuidBuy!=Guid.Empty) {  
				
				// Если есть уже отложеный Sell - меняем цену открытия
				if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) {
				var result = Trade.UpdatePendingPosition(posGuidBuy, 0.1, fr_all_Down,fr_all_Down+SL, fr_all_Up); 
				if (result.IsSuccessful) { posGuidBuy = result.Position.Id;  
				   Print("10 Sell UpdatePending - появился новый фр-рабочий - переносим цену и StopLoss - {0} - {1} - {2} ",fr_all_Down,fr_all_Up,DTime);}
				else { Print("10 Sell UpdatePending - ERROR - {0} ",DTime); }
				}
//====== CANCEL BUY  =========================================================================================================================
				// Если есть отложеный ордер Buy - отменяем его
				if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
				var res = Trade.CancelPendingPosition(posGuidSell); 
				 if (res.IsSuccessful) { posGuidSell=Guid.Empty;
				Print("10-1 Появился новый ВВНИЗ фрактал и подходит под рабочий - отменяем пред. BuyStop - CancelPending - {0} ",DTime); }
				}			

//====== CANCEL SELL  =========================================================================================================================			
			    // Если появился новый фрактал ВНИЗ и не подходит под рабочий и есть отрытый отложеный ордер - отменяем ордер Sell
			    if(frDown && !fr_all_Down_L  && posGuidBuy!=Guid.Empty) 
			       { if (Trade.GetPosition(posGuidBuy).State==PositionState.Pending) {
				     var res = Trade.CancelPendingPosition(posGuidBuy);
				     if (res.IsSuccessful) { posGuidBuy = Guid.Empty;
				         Print("11 Sell CancelPending - {0}",DTime);}}}
			   }
//=== CANCEL CLOSE BUY ==========================================================================================================			
			// Появился новый фрактал ВНИЗ и назначен рабочим и нет открытых Sell ордеров  (posGuidSell)- 
			 // ставим отложеный ордер SellStop и отменяем BuyStop - если открыт!
			if(frDown && fr_all_Down_L && posGuidBuy==Guid.Empty) 
			         {    // Закрываем SellLimit 
						   if (posGuidSell!=Guid.Empty)
							   { if(Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
							         {  var res = Trade.CancelPendingPosition(posGuidSell);   
							            if (res.IsSuccessful) { posGuidSell = Guid.Empty;  
									 		Print("12 Отменяем BuyStop бо появился раб-фрактал ВНИЗ  - {0}",DTime);}}
							      if(Trade.GetPosition(posGuidSell).State==PositionState.Active) 
								     {   var res = Trade.CloseMarketPosition(posGuidSell);
                  				         if (res.IsSuccessful) { posGuidSell = Guid.Empty; 
									        Print("12-a Отменяем Buy ордер бо появился раб-фрактал ВНИЗ - {0}",DTime);}} 
					           }
						  
//====  BUYLIMIT  =====================================================================================================
				        // Открываем BuyLimit
var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, fr_all_Down, 0, Stops.InPrice(fr_all_Down-SL,fr_all_Up), null, null, null);
						 if (result.IsSuccessful) { posGuidBuy=result.Position.Id;  
						         Print("13 BuyLimit! - {0} - {1} - {2}",fr_all_Down,fr_all_Up,DTime);} 
						 else {
						         //var result2 = Trade.Buy(Instrument.Id, 0.1);
							     //if (result2.IsSuccessful) { posGuidBuy = result2.Position.Id;
							     Print("14 BuyLimit  ERROR - {0}",DTime);}  	 
					  }
			
//====== BUY  UPDATE STOP =======================================================================================================
			// Появился новый фрактал ВВЕРХ и открыта позиция ВНИЗУ BuyLimit - переносим ТР
			  if(frUp && posGuidBuy!=Guid.Empty) { 
					if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active)  { 
				         var res3=Trade.UpdateMarketPosition(posGuidBuy, fr_all_Down-SL, frUpH, null); 
				         if (res3.IsSuccessful) { Print("15 SellStop - UpdatePending StopLoss когда Появился новый фрактал Up - {0} ",DTime);}
				       else  { var res2 = Trade.CloseMarketPosition(posGuidBuy); if (res2.IsSuccessful) posGuidBuy = Guid.Empty; }}
					 }
//===SELL UPDATE STOP ===========================================================================================================							 
					 // Появился новый фрактал ВНИЗ и ОТКРЫТА позиция ВВЕРХУ SellLimit - переносим стоп - Работает!
			if(frDown && posGuidSell!=Guid.Empty) { 
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) { 
				 var result2=Trade.UpdateMarketPosition(posGuidSell, fr_all_Up+SL,frDownL, null); 
				if (result2.IsSuccessful) { Print("14 BuyStop - !UpdatePending StopLoss когда Появился новый фрактал Down - {0}",DTime);}  
			  else { var res0 = Trade.CloseMarketPosition(posGuidSell); if (res0.IsSuccessful) posGuidSell = Guid.Empty;}}
			}
			
//=== КОРЕКЦИЯ ===========================================================================================================							 
			
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) 
			     {	posGuidBuy=Guid.Empty; Print("Buy - Закрыто по StopLoss (Корекция) - {0} ",DTime);}
				 
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) 
				{ posGuidSell=Guid.Empty;  Print("Sell - Закрыто по StopLoss (Корекция) - {0} ",DTime); }

//=== END ===========================================================================================================							 

	 }
				 
       protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
		

		
		private void HLine()
		{// Рисуем горизонтальную линию сигнала	Buy        
			Print("Сигнал на Buy - {0} -- {1}",frUp_Time,frUpH);			
            hline.Price = fr_all_Up;
			hline.Text="Buy - "+frUp_Time;}
		private void LLine()
		{// Рисуем горизонтальную линию сигнала	Sell		 			  
			 Print("Сигнал на Sell - {0} -- {1}",frDown_Time,frDownL);
                lline.Price = fr_all_Down;
			    lline.Text="Sell = "+fr_all_Down_Time;}
       private void Red() 
            { vlR.Time=Bars[Bars.Range.To-5].Time; vlR.Width=1;}
			      private void Blue() 
            {vlB.Time=Bars[Bars.Range.To-5].Time; vlB.Width=1;}
					private void Green() 
			{ vlG.Time=Bars[Bars.Range.To-5].Time;}	
				     private void Yellow() 
            {vlY.Time=Bars[Bars.Range.To-5].Time;}	
    }
}
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
    [TradeSystem("TC_Alligator+Fractal4")]   //copy of "TC_Alligator+Fractal3"
    public class Alligator1 : TradeSystem
    {
		private Guid _positionGuid=Guid.Empty;
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
		
		double BarH,BarL,BarC,StopL=0.2;
		bool aH=false;
		bool aL=false;
		// Fractal
		double frUpH = 0.0;   // Значение текущего верхнего Fractal
		double frUpL = 0.0;    // Значение Low - свечи с верхним фрактклом
        double frUpC = 0.0;
		double frDownH = 0.0;    // Значение High - свечи с нижним фракталом
		double frDownL = 0.0;  // Значение текущего нижнего Fractal
		double frDownC = 0.0;
		
		public DateTime frUp_Time; // Время текущего фрактала вверху
		public DateTime frDown_Time; // Время текущего фрактала внизу
		
    	bool fBuy=false; // Сигнал на покупку - есть фрактал со свечей выше Аллигатора
		bool fSell=false; // Сигнал на продажу - есть фрактал со свечей ниже Аллигатора
		bool pBuy=false;  // Сигнал - открыт ордер на покупку
		bool pSell=false; // Сигнал - открыт ордер на продажу
		bool Torg=false; //  Сигнал - Линии Alligator переплетены
		
        // 1. Фрактал выше/ниже зубов Alligator
		double fr_all_Up;     // Цена последней свечи с фракталом - полностью выше Аллигатора
		double fr_all_Down;   // Цена последней свечи с фракталом - полностью ниже Аллигатора
		bool fr_all_Down_L, fr_all_Up_L;
		bool frUp,frDown;
		public DateTime fr_all_Up_Time;  // Время последней свечи с фракталом - полностью выше Аллигатора
		public DateTime fr_all_Down_Time; // Время последней свечи с фракталом - полностью ниже Аллигатора
		public DateTime DTime;
				
		// Линии показывают АКТИВНЫЕ точки
		private VerticalLine vlineR;
		private VerticalLine vlineB;
		private HorizontalLine hline;
		private HorizontalLine lline;
		
		public PositionState psU=PositionState.NotActive;
		public PositionState psL=PositionState.NotActive;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
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
			// Значения текущего Бара
			BarH = Bars[Bars.Range.To-1].High;
			BarL = Bars[Bars.Range.To-1].Low;
			BarC = Bars[Bars.Range.To-1].Close;
			DTime = Bars[Bars.Range.To-1].Time;

			//  frUp frDown - Истина если появился НОВЫЙ фрактал Вверх/Вниз
			if (_frInd.TopSeries[Bars.Range.To-5]>0) frUp=true; else frUp=false; 
			if (_frInd.BottomSeries[Bars.Range.To-5]>0) frDown=true; else frDown=false;
			
			// Значения Alligator около фрактала
			aGuba5 = _allInd.LipsSeries[Bars.Range.To-5];
            aZub5 = _allInd.TeethSeries[Bars.Range.To-5];
			aChelust5 =  _allInd.JawsSeries[Bars.Range.To-5];
			
			// Значене Alligator у цены
			aGuba = _allInd.LipsSeries[Bars.Range.To];      //З
            aZub = _allInd.TeethSeries[Bars.Range.To];      //К
			aChelust =  _allInd.JawsSeries[Bars.Range.To];  //С
			
// =================================================================================================================================				 
// 1. Линии Алигатора переплетены - Торгуем
		     // Правильно ЗКС или СКЗ - если наоборот то  линии переплелись - Торг 
			if ( !((aGuba>aZub && aZub>aChelust) || (aChelust>aZub && aZub>aGuba)) ) { Torg=true;  Print("Torg - Линии Алигатора переплелись! ",DTime);}
// =================================================================================================================================				 
			//if(posGuidBuy!=Guid.Empty) Print("State={0} -- {1}",Trade.GetPosition(posGuidBuy).State,Bars[Bars.Range.To-1].Time); else Print("State=Empty! -- {0}",Bars[Bars.Range.To-1].Time);
			//if(posGuidSell!=Guid.Empty) Print("State={0} -- {1}",Trade.GetPosition(posGuidSell).State,Bars[Bars.Range.To-1].Time); else Print("State=Empty! -- {0}",Bars[Bars.Range.To-1].Time);
     					
			// Срабатывает - когда появился новый фрактал - frUp frDown=true!
			// Запоминаем значения Свечи бара-фрактала(frUpH) и время (frUp_Time)
			  if (frUp)    { 
				                frUpH=Bars[Bars.Range.To-5].High; 
				                frUpL=Bars[Bars.Range.To-5].Low;
				                frUpC=Bars[Bars.Range.To-5].Close;
				                frUp_Time = Bars[Bars.Range.To-5].Time;}
			  if (frDown) { 
				               frDownL=Bars[Bars.Range.To-5].Low; 
				               frDownH=Bars[Bars.Range.To-5].High; 
				               frDownC=Bars[Bars.Range.To-5].Close; 
				               frDown_Time = Bars[Bars.Range.To-5].Time;}
		  
 
		    // Появился новый фрактал и Свеча Fractalа ВЫШЕ Alligatora не касается Alligatorа 
			// низ Бар-Фрактала выше Alligator  - Назначаем рабочим (fr_all_Up) для Buy
			  // fr_all_Up_L - если есто рабочий фрактал Buy - true
			  if (frUp && frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5)   
			     { fr_all_Up=frUpH; fr_all_Up_Time=frUp_Time;  fr_all_Up_L=true;  Print("Buy Фрактал выше Алигатора - {0} ",Bars[Bars.Range.To-1].Time);}
			// Если появился новый фрактал  Buy и касается Алигатора - отменяем "рабочий Buy"
			  if ( frUp && !(frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5) ) { 
				  fr_all_Up_L=false; Print("Buy Фрактал касается Алигатора - {0} }",Bars[Bars.Range.To-1].Time); }  
			
			  
			// Появился новый фрактал и Свеча Fractalа НИЖЕ  Alligatora не касается Alligatorа 
			// низ Бар-Фрактала выше Alligator  - Назначаем рабочим (fr_all_Up) для Buy
			if (frDown && frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5) 
				{ fr_all_Down=frDownL;  fr_all_Down_Time=frDown_Time; fr_all_Down_L=true;  
					Print("Sell Фрактал ниже Алигатора - {0} ",Bars[Bars.Range.To-1].Time); } 
			// Если появился новый фрактал  Sell  и касается Алигатора - отменяем "рабочий Sell"	
			if (frDown && !(frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5)) { fr_all_Down_L=false; 
				    Print("Sell Фрактал касается Алигатора - {0} ",Bars[Bars.Range.To-1].Time); }	

//===============================================================================================================================
			//if( Trade.GetPosition(posGuidBuy).State == PositionState.Closed ) posGuidBuy=Guid.Empty;			
			
			// Появился новый фрактал и назначен рабочим но есть открытый ордер Buy 
			// меняем цену открытия и ставим Stop - Низ/Верх противоположного фрактала
			if (frUp && fr_all_Up_L &&  posGuidBuy!=Guid.Empty) {
			     if (Trade.GetPosition(posGuidBuy).State==PositionState.Pending)
			{ var result = Trade.UpdatePendingPosition(posGuidBuy, 0.1, fr_all_Up, frDownL-StopL, null); 
				if (result.IsSuccessful) { posGuidBuy = result.Position.Id; 
				Print("Buy UpdatePending - !появился новый фр-рабочий - переносим цену и StopLoss -  {0} ",DTime);}}}
			
			// Если появиля новый фрактал и не подходит под рабочий и есть отрытый отложеный ордер - отменяем ордер Buy
			//Print("UP Buy ={0} - {1} - {2} = {3} ",frUp,fr_all_Up_L,posGuidBuy,Bars[Bars.Range.To-1].Time);
			if(frUp && !fr_all_Up_L && posGuidBuy!=Guid.Empty) { 
				if (Trade.GetPosition(posGuidBuy).State==PositionState.Pending) {
				var res = Trade.CancelPendingPosition(posGuidBuy); 
				 if (res.IsSuccessful) posGuidBuy=Guid.Empty;
				Print("Buy CancelPending - {0} ",DTime);
                              }}
			
			// Алигатор переплетен и появился новый фрактал и назначен рабочим - ставим отложеный ордер на Buy
			//  и при этом нет открытых Buy ордеров  (posGuidBuy)
			if(Torg && frUp && fr_all_Up_L &&  posGuidBuy==Guid.Empty ) { 
			      // Закрываем SellStop
						  if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
							var res = Trade.CancelPendingPosition(posGuidSell);  Print("Sell Отменяем SellStop бо появился раб-фрактал на Buy - {0} ",DTime);
				 				if (res.IsSuccessful) posGuidSell = Guid.Empty;}
				// открываем BuyStop
				var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyStop, 0.1, fr_all_Up, 0, Stops.InPrice(frDownL-StopL), null, null, null);
				if (result.IsSuccessful) { posGuidBuy = result.Position.Id;  Print("BuyStop - {0}",DTime);} }
			
//===============================================================================================================================
			
			//if( Trade.GetPosition(posGuidSell).State == PositionState.Closed ) posGuidSell=Guid.Empty;		

		
			// Появился новый фрактал и назначен рабочим но есть открытый ордер Sell 
			// переносим цену открытия и ставим Stop - Низ/Верх противоположного фрактала
			if(Torg && frDown && fr_all_Down_L &&  posGuidSell!=Guid.Empty) {  
				if (Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
				var result = Trade.UpdatePendingPosition(posGuidSell, 0.1, fr_all_Down, frUpH+StopL, null); 
				if (result.IsSuccessful) { posGuidSell = result.Position.Id;  
				Print("Sell UpdatePending - !появился новый фр-рабочий - переносим цену и StopLoss - {0}  ",DTime);}}
			}			
			// Если появился новый фрактал ВНИЗ и не подходит под рабочий и есть отрытый отложеный ордер - отменяем ордер Sell
			if(frDown && !fr_all_Down_L  && posGuidSell!=Guid.Empty) 
			{ if (Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
				var res = Trade.CancelPendingPosition(posGuidSell);
				 if (res.IsSuccessful) posGuidSell = Guid.Empty;
				Print("Sell CancelPending - {0} ",DTime);}
			}
			
			// Алигатор переплетен и появился новый фрактал ВНИЗ и назначен рабочим и нет открытых Sell ордеров  (posGuidSell)- 
			 // ставим отложеный ордер SellStop и отменяем BuyStop - если открыт!
			if(Torg && frDown && fr_all_Down_L && posGuidSell==Guid.Empty) 
			         {    // Закрываем BuyStop
						  if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) {
							var res = Trade.CancelPendingPosition(posGuidBuy);   Print("Bay Отменяем BayStop бо появился раб-фрактал на Sell - {0} ",DTime);
				 				if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
				          // Открываем SellStop
						 var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellStop, 0.1, fr_all_Down, 0, Stops.InPrice(frUpH+StopL), null, null, null);
						 if (result.IsSuccessful) { posGuidSell=result.Position.Id;  
						 Print("SellStop - {0} ",DTime);} }
				
			
//===============================================================================================================================
				// Отменяем торг если вошли в позицию - Следующий вход - когда снова переплетется Алигатор 
			if (Torg && posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State ==PositionState.Active)  { Torg=false;  }
			if (Torg && posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) { Torg=false; } 
			
//===============================================================================================================================
			// Появился новый фрактал ВНИЗ и открыта позиция Buy - переосим стоп - Работает!
			if (frDown &&  Trade.GetPosition(posGuidBuy).State==PositionState.Active) { 
				 Trade.UpdateMarketPosition(posGuidBuy, frDownL-StopL, null, "Added stoploss and takeprofit"); 
			       Print("Buy - !UpdatePending StopLoss когда Появился новый фрактал Down - {0}-{1}",DTime,psU);}   
			// Появился новый фрактал ВВЕРХ и открыта позиция Sell - переосим стоп 
			if (frUp &&  Trade.GetPosition(posGuidSell).State==PositionState.Active)  { 
				 Trade.UpdateMarketPosition(posGuidSell, frUpH+StopL, null, "Added stoploss and takeprofit"); 
			 Print("Sell - UpdatePending StopLoss когда Появился новый фрактал Up - {0} - {1}",DTime,psL);
			}
 
//===============================================================================================================================
							
			//if  (Trade.GetPosition(posGuidBuy).State.ToString()=="Active" && Trade.GetPosition(posGuidBuy).Pips>300) { Trade.CloseMarketPosition(posGuidBuy);  pBuy=false; Torg=false;}
			//if  (Trade.GetPosition(posGuidSell).State.ToString()=="Active" && Trade.GetPosition(posGuidSell).Pips>300) { Trade.CloseMarketPosition(posGuidSell);  pSell=false; Torg=false;}
			if (posGuidSell!=Guid.Empty)  {
			    var posS = Trade.GetPosition(posGuidSell);
                var stateS = posS.State;
				if(stateS.ToString()=="Active") 
				{
					 var pos = Trade.GetPosition(posGuidSell);
                     var pips = pos.Pips;
					//Print("Pips={0} ",pips,Bars[Bars.Range.To-1].Time);
				}
			}
			
			
			if (posGuidBuy!=Guid.Empty)  {
          		var posB = Trade.GetPosition(posGuidBuy);
          		var stateB = posB.State;
				if(stateB.ToString()=="Active") 
				{
					 var pos = Trade.GetPosition(posGuidBuy);
                     var pips = pos.Pips;
					//Print("Pips={0} ",pips,Bars[Bars.Range.To-1].Time);
				}
			 }
			//if  (Trade.GetPosition(posGuidBuy).State.ToString()=="Active") Print("Pips={0}============================================================",Trade.GetPosition(posGuidSell).Pips);
// =================================================================================================================================
		    // Fractal Close ВЫШЕ Alligatora  
			// Закрытие Бар-Фрактала выше Alligator  - Назначаем рабочим (fr_all_Up) для Buy
//			  if ( frUpC>aZub5 && _frInd.TopSeries[Bars.Range.To-5]>0)   
//			     { fr_all_Up=frUpH; fr_all_Up_Time=frUp_Time; fBuy=true; HLine(); }  
 			// Fractal НИЖЕ Alligatora И свеча не касается Alligator - 
			// Закрытие Бар-Фрактала Ниже Alligator  - Назначаем рабочим (fr_all_Down) для Sell
//			if ( frDownC<aZub5 && _frInd.BottomSeries[Bars.Range.To-5]>0) 
//			     { fr_all_Down=frDownL; fr_all_Down_Time=frDown_Time; fSell=true; LLine(); } 
// =================================================================================================================================				 				 
			// Если есть фрактал, есть сигнал на продажу и разрешение на торг 
		    //НО! свеча нового фрактала пересекает линию Аллигатора - отменяем позицию.	
			//	if ( _frInd.TopSeries[Bars.Range.To-5]>0) 
			//		Print("Пересечение  Алигатора фракталом {0} {1}",( frUpL<aGuba5 || frUpL<aChelust5 || frUpL<aZub5 ),Bars[Bars.Range.To-5].Time);
				 
//			if (Torg && fBuy && ( frUpL<aGuba5 || frUpL<aChelust5 || frUpL<aZub5 ) && _frInd.TopSeries[Bars.Range.To-5]>0) 
//			      { fBuy=false; Print("Отмена - Buy {0} - на Баре {1}",fr_all_Up_Time,Bars[Bars.Range.To-1].Time); }
				 // Если есть фрактал, есть сигнал на продажу и разрешение на торг 
		        //НО! свеча нового фрактала пересекает линию Аллигатора - отменяем позицию.	 
				  //if (_frInd.TopSeries[Bars.Range.To-5]>0) Print("DOWN - {0} {1} {2} - {3}",(Torg,fSell frDownH>aGuba5 || frDownH>aChelust5 || frDownH>aZub5 ), Bars[Bars.Range.To-5].Time);
//            if (Torg && fSell && ( frDownH>aGuba5 || frDownH>aChelust5 || frDownH>aZub5 ) && _frInd.BottomSeries[Bars.Range.To-5]>0) 
//			               fSell=false; // Print("Отмена -  Sell {0} - на Баре -{1}",fr_all_Down_Time,Bars[Bars.Range.To-1].Time); }	
			 
				 
// =================================================================================================================================				      
					 
				 // Если есть сигнал на покупку(fBuy) и не открыт дугой ордер Buy
				 // и текущий бар пересек HIGH РАБОЧЕГО Fractala  - Открываем ордер Buy 
//				 if(Time_Start && Torg && fBuy && !pBuy && fr_all_Up<Bars[Bars.Range.To-1].High && _positionGuid==Guid.Empty) 
//				           { Buy(); pBuy=true; fBuy=false; Torg=false; 
//						   Print("fr_all_Up-{0}   Bars[Bars.Range.To-1].High-{1}",fr_all_Up,Bars[Bars.Range.To-1].High);
//						   }
				 
                 // Если есть сигнал на продажу(fBuy) и не открыт дугой ордер Sell
				 // и текущий бар пересек LOW РАБОЧЕГО Fractala  - Открываем ордер Sell
//				  if(Time_Start && Torg && fSell && !pSell && fr_all_Down>Bars[Bars.Range.To-1].Low  && _positionGuid==Guid.Empty)  
//				           { Sell(); pSell=true;  fSell=false; Torg=false; 
//						    Print("fr_all_Down-{0}   Bars[Bars.Range.To-1].Low-{1}",fr_all_Down,Bars[Bars.Range.To-1].Low);
//						   }
// =================================================================================================================================				 
				 // Закрываем ордер на покупку когда свеча каснется Зубов Alligator		   
				// if(pBuy && Bars[Bars.Range.To-1].Close<aGuba && posGuidBuy!=Guid.Empty) { Trade.CloseMarketPosition(posGuidBuy);  pBuy=false; Torg=false;}
			//	if(Trade.GetPosition(posGuidBuy).Pips>100 && posGuidBuy!=Guid.Empty) { 
			//		var res = Trade.CloseMarketPosition(posGuidBuy);  Print("Buy - Закрываем позицию - {0} ",DTime);
              //                if (res.IsSuccessful) posGuidBuy=Guid.Empty; Torg=false;}
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) {	posGuidBuy=Guid.Empty; Print("Buy - Закрыто по StopLoss (Корекция) - {0} ",DTime);}
			
			
			    //if(pBuy && Bars[Bars.Range.To-1].Close<aChelust && _positionGuid!=Guid.Empty) { ClosePosition();  pBuy=false; Torg=false;}
	//			 if(pBuy && aoUp1>0 && aoDown>0 && _positionGuid!=Guid.Empty) { ClosePosition();  pBuy=false; Torg=false;}
				 
				 // Закрываем ордер на продажу когда свеча каснется Зубов	
				// if(pSell && Bars[Bars.Range.To-1].Close>aGuba && posGuidSell!=Guid.Empty) { Trade.CloseMarketPosition(posGuidSell);  pSell=false; Torg=false;}
		//		if(Trade.GetPosition(posGuidSell).Pips>100 && posGuidSell!=Guid.Empty) { 
		//			var res = Trade.CloseMarketPosition(posGuidSell);   Print("Sell - Закрываем позицию - {0} ",DTime);
          //                    if (res.IsSuccessful) posGuidSell=Guid.Empty;  Torg=false; }
				if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) {	posGuidSell=Guid.Empty;  Print("Sell - Закрыто по StopLoss (Корекция) - {0} ",DTime); }
				//if(pSell && Bars[Bars.Range.To-1].Close>aChelust && _positionGuid!=Guid.Empty) { ClosePosition();  pSell=false; Torg=false;}
	//			if(pSell  && aoUp<0/ && aoDown1<0 && _positionGuid!=Guid.Empty) { ClosePosition();  pSell=false; Torg=false;}
				
				 
	 }
				 
       protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
		
		private void Buy()
		{
			if (_positionGuid!=Guid.Empty) return; 
			
			var result=Trade.Buy(Instrument.Id, 0.1); 		
			if (result.IsSuccessful) 
			{
				_positionGuid=result.Position.Id;
				
			}
    	}
		
		private void Sell()
		{
			if (_positionGuid!=Guid.Empty) return; 
			
			var result=Trade.Sell(Instrument.Id, 0.1); 
			if (result.IsSuccessful) 
			{
				_positionGuid=result.Position.Id;
				
			}
    	}
		
		private void ClosePosition()
		{
			var result =Trade.CloseMarketPosition(_positionGuid);
			if (result.IsSuccessful) 
			{
                Print("LR_Position_closed_by_inverse_signal", result.Position.Number, result.Position.ClosePrice);	
				_positionGuid = Guid.Empty;			
			}
		}
		
		private void HLine()
		{
						// Рисуем горизонтальную линию сигнала	Buy        
			Tools.Remove(hline); 
			Print("Сигнал на Buy - {0} -- {1}",frUp_Time,frUpH);
			hline = Tools.Create<HorizontalLine>();
            hline.Price = fr_all_Up;
			hline.Text="Buy - "+frUp_Time;				 
				 
		}
		
		private void LLine()
		{
						// Рисуем горизонтальную линию сигнала	Sell		 			  
				Tools.Remove(lline);  
					 Print("Сигнал на Sell - {0} -- {1}",frDown_Time,frDownL);
			    lline = Tools.Create<HorizontalLine>();
                lline.Price = fr_all_Down;
			    lline.Text="Sell = "+fr_all_Down_Time;
		}
       			  private void Red() 
            {
		   var vl = Tools.Create<VerticalLine>();
               vl.Time=Bars[Bars.Range.To-1].Time;
			   vl.Color=Color.Red;
			}
			      private void Blue() 
            {
		   var vl = Tools.Create<VerticalLine>();
               vl.Time=Bars[Bars.Range.To-1].Time;
			   vl.Color=Color.Blue;
			}
					private void Green() 
			{
			var vl = Tools.Create<VerticalLine>();
                vl.Time=Bars[Bars.Range.To-1].Time;
			    vl.Color=Color.LightSeaGreen;
			}	
				     private void Yellow() 
            {
		   var vl = Tools.Create<VerticalLine>();
               vl.Time=Bars[Bars.Range.To-1].Time;
			   vl.Color=Color.Yellow;
			}	
    }
}
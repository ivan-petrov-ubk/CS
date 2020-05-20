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
//Периоды активности Аллигатора делятся на 4 следующих:
 //   Аллигатор просыпается - бары с разных сторон нулевой линии окрашены в разные цвета.
 //   Аллигатор ест - зеленые бары с обеих сторон нулевой линии.
 //   Аллигатор насыщается - во время "еды" с одной из сторон появляется красный бар.
 //   Аллигатор спит - бары с обеих сторон красного цвета.


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
    [TradeSystem("TC_Alligator+Fractal8")]     //copy of "TC_Alligator+Fractal7"
    public class Alligator1 : TradeSystem
    {
		public double SL=0.0003;
		private Guid _positionGuid=Guid.Empty;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
        // Simple parameter example
		public Alligator _allInd;
		public Fractals _frInd;
		private GatorOscillator _goInd;
		
		double aGuba5, aZub5, aChelust5;   // Челюсть Синяя
		double aGuba1, aZub1, aChelust1;
		double aGuba;  //  Губы Зеленая
		double aZub;    // Зубы  Красная
		double aChelust;   // Челюсть Синяя
		
		double BarH,BarL,BarC, StopL=0.2;
		bool aH=false;
		bool aL=false;
		// Fractal
		double frUpH = 0.0;   // Значение текущего верхнего Fractal
		double frUpH1 = 0.0;  
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
		bool frUp=false;
		bool frDown=true;
		bool allSpit;
		public DateTime fr_all_Up_Time;  // Время последней свечи с фракталом - полностью выше Аллигатора
		public DateTime fr_all_Down_Time; // Время последней свечи с фракталом - полностью ниже Аллигатора
		public DateTime DTime;
				
		// Линии показывают АКТИВНЫЕ точки
		private VerticalLine vlineR;
		private VerticalLine vlineB;
		private HorizontalLine hline;
		private HorizontalLine lline;
		double gNU,gND,gPU,gPD;
		
		public double frSU=0,frSD=0;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
			// Вставить индикатор Alligator
			_allInd = GetIndicator<Alligator>(Instrument.Id, Timeframe);
			// Вставить индикатор Fractals
		    _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			
			_goInd = GetIndicator<GatorOscillator>(Instrument.Id, Timeframe);

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
			//Print("Fractal!! - Top={0} Bottom={1}",_frInd.TopSeries[Bars.Range.To-5],_frInd.BottomSeries[Bars.Range.To-5]);
			frSU=_frInd.TopSeries[Bars.Range.To-5];
			frSD=_frInd.BottomSeries[Bars.Range.To-5];
			//Print("Fractal2!! - Top={0} Bottom={1} -- {2}",frSU,frSD,Bars[Bars.Range.To-5].Time);
			//if (Trade.GetPosition(posGuidSell).State==PositionState.Active) 
		//		Print("15 $$$$$$$$$$$$$$$$$$$$$$$$frSU>0={3} Top={4} frUpH={0} frUp_Time={1} -- {2}",frUpH,frUp_Time,Bars[Bars.Range.To-1],frSU,_frInd.TopSeries[Bars.Range.To-5]);
			
			
			
			if(frSU>0) { frUp=true; } else { frUp=false; }
			if(frSD>0) { frDown=true; } else { frDown=false; }
			//if (_frInd.TopSeries[Bars.Range.To-5]>0) frUp=true; else frUp=false; 
			//if (_frInd.BottomSeries[Bars.Range.To-5]>0) frDown=true; else frDown=false;
			
			// Значения Alligator около фрактала
			aGuba5 = _allInd.LipsSeries[Bars.Range.To-5];
            aZub5 = _allInd.TeethSeries[Bars.Range.To-5];
			aChelust5 =  _allInd.JawsSeries[Bars.Range.To-5];
			
			// Значене Alligator у цены
			aGuba = _allInd.LipsSeries[Bars.Range.To];      //З
            aZub = _allInd.TeethSeries[Bars.Range.To];      //К
			aChelust =  _allInd.JawsSeries[Bars.Range.To];  //С
			
			// Значения Gator Oscillator 
			gNU=_goInd.NegativeSeriesUp[Bars.Range.To-1];
			gND=_goInd.NegativeSeriesDown[Bars.Range.To-1];
			gPD=_goInd.PositiveSeriesDown[Bars.Range.To-1];
			gPU=_goInd.PositiveSeriesUp[Bars.Range.To-1];
			
// =================================================================================================================================				 
// 1. Alligator СПИТ - Торгуем
		     // Правильно ЗКС или СКЗ - если наоборот то  линии переплелись - Торг 
			if ( !((aGuba>aZub && aZub>aChelust) || (aChelust>aZub && aZub>aGuba)) ) { Torg=true;  
				//Print("#01 Torg - Линии Алигатора переплелись! ",DTime);
			}
			
			//Print("gNU={0},gND={1},gPU={2},gPD={3} -- {4}",gNU,gND,gPU,gPD,Bars[Bars.Range.To-1].Time);
			//if (gPU>0 && gNU<0) allEst=true; else allEst=false;
				//Print("Алигатор ЕСТ - PU={0} NU={1} - {2} ",gPU,gNU,Bars[Bars.Range.To-1].Time);
			if ((gPD>0 && gND<0) || (gPD>0 && gNU<0) || (gPU>0 && gND<0)) { allSpit=true; 
			//Print("#01 Аллигатор СПИТ! -- {0} ",DTime);
			} else { allSpit=false; } 
			
			
				//Print("Алигатор СПИТ - PD={0} ND={1} - {2} ",gPD,gND,Bars[Bars.Range.To-1].Time);
			//if ((gPD>0 && gNU<0) || (gPU>0 && gND<0)) 
				//Print("Алигатор ПРОСЫПАЕТСЯ - PD>0={0} NU<0={1} gPU>0={2} gND<0={3} - {4} ",gPD,gNU,gPU,gND,Bars[Bars.Range.To-1].Time);
// =================================================================================================================================				 
     		// Срабатывает - когда появился новый фрактал - frUp frDown=true!
			// Запоминаем значения Свечи бара-фрактала(frUpH) и время (frUp_Time)
			  if (frSU>0)    { 
				                frUpH=Bars[Bars.Range.To-5].High; 
				                frUpL=Bars[Bars.Range.To-5].Low;
				                frUpC=Bars[Bars.Range.To-5].Close;
				                frUp_Time = Bars[Bars.Range.To-5].Time;}
			  if (frSD>0) { 
				               frDownL=Bars[Bars.Range.To-5].Low; 
				               frDownH=Bars[Bars.Range.To-5].High; 
				               frDownC=Bars[Bars.Range.To-5].Close; 
				               frDown_Time = Bars[Bars.Range.To-5].Time;}
//===============================================================================================================================		  
		    // Появился новый фрактал и Свеча Fractalа ВЫШЕ Alligatora не касается Alligatorа 
			// низ Бар-Фрактала выше Alligator  - Назначаем рабочим (fr_all_Up) для Buy
			  // fr_all_Up_L - если появился рабочий фрактал Buy - true
			  if (frSU>0 && frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5)   
			     { fr_all_Up=frUpH; fr_all_Up_Time=frUp_Time;  fr_all_Up_L=true;  
					 Print("02 Фрактал ВВЕРХ и выше Алигатора - {0} ",Bars[Bars.Range.To-1].Time);}
			// Если появился новый фрактал  ВВЕРХ и касается Алигатора - отменяем "рабочий"
			  if ( frSU>0 && !(frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5) ) { fr_all_Up_L=false; 
				     Print("03 Фрактал ВВЕРХ но касается Алигатора - {0} }",Bars[Bars.Range.To-1].Time); }  
//===============================================================================================================================			
     		// Появился новый фрактал ВНИЗ  и Свеча Fractalа НИЖЕ  Alligatora не касается Alligatorа 
			// ВЕРХ Бар-Фрактала НИЖЕ Alligator  - Назначаем рабочим (fr_all_Down) для SellLimit
			if (frSD>0 && frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5) 
				{ fr_all_Down=frDownL;  fr_all_Down_Time=frDown_Time; fr_all_Down_L=true;  
					Print("04 Фрактал НИЖЕ Алигатора - {0} ",Bars[Bars.Range.To-1].Time); } 
			// Если появился новый фрактал  ВНИЗ  и касается Алигатора - отменяем "рабочий"	
			if (frSD>0 && !(frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5)) { fr_all_Down_L=false; 
				    Print("05 Фрактал ВНИЗ и касается Алигатора - {0} ",Bars[Bars.Range.To-1].Time); }	
//===============================================================================================================================
			// Появился новый фрактал ВВЕРХ и назначен рабочим и есть отложеный ордер  
			// меняем цену открытия и ставим TP - Низ/Верх противоположного фрактала
			if (frSU>0 && fr_all_Up_L &&  posGuidBuy!=Guid.Empty) {
			     if (Trade.GetPosition(posGuidBuy).State==PositionState.Pending)
					       // Ордер SellLimit-ВНИЗ - Цена открытия=High раб фрактала, SL=200p TP=Low противопложного фрактала
			{ var result = Trade.UpdatePendingPosition(posGuidBuy, 0.1, fr_all_Up,frDownL, null); 
				if (result.IsSuccessful) { posGuidBuy = result.Position.Id; 
				Print("06 UpdatePending - !появился новый фр-рабочий - переносим цену и StopLoss TP -  {0} ",DTime);}}}
			
			// Если появился новый фрактал и не подходит под рабочий и есть отрытый отложеный ордер - отменяем ордер Buy
			if(frSU>0 && !fr_all_Up_L && posGuidBuy!=Guid.Empty) { 
				if (Trade.GetPosition(posGuidBuy).State==PositionState.Pending) {
				var res = Trade.CancelPendingPosition(posGuidBuy); 
				 if (res.IsSuccessful) posGuidBuy=Guid.Empty;
				Print("07 Появился новый ВВЕРХ фрактал и не подходит под рабочий - отменяем пред. BuyStop - CancelPending - {0} ",DTime);
                              }}
			
			// Алигатор СПИТ и появился новый фрактал и назначен рабочим и нет отложек- ставим отложеный ордер 
			//  и при этом нет открытых ордеров  (posGuidBuy)
			if(allSpit && frSU>0 && fr_all_Up_L &&  posGuidBuy==Guid.Empty ) { 
	// Закрываем SellStop
						  if (posGuidSell!=Guid.Empty){ 
	if(Trade.GetPosition(posGuidSell).State==PositionState.Pending) { var res = Trade.CancelPendingPosition(posGuidSell);   
							  Print("12 Отменяем SellStop бо появился раб-фрактал  - {0} ",DTime);
				 				if (res.IsSuccessful) posGuidSell = Guid.Empty;}
	if(Trade.GetPosition(posGuidSell).State==PositionState.Active) { var res = Trade.CloseMarketPosition(posGuidSell);
                  					 if (res.IsSuccessful) posGuidSell = Guid.Empty; }
						  }
	// открываем BuyStop - Цена=High раб. ВВЕРХ фрактала SL=200p TP=Низ против. фрактала
				var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyStop, 0.1, fr_all_Up, 0, Stops.InPrice(frDownL-SL,null), null, null, null);
				if (result.IsSuccessful) { posGuidBuy = result.Position.Id; 
					Print("09 BuyStop - {0}",DTime);} else 
					{
						//var result2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, fr_all_Up, -1, Stops.InPrice(frDownL,null), null, null);
				           var result2=Trade.Buy(Instrument.Id, 0.1);
						if (result2.IsSuccessful) { posGuidBuy = result2.Position.Id; 
					Print("09 Buy ОТКРЫТО!! - {0}",DTime);} 
						}  }
			
//===============================================================================================================================
			// Появился новый фрактал ВНИЗ  и назначен рабочим но есть отложеный ВНИЗУ ордер  
			// переносим цену открытия и ставим Stop - Низ/Верх противоположного фрактала
			if(allSpit && frSD>0 && fr_all_Down_L &&  posGuidSell!=Guid.Empty) {  
				if (Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
				var result = Trade.UpdatePendingPosition(posGuidSell, 0.1, fr_all_Down,frUpH+SL, null); 
				if (result.IsSuccessful) { posGuidSell = result.Position.Id;  
				Print("10 Sell UpdatePending - !появился новый фр-рабочий - переносим цену и StopLoss - {0}  ",DTime);}}
			}			
			// Если появился новый фрактал ВНИЗ и не подходит под рабочий и есть отрытый отложеный ордер - отменяем ордер Sell
			if(frSD>0 && !fr_all_Down_L  && posGuidSell!=Guid.Empty) 
			{ if (Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
				var res = Trade.CancelPendingPosition(posGuidSell);
				 if (res.IsSuccessful) posGuidSell = Guid.Empty;
				Print("11 Sell CancelPending - {0} ",DTime);}
			}
			
			// Алигатор переплетен и появился новый фрактал ВНИЗ и назначен рабочим и нет открытых Sell ордеров  (posGuidSell)- 
			 // ставим отложеный ордер SellStop и отменяем BuyStop - если открыт!
			if(allSpit && frSD>0 && fr_all_Down_L && posGuidSell==Guid.Empty) 
			         {    // Закрываем BuyStop
						  if (posGuidBuy!=Guid.Empty){ 
							  if(Trade.GetPosition(posGuidBuy).State==PositionState.Pending) {
							var res = Trade.CancelPendingPosition(posGuidBuy);   
							  Print("12 Отменяем BuyStop бо появился раб-фрактал  - {0} ",DTime);
				 				if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
							  if(Trade.GetPosition(posGuidBuy).State==PositionState.Active) {
							        var res = Trade.CloseMarketPosition(posGuidBuy);
                  					 if (res.IsSuccessful) posGuidBuy = Guid.Empty;
							  } }
				          // Открываем SellStop
						 var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellStop, 0.1, fr_all_Down, 0, Stops.InPrice(frUpH+SL,null), null, null, null);
						 if (result.IsSuccessful) { posGuidSell=result.Position.Id;  
						 Print("13 SellStop - {0} ",DTime);} else {
						//var result2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, fr_all_Up, -1, Stops.InPrice(frDownL,null), null, null);
				                  var result2 = Trade.Sell(Instrument.Id, 0.1);
							 if (result2.IsSuccessful) { posGuidBuy = result2.Position.Id; 
					Print("09 Sell ОТКРЫТО!! - {0}",DTime);} 
						} }
				
			
//===============================================================================================================================
				// Отменяем торг если вошли в позицию - Следующий вход - когда снова переплетется Алигатор 
			//if (Torg && posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State ==PositionState.Active)  { Torg=false;  }
			//if (Torg && posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) { Torg=false; } 
			
//===============================================================================================================================
			// Появился новый фрактал ВВЕРХ и открыта позиция Sell - переосим стоп 
					 if(frUpH!=frUpH1) { frUpH1=frUpH;
			
			if (Trade.GetPosition(posGuidSell).State==PositionState.Active)  { 
				    var res3=Trade.UpdateMarketPosition(posGuidSell, frUpH1+SL, null, null); 
				 if (res3.IsSuccessful) { Print("15 SellStop - UpdatePending StopLoss когда Появился новый фрактал Up - {0} ",DTime);}
				 else  { var res2 = Trade.CloseMarketPosition(posGuidSell); if (res2.IsSuccessful) posGuidSell = Guid.Empty; }}
					 }
								 
					 // Появился новый фрактал ВНИЗ и ОТКРЫТА позиция Buy - переносим стоп - Работает!
			if (frSD>0 &&  Trade.GetPosition(posGuidBuy).State==PositionState.Active) { 
				 var result2=Trade.UpdateMarketPosition(posGuidBuy, frDownL-SL,null, null); 
				if (result2.IsSuccessful) { Print("14 BuyStop - !UpdatePending StopLoss когда Появился новый фрактал Down - {0}",DTime);}  
			  else { var res0 = Trade.CloseMarketPosition(posGuidBuy); if (res0.IsSuccessful) posGuidBuy = Guid.Empty;}}
			  

 
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
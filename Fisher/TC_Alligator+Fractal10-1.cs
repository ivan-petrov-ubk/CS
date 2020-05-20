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
    [TradeSystem("TC_Alligator+Fractal10")]      //copy of "TC_Alligator+Fractal8"
    public class Alligator1 : TradeSystem
    {
		public double SL=0.0003;
		public double TP=0.0003;
		
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
		
		double BarH,BarL,BarC; 
				
		bool aH=false;
		bool aL=false;
		// Fractal
		double frUpH = 0.0;   // Значение текущего верхнего Fractal
		double frUpH1 = 0.0;
		double frDownH1 = 0.0;  
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
		private VerticalLine vlR,vlB,vlG,vlY;
		private HorizontalLine hline;
		private HorizontalLine lline;
		
		double gNU,gND,gPU,gPD;
		
		public double frSU=0,frSD=0;
		
		public ISeries<Bar> _barSeries,_barM15;
		private int _lastIndex = -1;
		private int _lastIndexM15 = -1;
		public FisherTransformOscillator _ftoInd,_ftoIndM15;
		public double sF=0,sF1=0,smF=0,smF1=0;
		public double sHF=0,sM15F=0;
		public Period periodM15;
		int ifrU=0,ifrD=0;
		bool allEstU=false,allEstD=false;
		
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
			
			//periodM15 = new Period(PeriodType.Minute, 15);
			//_barSeries = GetCustomSeries(Instrument.Id, Period.H1);
			//_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			//_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Period.H1); 
			//_ftoIndM15= GetIndicator<FisherTransformOscillator>(Instrument.Id, periodM15); 

        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
			 // Event occurs on every new quote
	 //if (_lastIndex < _barSeries.Range.To - 1) {
     	//	_lastIndex = _barSeries.Range.To - 1;
    	//	var closePrice = _barSeries[_lastIndex].Close;
	 	//	var timePrice = _barSeries[_lastIndex].Time;
	 	//	sF = _ftoInd.FisherSeries[_lastIndex];
	 	//	sF1 = _ftoInd.FisherSeries[_lastIndex-1];
	 	//	sHF=sF-sF1;  }

	 //if ((_barM15[_barM15.Range.To-1].High>0) && (_lastIndexM15 < _barM15.Range.To - 1)) {
     	//	_lastIndexM15 = _barM15.Range.To - 1;
     	//	var closePriceM15 = _barM15[_lastIndexM15].Close;
	 	//	var timePriceM15 = _barM15[_lastIndexM15].Time;
	 	//	smF = _ftoIndM15.FisherSeries[_lastIndexM15];
	 	//	smF1 = _ftoIndM15.FisherSeries[_lastIndexM15-1];
	 	//	sM15F=smF-smF1; }
					
        }
        
        protected override void NewBar()
        { 
// =================================================================================================================================				 
			if (Bars[Bars.Range.To-1].Time.Hour==23 && Bars[Bars.Range.To-1].Time.Minute==20 ) { Torg=true; Print("****************************** 6 год "); }
			if (Bars[Bars.Range.To-1].Time.Hour==06 && Bars[Bars.Range.To-1].Time.Minute==00) Torg=false;
			
			// Значения текущего Бара
			BarH = Bars[Bars.Range.To-1].High;
			BarL = Bars[Bars.Range.To-1].Low;
			BarC = Bars[Bars.Range.To-1].Close;
			DTime = Bars[Bars.Range.To-1].Time;

			//  frUp frDown - Истина если появился НОВЫЙ фрактал Вверх/Вниз
			//Print("Fractal!! - Top={0} Bottom={1}",_frInd.TopSeries[Bars.Range.To-5],_frInd.BottomSeries[Bars.Range.To-5]);
			frSU=_frInd.TopSeries[Bars.Range.To-5];
			frSD=_frInd.BottomSeries[Bars.Range.To-5];
					
			if(frSU>0) { frUp=true; } else { frUp=false; }
			if(frSD>0) { frDown=true; } else { frDown=false; }
		
			// Значения Alligator около фрактала
			aGuba5 = _allInd.LipsSeries[Bars.Range.To-5];
            aZub5 = _allInd.TeethSeries[Bars.Range.To-5];
			aChelust5 =  _allInd.JawsSeries[Bars.Range.To-5];
			
			// Значене Alligator у цены
			aGuba = _allInd.LipsSeries[Bars.Range.To];      //З
            aZub = _allInd.TeethSeries[Bars.Range.To];      //К
			aChelust =  _allInd.JawsSeries[Bars.Range.To];  //С

			// Значене Alligator у цены
			aGuba1 = _allInd.LipsSeries[Bars.Range.To-1];      //З
            aZub1 = _allInd.TeethSeries[Bars.Range.To-1];      //К
			aChelust1 =  _allInd.JawsSeries[Bars.Range.To-1];  //С
			
			
			// Значения Gator Oscillator 
			//gNU=_goInd.NegativeSeriesUp[Bars.Range.To-1];
			//gND=_goInd.NegativeSeriesDown[Bars.Range.To-1];
			//gPD=_goInd.PositiveSeriesDown[Bars.Range.To-1];
			//gPU=_goInd.PositiveSeriesUp[Bars.Range.To-1];
			
			
//      
			  if (aGuba>aGuba1 && aZub>aZub1 && aChelust>aChelust1) allEstU=true; else allEstU=false;
			  if (aGuba1>aGuba && aZub1>aZub && aChelust1>aChelust) allEstD=true; else allEstD=false;
// =================================================================================================================================				 
// 1. Alligator СПИТ - Торгуем
		     // Правильно ЗКС или СКЗ - если наоборот то  линии переплелись - Торг 
			//if ( !((aGuba>aZub && aZub>aChelust) || (aChelust>aZub && aZub>aGuba)) ) { Torg=true;  
				//Print("#01 Torg - Линии Алигатора переплелись! ",DTime);}
			
//  1-A. Определяем спит аллигатор или ест - по индикатору Gattor
			//Print("gNU={0},gND={1},gPU={2},gPD={3} -- {4}",gNU,gND,gPU,gPD,Bars[Bars.Range.To-1].Time);
			//if (gPU>0 && gNU<0) allEst=true; else allEst=false;
			
				//Print("Алигатор ЕСТ - PU={0} NU={1} - {2} ",gPU,gNU,Bars[Bars.Range.To-1].Time);
			//if ((gPD>0 && gND<0) || (gPD>0 && gNU<0) || (gPU>0 && gND<0)) { allSpit=true; 
			//Print("#01 Аллигатор СПИТ! -- {0} ",DTime);
			//} else { allSpit=false; } 
			
			
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
			  if (frSU>0) { 
				  if(frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5) { Red();  vlB.Time=Bars[Bars.Range.To-1000].Time;
			        fr_all_Up=frUpH; fr_all_Up_Time=frUp_Time;  fr_all_Up_L=true;  ifrU++; ifrD=0;
					 Print("02 Фрактал ВВЕРХ и выше Алигатора - {0} - {1} ",Bars[Bars.Range.To-1].Time,Torg);}
				else 
			// Если появился новый фрактал  ВВЕРХ и касается Алигатора - отменяем "рабочий"
			  //if ( frSU>0 && !(frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5) ) 
			  { fr_all_Up_L=false;   vlR.Time=Bars[Bars.Range.To-1000].Time;
				     Print("03 Фрактал ВВЕРХ но касается Алигатора - {0} - {1}}",Bars[Bars.Range.To-1].Time,Torg); 
			  if (Trade.GetPosition(posGuidBuy).State==PositionState.Pending) {
				var res = Trade.CancelPendingPosition(posGuidBuy); 
				 if (res.IsSuccessful) posGuidBuy=Guid.Empty;
				Print("02-1 Появился новый ВВЕРХ фрактал и НЕ подходит под рабочий - отменяем пред. BuyStop - CancelPending - {0} ",DTime);     }
			  }  }
//===============================================================================================================================			
     		// Появился новый фрактал ВНИЗ  и Свеча Fractalа НИЖЕ  Alligatora не касается Alligatorа 
			// ВЕРХ Бар-Фрактала НИЖЕ Alligator  - Назначаем рабочим (fr_all_Down) для SellLimit
			  
			if (frSD>0) {
				if(frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5) { Blue();  vlR.Time=Bars[Bars.Range.To-1000].Time;
					fr_all_Down=frDownL;  fr_all_Down_Time=frDown_Time; fr_all_Down_L=true;  ifrD++; ifrU=0;
					Print("04 Фрактал НИЖЕ Алигатора - {0} - {1}",Bars[Bars.Range.To-1].Time,Torg); } 
			else  
			// Если появился новый фрактал  ВНИЗ  и касается Алигатора - отменяем "рабочий"	
			//if (frSD>0 && !(frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5)) 
			{ fr_all_Down_L=false;   vlB.Time=Bars[Bars.Range.To-1000].Time;
				    Print("05 Фрактал ВНИЗ и касается Алигатора - {0} - {1}",Bars[Bars.Range.To-1].Time,Torg); 
			if (Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
				var res = Trade.CancelPendingPosition(posGuidSell);
				 if (res.IsSuccessful) posGuidSell = Guid.Empty;
				Print("02-2 появился новый фр-рабочий ВНИЗ  и не рабочий отменяем отложеный Sell CancelPending - {0}",DTime);}
			}	}
//===============================================================================================================================
			// Появился новый фрактал ВВЕРХ и назначен рабочим и есть отложеный ордер  
			// меняем цену открытия и ставим TP - Низ/Верх противоположного фрактала
			if (frSU>0 && fr_all_Up_L &&  posGuidBuy!=Guid.Empty) {
			     if (Trade.GetPosition(posGuidBuy).State==PositionState.Pending)
					       // Ордер SellLimit-ВНИЗ - Цена открытия=High раб фрактала
				{ var result = Trade.UpdatePendingPosition(posGuidBuy, 0.1, fr_all_Up,frDownL, null); 
				if (result.IsSuccessful) { posGuidBuy = result.Position.Id; 
				Print("06 UpdatePending - !появился новый фр-рабочий - переносим цену и StopLoss TP -  {0} - {1}",DTime,Torg);}}
			
			if (Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
				var res = Trade.CancelPendingPosition(posGuidSell);
				 if (res.IsSuccessful) posGuidSell = Guid.Empty;
				Print("06-2 появился новый фр-рабочий ВВЕРХ и рабочий отменяем отложеный Sell CancelPending - {0}",DTime);}
			
			}
			
			// Если появился новый фрактал и не подходит под рабочий и есть отрытый отложеный ордер - отменяем ордер Buy
			if(frSU>0 && !fr_all_Up_L && posGuidBuy!=Guid.Empty) { 
				if (Trade.GetPosition(posGuidBuy).State==PositionState.Pending) {
				var res = Trade.CancelPendingPosition(posGuidBuy); 
				 if (res.IsSuccessful) posGuidBuy=Guid.Empty;
				Print("07 Появился новый ВВЕРХ фрактал и не подходит под рабочий - отменяем пред. BuyStop - CancelPending - {0} ",DTime);
                              }}
			
			// Алигатор СПИТ и появился новый фрактал и назначен рабочим и нет отложек- ставим отложеный ордер 
			//  и при этом нет открытых ордеров  (posGuidBuy)
			if(ifrU>3  && frSU>0 && fr_all_Up_L &&  posGuidBuy==Guid.Empty ) { // Print("BUY - sHF={0} - sM15F={1}",sHF, sM15F);
	// Закрываем SellStop
						  if (posGuidSell!=Guid.Empty){ 
	if(Trade.GetPosition(posGuidSell).State==PositionState.Pending) { var res = Trade.CancelPendingPosition(posGuidSell);   
							  Print("12 Отменяем SellStop бо появился раб-фрактал  - {0} ",DTime);
				 				if (res.IsSuccessful) posGuidSell = Guid.Empty;}
	if(Trade.GetPosition(posGuidSell).State==PositionState.Active) { var res = Trade.CloseMarketPosition(posGuidSell);
                  					 if (res.IsSuccessful) posGuidSell = Guid.Empty; }
						  }
//=============================----------------------------------------=====================================================						  
	// $$== ТОРГУЕМ!!!! ==$$  открываем BuyStop - Цена=High раб. ВВЕРХ фрактала SL=200p TP=Низ против. фрактала
				var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyStop, 0.1, fr_all_Up+TP, 0, Stops.InPrice(frDownL,null), null, null, null);
				if (result.IsSuccessful) { posGuidBuy = result.Position.Id;
					Print("09 BuyStop - {0} - {1}",DTime,Torg);} else 
					{
						//var result2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, fr_all_Up, -1, Stops.InPrice(frDownL,null), null, null);
				           var result2=Trade.Buy(Instrument.Id, 0.1);
						if (result2.IsSuccessful) { posGuidBuy = result2.Position.Id; 
					Print("09 Buy ОТКРЫТО!! - {0}",DTime);} 
						}  }
			
		
//===============================================================================================================================
			// Появился новый фрактал ВНИЗ  и назначен рабочим но есть отложеный ВНИЗУ ордер  
			// переносим цену открытия и ставим Stop - Низ/Верх противоположного фрактала
			if(frSD>0 && fr_all_Down_L &&  posGuidSell!=Guid.Empty) {  
				if (Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
				var result = Trade.UpdatePendingPosition(posGuidSell, 0.1, fr_all_Down,frUpH+SL, null); 
				if (result.IsSuccessful) { posGuidSell = result.Position.Id;  
				Print("10 Sell UpdatePending - !появился новый фр-рабочий - переносим цену и StopLoss - {0} ",DTime);}}
				
				if (Trade.GetPosition(posGuidBuy).State==PositionState.Pending) {
				var res = Trade.CancelPendingPosition(posGuidBuy); 
				 if (res.IsSuccessful) posGuidBuy=Guid.Empty;
				Print("10-1 Появился новый ВВНИЗ фрактал и подходит под рабочий - отменяем пред. BuyStop - CancelPending - {0} ",DTime);     }
				
			}			
			// Если появился новый фрактал ВНИЗ и не подходит под рабочий и есть отрытый отложеный ордер - отменяем ордер Sell
			if(frSD>0 && !fr_all_Down_L  && posGuidSell!=Guid.Empty) 
			{ if (Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
				var res = Trade.CancelPendingPosition(posGuidSell);
				 if (res.IsSuccessful) posGuidSell = Guid.Empty;
				Print("11 Sell CancelPending - {0}",DTime);}
			}
			
			// Алигатор переплетен и появился новый фрактал ВНИЗ и назначен рабочим и нет открытых Sell ордеров  (posGuidSell)- 
			 // ставим отложеный ордер SellStop и отменяем BuyStop - если открыт!
			if(ifrD>3  && frSD>0 && fr_all_Down_L && posGuidSell==Guid.Empty) 
			         {    // Закрываем BuyStop 
						 //Print("SELL - sHF={0} - sM15F={1}",sHF, sM15F);
						  if (posGuidBuy!=Guid.Empty){ 
							  if(Trade.GetPosition(posGuidBuy).State==PositionState.Pending) {
							var res = Trade.CancelPendingPosition(posGuidBuy);   
							  Print("12 Отменяем BuyStop бо появился раб-фрактал  - {0}",DTime);
				 				if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
							  if(Trade.GetPosition(posGuidBuy).State==PositionState.Active) {
							        var res = Trade.CloseMarketPosition(posGuidBuy);
                  					 if (res.IsSuccessful) posGuidBuy = Guid.Empty;
							  } }
						  
//==================================================================------------------------------------=======================================
				          // Открываем SellStop
						 var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellStop, 0.1, fr_all_Down-TP, 0, Stops.InPrice(frUpH,null), null, null, null);
						 if (result.IsSuccessful) { posGuidSell=result.Position.Id;  
						 Print("13 SellStop - {0} -----------  ",DTime);} else {
						//var result2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, fr_all_Up, -1, Stops.InPrice(frDownL,null), null, null);
				                  var result2 = Trade.Sell(Instrument.Id, 0.1);
							 if (result2.IsSuccessful) { posGuidBuy = result2.Position.Id;
					Print("14 Sell ОТКРЫТО!! - {0} -----------",DTime);} 
						} }
		
//===============================================================================================================================
				// Отменяем торг если вошли в позицию - Следующий вход - когда снова переплетется Алигатор 
			//if (Torg && posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State ==PositionState.Active)  { Torg=false;  }
			//if (Torg && posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) { Torg=false; } 
			
//===============================================================================================================================
			// Появился новый фрактал ВВЕРХ и открыта позиция Sell - переосим стоп 
					 if(frUpH!=frUpH1) { frUpH1=frUpH;
			
			if (Trade.GetPosition(posGuidSell).State==PositionState.Active)  { 
				    var res3=Trade.UpdateMarketPosition(posGuidSell, frUpH, null, null); 
				 if (res3.IsSuccessful) { Print("15 SellStop - UpdatePending StopLoss когда Появился новый фрактал Up - {0} ",DTime);}
				 else  { var res2 = Trade.CloseMarketPosition(posGuidSell); if (res2.IsSuccessful) posGuidSell = Guid.Empty; }}
					 }
								 
					 // Появился новый фрактал ВНИЗ и ОТКРЫТА позиция Buy - переносим стоп - Работает!
			if(frDownH!=frDownH1) { frDownH1=frDownH;
			if (Trade.GetPosition(posGuidBuy).State==PositionState.Active) { 
				 var result2=Trade.UpdateMarketPosition(posGuidBuy, frDownL,null, null); 
				if (result2.IsSuccessful) { Print("14 BuyStop - !UpdatePending StopLoss когда Появился новый фрактал Down - {0}",DTime);}  
			  else { var res0 = Trade.CloseMarketPosition(posGuidBuy); if (res0.IsSuccessful) posGuidBuy = Guid.Empty;}}
			}

 
//===============================================================================================================================
							
			//if  (Trade.GetPosition(posGuidBuy).State.ToString()=="Active" && Trade.GetPosition(posGuidBuy).Pips>300) { Trade.CloseMarketPosition(posGuidBuy);  pBuy=false; Torg=false;}
			//if  (Trade.GetPosition(posGuidSell).State.ToString()=="Active" && Trade.GetPosition(posGuidSell).Pips>300) { Trade.CloseMarketPosition(posGuidSell);  pSell=false; Torg=false;}
			if (posGuidSell!=Guid.Empty)  {
			    var posS = Trade.GetPosition(posGuidSell);
                var stateS = posS.State;
				if(stateS==PositionState.Active) 
				{
					 var pos = Trade.GetPosition(posGuidSell);
                     var pips = pos.Pips;
					//Print("Pips={0} ",pips,Bars[Bars.Range.To-1].Time);
				}
			}
			
			
			if (posGuidBuy!=Guid.Empty)  {
          		var posB = Trade.GetPosition(posGuidBuy);
          		var stateB = posB.State;
				if(stateB==PositionState.Active) 
				{
					 var pos = Trade.GetPosition(posGuidBuy);
                     var pips = pos.Pips;
					//Print("Pips={0} ",pips,Bars[Bars.Range.To-1].Time);
				}
			 }
// =================================================================================================================================				 
				 // Закрываем ордер на покупку когда свеча каснется Зубов Alligator		   
			//if(Bars[Bars.Range.To-1].Close<aChelust && posGuidBuy!=Guid.Empty) 
			//	{ var res4 = Trade.CloseMarketPosition(posGuidBuy);  if (res4.IsSuccessful) posGuidBuy=Guid.Empty; } 
			
			//if(Trade.GetPosition(posGuidBuy).Pips<-150 && posGuidBuy!=Guid.Empty)  
			//	{ var res = Trade.CloseMarketPosition(posGuidBuy);  Print("Buy - Закрываем позицию - {0} ",DTime);
              //                if (res.IsSuccessful) posGuidBuy=Guid.Empty; }
			
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) 
			     {	posGuidBuy=Guid.Empty; Print("Buy - Закрыто по StopLoss (Корекция) - {0} ",DTime);}
			
			
				 
				 // Закрываем ордер на продажу когда свеча каснется Чeлюсти (Blue)
			//	if(Bars[Bars.Range.To-1].Close>aChelust && posGuidSell!=Guid.Empty) 
			//	     { var res5 = Trade.CloseMarketPosition(posGuidSell);   if (res5.IsSuccessful) posGuidBuy=Guid.Empty;}
				
				
				//if(Trade.GetPosition(posGuidSell).Pips<-150 && posGuidSell!=Guid.Empty) { 
				//	var res = Trade.CloseMarketPosition(posGuidSell);   Print("Sell - Закрываем позицию - {0} ",DTime);
                  //            if (res.IsSuccessful) posGuidSell=Guid.Empty; }
		
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) 
						{ posGuidSell=Guid.Empty;  Print("Sell - Закрыто по StopLoss (Корекция) - {0} ",DTime); }
			
				
				 
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
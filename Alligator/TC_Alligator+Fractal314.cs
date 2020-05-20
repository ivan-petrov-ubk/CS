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
    [TradeSystem("TC_Alligator+Fractal14")]       //copy of "TC_Alligator+Fractal10"
    public class Alligator1 : TradeSystem
    {
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
			// Набор линий для визуального контроля стратегии
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
// =================================================================================================================================				 
			// Определение времени торговли ОТ и ДО
			if (Bars[Bars.Range.To-1].Time.Hour==5  && Bars[Bars.Range.To-1].Time.Minute==00) Torg=true; 
			if (Bars[Bars.Range.To-1].Time.Hour==10 && Bars[Bars.Range.To-1].Time.Minute==00) Torg=false;
			
			// Значения текущего Бара
			BarH = Bars[Bars.Range.To-1].High;
			BarL = Bars[Bars.Range.To-1].Low;
			BarC = Bars[Bars.Range.To-1].Close;
			DTime = Bars[Bars.Range.To-1].Time;

			//  frUp frDown - Истина если появился НОВЫЙ фрактал Вверх/Вниз
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
			
			  if (aGuba>aGuba1 && aZub>aZub1 && aChelust>aChelust1) allEstU=true; else allEstU=false;
			  if (aGuba1>aGuba && aZub1>aZub && aChelust1>aChelust) allEstD=true; else allEstD=false;
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
			  if (frSU>0) 
			  {   if(frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5) {
			         fr_all_Up=frUpH; fr_all_Up_Time=frUp_Time;  fr_all_Up_L=true; }
				  else 
			// Если появился новый фрактал  ВВЕРХ и касается Алигатора - отменяем "рабочий"
			      { fr_all_Up_L=false;   vlR.Time=Bars[Bars.Range.To-1000].Time;
				   if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) {
			            	var res = Trade.CancelPendingPosition(posGuidBuy); 
				            if (res.IsSuccessful) posGuidBuy=Guid.Empty;  }
			      }        
			  }
//===============================================================================================================================			
     		// Появился новый фрактал ВНИЗ  и Свеча Fractalа НИЖЕ  Alligatora не касается Alligatorа 
			// ВЕРХ Бар-Фрактала НИЖЕ Alligator  - Назначаем рабочим (fr_all_Down) для SellLimit
			  
			if (frSD>0) 
			{  if(frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5) { 
					fr_all_Down=frDownL;  fr_all_Down_Time=frDown_Time; fr_all_Down_L=true; } 
			   else  
			   // Если появился новый фрактал  ВНИЗ  и касается Алигатора - отменяем "рабочий"	
			   { fr_all_Down_L=false; 				   
			     if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
				     var res = Trade.CancelPendingPosition(posGuidSell);
				     if (res.IsSuccessful) posGuidSell = Guid.Empty;  }
			   }	
			}
//===============================================================================================================================
			// Появился новый фрактал ВВЕРХ и назначен рабочим и есть отложеный ордер  
			// меняем цену открытия и ставим TP - Низ/Верх противоположного фрактала
			if (frSU>0 && fr_all_Up_L &&  posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			{     // Ордер SellLimit-ВНИЗ - Цена открытия=High раб фрактала
				  var result = Trade.UpdatePendingPosition(posGuidBuy, 0.1, fr_all_Up+TP,frDownL-TP, null); 
				  if (result.IsSuccessful) posGuidBuy = result.Position.Id; 
			}
//===============================================================================================================================			
			// Если появился новый фрактал и не подходит под рабочий и есть отрытый отложеный ордер - отменяем ордер Buy
			if(frSU>0 && !fr_all_Up_L && posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			{ 
 			   var res = Trade.CancelPendingPosition(posGuidBuy);  if (res.IsSuccessful) posGuidBuy=Guid.Empty;
			}

//=============================----------------------------------------=====================================================	
			if (Torg && frSU>0 && fr_all_Up_L &&  posGuidBuy==Guid.Empty) 
			{
	// $$== ТОРГУЕМ!!!! ==$$  открываем BuyStop - Цена=High раб. ВВЕРХ фрактала SL=200p TP=Низ против. фрактала
				var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyStop, 0.1, fr_all_Up+TP, 0, Stops.InPips(200,null), null, null, null);
				if (result.IsSuccessful) posGuidBuy = result.Position.Id;
			}
		
//===============================================================================================================================
			// Появился новый фрактал ВНИЗ  и назначен рабочим но есть отложеный ВНИЗУ ордер  
			// переносим цену открытия и ставим Stop - Низ/Верх противоположного фрактала
			if(frSD>0 && fr_all_Down_L &&  posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			{
				var result = Trade.UpdatePendingPosition(posGuidSell, 0.1, fr_all_Down-TP,frUpH+TP, null); 
				if (result.IsSuccessful) posGuidSell = result.Position.Id;
			}
//===============================================================================================================================				
			// Если появился новый фрактал ВНИЗ и не подходит под рабочий и есть отрытый отложеный ордер - отменяем ордер Sell
			if(frSD>0 && !fr_all_Down_L  && posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			{ 
				var res = Trade.CancelPendingPosition(posGuidSell);
				 if (res.IsSuccessful) posGuidSell = Guid.Empty;
			}
//===============================================================================================================================				
			// Алигатор переплетен и появился новый фрактал ВНИЗ и назначен рабочим и нет открытых Sell ордеров  (posGuidSell)- 
			 // ставим отложеный ордер SellStop и отменяем BuyStop - если открыт!
			if(Torg && frSD>0 && fr_all_Down_L && posGuidSell==Guid.Empty) 
			{   // Открываем SellStop
			   var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellStop, 0.1, fr_all_Down-TP, 0, Stops.InPips(200,null), null, null, null);
			   if (result.IsSuccessful) posGuidSell=result.Position.Id;  
            }
//===============================================================================================================================
			// Появился новый фрактал ВВЕРХ и открыта позиция Sell - переосим стоп 
		   if (frSU>0 && posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active)  
		   { 
			   var res3=Trade.UpdateMarketPosition(posGuidSell, frUpH+TP, null, null); 
			   if (res3.IsSuccessful)  posGuidSell=res3.Position.Id; 
           }

//===============================================================================================================================
			// Появился новый фрактал ВВЕРХ и открыта позиция Sell - переосим стоп 
		   if (frSD>0 && posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active)  
		   { 
			   var res4=Trade.UpdateMarketPosition(posGuidBuy, frDownL-TP, null, null); 
			   if (res4.IsSuccessful)  posGuidSell=res4.Position.Id; 
           }

// =================================================================================================================================				 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) 
			     {  posGuidBuy=Guid.Empty; Print("Buy - Закрыто по StopLoss (Корекция) - {0} ",DTime);}

			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) 
				 {  posGuidSell=Guid.Empty;  Print("Sell - Закрыто по StopLoss (Корекция) - {0} ",DTime); }
			 
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
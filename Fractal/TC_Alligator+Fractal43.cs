//  Торговая система скопирована с видео :
//  https://www.youtube.com/watch?v=un9gck_8q04#t=1172
//
//  Торговая система "Alligator + Fractals"
//1. Если Линии Alligator переплетены - сигнал Torg1
//2. Берутся фракталы - которые не касаются Alligator fr_all_Up
//3. Новый АКТИВНЫЙ фрактал анулирует предыдущий
//4. Позиция открывается при пересечении линии АКТИВНОГО фрактала


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
    [TradeSystem("Alligator_Fractal")]
    public class Alligator1 : TradeSystem
    {
		
		private Guid _positionGuid=Guid.Empty;
        		
        // Simple parameter example
		public Alligator _allInd;
		public Fractals _frInd;
		
		double aGuba5;  //  Губы Зеленая
		double aZub5;    // Зубы  Красная
		double aChelust5;   // Челюсть Синяя
		
		double aGuba;  //  Губы Зеленая
		double aZub;    // Зубы  Красная
		double aChelust;   // Челюсть Синяя

		// Fractal
		double frUp = 0.0;   // Значение текущего верхнего Fractal
		double frDown = 0.0;  // Значение текущего нижнего Fractal
		double frUpL = 0.0;    // Значение Low - свечи с верхним фрактклом
		double frDownH = 0.0;    // Значение High - свечи с нижним фракталом
		public DateTime frUp_Time; // Время текущего фрактала вверху
		public DateTime frDown_Time; // Время текущего фрактала внизу
		
		public DateTime DTime; // Время 
		
		bool Time_Start = false;
		
		bool fBuy=false; // Сигнал на покупку - есть фрактал со свечей выше Аллигатора
		bool fSell=false; // Сигнал на продажу - есть фрактал со свечей ниже Аллигатора
		bool pBuy=false;  // Сигнал - открыт ордер на покупку
		bool pSell=false; // Сигнал - открыт ордер на продажу
		bool Torg=false; //  Сигнал - Линии Alligator переплетены
		
		
				// 1. Фрактал выше/ниже зубов Alligator
		double fr_all_Up;     // Цена последней свечи с фракталом - полностью выше Аллигатора
		double fr_all_Down;   // Цена последней свечи с фракталом - полностью ниже Аллигатора
		public DateTime fr_all_Up_Time;  // Время последней свечи с фракталом - полностью выше Аллигатора
		public DateTime fr_all_Down_Time; // Время последней свечи с фракталом - полностью ниже Аллигатора
				
		double OpenPrice;
		
		// Линии показывают АКТИВНЫЕ точки
		private VerticalLine vlineR;
		private VerticalLine vlineB;
		private HorizontalLine hline;
		private HorizontalLine lline;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
			// Вставить индикатор Alligator
			_allInd = GetIndicator<Alligator>(Instrument.Id, Timeframe);
			// Вставить индикатор Fractals
		     _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
					lline = Tools.Create<HorizontalLine>(); 
					hline = Tools.Create<HorizontalLine>();
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {

			// Значения Alligator около фрактала
			aGuba5 = _allInd.LipsSeries[Bars.Range.To-5];
            aZub5 = _allInd.TeethSeries[Bars.Range.To-5];
			aChelust5 =  _allInd.JawsSeries[Bars.Range.To-5];
			
			// Значене Alligator у цены
			aGuba = _allInd.LipsSeries[Bars.Range.To];      //З
            aZub = _allInd.TeethSeries[Bars.Range.To];      //К
			aChelust =  _allInd.JawsSeries[Bars.Range.To];  //С
			
            DTime = Bars[Bars.Range.To-5].Time;
			
			if (DTime.TimeOfDay.ToString()=="05:00:00") Time_Start=true;
			if (DTime.TimeOfDay.ToString()=="18:00:00") Time_Start=false;
			Print("Stop - {0}",(DTime.TimeOfDay.ToString()=="18:00:00"));
           
            // 1. Линии Alligator переплетены - Торгуем
			if ( !((aZub>aChelust && aGuba>aZub) || (aZub>aGuba && aChelust>aZub)) ) Torg=true; 
			
			// Данные индиктора Fractal - который первый после текущей цены
			// Запоминаем значени бара-фрактала(frUp) и время (frUp_Time)
			// Срабатывает - когда появился новый фрактал!
			  if (_frInd.TopSeries[Bars.Range.To-5]>0)    { frUp=Bars[Bars.Range.To-5].High; frUpL=Bars[Bars.Range.To-5].Low; frUp_Time = Bars[Bars.Range.To-5].Time;}
			  if (_frInd.BottomSeries[Bars.Range.To-5]>0) { frDown=Bars[Bars.Range.To-5].Low; frDownH=Bars[Bars.Range.To-5].High; frDown_Time = Bars[Bars.Range.To-5].Time;}

		    // Fractal ВЫШЕ Alligatora И свеча не касается Alligator- 
			// низ Бар-Фрактала выше Alligator  - Назначаем рабочим (fr_all_Up) для Buy
			  if (frUp>0 && frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5 && _frInd.TopSeries[Bars.Range.To-5]>0)   
			     { fr_all_Up=frUp; fr_all_Up_Time=frUp_Time; fBuy=true;  
					 
			// Рисуем горизонтальную линию сигнала	Buy        
			Tools.Remove(hline); 
			Print("Сигнал на Buy - {0} -- {1}",frUp_Time,frUp);
			hline = Tools.Create<HorizontalLine>();
            hline.Price = frUp;
			hline.Text="Buy - "+frUp_Time;				 
				 }  
				 
			// Если есть фрактал, есть сигнал на продажу и разрешение на торг 
		    //НО! свеча нового фрактала пересекает линию Аллигатора - отменяем позицию.	
			//	if ( _frInd.TopSeries[Bars.Range.To-5]>0) 
			//		Print("Пересечение  Алигатора фракталом {0} {1}",( frUpL<aGuba5 || frUpL<aChelust5 || frUpL<aZub5 ),Bars[Bars.Range.To-5].Time);
				 
			if (frUp>0 && Torg && fBuy && ( frUpL<aGuba5 || frUpL<aChelust5 || frUpL<aZub5 ) && _frInd.TopSeries[Bars.Range.To-5]>0) 
			      { fBuy=false; Print("Отмена - Buy {0} - на Баре {1}",fr_all_Up_Time,Bars[Bars.Range.To-1].Time); }
			 
		    // Fractal НИЖЕ Alligatora И свеча не касается Alligator - 
			// верх Бар-Фрактала выше Alligator  - Назначаем рабочим (fr_all_Down) для Sell
			if (frDown>0 && frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5 && _frInd.BottomSeries[Bars.Range.To-5]>0) 
			     { fr_all_Down=frDown;  fr_all_Down_Time=frDown_Time; fSell=true; 
					 
			// Рисуем горизонтальную линию сигнала	Sell		 			  
				Tools.Remove(lline);  
					 Print("Сигнал на Sell - {0} -- {1}",frDown_Time,frDown);
			    lline = Tools.Create<HorizontalLine>();
                lline.Price = fr_all_Down;
			    lline.Text="Sell = "+fr_all_Down_Time;
		    	 } 
				 
				// Если есть фрактал, есть сигнал на продажу и разрешение на торг 
		        //НО! свеча нового фрактала пересекает линию Аллигатора - отменяем позицию.	 
				  //if (_frInd.TopSeries[Bars.Range.To-5]>0) Print("DOWN - {0} {1} {2} - {3}",(Torg,fSell frDownH>aGuba5 || frDownH>aChelust5 || frDownH>aZub5 ), Bars[Bars.Range.To-5].Time);
		     	
				  if (frDown>0 && Torg && fSell && ( frDownH>aGuba5 || frDownH>aChelust5 || frDownH>aZub5 ) && _frInd.BottomSeries[Bars.Range.To-5]>0) 
			               { fSell=false; Print("Отмена -  Sell {0} - на Баре -{1}",fr_all_Down_Time,Bars[Bars.Range.To-1].Time); }	     
					 
				 // Если есть сигнал на покупку(fBuy) и не открыт дугой ордер Buy
				 // и текущий бар пересек HIGH РАБОЧЕГО Fractala  - Открываем ордер Buy 
				 if(Time_Start && Torg && fBuy && !pBuy && fr_all_Up<Bars[Bars.Range.To-1].High && _positionGuid==Guid.Empty) 
				           { Buy(); pBuy=true; fBuy=false; Torg=false; }
				 
                 // Если есть сигнал на продажу(fBuy) и не открыт дугой ордер Sell
				 // и текущий бар пересек LOW РАБОЧЕГО Fractala  - Открываем ордер Sell
				  if(Time_Start && Torg && fSell && !pSell && fr_all_Down>Bars[Bars.Range.To-1].Low  && _positionGuid==Guid.Empty)  
				           { Sell(); pSell=true;  fSell=false; Torg=false; }
				 
				 // Закрываем ордер на покупку когда свеча каснется Зубов Alligator		   
				 if(pBuy && Bars[Bars.Range.To-1].Low<aZub && _positionGuid!=Guid.Empty) { ClosePosition();  pBuy=false; Torg=false;}
				 
				 // Закрываем ордер на продажу когда свеча каснется Зубов	
				 if(pSell && Bars[Bars.Range.To-1].High>aZub && _positionGuid!=Guid.Empty) { ClosePosition();  pSell=false; Torg=false;}
				 
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
    }
}
// Автор: Рымарь Виктор
// Skype: rymar_victor
//  
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
    [TradeSystem("TC_Alligator+Fractal3")]  //copy of "TC_Alligator+Fractal2"
    public class Alligator1 : TradeSystem
    {
		
		private Guid _positionGuid=Guid.Empty;
        		
        // Simple parameter example
		public Alligator _allInd;
		public Fractals _frInd;
		public AwesomeOscillator _awoInd;
		public AcceleratorOscillator _aoInd;
		public FisherTransformOscillator _ftoInd;
		
		double aGuba5, aZub5, aChelust5;   // Челюсть Синяя
		double aGuba1, aZub1, aChelust1;
		double aGuba;  //  Губы Зеленая
		double aZub;    // Зубы  Красная
		double aChelust;   // Челюсть Синяя
		
		double BarH,BarL,BarC;
		bool aH=false;
		bool aL=false;
		        double SMa20;
		double SMa50;
		double SMa20_1;
		double SMa50_1;
		double SMa20_2;
		double SMa50_2;
		// Fractal
		double frUpH = 0.0;   // Значение текущего верхнего Fractal
		double frUpL = 0.0;    // Значение Low - свечи с верхним фрактклом
        double frUpC = 0.0;
		double frDownH = 0.0;    // Значение High - свечи с нижним фракталом
		double frDownL = 0.0;  // Значение текущего нижнего Fractal
		double frDownC = 0.0;
		
		public double sF=0,sF1=0;
		
		int CBarH=0;
		int CBarL=0;
		
		int NBarH=0;
		int NBarL=0;
		
		int XBarH=0;
		int XBarL=0;
		
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
		
		int i;int j;int i1=0; int j1=0; int i2=0; int j2=0; int i3=0; int j3=0;
				// AO
		double aoUp, aoUp1, aoUp2 ;
		double aoDown, aoDown1, aoDown2;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
			// Вставить индикатор Alligator
			_allInd = GetIndicator<Alligator>(Instrument.Id, Timeframe);
			// Вставить индикатор Fractals
		     _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
		    _awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);
            //_aoInd =  GetIndicator<AcceleratorOscillator>(Instrument.Id, Timeframe);
			 _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
			
		    	
			
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
// =================================================================================================================================				 
			sF = _ftoInd.FisherSeries[Bars.Range.To-1];
			sF1 = _ftoInd.FisherSeries[Bars.Range.To-2];
			
			  aoUp    = _awoInd.SeriesUp[Bars.Range.To-2];  // Зелені лінії - Вверху>0  Внизу<0
			  aoDown  = _awoInd.SeriesDown[Bars.Range.To-2];
			   
			  aoUp1   = _awoInd.SeriesUp[Bars.Range.To-3]; 
			  aoDown1 = _awoInd.SeriesDown[Bars.Range.To-3];
  
			  aoUp2   = _awoInd.SeriesUp[Bars.Range.To-4];
			  aoDown2 = _awoInd.SeriesDown[Bars.Range.To-4];
			
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
			
						// Значене Alligator у цены -1
			aGuba1 = _allInd.LipsSeries[Bars.Range.To-1];      //З
            aZub1 = _allInd.TeethSeries[Bars.Range.To-1];      //К
			aChelust1 =  _allInd.JawsSeries[Bars.Range.To-1];  //С
			
			bool aUgolH=false;
			bool aUgolL=false;
			
// =================================================================================================================================				 
            
			
			//  Время работы АТС
			if (DTime.TimeOfDay.ToString()=="05:00:00") Time_Start=true;
			if (DTime.TimeOfDay.ToString()=="18:00:00") Time_Start=false;
// ==================================================================================================================================
// Сигнал №1  Buy - MA20 пересекла MA50 вверх	
		     // H20x50-Пересечение МА ВВЕРХ, L20x50 - пересечение ВНИЗ
		     // i-количество свечей Buy после   j - количество свечей Sell после 
		     //  ВСЕ Обнуляем!
		    if( aChelust1>=aGuba1 && aGuba>=aChelust ) { 
				 aH=true; aL=false; NBarH=Bars.Range.To;  i=0; i1=0; j=0; j1=0;} 
			 //Sell - MA20 пересекла MA50 ВНИЗ	
			if( aGuba1>=aChelust1 && aChelust>=aGuba ) { 
				 aL=true; aH=false; NBarL=Bars.Range.To; i=0; i1=0; j=0; j1=0;} 
// =================================================================================================================================			
// Сигнал №2  Цена за алигатором
			// Для Buy
			// Все цены после пересечения закрывались за алигатором
			//if(aH) Print("BUY -- Bars.Range.To{0}==(NBarH+i)({1}+{2}) && Bars[Bars.Range.To].Close{3}>aGuba{4} -- {5}",Bars.Range.To,NBarH,i,Bars[Bars.Range.To].Close,aGuba,Bars[Bars.Range.To].Time);
			if (aH && Bars.Range.To==(NBarH+i) && Bars[Bars.Range.To].Close>aGuba && i1==0)  Red();
            // Для Sell
			//if(aL) Print("SELL -- Bars.Range.To{0}==(NBarL+i)({1}+{2}) && Bars[Bars.Range.To].Close{3}>aGuba{4} -- {5}",Bars.Range.To,NBarL,i,Bars[Bars.Range.To].Close,aGuba,Bars[Bars.Range.To].Time);
			if (aL && Bars.Range.To==(NBarL+i) && Bars[Bars.Range.To].High<aGuba && i1==0)  Blue();

// =================================================================================================================================			
// Сигнал №3  Линии алигатора разошлись - Растояние между линиями больше 0.001
     		// Для Buy
			//if (aH) Print("BUY - {0}>5 - {1}",(aChelust-aGuba)*10000,Bars[Bars.Range.To-1].Time);
			if (aH && Bars.Range.To==(NBarH+i) && ((aGuba-aChelust)>0.001) ) { aUgolH=true; }
            // Для Sell
			//if (aL) Print("SELL - {0}>5 - {1}",(aGuba-aChelust)*10000,Bars[Bars.Range.To-1].Time);
			if (aL && Bars.Range.To==(NBarL+j) && ((aChelust-aGuba)>0.001)) { aUgolL=true; }
			

// =================================================================================================================================			
// Сигнал №4  - Для Buy - Цена ЗАКРЫЛАСЬ внутри алигатора  
			//if (aH && aUgolH) Print("BUY -- Bars.Range.To{0}==(NBarH+i){1} && Bars[Bars.Range.To].Close{2}<aGuba{3} - {4}",Bars.Range.To,(NBarH+i),Bars[Bars.Range.To].Close,aGuba,Bars[Bars.Range.To].Time);
     		if (aH && aUgolH && Bars.Range.To==(NBarH+i) && Bars[Bars.Range.To].Close<aGuba ) { i1++; }
			// Цена вышла с алигатора  и серия сигналов ЗАКОНЧИЛАСЬ
			//if (aH && Bars.Range.To==(NBarH+i) && i1>0 && Bars[Bars.Range.To].Close>aGuba)  { i2++; }
			// Для Sell
			//if (aL && aUgolL) Print("Sell -- Bars.Range.To{0}==(NBarH+j){1} && Bars[Bars.Range.To].Close{2}>aGuba{3} - {4}",Bars.Range.To,(NBarL+j),Bars[Bars.Range.To].Close,aGuba,Bars[Bars.Range.To].Time);
 			if (aL && aUgolL && Bars.Range.To==(NBarL+j) && Bars[Bars.Range.To].Close>aGuba ) { j1++; }
			// Цена вышла с алигатора  и серия сигналов ЗАКОНЧИЛАСЬ1
			//if (aL && aUgolL && Bars.Range.To==(NBarL+j) && j1>0 && Bars[Bars.Range.To].Close<aGuba )  { j2++; }
			//  
			  
// =================================================================================================================================			
// Сигнал №5  - Для AO - Сигнал БЛЮДЦЕ
			//  Блюдце - низинка
		     // Для Buy
			  if (aH && Bars.Range.To==(NBarH+i) && i1>0 && aoDown2>0 && aoUp>0 && aoUp1>0 && _positionGuid==Guid.Empty)  { Buy(); Red(); aH=false; aUgolH=false; pBuy=true; }    // Зеленый - красный
		     // Sell 
			  if (aL && aUgolL && Bars.Range.To==(NBarL+j) && aoUp2<0 && aoDown<0 && aoDown1<0 && _positionGuid==Guid.Empty)  { Sell(); Blue(); aL=false; aUgolL=false; pSell=true; } // Красный - зеленый 

// =================================================================================================================================			
			
			 if(aH) i++;
			 if(aL) j++;
			
			//if (((aGuba-aChelust)>0.001) &&  _positionGuid==Guid.Empty) { j=0;  } 	

// =================================================================================================================================				 
			
			// Данные индиктора Fractal - который первый после текущей цены
			// Запоминаем значения High Low бара-фрактала(frUpH) и время (frUp_Time)
			// Срабатывает - когда появился новый фрактал!
			  if (_frInd.TopSeries[Bars.Range.To-5]>0)    { 
				                frUpH=Bars[Bars.Range.To-5].High; 
				                frUpL=Bars[Bars.Range.To-5].Low;
				                frUpC=Bars[Bars.Range.To-5].Close;
				                frUp_Time = Bars[Bars.Range.To-5].Time;}
			  if (_frInd.BottomSeries[Bars.Range.To-5]>0) { 
				               frDownL=Bars[Bars.Range.To-5].Low; 
				               frDownH=Bars[Bars.Range.To-5].High; 
				               frDownC=Bars[Bars.Range.To-5].Close; 
				               frDown_Time = Bars[Bars.Range.To-5].Time;}

			  
			  
			  
		    // Fractal ВЫШЕ Alligatora И свеча не касается Alligator- 
			// низ Бар-Фрактала выше Alligator  - Назначаем рабочим (fr_all_Up) для Buy
//			  if (frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5 && _frInd.TopSeries[Bars.Range.To-5]>0)   
//			     { fr_all_Up=frUpH; fr_all_Up_Time=frUp_Time; fBuy=true;  HLine();}  
 			// Fractal НИЖЕ Alligatora И свеча не касается Alligator - 
			// верх Бар-Фрактала выше Alligator  - Назначаем рабочим (fr_all_Down) для Sell
//			if (frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5 && _frInd.BottomSeries[Bars.Range.To-5]>0) 
//			     { fr_all_Down=frDownL;  fr_all_Down_Time=frDown_Time; fSell=true; LLine();} 
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
				 //if(pBuy && Bars[Bars.Range.To-1].Close<aGuba && _positionGuid!=Guid.Empty) { ClosePosition();  pBuy=false; Torg=false;}
				 if(pBuy && aoUp1>0 && aoDown>0 && _positionGuid!=Guid.Empty) { ClosePosition();  pBuy=false; Torg=false;}
				 
				 // Закрываем ордер на продажу когда свеча каснется Зубов	
				 //if(pSell && Bars[Bars.Range.To-1].Close>aGuba && _positionGuid!=Guid.Empty) { ClosePosition();  pSell=false; Torg=false;}
				 if(pSell && aoUp<0 && aoDown1<0 && _positionGuid!=Guid.Empty) { ClosePosition();  pSell=false; Torg=false;}
				 
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
            hline.Price = frUpH;
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
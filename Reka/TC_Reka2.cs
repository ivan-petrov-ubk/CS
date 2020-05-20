//  http://tradelikeapro.ru/strategiya-spokoynaya-reka/#more-17658
// Стратегия «Спокойная река» — скальпинг без суеты
// Таймфрейм - М5
// Инструменти : две МА 20 и 50 - Expotencial
// Условия входа на покупку:
//    Условие №1  - 20-я EMA выше 50-й.
//    Условие №2  - Цена выше 20-й и 50-й EMA.
//    Условие №3  - Обе средние направлены вверх.
//    ???? Средние не должны пересекаться часто. 20-я EMA выше 50-й .
//    Условие №4  - Цена корректируется и касается 20-й или 50-й EMA.
//         - Перед Корекцией должно 5 свечей Условий-1-3
//    Условие №5  - Нет более 3 подряд закрытых свечей ниже 20-й EMA.
//    Условие №6  - Свеча закрывается выше 20-й EMA – сигнал на вход.
//  Стоп лосс ставиться за локальным экстремумом (Fractalom)

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

namespace IPro.TradeSystems
{
    [TradeSystem("MA1")]

	public class MA1 : TradeSystem
    {
		//  
		
		public DateTime _Time;
		private MovingAverage _ma20;  // Скользящая средняя с периодом 20
		private MovingAverage _ma50;  // Скользящая средняя с периодом 50
		private MovingAverage _ma200;  // Скользящая средняя с периодом 50
		
		public Fractals _frInd;
		private StochasticOscillator _stoInd;
		private FisherTransformOscillator _ftoInd;
		//  Параметры для скользящих
		private int _maPeriod20 = 20;   
		private int _maPeriod50 = 50;
		private int _maShift = 0;
		public VerticalLine vl ;
		private MaMethods _maMethod = MaMethods.Ema;
		private PriceMode _priceMode = PriceMode.Close;

		private Guid _positionGuid=Guid.Empty;
		//  Значения скользящих - 
        double SMa20;
		double SMa50;
		double SMa20_1;
		double SMa50_1;
		double SMa20_2;
		double SMa50_2;
		
		// Фиксирований ТР
		double _Profit;
		
		// Fractal
		double frUpH = 0.0;   // Значение текущего верхнего Fractal
		double frDownL = 0.0;  // Значение текущего нижнего Fractal
		double frUpL = 0.0;    // Значение Low - свечи с верхним фрактклом
		double frDownH = 0.0;    // Значение High - свечи с нижним фракталом
		public DateTime frUp_Time; // Время текущего фрактала вверху
		public DateTime frDown_Time; // Время текущего фрактала внизу

		// Сигналы - что выполненыно Условие №
		// Покупка :
		bool MAHigh=false;   // Сигнал №1  - МА20 Выше МА50
		bool CenaH=false;    // Сигнал №2  - Цена выше 20-й и 50-й EMA.
		bool Verh=false;     // Сигнал №3  - Обе средние направлены вверх.
		bool KasanieH3=false;// Сигнад №4  - Цена корректируется и касается 20-й или 50-й EMA.
		bool CloseH3=false;  // Сигнал №5  - 3 Свеча закрывается выше 20-й EMA – сигнал на вход.
		bool pBuy=false;
		
		bool H20x50=false;
		bool L20x50=false;
		
		// Продажа
		bool MALow=false;    // Сигнал №1  - МА20 Ниже МА50
		bool CenaL=false;    // Сигнал №2  - Цена Ниже 20-й и 50-й EMA.
		bool Niz=false;      // Сигнал №3  - Обе средние направлены вниз.
		bool KasanieL3=false;// Сигнад №4  - Цена корректируется и касается 20-й или 50-й EMA.
		bool CloseL3=false;  // Сигнал №5  - 3 Свеча закрывается ниже 20-й EMA – сигнал на вход.
        bool pSell = false;
		bool FirstH=true;
		bool FirstL=true;
		
		//  Счетчики ---
		int CountH=0; // Счетчик свечей Buy Условий 1-3
		int CountL=0; // Счетчик свечей Sell Условий 1-3
		
		int CSell=0;  // 
		int CBuy=0;   // 
		
		int CBarH=0;
		int CBarL=0;
		
		int NBarH=0;
		int NBarL=0;
		
		int XBarH=0;
		int XBarL=0;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
                    // На график наносится скользящая средняя с периодом 20
				    _ma20 = GetIndicator<MovingAverage>(Instrument.Id, Timeframe, _maPeriod20, _maShift, _maMethod, _priceMode);
			        // На график наносится скользящая средняя с периодом 50
					_ma50 = GetIndicator<MovingAverage>(Instrument.Id, Timeframe, _maPeriod50, _maShift, _maMethod, _priceMode);
			        //_ma200 = GetIndicator<MovingAverage>(Instrument.Id, Timeframe, 200, _maShift, _maMethod, _priceMode);
			        
			        _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			//_stoInd = GetIndicator<StochasticOscillator>(Instrument.Id, Timeframe);
			//_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
			
       }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			// Время рабочей свечи
			_Time = Bars[Bars.Range.To-1].Time;
			// Запоминаем значение МА20 и Ма50 на рабочем баре
			SMa20 = _ma20.SeriesMa[Bars.Range.To-1];
            SMa50 = _ma50.SeriesMa[Bars.Range.To-1];
			// Запоминаем значение МА20 и Ма50 на предыдущем баре
			SMa20_1 = _ma20.SeriesMa[Bars.Range.To-2];
            SMa50_1 = _ma50.SeriesMa[Bars.Range.To-2];
			// Запоминаем значение МА20 и Ма50 на предыдущем баре
			SMa20_2 = _ma20.SeriesMa[Bars.Range.To-3];
            SMa50_2 = _ma50.SeriesMa[Bars.Range.To-3];
			
		  // Определяем значение фрактала ВВЕРХ(Тop)\ВНИЗ(Bottom)
		  if (_frInd.TopSeries[Bars.Range.To-5]>0)    { frUpH=Bars[Bars.Range.To-5].High; frUpL=Bars[Bars.Range.To-5].Low; frUp_Time = Bars[Bars.Range.To-5].Time;}
		  if (_frInd.BottomSeries[Bars.Range.To-5]>0) { frDownL=Bars[Bars.Range.To-5].Low; frDownH=Bars[Bars.Range.To-5].High; frDown_Time = Bars[Bars.Range.To-5].Time;}

// Сигнал №1  Buy - Определяем МА20 Выше  МА50
			if(SMa20>SMa50) MAHigh=true; else MAHigh=false;
            // Sell - Определяем МА20 Ниже  МА50			
			if(SMa50>SMa20) MALow=true; else MALow=false;
			
			
			if( SMa50_1>=SMa20_1 && SMa20>=SMa50 ) { NBarH=Bars.Range.To; H20x50=true; NBarL=0; L20x50=false;} 
			if( SMa20_1>=SMa50_1 && SMa50>=SMa20 ) { NBarL=Bars.Range.To; L20x50=true; NBarH=0; H20x50=false;} 
			
    //        Print("L20x50={0} && Bars[NBarL+1].High={1}<SMa20={2} -- {3} - {4}",L20x50, Bars[NBarL+1].High,SMa20, NBarL,Bars[Bars.Range.To-1].Time);
	//		  if(H20x50 && Bars.Range.To==(NBarH+1) && Bars[NBarH+1].Low>SMa20) Red();
	//	      if(H20x50 && Bars.Range.To==(NBarH+2) && Bars[NBarH+2].Low>SMa20) Red();
	//		  if(H20x50 && Bars.Range.To==(NBarH+3) && Bars[NBarH+3].Low>SMa20) Red();
	//		  if(H20x50 && Bars.Range.To==(NBarH+4) && Bars[NBarH+4].Low>SMa20) Red();
	//		  if(H20x50 && Bars.Range.To==(NBarH+5) && Bars[NBarH+5].Low>SMa20) Red();
			  
			  
	//		  if(L20x50 && Bars.Range.To==(NBarL+1) && Bars[NBarL+1].High<SMa20) Blue();
	//		  if(L20x50 && Bars.Range.To==(NBarL+2) && Bars[NBarL+2].High<SMa20) Blue();
	//		  if(L20x50 && Bars.Range.To==(NBarL+3) && Bars[NBarL+3].High<SMa20) Blue();
	//		  if(L20x50 && Bars.Range.To==(NBarL+4) && Bars[NBarL+4].High<SMa20) Blue();
	//		  if(L20x50 && Bars.Range.To==(NBarL+5) && Bars[NBarL+5].High<SMa20) Blue();
			  
// Сигнал №2  Buy - Цена выше 20-й и 50-й EMA.
			if(Bars[Bars.Range.To-1].Low>SMa20)  { CenaH=true; KasanieH3=false; KasanieL3=false;}	 else CenaH=false;
			// Sell - Цена ниже 20-й и 50-й EMA.
			if(Bars[Bars.Range.To-1].High<SMa20) { CenaL=true;  KasanieL3=false; KasanieH3=false;} else CenaL=false;
			
// Сигнал №3  Buy - Обе средние направлены вверх.
			if(((SMa20+0.0001)>SMa20_1) && (SMa50>SMa50_1)) Verh=true; else Verh=false;
			// Sell - Обе средние направлены вниз.
			if(((SMa20_1+0.0001)>SMa20) && (SMa50_1>SMa50)) Niz=true; else Niz=false;
			
// Сигнал №4  Buy - Цена касается 20-й MA.
			if(MAHigh && !KasanieH3 && (Bars[Bars.Range.To-1].Close)<SMa20 && Bars[Bars.Range.To-1].Close>SMa50)  
				  { Red(); XBarH=Bars.Range.To; XBarL=0; KasanieH3=true;} 
			     //if(Bars.Range.To==(NBarL+1) && Bars[NBarL+1].Close>SMa50) Red();	  
			     //if(Bars.Range.To==(NBarL+2) && Bars[NBarL+2].Close>SMa50) Red();	  				 
				 //if(Bars.Range.To==(NBarL+3) && Bars[NBarL+3].Close>SMa20) Red(); 
				  
            // Sell - Цена касается 20-й MA.
			if(MALow && !KasanieL3 && (Bars[Bars.Range.To-1].Close)>SMa20 && Bars[Bars.Range.To-1].Close<SMa50) 
			      { Blue(); XBarL=Bars.Range.To; XBarH=0; KasanieL3=true;} 
			     //if(Bars.Range.To==(NBarL+1) && Bars[NBarL+1].Close<SMa50) Red();	
				 //if(Bars.Range.To==(NBarL+1) && Bars[NBarL+1].Close<SMa50) Red();
				 //if(Bars.Range.To==(NBarL+2) && Bars[NBarL+1].Close<SMa20) Red(); 			
			
			
			

// Сигнал №5  Buy - Свеча закрывается выше 20-й EMA.
			if(MAHigh && Bars[Bars.Range.To-1].Close>SMa20)  CloseH3=true; else CloseH3=false;
			// Sell - Свеча закрывается ниже 20-й EMA.
			if(MALow && Bars[Bars.Range.To-1].Close<SMa20)  CloseL3=true; else CloseL3=false;
			
			
			//Print("Buy  - MAHigh={0} Verh=={1} (SMa20>SMa20_1)-{2}>{3}={4} - {5}",MAHigh,Verh,Niz,SMa20,SMa20_1,(SMa20>SMa20_1),_Time);
			
			// Вычисляем - Средние не должны пересекаться часто. 20-я EMA выше 50-й .
//			if (MAHigh && Verh && CenaH) { if(FirstH){CountH=0;CountL=0;} CountH++; FirstH=false;} else {  FirstH=true; }
//			if (MALow && Niz && CenaL)   { if(FirstL){CountL=0;CountH=0;} CountL++; FirstL=false;} else {  FirstL=true; } 
			
			// Ждем 3 свечу после касания
			//if ( CBuy>0 && CBuy<5 ) Red();
			//if ( CSell>0 && CSell<5 ) Blue();

//			if ( KasanieH3 && CountH>4 ) { Red(); Print("CountH={0}",CountH); }
//			if ( KasanieL3 && CountL>4 ) { Blue();  Print("CountL={0}",CountL); }
		
//			if ( KasanieH3 && CountH>4) { CBarH=Bars.Range.To+3;} 
//			if ( KasanieL3 && CountL>4) { CBarL=Bars.Range.To+3;}
			
				//if ( KasanieH3 && CBuy>5 ) { CBuy=0; Red(); }
					//if(Bars[Bars.Range.To-1].Low>SMa20) { Print("ВВЕРХ"); }  }
				
			// Sell - Обе средние направлены вниз.
			//if((SMa20_1>SMa20) && (SMa50_1>SMa50)) Niz=true; else Niz=false;
            //Print("Sell  - MALow={0} Niz=={1} SMa20-{2}={3} Sma50-{4}={5} - {6}",MALow,Niz,Math.Round((SMa20_1-SMa20)*100000),(SMa20_1>SMa20),Math.Round((SMa50_1-SMa50)*100000),(SMa50_1>SMa50),_Time);
		    
					// if(Bars[Bars.Range.To-1].High<SMa20) {  Print("ВНИЗ");	} }
            //if ( KasanieL3 && CSell>5 ) { CSell=0; Blue(); }
				
			//if( Bars[Bars.Range.To-4].Low<SMa20 && Bars[Bars.Range.To-1].Close>SMa20   ) 
			// { 
			//   vl = Tools.Create<VerticalLine>();
            //   vl.Time=Bars[Bars.Range.To-1].Time;
			//   vl.Color=Color.Red;
			//	}
	        //if (CBuy>0) Print("KasanieH3={0} CBuy={1} ",KasanieH3 ,CBuy);
		    //if (CSell>0) Print("KasanieL3={0} CSell={1} ",KasanieL3 ,CSell);

			//  Если : 3 свеча после касания и закрыта за МА20 и после касания МА20 небыло каксаний М50 - торгуем
//			if (CBarH==Bars.Range.To &&  CloseH3  && Bars[CBarH-1].Low>SMa50_1 && Bars[CBarH-2].Low>SMa50_2 && _positionGuid==Guid.Empty)  
//			           {   Buy(); pBuy=true; _Profit=Bars[Bars.Range.To-1].Close; }
//					   if (CBarH==Bars.Range.To)  CBarH=0;
//			if (CBarL==Bars.Range.To &&  CloseL3  && Bars[CBarL-1].High<SMa50_1 && Bars[CBarL-2].High<SMa50_2 && _positionGuid==Guid.Empty) 
//			           {   Sell(); pSell=true;  _Profit=Bars[Bars.Range.To-1].Close; }
//			if (CBarL==Bars.Range.To)  CBarL=0;
			//if(_Profit>0) Print("_Profit={0} - {1}     {2}",_Profit,(Bars[Bars.Range.To-1].Close-_Profit)*10000,Bars[Bars.Range.To-1].Time);
			
//			if (pBuy && Bars[Bars.Range.To-1].Close<frDownL  && _positionGuid!=Guid.Empty) { ClosePosition(); pBuy=false;_Profit=0;}
//			if (pSell && Bars[Bars.Range.To-1].Close>frUpH  && _positionGuid!=Guid.Empty) { ClosePosition(); pSell=false;_Profit=0;}
			//if (Math.Abs(Bars[Bars.Range.To-1].Close-_Profit)>0.001 && _positionGuid!=Guid.Empty) { ClosePosition(); pBuy=false; pSell=false; _Profit=0;}
			
			//if (pBuy && Bars[Bars.Range.To-1].Close<SMa50 && _positionGuid!=Guid.Empty)  { ClosePosition(); pBuy=false; _Profit=0;}
			//if (pSell && Bars[Bars.Range.To-1].Close>SMa50 && _positionGuid!=Guid.Empty) { ClosePosition(); pSell=false; _Profit=0;}
			//if ( CBuy>5) Red(); 
			//if ( CSell>5) Blue(); 			
			//if( !MAHigh && Bars[Bars.Range.To-4].Low>SMa20 && Bars[Bars.Range.To-1].Close<SMa20   ) 
			// { 
			//	  vl = Tools.Create<VerticalLine>();
            //    vl.Time=Bars[Bars.Range.To-1].Time;
			//    vl.Color=Color.Blue;
			//	}
						
        }
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            
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
                //Print("LR_Position_closed_by_inverse_signal", result.Position.Number, result.Position.ClosePrice);	
				_positionGuid = Guid.Empty;			
			}
		}
          private void Red() 
            {
		   var vl = Tools.Create<VerticalLine>();
               vl.Time=_Time;
			   vl.Color=Color.Red;
			}

		private void Blue() 
			{
			var vl = Tools.Create<VerticalLine>();
                vl.Time=_Time;
			    vl.Color=Color.Blue;
			}
	}
}
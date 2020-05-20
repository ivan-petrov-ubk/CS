//  http://tradelikeapro.ru/strategiya-spokoynaya-reka/#more-17658
// Стратегия «Спокойная река» — скальпинг без суеты
// Таймфрейм - М5
// Инструменти : две МА 20 и 50 - Expotencial
// Условия входа на покупку:
//    Условие №1  - 20-я EMA выше 50-й. TRIGER MAHigh MALow
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
		
		int i;int j;int i1=0; int j1=0; int i2=0; int j2=0; int i3=0; int j3=0;
		
		// Сигналы - что выполненыно Условие №
		// Покупка :
		bool MAHigh=false;   // Сигнал №1  - МА20 Выше МА50
		bool CenaH=false; int cCenaH=0;   // Сигнал №2  - Цена выше 20-й и 50-й EMA.
		bool Verh=false;     // Сигнал №3  - Обе средние направлены вверх.
		bool KasanieH3=false;// Сигнад №4  - Цена корректируется и касается 20-й или 50-й EMA.
		bool CloseH3=false;  // Сигнал №5  - 3 Свеча закрывается выше 20-й EMA – сигнал на вход.
		bool pBuy=false;
		
		bool H20x50=false;
		bool L20x50=false;
		
		// Продажа
		bool MALow=false;    // Сигнал №1  - МА20 Ниже МА50
		bool CenaL=false; int cCenaL=0;    // Сигнал №2  - Цена Ниже 20-й и 50-й EMA.
		bool Niz=false;      // Сигнал №3  - Обе средние направлены вниз.
		bool KasanieL3=false;// Сигнад №4  - Цена корректируется и касается 20-й или 50-й EMA.
		bool CloseL3=false;  // Сигнал №5  - 3 Свеча закрывается ниже 20-й EMA – сигнал на вход.
        bool pSell = false;
		bool FirstH=true;
		bool FirstL=true;

		
		//  Счетчики ---
		int CountH=0; // Счетчик свечей Buy Условий 1-3
		int CountL=0; // Счетчик свечей Sell Условий 1-3
		
		bool CSell=false;  // 
		bool CBuy=false;   // 
		
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
			        
			        _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
       }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
				 var posActiveMine = Trade.GetActivePositions(null, true);
            if(posActiveMine.Length==0) { _positionGuid = Guid.Empty; pBuy=false; pSell=false; }
			
			// Время рабочей свечи
			_Time = Bars[Bars.Range.To-1].Time;
			// Запоминаем значение МА20 и Ма50 на рабочем баре
			SMa20 = _ma20.SeriesMa[Bars.Range.To-1];
            SMa50 = _ma50.SeriesMa[Bars.Range.To-1];
			// Запоминаем значение МА20 и Ма50 на предыдущем баре
			SMa20_1 = _ma20.SeriesMa[Bars.Range.To-2];
            SMa50_1 = _ma50.SeriesMa[Bars.Range.To-2];

		  // Определяем значение фрактала ВВЕРХ(Тop)\ВНИЗ(Bottom)
		  if (_frInd.TopSeries[Bars.Range.To-5]>0)    { frUpH=Bars[Bars.Range.To-5].High; frUpL=Bars[Bars.Range.To-5].Low; frUp_Time = Bars[Bars.Range.To-5].Time;}
		  if (_frInd.BottomSeries[Bars.Range.To-5]>0) { frDownL=Bars[Bars.Range.To-5].Low; frDownH=Bars[Bars.Range.To-5].High; frDown_Time = Bars[Bars.Range.To-5].Time;}

			
// Сигнал №1  Buy - MA20 пересекла MA50 вверх	
		     // H20x50-Пересечение МА ВВЕРХ, L20x50 - пересечение ВНИЗ
		     // i-количество свечей Buy после   j - количество свечей Sell после 
		  //  ВСЕ Обнуляем!
		    if( SMa50_1>=SMa20_1 && SMa20>=SMa50 ) { H20x50=true; NBarH=Bars.Range.To; L20x50=false; 
				 i=0; i1=0; j=0; j1=0; } 
			  //Sell - MA20 пересекла MA50 ВНИЗ	
			if( SMa20_1>=SMa50_1 && SMa50>=SMa20 ) { L20x50=true; NBarL=Bars.Range.To; H20x50=false; 
				 i=0; i1=0; j=0; j1=0;} 
             
// Сигнал №2  Buy - Цена выше 20-й и 50-й EMA.			
	          // Отмечаем ВСЕ свечи выше МА и с пересечения
			  //if(H20x50) Print("Bars.Range.To{0}==(NBarH+i){1} && Bars[Bars.Range.To-1].Close{2}>SMa20{3} && i1{4}==0",Bars.Range.To,(NBarH+i),Bars[Bars.Range.To-1].Low,SMa20,i1);
			  if(H20x50 && Bars.Range.To==(NBarH+i) && Bars[Bars.Range.To].Low>SMa20 && i1==0 && i==0)  { Red(); Buy(); pBuy=true;}
			  if(H20x50 && Bars.Range.To==(NBarH+i) && Bars[Bars.Range.To].Low>SMa20 && i1==0)  Red();
			  if(H20x50) i++;
			  
			  //if(L20x50) Print("Bars.Range.To{0}==(NBarL+i){1} && Bars[Bars.Range.To-1].Close{2}<SMa20{3} && i1{4}==0",Bars.Range.To,(NBarL+i),Bars[Bars.Range.To-1].Close,SMa20,i1);
			  if(L20x50 && Bars.Range.To==(NBarL+j) && Bars[Bars.Range.To].High<SMa20 && j1==0 && j==0)  { Blue(); Sell();pSell=true;} 
			  if(L20x50 && Bars.Range.To==(NBarL+j) && Bars[Bars.Range.To].High<SMa20 && j1==0 ) Blue();
			  if(L20x50 && Bars.Range.To==(NBarL+j) && Bars[Bars.Range.To].High>SMa20 ) j1++;  
			  if(L20x50) j++;

// Сигнал №4  Buy - Цена касается 20-й MA.
			 // Print("pBuy={0} && Close={1}<SMa20={2} && Close={1}>SMa50={3}",pBuy,Bars[Bars.Range.To-1].Close,SMa20,SMa50);
			if(i>7 &&  i1==1 && Bars[Bars.Range.To-1].Close<SMa20 )  
				  { XBarH=Bars.Range.To; XBarL=0; i2=1; i=0; Green(); i1=0; }
				  //if(i>5) Print("(i{0}==6 && Bars.Range.To{1}==(NBarL+1){2} && Bars[NBarL+1].Close{3}>SMa50{4})",i,Bars.Range.To,NBarL+1,Bars[NBarL+1].Close,SMa50);
			     if(i2==1 && Bars.Range.To==(XBarH+1) ) { i2++;Green();} 	  
			     if(i2==2 && Bars.Range.To==(XBarH+2) ) { i2++;Green();}	  				 
				 if(i2==3 && Bars.Range.To==(XBarH+3) && Bars[XBarH+3].Close>SMa20  && _positionGuid==Guid.Empty) 
				 {Blue();  i1=0; i3=0; i2=0;    Print("Buy!! - {0} = {1} ",(SMa50-SMa20),Bars[Bars.Range.To-1].Time);} 
				 
				 
            // Sell - Цена касается 20-й MA.
			if(j>7 && j1==1 &&  (Bars[Bars.Range.To-1].Close)>SMa20 ) 
			      { XBarL=Bars.Range.To; XBarH=0; j2=1; j=0; Yellow(); j1=0;} 
			     if(j2==1 && Bars.Range.To==(XBarL+1)) { j2++;Yellow();} 		
				 if(j2==2 && Bars.Range.To==(XBarL+2) ) { j2++;Yellow();}
				 if(j2==3 && Bars.Range.To==(XBarL+3) && Bars[XBarL+3].Close<SMa20 && _positionGuid==Guid.Empty) 
				 {  Red();  j1=0; j3=0; j2=0;  j=0; Print("SEL!! - {0} = {1}",(SMa50-SMa20),Bars[Bars.Range.To-1].Time);}
								 
				 if(pBuy)  i3++;
				 if(pSell) j3++;					 
		
		//		 if (pBuy && i1==7 && _positionGuid!=Guid.Empty) { ClosePosition(); pBuy=false; }
		//		 if (pSell && j1==7 && _positionGuid!=Guid.Empty) { ClosePosition(); pSell=false; }

    	//		 
	        if (pBuy && i3>5 && Bars[Bars.Range.To-1].Close<frDownL  && _positionGuid!=Guid.Empty) { ClosePosition(); pBuy=false; i3=0; } 
		//		        Print("{0} --Bars[Bars.Range.To-1].Close{1}<frDownL{2} - {3} ",Bars[Bars.Range.To-1].Time,Bars[Bars.Range.To-1].Close,frDownL,frDown_Time); }
		    if (pSell && j3>5 && Bars[Bars.Range.To-1].Close>frUpH  && _positionGuid!=Guid.Empty) { ClosePosition(); pSell=false; j3=0;}		
		//	
			if (pBuy && Bars[Bars.Range.To-1].Close<SMa20 && _positionGuid!=Guid.Empty)  { ClosePosition(); pBuy=false; i3=0; }
		//	     Print("{0} -- Bars[Bars.Range.To-1].Close{1}<SMa50{2}",Bars[Bars.Range.To-1].Time,Bars[Bars.Range.To-1].Close,SMa50); }
			if (pSell && Bars[Bars.Range.To-1].Close>SMa20 && _positionGuid!=Guid.Empty) { ClosePosition(); pSell=false; j3=0;}
					
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

	     private void Yellow() 
            {
		   var vl = Tools.Create<VerticalLine>();
               vl.Time=_Time;
			   vl.Color=Color.Yellow;
			}	
			
		private void Blue() 
			{
			var vl = Tools.Create<VerticalLine>();
                vl.Time=_Time;
			    vl.Color=Color.Blue;
			}
			
		private void Green() 
			{
			var vl = Tools.Create<VerticalLine>();
                vl.Time=_Time;
			    vl.Color=Color.LightSeaGreen;
			}	
	}
}
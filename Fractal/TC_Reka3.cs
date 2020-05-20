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
		int i;
		int j;
		int i1=0;
        int j1=0;
		int i2=0;
        int j2=0;
		
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
			        
			        _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
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

			
// Сигнал №1  Buy - MA20 пересекла MA50 вверх	NBarH-номер бара пересечения		
			if( SMa50_1>=SMa20_1 && SMa20>=SMa50 ) { NBarH=Bars.Range.To; i=1; H20x50=true; NBarL=0; L20x50=false; } 
			  //Sell - MA20 пересекла MA50 ВНИЗ	
			if( SMa20_1>=SMa50_1 && SMa50>=SMa20 ) { NBarL=Bars.Range.To; j=1; L20x50=true; NBarH=0; H20x50=false; } 
             
// Сигнал №2  Buy - Цена выше 20-й и 50-й EMA.			
			do {
				if(H20x50 && Bars.Range.To==(NBarH+i) && Bars[NBarH+1].Low>SMa20) { i++; Red(); } 
			} while (Bars[Bars.Range.To-1].Close<SMa20 && Bars[Bars.Range.To-1].Close>SMa50);
			
		//	  if(H20x50 && Bars.Range.To==(NBarH+1) && Bars[NBarH+1].Low>SMa20) { i++; Red(); } 
		//	  if(H20x50 && Bars.Range.To==(NBarH+2) && Bars[NBarH+2].Low>SMa20) { i++; Red(); } 
		//	  if(H20x50 && Bars.Range.To==(NBarH+3) && Bars[NBarH+3].Low>SMa20) { i++; Red(); } 
		//	  if(H20x50 && Bars.Range.To==(NBarH+4) && Bars[NBarH+4].Low>SMa20) { i++; Red(); } 
		//	  if(H20x50 && Bars.Range.To==(NBarH+5) && Bars[NBarH+5].Low>SMa20) { i++; Red(); } 
			  //if(i>0 && i<5) Print("BUY {0} - (H20x50={1} && Bars.Range.To={2} == (NBarH+i){3} && Bars[NBarH+i].Low{4}>SMa20={5})",i,H20x50, Bars.Range.To,(NBarH+i),Bars[NBarH+i].Low,SMa20);
			  
			  // Sell - Цена ниже 20-й и 50-й EMA.
          do 
          {
			  if(L20x50 && Bars.Range.To==(NBarL+1) && Bars[NBarL+1].High<SMa20) {  j++; Blue(); }
		  }  while(Bars[Bars.Range.To-1].Close>SMa20 && Bars[Bars.Range.To-1].Close<SMa50);
		  
	//		if(L20x50 && Bars.Range.To==(NBarL+1) && Bars[NBarL+1].High<SMa20) {  j++; Blue(); } 
	//		  if(L20x50 && Bars.Range.To==(NBarL+2) && Bars[NBarL+2].High<SMa20) {  j++; Blue(); }
	//		  if(L20x50 && Bars.Range.To==(NBarL+3) && Bars[NBarL+3].High<SMa20) {  j++; Blue(); }
	//		  if(L20x50 && Bars.Range.To==(NBarL+4) && Bars[NBarL+4].High<SMa20) {  j++; Blue(); }
	//		  if(L20x50 && Bars.Range.To==(NBarL+5) && Bars[NBarL+5].High<SMa20) {  j++; Blue(); }
	  		  //if(j>0 && j<5) Print("SELL {0} - (L20x50={1} && Bars.Range.To={2} == (NBarL+i){3} && Bars[NBarL+i].High{4}<SMa20={5})",j,L20x50, Bars.Range.To,(NBarL+j),Bars[NBarL+j].High,SMa20);

// Сигнал №4  Buy - Цена касается 20-й MA.
			 // Print("pBuy={0} && Close={1}<SMa20={2} && Close={1}>SMa50={3}",pBuy,Bars[Bars.Range.To-1].Close,SMa20,SMa50);
			if(i==5 && Bars[Bars.Range.To-1].Close<SMa20 && Bars[Bars.Range.To-1].Close>SMa50)  
				  { XBarH=Bars.Range.To; XBarL=0; i2=1; i=0; Green(); }
				  //if(i>5) Print("(i{0}==6 && Bars.Range.To{1}==(NBarL+1){2} && Bars[NBarL+1].Close{3}>SMa50{4})",i,Bars.Range.To,NBarL+1,Bars[NBarL+1].Close,SMa50);
			     if(i2==1 && Bars.Range.To==(XBarH+1) && Bars[XBarH+1].Close>SMa50) { i2++;} 	  
			     if(i2==2 && Bars.Range.To==(XBarH+2) && Bars[XBarH+2].Close>SMa50) { i2++;}	  				 
				 if(i2==3 && Bars.Range.To==(XBarH+3) && Bars[XBarH+3].Close>SMa20  && _positionGuid==Guid.Empty) {Buy(); i1=0; i2=0; pBuy=true;   Print("Buy!! - {0} ==============================================",Bars[Bars.Range.To-1].Time);} 
				 
				 
            // Sell - Цена касается 20-й MA.
			if(j==5 &&  (Bars[Bars.Range.To-1].Close)>SMa20 && Bars[Bars.Range.To-1].Close<SMa50) 
			      { XBarL=Bars.Range.To; XBarH=0; j2=1; j=0; Yellow(); } 
			     if(j2==1 && Bars.Range.To==(XBarL+1) && Bars[XBarL+1].Close<SMa50) { j2++;} 		
				 if(j2==2 && Bars.Range.To==(XBarL+2) && Bars[XBarL+2].Close<SMa50) { j2++;}
				 if(j2==3 && Bars.Range.To==(XBarL+3) && Bars[XBarL+3].Close<SMa20 && _positionGuid==Guid.Empty) { Sell(); j1=0; j2=0; pSell=true;  j=0; Print("SEL!! - {0} ==============================================",Bars[Bars.Range.To-1].Time);}
								 
				 if(pBuy) i1++;
				if(pSell) j1++;					 
		
				 if (pBuy && i1==7 && _positionGuid!=Guid.Empty) { ClosePosition(); pBuy=false; }
				 if (pSell && j1==7 && _positionGuid!=Guid.Empty) { ClosePosition(); pSell=false; }


				 
				 
	        if (pBuy && i1>5 && Bars[Bars.Range.To-1].Close<frDownL  && _positionGuid!=Guid.Empty) { ClosePosition(); pBuy=false; 
				        Print("{0} --Bars[Bars.Range.To-1].Close{1}<frDownL{2} - {3} ",Bars[Bars.Range.To-1].Time,Bars[Bars.Range.To-1].Close,frDownL,frDown_Time); }
		    if (pSell && j1>5 && Bars[Bars.Range.To-1].Close>frUpH  && _positionGuid!=Guid.Empty) { ClosePosition(); pSell=false;}		
			
			if (pBuy && Bars[Bars.Range.To-1].Close<SMa50 && _positionGuid!=Guid.Empty)  { ClosePosition(); pBuy=false; 
			     Print("{0} -- Bars[Bars.Range.To-1].Close{1}<SMa50{2}",Bars[Bars.Range.To-1].Time,Bars[Bars.Range.To-1].Close,SMa50); }
			
			if (pSell && Bars[Bars.Range.To-1].Close>SMa50 && _positionGuid!=Guid.Empty) { ClosePosition(); pSell=false;}
					
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
			    vl.Color=Color.DarkGreen;
			}	
	}
}
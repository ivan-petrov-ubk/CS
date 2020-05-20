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
    [TradeSystem("TC_Alligator+Fractal11")]       //copy of "TC_Alligator+Fractal10"
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
		bool allEst=false;
		
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
		
		public ISeries<Bar> _barSeries,_barM15;
		private int _lastIndex = -1;
		private int _lastIndexM15 = -1;
		public FisherTransformOscillator _ftoInd,_ftoIndM15;
		public double sF=0,sF1=0,smF=0,smF1=0;
		public double sHF=0,sM15F=0;
		public Period periodM15;

		 int[] fUP = new int[15];
		 int[] fDown = new int[15];
		int ifrU=0,ifrD=0;
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
			// Вставить индикатор Alligator
			_allInd = GetIndicator<Alligator>(Instrument.Id, Timeframe);
			// Вставить индикатор Fractals
		    _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
				
			periodM15 = new Period(PeriodType.Minute, 15);
			_barSeries = GetCustomSeries(Instrument.Id, Period.H1);
			_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			//_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Period.H1); 
			//_ftoIndM15= GetIndicator<FisherTransformOscillator>(Instrument.Id, periodM15);
			
			
			_goInd = GetIndicator<GatorOscillator>(Instrument.Id, Timeframe);

        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
			 // Event occurs on every new quote
	 if (_lastIndex < _barSeries.Range.To - 1) {
     		_lastIndex = _barSeries.Range.To - 1;
    		var closePrice = _barSeries[_lastIndex].Close;
	 		var timePrice = _barSeries[_lastIndex].Time;
	 		sF = _ftoInd.FisherSeries[_lastIndex];
	 		sF1 = _ftoInd.FisherSeries[_lastIndex-1];
	 		sHF=sF-sF1;  }

	 if ((_barM15[_barM15.Range.To-1].High>0) && (_lastIndexM15 < _barM15.Range.To - 1)) {
     		_lastIndexM15 = _barM15.Range.To - 1;
     		var closePriceM15 = _barM15[_lastIndexM15].Close;
	 		var timePriceM15 = _barM15[_lastIndexM15].Time;
	 		smF = _ftoIndM15.FisherSeries[_lastIndexM15];
	 		smF1 = _ftoIndM15.FisherSeries[_lastIndexM15-1];
	 		sM15F=smF-smF1; }
					
        }
        
        protected override void NewBar()
        { 
// =================================================================================================================================				 
			if (Bars[Bars.Range.To-1].Time.Hour==23 && Bars[Bars.Range.To-1].Time.Minute==20 ) { Torg=true; Print("****************************** 6 год "); }
			if (Bars[Bars.Range.To-1].Time.Hour==06 && Bars[Bars.Range.To-1].Time.Minute==00) 	  Torg=false;
			
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
			
			// Значения Gator Oscillator 
			gNU=_goInd.NegativeSeriesUp[Bars.Range.To-1];
			gND=_goInd.NegativeSeriesDown[Bars.Range.To-1];
			gPD=_goInd.PositiveSeriesDown[Bars.Range.To-1];
			gPU=_goInd.PositiveSeriesUp[Bars.Range.To-1];
			
// =================================================================================================================================				 
// 1. Alligator СПИТ - Торгуем
		    // Правильно ЗКС или СКЗ - если наоборот то  линии переплелись - Торг 
			// if ( !((aGuba>aZub && aZub>aChelust) || (aChelust>aZub && aZub>aGuba)) ) { Torg=true;  
				//Print("#01 Torg - Линии Алигатора переплелись! ",DTime);}
			
//  1-A. Определяем спит аллигатор или ест - по индикатору Gattor
			// Print("gNU={0},gND={1},gPU={2},gPD={3} -- {4}",gNU,gND,gPU,gPD,Bars[Bars.Range.To-1].Time);
			// if (gPU>0 && gNU<0) { allEst=true; Red(); } else allEst=false;
			
			// Print("Алигатор ЕСТ - PU={0} NU={1} - {2} ",gPU,gNU,Bars[Bars.Range.To-1].Time);
			// if ((gPD>0 && gND<0) || (gPD>0 && gNU<0) || (gPU>0 && gND<0)) { allSpit=true; 
			// Print("#01 Аллигатор СПИТ! -- {0} ",DTime);
			//} else { allSpit=false; } 
			
			
			// Print("Алигатор СПИТ - PD={0} ND={1} - {2} ",gPD,gND,Bars[Bars.Range.To-1].Time);
			// if ((gPD>0 && gNU<0) || (gPU>0 && gND<0)) 
			// Print("Алигатор ПРОСЫПАЕТСЯ - PD>0={0} NU<0={1} gPU>0={2} gND<0={3} - {4} ",gPD,gNU,gPU,gND,Bars[Bars.Range.To-1].Time);
// =================================================================================================================================				 
     		// Срабатывает - когда появился новый фрактал - frUp frDown=true!
			// Запоминаем значения Свечи бара-фрактала(frUpH) и время (frUp_Time)
			  if (frSU>0)    { 
				                frUpH=Bars[Bars.Range.To-5].High; 
				                frUpL=Bars[Bars.Range.To-5].Low;
				                frUpC=Bars[Bars.Range.To-5].Close;
				                frUp_Time = Bars[Bars.Range.To-5].Time;}
			  if (frSD>0)    { 
				               frDownL=Bars[Bars.Range.To-5].Low; 
				               frDownH=Bars[Bars.Range.To-5].High; 
				               frDownC=Bars[Bars.Range.To-5].Close; 
				               frDown_Time = Bars[Bars.Range.To-5].Time;}
// ==================================================================================================================================		  

		    // Появился новый фрактал и Свеча Fractalа ВЫШЕ Alligatora не касается Alligatorа 
			// низ Бар-Фрактала выше Alligator  - Назначаем рабочим (fr_all_Up) для Buy
			  // fr_all_Up_L - если появился рабочий фрактал Buy - true
			  if (frSU>0 && frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5)   
			     { fr_all_Up=frUpH; fr_all_Up_Time=frUp_Time;  fr_all_Up_L=true;  ifrU++; ifrD=0;
					 Print("02 Фрактал ВВЕРХ и выше Алигатора - {0} - {1} ",Bars[Bars.Range.To-1].Time,Torg);}
			// Если появился новый фрактал  ВВЕРХ и касается Алигатора - отменяем "рабочий"
			  if ( frSU>0 && !(frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5) ) { fr_all_Up_L=false; ifrU=0; 
				     Print("03 Фрактал ВВЕРХ но касается Алигатора - {0} - {1}}",Bars[Bars.Range.To-1].Time,Torg); }  
//===============================================================================================================================			
     		// Появился новый фрактал ВНИЗ  и Свеча Fractalа НИЖЕ  Alligatora не касается Alligatorа 
			// ВЕРХ Бар-Фрактала НИЖЕ Alligator  - Назначаем рабочим (fr_all_Down) для SellLimit
			if (frSD>0 && frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5) 
				{ fr_all_Down=frDownL;  fr_all_Down_Time=frDown_Time; fr_all_Down_L=true; ifrD++;ifrU=0;  
					Print("04 Фрактал НИЖЕ Алигатора - {0} - {1}",Bars[Bars.Range.To-1].Time,Torg); } 
			// Если появился новый фрактал  ВНИЗ  и касается Алигатора - отменяем "рабочий"	
			if (frSD>0 && !(frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5)) { fr_all_Down_L=false; ifrD=0; 
				    Print("05 Фрактал ВНИЗ и касается Алигатора - {0} - {1}",Bars[Bars.Range.To-1].Time,Torg); }	
//===============================================================================================================================
			  
	if (ifrU>2) Red();
	if (ifrD>2) Blue();
			  
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
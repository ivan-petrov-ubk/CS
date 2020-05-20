//  https://www.youtube.com/watch?v=un9gck_8q04#t=1172
//  Торговая система "Alligator + Fractals"
//1. Линии Алигатора переплетены.
//2. Берутся фракталы - которые не касаются Алигатора
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

		double aGuba0;  //  Губы Зеленая
		double aZub0;    // Зубы  Красная
		double aChelust0;   // Челюсть Синяя
		
		
		// Fractal
		double frUp = 0.0;
		double frDown = 0.0;
		double frUpL = 0.0;
		double frDownH = 0.0;
		public DateTime frUp_Time;
		public DateTime frDown_Time;
		public DateTime fr_all_Up_Time;
		public DateTime fr_all_Down_Time;
		bool fBay=false;
		bool fSell=false;
		bool pBay=false;
		bool pSell=false;
		
		
				// 1. Фрактал выше/ниже зубов алигатора
		double fr_all_Up;     // 
		double fr_all_Down;   //  
		int Sig=0;
		int Torg=0;
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
						   // Event occurs on every new bar
			   // Print("Бар № {0} - {1} ==================================================================",Bars.Range.To, Bars[Bars.Range.To-1].Close);
			   // Print("Alligator челюсть синяя: {0}", _allInd.JawsSeries[Bars.Range.To-1]);
			   // Print("Alligator губы зеленая: {0}", _allInd.LipsSeries[Bars.Range.To-1]);
		       // Print("Alligator зубы красная: {0}", _allInd.LipsSeries[Bars.Range.To-1]);
			
			// Значения Алигатора около фрактала
			aGuba5 = _allInd.LipsSeries[Bars.Range.To-5];
            aZub5 = _allInd.TeethSeries[Bars.Range.To-5];
			aChelust5 =  _allInd.JawsSeries[Bars.Range.To-5];
			
			
			// Значене Алигатора у цены
			aGuba = _allInd.LipsSeries[Bars.Range.To];      //З
            aZub = _allInd.TeethSeries[Bars.Range.To];      //К
			aChelust =  _allInd.JawsSeries[Bars.Range.To];  //С

			
	
            
            // 1. Линии Алигатора переплетены - Торгуем
			if ( !((aZub>aChelust && aGuba>aZub) || (aZub>aGuba && aChelust>aZub)) ) Torg=1; 
			
			// Print("{0}  = З-{1} K-{2} C-{3} Fractal = {4}",Bars[Bars.Range.To-5].Time,aGuba5,aZub5,aChelust5,_frInd.BottomSeries[Bars.Range.To-5]);
			// Print("!((aZub{1}>aChelust{2} && aGuba{0}>aZub{1}) || (aZub{1}>aGuba{0} && aChelust{2}>aZub{1}))",aGuba,aZub,aChelust);
			//if ( !((aZub>aChelust && aGuba>aZub)) ) { Torg=1; Red(); }
			//Print("Standart - {0} - {1} - {2}",(!(aZub>aChelust && aGuba>aZub)),Bars[Bars.Range.To-1].Time,aZub);
			//Print("Fractal - {0} - {1} - {2}",(!(aZub5>aChelust5 && aGuba5>aZub5)),Bars[Bars.Range.To-1].Time,aZub5);
			
			// Данные индиктора Fractal - который первый после текущей цены
			// Запоминаем значени бара-фрактала(frUp) и время (frUp_Time)
			// Срабатывает - когда появился новый фрактал!
	
			  if (_frInd.TopSeries[Bars.Range.To-5]>0)    { frUp=Bars[Bars.Range.To-5].High; frUpL=Bars[Bars.Range.To-5].Low; frUp_Time = Bars[Bars.Range.To-5].Time;}
			  if (_frInd.BottomSeries[Bars.Range.To-5]>0) { frDown=Bars[Bars.Range.To-5].Low; frDownH=Bars[Bars.Range.To-5].High; frDown_Time = Bars[Bars.Range.To-5].Time;}

     		//  Нижний и Верхний фрактал
			//  Print("Низ={0}  - Верх={1} ",_frInd.BottomSeries[Bars.Range.To-5], _frInd.TopSeries[Bars.Range.To-5]);
			
		    // Fractal ВЫШЕ Alligatora И свеча не касается алигатора
			// Если не пустой низ Бар-Фрактала выше Алигатора  и новый - Назначаем рабочим (fr_all_Up) для Buy
			
			  if (frUp>0 && frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5 && _frInd.TopSeries[Bars.Range.To-5]>0)   
			     { fr_all_Up=frUp; fr_all_Up_Time=frUp_Time; fBay=true;  
					 
				        
			Tools.Remove(hline); 
					 Print("Buy - {0} -- {1}={2}",frUp_Time,_frInd.TopSeries[Bars.Range.To-5],frUp);
			hline = Tools.Create<HorizontalLine>();
            hline.Price = frUp;
			hline.Text="Buy - "+frUp_Time;				 
				 }
				 //Print("ВИШЕ Alligatora - {0}>{1}",frUp,aGuba5);
				 //Print("BUY={0} === {1}>{2} && {1}>{3} =========================================", fBay,Bars[Bars.Range.To-5].Low,aGuba5,aChelust5);
				 // Если фрактал касается Алигатора - отменяем сигнал на покупку
				 //}  else { if(frUp>0 && fBay) fBay=false;  Tools.Remove(vlineR); Tools.Remove(hline);}
				 
			// Fractal НИЖЕ Alligatora И свеча не касается алигатора
		    // Если не пустая верхушка  Бар-Фрактала ниже Алигатора  и новый - Назначаем рабочим (fr_all_Down) для Sell
				 //Print("НИЖЕ Alligatora - {0}<{1} && {1}>0",frDown,aGuba5);
			if (frDown>0 && frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5 && _frInd.BottomSeries[Bars.Range.To-5]>0) 
			     { fr_all_Down=frDown;  fr_all_Down_Time=frDown_Time; fSell=true; 
					 
					 			  
				Tools.Remove(lline);  
					 Print("Sell - {0} -- {1}={2}",frDown_Time,_frInd.BottomSeries[Bars.Range.To-5],frDown);
			lline = Tools.Create<HorizontalLine>();
            lline.Price = fr_all_Down;
			lline.Text="Sell = "+fr_all_Down_Time;
				 }
					 
				// Print("SELL={0} === {1}<{2} && {1}<{3}  ============================================",fSell,Bars[Bars.Range.To-5].High,aGuba5,aChelust5);
			    // Если фрактал касается Алигатора - отменяем сигнал на покупку
//				 } else { if(frDown>0 && fSell) fSell=false; Tools.Remove(vlineB); Tools.Remove(lline); }
				 
				 
				 // Если есть сигнал на покупку и нет открытых ордеров - рисуем красную - ЕТО РАБОЧИЙ БАР НА ПОКУПКУ
				 //if (fBay && _positionGuid==Guid.Empty ) { Red(); Sig=1; }
				 
				 // Если текущий бар пересек HIGH РАБОЧЕГО Fractala  - Открываем ордер Buy 
				 //Print("{0} = {1} && {2} && {3} < {4} ",(fBay && !pBay && fr_all_Up<Bars[Bars.Range.To-1].High && _positionGuid==Guid.Empty),fBay,!pBay,fr_all_Up,Bars[Bars.Range.To-1].High);
				 if(fBay && !pBay && fr_all_Up<Bars[Bars.Range.To-1].High && _positionGuid==Guid.Empty) 
				          { Sell();  Print("BUY ========================================"); pBay=true; fBay=false; }
				 //Print("fSell-{0} && {1}==0",fSell,_positionGuid);
				 
				   // Если есть сигнал на покупку и нет открытых ордеров - рисуем синюю - ЕТО РАБОЧИЙ БАР НА ПРОДАЖУ
//				  if (fSell && _positionGuid==Guid.Empty) { Blue(); }
				   // Если бар пересек низ РАБОЧЕГО  - Открываем Селл
				  //Print("{0} = {1} && {2} && {3} > {4} ",(fBay && pBay && Bars[Bars.Range.To-1].Low<aZub && _positionGuid==Guid.Empty),fSell,!pSell,fr_all_Down,Bars[Bars.Range.To-1].Low);
				  if(fSell && !pSell && fr_all_Down>Bars[Bars.Range.To-1].Low  && _positionGuid==Guid.Empty)  
				           { Buy();  Print("SELL ========================================"); pSell=true;  fSell=false; }
				 
				 // Закрываем ордер на покупку когда свеча каснется Зубов Alligator		   
				 if(pBay && Bars[Bars.Range.To-1].Low<aZub && _positionGuid!=Guid.Empty) { ClosePosition();  pBay=false;}
				 // Закрываем ордер когда прибыль > 150п
//				 if(fBay && (Bars[Bars.Range.To-1].Close-OpenPrice)>0.0015 && Sig>0 && _positionGuid!=Guid.Empty) { ClosePosition(); fBay=false; }
				 
				 //Print("{0}<0 && {1}!=0 && {2}>{3}",Sig,_positionGuid,Bars[Bars.Range.To-1].High,aChelust5) ;
				 // Закрываем ордер на продажу когда свеча каснется Зубов	
				 if(pSell && Bars[Bars.Range.To-1].High>aZub && _positionGuid!=Guid.Empty) { ClosePosition();  pSell=false; }
				 // Закрываем ордер когда прибыль > 150п
//			     if(fSell && (OpenPrice-Bars[Bars.Range.To-1].Close)>0.0015 && Sig<0 && _positionGuid!=Guid.Empty) { ClosePosition(); fSell=false; }
				 //Print("OK! ===================================================================================")
				 
				 Print("_positionGuid = {0}",(_positionGuid!=Guid.Empty));
				 }
				 
		
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
		private void Red()
		{
		    vlineR = Tools.Create<VerticalLine>();
            vlineR.Color=Color.Red;
		    vlineR.Time=fr_all_Up_Time;	
		}
		
		private void Red5()
		{
		    vlineR = Tools.Create<VerticalLine>();
            vlineR.Color=Color.Red;
		    vlineR.Time=fr_all_Up_Time;	
		}
		
		private void Blue()
		{
		    vlineB = Tools.Create<VerticalLine>();
            vlineB.Color=Color.Blue;
		    vlineB.Time=fr_all_Down_Time;	
		}
		
		private void RedH()
		{
            hline = Tools.Create<HorizontalLine>();
            hline.Price = Bars[Bars.Range.To-1].High;
			hline.Text="Buy = "+Bars[Bars.Range.To-1].High;
			
		}
		
		private void RedL()
		{
            lline = Tools.Create<HorizontalLine>();
            lline.Price = Bars[Bars.Range.To-1].Low;
			lline.Text="Sell = "+Bars[Bars.Range.To-1].Low;
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
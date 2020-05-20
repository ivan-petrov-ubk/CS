//  http://tradelikeapro.ru/strategiya-spokoynaya-reka/#more-17658
// Стратегия «Спокойная река» — скальпинг без суеты
// Таймфрейм - М5
// Инструменти : две МА 20 и 50 - Expotencial
// Условия входа :
// 1. Цена должна быть ниже/выше всех скользящих
// 2. МА20 должна быть ниже/выше М50
// 3. Средние не должны часто пересекаться (Не понятно?)
// 4. МА имеют явное направление (?)
// 5. ждем коррекции цены до 20 или 50 средней. 
// 6. не должно быть больше 3 закрытий подряд между двумя средними
//     после третьей свечи, цена должна закрыться за 20 средней
// 7. на закрытии данной свечи входим в сделку.
// 8. Стоп лосс ставиться за локальным экстремумом (Fractalom)

//Вход на покупку:
//
//    Цена выше 20-й и 50-й EMA.
//    20-я EMA выше 50-й.
//    Обе средние направлены вверх.(Угол МА > 30гр)
//    Средние не должны пересекаться часто. 20-я EMA выше 50-й .
//    Цена корректируется и касается 20-й или 50-й EMA.
//    Нет более 3 подряд закрытых свечей ниже 20-й EMA.
//    Свеча закрывается выше 20-й EMA – сигнал на вход.



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
		private MovingAverage _ma20;
		private MovingAverage _ma50;
		private int _maPeriod20 = 20;
		private int _maPeriod50 = 50;
		private int _maShift = 0;
		public VerticalLine vl ;
		private MaMethods _maMethod = MaMethods.Ema;
		private PriceMode _priceMode = PriceMode.Close;

		private Guid _positionGuid=Guid.Empty;
		
        double SMa20;
		double SMa50;
		
		double SMa20_1;
		double SMa50_1;
		
		double SMa20_2;
		double SMa50_2;
		
		double speed;
		double speed1;
		int i=0;
		int i1=0;
		// Сигналы 
		bool MAHigh=false; // МА20 Выше МА50
		bool Tr1=false;
		bool Vl=true;
		bool Vl1=true;
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
           
				    _ma20 = GetIndicator<MovingAverage>(Instrument.Id, Timeframe, _maPeriod20, _maShift, _maMethod, _priceMode);
					_ma50 = GetIndicator<MovingAverage>(Instrument.Id, Timeframe, _maPeriod50, _maShift, _maMethod, _priceMode);
			
			
			        
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
			
            // Event occurs on every new bar
			// Значения Alligator около фрактала
			SMa20 = _ma20.SeriesMa[Bars.Range.To-1];
            SMa50 = _ma50.SeriesMa[Bars.Range.To-1];
			
			SMa20_1 = _ma20.SeriesMa[Bars.Range.To-2];
            SMa50_1 = _ma50.SeriesMa[Bars.Range.To-2];

			SMa20_2 = _ma20.SeriesMa[Bars.Range.To-3];
            SMa50_2 = _ma50.SeriesMa[Bars.Range.To-3];
			
			
			// Определяем МА20 Выше/Ниже  МА50
			if(SMa20>SMa50) MAHigh=true; else MAHigh=false;
			
			if (MAHigh) {  
			//speed = ((SMa20-SMa50)+(SMa20_1-SMa50_1)+(SMa20_2-SMa50_2))/3;
				//speed = (((SMa20_2-SMa50_2)-(SMa20_1-SMa50_1))-((SMa20_1-SMa50_1)-(SMa20-SMa50)));
				speed = ((SMa20_1-SMa50_1)-(SMa20-SMa50))*100000;
				//Print("ВВЕРХ speed={0} - {1}",speed,Bars[Bars.Range.To-1].Time);
            //if (speed> 0.001)
		    //if ( ( (SMa20_1-SMa50_1)-(SMa20-SMa50) )<( (SMa20_2-SMa50_2)-(SMa20_1-SMa50_1) ) )
            if ( speed > 0)
			{   i=0;  Vl=false;	
			vl = Tools.Create<VerticalLine>();
            vl.Time=Bars[Bars.Range.To-1].Time;
			vl.Color=Color.Red;			
			if(_positionGuid!=Guid.Empty) { ClosePosition(); }	
			} else 
			{
			vl = Tools.Create<VerticalLine>();
            vl.Time=Bars[Bars.Range.To-1].Time;
			vl.Color=Color.Blue;		
				
				Print("Buy Vol={0} i={1} Vl={2} - {3}",Bars[Bars.Range.To-1].TickCount,i,Vl,Bars[Bars.Range.To-1].Time);
				if(Vl && i==1 && (Bars[Bars.Range.To-1].Open<Bars[Bars.Range.To-1].Close) && _positionGuid==Guid.Empty) Buy();
				i++;
				if(Bars[Bars.Range.To-1].TickCount>100 && !Vl) { Vl=true; };
			}
				
			speed=0;	
				
				//Print("speed={0} - 20-{1} 50-{2}",speed,speed20,speed50);
			}   else
			{
				// speed = ((SMa50-SMa20)+(SMa50_1-SMa20_1)+(SMa50_2-SMa20_2))/3;
				//speed = (((SMa50_1-SMa20_1)-(SMa50-SMa20))-((SMa50_2-SMa20_2)-(SMa50_1-SMa20_1)));
				speed = ((SMa50_1-SMa20_1)-(SMa50-SMa20))*100000;
				//Print("ВНИЗ speed={0} - {1}",speed,Bars[Bars.Range.To-1].Time);
            //if (speed > 0.001)
			//if ( ( (SMa50_2-SMa20_2)-(SMa50_1-SMa20_1) ) > ( (SMa50_1-SMa20_1)-(SMa50-SMa20) ) )
            if ( speed > 0)
			{   i1=0; Vl1=true;
			vl = Tools.Create<VerticalLine>();
            vl.Time=Bars[Bars.Range.To-1].Time;
			vl.Color=Color.Gold;
			if(_positionGuid!=Guid.Empty) { ClosePosition();}
			i1++;
			} else 
			{
			vl = Tools.Create<VerticalLine>();
            vl.Time=Bars[Bars.Range.To-1].Time;
			vl.Color=Color.SeaGreen;	
				Print("Sell Vol={0} i1={1} Vl1={2} - {3}",Bars[Bars.Range.To-1].TickCount,i1,Vl1,Bars[Bars.Range.To-1].Time);
				
				if(Vl1 && i1==1 && (Bars[Bars.Range.To-1].Open>Bars[Bars.Range.To-1].Close) && _positionGuid==Guid.Empty)  Sell();
				i++;
				if(Bars[Bars.Range.To-1].TickCount>100 && !Vl1) { Vl1=true; };
				
				//if(Bars[Bars.Range.To-1].TickCount<100) Vl1=false;
				//if(_positionGuid==Guid.Empty && Vl) Sell();
				//i++; i1=0;
			}
				
			speed=0;	
			}
				//Print("MAHigh={0} - {1}",MAHigh,Bars[Bars.Range.To-1].Time);
			
			// Градус наклона МА
			
			
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
    }
}
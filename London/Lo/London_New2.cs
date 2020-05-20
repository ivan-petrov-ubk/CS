// Автор Рымарь Виктор
// E-Mail: rymar@ukr.net
// Skype: rymar_victor
// ТС разарботана по инструкции с сайта :
// http://strategy4you.ru/prostaya-strategiya-foreks/london-explosion.html
// https://www.youtube.com/watch?v=ASPZiVuTD_A
// 1) Ценовой диапазон (от max до min) азиатской сессии должен быть не более 600 пунктов.
//    (23:00  -  9:00 GMT)
// 2) Закрытие Азиатской сессии (9:00) должно произойти на расстоянии больше 50 пунктов от минимума диапазона. 
//   В противном случае не торгуем в этот день вообще.
// 3) Нижняя граница азиатской сессии (желтый прямоугольник) находится ниже скользящей средней. 
//   Азиатская сессия должна полностью завершиться.
// 4) Ниже минимума Азиатской сессии (желтого прямоугольника) на 20 пункта устанавливается отложенный ордер Sell Stop.
// 5) Если ордер не открылся в течении всей лондонской сессии (синий прямоугольник), то его следует удалить.
// 6) Страховочный Stop loss устанавливается выше середины ценового диапазона внутри азиатской сессии, но не меньше чем 20 пунктов.
//   После прохождения ценой в положительной зоне расстояния, равного всему ценовому диапазону азиатской сессии сделка 
//   переводится в уровень безубытка.
// 7) Ордер фиксации прибыли — Take profit устанавливается на расстоянии двух размеров азиатской сессии, отложенном 
//   от минимума азиатской сессии.
//   Если через два часа (конец розовой зоны) после завершения лондонской сессии сделка еще не закрыта по стопу или профиту, 
//   то её следует при любом текущем результате закрыть по рыночной цене.
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
    [TradeSystem("London_New2")]  //copy of "London_"
    public class London1 : TradeSystem
    {
        // Simple parameter example
        private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public DateTime DTime; // Время
		
		public double x;
		public int xp;
				private MovingAverage _ma1;	
		// MA1
		private int _maPeriod1 = 360;
        private int _maShift1 = 0;
        private MaMethods _maMethod1 = MaMethods.Ema;
        private PriceMode _priceMode1 = PriceMode.Close;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            _ma1 = GetIndicator<MovingAverage>(Instrument.Id,Timeframe,_maPeriod1, _maShift1, _maMethod1, _priceMode1);
			
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
	// =================================================================================================================================				 
			DTime = Bars[Bars.Range.To-1].Time;
			// Event occurs on every new bar
			// Ценовой диапазон (от max до min) азиатской сессии должен быть не более 600 пунктов.(23:00-9:00 GMT)
          if ( DTime.Hour==08 && DTime.Minute==0 ) 
          {  // 1. Измеряем высоту диапазона в промежутке с 9 до 23 часов по GMT.
				var highestIndex = Series.Highest(Bars.Range.To, 10, PriceMode.High);
     			var highestPrice = Bars[highestIndex].High;
		    	var lowestIndex = Series.Lowest(Bars.Range.To, 10, PriceMode.Low);
     			var lowestPrice = Bars[lowestIndex].Low;
			      
			
			   x=Math.Round(((highestPrice-lowestPrice)/2),5);
		       xp=(int)(x*100000);
			  // 1) Ценовой диапазон (от max до min) азиатской сессии должен быть не более 600 пунктов.
			  // 2) Закрытие Азиатской сессии должно произойти на расстоянии больше 50 пунктов от минимума диапазона. 
			  //    В противном случае не торгуем в этот день вообще.
			  if(xp<320 && (Instrument.Bid-lowestPrice)>0.0005) {
			  // 3) Верхняя граница азиатской сессии (желтый цвет) находится выше скользящей средней. 
			  //	  Азиатская сессия должна полностью завершиться.
			  if (highestPrice>_ma1.SeriesMa[Bars.Range.To-1]) {
			   // 4) Выше максимума Азиатской сессии на 20+спред пункта устанавливается ордер Buy Stop.
				var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyStop, 0.1, highestPrice+Instrument.Spread+0.00020, 0, Stops.InPips(xp+20,4*xp), null, null, null);
						 if (result.IsSuccessful)  posGuidBuy=result.Position.Id; 
			  }
			  if (lowestPrice<_ma1.SeriesMa[Bars.Range.To-1]) {
		       var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellStop, 0.1,  lowestPrice-Instrument.Spread-0.00020, 0, Stops.InPips(xp+20,4*xp), null, null, null);
						 if (result1.IsSuccessful)  posGuidSell=result1.Position.Id; }
			  }
		  }
		  
		  		  //=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) 
			     {	posGuidBuy=Guid.Empty; Print("09 - Buy - Закрыто по StopLoss (Корекция) - {0} ",DTime);  }
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) 
				{ posGuidSell=Guid.Empty;  Print("10 -Sell - Закрыто по StopLoss (Корекция) - {0} ",DTime); }
		  
		  // 4. На следующий день около 9:00 по Гринвичу удаляем и закрываем все ордера независимо от прибыли / убытка.
		  if ( DTime.Hour==15 && DTime.Minute==00 ) 
		  {    
			    if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			       					{   var res = Trade.CancelPendingPosition(posGuidBuy);
			     	   					if (res.IsSuccessful) { posGuidBuy = Guid.Empty; }
				   					}
				if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
				   				{  var result2=Trade.CloseMarketPosition(posGuidBuy); 
				   				   if (result2.IsSuccessful) {  posGuidBuy = result2.Position.Id; }  
			       				}		
								
				if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			       					{   var res1 = Trade.CancelPendingPosition(posGuidSell);
			     	   					if (res1.IsSuccessful) { posGuidSell = Guid.Empty; }
				   					}
				if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
				   				{  var result3=Trade.CloseMarketPosition(posGuidSell); 
				   				   if (result3.IsSuccessful) {  posGuidBuy = result3.Position.Id; }  
			       				}
		  }



// 6. Важно ! После активации одного ордера, необходимо удалить второй.
			//	if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			//	{
			//		var res = Trade.CancelPendingPosition(posGuidBuy);
			  //   	if (res.IsSuccessful) posGuidBuy = Guid.Empty; 
				//}

			//	if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			//	{
			//		var res = Trade.CancelPendingPosition(posGuidSell);
			  //   	if (res.IsSuccessful)  posGuidSell = Guid.Empty; 
				//}
				
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
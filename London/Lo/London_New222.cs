// Автор Рымарь Виктор
// E-Mail: rymar@ukr.net
// Skype: rymar_victor
// ТС разарботана по инструкции с сайта :
// http://tradelikeapro.ru/strategiya-londonskiy-vzryiv/
// https://www.youtube.com/watch?v=t4CcW6pBH2A
// Валютные пары: EURUSD, GBPUSD, USDJPY
// 1. Измеряем высоту диапазона в промежутке с 9 до 18 часов по GMT. Ее размер в пунктах равен 2Х.
// 2. После 18:00 выставляем 2 ордера: Buy Limit и Sell Limit на расстоянии Х пунктов от текущей цены.
// 3. Тейк-профит каждого ордера равен 2Х, стоп-лосс = X+5 пунктов.
// 4. На следующий день около 9:00 по Гринвичу удаляем и закрываем все ордера независимо от прибыли / убытка.
// 5. Если диапазон с 9 до 18 часов равен или превышает 600 пунктов, — НЕ ТОРГУЕМ.

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
    [TradeSystem("London_New")] 
    public class London1 : TradeSystem
    {
        // Simple parameter example
        private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public DateTime DTime; // Время
		
		public double x;
		public int xp;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
           
			
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
          if ( DTime.Hour==18 && DTime.Minute==0 ) 
          {  // 1. Измеряем высоту диапазона в промежутке с 9 до 18 часов по GMT.
				var highestIndex = Series.Highest(Bars.Range.To, 9, PriceMode.High);
     			var highestPrice = Bars[highestIndex].High;
		    	var lowestIndex = Series.Lowest(Bars.Range.To, 9, PriceMode.Low);
     			var lowestPrice = Bars[lowestIndex].Low;
			      
			 // 2. После 18:00 выставляем 2 ордера: Buy Limit и Sell Limit на расстоянии Х пунктов от текущей цены.
			   x=Math.Round(((highestPrice-lowestPrice)/2),5);
		       xp=(int)(x*100000);
			  // 5. Если диапазон с 9 до 18 часов равен или превышает 600 пунктов, — НЕ ТОРГУЕМ.
			  if(xp<300) {
			  // 3. Тейк-профит каждого ордера равен 2Х, стоп-лосс = X+5 пунктов.
			   var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, Instrument.Bid-x, 0, Stops.InPips(xp+5,2*xp), null, null, null);
						 if (result.IsSuccessful)  posGuidBuy=result.Position.Id; 
			   var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  Instrument.Ask+x, 0, Stops.InPips(xp+5,2*xp), null, null, null);
						 if (result1.IsSuccessful)  posGuidSell=result1.Position.Id; 
			  }
		  }
		  // 4. На следующий день около 9:00 по Гринвичу удаляем и закрываем все ордера независимо от прибыли / убытка.
		  if ( DTime.Hour==9 && DTime.Minute==00 ) 
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

		  //=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) 
			     {	posGuidBuy=Guid.Empty; Print("09 - Buy - Закрыто по StopLoss (Корекция) - {0} ",DTime);  }
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) 
				{ posGuidSell=Guid.Empty;  Print("10 -Sell - Закрыто по StopLoss (Корекция) - {0} ",DTime); }

// 6. Важно ! После активации одного ордера, необходимо удалить второй.
				if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
				{
					var res = Trade.CancelPendingPosition(posGuidBuy);
			     	if (res.IsSuccessful) posGuidBuy = Guid.Empty; 
				}

				if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
				{
					var res = Trade.CancelPendingPosition(posGuidSell);
			     	if (res.IsSuccessful)  posGuidSell = Guid.Empty; 
				}
				
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
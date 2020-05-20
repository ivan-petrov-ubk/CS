//  http://strategy4you.ru/prostaya-strategiya-foreks/strategy-forex-trend-reversal.html
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
    [TradeSystem("Trend Reversal_Down")]    //copy of "Trend Reversal_Up"
    public class Trend_Reversal : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
       private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public DateTime DTime; // Время
		public FisherTransformOscillator _ftoInd;
		public double x;
		public int xp;

        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			_ftoInd= GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			DTime = Bars[Bars.Range.To-1].Time;
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  


			
		if ( DTime.Hour==0 && DTime.Minute==0 ) 
          {  // 1. Измеряем высоту диапазона в промежутке с 9 до 18 часов по GMT.
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) {
										var res1 = Trade.CancelPendingPosition(posGuidBuy);
			     	   					if (res1.IsSuccessful) { posGuidBuy = Guid.Empty; } }
			
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) {
										var res1 = Trade.CancelPendingPosition(posGuidSell);
			     	   					if (res1.IsSuccessful) { posGuidSell = Guid.Empty; } } 
			  
			  
				var highestIndex = Series.Highest(Bars.Range.To, 24, PriceMode.High);
     			var highestPrice = Bars[highestIndex].High;
		    	var lowestIndex = Series.Lowest(Bars.Range.To, 24, PriceMode.Low);
     			var lowestPrice = Bars[lowestIndex].Low;
			      
			 // 2. После 18:00 выставляем 2 ордера: Buy Limit и Sell Limit на расстоянии Х пунктов от текущей цены.
			if(Instrument.Name.EndsWith("JPY")) {
			   x=Math.Round((highestPrice-lowestPrice),3);
		       xp=(int)(x*1000); }  else 
		       {
			   x=Math.Round((highestPrice-lowestPrice),5);
		       xp=(int)(x*100000); }  
			  // 5. Если диапазон с 9 до 18 часов равен или превышает 600 пунктов, — НЕ ТОРГУЕМ.
			  if(xp>400) {
				  if(Bars[Bars.Range.To-24].Open<Bars[Bars.Range.To-1].Close) //Вниз
				  {
				    if (posGuidBuy==Guid.Empty)    {	  
	   var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1, highestPrice, 0, Stops.InPips(200,200), null, null, null);
				 if (result.IsSuccessful)  posGuidBuy=result.Position.Id; 
				    var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Blue; }
				  }
				  
			 if(Bars[Bars.Range.To-24].Open<Bars[Bars.Range.To-1].Close) //Вверх
				  {	
					  if (posGuidSell==Guid.Empty)   {
//var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1,  lowestPrice, 0, Stops.InPips(200,200), null, null, null);
//					if (result1.IsSuccessful)  posGuidSell=result1.Position.Id;
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Red; }
				  }
			  }
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
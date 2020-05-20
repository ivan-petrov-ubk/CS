
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
    [TradeSystem("W1-Откат")]
    public class W1Откат : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		// Simple parameter example
		public Alligator _allInd;
		public Fractals _frInd;
		
		private Guid _positionGuid=Guid.Empty;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		double BarH,BarL,BarC; 
		public DateTime DTime;
		private VerticalLine vlR,vlB;
		public double x;
		public int xp;
		public HorizontalLine hline;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			// Вставить индикатор Alligator
			_allInd = GetIndicator<Alligator>(Instrument.Id, Timeframe);
			// Вставить индикатор Fractals
		    _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			
			vlR = Tools.Create<VerticalLine>();
			vlR.Color=Color.Red;
			vlB = Tools.Create<VerticalLine>();
			vlB.Color=Color.Blue;
			hline = Tools.Create<HorizontalLine>();
			hline.Price= Bars[Bars.Range.To-1].Close;
			
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			// Значения текущего Бара
			BarH  = Bars[Bars.Range.To-1].High;
			BarL  = Bars[Bars.Range.To-1].Low;
			BarC  = Bars[Bars.Range.To-1].Close;
			DTime = Bars[Bars.Range.To-1].Time;
			
//			Print("{0} - {1}",DTime.DayOfWeek,DTime.Day);
		  if ( DTime.DayOfWeek==DayOfWeek.Monday && DTime.Hour==1 && DTime.Minute==0 ) 
          {  // 1. Измеряем высоту диапазона в промежутке с 9 до 18 часов по GMT.
				var highestIndex = Series.Highest(Bars.Range.To, 120, PriceMode.High);
     			var highestPrice = Bars[highestIndex].High;
		    	var lowestIndex  = Series.Lowest(Bars.Range.To, 120, PriceMode.Low);
     			var lowestPrice  = Bars[lowestIndex].Low;
		    	var openIndex    = Series.Lowest(Bars.Range.To, 120, PriceMode.Open);
     			var openPrice    = Bars[lowestIndex].Open;
		    	var closeIndex   = Series.Lowest(Bars.Range.To, 120, PriceMode.Close);
     			var closePrice   = Bars[lowestIndex].Close;
			var vlY = Tools.Create<VerticalLine>();
			vlY.Color=Color.Yellow;
			vlY.Time=Bars[Bars.Range.To-2].Time;
			  
			 // 2. После 18:00 выставляем 2 ордера: Buy Limit и Sell Limit на расстоянии Х пунктов от текущей цены.
			   x=Math.Round((highestPrice-lowestPrice),5);
		       xp=(int)(x*100000);
			  if(openPrice>closePrice) 
			 	Print("{0} {1} - {2} - {3}  {4} - ВНИЗ - торг вверх! Красний",DTime.DayOfWeek,DTime.Day,xp,openPrice,closePrice);
			  else
				Print("{0} {1} - {2} - {3}  {4} - ВВЕРХ - торг вниз! Синий",DTime.DayOfWeek,DTime.Day,xp,openPrice,closePrice);
 			  
	   if(xp>2000 && openPrice>closePrice) {
			var vlR = Tools.Create<VerticalLine>();
			vlR.Color=Color.Red;
			vlR.Time=DTime; 
     		   hline.Price=BarC+0.05;
		   		var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid,-1,Stops.InPips(200,500),null);
				if (result.IsSuccessful) posGuidBuy=result.Position.Id;  
			  }
	   
		if(xp>2000 && openPrice<closePrice) { 
			var vlB = Tools.Create<VerticalLine>();
			vlB.Color=Color.Blue;
			vlB.Time=DTime; 	
			hline.Price=BarC-0.05;
		   		var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask,-1,Stops.InPips(200,500),null);
				if (result.IsSuccessful) posGuidSell=result.Position.Id;  
			
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
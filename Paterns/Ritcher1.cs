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
using System.IO;


namespace IPro.TradeSystems
{
    [TradeSystem("Ritcher1")]
    public class Ritcher1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
        // Simple parameter example
        private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public DateTime DTime; // Время
		public HorizontalLine Line1,Line2,Line3;
		public double H1,L1,O1,C1,H2,L2,O2,C2,O3,C3;
		public ArrowDown ad;
		public ArrowUp au;
		public int tr=0;
		private VerticalLine vlR,vlB;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			 Line1 = Tools.Create<HorizontalLine>();
			 Line2 = Tools.Create<HorizontalLine>();
			 Line3 = Tools.Create<HorizontalLine>();
			 ad = Tools.Create<ArrowDown>();
			 au = Tools.Create<ArrowUp>();
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
			O1 = Bars[Bars.Range.To-1].Open;
			C1 = Bars[Bars.Range.To-1].Close;
			
			// Значения пред Бара
			O2 = Bars[Bars.Range.To-2].Open;
			C2 = Bars[Bars.Range.To-2].Close;

			O3 = Bars[Bars.Range.To-3].Open;
			C3 = Bars[Bars.Range.To-3].Close;			
			
			DTime = Bars[Bars.Range.To-1].Time;
			// Event occurs on every new bar
          if ( DTime.Hour==00 && DTime.Minute==00 ) 
          {  // 1. Измеряем высоту диапазона в промежутке с 9 до 18 часов по GMT.
            // Event occurs on every new bar 
			    var vW = Tools.Create<VerticalLine>();
			    vW.Color=Color.White;
				vW.Time = Bars[Bars.Range.To-1].Time; 	
	    		  
	    		   tr=0; 
			      H2 = H1;
     			  L2 = L1;

			  
			    var highestIndex = Series.Highest(Bars.Range.To, 24, PriceMode.High);
     			var highestPrice = Bars[highestIndex].High;
			     H1 = highestPrice;
		    	var lowestIndex  = Series.Lowest(Bars.Range.To, 24, PriceMode.Low);
			    var lowestPrice = Bars[lowestIndex].Low;
			     L1 = lowestPrice;
     			 
				      Line1.Price = highestPrice;
					  Line2.Price = lowestPrice;
			  
	          // Тренд ВВЕРХ  L1
		    if ( H1>H2 && L1>L2 ) { tr = 1;
			  var ad1 = Tools.Create<ArrowDown>();
			 // ad1.Point=new ChartPoint(Bars[Bars.Range.To-1].Time, L1); 
				}	
					  // Тренд ВНИЗ	  H1
		  if ( H1<H2 && L1<L2 && O2<H1 ) { tr = 2;
			  var au1 = Tools.Create<ArrowUp>();
			  //au1.Point=new ChartPoint(Bars[Bars.Range.To-1].Time, H1);
		  }
			  //Vverh
			  if(H2>L1 && H2<H1 && L2<L1 ) { 
				  Line3.Price = H2; 
			     au.Point=new ChartPoint(Bars[Bars.Range.To-1].Time, H2);
			  }     
			  
			  //Vniz
			  if(L2>L1 && L2<H1 && H2>H1 )  { 
				  Line3.Price = L2;
			     ad.Point=new ChartPoint(Bars[Bars.Range.To-1].Time, L2);
			  }
			  
     	  }
          
// ==========================================================================================================================
/* //  Обработака уровня 3 - пробой против тренда!!!!		  
		  // Тренд ВВЕРХ  L1
		  Print("tr={0} O1={1} C1={2} -- {3}",tr, O1,C1,Bars[Bars.Range.To-1].Time);
		  
		//if ( DTime.Hour>4 && DTime.Hour<18 ) {  
		    if ( tr==1 && O2>L1 && C2<L1 && C1<L1  ) {  Print("ВВЕРХ - {0} ",Bars[Bars.Range.To-1].Time);
			var vR = Tools.Create<VerticalLine>();
			    vR.Color=Color.Red;
				vR.Time = Bars[Bars.Range.To-1].Time;
				Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Bid, -1, Stops.InPips(200,100), null, null);
             }
		  // Тренд ВНИЗ	  H1
		  if ( tr==2 && O2<H1 && C2>H1 && C1>H1 ) { Print("ВНИЗ - {0} ",Bars[Bars.Range.To-1].Time);
			  
			var vB = Tools.Create<VerticalLine>();
			    vB.Color=Color.Blue;
			  vB.Time = Bars[Bars.Range.To-1].Time;
			  Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Ask, -1, Stops.InPips(200,100), null, null);
		  //}
		}
*/		  
// =====================================================================================================================		  
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
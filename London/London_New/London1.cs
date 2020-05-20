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
    [TradeSystem("London1")]
    public class London1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		
		double BarH,BarL,BarC,BarO,Rez,Rez1;
		double BarH1,BarL1,BarC1,BarO1;
		double BarH2,BarL2,BarC2,BarO2;
		public DateTime DTime; // Время
		public DateTime DTime1; // Время
		private ISeries _barSeries;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			_barSeries = GetCustomSeries(Instrument.Id, Period.H1);
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
			// =================================================================================================================================				 
					// Значения текущего Бара
			BarH = Bars[Bars.Range.To-1].High;
			BarL = Bars[Bars.Range.To-1].Low;
			BarC = Bars[Bars.Range.To-1].Close;
			BarO = Bars[Bars.Range.To-1].Open;
			DTime = Bars[Bars.Range.To-1].Time;
			// Event occurs on every new bar
		  Print("Hour={0} - Min={1}",DTime.Hour,DTime.Minute);
          if ( DTime.Hour==5 && DTime.Minute==0 ) {
			  Print(" 1 ==========================================================================================");
		  var series = GetCustomSeries(Instrument.Id, Period.H1,6);
         Print(" 2 ==========================================================================================");
			  var highestIndex = Series.Highest(series, series.Range.To, 5, PriceMode.High);
          Print(" 3 ==========================================================================================");
			  var highestPrice = _barSeries[highestIndex].High;
Print(" 4 ==========================================================================================");
		  var series2 = GetCustomSeries(Instrument.Id, Period.H1);
Print(" 5 ==========================================================================================");			  
          var lowestIndex = Series.Lowest(series2, series2.Range.To, 5, PriceMode.Low);
	Print(" 6 ==========================================================================================");		  
          //var lowestPrice = series[lowestIndex].Low;
	  			  
			//Print("{0} - {1} ===================================================================================",highestPrice,lowestPrice);
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
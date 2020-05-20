using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;

namespace IPro.TradeSystems
{
    [TradeSystem("Grafics1")]
    public class Grafics1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
        int Count=1;
		private VerticalLine vline;
		private HorizontalLine hline;
		
        private bool firstBar=true;
        protected override void NewBar()
        {
				
//Графический инструмент «Линия тренда»
			if (firstBar)
   		 {
	     var toolTrendLine = Tools.Create<TrendLine>();
         toolTrendLine.Point1= new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].Low);
         toolTrendLine.Point2= new ChartPoint(Bars[Bars.Range.To-10].Time, Bars[Bars.Range.To-10].High);
  			firstBar=false;
		 } 						
        }

    }
}
			
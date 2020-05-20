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
    [TradeSystem("Rect1")]
    public class Rect1 : TradeSystem
    {
		[Parameter("Время :", DefaultValue = "07.11.2016 0:00:00")]
        public DateTime dt1 { get; set; }
		
        public bool firstBar;
        protected override void Init()
        {
			firstBar=true;
        }        
        
        protected override void NewBar()
        {
			     if (firstBar)
     {
        var toolRectangle = Tools.Create<Rectangle>();
	    toolRectangle.BorderColor=Color.Red;
	    toolRectangle.BorderWidth = 3;
	    toolRectangle.BorderStyle =LineDashStyle.Dash;
         toolRectangle.Point1=new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High);
         toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(35), Bars[Bars.Range.To-1].High+Instrument.Point*30);
          firstBar=false;
     }  
        }

    }
}
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
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        private bool firstBar=true;
        protected override void NewBar()
        {
/*			
//Вертикальная линия
			if (Count==1) 
			{
				vline = Tools.Create<VerticalLine>();
            	vline.Color=Color.Red;
		    	vline.Time=Bars[Bars.Range.To-1].Time;	
//Горизонтальная линия			
				hline = Tools.Create<HorizontalLine>();
            	hline.Price = Bars[Bars.Range.To-1].Low;
				hline.Text="Цена = "+Bars[Bars.Range.To-1].Low;
			}
			
			Count++;   if(Count==10) { Tools.Remove(vline);  Tools.Remove(hline);}
*/
			
// Горизонтальная линия
/*				var hline = Tools.Create<HorizontalLine>();
			    hline.Price = Bars[Bars.Range.To-1].Low;
                hline.Text="Last bar Low price = "+Bars[Bars.Range.To-1].Low;
*/
			
//Графический инструмент «Ломаная линия»
/*	if (firstBar)
    {   var toolPolyLine = Tools.Create<PolyLine>();
        toolPolyLine.AddPoint(new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High));
        toolPolyLine.AddPoint(new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(10), Bars[Bars.Range.To-1].High));
        toolPolyLine.AddPoint(new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(20), Bars[Bars.Range.To-1].High+Instrument.Point*30));   
      firstBar=false; }       
*/
			
//  Графический инструмент «Прямоугольник»
/*		if (firstBar)
    {     var toolRectangle = Tools.Create<Rectangle>();
		  toolRectangle.BorderColor=Color.Red;
		  toolRectangle.Width = 3;
		  toolRectangle.BorderStyle =LineDashStyle.Dash;
          toolRectangle.Point1=new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High);
          toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(5), Bars[Bars.Range.To-1].High+Instrument.Point*30);
        firstBar=false;  }       
*/	
	
// Трендова линия
/*	if (firstBar)
    {
         var toolTrendLine = Tools.Create<TrendLineRay>();
         toolTrendLine.Point1 = new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].Low);
         toolTrendLine.Point2 = new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(1), Bars[Bars.Range.To-1].High);
		firstBar=false; 
    }
*/
// графический элемент «Текст» 
/*		if (firstBar)
   		 {
        var toolText = Tools.Create<Text>();
		toolText.Color=Color.Blue;	 
		// toolText.FontFamily = new FontFamily("Arial");
		toolText.Style = TextStyle.Italic;			 
		toolText.FontSize=12;	 
        toolText.Point=new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High);
        toolText.Caption=string.Format("Last bar close price = {0}",Bars[Bars.Range.To-1].Close);
		firstBar=false;
		 }
*/	
			
//Графический инструмент «Линия тренда»
/*			if (firstBar)
   		 {
			          var toolTrendLine = Tools.Create<TrendLine>();
         toolTrendLine.Point1= new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].Low);
         toolTrendLine.Point2= new ChartPoint(Bars[Bars.Range.To-10].Time, Bars[Bars.Range.To-10].High);
  			firstBar=false;
		 } 
*/		

// графического инструмента «Луч».
/*			if (firstBar) {		 
		var toolTrendLineRay = Tools.Create<TrendLineRay>();
         toolTrendLineRay.Point1= new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].Low);
         toolTrendLineRay.Point2= new ChartPoint(Bars[Bars.Range.To-10].Time, Bars[Bars.Range.To-10].High);
			firstBar=false;	
			}
*/
// Графический инструмент «Треугольник»
/*			if (firstBar) {		
			        var toolTriangle = Tools.Create<Triangle>();
        toolTriangle.Point1=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(-10), Bars[Bars.Range.To-1].High);
        toolTriangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High+Instrument.Point*30);
        toolTriangle.Point3=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(10), Bars[Bars.Range.To-1].Low-Instrument.Point*30);
        firstBar=false;
			}
*/


			
        }

    }
}
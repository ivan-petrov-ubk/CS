using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;

namespace IPro.Indicators
{
    [Indicator("ex1")]
    public class ex1 : Indicator
    {
  		
        //[Parameter("LR_Ma1Period", DefaultValue = 9, MinValue = 1)]
        //public int Ma1Period { get; set; }
		
		//[Series("LR_Up", Color = Color.LightSeaGreen, Style = SeriesStyle.Histogram)]
        //public IIndicatorSeries UpSeries { get; set; }
		
		//[Series("LR_Ma1", Color = Color.Blue)]
        //public IIndicatorSeries Ma1Series { get; set; }
		
		[Series("Вертикальная", Color = Color.Blue)]
		public VerticalLine toolVerticalLine  { get; set; }
		
		private ISeries<double> _values;
		public int Sr;
		//public VerticalLine toolVerticalLine;
		public HorizontalLine toolHorizLine;
		
		        protected override void Init()
        {
            //_values = CreateSeries<double>();
			Print("TotalPeriod: {0}", TotalPeriod);
			toolVerticalLine = Tools.Create<VerticalLine>();
			 toolHorizLine = Tools.Create<HorizontalLine>();
        }

        protected override void Calculate(int index)
        {
            // calculate
              //  UpSeries[index] = index;
			  // Ma1Series[index] =  index-5;
			//Print("{0}",index);
			if (Sr!=index) { Sr=index;
				if(Bars[Bars.Range.To-1].Time.Hour==5) {
			//var toolVerticalLine = Tools.Create<VerticalLine>();
			toolVerticalLine.Time=Bars[Bars.Range.To-1].Time; 
					toolHorizLine.Price = Bars[Bars.Range.To-1].Low;
					toolHorizLine.Text="Last bar Low price = "+Bars[Bars.Range.To-1].Low;
			Print("{0}",Bars[Bars.Range.To-1].Time); 
					System.Console.Beep(800,800);
				}
			}
        }
    }
}
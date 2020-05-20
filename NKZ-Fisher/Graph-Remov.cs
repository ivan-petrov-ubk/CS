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
    [TradeSystem("Graph-Remov")]
    public class GraphRemov : TradeSystem
    {   
		[Parameter("Номе версии", DefaultValue = 1)]
        public int ver { get; set; }
		
		public TrendLine tl;
		public bool first=true;
		public int k;

        protected override void Init()
        {
			Print("Init var={0}",ver);
			k=1;
			tl = Tools.Create<TrendLine>(); 
			tl.Color=Color.HotPink;
			tl.Width=3;
        }        
    
        protected override void NewBar()
        { 
 tl.Point1= new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(25), Bars[Bars.Range.To-1].High); 
 tl.Point2= new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High); 
			//}
			k++;
			Print("{0} - {1}",Bars[Bars.Range.To-1].Time,(tl!=null));
        }

    }
}
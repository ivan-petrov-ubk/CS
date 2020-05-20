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
    [TradeSystem("Color2")]
    public class Color2 : TradeSystem
    {
		public int k=0;
     
        protected override void NewBar()
        { k++;
			if(k==1) {
	var toolTriangle = Tools.Create<Triangle>();
				toolTriangle.Color=Color.DarkSeaGreen;
				toolTriangle.BorderColor=Color.DarkSeaGreen;
        toolTriangle.Point1=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(-10), Bars[Bars.Range.To-1].High);
        toolTriangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High+Instrument.Point*30);
        toolTriangle.Point3=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(10), Bars[Bars.Range.To-1].Low-Instrument.Point*30);			
			}
/*						if(k==15) {
	var toolTriangle = Tools.Create<Triangle>();
        toolTriangle.Point1=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(-10), Bars[Bars.Range.To-1].High);
        toolTriangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High+Instrument.Point*30);
        toolTriangle.Point3=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(10), Bars[Bars.Range.To-1].Low-Instrument.Point*30);		}
        }
        */
		}
    }
}
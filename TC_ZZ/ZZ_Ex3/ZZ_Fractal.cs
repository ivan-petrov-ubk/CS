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
    [TradeSystem("ZZ_Fractal")]
    public class ZZ_Fractal : TradeSystem
    {
		private ZigZag _wprInd;
		public Fractals _frInd;		

        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=30;
			
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			_frInd.Range=5;
			
        }        


        
        protected override void NewBar()
        {
           			_wprInd.ReInit();
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{	var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
					toolVerticalLine.Color=Color.Red; }
        }
        

    }
}
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
    [TradeSystem("Ichimoku1")]
    public class Ichimoku1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		private Ichimoku _ichInd;
		private double tank1,senA1,senB1,kij1;
		private double tank2,senA2,senB2,kij2;

        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			_ichInd = GetIndicator<Ichimoku>(Instrument.Id, Timeframe);
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {    _ichInd.ReInit();
            // Event occurs on every new bar
			// Print("ChinkouSpan={0} KijunSen={1} _ichInd.KijunSenSeries={2} SecurityExceptionOccurred={3}",_ichInd.ChinkouSpan,_ichInd.KijunSen,_ichInd.KijunSenSeries,_ichInd.SecurityExceptionOccurred);
			// Print("SenkouSpan={0} SenkouSpanASeries={1} SenkouSpanBSeries={2} TankanSen={3}",_ichInd.SenkouSpan,_ichInd.SenkouSpanASeries,_ichInd.SenkouSpanBSeries,_ichInd.TankanSen);
        	// Print("TankanSenSeries={0} -- {1}",_ichInd.TankanSenSeries,Bars[Bars.Range.To-1]);
			//Print("{0} {1} {2} -- {3}",_ichInd.TankanSenSeries[Bars.Range.To-1],_ichInd.SenkouSpanASeries[Bars.Range.To-1],_ichInd.SenkouSpanBSeries[Bars.Range.To-1],Bars[Bars.Range.To-1].Time);
			tank1=_ichInd.TankanSenSeries[Bars.Range.To-1];
			senA1=_ichInd.SenkouSpanASeries[Bars.Range.To-1];
			senB1=_ichInd.SenkouSpanBSeries[Bars.Range.To-1];
			 kij1=_ichInd.KijunSenSeries[Bars.Range.To-1];
			
			
			tank2=_ichInd.TankanSenSeries[Bars.Range.To-2];
			senA2=_ichInd.SenkouSpanASeries[Bars.Range.To-2];
			senB2=_ichInd.SenkouSpanBSeries[Bars.Range.To-2];
			 kij2=_ichInd.KijunSenSeries[Bars.Range.To-2];
			
			if(tank1>=kij1 && kij2>=tank2)
			{
				var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				toolVerticalLine.Color=Color.Red;	
			}
			if(tank1<=kij1 && kij2<=tank2)
			{
				var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				toolVerticalLine.Color=Color.Blue;	
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
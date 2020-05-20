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
    [TradeSystem("Fractal_FIsher0")]
    public class Fractal_FIsher0 : TradeSystem
    {

		public FisherTransformOscillator _ftoInd;
		public Fractals _frInd;
		private ZigZag _wprInd;
				// Fractal
		double frUp = 0.0;
		double frDown = 0.0;
		double frUpL = 0.0;
		double frDownH = 0.0;
		public DateTime frUp_Time;
		public DateTime frDown_Time;
		public DateTime fr_all_Up_Time;
		public DateTime fr_all_Down_Time;
		

        protected override void Init()
        {
			
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			_ftoInd = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=3;
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {		_wprInd.ReInit();
				
			   //Print("{0} - {1}",Bars[Bars.Range.To-1].Time,_ftoInd.FisherSeries[Bars.Range.To-1]);
			
			  if (_frInd.TopSeries[Bars.Range.To-5]>0)    { frUp=Bars[Bars.Range.To-5].High; frUpL=Bars[Bars.Range.To-5].Low; frUp_Time = Bars[Bars.Range.To-5].Time;}
			  if (_frInd.BottomSeries[Bars.Range.To-5]>0) { frDown=Bars[Bars.Range.To-5].Low; frDownH=Bars[Bars.Range.To-5].High; frDown_Time = Bars[Bars.Range.To-5].Time;}
			  // ВВЕРХ
			  if ( _ftoInd.FisherSeries[Bars.Range.To-2]<0 && _ftoInd.FisherSeries[Bars.Range.To-1]>0 && _ftoInd.FisherSeries[Bars.Range.To-1]-_ftoInd.FisherSeries[Bars.Range.To-2]>0.35) 
			  {
				  var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-1].Time; vr.Width=3;
				  
				  Print("UP {0} - {1} - {2} = {3}",Bars[Bars.Range.To-1].Time,_ftoInd.FisherSeries[Bars.Range.To-2],_ftoInd.FisherSeries[Bars.Range.To-1],_ftoInd.FisherSeries[Bars.Range.To-1]-_ftoInd.FisherSeries[Bars.Range.To-2]);
			  }
			  // ВНИЗ
			  if ( _ftoInd.FisherSeries[Bars.Range.To-2]>0 && _ftoInd.FisherSeries[Bars.Range.To-1]<0 && _ftoInd.FisherSeries[Bars.Range.To-2]-_ftoInd.FisherSeries[Bars.Range.To-1]>0.35) 
			  {
				  var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-1].Time; vr.Width=3;
				   Print("DOWN {0} - {1} - {2} = {3}",Bars[Bars.Range.To-1].Time,_ftoInd.FisherSeries[Bars.Range.To-2],_ftoInd.FisherSeries[Bars.Range.To-1],_ftoInd.FisherSeries[Bars.Range.To-2]-_ftoInd.FisherSeries[Bars.Range.To-1]);
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
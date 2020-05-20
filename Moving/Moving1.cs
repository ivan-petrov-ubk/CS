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

namespace IPro.TradeSystems
{
    [TradeSystem("Moving1")]
    public class Moving1 : TradeSystem
    {
        // Simple parameter example		private MovingAverage _ma1;
		private MovingAverage _ma1;		
		private MovingAverage _ma2;
		private double _ma1Value;
		private double _ma2Value;
		private ISeries<Bar> _barSeries;
		private Period _period = new Period(PeriodType.Minute, 1);
		// MA1
		private int _maPeriod1 = 9;
        private int _maShift1 = 0;
        private MaMethods _maMethod1 = MaMethods.Sma;
        private PriceMode _priceMode1 = PriceMode.Close;

		// MA2
		private int _maPeriod2 = 21;
        private int _maShift2 = 0;
        private MaMethods _maMethod2 = MaMethods.Sma;
        private PriceMode _priceMode2 = PriceMode.Close;
		

        protected override void Init()
        {
            // Event occurs once at the start of the strategy
			_period=Timeframe;
          _ma1 = GetIndicator<MovingAverage>(Instrument.Id, _period,_maPeriod1, _maShift1, _maMethod1, _priceMode1);
          _ma2 = GetIndicator<MovingAverage>(Instrument.Id, _period,_maPeriod2, _maShift2, _maMethod2, _priceMode2);
			           
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			//Print("MA1 = {0} - {1} - {2}",_ma1.SeriesMa[Bars.Range.To -1],_ma2.SeriesMa[Bars.Range.To-1],_ma1.SeriesMa[Bars.Range.To-1]-_ma2.SeriesMa[Bars.Range.To-1]);
			//Print("To-From-Length - {0} {1} {2} ",Bars.Range.To, Bars.Range.From, Bars.Range.Length );
			Print("{0}={1}={2}={3}={4}={5}={6}",Bars[Bars.Range.To-1].Open,Bars[Bars.Range.To-1].Close,Bars[Bars.Range.To-1].High,Bars[Bars.Range.To-1].Low,Bars[Bars.Range.To-1].TickCount,Bars[Bars.Range.To-1].Time,Bars[Bars.Range.To-1].Volume);
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
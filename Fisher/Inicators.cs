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
    [TradeSystem("Inicators")]
    public class Inicators : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		//  Фрактал
		public Fractals _frInd;
		public Alligator _allInd;
		public AwesomeOscillator _awoInd;
		public FisherTransformOscillator _ftoInd;
		private MovingAverage _ma1;
		private StochasticOscillator _stoInd;
		private MovingAverageConvergenceDivergence _macd;
		
		private ZigZag _wprInd;
		public double aoUp, aoDown, sF,  sL, mL, _macdInd, ZZ5;
		
        protected override void Init()
        {
			_allInd = GetIndicator<Alligator>(Instrument.Id, Timeframe); 
			// Fractals
		    _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe); _frInd.Range=5;
			// AO   - определяет движущую силу рынка
			_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);
			// Fisher
			_ftoInd= GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 			
			// Stohastic
			 _stoInd = GetIndicator<StochasticOscillator>(Instrument.Id, Timeframe);
			// Moving Average
			_ma1 = GetIndicator<MovingAverage>(Instrument.Id, Timeframe, 85, 0, MaMethods.Lwma, PriceMode.Close);
			// MACD
			_macd = GetIndicator<MovingAverageConvergenceDivergence> (Instrument.Id, Timeframe);
			_macd.FastEmaPeriod=15;  _macd.AppliedPrice=PriceMode.Low; _macd.SlowEmaPeriod=26; _macd.SmaPeriod=1;
			//  
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Blue;
			
            // Event occurs on every new bar
//==========================================================================================================================
			// AO  - определяет движущую силу рынка
			aoUp    = _awoInd.SeriesUp[Bars.Range.To-1];   // Зелені лінії  - Вверху>0  Внизу<0
			aoDown  = _awoInd.SeriesDown[Bars.Range.To-1]; // Червоні лінії	- Вверху>0  Внизу<0		
//==========================================================================================================================
			// Fisher
			sF  = _ftoInd.FisherSeries[Bars.Range.To-1];  // Фішеер - Плюс-зверху Мінус - знизу
//==========================================================================================================================
			// Stohastic
			  sL = _stoInd.SignalLine[Bars.Range.To-1]; 
			  mL = _stoInd.MainLine[Bars.Range.To-1];	
//==========================================================================================================================
			// MACD
			 _macdInd = _macd.SeriesSignal[Bars.Range.To-1];
//==========================================================================================================================
			// ZigZag
			ZZ5 = _wprInd.MainIndicatorSeries[Bars.Range.To-5];
//==========================================================================================================================
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
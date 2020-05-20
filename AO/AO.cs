using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using IPro.Model.Client.MarketData;
using IPro.Model.Programming.Indicators.Standard;
using System.Collections.Generic;

namespace IPro.TradeSystems
{
    [TradeSystem("AO")]
    public class AO : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		public AwesomeOscillator _awoInd,_awoIndH1,_awoIndH4,_awoIndD1;
		double aoUp, aoUpH1, aoUpH4,aoUpD1;
		double aoDown, aoDownH1, aoDownH4,aoDownD1;		
		private ISeries<Bar> _barSeries,_barH4,_barD1;
		public Period periodH4,periodD1;
		private VerticalLine vlR,vlB,vlR1;



        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);
			
			periodD1 = new Period(PeriodType.Day, 1);
			periodH4 = new Period(PeriodType.Hour, 4);
			_barSeries = GetCustomSeries(Instrument.Id, Period.H1);
			_barH4 = GetCustomSeries(Instrument.Id,periodH4);
			
			_awoIndH1 = GetIndicator<AwesomeOscillator>(Instrument.Id, Period.H1);
			_awoIndH4 = GetIndicator<AwesomeOscillator>(Instrument.Id, periodH4);
			_awoIndD1 = GetIndicator<AwesomeOscillator>(Instrument.Id, periodD1);

			vlR = Tools.Create<VerticalLine>();
			vlR.Color=Color.Red;
			vlB = Tools.Create<VerticalLine>();
			vlB.Color=Color.Blue;
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			aoUp    = _awoInd.SeriesUp[Bars.Range.To-1];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown  = _awoInd.SeriesDown[Bars.Range.To-1];
			
			aoUpH1   = _awoIndH1.SeriesUp[_barSeries.Range.To];
			aoDownH1 = _awoIndH1.SeriesDown[_barSeries.Range.To];
			
			aoUpH4   = _awoIndH4.SeriesUp[_barH4.Range.To];
			aoDownH4 = _awoIndH4.SeriesDown[_barH4.Range.To];
			
			aoUpD1   = _awoIndD1.SeriesUp[_barD1.Range.To];
			aoDownD1 = _awoIndD1.SeriesDown[_barD1.Range.To];
			
			
			if((aoUpD1>0 || aoUpD1<0) && (aoUpH1>0 || aoUpH1<0) && (aoUpH4>0 || aoUpH4<0)) 
			{   var vlR1 = Tools.Create<VerticalLine>();
			    vlR1.Color=Color.Red;
				vlR1.Time=Bars[Bars.Range.To-1].Time; }
			
			if((aoDownD1>0 || aoDownD1<0) && (aoDownH1>0 || aoDownH1<0) && (aoDownH4>0 || aoDownH4<0)) 
			{   var vlB1 = Tools.Create<VerticalLine>();
			    vlB1.Color=Color.Blue;
				vlB1.Time=Bars[Bars.Range.To-1].Time; }
			//if(!(aoUp>0 || aoUp<0)) Print("aoUp - не число!");
			Print("H1-{0} H4-{1} D1-{2}) - {3}",_barSeries[_barSeries.Range.To].Time,_barH4[_barH4.Range.To].Time,_barD1[_barD1.Range.To].Time,Bars[Bars.Range.To-1].Time);
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
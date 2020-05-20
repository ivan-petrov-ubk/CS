// https://www.youtube.com/watch?v=_AVPcPAr4PE
//http://forum.tradelikeapro.ru/index.php?topic=1587.30

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
    [TradeSystem("Price Action_Fisher")] //copy of "Price Action1"
    public class Price_Action1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }

		public ISeries<Bar> _barH1,_barM30,_barM15,_barM5;
		public Period periodH1,periodM30,periodM15,periodM5;
		public double CloseH1,OpenH1,CloseM30,OpenM30,CloseM15,OpenM15,CloseM5,OpenM5;
		public double fsH1,fsM30,fsM15,fsM5;
		public DateTime timeH1,timeM30,timeM15,timeM5;
		private int _lastIndexM5 = -1;
		private int _lastIndexM15 = -1;
		private int _lastIndexM30 = -1;
		private int _lastIndexH1 = -1,i;
		
		private ISeries<double> _values;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			// M5
			periodM5 = new Period(PeriodType.Minute, 5);
			_barM5 = GetCustomSeries(Instrument.Id,periodM5);
			
     		// M15
			periodM15 = new Period(PeriodType.Minute, 15);
			_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			
			// M30
			periodM30 = new Period(PeriodType.Minute, 30);
			_barM30 = GetCustomSeries(Instrument.Id,periodM30);
			
			// H1
            _barH1 = GetCustomSeries(Instrument.Id, Period.H1);
			
			i=0;
			_values = CreateSeries<double>();
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
		if (_lastIndexH1 < _barH1.Range.To-1) {
     		_lastIndexH1 = _barH1.Range.To-1;
    		CloseH1 = _barH1[_lastIndexH1].Close;
			OpenH1 = _barH1[_lastIndexH1].Open;
	 		timeH1 = _barH1[_lastIndexH1].Time;
			
			
			}
		
			if (_lastIndexM30 < _barM30.Range.To - 1) {
     		_lastIndexM30 = _barM30.Range.To - 1;
    		CloseM30 = _barM30[_lastIndexM30].Close;
			OpenM30 = _barM30[_lastIndexM30].Open;		
	 		timeM30 = _barM30[_lastIndexM30].Time; 
				
			}
		
			if (_lastIndexM15 < _barM15.Range.To - 1) {
     		_lastIndexM15 = _barM15.Range.To - 1;
    		CloseM15 = _barM15[_lastIndexM15].Close;
			OpenM15 = _barM15[_lastIndexM15].Open;		
	 		timeM15 = _barM15[_lastIndexM15].Time; 
			
			
			}

			if (_lastIndexM5 < _barM5.Range.To - 1) {
     		_lastIndexM5 = _barM5.Range.To - 1;
    		CloseM5 = _barM5[_lastIndexM5].Close;
			OpenM5 = _barM5[_lastIndexM5].Open;		
	 		timeM5 = _barM5[_lastIndexM5].Time;
				
			}
	
        }
        
        protected override void NewBar()
        {  i++;
            // Event occurs on every new bar
/*			if(CloseH1>0)  Print("{3} -- H1 ={0}-{1}-{2} " ,OpenH1,CloseH1,timeH1,i);
			if(CloseM30>0) Print("{3} -- M30={0}-{1}-{2} ",OpenM30,CloseM30,timeM30,i);
			if(CloseM15>0) Print("{3} -- M15={0}-{1}-{2} ",OpenM15,CloseM15,timeM15,i);
			if(CloseM5>0)  Print("{3} -- M5 ={0}-{1}-{2} " ,OpenM5,CloseM5,timeM5,i);
*/
			//if(fsH1>0)  
				Print("{1} -- H1 ={0} " ,fsH1,i);
			//if(fsM30>0)  
				Print("{1} -- M30 ={0} " ,fsM30,i);
			//if(fsM15>0)  
				Print("{1} -- M15 ={0} " ,fsM15,i);
			//if(fsM5>0)  
				Print("{1} -- M5 ={0} " ,fsM5,i);

			
			Print("{1} -- {0} ========================================================================================================",Bars[Bars.Range.To-2].Time,i);
			
	/*		if(CloseM15>0 && CloseM5>0 && CloseM30>0 && CloseH1>0 && Bars[Bars.Range.To-1].Time.Minute.ToString()=="5" ) 
			{
			   if( CloseM5>OpenM5 && CloseM15>OpenM15 && CloseM30>OpenM30 && CloseH1>OpenH1  ) 
			   {  Print("{0}-{1} {2}-{3} {4}-{5} {6}-{7}  {8}  Buy!!!",OpenH1,CloseH1,OpenM30,CloseM30,OpenM15,CloseM15,OpenM5,CloseM5,Bars[Bars.Range.To-1].Time);Red();	}
			   if( CloseM5<OpenM5 && CloseM15<OpenM15 && CloseM30<OpenM30 && CloseH1<OpenH1  ) 
			   { Print("{0}-{1} {2}-{3} {4}-{5}  {6}-{7}  {8}  Sell!!!",OpenH1,CloseH1,OpenM30,CloseM30,OpenM15,CloseM15,OpenM5,CloseM5,Bars[Bars.Range.To-1].Time);Blue();  }	
			}
	*/		


        }
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }

	private  void CalculateFs(int index)
        {
            var max = GetPrice(Bars[Series.Highest(index, Period, PriceMode.High)], PriceMode.High);
            var min = GetPrice(Bars[Series.Lowest(index, Period, PriceMode.Low)], PriceMode.Low);

            var price = GetPrice(Bars[index], PriceMode.Close);

            var val = 0.33 * 2 * ((price - min) / (max - min) - 0.5) + 0.67 * ValueOrDefault(_values[index - 1]);
            val = Math.Min(Math.Max(val, -0.999), 0.999);

            var fish = 0.5 * Math.Log((1.0 + val) / (1.0 - val)) + 0.5 * ValueOrDefault(FisherSeries[index - 1]);

            _values[index] = val;
            FisherSeries[index] = ValueOrDefault(fish);

            if (fish >= 0)
            {
                UpSeries[index] = fish;
                DownSeries[index] = double.NaN;
            }
            else if (fish < 0)
            {
                DownSeries[index] = fish;
                UpSeries[index] = double.NaN;
            }

            Ma1Series[index] = ValueOrDefault(Series.Ma(FisherSeries, index, Ma1Period, Ma1Method));
            Ma2Series[index] = ValueOrDefault(Series.Ma(Ma1Series, index, Ma2Period, Ma2Method));
        }
    }
}
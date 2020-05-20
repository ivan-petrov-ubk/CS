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

namespace IPro.Samples
{
    [Indicator("Sample Fisher Transform Oscillator", LevelsAllowed = true)]
    public sealed class SampleFisherTransformOscillator : Indicator
    {
		
		 [Series("Result",  Color = Color.Red, Style = SeriesStyle.Line)]
          public IIndicatorSeries Result { get; set; }
		
        [Parameter("LR_Period", DefaultValue = 18, MinValue = 1)]
        public int Period { get; set; }

        [Parameter("LR_Ma1Period", DefaultValue = 9, MinValue = 1)]
        public int Ma1Period { get; set; }

        [Parameter("LR_Ma1Method", DefaultValue = MaMethods.Sma)]
        public MaMethods Ma1Method { get; set; }

        [Parameter("LR_Ma2Period", DefaultValue = 24, MinValue = 1)]
        public int Ma2Period { get; set; }

        [Parameter("LR_Ma2Method", DefaultValue = MaMethods.Sma)]
        public MaMethods Ma2Method { get; set; }

        [Series("LR_Up", Color = Color.LightSeaGreen, Style = SeriesStyle.Histogram)]
        public IIndicatorSeries UpSeries { get; set; }

        [Series("LR_Down", Color = Color.DarkGoldenrod, Style = SeriesStyle.Histogram)]
        public IIndicatorSeries DownSeries { get; set; }

        [Series("LR_Fisher", Color = Color.Gray)]
        public IIndicatorSeries FisherSeries { get; set; }

        [Series("LR_Ma1", Color = Color.Blue)]
        public IIndicatorSeries Ma1Series { get; set; }

        [Series("LR_Ma2", Color = Color.Red)]
        public IIndicatorSeries Ma2Series { get; set; }
        

		public VerticalLine toolVerticalLine;
		public VerticalLine toolVerticalLine2;

		private Period _period;
		private int _lastIndexM15 = -1;
		public Period periodM15;
		public FisherTransformOscillator _ftoIndM15;
		double smF,smF1; 
		public ISeries<Bar> _barM15;
		private int _lastIndex = -1;
				private MovingAverage _ma1;	
		// MA1
		private int _maPeriod1 = 360;
        private int _maShift1 = 0;
        private MaMethods _maMethod1 = MaMethods.Ema;
        private PriceMode _priceMode1 = PriceMode.Close;

		
        protected override int TotalPeriod
        {
            get { return Period; }
        }

        protected override bool DependsOnPreviousData
        {
            get { return true; }
        }

		//Метод Init вызывается при инициализации (старте) индикатора.
        protected override void Init()
        {
             // получить значение начального Time Frame, на котором был запущен индикатор.
			_period = Timeframe;

			Print("Init");
			toolVerticalLine = Tools.Create<VerticalLine>();
			toolVerticalLine.Color=Color.Red;
			toolVerticalLine2 = Tools.Create<VerticalLine>();
			toolVerticalLine2.Color=Color.Blue;

			
			//periodM15 = new Period(PeriodType.Minute, 15);
			//_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			_ftoIndM15= GetIndicator<FisherTransformOscillator>(Instrument.Id, _period); 
			 _ma1 = GetIndicator<MovingAverage>(Instrument.Id, _period,_maPeriod1, _maShift1, _maMethod1, _priceMode1);

        }

		// Метод Calculate вызывается при поступлении нового тика по символу. 
        protected override void Calculate(int index)
        { 
			//if ((_barM15[_barM15.Range.To-1].High>0) && (_lastIndexM15 < _barM15.Range.To - 1)) {
     		//_lastIndexM15 = _barM15.Range.To - 1;
	 		//smF  = _ftoIndM15.FisherSeries[_lastIndexM15-1];
	 		//smF1 = _ftoIndM15.FisherSeries[_lastIndexM15-2];
			//	Print("{0} {1} - {2}",smF1,smF,_barM15[_barM15.Range.To-1].Time);

			if ((Bars[Bars.Range.To-1].High>0) && (_lastIndexM15<Bars.Range.To-1)) 
			{
     			_lastIndexM15 = Bars.Range.To - 1;
	 			smF  = _ftoIndM15.FisherSeries[_lastIndexM15-1];
	 			smF1 = _ftoIndM15.FisherSeries[_lastIndexM15-2];
				Print("{0} {1} - {2}",smF1,smF,Bars[Bars.Range.To-1].Time);
			
				if (smF1>0 && smF<0) 
			   	{ toolVerticalLine.Time=Bars[Bars.Range.To-1].Time; 
                	 System.Console.Beep(800,900); }	
				if (smF1<0 && smF>0) 
		       { toolVerticalLine2.Time=Bars[Bars.Range.To-1].Time; 
					 System.Console.Beep(1200,900); }				
			}
			
		}	
  }
}


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
    [Indicator("Fisher4", LevelsAllowed = true)] //copy of "Fisher3"
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
        
		private ISeries<double> _values;
		public VerticalLine toolVerticalLine;
				public VerticalLine toolVerticalLine2;
		public VerticalLine toolVerticalLine3;
		public VerticalLine toolVerticalLine4;
		public HorizontalLine toolHorizLine;
		public int Sr;
		bool SU=false;
		bool SD=false;
		public int fs;
		public int fs1;
private Period _period;
		double PrevFish=0.0;
		private int _lastIndexM15 = -1;
		public Period periodM15;
		public FisherTransformOscillator _ftoInd,_ftoIndM15;
	double sM15F,smF,smF1; 
		public ISeries<Bar> _barSeries,_barM15;
		private int _lastIndex = -1;
		
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
			//  получить серию баров для торгового инструмента и торгового периода (Time Frame)
			 //_barSeries = GetCustomSeries(Instrument.Id, Timeframe);
			
			_values = CreateSeries<double>();
			Print("Init");
			toolVerticalLine = Tools.Create<VerticalLine>();
			toolVerticalLine.Color=Color.Red;
			toolVerticalLine2 = Tools.Create<VerticalLine>();
			toolVerticalLine2.Color=Color.Blue;
			toolVerticalLine3 = Tools.Create<VerticalLine>();
			toolVerticalLine3.Color=Color.Aqua;
			toolVerticalLine4 = Tools.Create<VerticalLine>();
			toolVerticalLine4.Color=Color.DarkMagenta;
			
			 toolHorizLine = Tools.Create<HorizontalLine>();
			
			//вывести в «Журнал событий» сообщение с периодом расчета индикатора.
			 Print("TotalPeriod: {0}", TotalPeriod);
			

        }

		// Метод Calculate вызывается при поступлении нового тика по символу. 
        protected override void Calculate(int index)
        { 

			
			// Метод GetPrice возвращает цену бара в соответствии с типом цены.
			// double GetPrice(Bar bar, PriceMode appliedPrice)           
			// получить информацию о значениях цен закрытия,  - var closePrice = GetPrice(Bars[Bars.Range.To - 1], PriceMode.Close);
            var max = GetPrice(Bars[Series.Highest(index, Period, PriceMode.High)], PriceMode.High);
            var min = GetPrice(Bars[Series.Lowest(index, Period, PriceMode.Low)], PriceMode.Low);

            var price = GetPrice(Bars[index], PriceMode.Close);

            var val = 0.33 * 2 * ((price - min) / (max - min) - 0.5) + 0.67 * ValueOrDefault(_values[index - 1]);
            val = Math.Min(Math.Max(val, -0.999), 0.999);

            var fish = 0.5 * Math.Log((1.0 + val) / (1.0 - val)) + 0.5 * ValueOrDefault(FisherSeries[index - 1]);

            _values[index] = val;
			// Метод ValueOrDefault проверяет значение аргумента val и возвращает его значение, если оно является числом 
			//и не является бесконечной величиной. В противном случае возвращает значение по умолчанию (defaultVal).
			//   double ValueOrDefault(double val, double defaultVal = 0)
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
			
		if (_lastIndex != Bars.Range.To - 1) 
		{   _lastIndex = Bars.Range.To - 1;
// $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
			// Перетин 0 ВВЕРХ - на BUY
			if (FisherSeries[Bars.Range.To-2]<0 && FisherSeries[Bars.Range.To-1]>0) 
			   {  toolVerticalLine.Time=Bars[Bars.Range.To-1].Time; 
				   Print("{0} - Пересечение 0 линии ВВЕРХ!",Instrument.Name);
                  System.Console.Beep(800,800); }
		    // Перетин 0 ВНИЗ - на SELL
	    	if (FisherSeries[Bars.Range.To-2]>0 && FisherSeries[Bars.Range.To-1]<0) 
		       { toolVerticalLine2.Time=Bars[Bars.Range.To-1].Time; 
				   Print("{0} - Пересечение 0 линии ВНИЗ!",Instrument.Name);
			//Частота сигнала в диапазоне от 37 до 32767 Гц. 2. Длительность сигнала в миллисекундах.
			     System.Console.Beep(1200,800); }
			   
			if (FisherSeries[Bars.Range.To-2]>Ma1Series[Bars.Range.To-2] && FisherSeries[Bars.Range.To-1]<Ma1Series[Bars.Range.To-1]) 
			   {  toolVerticalLine3.Time=Bars[Bars.Range.To-1].Time;
				   Print("{0} - Пересечение MA1 линии гистограм ВВЕРХ!",Instrument.Name);
                  System.Console.Beep(1800,800); }
			   
			if (FisherSeries[Bars.Range.To-2]<Ma1Series[Bars.Range.To-2] && FisherSeries[Bars.Range.To-1]>Ma1Series[Bars.Range.To-1]) 
			   {  toolVerticalLine4.Time=Bars[Bars.Range.To-1].Time; 
				    Print("{0} - Пересечение MA1 линии гистограм ВНИЗ!",Instrument.Name);
                  System.Console.Beep(400,800); }
		}	
	}
  }
}


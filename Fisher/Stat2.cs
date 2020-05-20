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
    [TradeSystem("Stat1")]
    public class Stat1 : TradeSystem
	{
		
		/// </summary>
		[Parameter("Fisher_LR_Period", DefaultValue = 12, MinValue = 1)]
        public int FisherPeriod { get; set; }

        [Parameter("Fisher_LR_Ma1Period", DefaultValue = 6, MinValue = 1)]
        public int FisherMa1Period { get; set; }

        [Parameter("Fisher_LR_Ma1Method", DefaultValue = MaMethods.Ema)]
        public MaMethods FisherMa1Method { get; set; }

        [Parameter("Fisher_LR_Ma2Period", DefaultValue = 18, MinValue = 1)]
        public int FisherMa2Period { get; set; }

        [Parameter("Fisher_LR_Ma2Method", DefaultValue = MaMethods.Ema)]
        public MaMethods FisherMa2Method { get; set; }

		
		[Parameter("Log_FileName", DefaultValue = @"Stat")]
		public string LogFileName { get;set; }
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		private string trueLogPath = "";
		
		private FisherTransformOscillator FTOind;
		
		private ISeries<Bar> _barSeries;
		private Period _period = new Period(PeriodType.Minute, 15);
		public static int ci = 0;
		public double max=0,min=1000;
		public double maxb,minb,closeU,closeD,cl;
		public bool up;

		protected override void Init()
        {
				InitLogFile();
			closeU = Bars[Bars.Range.To-1].Close;
			closeD = Bars[Bars.Range.To-1].Close;
						_period = Timeframe;
			_barSeries = GetCustomSeries(Instrument.Id, 
											_period);
			FTOind = GetIndicator<FisherTransformOscillator>(
												Instrument.Id,
												_period,
												FisherPeriod,
												FisherMa1Period,
												FisherMa1Method,
												FisherMa2Period,
												FisherMa2Method);
			XXPrint("NM;DATE;CLOSE;MAX;MIN;SL;TP");

		}        

        
        protected override void NewBar()
        {   ci = Bars.Range.To - 1;
			if (Bars[ci].High>max)  max=Bars[ci].High;  
			if (Bars[ci].Low<min)   min=Bars[ci].Low;

		   // Вверх
		if(FTOind.FisherSeries[ci]>0.1 &&	FTOind.FisherSeries[ci-1]<=0)  { 	
		// Красная - синюю if(FTOind.Ma1Series[ci]>FTOind.Ma2Series[ci] &&	FTOind.Ma1Series[ci-1]<=FTOind.Ma2Series[ci-1])  { 
			minb = Math.Round(min-closeU,5)*100000;
			maxb = Math.Round(max-closeU,5)*100000;
			cl = Math.Round(closeU-Bars[ci].Close,5)*100000;
			var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[ci].Time;
			XXPrint("0;{0};{1};{2};{3};{4};{5};{6}", Bars[ci].Time,closeU,Bars[ci].Close,max,min,maxb,minb,cl);
			closeD = Bars[ci].Close;
			max=0; min=2;  }

			// Вниз
		if(FTOind.FisherSeries[ci]<-0.1 &&	FTOind.FisherSeries[ci-1]>=0) {
		// Красная - синюю  if(FTOind.Ma1Series[ci]<FTOind.Ma2Series[ci] &&	FTOind.Ma1Series[ci-1]>=FTOind.Ma2Series[ci-1]) {
			minb = Math.Round(closeD-min,5)*100000;
			maxb = Math.Round(closeD-max,5)*100000;
			cl = Math.Round(Bars[ci].Close-closeD,5)*100000;
			var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[ci].Time;
 			XXPrint("1;{0};{1};{2};{3};{4};{5};{6}", Bars[ci].Time,closeD,Bars[ci].Close,max,min,minb,maxb,cl);
			closeU = Bars[ci].Close;
			min=2; max=0; }
		}
		protected void InitLogFile()
		{	trueLogPath=PathToLogFile+"\\"+LogFileName+DateTime.Now.Minute.ToString().Trim()+".LOG";    }		
		
		protected void XXPrint(string xxformat, params object[] parameters)
		{		var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);		}
		
    }
}
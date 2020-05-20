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
    [TradeSystem("Fisher")]
    public class Fisher : TradeSystem
    {
        public FisherTransformOscillator _ftoInd;
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		private string trueLogPath = "";
		public string LogFileName = @"Fisher";		
				
        protected override void Init()
        {	trueLogPath=PathToLogFile+"\\"+LogFileName+".LOG";
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
        }        

        protected override void NewBar()
        {   Print("{0} : {1}", Bars[Bars.Range.To-1].Time,_ftoInd.FisherSeries[Bars.Range.To-1]);
			XXPrint("{0} : {1}", Bars[Bars.Range.To-1].Time,_ftoInd.FisherSeries[Bars.Range.To-1]);
			
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]>0 &&  _ftoInd.FisherSeries[Bars.Range.To-2]<0 ) {
		   // var vline = Tools.Create<VerticalLine>();
           // vline.Color=Color.Red;
		   // vline.Time=Bars[Bars.Range.To-1].Time;	
			//}
        }

		protected void XXPrint(string xxformat, params object[] parameters)
		{   var logString=string.Format(xxformat,parameters)+Environment.NewLine;
			File.AppendAllText(trueLogPath, logString);   }
    }
}
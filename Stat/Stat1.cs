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
		[Parameter("Log_FileName", DefaultValue = @"Stat")]
		public string LogFileName { get;set; }
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		private string trueLogPath = "";

		protected override void Init()
        {
				InitLogFile();
		}        

        
        protected override void NewBar()
        {
			XXPrint("{0}", Bars[Bars.Range.To-1].Time);
        }
        
	
		protected void InitLogFile()
		{	trueLogPath=PathToLogFile+"\\"+LogFileName+DateTime.Now.Minute.ToString().Trim()+".LOG";
			//trueLogPath=PathToLogFile+"\\"+LogFileName+".LOG";
			
		}		
		
		protected void XXPrint(string xxformat, params object[] parameters)
		{		var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);		}
		
    }
}
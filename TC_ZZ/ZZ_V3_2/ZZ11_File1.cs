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
    [TradeSystem("ZZ11")]       //copy of "ZZ7"
    public class ZigZag2 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "TC ZZ11")]
        public string CommentText { get; set; }
		
		[Parameter("Debug_Mode", DefaultValue = false)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = true)]
		public bool LogMode { get;set; }

		[Parameter("Log_FileName", DefaultValue = @"Stat")]
		public string LogFileName { get;set; }
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		//private static string PathToLogFile = "d:\\log";
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
		{
			//trueLogPath=PathToLogFile+"\\"+LogFileName+DateTime.Now.Ticks.ToString().Trim()+".LOG";
			trueLogPath=PathToLogFile+"\\"+LogFileName+".LOG";
			Print("{0}",trueLogPath);
			
		}
		protected void XXPrint(string xxformat, params object[] parameters)
		{
			if(LogMode)
			{
				var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);
			}
			
			if(DebugMode)
			{
				Print(xxformat,parameters);
			}	
		}
    }
}
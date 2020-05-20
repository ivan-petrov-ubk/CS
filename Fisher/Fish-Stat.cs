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
    [TradeSystem("Fish-Stat")]
    public class FishStat : TradeSystem
    {
 		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		private string trueLogPath = "";
				public FisherTransformOscillator _ftoIndM15,_ftoInd;
		public bool isFU,isFD;
		public double FU,FD,F1,F2,rz;
		public static int ci = 0;	
		public DateTime DTime; // Время		
        protected override void Init()
        {
			InitLogFile(); 
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
        }        


        
        protected override void NewBar()
        {
		  	DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
			//Значение счетчика прохода фишера через 0  FU - вверх  FD - вниз
			F1=_ftoInd.FisherSeries[Bars.Range.To-1];
			F2=_ftoInd.FisherSeries[Bars.Range.To-2];
			
		    if ( F2<0 && F1>0 )  
				 { 
					 rz=Math.Round((F1-F2)*1000,0);
					 F1=Math.Round(F1*1000,0);
					 F1=Math.Round(F2*1000,0);					 
					 Print("DW {0} ; {1} ; {2} ; {3} ",DTime,F1,F2,rz); 
					 var toolText = Tools.Create<Text>(); toolText.Point=new ChartPoint(DTime, Bars[Bars.Range.To-1].High+0.001);
					 toolText.Caption=string.Format("{0}",rz);
					 var vl1 = Tools.Create<VerticalLine>(); vl1.Time=DTime; vl1.Color=Color.Aqua; 
				 } 
			if (  F2>0 && F1<0 ) 
				 {   rz=Math.Round((F2-F1)*1000,0);
					 F1=Math.Round(F1*1000,0);
					 F1=Math.Round(F2*1000,0);
					 Print("DW {0} ; {1} ; {2} ; {3} ",DTime,F1,F2,rz); 
                     var toolText = Tools.Create<Text>(); toolText.Point=new ChartPoint(DTime, Bars[Bars.Range.To-1].Low-0.001);
					 					 toolText.Caption=string.Format("{0}",rz);
					 var vl1 = Tools.Create<VerticalLine>(); vl1.Time=DTime; vl1.Color=Color.Aqua; 
					 
					 
				 } 
        }
  		protected void InitLogFile()
		{trueLogPath=PathToLogFile+"\\"+Instrument.Name.ToString()+".log";}
		
		protected void XXPrint(string xxformat, params object[] parameters)
		{
				var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);
		}      

    }
}
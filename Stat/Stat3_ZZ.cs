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
    [TradeSystem("Stat3_ZZ")] //copy of "Stat2"
    public class Stat1 : TradeSystem
	{
				[Parameter("ExtDepth:", DefaultValue = 5)]
        public int ED { get; set; }		
		
	[Parameter("Log_FileName", DefaultValue = @"Stat")]
		public string LogFileName { get;set; }
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		private string trueLogPath = "";
		
				private ZigZag _wprInd;
		private int ci;
		private double zzd1,zzd2,zzd3,zzd4,zzd5;
		private int zzi1,zzi2,zzi3,zzi4,zzi5;
		private double zz1,zz2,zz3,zz4,zz5;
		private int zi1,zi2,zi3,zi4,zi5;
		
		
		protected override void Init()
        {
				InitLogFile();
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;

			XXPrint("NM;DATE;CLOSE;MAX;MIN;SL;TP");

		}        

        
        protected override void NewBar()
        {   
			 _wprInd.ReInit();
			ci = Bars.Range.To - 1;
//========== Если появился пик зигзага  ================================================================================================
			if( _wprInd.MainIndicatorSeries[ci]>0) 
			{    // Значения 3 значений - для определения направления
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[ci];
				 zi2=zi1;  zi1=ci;
				Print("{0} - {1} {2}",Bars[ci].Time,zz1<zz2,zz3<zz2);
//======================= ПИК - ВЕШИНА ВВЕРХУ 
				if(zz2<zz3 && zz2<zz1) 
				{   zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2; 
					zzi5=zzi4; zzi4=zzi3; zzi3=zzi2; zzi2=zzi1; zzi1=zi2; 
					var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[zzi1].Time;
				}
				if(zz2>zz3 && zz2>zz1) 
				{   zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2; 
					zzi5=zzi4; zzi4=zzi3; zzi3=zzi2; zzi2=zzi1; zzi1=zi2; 
					var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[zzi1].Time;
				}
			}	

		}
		protected void InitLogFile()
		{	trueLogPath=PathToLogFile+"\\"+LogFileName+DateTime.Now.Minute.ToString().Trim()+".LOG";    }		
		
		protected void XXPrint(string xxformat, params object[] parameters)
		{		var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);		}
		
    }
}
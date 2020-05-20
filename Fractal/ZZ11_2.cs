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
    [TradeSystem("ZZ11_2")]        //copy of "ZZ11"
    public class ZigZag2 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "TC ZZ11")]
        public string CommentText { get; set; }
		[Parameter("TakeProfit=", DefaultValue = 100)]
        public int TP { get; set; }
		[Parameter("StopLoss=", DefaultValue = 200)]
        public int SL { get; set; }
		[Parameter("Spred=", DefaultValue = 0)]
        public int SP { get; set; }	
		[Parameter("ExtDepth=", DefaultValue = 12)]
        public int ED { get; set; }		
		[Parameter("Kor1=", DefaultValue = 0)]
        public int kor { get; set; }		
		[Parameter("Kor2=", DefaultValue = 30)]
        public int Pt { get; set; }	
		
		[Parameter("Debug_Mode", DefaultValue = false)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = true)]
		public bool LogMode { get;set; }

		[Parameter("Log_FileName", DefaultValue = @"ZZ11_")]
		public string LogFileName { get;set; }
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		// private static string PathToLogFile = "D:\\Log_ZZ\\";
		private string trueLogPath = "";
		
		private ZigZag _wprInd;
		private double zz1,zz2,zz3,zzU,zzD;
		private int zzd1,zzd2,zzd3;
		private int zzi1,zzi2,zzi3;
		private bool up;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double kor2;
		//public Fractals _frInd;
		public VerticalLine vr,vb,vy,vw;

		
		
        protected override void Init()
        {   
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			InitLogFile();
			XXPrint("Старт торговой системы: {0} {1} - ZZ11",Bars[Bars.Range.To].Time, this.Account.Number);
			
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
			if(Instrument.Name.EndsWith("JPY")) kor2=0.001*Pt; else kor2=0.00001*Pt;		
			//_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);

			vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; 
			vb=Tools.Create<VerticalLine>(); vb.Color=Color.Blue; 
			vy=Tools.Create<VerticalLine>(); vy.Color=Color.Yellow;
			vw=Tools.Create<VerticalLine>(); vw.Color=Color.White; 
        }        
        
        protected override void NewBar()
        {
			var posActiveMine = Trade.GetActivePositions(null, true);
		    if(posActiveMine.Length>1) {
				 if(posActiveMine[posActiveMine.Length-1].Type.IsBuy()) 
				 {  for (int i=0; i<posActiveMine.Length-1; i++)  
					if(posActiveMine[i].Type.IsSell()) { var res = Trade.CloseMarketPosition(posActiveMine[i].Id);}     }
				 
				 if(posActiveMine[posActiveMine.Length-1].Type.IsSell()) 
				 {	 for (int i=0; i<posActiveMine.Length-1; i++)  
						 if(posActiveMine[i].Type.IsBuy()) { var res = Trade.CloseMarketPosition(posActiveMine[i].Id); }}
			}
				 _wprInd.ReInit();
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-2]>0) 
			{ // Всі точки перегину зиззага
				if(Instrument.Name.EndsWith("JPY")) kor2=0.001*Pt; else kor2=0.00001*Pt;
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-2];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-2;
				 
			if(zz3>zz2 && zz2<zz1)  { zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;}
			if(zz3<zz2 && zz2>zz1)  { zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;}
			

			if(Bars[zzd2].Low>Bars[zzi1].Low &&  Bars[zzd2].High<Bars[zzd1].High && Bars[zzi1].High<Bars[zzd1].High &&  Bars[zzd3].Low-Bars[zzi1].Low>kor2) 
			  { //Print("Ввверху Максимум"); 
				vr.Time = Bars[zzd2].Time;
				vb.Time = Bars[zzd1].Time;
				vy.Time = Bars[zzi1].Time;
				if(Bars[zzd1].Low!=zzU) { zzU=Bars[zzd1].Low;	
					
			   	var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  Bars[zzd1].High-Instrument.Spread, 0, Stops.InPips(SL,TP), Bars[zzd1].Time.AddHours(15), null, null);
					XXPrint("Sell {0} {1} {2} {3} {4} {5}",Bars[zzd1].Time,result1.Position.Number,Bars[zzd1].High-Instrument.Spread-kor,Math.Abs(zz3-zz2),Math.Abs(zz1-zz2),Math.Abs(zz3-zz1));
				}
													
			  }
					
			if(Bars[zzd2].High<Bars[zzi1].High &&  Bars[zzd1].Low<Bars[zzd2].Low && Bars[zzd1].Low<Bars[zzi1].Low &&  Bars[zzi1].Low-Bars[zzd3].Low>kor2) 
				{  //Print("Внизу Максимум");
					vr.Time = Bars[zzd2].Time;
			  		vb.Time = Bars[zzd1].Time;
					vy.Time = Bars[zzi1].Time;
					if(Bars[zzd1].Low!=zzD) { zzD=Bars[zzd1].Low;
						
					var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, Bars[zzd1].Low+Instrument.Spread+kor, 0, Stops.InPips(SL,TP), Bars[zzd1].Time.AddHours(15), null, null);
					XXPrint("Buy {0} {1} {2} {3} {4} {5}",Bars[zzd1].Time,result.Position.Number,Bars[zzd1].High-Instrument.Spread-kor,Math.Abs(zz3-zz2),Math.Abs(zz1-zz2),Math.Abs(zz3-zz1));

						}

				}			
			}	
        }
		protected void InitLogFile()
		{
			//trueLogPath=PathToLogFile+"\\"+LogFileName+DateTime.Now.Ticks.ToString().Trim()+".LOG";
			trueLogPath=PathToLogFile+"\\"+LogFileName+Instrument.Name.ToString()+".LOG";
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
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
    [TradeSystem("ZZ16")]      //copy of "ZZ15"
    public class ZigZag2 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "TC ZZ11")]
        public string CommentText { get; set; }
		[Parameter("TakeProfit:", DefaultValue = 100)]
        public int TP { get; set; }
		[Parameter("StopLoss=", DefaultValue = 200)]
        public int SL { get; set; }
		[Parameter("Spred:", DefaultValue = 0)]
        public int SP { get; set; }	
		[Parameter("ExtDepth:", DefaultValue = 5)]
        public int ED { get; set; }		
		[Parameter("Kor1:", DefaultValue = 0)]
        public int kor { get; set; }		
		[Parameter("Дельта:", DefaultValue = 30)]
        public int Pt { get; set; }	
	
		[Parameter("Debug_Mode:", DefaultValue = false)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode:", DefaultValue = true)]
		public bool LogMode { get;set; }

		[Parameter("Log_FileName:", DefaultValue = @"ZZ15")]
		public string LogFileName { get;set; }
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		// private static string PathToLogFile = "D:\\Log_ZZ\\";
		private string trueLogPath = "";
		
		private ZigZag _wprInd;
		private double zz1,zz2,zz3,zzU,zzD;
		private int zzd1,zzd2,zzd3;
		private int zzi1,zzi2,zzi3;
		private bool up,bD,bU;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double kor2;
		//public Fractals _frInd;
		public VerticalLine vr,vb,vy,vw,vg;
		public int n,Zn,iU1,iD1,iU3,iD3;
		//public FisherTransformOscillator _ftoInd;
		double aoUp1,aoDown1,aoUp2,aoDown2;
		public AwesomeOscillator _awoInd;
	
        protected override void Init()
        {   				
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			InitLogFile();
			//XXPrint("ZZ11:{0} Счет:{1} Пара:{2}",Bars[Bars.Range.To].Time, this.Account.Number,this.Instrument.Name);
			//XXPrint("Ордер Дата Time №Ордер Цена Left Right Delta VLeft VOrder VRight");	
			_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
			
			
			if(Instrument.Name.EndsWith("JPY")) { 
				kor2=0.001*Pt; Zn=1000; n=3;} else { kor2=0.00001*Pt; Zn=100000; n=5;
				}		
			//_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);

			vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; 
			vb=Tools.Create<VerticalLine>(); vb.Color=Color.Blue; 
			vy=Tools.Create<VerticalLine>(); vy.Color=Color.Yellow;
			vw=Tools.Create<VerticalLine>(); vw.Color=Color.MediumVioletRed;
			vg=Tools.Create<VerticalLine>(); vw.Color=Color.Teal;
        }        
        
        protected override void NewBar()
        { 	_wprInd.ReInit();
	// ЗАКРИТТЯ ПОЗИЦІЙ !!!
			// ЗНИЗУ ВВЕРХ
			if (_awoInd.SeriesUp[Bars.Range.To-3]<0   && _awoInd.SeriesUp[Bars.Range.To-2]>0) {
				iU1=Bars.Range.To;iU3=Bars.Range.To-3;
			  var vr=Tools.Create<VerticalLine>(); vr.Color=Color.BlueViolet; vr.Time=Bars[iU1].Time;
			if(Trade.GetPosition(posGuidSell).State==PositionState.Active)
				{	var res = Trade.CloseMarketPosition(posGuidSell);
                        if (res.IsSuccessful) posGuidSell=Guid.Empty;    }	
			if(Trade.GetPosition(posGuidSell).State==PositionState.Pending)
				{	var res1 = Trade.CancelPendingPosition(posGuidSell);
                        if (res1.IsSuccessful) posGuidSell=Guid.Empty;    }	
			
			}
			// ЗВЕРХУ ВНИЗ
			if (_awoInd.SeriesDown[Bars.Range.To-3]>0 && _awoInd.SeriesDown[Bars.Range.To-2]<0) {
				iD1=Bars.Range.To; iD3=Bars.Range.To-3; 
			var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Black; vr.Time=Bars[iD1].Time;
				if(Trade.GetPosition(posGuidBuy).State==PositionState.Active)
				{	var res2 = Trade.CloseMarketPosition(posGuidBuy);
                        if (res2.IsSuccessful) posGuidBuy=Guid.Empty;    }
				if(Trade.GetPosition(posGuidBuy).State==PositionState.Pending)
				{	var res3 = Trade.CancelPendingPosition(posGuidBuy);
                        if (res3.IsSuccessful) posGuidBuy=Guid.Empty;    }
			}
			
		//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty  && Trade.GetPosition(posGuidBuy).State==PositionState.Closed)  posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  

		//=== Закрытие всех ордеров если пятница 16:00 (19:00 Kiev) ===========================================================================
          if ( Bars[Bars.Range.To-1].Time.DayOfWeek==DayOfWeek.Friday && Bars[Bars.Range.To-1].Time.Hour==16 ) 
		  {  
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
		  }

			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{ // Всі точки  зиззага
				vy.Time=Bars[Bars.Range.To-1].Time; 

				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1]; // Значення ZigZag
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;                             // Індекс свічок
				 //  Індекси свічок в Куті перегибу

				// ПИК ВВЕРХУ - закрыть покупку 
				if(zz3<zz2 && zz2>zz1)  
				{   zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
		 			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
					{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) 
					      posGuidBuy = Guid.Empty; }
				}
				// ПИК ВНИЗУ - закрыть продажа 
				if(zz3>zz2 && zz2<zz1)  
				{   zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
						{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
				}

//============== Тренд ВВЕРХ - індекси свічок zzd1-3  ====================================================
				if(Bars[zzd2].High>Bars[zzd1].High && 
				   Bars[zzd2].High>Bars[zzd3].High && 
				   Bars[zzd1].High>Bars[zzd3].High &&
				(Bars[zzd2].High-Bars[zzd1].Low)>(Bars[zzd2].High-Bars[zzd3].Low)*0.23 &&
				(Bars[zzd2].High-Bars[zzd1].Low)<(Bars[zzd2].High-Bars[zzd3].Low)*0.8
					)
				{	//vr.Time=Bars[zzd1].Time; vb.Time=Bars[zzd2].Time; vg.Time=Bars[zzd3].Time;
  				    Print("{0}  Buy={1} iU={2} zzd3={3} zzd2={4}",Bars[Bars.Range.To-1].Time,posGuidBuy,iU1,zzd3,zzd2);
					//var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[zzd1].Time;
					 if (posGuidBuy==Guid.Empty && zzd3<iU1 && zzd2>iU3) {
					var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyStop, 0.1, Bars[zzd2].High, 0, Stops.InPrice(Bars[zzd3].Low,null), null, null, null);	
					  //var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1, Stops.InPrice(Bars[zzd3].Low,null), null, null);
						if (result.IsSuccessful)  posGuidBuy=result.Position.Id; }
				}
				
//============= Тренд ВНИЗ - індекси свічок zzd1  ==========================================================
				if(Bars[zzd3].Low>Bars[zzd2].Low && 
				   Bars[zzd1].Low>Bars[zzd2].Low && 
				   Bars[zzd3].Low>Bars[zzd1].Low &&
				(Bars[zzd1].High-Bars[zzd2].Low)>(Bars[zzd3].High-Bars[zzd2].Low)*0.23 &&
				(Bars[zzd1].High-Bars[zzd2].Low)<(Bars[zzd3].High-Bars[zzd2].Low)*0.8
					)
				{   // vr.Time=Bars[zzd1].Time; vb.Time=Bars[zzd2].Time; vg.Time=Bars[zzd3].Time;
					// var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[zzd1].Time;
					Print("{0}  Sell={1} iD={2} zzd3={3} zzd2={4} ",Bars[Bars.Range.To-1].Time,posGuidSell,iD1,zzd3,zzd2);
					if (posGuidSell==Guid.Empty && zzd3<iD1 && zzd2>iD3) { 
						var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellStop, 0.1, Bars[zzd2].Low, 0, Stops.InPrice(Bars[zzd3].High,null), null, null, null);
					   //var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1, Stops.InPrice(Bars[zzd3].High,null), null, null);		
						 if (result1.IsSuccessful) { 
							   posGuidSell=result1.Position.Id; 
						       //XXPrint("{0} ",Bars[Bars.Range.To-1].Time,);
						 } }
				}		
			}	
        }
		protected void InitLogFile()
		{
		  //trueLogPath=PathToLogFile+"\\"+LogFileName+DateTime.Now.Ticks.ToString().Trim()+".LOG";
			trueLogPath=PathToLogFile+"\\"+LogFileName+".LOG";
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
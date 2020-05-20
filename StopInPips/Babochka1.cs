//http://strategy4you.ru/strategii-foreks-na-osnove-skolzyashhix-srednix/pattern-galstuk-babochka.html
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
    [TradeSystem("Babochka1")]    //copy of "Puria5_Fractal"
    public class Puria : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Puria!")]
        public string CommentText { get; set; }
        [Parameter("Profit", DefaultValue = 150)]
        public int TP1 { get; set; }
		       [Parameter("StopLoss", DefaultValue = 200)]
        public int SL1 { get; set; }

		[Parameter("LR_Method", DefaultValue = MaMethods.Lwma)]
		public MaMethods MethodMA1 { get; set; }
		[Parameter("LR_Method", DefaultValue = MaMethods.Ema)]
		public MaMethods MethodMA2 { get; set; }
        [Parameter("LR_Apply_to", DefaultValue = PriceMode.Low)]
        public PriceMode ApplyMA1To { get; set; }
        [Parameter("LR_Apply_to", DefaultValue = PriceMode.Close)]
        public PriceMode ApplyMA2To { get; set; }	
		
		[Parameter("Debug_Mode", DefaultValue = false)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = true)]
		public bool LogMode { get;set; }

		[Parameter("Log_FileName", DefaultValue = @"Puria_")]
		public string LogFileName { get;set; }
		private string trueLogPath = "";
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		
		// Период MA1 — 85, Метод MA — Linear Weighted, применить к Low, цвет красный.
		// Период MA2 — 75, Метод MA — Linear Weighted, применить к Low, цвет красный.
		// Период MA3 — 5, Метод MA — Exponential, применить к Close, цвет желтый.
		//       MACD - Быстрый EMA 15, Медленный EMA 26, MACD SMA 1.
		private MovingAverage _ma1, _ma2, _ma3, _ma200;
		private MovingAverageConvergenceDivergence _macd;
		public double _macdInd1,_macdInd2,_macdInd3,_macdInd4;
		public double _maInd1_1,_maInd2_1,_maInd3_1,_maInd200,_maInd200_4;
		public double _maInd1_2,_maInd2_2,_maInd3_2; 
		public double _maInd1_3,_maInd2_3,_maInd3_3; 
		public double _maInd1_4,_maInd2_4,_maInd3_4;
		public double sF;
		private List<Guid> listOfBuyStop = new List<Guid>();
		private List<Guid> listOfSellStop = new List<Guid>();
		public AwesomeOscillator _awoInd;
		public FisherTransformOscillator _ftoInd;
		public Fractals _frInd;
		public bool Speed=false,verh=false,niz=false;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public DateTime DTime; // Время
		
				// Fractal
		double frUpH = 0.0;   // Значение текущего верхнего Fractal
		double frDownL = 0.0;  // Значение текущего нижнего Fractal
		public bool frUp=false,L1,L2,H1,H2;
		public bool frDown=true,rh=false,rl=false;
	
		
		
        protected override void Init()
        {           	
			     InitLogFile();
#region	GetIndicatorsAndFrames // Get Indicators' references...        			
            // Event occurs once at the start of the strategy
			//XXPrint("Starting TS on account: {0}, comment: {1}, buystop : {2} sellstop : {3}", 
			//	this.Account.Number, "CommentText", listOfBuyStop.Count, listOfSellStop.Count);
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			
		
			
			 _ma1 = GetIndicator<MovingAverage>(Instrument.Id, Timeframe, 10, 0, MethodMA2, ApplyMA2To);
			 _ma2 = GetIndicator<MovingAverage>(Instrument.Id, Timeframe, 20, 0, MethodMA2, ApplyMA2To);
			 _ma3 = GetIndicator<MovingAverage>(Instrument.Id, Timeframe, 30, 0, MethodMA2, ApplyMA2To);
			 _ma200 = GetIndicator<MovingAverage>(Instrument.Id, Timeframe,  200, 0, MethodMA2, ApplyMA2To);
			 //_macd = GetIndicator<MovingAverageConvergenceDivergence> (Instrument.Id, Timeframe);
			 //_macd.FastEmaPeriod=15;
			 //_macd.AppliedPrice=ApplyMA2To;
			 //_macd.SlowEmaPeriod=26;
			 //_macd.SmaPeriod=1;
				//_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);
			_ftoInd= GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			
			
#endregion //GetIndicatorsAndFrames				
			
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {   DTime = Bars[Bars.Range.To-1].Time;
			
            // Event occurs on every new bar
			_maInd1_1 = _ma1.SeriesMa[Bars.Range.To-1];
			_maInd2_1 = _ma2.SeriesMa[Bars.Range.To-1];
			_maInd3_1 = _ma3.SeriesMa[Bars.Range.To-1];		
			
			_maInd1_2 = _ma1.SeriesMa[Bars.Range.To-2];
			_maInd2_2 = _ma2.SeriesMa[Bars.Range.To-2];
			_maInd3_2 = _ma3.SeriesMa[Bars.Range.To-2];
			
			_maInd1_3 = _ma1.SeriesMa[Bars.Range.To-3];
			_maInd2_3 = _ma2.SeriesMa[Bars.Range.To-3];
			_maInd3_3 = _ma3.SeriesMa[Bars.Range.To-3];
	
			_maInd1_4 = _ma1.SeriesMa[Bars.Range.To-4];
			_maInd2_4 = _ma2.SeriesMa[Bars.Range.To-4];
			_maInd3_4 = _ma3.SeriesMa[Bars.Range.To-4];
			
			_maInd200 = _ma200.SeriesMa[Bars.Range.To-1];
			_maInd200_4 = _ma200.SeriesMa[Bars.Range.To-4];
			
			if(_maInd200_4>_maInd1_4 && _maInd200>_maInd1_1)  rl=((_maInd200_4-_maInd1_4)<(_maInd200-_maInd1_1)); else rl=false;
			if(_maInd200_4<_maInd1_4 && _maInd200<_maInd1_1)  rh=((_maInd1_4-_maInd200_4)<(_maInd1_1-_maInd200)); else rh=false;
			//_maInd1,_maInd2,_maInd3,_maInd1_1,_maInd2_1,_maInd3_1,_maInd1_2,_maInd2_2,_maInd3_2,_maInd1_3,_maInd2_3,_maInd3_3,
			
			//_macdInd1 = _macd.SeriesSignal[Bars.Range.To-1];
			//_macdInd2 = _macd.SeriesSignal[Bars.Range.To-2];
			//_macdInd3 = _macd.SeriesSignal[Bars.Range.To-3];
			//_macdInd4 = _macd.SeriesSignal[Bars.Range.To-4];
			//_macdInd,_macdInd1,_macdInd2,_macdInd3
			
			sF  = _ftoInd.FisherSeries[Bars.Range.To-1];
			
//=== Определение уровня STOP ===========================================================================================================				
		 	if(_frInd.TopSeries[Bars.Range.To-5]>0) { frUp=true; frUpH=Bars[Bars.Range.To-5].High; } else frUp=false;
   		 	if(_frInd.BottomSeries[Bars.Range.To-5]>0) { frDown=true; frDownL=Bars[Bars.Range.To-5].Low; } else frDown=false;
	
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
//=== КОРЕКЦИЯ STOP ===========================================================================================================		
			 var posBuy = Trade.GetPosition(posGuidBuy); 
			var posSell = Trade.GetPosition(posGuidSell); 
			  if(frUp && posGuidSell!=Guid.Empty && frUpH<(posSell.OpenPrice+0.00200))   Trade.UpdateMarketPosition(posGuidSell, frUpH, null, null); 
			  if(frDown && posGuidBuy!=Guid.Empty && frDownL>(posBuy.OpenPrice-0.00200))  Trade.UpdateMarketPosition(posGuidBuy, frDownL, null, null); 	
//=== БЕЗУБЫТОК STOP ===========================================================================================================				  
//		      if (posGuidBuy!=Guid.Empty)  { var posBuy = Trade.GetPosition(posGuidBuy);   
//				             Print("Buy Price={0} Pips{1}", posBuy.OpenPrice,posBuy.Pips);
//				             if(posBuy.Pips>100 && posBuy.StopLoss<posBuy.OpenPrice) 
//								       Trade.UpdateMarketPosition(posGuidBuy, posBuy.OpenPrice, null, null); }
//		      if (posGuidSell!=Guid.Empty) { var posSell = Trade.GetPosition(posGuidSell);   
//				             Print("Sell Price={0} Pips{1}", posSell.OpenPrice,posSell.Pips);
//				             if(posSell.Pips>100  && posSell.StopLoss>posSell.OpenPrice) 
//								       Trade.UpdateMarketPosition(posGuidSell, posSell.OpenPrice, null, null); }
			 
//  1
			  if(_maInd3_1>_maInd2_1 && _maInd2_1>_maInd1_1) L1=true; else L1=false;
			  if(_maInd3_4>_maInd2_4 && _maInd2_4>_maInd1_4) L2=false; else L2=true;
			  
			  if(_maInd1_1>_maInd2_1 && _maInd2_1>_maInd3_1) H1=true;  else H1=false;
			  if(_maInd1_4>_maInd2_4 && _maInd2_4>_maInd3_4) H2=false; else H2=true;
 
			  		  
	if( L1 && L2 &&  sF<0 &&  _maInd200>_maInd3_1)
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Blue;
				if (posGuidSell==Guid.Empty){
				var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1, Stops.InPrice(frUpH,null), null, null);
				//	var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1, Stops.InPips(200,null), null, null);
				   if (result.IsSuccessful)  posGuidSell = result.Position.Id;
				}
			} 
//  2	
	if( H1 && H2 &&  sF>0 && _maInd200<_maInd3_1 )			
	   {
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Red;
								if (posGuidBuy==Guid.Empty){
				var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Bid, -1, Stops.InPrice(frDownL,null), null, null);
				//var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Bid, -1, Stops.InPips(200,null), null, null);					
								if (result.IsSuccessful)  posGuidBuy = result.Position.Id;
								}
								}
		
//Print("{0} {1} {2} {3} {4} {5} {6} {7}",Bars[Bars.Range.To-1].Time,_maInd3_3,_maInd3_2,_maInd3_1,_maInd1_1,_maInd2_1,_macdInd1,sF);
//Print("{0} - {1} {2} {3} {4}",Bars[Bars.Range.To-1].Time, (_maInd1_1>_maInd2_1), (_maInd3_3>_maInd1_3), (_maInd3_1>_maInd2_1), sF );
			//Print("{0}",Bars[Bars.Range.To-1].Time);	
			//Print("OK!");
		}
		
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
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
		
		protected void InitLogFile()
		{
			//trueLogPath=PathToLogFile+"\\"+LogFileName+DateTime.Now.Ticks.ToString().Trim()+".LOG";
			trueLogPath=PathToLogFile+"\\"+LogFileName+Instrument.Name.ToString()+".LOG";
		}	
	
	
	}
}
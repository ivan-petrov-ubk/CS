// https://www.youtube.com/watch?v=lstXN2x6oqQ
// http://strategy4you.ru/skalpiruyushhie-strategii-foreks/metod-puria.html
// 1) Открываем сделку, как только желтый мувинг пересекает 2 красных мувинга снизу вверх и при этом получено 
//     подтверждение от индикатора MACD (один бар закрылся выше Нулевого уровня)
// 2) Стоп-лосс ставим максимум — 140 пипсов, закрытие сделки по стоп-лоссу случается очень редко.
// 3) Закрытие сделки по Тейк-профиту — в зависисмости от выбранной валютной пары — смотрите таблицу в начале стратегии форекс.
// EURUSD — M30 — 15  - 583
// GBPUSD — М30 — 20

// AUDJPY — M30 — 15
// NZDUSD — H1  — 25 
// USDCAD — H1  — 20 
// EURGBP — H1  — 10 
// USDJPY — M30 — 15

// USDCHF — M30 — 10
// EURCHF — H1  — 15 
// AUDUSD — M30 — 10
// EURJPY — M30 — 15
// CHFJPY — 1H  — 15
// CADJPY — M30 — 20


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
    [TradeSystem("Puria5_Fractal_Revers")]    //copy of "Puria5_Fractal"
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
		private MovingAverage _ma1, _ma2, _ma3;
		private MovingAverageConvergenceDivergence _macd;
		public double _macdInd1,_macdInd2,_macdInd3,_macdInd4;
		public double _maInd1_1,_maInd2_1,_maInd3_1;
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
		public bool frUp=false;
		public bool frDown=true;
	
		
		
        protected override void Init()
        {           	
			     InitLogFile();
#region	GetIndicatorsAndFrames // Get Indicators' references...        			
            // Event occurs once at the start of the strategy
			//XXPrint("Starting TS on account: {0}, comment: {1}, buystop : {2} sellstop : {3}", 
			//	this.Account.Number, "CommentText", listOfBuyStop.Count, listOfSellStop.Count);
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			
		
			
			 _ma1 = GetIndicator<MovingAverage>(Instrument.Id, Timeframe, 85, 0, MethodMA1, ApplyMA1To);
			 _ma2 = GetIndicator<MovingAverage>(Instrument.Id, Timeframe, 75, 0, MethodMA1, ApplyMA1To);
			 _ma3 = GetIndicator<MovingAverage>(Instrument.Id, Timeframe,  5, 0, MethodMA2, ApplyMA2To);
			
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
			  if(frUp && posGuidSell!=Guid.Empty)   Trade.UpdateMarketPosition(posGuidSell, frUpH, null, null); 
			  if(frDown && posGuidBuy!=Guid.Empty)  Trade.UpdateMarketPosition(posGuidBuy, frDownL, null, null); 	
//=== БЕЗУБЫТОК STOP ===========================================================================================================				  
//		      if (posGuidBuy!=Guid.Empty)  { var posBuy = Trade.GetPosition(posGuidBuy);   Print("Buy Price={0} Pips{1}", posBuy.OpenPrice,posBuy.Pips);
//				             if(posBuy.Pips>100 && posBuy.StopLoss<posBuy.OpenPrice) Trade.UpdateMarketPosition(posGuidBuy, posBuy.OpenPrice, null, null); }
//		      if (posGuidSell!=Guid.Empty) { var posSell = Trade.GetPosition(posGuidSell);   Print("Sell Price={0} Pips{1}", posSell.OpenPrice,posSell.Pips);
//				             if(posSell.Pips>100  && posSell.StopLoss>posSell.OpenPrice) Trade.UpdateMarketPosition(posGuidSell, posSell.OpenPrice, null, null); }
			 
//  1
	if( _maInd2_1>_maInd1_1 && _maInd3_3>_maInd2_3 && _maInd3_1<_maInd1_1  &&  sF<0)
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Yellow;
				if (posGuidBuy==Guid.Empty){
				//var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1, Stops.InPrice(frUpH,null), null, null);
					var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Ask, -1, Stops.InPips(200,140), null, null);
				   if (result.IsSuccessful)  posGuidBuy = result.Position.Id;
				}
			} 
//  2	
	if(_maInd2_1>_maInd1_1 && _maInd3_3<_maInd1_3 && _maInd3_1>_maInd2_1  &&  sF>0)
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Red;
								if (posGuidSell==Guid.Empty){
				//var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Bid, -1, Stops.InPrice(frDownL,null), null, null);
				var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Bid, -1, Stops.InPips(200,140), null, null);					
								if (result.IsSuccessful)  posGuidSell = result.Position.Id;
								}
								}
			
//  3
	if( _maInd2_1<_maInd1_1  && _maInd3_3>_maInd1_3 && _maInd3_1<_maInd2_1  && sF<0)
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Blue;
				if (posGuidBuy==Guid.Empty){
				//var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1,Stops.InPrice(frUpH,null), null, null);
					var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Ask, -1, Stops.InPips(200,140), null, null);
				   if (result.IsSuccessful)  posGuidBuy = result.Position.Id;
				}
			}


//  4
	if( _maInd2_1<_maInd1_1  && _maInd3_3<_maInd2_3 && _maInd3_1>_maInd1_1  && sF>0 )
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.White;
								if (posGuidSell==Guid.Empty){
				//var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Bid, -1, Stops.InPrice(frDownL,null), null, null);
				var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Bid, -1, Stops.InPips(200,140), null, null);					
								if (result.IsSuccessful)  posGuidSell = result.Position.Id;
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
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
    [TradeSystem("Puria4")]  //copy of "Puria3"
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
			
			 _macd = GetIndicator<MovingAverageConvergenceDivergence> (Instrument.Id, Timeframe);
			 _macd.FastEmaPeriod=15;
			 _macd.AppliedPrice=ApplyMA2To;
			 _macd.SlowEmaPeriod=26;
			 _macd.SmaPeriod=1;
				_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);
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
			
			_macdInd1 = _macd.SeriesSignal[Bars.Range.To-1];
			_macdInd2 = _macd.SeriesSignal[Bars.Range.To-2];
			_macdInd3 = _macd.SeriesSignal[Bars.Range.To-3];
			_macdInd4 = _macd.SeriesSignal[Bars.Range.To-4];
			//_macdInd,_macdInd1,_macdInd2,_macdInd3
			
						sF  = _ftoInd.FisherSeries[Bars.Range.To-1];
			
			// Вверх 85<75
			//Speed = true;
/*			if (_ma3.SeriesMa[Bars.Range.To-1]<_ma3.SeriesMa[Bars.Range.To-2] &&  _ma3.SeriesMa[Bars.Range.To-2]<_ma3.SeriesMa[Bars.Range.To-3] && _ma3.SeriesMa[Bars.Range.To-3]<_ma3.SeriesMa[Bars.Range.To-4] && _ma3.SeriesMa[Bars.Range.To-4]<_ma3.SeriesMa[Bars.Range.To-5]) Speed=true;
	        if (_ma3.SeriesMa[Bars.Range.To-1]>_ma3.SeriesMa[Bars.Range.To-2] &&  _ma3.SeriesMa[Bars.Range.To-2]>_ma3.SeriesMa[Bars.Range.To-3] && _ma3.SeriesMa[Bars.Range.To-3]>_ma3.SeriesMa[Bars.Range.To-4] && _ma3.SeriesMa[Bars.Range.To-4]>_ma3.SeriesMa[Bars.Range.To-5]) Speed=true;
*/
			
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
								
			
	
if( _maInd3_4<_maInd3_3 && _maInd3_3<_maInd3_2 && _maInd3_2<_maInd3_1 ) verh=true; else verh=false;
if( _maInd3_4>_maInd3_3 && _maInd3_3>_maInd3_2 && _maInd3_2>_maInd3_1 )	niz=true; else niz=false;

//  1
if( verh && _maInd2_1>_maInd1_1 && _maInd3_3<_maInd1_3 && _maInd3_1>_maInd2_1 && _macdInd1>0 && sF>0)
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Red;
				if (posGuidBuy==Guid.Empty){
				var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Ask, -1, Stops.InPips(SL1,TP1), null, null);
				   if (result.IsSuccessful)  posGuidBuy = result.Position.Id;
				}
			} 
	
//  2	
if( niz  && _maInd2_1>_maInd1_1 && _maInd3_3>_maInd2_3 && _maInd3_1<_maInd1_1  && _macdInd1<0 && sF<0)
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Blue;
								if (posGuidSell==Guid.Empty){
				var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1, Stops.InPips(SL1,TP1), null, null);
								if (result.IsSuccessful)  posGuidSell = result.Position.Id;
								}
								}

//  3
if( verh && _maInd1_1>_maInd2_1 && _maInd3_3<_maInd2_3 && _maInd3_1>_maInd1_1  && _macdInd1>0 && sF>0)	
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Yellow;
				if (posGuidBuy==Guid.Empty){
				var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Ask, -1, Stops.InPips(SL1,TP1), null, null);
				   if (result.IsSuccessful)  posGuidBuy = result.Position.Id;
				}
			}


//  4
if( niz  && _maInd1_1>_maInd2_1 && _maInd3_3>_maInd1_3 && _maInd3_1<_maInd2_1  && _macdInd1<0 && sF<0 )
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.White;
								if (posGuidSell==Guid.Empty){
				var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1, Stops.InPips(SL1,TP1), null, null);
								if (result.IsSuccessful)  posGuidSell = result.Position.Id;
								}
				}

/*	
			//  1 ==================================================================================================================
if (Speed) {
			if( _maInd1>_maInd2 && _maInd3_1<_maInd1_1 && _maInd3>_maInd1 &&  _macdInd>0) 
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Red;
				Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Ask, -1, Stops.InPips(150,TP1), null, null);
				//Print("MA1={0} MA2={1} MA3={2} MACD={3}  - {4} -- ---     BAY  1",_maInd1,_maInd2,_maInd3, _macdInd, Bars[Bars.Range.To-1].Time);
        	//XXPrint("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}","BUY",Bars[Bars.Range.To-1].Time,Instrument.Name,Instrument.Ask,_maInd3_3,_maInd3_2,_maInd3_1,_maInd3,_maInd1,_maInd2);
			}

//  2 ==================================================================================================================
			if( _maInd1>_maInd2 && _maInd3_1>_maInd2_1 && _maInd3<_maInd2  && _macdInd<0) 
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Blue;
				Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1, Stops.InPips(150,TP1), null, null);
				//Print("MA1={0} MA2={1} MA3={2} MACD={3}  - {4} -- ---     SELL  2",_maInd1,_maInd2,_maInd3, _macdInd, Bars[Bars.Range.To-1].Time);
			//XXPrint("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}","SELL",Bars[Bars.Range.To-1].Time,Instrument.Name,Instrument.Ask,_maInd3_3,_maInd3_2,_maInd3_1,_maInd3,_maInd1,_maInd2);
			}

			//  3 ==================================================================================================================
			if( _maInd2>_maInd1 && _maInd3_1<_maInd2_1 && _maInd3>_maInd2 && _macdInd>0) 
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Yellow;
				Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Bid, -1, Stops.InPips(150,TP1), null, null);
				//Print("MA1={0} MA2={1} MA3={2} MACD={3}  - {4}  ----  BUY  3",_maInd1,_maInd2,_maInd3, _macdInd, Bars[Bars.Range.To-1].Time);
			//XXPrint("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}","BUY",Bars[Bars.Range.To-1].Time,Instrument.Name,Instrument.Ask,_maInd3_3,_maInd3_2,_maInd3_1,_maInd3,_maInd1,_maInd2);
			}

			//  4 ==================================================================================================================
			if( _maInd2>_maInd1 && _maInd3_1>_maInd1_1 && _maInd3<_maInd1  && _macdInd<0) 
			{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.White;
				Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1, Stops.InPips(150,TP1), null, null);
			//	Print("MA1={0} MA2={1} MA3={2} MACD={3}  - {4} -- ---     SELL 4",Math.Round(_maInd1,5),Math.Round(_maInd2,5),Math.Round(_maInd3,5), Math.Round(_macdInd,5), Bars[Bars.Range.To-1].Time);
			XXPrint("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}","SELL",Bars[Bars.Range.To-1].Time,Instrument.Name,Instrument.Ask,_maInd3_3,_maInd3_2,_maInd3_1,_maInd3,_maInd1,_maInd2);
			}
			
		}
*/				
				
//			Print("MA1_1={0} MA2_1={1} MA3_1={2} -- MA1={3} MA2={4} MA3={5} MACD={6} - {7}",Math.Round(_maInd1_1,5),Math.Round(_maInd2_1,5),Math.Round(_maInd3_1,5),Math.Round(_maInd1,5),Math.Round(_maInd2,5),Math.Round(_maInd3,5), Math.Round(_macdInd,5), Bars[Bars.Range.To-1].Time);
			
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
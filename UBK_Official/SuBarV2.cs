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
    [TradeSystem("SuBarV2")]
    public class SuBarV2 : TradeSystem
    {
/// <summary>
/// Envelope's No.1 description 
/// </summary>		
		[Parameter("TP_Envelope_period", DefaultValue = 23, MinValue = 1, MaxValue = 100)]
		public int envelopesTPperiod { get;set; }
		
		[Parameter("TP_Envelope_deviation", DefaultValue = 0.300, MinValue = 0.000, MaxValue = 3.000)]
		public double envelopesTPdev { get;set; }

		[Parameter("TP_Envelope_Shift", DefaultValue = 0, MinValue = -100, MaxValue = 100)]
		public int envelopesTPshift { get;set; }

		[Parameter("TP_Envelope_Method", DefaultValue = MaMethods.Ema)]
		public MaMethods envelopesTPmethod { get;set; }

		[Parameter("TP_Envelope_Method", DefaultValue = PriceMode.Median)]
		public PriceMode envelopesTPprice { get;set; }

/// <summary>
/// Envelope's No.2 description 
/// </summary>		
		[Parameter("SL_Envelope_period", DefaultValue = 23, MinValue = 1, MaxValue = 100)]
		public int envelopesSLperiod { get;set; }
		
		[Parameter("SL_Envelope_deviation", DefaultValue = 0.90, MinValue = 0.000, MaxValue = 3.000)]
		public double envelopesSLdev { get;set; }

		[Parameter("SL_Envelope_Shift", DefaultValue = 0, MinValue = -100, MaxValue = 100)]
		public int envelopesSLshift { get;set; }

		[Parameter("SL_Envelope_Method", DefaultValue = MaMethods.Ema)]
		public MaMethods envelopesSLmethod { get;set; }

		[Parameter("SL_Envelope_Method", DefaultValue = PriceMode.Median)]
		public PriceMode envelopesSLprice { get;set; }

/// <summary>
/// Envelope's No.3 description 
/// </summary>		
		[Parameter("Signal_Envelope_period", DefaultValue = 23, MinValue = 1, MaxValue = 100)]
		public int envelopesSIGNperiod { get;set; }
		
		[Parameter("Signal_Envelope_deviation", DefaultValue = 0.15, MinValue = 0.000, MaxValue = 3.000)]
		public double envelopesSIGNdev { get;set; }

		[Parameter("Signal_Envelope_Shift", DefaultValue = 0, MinValue = -100, MaxValue = 100)]
		public int envelopesSIGNshift { get;set; }

		[Parameter("Signal_Envelope_Method", DefaultValue = MaMethods.Ema)]
		public MaMethods envelopesSIGNmethod { get;set; }

		[Parameter("Signal_Envelope_Method", DefaultValue = PriceMode.Median)]
		public PriceMode envelopesSIGNprice { get;set; }

		[Parameter("P*WHERE**", DefaultValue = 0.33, MinValue = -0.33, MaxValue = 1.33)]
		public double CoefB { get;set; }

		[Parameter("B*UNTILL**", DefaultValue = 1, MinValue = 0, MaxValue = 1000)]
		public int  UntillBar { get;set; }

		[Parameter("Use_Pending", DefaultValue = false)]
		public bool UsePending { get;set; }

		[Parameter("Use_Twins", DefaultValue = false)]
		public bool UseTwins { get;set; }

		[Parameter("Use_Trail", DefaultValue = false)]
		public bool UseTrail { get;set; }

		[Parameter("Trail_SLTP", DefaultValue = false)]
		public bool TrailSLTP { get;set; }

		[Parameter("Lots", DefaultValue = 0.1, MinValue=0.01, MaxValue = 2.0)]
		public double vol { get;set; }
		
/// <summary>
/// Debugging messages settings....
/// </summary>
		[Parameter("Debug_Mode", DefaultValue = false)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = true)]
		public bool LogMode { get;set; }

		[Parameter("Log_FileName", DefaultValue = @"SuBaRv2")]
		public string LogFileName { get;set; }
		
	
		private ISeries<Bar> _barSeries;
		private Period _period = new Period(PeriodType.Minute, 30);
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		private string trueLogPath = "";
		
		private Envelopes EnvindTP;
		private Envelopes EnvindSL;
		private Envelopes EnvindSIGN;

		private List<Guid> listOfBuyStop = new List<Guid>();
		private List<Guid> listOfSellStop = new List<Guid>();
		
		
		private bool UP_DOWN = false;
		private bool DOWN_UP = false;
		
		private	double B2H = 0.0;
		private	double B2L = 0.0;
		private	double B1H = 0.0;
		private	double B1L = 0.0;
		private	double B1C = 0.0;

		private double sellOpenLevel = 0.0; 
		private double buyOpenLevel = 0.0; 
		
		private double medianUP = 0.0;
		private double medianDN = 0.0;
		
		private int sellUntill = 0;
		private int buyUntill = 0;

		private int _CIndex = 2;
		
		private int magicNumber = 999;

		private static object TickLock = new Object();

		public static int BarTickCount = 0;
		public static int MaxFreeDelay = 1;
		
/// <summary>
///  Main Initialization...
/// </summary>
		protected override void Init()
        {
        	InitLogFile();
#region	GetIndicatorsAndFrames // Get Indicators' references...           

			XXPrint("Starting TS on account: {0}", this.Account.Number);
			
			_period = Timeframe;
			_barSeries = GetCustomSeries(Instrument.Id, 
											_period);
			
			EnvindTP = GetIndicator<Envelopes>(Instrument.Id,
											_period,
													envelopesTPperiod,
														envelopesTPdev,
															envelopesTPshift,
																envelopesTPmethod,
																	envelopesTPprice);
			EnvindSL = GetIndicator<Envelopes>(Instrument.Id,
											_period,
													envelopesSLperiod,
														envelopesSLdev,
															envelopesSLshift,
																envelopesSLmethod,
																	envelopesSLprice);
			EnvindSIGN = GetIndicator<Envelopes>(Instrument.Id,
											_period,
													envelopesSIGNperiod,
														envelopesSIGNdev,
															envelopesSIGNshift,
																envelopesSIGNmethod,
																	envelopesSIGNprice);

			// Event occurs once at the start of the strategy
#endregion	//GetIndicatorsAndFrames
		}        

		protected void InitLogFile()
		{
			trueLogPath=PathToLogFile+"\\"+LogFileName+DateTime.Now.Ticks.ToString().Trim()+".LOG";
		}
		
		[STAThread]  
        protected override void NewQuote()
        {
            // Event occurs on every new quote
 			
			BarTickCount++;
			if(BarTickCount <= MaxFreeDelay) return;
			
			lock(TickLock)
			{

				if(_CIndex < sellUntill)
				{
					if(Instrument.Bid <= sellOpenLevel)
					{
						setMarketSellOrder();
						sellUntill = -1;
						sellOpenLevel = double.MinValue; 
					}	
				}			
        		if(_CIndex < buyUntill)
				{
					if(Instrument.Ask >= buyOpenLevel)
					{
						setMarketBuyOrder();
						buyUntill = -1;
						buyOpenLevel = double.MaxValue; 
					}	
				}			
			}
		}
		
		[STAThread]  
        protected override void NewBar()
        {
 			// Event occurs on every new bar
			lock(TickLock)
			{

				if (_CIndex >= _barSeries.Range.To - 1) 
				{
					return;
				}
				else
				{
					_CIndex = _barSeries.Range.To - 1;
				}

				if(listOfSellStop.Count > 0 && _CIndex >= sellUntill) 
				{
					XXPrint("sellpending = {0}  Try to cancel...",
											listOfSellStop.Count);
				
					SearchForRemoveSellPending();
					sellUntill = -1;
				}
			
				if(listOfBuyStop.Count > 0 && _CIndex >= buyUntill) 
				{
					XXPrint("buypending = {0}  Try to cancel...",
											listOfBuyStop.Count);
					
					SearchForRemoveBuyPending();
					buyUntill = -1;
				}
			
				medianUP = EnvindSIGN.SeriesUp[_CIndex];
				medianDN = EnvindSIGN.SeriesDown[_CIndex];
			
				B2H = Bars[_CIndex-1].High;
				B2L = Bars[_CIndex-1].Low;
				B1H = Bars[_CIndex].High;
				B1L = Bars[_CIndex].Low;
				B1C = Bars[_CIndex].Close;

// calcuate SuBAR ...		
			
				DOWN_UP = 	((B1H > B2H) && (B1L > B2L)) 
								&& (B1C < (B1L + (B1H - B1L) * CoefB)) 
									&& (Instrument.Bid > medianUP);
				UP_DOWN = 	((B1H < B2H) && (B1L < B2L)) 
								&& (B1C > (B1H - (B1H - B1L) * CoefB)) 
									&& (Instrument.Bid < medianDN);
			
				if(DOWN_UP)
				{
					if(Instrument.Bid > B1L)
					{	
						if(UsePending)
						{	
							setPendingSellOrder();
							if(UseTwins) 
							{
								setBuyTrigger();
							}
						}
						else
						{	
							setSellTrigger();
						}
					}
					else
					{	
						setMarketSellOrder();
					}
				}
			
				if(UP_DOWN)
				{
					if(Instrument.Ask < B1H)
					{	
						if(UsePending)
						{	
							setPendingBuyOrder();
							if(UseTwins)
							{
								setSellTrigger();
							}
						}
						else
						{
							setBuyTrigger();
						}
					}
					else
					{	
						setMarketBuyOrder();
					}
				}
			
				if(UseTrail)
				{
					trailActivePositions();
				}
			}
		}
		
		protected void trailActivePositions()
		{
	
			double CurrentBuySL = CalculateSL(ExecutionRule.Buy);	
			double CurrentSellSL = CalculateSL(ExecutionRule.Sell);	
			double CurrentBuyTP = CalculateTP(ExecutionRule.Buy);	
			double CurrentSellTP = CalculateTP(ExecutionRule.Sell);	
			
			
			IPosition[] activePositions = Trade.GetActivePositions(magicNumber);
			
			if(activePositions!=null && activePositions.Length > 0)
			{
				for(int i = activePositions.Length-1; i >= 0; i--)
				{
					TradeResult tR = null;
					int attempts = 0;
					
					if((int)activePositions[i].Type==(int)ExecutionRule.Buy)
					{
						if(TrailSLTP)
						{
							if((activePositions[i].TakeProfit != CurrentBuyTP) 
									|| (activePositions[i].StopLoss != CurrentBuySL))
							{
								if((Instrument.Bid >= CurrentBuyTP) || (Instrument.Bid <= CurrentBuySL))
								{
									do
									{
										attempts++;
										tR = Trade.CloseMarketPosition(	activePositions[i].Id,
																		activePositions[i].Lots,
																		Instrument.Bid,
																		-1,
																		"Closed on changed TP,SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 10));
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't close position {0} => attempt to set TP = {2} SL={1} with Bid = {3}",
																	activePositions[i].Id,CurrentBuySL,CurrentBuyTP,Instrument.Bid);
									}
								}
								else
								{
									do
									{
										attempts++;
										tR = Trade.UpdateMarketPosition(activePositions[i].Id,
																		CurrentBuySL,
																		CurrentBuyTP,
																		"trail Buy TP,SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 10));
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't modify position {0} => attempt to set TP = {2} SL = {1}",
																activePositions[i].Id,CurrentBuySL,CurrentBuyTP);
									}
								}
							}				
						}	
						else
						{
							if(activePositions[i].StopLoss != CurrentBuySL)
							{
								if(Instrument.Bid <= CurrentBuySL)
								{
									do
									{
										attempts++;
										tR = Trade.CloseMarketPosition(	activePositions[i].Id,
																		activePositions[i].Lots,
																		Instrument.Bid,
																		-1,
																		"Closed on changed SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 10));
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't close position {0} => attempt to set SL={1} with Bid = {2}",
																	activePositions[i].Id,CurrentBuySL,Instrument.Bid);
									}
								}
								else
								{	
									do
									{
										attempts++;
										tR = Trade.UpdateMarketPosition(activePositions[i].Id,
																		CurrentBuySL,
																		activePositions[i].TakeProfit,
																		"trail Buy SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 10));
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't modify position {0} => attempt to set SL = {1}",
																	activePositions[i].Id,CurrentBuySL);
									}
								}
							}				
						}
					}

					if((int)activePositions[i].Type==(int)ExecutionRule.Sell)
					{
						if(TrailSLTP)
						{
							if((activePositions[i].TakeProfit != CurrentSellTP) 
									|| (activePositions[i].StopLoss != CurrentSellSL))
							{
								if((Instrument.Ask <= CurrentSellTP) || (Instrument.Ask >= CurrentSellSL))
								{
									do
									{
										attempts++;
										tR = Trade.CloseMarketPosition(	activePositions[i].Id,
																		activePositions[i].Lots,
																		Instrument.Ask,
																		-1,
																		"Closed on changed TP,SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 10));
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't close position {0} => attempt to set TP = {2} SL={1} with Ask = {3}",
																	activePositions[i].Id,CurrentSellSL,CurrentSellTP,Instrument.Ask);
									}
								}
								else
								{
									do
									{
										attempts++;
										tR = Trade.UpdateMarketPosition(activePositions[i].Id,
																		CurrentSellSL,
																		CurrentSellTP,
																		"trail Sell TP,SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 10));
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't modify position {0} => attempt to set TP = {2} SL = {1}",
																activePositions[i].Id,CurrentSellSL,CurrentSellTP);
									}
								}
							}				
						}	
						else
						{
							if(activePositions[i].StopLoss != CurrentSellSL)
							{
								if(Instrument.Ask >= CurrentSellSL)
								{
									do
									{
										attempts++;
										tR = Trade.CloseMarketPosition(	activePositions[i].Id,
																		activePositions[i].Lots,
																		Instrument.Ask,
																		-1,
																		"Closed on changed SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 10));
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't close position {0} => attempt to set SL={1} with Ask = {2}",
																	activePositions[i].Id,CurrentSellSL,Instrument.Ask);
									}
								}
								else
								{	
									do
									{
										attempts++;
										tR = Trade.UpdateMarketPosition(activePositions[i].Id,
																		CurrentSellSL,
																		activePositions[i].TakeProfit,
																		"trail Sell SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 10));
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't modify position {0} => attempt to set SL = {1}",
																	activePositions[i].Id,CurrentBuySL);
									}
								}
							}				
						}
					}
				}
			}
		}
		
		protected void setSellTrigger()
		{
			sellOpenLevel = B1L;
			sellUntill = _CIndex + UntillBar; 		
		}
		
		protected void setBuyTrigger()
		{
			buyOpenLevel = B1H;
			buyUntill = _CIndex + UntillBar; 		
		}
	
		protected void SearchForRemoveBuyPending()
		{
			foreach(Guid pos in listOfBuyStop)
			{

				IPosition position = Trade.GetPosition(pos);
				
				TradeResult tR = null;
				int attempts = 0;
				
				if(position!=null)
				{
					
					if((int)position.Type==(int)ExecutionRule.BuyStop)
					{
						do
						{
							attempts++;
							tR = Trade.CancelPendingPosition(position.Id);
						} 
						while((tR==null? true: !tR.IsSuccessful) && (attempts < 10));
						
						if(tR==null? true: !tR.IsSuccessful)
						{
							XXPrint("Can't remove BuyStop {0} after {1} attempts...",
																		pos,attempts);
						}					
						else
						{
							listOfBuyStop.Remove(pos);
							XXPrint("{0} BuyStop removed...",	
														pos);
						}
					}
					else
					{
							listOfBuyStop.Remove(pos);
							XXPrint("{0} BuyStop opened...",
														pos);
					}
				}	
				else
				{
					XXPrint("BuyStop position {0} from list not defined... ",
																		pos); 	
				}
			}
		}
		
		protected void SearchForRemoveSellPending()
		{
			foreach(Guid pos in listOfSellStop)
			{

				IPosition position = Trade.GetPosition(pos);
		
				TradeResult tR = null;
				int attempts = 0;
				
				if(position!=null)
				{
					
					if((int)position.Type==(int)ExecutionRule.SellStop)
					{
						do
						{
							attempts++;
							tR = Trade.CancelPendingPosition(position.Id);
						} 
						while((tR==null? true: !tR.IsSuccessful) && (attempts < 10));
						
						if(tR==null? true: !tR.IsSuccessful)
						{
							XXPrint("Can't remove SellStop {0} after {1} attempts...",
																		pos,attempts);
						}					
						else
						{
							listOfSellStop.Remove(pos);
							XXPrint("{0} SellStop removed...",	
															pos);
						}
					}
					else
					{
							listOfSellStop.Remove(pos);
							XXPrint("{0} SellStop opened...",
														pos);
					}
				}	
				else
				{
					XXPrint("SellStop position {0} from list not defined... ",
																			pos); 	
				}
			}
		}
		
		
		protected double CalculateTP(ExecutionRule orderType)
		{
			double TP = 0.3333;
			if((int)orderType==(int)ExecutionRule.Buy) 
			{
				TP = EnvindTP.SeriesUp[_CIndex];	
			} 	
			if((int)orderType==(int)ExecutionRule.Sell) 
			{
				TP = EnvindTP.SeriesDown[_CIndex];
			}
			return (double)Math.Round(TP,Instrument.PriceScale);
		
		}

		protected double CalculateSL(ExecutionRule orderType)
		{
			double SL = 0.3333;
			if((int)orderType==(int)ExecutionRule.Sell) 
			{
				SL = EnvindSL.SeriesUp[_CIndex];	
			} 	
			if((int)orderType==(int)ExecutionRule.Buy) 
			{
				SL = EnvindSL.SeriesDown[_CIndex];
			}
			return (double)Math.Round(SL,Instrument.PriceScale);
		}		

		protected void setPendingSellOrder()
		{
			
			double SL = CalculateSL(ExecutionRule.Sell);
			double TP = CalculateTP(ExecutionRule.Sell);
			double OP = B1L;
			
			if(Instrument.Bid >= SL)
			{
				SL = (Instrument.Bid - SL) + Instrument.Bid + 33 * Instrument.Point;
			}
			
			if(TP >= OP)
			{
				TP = OP - (TP - OP) - 33 * Instrument.Point;
			}
			
			Stops SLTP = Stops.InPrice(
									SL,
									TP
									);
			
			
			int attempt = 0;
			TradeResult tR = null;
			
			do 
			{
				attempt++;
				tR = Trade.OpenPendingPosition(	Instrument.Id,
												ExecutionRule.SellStop,
												vol,
												OP,
												-1,
												SLTP,
												Bars[_CIndex].Time.AddDays(90.0),
												"sellStop on bar signal",
												magicNumber);	
			
			}
			while ((tR==null? true: !tR.IsSuccessful) && attempt < 10);
			
			if((tR==null? true: !tR.IsSuccessful))
			{
				XXPrint("[{2}>>]{1} Cann't Send  SellStop order on Price {4} at Bid: {0} ",
											Instrument.Bid,DateTime.Now,Instrument.Name,OP);
			}
			else
			{
				TradeResult ntR = null;
				do 
				{
					attempt++;
					ntR = Trade.UpdatePendingPosition(	tR.Position.Id,
														vol,
														OP,
														-1,
														Bars[_CIndex].Time.AddDays(90.0),
														SL,
														TP,
														"sellStop set SL,TP");	
				}
				while ((ntR==null? true: !ntR.IsSuccessful) && attempt < 10);
				
				if(ntR==null? true: !ntR.IsSuccessful)
				{
					XXPrint("[{2}>>]{1} Cann't modify  SellStop order on Price {4} at Bid: {0} ",
												Instrument.Bid,DateTime.Now,Instrument.Name,OP);
				}
				else
				{
					listOfSellStop.Add(ntR.Position.Id);
				
					XXPrint("[{3}>>]{2} Sended successfully SellStop order for position {0} at Price: {1} ",
									tR.Position.Id,tR.Position.OpenPrice,Bars[_CIndex].Time, Instrument.Name); 
				}
			}
		}    
		
		protected void setMarketSellOrder()
		{

			double cAsk = Instrument.Ask;
			double cBid = Instrument.Bid;
			
			double SL = CalculateSL(ExecutionRule.Sell);
			double TPC = CalculateTP(ExecutionRule.Sell);
			double TP = 0.0;
			
			if(cAsk >= SL)
			{
				SL =  cAsk + (cAsk - SL) + 40 * Instrument.Point;
			}

			XXPrint("Sell s1 => TP= {0} SL = {1} on cBid = {2} (Point = {3}, TPC = {4}, spread = {5})", 
											TP, SL, cBid, Instrument.Point, TPC, Instrument.Spread);
			
			if(TPC >= (cBid - Instrument.Spread))
			{
				TP = cAsk - 60 * Instrument.Point;
			}
			else
			{
				TP = TPC;
			}
			
			Stops SLTP = Stops.InPrice(
									SL,
									TP
									);
			
			int attempt = 0;
			TradeResult tR = null;

			XXPrint("Sell s2 => TP= {0} SL = {1} on cBid = {2} (Point = {3}, TPC = {4}, spread = {5})", 
												TP, SL, cBid, Instrument.Point, TPC, Instrument.Spread);
			
			do 
			{
				attempt++;
				tR = Trade.OpenMarketPosition(	Instrument.Id,
												ExecutionRule.Sell,
												vol,
												cBid,
												-1,
												SLTP,
												"sell on bar signal",
												magicNumber);	
			}
			while ((tR==null? true: !tR.IsSuccessful) && attempt < 10);
			
			if((tR==null? true: !tR.IsSuccessful))
			{
				XXPrint("[{2}>>]{1} Cann't Send  Sell order  at Bid: {0} ",
								Instrument.Bid,DateTime.Now,Instrument.Name);
			}
			else
			{
				XXPrint("[{3}>>]{2} Sended successfully Sell order for position {0} at Bid: {1} ",
						tR.Position.Id,tR.Position.OpenPrice,tR.Position.OpenTime,Instrument.Name);
			}
		}    

		protected void setPendingBuyOrder()
		{
			
			double SL = CalculateSL(ExecutionRule.Buy);
			double TP = CalculateTP(ExecutionRule.Buy);
			double OP = B1H; 
			
			if(Instrument.Ask <= SL)
			{
				SL = Instrument.Ask - (SL - Instrument.Ask) - 33 * Instrument.Point;
			}
			if(TP <= OP)
			{
				TP = OP + (OP - TP) + 33 * Instrument.Point;
			}	
			
			Stops SLTP = Stops.InPrice(
									SL,
									TP
									);
			
			
			int attempt = 0;
			TradeResult tR = null;
			
			do 
			{
				attempt++;
				tR = Trade.OpenPendingPosition(	Instrument.Id,
												ExecutionRule.BuyStop,
												vol,
												OP,
												-1,
												SLTP,
												Bars[_CIndex].Time.AddDays(90.0),
												"buyStop on bar signal",
												magicNumber);	
			
			}
			while ((tR==null? true: !tR.IsSuccessful) && attempt < 100);
			
			if((tR==null? true: !tR.IsSuccessful))
			{
				XXPrint("[{2}>>]{1} Cann't Send BuyStop order on Price {4} at Ask: {0} ",
										Instrument.Bid,DateTime.Now,Instrument.Name,OP);
			}
			else
			{
				TradeResult ntR = null;
				do 
				{
					attempt++;
					ntR = Trade.UpdatePendingPosition(	tR.Position.Id,
														vol,
														OP,
														-1,
														Bars[_CIndex].Time.AddDays(90.0),
														SL,
														TP,
														"buyStop set SL,TP");	
				}
				while ((ntR==null? true: !ntR.IsSuccessful) && attempt < 10);
				
				if(ntR==null? true: !ntR.IsSuccessful)
				{	
					XXPrint("[{2}>>]{1} Cann't Modify BuyStop order on Price {4} at Ask: {0} ",
												Instrument.Bid,DateTime.Now,Instrument.Name,OP);
				}
				else
				{
					listOfBuyStop.Add(ntR.Position.Id);
				
					XXPrint("[{3}>>]{2} Sended successfully BuyStop order for position {0} at Price: {1} ",
									tR.Position.Id,tR.Position.OpenPrice,Bars[_CIndex].Time,Instrument.Name);
				}
			}
		}    
    	
		protected void setMarketBuyOrder()
		{

			double cAsk = Instrument.Ask;
			double cBid = Instrument.Bid;
			
			double SL = CalculateSL(ExecutionRule.Buy);
			double TPC = CalculateTP(ExecutionRule.Buy);
			double TP = 0.0;
			
			if(cBid <= SL)
			{
				SL = cBid - (SL - cBid) - 33 * Instrument.Point;
			}
			
			if(TPC <= (cBid + Instrument.Spread))
			{
				TP = cBid + 60 * Instrument.Point;
			}
			else
			{
				TP = TPC;
			}
			Stops TPSL=Stops.InPrice(
									SL,
									TP
									);


			int attempt = 0;
			TradeResult tR = null;

			XXPrint("Buy => TP= {0} SL = {1} on cAsk={2} (Point = {3}, TPC = {4})", 
												TP, SL, cAsk, Instrument.Point, TPC);

			do 
			{
				attempt++;
				tR = Trade.OpenMarketPosition(	Instrument.Id,
												ExecutionRule.Buy,
												vol,
												cAsk,
												-1,
												TPSL,
												"buy on bar signal",
												magicNumber);	
			}
			while ((tR==null? true: !tR.IsSuccessful) && attempt < 10);
			
			if((tR==null? true: !tR.IsSuccessful))
			{
				XXPrint("[{2}>>]{1} Cann't Send Buy order at Ask: {0} ",
							Instrument.Bid,DateTime.Now,Instrument.Name);
			}
			else
			{
				XXPrint("[{3}>>]{2} Sended successfully Buy order for position {0} at Ask: {1} ",
						tR.Position.Id,tR.Position.OpenPrice,tR.Position.OpenTime,Instrument.Name);
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
	}
}
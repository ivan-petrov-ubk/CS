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
    [TradeSystem("BolaV2")]
    public class BolaV2 : TradeSystem
    {

		[Parameter("BB_period", DefaultValue = 23, MinValue = 1, MaxValue = 100)]
		public int BBperiod { get;set; }
		
		[Parameter("BB_deviation", DefaultValue = 2.62, MinValue = 0.500, MaxValue = 5.000)]
		public double BBdev { get;set; }

		[Parameter("BB_Shift", DefaultValue = 0, MinValue = -100, MaxValue = 100)]
		public int BBshift { get;set; }

		[Parameter("BB_PriceMode", DefaultValue = PriceMode.Close)]
		public PriceMode BBprice { get;set; }

		[Parameter("Use_Trail", DefaultValue = false)]
		public bool UseTrail { get;set; }

		[Parameter("Trail_SLTP", DefaultValue = true)]
		public bool TrailSLTP { get;set; }


		[Parameter("Lots", DefaultValue = 0.1, MinValue=0.01, MaxValue = 2.0)]
		public double vol { get;set; }

		[Parameter("Bola_Only", DefaultValue = true)]
		public bool BolaOnly { get;set; }

		[Parameter("Static SL", DefaultValue = 500, MinValue=10, MaxValue = 5000)]
		public int staticSL { get;set; }

		[Parameter("Analyze Depth", DefaultValue = 3, MinValue=1, MaxValue = 50)]
		public int ExtHistoryDepth { get;set; }



/// <summary>
/// Debugging messages settings....
/// </summary>
		[Parameter("Debug_Mode", DefaultValue = false)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = true)]
		public bool LogMode { get;set; }

		[Parameter("Log_FileName", DefaultValue = @"BoLaLoG")]
		public string LogFileName { get;set; }
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
	
		private ISeries<Bar> _barSeries;
		private Period _period = new Period(PeriodType.Minute, 15);

		private List<Guid> listOfBuyStop = new List<Guid>();
		private List<Guid> listOfSellStop = new List<Guid>();
		
		private BollingerBands BolaInd;

		private string trueLogPath = "";

		private int _CIndex = 2;
		
		private static int magicNumber = 911;
		
		private static int MaxQuantityOfSellOrders = 1;
		private static int MaxQuantityOfBuyOrders = 1;

		private double BBHB = 0.0;
		private double BBLB = 0.0;			
			
		private double BH0 = 0.0;
		private double BL0 = 0.0;

		public static double ExtremumMin = 0.0;
		public static double ExtremumMax = 0.0;

		
        protected override void Init()
        {
        	InitLogFile();

#region	GetIndicatorsAndFrames // Get Indicators' references...           

			XXPrint("Starting TS on account: {0}, comment: {1}, buystop : {2} sellstop : {3}", 
				this.Account.Number, "CommentText", listOfBuyStop.Count, listOfSellStop.Count);
			
			_period = Timeframe;
			_barSeries = GetCustomSeries(Instrument.Id, 
											_period);
			
			BolaInd = GetIndicator<BollingerBands>( Instrument.Id,
													_period,
														BBperiod,
															BBdev,
																BBshift,
																	BBprice);
		
#endregion //GetIndicatorsAndFrames		
		}        

        protected override void NewBar()
        {
			if (_CIndex >= _barSeries.Range.To - 1) 
			{
				return;
			}
			else 
			{
				_CIndex = _barSeries.Range.To - 1;
			}
			
			BBHB = BolaInd.SeriesUp[_CIndex];
			BBLB = BolaInd.SeriesDown[_CIndex];			
			
			BH0 = Bars[_CIndex].High;
			BL0 = Bars[_CIndex].Low;
			
		
			if(BH0 >= BBHB)
			{
				if(listOfSellStop.Count < MaxQuantityOfSellOrders)	
				{
					setPendingSellOrder();
				}
			}
			
			if(BL0 <= BBLB)
			{
				if(listOfBuyStop.Count < MaxQuantityOfBuyOrders)	
				{
					setPendingBuyOrder();
				}
			}
			if(UseTrail)			
			{
				TrailPendingOrders();
				TrailActiveOrders();
			}
        }
        
		protected void setPendingSellOrder()
		{
			if(listOfSellStop.Count >= MaxQuantityOfSellOrders)
			{
				return;
			}
			
			double OP = CalculateOpenPrice(ExecutionRule.SellStop); 			
			double CurrentAsk = Instrument.Ask;
			double CurrentBid = Instrument.Bid;
			
			if(OP >= (CurrentBid - 2*Instrument.Spread))
			{
				OP = (double) Math.Round(CurrentAsk - 3.14 * Instrument.Spread, Instrument.PriceScale);
			}
			
			double SL = CalculateSL(ExecutionRule.SellStop, OP);
			
			if(OP >=SL )
			{
				SL = (double) Math.Round(OP + staticSL * Instrument.Point,Instrument.PriceScale);
			}
			
			double TP = CalculateTP(ExecutionRule.SellStop, OP);
			
			if(CurrentBid >= SL)
			{
				SL = (double) Math.Round((CurrentBid - SL) + CurrentBid + 3.14 * Instrument.Spread,Instrument.PriceScale);
			}
			
			if(TP >= OP)
			{
				TP = (double) Math.Round(OP - (TP - OP) - 3.14 * Instrument.Spread,Instrument.PriceScale);
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
				XXPrint("[{2}>>]{1} Cann't Send  SellStop order on Price {3} at Bid: {0} ",
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
					XXPrint("[{2}>>]{1} Cann't modify  SellStop order on Price {3} at Bid: {0}",
												Instrument.Bid,DateTime.Now,Instrument.Name,OP);
					
					attempt = 0;
					do
					{
						attempt++;
						ntR = Trade.CancelPendingPosition(tR.Position.Id);
					}
					while((ntR==null? true: !ntR.IsSuccessful) && attempt < 10); 
				
					if(ntR==null? true: !ntR.IsSuccessful)
					{
						XXPrint("[*EC*{2}>>]{1} Cann't cancel strange SellStop order on Price {3} at Bid: {0}",
																Instrument.Bid,DateTime.Now,Instrument.Name,OP);
					}
				}
				else
				{
					listOfSellStop.Add(ntR.Position.Id);
					XXPrint("[{3}>>]{2} Sended successfully SellStop order for position {0} at Price: {1}",
									tR.Position.Id,tR.Position.OpenPrice,Bars[_CIndex].Time, Instrument.Name); 
				}
			}
			
		}
        
		protected void setPendingBuyOrder()
		{
			if(listOfBuyStop.Count >= MaxQuantityOfBuyOrders)
			{
				return;
			}
			
			double OP = CalculateOpenPrice(ExecutionRule.BuyStop); 			
			double CurrentAsk = Instrument.Ask;
			double CurrentBid = Instrument.Bid;
			
			if(OP <= (CurrentAsk + 2*Instrument.Spread))
			{
				OP = Math.Round(CurrentAsk + 3.14 * Instrument.Spread, Instrument.PriceScale);
			}
			
			double SL = CalculateSL(ExecutionRule.BuyStop, OP);
			if(OP <= SL )
			{
				SL = (double) Math.Round(OP - staticSL * Instrument.Point, Instrument.PriceScale);
			}
			double TP = CalculateTP(ExecutionRule.BuyStop, OP);
			
			if(CurrentAsk <= SL)
			{
				SL = (double) Math.Round(CurrentAsk - (CurrentAsk - SL) -  3.14 * Instrument.Spread, Instrument.PriceScale);
			}
			
			if(TP <= OP)
			{
				TP = (double) Math.Round(OP + (OP - TP) + 3.14 * Instrument.Spread, Instrument.PriceScale);
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
			while ((tR==null? true: !tR.IsSuccessful) && attempt < 10);
			
			if((tR==null? true: !tR.IsSuccessful))
			{
				XXPrint("[{2}>>]{1} Cann't Send  BuyStop order on Price {3} at Bid: {0} ",
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
					XXPrint("[{2}>>]{1} Cann't modify  BuyStop order on Price {3} at Bid: {0} ",
												Instrument.Bid,DateTime.Now,Instrument.Name,OP);
					
					attempt = 0;
					do
					{
						attempt++;
						ntR = Trade.CancelPendingPosition(tR.Position.Id);
					}
					while((ntR==null? true: !ntR.IsSuccessful) && attempt < 10); 
				
					if(ntR==null? true: !ntR.IsSuccessful)
					{
						XXPrint("[*EC*{2}>>]{1} Cann't cancel strange BuyStop order on Price {3} at Bid: {0} ",
																Instrument.Bid,DateTime.Now,Instrument.Name,OP);
					}
				}
				else
				{
					listOfBuyStop.Add(ntR.Position.Id);
					XXPrint("[{3}>>]{2} Sended successfully BuyStop order for position {0} at Price: {1} ",
									tR.Position.Id,tR.Position.OpenPrice,Bars[_CIndex].Time, Instrument.Name); 
				}
			}
		}

		protected void TrailPendingOrders()
		{
			foreach(Guid pos in listOfBuyStop)
			{
				IPosition position = Trade.GetPosition(pos);

				if(position==null)
				{
					listOfBuyStop.Remove(pos);
					continue;
				}
				
				if((int)position.Type == (int)ExecutionRule.Buy)
				{
					XXPrint("BuyStop {0} opened ....",position.Id);
					listOfBuyStop.Remove(pos);
					continue;
				}
				
				TradeResult ntR = null;
				int attempts = 0;
				double CurrentAsk = Instrument.Ask;
				double CurrentBid = Instrument.Bid;

				double OP = CalculateOpenPrice(ExecutionRule.BuyStop);
				if(OP <= (CurrentAsk + 2*Instrument.Spread))
				{
					OP = Math.Round(CurrentAsk + 3.14 * Instrument.Spread, Instrument.PriceScale);
				}
				
				double SL = CalculateSL(ExecutionRule.BuyStop, OP); 
				double TP = Math.Round(BolaInd.SeriesUp[_CIndex], Instrument.PriceScale);

				if(OP <= SL)
				{
					SL = (double) Math.Round(OP - staticSL * Instrument.Point, Instrument.PriceScale);
				}
				if(TP <= OP)
				{
					TP = (double) Math.Round(OP + 6.28 * Instrument.Spread, Instrument.PriceScale);
				}
				
				do 
				{
					attempts++;
					ntR = Trade.UpdatePendingPosition(	position.Id,
														position.Lots,
														OP,
														-1,
														Bars[_CIndex].Time.AddDays(90.0),
														SL,
														TP,
														"buyStop set OP");	
				}
				while ((ntR==null? true: !ntR.IsSuccessful) && attempts < 10);
				
				if(ntR==null? true: !ntR.IsSuccessful)
				{
					XXPrint("[{2}>>]{1} Cann't modify  BuyStop order on Price {3} at Bid: {0} ",
												Instrument.Bid,DateTime.Now,Instrument.Name,OP);
					
					attempts = 0;
					do
					{
						attempts++;
						ntR = Trade.CancelPendingPosition(position.Id);
					}
					while((ntR==null? true: !ntR.IsSuccessful) && attempts < 10); 
				
					if(ntR==null? true: !ntR.IsSuccessful)
					{
						XXPrint("[*EC*{2}>>]{1} Cann't cancel strange BuyStop order on Price {3} at Bid: {0} ",
																Instrument.Bid,DateTime.Now,Instrument.Name,OP);
					}
				}
				else
				{
					XXPrint("[{3}>>]{2} Modified successfully BuyStop order for position {0} at Price: {1} ",
									ntR.Position.Id,ntR.Position.OpenPrice,Bars[_CIndex].Time, Instrument.Name); 
				}
			}
			foreach(Guid pos in listOfSellStop)
			{
				IPosition position = Trade.GetPosition(pos);
				if(position==null)
				{
					listOfSellStop.Remove(pos);
					continue;
				}
				if((int)position.Type == (int)ExecutionRule.Sell)
				{
					XXPrint("SellStop {0} opened ....",position.Id);
					listOfSellStop.Remove(pos);					
					continue;
				}
				TradeResult ntR = null;
				int attempts = 0;
				
				double CurrentAsk = Instrument.Ask;
				double CurrentBid = Instrument.Bid;
				
				double OP = CalculateOpenPrice(ExecutionRule.SellStop);
				if(OP >= (CurrentBid - 2*Instrument.Spread))
				{
					OP = Math.Round(CurrentBid - 3.14 * Instrument.Spread, Instrument.PriceScale);
				}
			
				double SL = CalculateSL(ExecutionRule.SellStop, OP);
				double TP = Math.Round(BolaInd.SeriesDown[_CIndex], Instrument.PriceScale);

				if(OP >= SL)
				{
					SL = OP + staticSL * Instrument.Point;
				}
				if(TP >= OP)
				{
					TP = (double) Math.Round(OP - 6.28 * Instrument.Spread, Instrument.PriceScale);
				}

				do 
				{
					attempts++;
					ntR = Trade.UpdatePendingPosition(	position.Id,
														position.Lots,
														OP,
														-1,
														Bars[_CIndex].Time.AddDays(90.0),
														SL,
														TP,
														"SellStop set OP");	
				}
				while ((ntR==null? true: !ntR.IsSuccessful) && attempts < 10);
				
				if(ntR==null? true: !ntR.IsSuccessful)
				{
					XXPrint("[{2}>>]{1} Cann't modify  SellStop order on Price {3} at Bid: {0} ",
												Instrument.Bid,DateTime.Now,Instrument.Name,OP);
					
					attempts = 0;
					do
					{
						attempts++;
						ntR = Trade.CancelPendingPosition(position.Id);
					}
					while((ntR==null? true: !ntR.IsSuccessful) && attempts < 10); 
				
					if(ntR==null? true: !ntR.IsSuccessful)
					{
						XXPrint("[*EC*{2}>>]{1} Cann't cancel strange SellStop order on Price {3} at Bid: {0} ",
																Instrument.Bid,DateTime.Now,Instrument.Name,OP);
					}
				}
				else
				{
					XXPrint("[{3}>>]{2} Modified successfully SellStop order for position {0} at Price: {1} ",
									ntR.Position.Id,ntR.Position.OpenPrice,Bars[_CIndex].Time, Instrument.Name); 
				}
				
			}
			
			
		}

		protected void TrailActiveOrders()
		{
			
			double CurrentAsk = Instrument.Ask;
			double CurrentBid = Instrument.Bid;
			
			double CurrentBuySL = CalculateSL(ExecutionRule.Buy,Instrument.Ask);	
			double CurrentSellSL = CalculateSL(ExecutionRule.Sell,Instrument.Bid);	
			double CurrentBuyTP = CalculateTP(ExecutionRule.Buy,Instrument.Bid);	
			double CurrentSellTP = CalculateTP(ExecutionRule.Sell,Instrument.Ask);	
			
			
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
								if((CurrentBid >= CurrentBuyTP) || (CurrentBid <= CurrentBuySL))
								{
									do
									{
										attempts++;
										tR = Trade.CloseMarketPosition(	activePositions[i].Id,
																		activePositions[i].Lots,
																		CurrentBid,
																		-1,
																		"Closed on changed TP,SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 10));
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't close position {0} => attempt to set TP = {2} SL={1} with Bid = {3}",
																	activePositions[i].Id,CurrentBuySL,CurrentBuyTP, CurrentBid);
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
								if(CurrentBid <= CurrentBuySL)
								{
									do
									{
										attempts++;
										tR = Trade.CloseMarketPosition(	activePositions[i].Id,
																		activePositions[i].Lots,
																		CurrentBid,
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
								if((CurrentAsk <= CurrentSellTP) || (CurrentAsk >= CurrentSellSL))
								{
									do
									{
										attempts++;
										tR = Trade.CloseMarketPosition(	activePositions[i].Id,
																		activePositions[i].Lots,
																		CurrentAsk,
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
								if(CurrentAsk >= CurrentSellSL)
								{
									do
									{
										attempts++;
										tR = Trade.CloseMarketPosition(	activePositions[i].Id,
																		activePositions[i].Lots,
																		CurrentAsk,
																		-1,
																		"Closed on changed SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 10));
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't close position {0} => attempt to set SL={1} with Ask = {2}",
																	activePositions[i].Id,CurrentSellSL,CurrentAsk);
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
		
		protected double CalculateOpenPrice(ExecutionRule orderType)
		{
			int cB = _CIndex;
			ExtremumMin = Bars[cB].Low;
			ExtremumMax = Bars[cB].High;
			
			while((cB > (_CIndex - ExtHistoryDepth)) && (cB >= 0))
			{
				if(ExtremumMax <= Bars[cB].High)
				{
					ExtremumMax = Bars[cB].High;
				}
				if(ExtremumMin >= Bars[cB].Low)
				{
					ExtremumMin = Bars[cB].Low;
				}
				cB--;
			}
			
			if(((int)orderType==(int)ExecutionRule.Buy)
				|| ((int)orderType==(int)ExecutionRule.BuyStop))
 			{
				return (double) Math.Round(ExtremumMax,Instrument.PriceScale);
			}				
			if(((int)orderType==(int)ExecutionRule.Sell)
				|| ((int)orderType==(int)ExecutionRule.SellStop))
 			{
				return (double) Math.Round(ExtremumMin,Instrument.PriceScale);
			}				
			return (double) Math.Round((ExtremumMin + ExtremumMax)/2.0,Instrument.PriceScale);
		}
        	
		
		protected double CalculateTP(ExecutionRule orderType, double openPrice)
		{
			double TP = 0.3333;
			if(((int)orderType==(int)ExecutionRule.Buy)
				|| ((int)orderType==(int)ExecutionRule.BuyStop))
			{
					TP = BolaInd.SeriesUp[_CIndex];	
			}
			if(((int)orderType==(int)ExecutionRule.Sell)
				|| ((int)orderType==(int)ExecutionRule.SellStop))
			{
					TP = BolaInd.SeriesDown[_CIndex];	
			}
			return (double) Math.Round(TP, Instrument.PriceScale);

		}
		
		protected double CalculateSL(ExecutionRule orderType, double openPrice)
		{
			double SL = 0.3333;
			double CurrentBid = Instrument.Bid;
			double CurrentAsk = Instrument.Ask;
			
			if(((int)orderType==(int)ExecutionRule.Buy)
				|| ((int)orderType == (int)ExecutionRule.BuyStop))
			{
				if(BolaOnly)
				{
					SL = BolaInd.SeriesDown[_CIndex];
				}
				else
				{
					SL = openPrice - staticSL * Instrument.Point;
				}
			}
			if(((int)orderType==(int)ExecutionRule.Sell)
				|| ((int)orderType==(int)ExecutionRule.SellStop))
			{
				if(BolaOnly)
				{
					SL = BolaInd.SeriesUp[_CIndex];
				}
				else
				{
					SL = openPrice + staticSL * Instrument.Point;
				}
			}
			return (double) Math.Round(SL, Instrument.PriceScale);

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
			trueLogPath=PathToLogFile+"\\"+LogFileName+DateTime.Now.Ticks.ToString().Trim()+".LOG";
		}
		
		protected override void PositionChanged(IPosition position, ModificationType type)
        {
            if (type==ModificationType.Closed || type==ModificationType.Canceled)
            {
				if (listOfBuyStop.Contains(position.Id))
				{
					listOfBuyStop.Remove(position.Id);
				}
				
				if (listOfSellStop.Contains(position.Id))
				{
					listOfSellStop.Remove(position.Id);
				}
            }
			
		}
   }
}
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
    [TradeSystem("BoliAndPivO")]
    public class BoliAndPivO : TradeSystem
    {
        // Simple parameter example
 		[Parameter("BB_period", DefaultValue = 23, MinValue = 1, MaxValue = 100)]
		public int BBperiod { get;set; }
		
		[Parameter("BB_deviation", DefaultValue = 2.4, MinValue = 0.500, MaxValue = 5.000)]
		public double BBdev { get;set; }

		[Parameter("BB_Shift", DefaultValue = -1, MinValue = -100, MaxValue = 100)]
		public int BBshift { get;set; }

		[Parameter("BB_PriceMode", DefaultValue = PriceMode.Close)]
		public PriceMode BBprice { get;set; }

		[Parameter("Quant_of_week", DefaultValue = 1, MinValue = 1, MaxValue = 10)]
		public int _QuantityOfWeek { get;set; }

		[Parameter("Quant_of_orders", DefaultValue = 3, MinValue = 2, MaxValue = 20)]
		public int _QuantityOfOrders { get;set; }

		[Parameter("MAX Lots :", DefaultValue = 1.0, MinValue=0.01, MaxValue = 10.0)]
		public double vol { get;set; }

		[Parameter("Static SL :", DefaultValue = 500, MinValue=10, MaxValue = 5000)]
		public int staticSL { get;set; }

		[Parameter("Static TP :", DefaultValue = 500, MinValue=50, MaxValue = 5000)]
		public int staticTP { get;set; }

		[Parameter("Deposit :", DefaultValue = 10000.0, MinValue=100.0, MaxValue = 500000.0)]
		public double _cDepo { get;set; }

		[Parameter("Avail_Lost :", DefaultValue = 0.35, MinValue= 0.01, MaxValue = 1.0)]
		public double _AvaLost { get;set; }

		[Parameter("Debug_Mode", DefaultValue = true)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = false)]
		public bool LogMode { get;set; }

		[Parameter("Log_Path", DefaultValue = @"PiviBoLiLoG")]
		public string LogPath { get;set; }

// Position describers     ... 
		private static double[] BuyPositionOP =   {	0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
													0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 };
		private static double[] SellPositionOP =  {	0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
													0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 };
		private static double[] BuyPositionTP =   {	0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
													0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 };
		private static double[] SellPositionTP =  {	0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
													0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 };
		private static double[] BuyPositionSL =   {	0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
													0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 };
		private static double[] SellPositionSL =  {	0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
													0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 };
// Saved state of position ...		
		private static double[] savedBuyPositionOP =  {	0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
														0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 };
		private static double[] savedSellPositionOP = { 0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
														0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 };
		private static double[] savedBuyPositionTP =  {	0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
														0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 };
		private static double[] savedSellPositionTP = { 0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
														0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 };
		private static double[] savedBuyPositionSL =  {	0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
														0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 };
		private static double[] savedSellPositionSL = { 0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,
														0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0 };
// --------------------------		
//		private ISeries<Bar> _PivotBarSeries;

		private static int _CIndex = 2;
		
		private List<Guid> listOfBuyStop = new List<Guid>();
		private List<Guid> listOfSellStop = new List<Guid>();
		
		private ISeries<Bar> _barSeries;
		private BollingerBands BolaInd;
		private Period _period = new Period(PeriodType.Minute, 15);
		private string trueLogPath = "";
		
		
		public static bool PivOCounted = false; 
		public static DateTime NextEnd;
		public static double PivO_Low = double.MaxValue;
		public static double PivO_High = 0.0;
		
		public static double PivOClose = 0.0;
		public static int PivO_lowBorder = 0;
	
//---- Pivot values	-----	
		public static double PivO = 0.0;

		public static double PivO_S1 = 0.0;
		public static double PivO_S2 = 0.0;
		public static double PivO_S3 = 0.0;
		
		public static double PivO_R1 = 0.0;
		public static double PivO_R2 = 0.0;
		public static double PivO_R3 = 0.0;
//-----------------------
		
		public static StreamWriter LogSW = null;

		public static int magicNumber = 96969;
		
		public static string SSC = "SS"; 			//SellStopComment
		public static string BSC = "BS"; 			//BuyStopComment
		public static string[] bcommstruct = { "BS", "-" };
		public static string[] scommstruct = { "SS", "-" };
		
		public static bool BuyPosOpened  = false;
		public static bool SellPosOpened = false;

		public static int BuyLevel  = 0;
		public static int SellLevel = 0;
	
		public static int ActiveBuyCount  = 0; 
		public static int ActiveSellCount = 0; 
		
		public static double Vbuy  = 0.0;
		public static double Vsell = 0.0;
		
		public static double c_cDepo = 0.0;

		public static bool TrailSLTP = true;
		public static bool FullPositionCalculated = false;

		public static Guid cAUG = Guid.Empty; 		
		public static Guid cADG = Guid.Empty; 		

		public ArrowUp cArrowUp;
		public ArrowDown cArrowDown;

		
		protected override void Init()
        {
			_period = Timeframe;
			_barSeries = GetCustomSeries(Instrument.Id, 
											_period);
        	c_cDepo=_cDepo;
			
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
///////////////////////////////////////////////////			
//			PivotInd = GetIndicator<Pivot>(Instrument.Id, _period);	
//			PivotInd.Range= PivotBarPeriod.D1;
///////////////////////////////////////////////////////////
		
#endregion //GetIndicatorsAndFrames		

			if (_CIndex >= _barSeries.Range.To) // - 1) 
			{
				return;
			}
			else 
			{
				_CIndex = _barSeries.Range.To; // - 1;
			}
			
//////////// Push Pivot Calculating (shift right date border into old time)....            
			NextEnd = Bars[_CIndex].Time.AddDays((double)(-1));
			
			if(_QuantityOfOrders < 2) 
			{
				_QuantityOfOrders = 2;
			}
			
			// Event occurs once at the start of the strategy
		}        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
		[STAThread]  
        protected override void NewBar()
        {
 		try
			{
				if (_CIndex >= _barSeries.Range.To - 1) 
				{
					return;
				}
				else 
				{
					_CIndex = _barSeries.Range.To - 1;
				}

				double nbCurrentAsk = Instrument.Ask;
				double nbCurrentBid = Instrument.Bid;
				double nbBolaUp = BolaInd.SeriesUp[_CIndex];
				double nbBolaDown = BolaInd.SeriesDown[_CIndex];

				if(		
				(double.IsNaN(nbCurrentAsk)) 	//(nbCurrentAsk.Equals(double.NaN)) 
				|| 	
				(double.IsNaN(nbCurrentBid)) 	//(nbCurrentBid.Equals(double.NaN)) 
				|| 	
				(double.IsNaN(nbBolaUp))      	//(nbBolaUp.Equals(double.NaN)) 
				|| 	
				(double.IsNaN(nbBolaDown)) 		//(nbBolaDown.Equals(double.NaN))
			   	) 
				{
					PivOCounted = false;
					XXPrint("(*!*) - False Pivot Calculation or NaN in main data...");
					return;
				}
		
				if(!PivOCounted || Bars[_CIndex].Time > NextEnd) //
				{
					CalculatePivo();
				}

			//			XXPrint(" Pv={0} R1={1} R2={2} R3={3} S1={4} S2={5} S3={6} DTNE={7} DTBT ={8}", 
			//					PivO, PivO_R1,PivO_R2,PivO_R3,PivO_S1,PivO_S2,PivO_S3,NextEnd, Bars[_CIndex].Time);			
			
				ActiveBuyCount = 0;
				ActiveSellCount = 0;
				
				TrailActiveOrders();

				BuyLevel  = (ActiveBuyCount  == 0)? 0 : (ActiveBuyCount  - 1);
				SellLevel = (ActiveSellCount == 0)? 0 : (ActiveSellCount - 1);

				ClearPositions();

				CalculateFullPositions();			
				SetFullPosition();
				
			}
			catch(Exception Ex)
			{
				XXPrint("[[[*E*U*Kn*]]] {0}; {1}; {2}", 
				Ex.Message, Ex.InnerException, Ex.Source);
			}
			// Event occurs on every new bar
        }
        
 		protected override void PositionChanged(IPosition position, ModificationType type)
        {

			BuyPosOpened  = false;
			SellPosOpened = false;

			if (type == ModificationType.Opened)
			{
				XXPrint("buyLevel = {0} sellLevel = {1}",BuyLevel,SellLevel);
				
				if((int)position.Type == (int)ExecutionRule.Buy)
				{
					BuyPosOpened = true;

					if(listOfBuyStop.Contains(position.Id))
					{
						listOfBuyStop.Remove(position.Id);
					}
					
					string pcomm = position.Comment; 
					pcomm = pcomm.Substring(pcomm.IndexOf(bcommstruct[0]));
						XXPrint("{0}",pcomm);
					string[] lv = pcomm.Split(bcommstruct,StringSplitOptions.RemoveEmptyEntries);
					
					BuyLevel = int.Parse(lv[0]);
					if(BuyLevel == 0)
					{
						SaveBuyPos();
					}
				
				}
				
				if((int)position.Type == (int)ExecutionRule.Sell)
				{
					SellPosOpened = true;
					
					if(listOfSellStop.Contains(position.Id))
					{
						listOfSellStop.Remove(position.Id);
					}
					
					string pcomm = position.Comment; 
					pcomm = pcomm.Substring(pcomm.IndexOf(scommstruct[0]));
						XXPrint("{0}",pcomm);
					string[] lv = pcomm.Split(scommstruct,StringSplitOptions.RemoveEmptyEntries);

					SellLevel = int.Parse(lv[0]);
					if(SellLevel == 0)
					{
						SaveSellPos();
					}
				}

				XXPrint("buyLevel = {0} sellLevel = {1} ", BuyLevel,SellLevel);
				
			}			

			if (type==ModificationType.Closed)
            {

				if((int)position.Type == (int)ExecutionRule.Buy)
				{
						
					double xComission = (double)position.Comission;
					double xPipValue =  (double)position.PipValue;
					double xSwap =  (double)position.Swap;
					double xClosePrice = position.ClosePrice==null? 0.0: (double)position.ClosePrice;  
					c_cDepo += 	Math.Round(
											(xClosePrice - position.OpenPrice) * xPipValue / Instrument.Point
											- xComission 
											+ xSwap	
											, 2  //Balans PriceScale
											);
					BuyLevel = 0;
				}
				if((int)position.Type == (int)ExecutionRule.Sell)
				{
					
					double xComission = (double)position.Comission;
					double xPipValue =  (double)position.PipValue;
					double xSwap = (double)position.Swap;
					double xClosePrice = position.ClosePrice==null? 0.0: (double)position.ClosePrice;  
					c_cDepo += 	Math.Round(
											(position.OpenPrice - xClosePrice) * xPipValue / Instrument.Point
											- xComission 
											+ xSwap	
											, 2 //Balans PriceScale
											);
					SellLevel = 0;
				}
				
				XXPrint("(*C*)--->>>[[DD = {0}]]",c_cDepo.ToString("0.00"));

			}					


			if(type==ModificationType.Canceled)
			{
				XXPrint(" Canceled {0} order {1}", position.Type,position.Id);
			}	
        }

		protected void SaveBuyPos()
		{
			for(int p = 0; p <_QuantityOfOrders; p++)
			{
				savedBuyPositionOP[p] = BuyPositionOP[p];
				savedBuyPositionTP[p] = BuyPositionTP[p];
				savedBuyPositionSL[p] = BuyPositionSL[p];
			}
		}

		protected void SaveSellPos()
		{
			for(int p = 0; p <_QuantityOfOrders; p++)
			{
				savedSellPositionOP[p] = SellPositionOP[p];
				savedSellPositionTP[p] = SellPositionTP[p];
				savedSellPositionSL[p] = SellPositionSL[p];
			}
		}
		
		protected void ClearPositions()
		{
			int attempt = 0;
			TradeResult ntR = null;

			if(
				(listOfBuyStop.Count  < _QuantityOfOrders) 
				&& 
				(ActiveBuyCount  == 0)
			)
			{
				for( int p = listOfBuyStop.Count-1; p >=0; p--)
				{
					attempt = 0;
					ntR = null;
					do
					{
						attempt++;
						ntR = Trade.CancelPendingPosition(listOfBuyStop[p]);
					}
					while((ntR==null? true: !ntR.IsSuccessful) && attempt < 8); 
				
					if(ntR==null? true: !ntR.IsSuccessful)
					{
						
						XXPrint("[*EC*{2}>>]{1} Cann't cancel strange BuyLimit order at Bid: {0}",
													Instrument.Bid,DateTime.Now,Instrument.Name);
					}
				    else
					{
						listOfBuyStop.RemoveAt(p);
					}
				}
	
				listOfBuyStop.Clear();
				BuyPosOpened = false;
			}

			if(
				(listOfSellStop.Count < _QuantityOfOrders)
				&& 
				(ActiveSellCount == 0)
			)
			{
				for( int p = listOfSellStop.Count-1; p >=0; p--)
				{
					attempt = 0;
					ntR = null;
					do
					{
						attempt++;
						ntR = Trade.CancelPendingPosition(listOfSellStop[p]);
					}
					while((ntR==null? true: !ntR.IsSuccessful) && attempt < 8); 
				
					if(ntR==null? true: !ntR.IsSuccessful)
					{
						
						XXPrint("[*EC*{2}>>]{1} Cann't cancel strange SellLimit order at Bid: {0}",
																Instrument.Bid,DateTime.Now,Instrument.Name);
					}
				    else
					{
						listOfSellStop.RemoveAt(p);
					}
				}
				
				listOfSellStop.Clear();
				SellPosOpened = false;
			}
	
		}
		
		protected void SetFullPosition()
		{

			
			TradeResult ntR = null;
			int attempts = 0;
			

			double CurrentAsk = Math.Round(Instrument.Ask, Instrument.PriceScale);
			double CurrentBid = Math.Round(Instrument.Bid, Instrument.PriceScale);
			
							
//			if(CurrentAsk.Equals(double.NaN) || CurrentBid.Equals(double.NaN))
			if(double.IsNaN(CurrentAsk) || double.IsNaN(CurrentBid))
			{
				XXPrint("[*!*] Positions did not modified - NaNs in main data : Very-Very Bad....");
				return;
			}

			if(ActiveBuyCount == 0)
			{
				
				BuyPosOpened = false;

				if(listOfBuyStop.Count > 0)
				{
					
					XXPrint("Attempt to trail BuyLimit. Quantity ={0}", listOfBuyStop.Count);

					for(int p = listOfBuyStop.Count-1; p >=0; p--)
					{

						IPosition xposition = Trade.GetPosition(listOfBuyStop[p]);

						ntR = null;
						attempts = 0;
						
						if(xposition==null)
						{

							XXPrint("BuyLimit {0} not found ....", listOfBuyStop[p]);
							listOfBuyStop.RemoveAt(p);

						}
						else
						{
							if((int)xposition.Type == (int)ExecutionRule.Buy)
							{
								XXPrint("BuyLimit {0} opened ....", xposition.Id);
								listOfBuyStop.RemoveAt(p);
								BuyPosOpened = true;
							}
							else
							{

								string pcomm = xposition.Comment; 
								pcomm = pcomm.Substring(pcomm.IndexOf(bcommstruct[0]));
									XXPrint("{0}",pcomm);
								string[] lv = pcomm.Split(bcommstruct,StringSplitOptions.RemoveEmptyEntries);
								int BL = int.Parse(lv[0]);
								
								double OP = (BuyPositionOP[BL].Equals(double.NaN))? xposition.OpenPrice : BuyPositionOP[BL];
								if(OP >= (CurrentAsk - 2 * Instrument.Spread))
								{
									OP = Math.Round(CurrentAsk -  4 * Instrument.Spread, Instrument.PriceScale); 
								}
								
								double oSL = xposition.StopLoss??(double)xposition.StopLoss;
								double oTP = xposition.TakeProfit??(double)xposition.TakeProfit;
								
								do 
								{
									attempts++;
									ntR = Trade.UpdatePendingPosition(	
																xposition.Id,
																xposition.Lots, 												//Vbuy, 
																OP, 															//BuyPositionOP[BL],
																-1,
																Bars[_CIndex].Time.AddDays(90.0),
																(BuyPositionSL[BL].Equals(double.NaN))? oSL :BuyPositionSL[BL], //BuyPositionSL[BL],
																(BuyPositionTP[BL].Equals(double.NaN))? oTP :BuyPositionTP[BL], //BuyPositionTP[BL],
																BSC+BL.ToString().Trim()+"- BuyLimit set TP,OP,VOL");	
								}
								while ((ntR==null? true: !ntR.IsSuccessful) && attempts < 8);
						
								if(ntR!=null && ntR.IsSuccessful)
								{
									XXPrint("[{3}>>]{2} Modified successfully BuyLimit order for position {0} at Price: {1} ",
											ntR.Position.Id, ntR.Position.OpenPrice, Bars[_CIndex].Time, Instrument.Name); 
								}
							}
						}
					}
				}	
				else
				{
					for(int p = 0; p < _QuantityOfOrders; p++)
					{
						setPendingBuyOrder(p);
					}
				}
			}
			
			if(ActiveSellCount == 0)
			{
				SellPosOpened = false;
			
				if(listOfSellStop.Count > 0)
				{
					
					XXPrint("Attempt to trail SellLimit. Quantity = {0}", listOfSellStop.Count);
					
					for(int p = listOfSellStop.Count -1; p >=0; p--)
					{
						IPosition xposition = Trade.GetPosition(listOfSellStop[p]);
	
						ntR = null;
						attempts = 0;
			
						if(xposition==null)
						{
							XXPrint("SellLimit {0} not found ....", listOfSellStop[p]);
							listOfSellStop.RemoveAt(p);
						}
						else
						{	
							if((int)xposition.Type == (int)ExecutionRule.Sell)
							{
								XXPrint("SellLimit {0} opened ....", xposition.Id);
								listOfSellStop.RemoveAt(p);
								SellPosOpened = true;
							}
							else
							{

								string pcomm = xposition.Comment; 
								pcomm = pcomm.Substring(pcomm.IndexOf(scommstruct[0]));
									XXPrint("{0}",pcomm);
								string[] lv = pcomm.Split(scommstruct,StringSplitOptions.RemoveEmptyEntries);
								int BL = int.Parse(lv[0]);
							
								XXPrint("Sell BL = {0}, Count = {1}", BL,listOfSellStop.Count);

								double OP = (SellPositionOP[BL].Equals(double.NaN))? xposition.OpenPrice : SellPositionOP[BL]; 	//SellPositionOP[BL];
								if(OP <= (CurrentBid + Instrument.Spread))
								{
									OP = Math.Round(CurrentBid +  4 * Instrument.Spread, Instrument.PriceScale); 
								}

								double oSL = xposition.StopLoss??(double)xposition.StopLoss;
								double oTP = xposition.TakeProfit??(double)xposition.TakeProfit;
								
								do 
								{
									attempts++;
									ntR = Trade.UpdatePendingPosition(	
																xposition.Id,
																xposition.Lots,														// Vsell,
																OP, 																//SellPositionOP[BL],
																-1,
																Bars[_CIndex].Time.AddDays(90.0),
																((SellPositionSL[BL].Equals(double.NaN))? oSL : SellPositionSL[BL]), //SellPositionTP[BL],
																((SellPositionTP[BL].Equals(double.NaN))? oTP : SellPositionTP[BL]), //SellPositionSL[BL],
																SSC+BL.ToString().Trim()+"- SellLimit set TP,OP,VOL");	
								}
								while ((ntR==null? true: !ntR.IsSuccessful) && attempts < 8);
						
								if(ntR!=null && ntR.IsSuccessful)
								{
									XXPrint("[{3}>>]{2} Modified successfully SellLimit order for position {0} at Price: {1} ",
											ntR.Position.Id, ntR.Position.OpenPrice, Bars[_CIndex].Time, Instrument.Name); 
								}
							}
						}
					}
				}	
				else
				{
					for(int p = 0; p < _QuantityOfOrders; p++)
					{
						setPendingSellOrder(p);
					}
				}
			}
		}

		protected void TrailActiveOrders()
		{
			
			double CurrentAsk = Instrument.Ask;
			double CurrentBid = Instrument.Bid;
			
			ActiveBuyCount = 0;
			ActiveSellCount = 0;

			IPosition[] activePositions = Trade.GetActivePositions(magicNumber);
			
			if(activePositions!=null)
			{
				for(int p = activePositions.Length-1; p>=0; p--)
				{
					if((int)activePositions[p].Type == (int)ExecutionRule.Buy)
					{
						ActiveBuyCount++;
					}
					if((int)activePositions[p].Type == (int)ExecutionRule.Sell)
					{
						ActiveSellCount++;
					}
				}
			}

			if(ActiveBuyCount  > 0)
			{
				BuyLevel  = ActiveBuyCount  - 1;
			}
			if(ActiveSellCount > 0)
			{
				SellLevel = ActiveSellCount - 1;
			}

			if(	
				(double.IsNaN(CurrentAsk))					//(CurrentAsk.Equals(double.NaN)) 
				|| 
				(double.IsNaN(CurrentBid))					//(CurrentBid.Equals(double.NaN))
				||
				(double.IsNaN(PivO_R3))						//(PivO_R3.Equals(double.NaN))
				||
				(double.IsNaN(PivO_S3))						//(PivO_S3.Equals(double.NaN))
				||
				(double.IsNaN(PivO_R2))						//(PivO_R2.Equals(double.NaN))
				||
				(double.IsNaN(PivO_S2))						//(PivO_S2.Equals(double.NaN))
				||
				(double.IsNaN(PivO_R1))						//(PivO_R1.Equals(double.NaN))
				||
				(double.IsNaN(PivO_S1))						//(PivO_S1.Equals(double.NaN))
				||	
				(double.IsNaN(PivO))						//(PivO.Equals(double.NaN))
			  )
			{
				XXPrint("!!! {0} - NaNs in main data... Very-very bad :(...",DateTime.Now.ToString("o"));
				XXPrint("(*!AO!PivO!*) - False Pivot Calculation (or NaN in main data) Active orders didn't modified...");
				PivOCounted = false;
				return;
			}

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
							if((activePositions[i].TakeProfit != savedBuyPositionTP[BuyLevel])) 
							{
								if(
									(CurrentBid >= savedBuyPositionTP[BuyLevel]) 
									|| 
									(CurrentBid <= savedBuyPositionSL[BuyLevel])
								)
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
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 8));
									
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't close position {0} => attempt to set TP = {2} SL={1} with Bid = {3}",
										activePositions[i].Id,savedBuyPositionSL[BuyLevel],savedBuyPositionTP[BuyLevel], CurrentBid);
									}
								}
								else
								{
									do
									{
										attempts++;
										tR = Trade.UpdateMarketPosition(activePositions[i].Id,
																		activePositions[i].StopLoss,
																		savedBuyPositionTP[BuyLevel],
																		"trail Buy TP,SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 8));
									
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't modify position {0} => attempt to set TP = {2} SL = {1}",
										activePositions[i].Id,savedBuyPositionSL[BuyLevel],savedBuyPositionTP[BuyLevel]);
									}
								}
							}				
						}	
						else
						{
							if(activePositions[i].StopLoss != savedBuyPositionSL[BuyLevel])
							{
								if(CurrentBid <= savedBuyPositionSL[BuyLevel])
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
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 8));
									
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't close position {0} => attempt to set SL={1} with Bid = {2}",
																	activePositions[i].Id,savedBuyPositionSL[BuyLevel],Instrument.Bid);
									}
								}
								else
								{	
									do
									{
										attempts++;
										tR = Trade.UpdateMarketPosition(activePositions[i].Id,
																		savedBuyPositionSL[BuyLevel],
																		activePositions[i].TakeProfit,
																		"trail Buy SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 8));
									
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't modify position {0} => attempt to set SL = {1}",
																	activePositions[i].Id,savedBuyPositionSL[BuyLevel]);
									}
								}
							}				
						}
					}

					if((int)activePositions[i].Type==(int)ExecutionRule.Sell)
					{
						if(TrailSLTP)
						{
							if((activePositions[i].TakeProfit != savedSellPositionTP[SellLevel])) 
							{
								if(	
									(CurrentAsk <= savedSellPositionTP[SellLevel]) 
									|| 
									(CurrentAsk >= savedSellPositionSL[SellLevel])
								)
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
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 8));
									
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't close position {0} => attempt to set TP = {2} SL={1} with Ask = {3}",
											activePositions[i].Id,savedSellPositionSL[SellLevel],savedSellPositionTP[SellLevel],Instrument.Ask);
									}
								}
								else
								{
									do
									{
										attempts++;
										tR = Trade.UpdateMarketPosition(activePositions[i].Id,
																		activePositions[i].StopLoss,
																		savedSellPositionTP[SellLevel],
																		"trail Sell TP,SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 8));
									
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't modify position {0} => attempt to set TP = {2} SL = {1}",
																activePositions[i].Id,savedSellPositionSL[SellLevel],savedSellPositionTP[SellLevel]);
									}
								}
							}				
						}	
						else
						{
							if(activePositions[i].StopLoss != savedSellPositionSL[SellLevel])
							{
								if(CurrentAsk >= savedSellPositionSL[SellLevel])
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
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 8));
									
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't close position {0} => attempt to set SL={1} with Ask = {2}",
																		activePositions[i].Id,savedSellPositionSL[SellLevel],CurrentAsk);
									}
								}
								else
								{	
									do
									{
										attempts++;
										tR = Trade.UpdateMarketPosition(activePositions[i].Id,
																		savedSellPositionSL[SellLevel],
																		activePositions[i].TakeProfit,
																		"trail Sell SL");
									}
									while((tR==null? true: !tR.IsSuccessful) && (attempts < 8));
									
									if(tR==null? true: !tR.IsSuccessful)
									{
										XXPrint(" *!* Can't modify position {0} => attempt to set SL = {1}",
																	activePositions[i].Id,SellPositionSL[SellLevel]);
									}
								}
							}				
						}
					}
				}
			}
		}
        

		protected void setPendingSellOrder(int level)
		{
			
			double CurrentAsk = Instrument.Ask;
			double CurrentBid = Instrument.Bid;
			
			
			string xS = "";
			for(int s=0; s<_QuantityOfOrders; s++) xS+=(SellPositionOP[s].ToString().Trim()+";");

			double OP = SellPositionOP[level];
			double SL = SellPositionSL[level];
			double TP = SellPositionTP[level];

			XXPrint("All:{4} => sell {0} - OP = {1}, TP = {2}, SL = {3}", level, OP, TP, SL, xS);
			
		
			if(OP <= (CurrentBid + 2 * Instrument.Spread))
			{
				OP = Math.Round(CurrentBid + 4 * Instrument.Spread, Instrument.PriceScale);   
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
												ExecutionRule.SellLimit,
												Vsell,
												OP,
												-1,
												SLTP,
												Bars[_CIndex].Time.AddDays(60.0),
												SSC+level.ToString().Trim()+"-sellLimit on bar signal",
												magicNumber);	
			
			}
			while ((tR==null? true: !tR.IsSuccessful) && attempt < 8);
			
			if((tR==null? true: !tR.IsSuccessful))
			{
				XXPrint("[{2}>>]{1} Cann't Send  SellLimit order on Price {3} at Bid: {0} ",
											Instrument.Bid,DateTime.Now,Instrument.Name,OP);
			}
			else
			{
				TradeResult ntR = null;
				do 
				{
					attempt++;
					ntR = Trade.UpdatePendingPosition(	tR.Position.Id,
														Vsell,
														OP,
														-1,
														Bars[_CIndex].Time.AddDays(60.0),
														SL,
														TP,
														SSC+level.ToString().Trim()+"-sellLimit set SL,TP");	
				}
				while ((ntR==null? true: !ntR.IsSuccessful) && attempt < 8);
				
				if(ntR==null? true: !ntR.IsSuccessful)
				{
					XXPrint("[{2}>>]{1} Cann't modify  SellLimit order on Price {3} at Bid: {0}",
												Instrument.Bid,DateTime.Now,Instrument.Name,OP);
					
					attempt = 0;
					do
					{
						attempt++;
						ntR = Trade.CancelPendingPosition(tR.Position.Id);
					}
					while((ntR==null? true: !ntR.IsSuccessful) && attempt < 8); 
				
					if(ntR==null? true: !ntR.IsSuccessful)
					{
						
						XXPrint("[*EC*{2}>>]{1} Cann't cancel strange SellLimit order on Price {3} at Bid: {0}",
																Instrument.Bid,DateTime.Now,Instrument.Name,OP);
					}
				}
				else
				{
					listOfSellStop.Add(ntR.Position.Id);
					
					
					XXPrint("[{3}>>]{2} Sended successfully SellLimit order for position {0} at Price: {1}",
									tR.Position.Id,tR.Position.OpenPrice,Bars[_CIndex].Time, Instrument.Name); 
				}
			}
			
		}

		protected void setPendingBuyOrder(int level)
		{

			double CurrentAsk = Instrument.Ask;
			double CurrentBid = Instrument.Bid;
			
			string xS = "";
			for(int s=0; s<_QuantityOfOrders; s++) xS+=(BuyPositionOP[s].ToString().Trim()+";");
			
			double OP = BuyPositionOP[level];
			double SL = BuyPositionSL[level];
			double TP = BuyPositionTP[level];

			XXPrint("All:{4} => Buy {0} - OP = {1}; TP = {2}; SL = {3}", level, OP, TP, SL, xS);
			
			if(OP >= (CurrentBid - 2 * Instrument.Spread))
			{
				OP = Math.Round(CurrentAsk - 4 * Instrument.Spread, Instrument.PriceScale);   
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
												ExecutionRule.BuyLimit,
												Vbuy,
												OP,
												-1,
												SLTP,
												Bars[_CIndex].Time.AddDays(60.0),
												BSC+level.ToString().Trim()+"-buyLimit on bar signal",
												magicNumber);	
			
			}
			while ((tR==null? true: !tR.IsSuccessful) && attempt < 8);
			
			if((tR==null? true: !tR.IsSuccessful))
			{
				XXPrint("[{2}>>]{1} Cann't Send  BuyLimit order on Price {3} at Bid: {0} ",
											Instrument.Bid,DateTime.Now,Instrument.Name,OP);
			}
			else
			{
				TradeResult ntR = null;
				do 
				{
					attempt++;
					ntR = Trade.UpdatePendingPosition(	tR.Position.Id,
														Vbuy,
														OP,
														-1,
														Bars[_CIndex].Time.AddDays(60.0),
														SL,
														TP,
														BSC+level.ToString().Trim()+"-buyLimit set SL,TP");	
				}
				while ((ntR==null? true: !ntR.IsSuccessful) && attempt < 8);
				
				if(ntR==null? true: !ntR.IsSuccessful)
				{
					XXPrint("[{2}>>]{1} Cann't modify  BuyLimit order on Price {3} at Bid: {0} ",
												Instrument.Bid,DateTime.Now,Instrument.Name,OP);
					
					attempt = 0;
					do
					{
						attempt++;
						ntR = Trade.CancelPendingPosition(tR.Position.Id);
					}
					while((ntR==null? true: !ntR.IsSuccessful) && attempt < 8); 
				
					if(ntR==null? true: !ntR.IsSuccessful)
					{
						XXPrint("[*EC*{2}>>]{1} Cann't cancel strange BuyLimit order on Price {3} at Bid: {0} ",
																Instrument.Bid,DateTime.Now,Instrument.Name,OP);
					}
				}
				else
				{
					listOfBuyStop.Add(ntR.Position.Id);
					XXPrint("[{3}>>]{2} Sended successfully BuyLimit order for position {0} at Price: {1} ",
									tR.Position.Id,tR.Position.OpenPrice,Bars[_CIndex].Time, Instrument.Name); 
				}
			}
		}
	
		

		protected void CalculatePivo()
		{
			int oldCIndex = _CIndex;
			DateTime DTC = Bars[_CIndex].Time;
			
			if((int)DTC.DayOfWeek < 6)
			{
				DTC = DTC.AddDays((double)(-(int)DTC.DayOfWeek - 2));
			}
			else
			{	
				DTC = DTC.AddDays((double)(-((int)DTC.DayOfWeek - 5)));
			}
			
			NextEnd = DTC.AddDays(7.0); 

			DateTime DTLastBarOfWeek = DateTime.MaxValue; 	
			
			PivO_lowBorder = 0;
		 	
			PivO_Low = double.MaxValue;
			PivO_High = 0.0;
		
			switch(Timeframe.ToMinutes())
			{
				case 15: 
				{
					PivO_lowBorder= _CIndex - _QuantityOfWeek * 24 * 4 * 7;
					DTLastBarOfWeek = new DateTime(DTC.Year, DTC.Month, DTC.Day, 23, 45, 0); 	
					NextEnd = new DateTime(NextEnd.Year, NextEnd.Month, NextEnd.Day, 23, 45, 0); 
					break;
				}
				case 30:  
				{
					PivO_lowBorder= _CIndex - _QuantityOfWeek * 24 * 2 * 7;
					DTLastBarOfWeek = new DateTime(DTC.Year, DTC.Month, DTC.Day, 23, 30, 0); 	
					NextEnd = new DateTime(NextEnd.Year, NextEnd.Month, NextEnd.Day, 23, 30, 0); 
					break;
				}
				case 60:  
				{
					PivO_lowBorder= _CIndex - _QuantityOfWeek * 24 * 7;
					DTLastBarOfWeek = new DateTime(DTC.Year, DTC.Month, DTC.Day, 23, 0, 0); 	
					NextEnd = new DateTime(NextEnd.Year, NextEnd.Month, NextEnd.Day, 23, 0, 0); 
					break;
				}
				case 240:  
				{
					PivO_lowBorder= _CIndex - _QuantityOfWeek * 6 * 7;	
					DTLastBarOfWeek = new DateTime(DTC.Year, DTC.Month, DTC.Day, 20, 0, 0); 	
					NextEnd = new DateTime(NextEnd.Year, NextEnd.Month, NextEnd.Day, 20, 0, 0); 
					break;
				}
				case 1440:  
				{
					PivO_lowBorder= _CIndex - _QuantityOfWeek * 7;
					DTLastBarOfWeek = new DateTime(DTC.Year, DTC.Month, DTC.Day, 0, 0, 0); 	
					NextEnd = new DateTime(NextEnd.Year, NextEnd.Month, NextEnd.Day, 0, 0, 0); 
					break;
				}
				default:   
				{
					PivO_lowBorder= _CIndex - _QuantityOfWeek * 24 * 7;	
					DTLastBarOfWeek = new DateTime(DTC.Year, DTC.Month, DTC.Day, 0, 0, 0); 	
					NextEnd = new DateTime(NextEnd.Year, NextEnd.Month, NextEnd.Day, 0, 0, 0); 
					break;
				}
			}

			while(Bars[_CIndex].Time > DTLastBarOfWeek && _CIndex > 0) 
			{
				_CIndex--;
			}
			
			if(_CIndex==0 && Bars[_CIndex].Time > DTLastBarOfWeek)
			{
				_CIndex=oldCIndex;
				PivOCounted = false;
				XXPrint("(*!*) Too short history. Wait for full story;)...");
				
				return;
			}
			
			PivOClose = Math.Round(Bars[_CIndex].Close, Instrument.PriceScale);

			XXPrint("|| "	+ Bars[oldCIndex].Time.ToString("o") 
							+ " || -*- || "
							+ NextEnd.ToString("o")+" ||");
			
			while(_CIndex > PivO_lowBorder && _CIndex >= 0 ) /* && Bars[_CIndex].Time >PivO_lowBorderTime */ 
			{
			  	if((	
					(!double.IsNaN(Bars[_CIndex].Close))			//(!Bars[_CIndex].Close.Equals(double.NaN))
						&&
					(!double.IsNaN(Bars[_CIndex].Open))				//(!Bars[_CIndex].Open.Equals(double.NaN))
						&&
					(!double.IsNaN(Bars[_CIndex].High))				//(!Bars[_CIndex].High.Equals(double.NaN))
						&&
					(!double.IsNaN(Bars[_CIndex].Low))				//(!Bars[_CIndex].Low.Equals(double.NaN))
				  ))
				{
				
					if(Bars[_CIndex].High > PivO_High)
					{
						PivO_High = Bars[_CIndex].High;
					}
			  		if(Bars[_CIndex].Low < PivO_Low)
					{
						PivO_Low = Bars[_CIndex].Low;
					}
				}
				else
				{
					XXPrint("[[<*>]] Very-Very Bad - NaNs  in history data: B.i = {0} B.time = {1}",_CIndex,Bars[_CIndex].Time);
				}
				_CIndex--;
			}	
			
			_CIndex = oldCIndex;				

			PivO_High = Math.Round(PivO_High, Instrument.PriceScale);
			PivO_Low = Math.Round(PivO_Low, Instrument.PriceScale);
		
			PivO = Math.Round((PivO_High+PivO_Low+PivOClose)/3.0, Instrument.PriceScale);

			PivO_S1 = Math.Round((PivO - PivO_High + PivO), Instrument.PriceScale);
			PivO_S2 = Math.Round((PivO - PivO_High + PivO_Low), Instrument.PriceScale);
			PivO_S3 = Math.Round((PivO_Low - 2 * (PivO_High - PivO)), Instrument.PriceScale);
		
			PivO_R1 = Math.Round((PivO - PivO_Low + PivO), Instrument.PriceScale);
			PivO_R2 = Math.Round((PivO + PivO_High - PivO_Low), Instrument.PriceScale);
			PivO_R3 = Math.Round((PivO_High + 2 * (PivO - PivO_Low)), Instrument.PriceScale);
		
			if(	
				(double.IsNaN(PivO_R3))				//(PivO_R3.Equals(double.NaN))
				||
				(double.IsNaN(PivO_S3))				//(PivO_S3.Equals(double.NaN))
				||
				(double.IsNaN(PivO_R2))				//(PivO_R2.Equals(double.NaN))
				||
				(double.IsNaN(PivO_S2))				//(PivO_S2.Equals(double.NaN))
				||
				(double.IsNaN(PivO_R1))				//(PivO_R1.Equals(double.NaN))
				||
				(double.IsNaN(PivO_S1))				//(PivO_S1.Equals(double.NaN))
				||	
				(double.IsNaN(PivO))				//(PivO.Equals(double.NaN))
			  )
			{
				XXPrint("(*!PivO!*) - False Pivot Calculation (or NaN in main data)...");
				PivOCounted = false;
			}
			else
			{
				PivOCounted = true;
			}
		}

		protected void CalculateFullPositions()
		{
			
			FullPositionCalculated = false;

			double CurrentBid = Instrument.Bid;
			double CurrentAsk = Instrument.Ask;
			
			double BolaUp = Math.Round(BolaInd.SeriesUp[_CIndex], Instrument.PriceScale);
			double BolaDown = Math.Round(BolaInd.SeriesDown[_CIndex], Instrument.PriceScale);
			
			if(	
				((!PivOCounted))
				||
				(double.IsNaN(CurrentAsk))				//(CurrentAsk.Equals(double.NaN)) 
				|| 	
				(double.IsNaN(CurrentBid))				//(CurrentBid.Equals(double.NaN)) 
				|| 	
				(double.IsNaN(BolaUp))					//(BolaUp.Equals(double.NaN)) 
				|| 	
				(double.IsNaN(BolaDown))				//(BolaDown.Equals(double.NaN))
				||
				(double.IsNaN(PivO_R3))					//(PivO_R3.Equals(double.NaN))
				||
				(double.IsNaN(PivO_S3))					//(PivO_S3.Equals(double.NaN))
			   ) 
			{
				PivOCounted = false;
				XXPrint("(*!PivO!*) - False Pivot Calculation or NaN in main data...");
				return;
			}
			
			if(BuyPosOpened || SellPosOpened) return;
			
			XXPrint("[[*!*]]-------->>>|[ Current Depo = {0}]|", c_cDepo.ToString("0.00"));
			
			double BuyD  = Math.Round((BolaDown - PivO_S3)/(double)(_QuantityOfOrders - 1), Instrument.PriceScale);
			double SellD = Math.Round((-BolaUp  + PivO_R3)/(double)(_QuantityOfOrders - 1), Instrument.PriceScale);

			if(BuyD  < (25.0 * Instrument.Point)) 
			{
				BuyD  = Math.Round(staticTP * Instrument.Point, Instrument.PriceScale);
			}
			if(SellD < (25.0 * Instrument.Point)) 
			{
				SellD = Math.Round(staticTP * Instrument.Point, Instrument.PriceScale);
			}
			
			BuyPositionOP[0]  = BolaDown;
			SellPositionOP[0] = BolaUp;
//////////// recalculate
			BuyPositionTP[0]  = Math.Round(BuyPositionOP[0]  + staticTP * Instrument.Point , Instrument.PriceScale);
			SellPositionTP[0] = Math.Round(SellPositionOP[0] - staticTP * Instrument.Point , Instrument.PriceScale);
////////////////////////////			
			BuyPositionSL[0]  = Math.Round(PivO_S3 - ((BuyD  < (200.0 * Instrument.Point))? (staticSL * Instrument.Point) :  BuyD), Instrument.PriceScale);
			SellPositionSL[0] = Math.Round(PivO_R3 + ((SellD < (200.0 * Instrument.Point))? (staticSL * Instrument.Point) : SellD), Instrument.PriceScale);
			
			double SellP = 0.0;
			double BuyP  = 0.0;
			
			double SellPP = 0.0;
			double BuyPP  = 0.0;

			SellP += SellPositionOP[0];
			BuyP  += BuyPositionOP[0];

			
			for(int oL = 1; oL < _QuantityOfOrders; oL++)
			{
				BuyPositionOP[oL]  = Math.Round(BuyPositionOP[oL-1]  - ((BuyD  < (100.0 * Instrument.Point))? (staticSL * Instrument.Point):  BuyD) , Instrument.PriceScale);
				SellPositionOP[oL] = Math.Round(SellPositionOP[oL-1] + ((SellD < (100.0 * Instrument.Point))? (staticSL * Instrument.Point): SellD) , Instrument.PriceScale);
				
				SellP += SellPositionOP[oL];
				BuyP  += BuyPositionOP[oL];
				SellPP += SellPositionOP[oL];
				BuyPP  += BuyPositionOP[oL];
				
				BuyPositionSL[oL]  = Math.Round(PivO_S3 - ((BuyD  < (100.0 * Instrument.Point))? (staticSL * Instrument.Point):  BuyD) , Instrument.PriceScale);								
				SellPositionSL[oL] = Math.Round(PivO_R3 + ((SellD < (100.0 * Instrument.Point))? (staticSL * Instrument.Point): SellD) , Instrument.PriceScale);								

				BuyPositionTP[oL]  = Math.Round((BuyPositionTP[0]  +  BuyPP)/(double)(oL + 1), Instrument.PriceScale);
				SellPositionTP[oL] = Math.Round((SellPositionTP[0] + SellPP)/(double)(oL + 1), Instrument.PriceScale);
			}
			
			SellP = Math.Round( SellP /= (double) _QuantityOfOrders, Instrument.PriceScale);
			BuyP =  Math.Round( BuyP  /= (double) _QuantityOfOrders, Instrument.PriceScale);
			
			Vbuy  = Math.Round(c_cDepo * _AvaLost * Instrument.Point / ((double)_QuantityOfOrders * Math.Abs(BuyP  - BuyPositionSL[_QuantityOfOrders  - 1])), 2); 
			Vsell = Math.Round(c_cDepo * _AvaLost * Instrument.Point / ((double)_QuantityOfOrders * Math.Abs(SellP - SellPositionSL[_QuantityOfOrders - 1])), 2); 

			Vbuy  = (Vbuy  > vol)? vol : Vbuy;
			Vsell = (Vsell > vol)? vol : Vsell;
			
			XXPrint(
					"Bcp = {0} => BSL {2} Vb = {4} || Scp={1} SSL={3} Vs = {5} || DD ={6}", 
					BuyP.ToString("0.00000"), 
					SellP.ToString("0.00000"), 
					BuyPositionSL[_QuantityOfOrders-1].ToString("0.00000"), 
					SellPositionSL[_QuantityOfOrders-1].ToString("0.00000"),
					Vbuy.ToString("0.00"),
					Vsell.ToString("0.00"),
					c_cDepo
					); 

			SetUpArrows();

			FullPositionCalculated = true;
			for(int i = 0; i < _QuantityOfOrders; i++)
			{
				if(
				(double.IsNaN(BuyPositionOP[i]))					//(BuyPositionOP[i].Equals(double.NaN))
				||
				(double.IsNaN(SellPositionOP[i]))					//(SellPositionOP[i].Equals(double.NaN))
				||
				(double.IsNaN(BuyPositionTP[i]))					//(BuyPositionTP[i].Equals(double.NaN))
				||
				(double.IsNaN(SellPositionTP[i]))					//(SellPositionTP[i].Equals(double.NaN))
				||
				(double.IsNaN(BuyPositionSL[i]))					//(BuyPositionSL[i].Equals(double.NaN))
				||
				(double.IsNaN(SellPositionSL[i]))					//(SellPositionSL[i].Equals(double.NaN))
				||
				(double.IsNaN(savedBuyPositionOP[i]))  				//(savedBuyPositionOP[i].Equals(double.NaN))
				||
				(double.IsNaN(savedSellPositionOP[i]))				//(savedSellPositionOP[i].Equals(double.NaN))
				||
				(double.IsNaN(savedBuyPositionTP[i]))				//(savedBuyPositionTP[i].Equals(double.NaN))
				||
				(double.IsNaN(savedSellPositionTP[i]))				//(savedSellPositionTP[i].Equals(double.NaN))
				||
				(double.IsNaN(savedBuyPositionSL[i]))				//(savedBuyPositionSL[i].Equals(double.NaN))
				||
				(double.IsNaN(savedSellPositionSL[i]))				//(savedSellPositionSL[i].Equals(double.NaN))
				)
				{
					FullPositionCalculated = false; 
					XXPrint("[*!{0}!*NaN**] - invalid Full Positions Calculation ...", Bars[_CIndex].Time.ToString("o"));
					break;
				}	
			}
		}
/// <summary>
///  for future projects ...
/// </summary>
		protected void CalculateADDevia()
		{
			int aPeriod = BBperiod;
			int CurrentBI = _CIndex;
			int cN = 0;
			
			double A = 0.0;
			double Dh2 = 0.0;
			double Dl2 = 0.0;
			double D2 = 0.0;

			for( int b = CurrentBI; (b > (CurrentBI - aPeriod)) && (b >= 0); b--)
			{
				if(
					(double.IsNaN(Bars[b].High)) 					//Bars[b].High.Equals(double.NaN) 
					|| 	
					(double.IsNaN(Bars[b].Low))						//Bars[b].Low.Equals(double.NaN)
					|| 	
					(double.IsNaN(Bars[b].Close))					//Bars[b].Close.Equals(double.NaN)
					||	
					(double.IsNaN(Bars[b].Open))					//Bars[b].Open.Equals(double.NaN)
				)
				{
					continue;
				}
				
				A += (
						(	
						Bars[b].High 
					+ 	
						Bars[b].Low 
					+ 	
						Bars[b].Close 
					+ 	
						Bars[b].Open
						) 
					* 
						0.25
					 );
				cN++;	
			}
			
			A /= (double)((cN == 0)? 1: cN);	
			cN = 0;
			
			for( int b = CurrentBI; (b > (CurrentBI - aPeriod)) && (b >= 0); b--)
			{
				if(
					(double.IsNaN(Bars[b].High)) 					//Bars[b].High.Equals(double.NaN) 
					|| 	
					(double.IsNaN(Bars[b].Low))						//Bars[b].Low.Equals(double.NaN)
					|| 	
					(double.IsNaN(Bars[b].Close))					//Bars[b].Close.Equals(double.NaN)
					||	
					(double.IsNaN(Bars[b].Open))					//Bars[b].Open.Equals(double.NaN)
				)
				{
					continue;
				}

				Dh2 = Bars[b].High - A;
				Dl2 = Bars[b].Low - A; 
				
				Dh2 *= Dh2;	
				Dl2 *= Dl2;
				
				D2 += ((Dh2 > Dl2) ? Dh2 : Dl2);
				cN++;
			}
			
			D2 = Math.Sqrt( D2 / (double)((cN == 0)? 1: cN));
			
		}
/// <summary>
/// Silly Universal Print 
/// </summary>
/// <param name="xxformat"></param>
/// <param name="parameters"></param>		
		
		protected void XXPrint(string xxformat, params object[] parameters)
		{
			if(LogSW!=null)
			{
				if(LogSW.BaseStream.CanSeek && LogSW.BaseStream.CanWrite)
				{
					LogSW.BaseStream.Seek(0,SeekOrigin.End);
					LogSW.WriteLine(string.Format(xxformat,parameters));
					LogSW.Flush();
				}
			}
			if(DebugMode)
			{
				Print(xxformat,parameters);
			}	
		}
		
		protected void InitLogFile()
		{
			if(LogMode)
			{
				trueLogPath = LogPath+DateTime.Now.Ticks.ToString().Trim() + ".LOG";
				if(File.Exists(trueLogPath))
				{ 
					File.Delete(trueLogPath);
				}
				LogSW = new StreamWriter(File.Open(trueLogPath, 
													FileMode.Create, 
														FileAccess.ReadWrite, 
															FileShare.ReadWrite)); 
			}
		}
		
		protected override void Deinit()
		{
			if(LogMode && LogSW!=null)
			{
				LogSW.Flush();
				LogSW.Close();
			}
		}

	
		protected void SetUpArrows()
		{
//
			if((cArrowUp != null) && Tools.GetById(cAUG).Equals(cArrowUp))
			{
				Tools.Remove(cArrowUp);
				cArrowUp = null; // Artifishial using... 
			}	
// 			
			if((cArrowDown != null) && Tools.GetById(cADG).Equals(cArrowDown))
			{
				Tools.Remove(cArrowDown);
				cArrowDown = null; // Artifishial using... 
			}
//
			if(Bars[Bars.Range.To-1].Open <= Bars[Bars.Range.To-1].Close)
			{
				cArrowUp = Tools.Create<ArrowUp>();   		
				cAUG = cArrowUp.Id;
					cArrowUp.Color 	 = Color.Blue;
					cArrowUp.Width 	 = 1 ;
					cArrowUp.Point 	 = new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].Low); 
			}
//			
			if(Bars[Bars.Range.To-1].Open >= Bars[Bars.Range.To-1].Close)
			{
				cArrowDown = Tools.Create<ArrowDown>();   		
				cADG = cArrowDown.Id;			
					cArrowDown.Color = Color.Red;
					cArrowDown.Width = 1 ;
					cArrowDown.Point = new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High);
			}
//
		}
	}
}
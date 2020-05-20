using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using IPro.Model.Client.MarketData;
using IPro.Model.Programming.Indicators.Standard;
using System.Collections.Generic;
using System.IO;


namespace IPro.TradeSystems
{
    [TradeSystem("F03_1")]       //copy of "F03"
	
	public class Elder : TradeSystem
    {
		[Parameter("Начало:", DefaultValue = 4)]
        public int tm1 { get; set; }		
		[Parameter("Конец:", DefaultValue = 17)]
        public int tm2 { get; set; }	
		[Parameter("Buy:", DefaultValue = true)]
        public bool tu { get; set; }	
		[Parameter("Sell:", DefaultValue = true)]
        public bool td { get; set; }	

		[Parameter("Fractal :", DefaultValue = 7, MinValue = 2, MaxValue = 200)]
		public int frac { get;set; }
		
		[Parameter("Debug_Mode", DefaultValue = true)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = true)]
		public bool LogMode { get;set; }

		[Parameter("Log_Path", DefaultValue = @"F11")]
		public string LogPath { get;set; }
		
		[Parameter("Log_FileName", DefaultValue = @"Fish11_")]
		public string LogFileName { get;set; }
		
 		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		private bool torg=false;
		public static int ci = 0;				
		private double maxu=0,mind=1000,rz1,rz2,cn;
		private double max1,max2,max3,max4,min1,min2,min3,min4;
		private int kl=0;
		public static int magicNumber = 57557;		

		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		private string trueLogPath = "";
		private FisherTransformOscillator _ftoInd;
		

     
		protected override void Init()
        {	
						
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			InitLogFile();

        }        

        
        protected override void NewBar()
        {  
			ci = Bars.Range.To - 1;
			if(
				_ftoInd.Ma1Series[ci].Equals(double.NaN)
				||
				_ftoInd.Ma2Series[ci].Equals(double.NaN)
								||
				_ftoInd.FisherSeries[ci].Equals(double.NaN)
				||
				Instrument.Ask.Equals(double.NaN)
				||
				Instrument.Bid.Equals(double.NaN)
			   )
			{
				return;
			}
		/*if ( Bars[Bars.Range.To-1].Time.Hour>=tm2 ) 
		  {  
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
	
		  }*/
  
			
			//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
		
			//UP - ВВЕРХ - визанаємо МІНІМУМ
			if(_ftoInd.FisherSeries[Bars.Range.To-1]>0.3 &&  _ftoInd.FisherSeries[Bars.Range.To-2]<0.3 )
			{   kl=0; mind=1000; 
				do
				{ kl++; if(Bars[Bars.Range.To-kl].Low<mind) mind=Bars[Bars.Range.To-kl].Low;}
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]<0  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]>0));
				min4=min3;min3=min2;min2=min1; min1=mind;


				//UpMBuy();
				Buy1();		
				//if( min1>min2 && max1>max2 && tu) {var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-1].Time;
				//					Buy1(); td=true; tu=false; }
			} 
            // DOWN - ВНИЗ - ВИЗНАЧАЄМО МАКСИМУМ
			if(_ftoInd.FisherSeries[Bars.Range.To-1]<-0.3 &&  _ftoInd.FisherSeries[Bars.Range.To-2]>-0.3  ) 
			{   kl=0;maxu=0;
				do
				{ kl++; if(Bars[Bars.Range.To-kl].High>maxu) maxu=Bars[Bars.Range.To-kl].High;  }
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]>-0  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]<-0));
				max4=max3;max3=max2;max2=max1; max1=maxu;
		
				Sell1();
				//UpMSell();
				//if( max2>max1 && min2>min1) {  var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-1].Time;
				//   					Sell1(); tu=true; td=false; }
		     }
			TrailActiveOrders();
			 if(_ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma1Series[Bars.Range.To-1] &&  
				_ftoInd.FisherSeries[Bars.Range.To-2]>_ftoInd.Ma1Series[Bars.Range.To-2] )  { 
					CloseBuy1(); }
			if(_ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma1Series[Bars.Range.To-1] &&  
				_ftoInd.FisherSeries[Bars.Range.To-2]<_ftoInd.Ma1Series[Bars.Range.To-2] )  { CloseSell1();
					}
		
		}		

protected void UpMSell()	
			{	
				if(posGuidSell!=Guid.Empty) { var result = Trade.UpdateMarketPosition(posGuidSell, max1, null, null); 
						if (result.IsSuccessful)  posGuidSell = result.Position.Id; }			
			}
protected void UpMBuy()	
			{	
				if(posGuidBuy!=Guid.Empty) { var result = Trade.UpdateMarketPosition(posGuidBuy, min1, null, null); 
						if (result.IsSuccessful)  posGuidBuy = result.Position.Id; }			
			}
protected void Buy1()
		    {
				if (posGuidBuy==Guid.Empty) { var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
												//Stops.InPrice(min3-0.0003,null), null, null);
												Stops.InPips(200,null), null, magicNumber);
						
				if (result1.IsSuccessful)  {posGuidBuy=result1.Position.Id; 				
				rz1=Math.Round((max1-min1)*100000,0);
				rz2=Math.Round((Bars[Bars.Range.To-1].High-min1)*100000,0);
				cn=Bars[Bars.Range.To-1].Close;}}
			}

protected void Sell1()
		    {
				if (posGuidSell==Guid.Empty) {var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
						//Stops.InPrice(max3+0.0003,null), null, null);    						
					 Stops.InPips(200,null), null, magicNumber);
				if (result2.IsSuccessful)  { posGuidSell=result2.Position.Id;
				rz1=Math.Round((max1-min1)*100000,0);
				rz2=Math.Round((max1-Bars[Bars.Range.To-1].Low)*100000,0);
				cn=Bars[Bars.Range.To-1].Close;}} 
			}

protected void CloseBuy1()
		    {
				if (posGuidBuy!=Guid.Empty) 
				{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) { posGuidBuy = Guid.Empty;
				 XXPrint("Buy;{0};{1};{2};{3}",Bars[Bars.Range.To-1].Time,rz1,rz2,Math.Round((Bars[Bars.Range.To-1].Close-cn)*100000,0));}}	
			}
protected void CloseSell1()
		    {
				if (posGuidSell!=Guid.Empty) 
				{ var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) 
				{posGuidSell = Guid.Empty;
				XXPrint("SELL;{0};{1};{2};{3}",Bars[Bars.Range.To-1].Time,rz1,rz2,Math.Round((cn-Bars[Bars.Range.To-1].Close)*100000,0)); }}	
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
			
			protected void TrailActiveOrders()
		{
			
			IPosition[] activeposition = Trade.GetActivePositions(magicNumber);
			
			if(activeposition != null)
			{
				for(int p = 0; p < activeposition.Length; p++)
				{
					TradeResult tr = null;
					if(activeposition[p].Type == ExecutionRule.Buy)
					{
						tr = Trade.UpdateMarketPosition(
								activeposition[p].Id,
								getSL(1),
								null,
								" - update TP,SL"
														);
						if(tr == null)
						{
							XXPrint("Can't update  buy position {0}", activeposition[p].Id);
						}
					}
					if(activeposition[p].Type == ExecutionRule.Sell)
					{
						tr = Trade.UpdateMarketPosition(
								activeposition[p].Id,
								getSL(0),
								null,
								" - update TP,SL"
														);
						if(tr == null)
						{
							XXPrint("Can't update sell position {0}", activeposition[p].Id);
						}
					}
				}
			}
		}	
				protected double getSL(int type)
		{
			switch(type)
			{
				case 0:
						{
							double MAX = double.MinValue;
							for(int i = 0; i < frac; i++)
							{
								if(Bars[ci - i].High > MAX)
									MAX = Bars[ci - i].High; 
							}	
							return Math.Round(MAX, Instrument.PriceScale);
						}
				case 1:
						{
							double MIN = double.MaxValue;
							for(int i = 0; i < frac; i++)
							{
								if(Bars[ci - i].Low < MIN)
									MIN = Bars[ci - i].Low; 
							}	
							return Math.Round(MIN, Instrument.PriceScale);
						}
				default: 
					break;
			}
			return 0.0;
		}
    }
}

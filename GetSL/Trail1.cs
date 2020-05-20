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
    [TradeSystem("Trail1")]         //copy of "Lo1"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Версия", DefaultValue = "10.05.2018")]
        public string CommentText { get; set; }
		
		[Parameter("StopLoss", DefaultValue = 250)]
        public int SL1 { get; set; }		
		
		[Parameter("Torg", DefaultValue = false)]
        public bool  torg { get; set; }			

		[Parameter("Buy", DefaultValue = false)]
        public bool  Buy7 { get; set; }	
		
		[Parameter("Sell", DefaultValue = false)]
        public bool  Sell7 { get; set; }		
		
		[Parameter("Fractal", DefaultValue = 7)]
		public int frac { get;set; }		

		[Parameter("Отступ Stop :", DefaultValue = 20)]
		public int dl { get;set; }			
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private IPosition[] posActiveMineB;
		private IPosition[] posActiveMineS;		
		public FisherTransformOscillator _ftoInd;
		public DateTime DTime,Dsl; // Время
		private bool FsU,FsD;
		private int mgS,mgB,ci = 0;
		private double dlt,zAm;
		private HorizontalLine hline;
		private VerticalLine vline;
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		private string trueLogPath = "";		
		
       protected override void Init()
        { 	
			frac=7;
			dlt=dl*Instrument.Point;
			hline = Tools.Create<HorizontalLine>();
			vline = Tools.Create<VerticalLine>();
        }            
//===============================================================================================================================
        protected override void NewBar()
        {
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
		
//=== КОРЕКЦИЯ =======================================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) { posGuidBuy=Guid.Empty;  }  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) { posGuidSell=Guid.Empty;  } 
//=========  Рисуем линию  начала торгов Европы =============================================================================			
			if ( DTime.Hour==3 && DTime.Minute==00 ) 
			{  var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time.AddHours(4); vl1.Color=Color.Aqua; 	
			   			zAm=Bars[TimeToIndex(Bars[ci].Time.AddHours(-6), Timeframe)].Open;
			   var vl2 = Tools.Create<VerticalLine>(); vl2.Time=Bars[ci].Time.AddHours(-6); vl2.Color=Color.DarkCyan; 
			   var toolPolyLine = Tools.Create<PolyLine>();
				toolPolyLine.Color=Color.DarkGoldenrod;
               toolPolyLine.AddPoint(new ChartPoint(vl2.Time=Bars[ci].Time.AddHours(-6), zAm));
               toolPolyLine.AddPoint(new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(14), zAm));				
			}
							
//=== Трелинг  =======================================================================================================================	
			TrailActiveOrders();
//==== Торги =========================================================================================================================
	 
					if (posGuidBuy==Guid.Empty && Buy7 && torg ) { torg=false;		
						     var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								             Stops.InPips(SL1,null),null,mgB);
						     if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id;
							// var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Red; 
						} 
			   			
					if (posGuidSell==Guid.Empty && Sell7 && torg) {	torg=false; 	
							 var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                 Stops.InPips(SL1,null),null,mgS); 
						     if (result207.IsSuccessful)  posGuidSell=result207.Position.Id;
							 // var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Blue;
						}
		}		
//===== Функции =================================================================================================================   
		protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { 
			  var tr = Trade.UpdateMarketPosition(posGuidBuy,	  getSL(1),null,DTime.ToString()); 
			  
			    hline.Price = getSL(1);
			    vline.Time = Dsl;
		  //Print("{0} BUY  UPDATE  posGuidBuy={1}",Bars[ci].Time,posGuidBuy.ToString());
		  }
		  if(posGuidSell!=Guid.Empty) { 
			  var tr = Trade.UpdateMarketPosition(posGuidSell,  getSL(0),null,DTime.ToString());
				hline.Price = getSL(0);
			    vline.Time = Dsl;

		  //Print("{0} SELL UPDATE  posGuidSell={1}",Bars[ci].Time,posGuidSell.ToString()); 
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
								    Dsl = Bars[ci - i].Time;
							}	
							return Math.Round(MAX+dlt+Instrument.Spread, Instrument.PriceScale);
						}
				case 1:
						{
							double MIN = double.MaxValue;
							for(int i = 0; i < frac; i++)
							{
								if(Bars[ci - i].Low < MIN)
									MIN = Bars[ci - i].Low;
								    Dsl = Bars[ci - i].Time;
							}	
							return Math.Round(MIN-dlt-Instrument.Spread, Instrument.PriceScale);
						}
				default: 
					break;
			}
			return 0.0;
		}
    }
}

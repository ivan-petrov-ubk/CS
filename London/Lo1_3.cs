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

 //Рисует линию от закрытия америки до открытия америки

namespace IPro.TradeSystems
{
    [TradeSystem("Lo1_3")]        
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Версия", DefaultValue = "10.05.2018")]
        public string CommentText { get; set; }
		
		[Parameter("StopLoss", DefaultValue = 250)]
        public int SL1 { get; set; }		
		
		[Parameter("Fractal", DefaultValue = 7)]
		public int frac { get;set; }	
		

		[Parameter("Отступ Stop :", DefaultValue = 30)]
		public int dl { get;set; }			
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
	

		public DateTime DTime; // Время
		public Fractals _frInd;
		public FisherTransformOscillator _ftoInd;	
		private double dlt,zAm,zAm1;
		private int ci;		
		private bool torg;
		private DateTime dt1; 
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		private string trueLogPath = "";		
		
       protected override void Init()
        { 	
			Print("{0} Start INIT ",Bars[Bars.Range.To-1].Time);
			InitLogFile(); 
			dlt=dl*Instrument.Point; 
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
       }            
//===============================================================================================================================
        protected override void NewBar()
        {
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
			
			
//=========  Рисуем линию  начала торгов Европы =============================================================================			
			if ( DTime.Hour==7 && DTime.Minute==00 ) 
			{  var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Aqua; 	
			   			zAm=Bars[TimeToIndex(Bars[ci].Time.AddHours(-9), Timeframe)].Open;
			   var vl2 = Tools.Create<VerticalLine>(); vl2.Time=Bars[ci].Time.AddHours(-10); vl2.Color=Color.DarkCyan; 
			   var toolPolyLine = Tools.Create<PolyLine>();
				toolPolyLine.Color=Color.DarkGoldenrod;
               toolPolyLine.AddPoint(new ChartPoint(vl2.Time=Bars[ci].Time.AddHours(-10), zAm));
               toolPolyLine.AddPoint(new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(8), zAm));				
			}
			
	Print("{0} - America={1}",Bars[ci].Time,zAm);
//=== КОРЕКЦИЯ =======================================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) { posGuidBuy=Guid.Empty;  }  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) { posGuidSell=Guid.Empty;  } 
				
//=== Трелинг  =======================================================================================================================	
			TrailActiveOrders();
			if(zAm!=zAm1) {torg=true; zAm1=zAm;}
//=== Торговля ====================================================================		
/*				    if(posGuidBuy==Guid.Empty && Bars[Bars.Range.To-1].High>zAm && DTime.Hour==6 && DTime.Minute==00 && torg){ torg=false;
						  var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, 
							           zAm, 0, 
							           Stops.InPips(200,400), null, null, null);	
						     if (result.IsSuccessful)  posGuidBuy=result.Position.Id; }
					
					 if(posGuidSell==Guid.Empty && Bars[Bars.Range.To-1].Low<zAm && DTime.Hour==6 && DTime.Minute==45 && torg){ torg=false;
							var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  
							               zAm, 0, 
							               Stops.InPips(200,400), null, null, null);
						        if (result1.IsSuccessful)  posGuidSell=result1.Position.Id; }
*/			
//----- Сбор логов --------------------------------------------------------------------------------------------------------------			
					
	if(posGuidBuy!=Guid.Empty)  XXPrint("{0} ----- {1} ",Bars[ci].Time,posGuidBuy.ToString());
	if(posGuidSell!=Guid.Empty)	XXPrint("{0} ----- {1}",Bars[ci].Time,posGuidSell.ToString());
	if(posGuidSell==Guid.Empty && posGuidBuy==Guid.Empty)
		                        XXPrint("{0} ----- ",Bars[ci].Time);
		}		
//===== Функции =================================================================================================================   
		protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { 
			  var tr = Trade.UpdateMarketPosition(posGuidBuy,	  getSL(1),null,DTime.ToString()); 
		  XXPrint("{0} BUY  UPDATE  posGuidBuy={1}",Bars[ci].Time,posGuidBuy.ToString() );
		  }
		  if(posGuidSell!=Guid.Empty) { 
			  var tr = Trade.UpdateMarketPosition(posGuidSell,  getSL(0),null,DTime.ToString()); 
		  XXPrint("{0} SELL UPDATE posGuidSell={1}",Bars[ci].Time,posGuidSell.ToString()); }
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
							return Math.Round(MAX+dlt+Instrument.Spread, Instrument.PriceScale);
						}
				case 1:
						{
							double MIN = double.MaxValue;
							for(int i = 0; i < frac; i++)
							{
								if(Bars[ci - i].Low < MIN)
									MIN = Bars[ci - i].Low; 
							}	
							return Math.Round(MIN-dlt-Instrument.Spread, Instrument.PriceScale);
						}
				default: 
					break;
			}
			return 0.0;
		}
		protected void InitLogFile()
		{trueLogPath=PathToLogFile+"\\"+Instrument.Name.ToString()+".log";}
		
		protected void XXPrint(string xxformat, params object[] parameters)
		{
				var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);
		}
    }
}

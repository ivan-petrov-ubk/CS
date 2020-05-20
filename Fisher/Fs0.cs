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
    [TradeSystem("Lo1")]        //copy of "Lo0"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Версия", DefaultValue = "02.11.2018")]
        public string CommentText { get; set; }
		
		[Parameter("StopLoss", DefaultValue = 250)]
        public int SL1 { get; set; }	
		[Parameter("Отступ Stop :", DefaultValue = 30)]
		public int dl { get;set; }				
		
		[Parameter("Torg", DefaultValue = false)]
        public bool  torg { get; set; }			

		[Parameter("Buy", DefaultValue = false)]
        public bool  Buy7 { get; set; }	
		
		[Parameter("Sell", DefaultValue = false)]
        public bool  Sell7 { get; set; }		

		[Parameter("Blue line", DefaultValue = false)]
        public bool  Blue { get; set; }
	
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;

		public FisherTransformOscillator _ftoInd;
		public DateTime DTime; // Время
		private bool FsU,FsD,FsU1,FsD1;
		private int ci = 0;
		private double dlt;
		
		
		
       protected override void Init()
        { 	FsU=false;FsD=false;
			
			dlt=dl*Instrument.Point; 
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
			
        }            
//===============================================================================================================================
        protected override void NewBar()
        {
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
		
//=======  Определение разворота ==========================================================================		

// Выход синей линии
	if(_ftoInd.FisherSeries[Bars.Range.To-1]> _ftoInd.Ma1Series[Bars.Range.To-1]) FsU1=true; else FsU1=false;
	if(_ftoInd.FisherSeries[Bars.Range.To-1]< _ftoInd.Ma1Series[Bars.Range.To-1]) FsD1=true; else FsD1=false;

// Проход через 0
	if(_ftoInd.FisherSeries[Bars.Range.To-2]<0 && _ftoInd.FisherSeries[Bars.Range.To-1]>0) FsU=true; else FsU=false;
	if(_ftoInd.FisherSeries[Bars.Range.To-2]>0 && _ftoInd.FisherSeries[Bars.Range.To-1]<0) FsD=true; else FsD=false;
		
//=== КОРЕКЦИЯ ========================================================================================================						 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) { posGuidBuy=Guid.Empty;  }  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) { posGuidSell=Guid.Empty;  } 
				

//====================================================================================================================================

					if (posGuidBuy==Guid.Empty && Buy7 && torg && FsU) { torg=false;		
						     var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								             Stops.InPips(SL1,null),null,null);
						     if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id;
						} 
			   			
					if (posGuidSell==Guid.Empty && Sell7 && torg && FsD) {	torg=false; 	
							 var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                 Stops.InPips(SL1,null),null,null); 
						     if (result207.IsSuccessful)  posGuidSell=result207.Position.Id;
												}
			
//====================================================================================================================================

		if(Blue)	{		
					if (posGuidBuy!=Guid.Empty && FsD1) 
					{var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty;}
				if (posGuidSell!=Guid.Empty && FsU1) 	
					{var res3 = Trade.CloseMarketPosition(posGuidSell); if (res3.IsSuccessful) posGuidSell = Guid.Empty;}	
		} else 
		{
					if (posGuidBuy!=Guid.Empty && FsD) 
					{var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty;}
				if (posGuidSell!=Guid.Empty && FsU) 	
					{var res3 = Trade.CloseMarketPosition(posGuidSell); if (res3.IsSuccessful) posGuidSell = Guid.Empty;}	
			
			
		}
		
				}		
		
		
	
    }
}

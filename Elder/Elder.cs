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



namespace IPro.TradeSystems
{
    [TradeSystem("Elder")]
	
	public class Elder : TradeSystem
    {
		[Parameter("Время работы:", DefaultValue = "EUR=7-17  USD=12-22 AUD=21-7 JPY=23-9")]
        public string CommentText { get; set; }
		[Parameter("Начало:", DefaultValue = 0)]
        public int tm1 { get; set; }		
		[Parameter("Конец:", DefaultValue = 24)]
        public int tm2 { get; set; }	
		[Parameter("Через 0:", DefaultValue = false)]
        public bool tm0 { get; set; }	
		
		
 		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
        private ParabolicSar _psInd;
		private MovingAverage _maInd;
		private MovingAverageConvergenceDivergence _macdInd;
		private BollingerBands _bbInd;
		private double mv1,md1,ss1,pr1,bc1,bd1,bu1,dmv,dmacd;
		private double mv2,md2,ss2,pr2,bc2,bd2,bu2,dbu,dbd,db;
		private bool ib=true,isl=true,torg=false;
		
     
		protected override void Init()
        {
  			_psInd= GetIndicator<ParabolicSar>(Instrument.Id, Timeframe);
			_psInd.CoefStep=0.01;
			_maInd= GetIndicator<MovingAverage>(Instrument.Id, Timeframe);
			_maInd.Period=26;
			_macdInd= GetIndicator<MovingAverageConvergenceDivergence>(Instrument.Id, Timeframe);
			_bbInd = GetIndicator<BollingerBands>(Instrument.Id, Timeframe);
        }        

        
        protected override void NewBar()
        {   _psInd.ReInit();
			//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			// EUR - 7-17  USD-12-22 AUD-21-7 JPY-23-9
		if ( Bars[Bars.Range.To-1].Time.Hour>=tm2 ) 
		  {  
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
	
		  }	
			if(!tm0)
			if ( Bars[Bars.Range.To-1].Time.Hour>tm1 && Bars[Bars.Range.To-1].Time.Hour<tm2) torg=true; else torg=false;
			else
			if ( Bars[Bars.Range.To-1].Time.Hour>tm1 || Bars[Bars.Range.To-1].Time.Hour<tm2) torg=true; else torg=false;	
			
 			mv1 = _maInd.SeriesMa[Bars.Range.To-1];
			md1 = _macdInd.SeriesMacd[Bars.Range.To-1];
			ss1 = _macdInd.SeriesSignal[Bars.Range.To-1];
 			mv2 = _maInd.SeriesMa[Bars.Range.To-2];
			md2 = _macdInd.SeriesMacd[Bars.Range.To-2];
			ss2 = _macdInd.SeriesSignal[Bars.Range.To-2];
			
			bc1 = _bbInd.SeriesCenter[Bars.Range.To-1];
			bd1 = _bbInd.SeriesDown[Bars.Range.To-1];
			bu1 = _bbInd.SeriesUp[Bars.Range.To-1];
			bc2 = _bbInd.SeriesCenter[Bars.Range.To-2];
			bd2 = _bbInd.SeriesDown[Bars.Range.To-2];
			bu2 = _bbInd.SeriesUp[Bars.Range.To-2];
			
			dmv=mv1-mv2; dmacd=ss1-ss2;
			dbd=(bd1-bd2)*10000; dbu=(bu1-bu2)*100000; db=Math.Round((bu1-bd1)*100000,0);
			
			if(torg && dmv>0 && dmacd>0) 
			{  //Buy
				var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-1].Time;
				// Print("{0}  Buy - db={1} dbu={2} dbd={3} ib={4}",Bars[Bars.Range.To-1].Time,db,dbu,dbd,ib);
			    if (posGuidBuy==Guid.Empty) { ib=true; var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1,Stops.InPips(300,null), null, null);
											Print("BUY   {0} - {1} ",Bars[Bars.Range.To-1].Time,db);					      
											if (result1.IsSuccessful)  posGuidBuy=result1.Position.Id; } 				
			}
			
			if(dmv<0 || dmacd<0) 
			{	if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
									{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}		        
			}
			
			
			if(torg && dmv<0 && dmacd<0) 
			{ //Sell
				var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-1].Time; 
				// Print("{0} Sell - db={1} dbu={2} dbd={3} isl={4}",Bars[Bars.Range.To-1].Time,db,dbu,dbd,isl);
				if (posGuidSell==Guid.Empty) {ib=false; 	var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,Stops.InPips(300,null), null, null);
											Print("SELL  {0} - {1} ",Bars[Bars.Range.To-1].Time,db);
												if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;	} 				
			}
			
			if(dmv>0 || dmacd>0) { if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
											{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
			}	
			
			
			// pr1 = _psInd.SarSeries[Bars.Range.To-1];
			
			// Print("MV={0} macd_ser={1} macd_sign={2} parabolic={3}",mv,md,ss,pr);
        } 
        
    }
}

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
    [TradeSystem("Parabolic_London")] //copy of "Parabolic"
    public class Parabolic : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		[Parameter("TP:", DefaultValue = 100)]
        public int TP1 { get; set; }			
		[Parameter("StopLoss:", DefaultValue = 250)]
        public int SL1 { get; set; }	
		[Parameter("Отступ Stop :", DefaultValue = 30)]
		public int dl { get;set; }	
		[Parameter("Fractal", DefaultValue = 7)]
		public int frac { get;set; }			
		
        private ParabolicSar _psInd;
 		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public double pr1;
		private int ci = 0;
		public DateTime DTime; // Время
		public bool torg;
		private double dlt,frUp,frDown,frUp0,frDown0;
		public Fractals _frInd;	
		private double tpU,tpD;
		
        protected override void Init()
        {
  			_psInd= GetIndicator<ParabolicSar>(Instrument.Id, Timeframe);
			_psInd.CoefStep=0.01;
			dlt=dl*Instrument.Point; // Отступ от стопа
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
        }        

        
        protected override void NewBar()
        {
            _psInd.ReInit();
			pr1 = _psInd.SarSeries[Bars.Range.To-1];
			pr1=Math.Round(pr1,Instrument.PriceScale); 
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
						TrailActiveOrders();
//=========  Рисуем линию  начала торгов Европы =============================================================================			
			if ( DTime.Hour==7 && DTime.Minute==00 ) 
			{  var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Aqua; vl1.Width=3;	}	
			if ( DTime.Hour<7 ) torg=true; 
			
//====  Fractal ====================================================================================
			  if (_frInd.TopSeries[Bars.Range.To-5]>0) 		frUp=Bars[Bars.Range.To-5].High; 
			  if (_frInd.BottomSeries[Bars.Range.To-5]>0)   frDown=Bars[Bars.Range.To-5].Low; 				
			//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty; 
if ( DTime.Hour>6 ) { 
			// Parabolic выше цены
			if(Bars[Bars.Range.To-1].Close<pr1 ) {	
			if(posGuidBuy==Guid.Empty && torg){ torg=false; var result1 = Trade.OpenPendingPosition(Instrument.Id, 
					ExecutionRule.BuyStop, 0.1,  pr1, 0, Stops.InPips(SL1,TP1), null, null, null);
					if (result1.IsSuccessful)   posGuidBuy=result1.Position.Id; }
			 
			if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
				      {  tpU=Math.Round(pr1+(TP1*Instrument.Point),Instrument.PriceScale); 
						  var result = Trade.UpdatePendingPosition(posGuidBuy, 0.1, pr1,null, tpU); 
						if (result.IsSuccessful)  posGuidBuy = result.Position.Id; }
					  
			//if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			//	{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}						  
	  
			}
			
			// Parabolic Ниже цены
			if(Bars[Bars.Range.To-1].Close>pr1) {	
			if(posGuidSell==Guid.Empty && torg) { torg=false; var result1 = Trade.OpenPendingPosition(Instrument.Id, 
					ExecutionRule.SellStop, 0.1,  pr1, 0, Stops.InPips(SL1,TP1), null, null, null);
					if (result1.IsSuccessful)   posGuidSell=result1.Position.Id; }
			
			if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			         { tpD=Math.Round(pr1-(TP1*Instrument.Point),Instrument.PriceScale);
						 var result = Trade.UpdatePendingPosition(posGuidSell, 0.1, pr1, null, tpD); 
						if (result.IsSuccessful)  posGuidSell = result.Position.Id; }
			

			//			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			//	{ var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}		
			
			}
			//var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-1].Time;
}	
			// Print("{0} --  parabolic={1}",Bars[Bars.Range.To-1].Time,pr1);
        }
 //===============================================================================================================================   
		protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { var tr = Trade.UpdateMarketPosition(posGuidBuy,	  getSL(1),tpU," - update TP,SL"); }
		  if(posGuidSell!=Guid.Empty) { var tr = Trade.UpdateMarketPosition(posGuidSell,  getSL(0),tpD," - update TP,SL");  }
		} 		  
	    protected double getSLfr(int type)
	    {
			switch(type)
			{   case 0: { return Math.Round(frUp+dlt+Instrument.Spread, Instrument.PriceScale); }
				case 1: { return Math.Round(frDown+dlt+Instrument.Spread, Instrument.PriceScale); }
			default:  break;
						}
			return 0.0;		
		}		
		
		protected double getSL(int type)
		{
			switch(type)
			{   case 0: {   double MAX = double.MinValue;
							for(int i = 0; i < frac; i++)
							{ if(Bars[ci - i].High > MAX)
									MAX = Bars[ci - i].High; 
							}	
							return Math.Round(MAX+dlt+Instrument.Spread, Instrument.PriceScale);
						}
				case 1: {   double MIN = double.MaxValue;
							for(int i = 0; i < frac; i++)
							{  if(Bars[ci - i].Low < MIN)
									MIN = Bars[ci - i].Low; 
							}	
							return Math.Round(MIN-dlt-Instrument.Spread, Instrument.PriceScale);
						}
				default:  break;
			}
			return 0.0;
		}       
 
    }
}
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
    [TradeSystem("L123")] //copy of "Para7"
    public class Para7 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Parabolic-London7 21/07/2018")]
        public string CommentText { get; set; }

		[Parameter("StopLoss:", DefaultValue = 200)]
        public int SL1 { get; set; }	
		[Parameter("Отступ Stop :", DefaultValue = 20)]
		public int dl { get;set; }	
		[Parameter("Fractal", DefaultValue = 7)]
		public int frac { get;set; }	

 		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public double pr1,FU3,FU2,FU1,FD3,FD2,FD1,SL2;
		private int ci = 0;
		public DateTime DTime; // Время
		public bool torg7,torg12,tr;
		private double dlt,frUp,frDown,frUp0,frDown0;
		public Fractals _frInd;	
		private double tpU,tpD;
		
        protected override void Init()
        {
			dlt=dl*Instrument.Point; // Отступ от стопа
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);				
        }        

       
        protected override void NewBar()
        {
           
			 _frInd.ReInit();
			
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
			TrailActiveOrders();
//=========  Рисуем линию  начала торгов Европы =============================================================================			
			if ( DTime.Hour==7 && DTime.Minute==00 ) 
			{  var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Aqua; vl1.Width=2;	}	
			
			if ( DTime.Hour<6 ) { torg7=false; torg12=false; } 
			
//====  Fractal ====================================================================================
			  if (_frInd.TopSeries[Bars.Range.To-5]>0) 		{ frUp=Bars[Bars.Range.To-5].High; FU3=FU2; FU2=FU1; FU1=frUp; } 
			  if (_frInd.BottomSeries[Bars.Range.To-5]>0)   { frDown=Bars[Bars.Range.To-5].Low; FD3=FD2; FD2=FD1; FD1=frDown;  }				
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;            // Event occurs on every new bar
//=== Запрет повторных операций ===========================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active)   { torg7=false; torg12=false; }   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) { torg7=false;  torg12=false; }           // Event occurs on every new bar
//=== КОРЕКЦИЯ ===========================================================================================================							 
	 if ( DTime.Hour==7 && DTime.Minute==00 ) { torg7=true; Print("===================     6:45 =============================="); }
	 if ( DTime.Hour==11 && DTime.Minute==55 ) { torg12=true;	Print("===================     11:55 ============================");}
	  	
	 if ( DTime.Hour==11 && DTime.Minute==40 ) { Print("===================     11:40 ==============================");
		  torg7=false;
	 	if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
				{ var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}
		if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
				{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
				
		if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
				{ var res = Trade.CancelPendingPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}
		if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
				{var res = Trade.CancelPendingPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}			
	 }
	 if ( DTime.Hour==17 && DTime.Minute==45 ) { Print("===================     17:45 ==============================");
		 torg12=false;
	 	if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
				{ var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}
		if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
				{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
				
		if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
				{ var res = Trade.CancelPendingPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}
		if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
				{var res = Trade.CancelPendingPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}			
	 }

	 //=== PARABOLIC ===========================================================================================================							 
if ( torg7 ) { 
			// Parabolic выше цены
			if(posGuidSell==Guid.Empty) {	
			if(posGuidBuy==Guid.Empty){ var result1 = Trade.OpenPendingPosition(Instrument.Id, 
					ExecutionRule.BuyStop, 0.1,  pr1, 0, Stops.InPips(SL1,null), null, null, null);
					if (result1.IsSuccessful)   posGuidBuy=result1.Position.Id; }
			if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
				      { SL2 = Math.Round(pr1-SL1*Instrument.Point,Instrument.PriceScale);
						  var result = Trade.UpdatePendingPosition(posGuidBuy, 0.1, pr1,SL2, null); 
						if (result.IsSuccessful)  posGuidBuy = result.Position.Id; }
			}
			// Parabolic Ниже цены
			if(posGuidBuy==Guid.Empty) {	
			if(posGuidSell==Guid.Empty) { var result1 = Trade.OpenPendingPosition(Instrument.Id, 
					ExecutionRule.SellStop, 0.1,  pr1, 0, Stops.InPips(SL1,null), null, null, null);
					if (result1.IsSuccessful)   posGuidSell=result1.Position.Id; }
			if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			         {  SL2 = Math.Round(pr1+SL1*Instrument.Point,Instrument.PriceScale);
						 var result = Trade.UpdatePendingPosition(posGuidSell, 0.1, pr1, SL2, null); 
						if (result.IsSuccessful)  posGuidSell = result.Position.Id; }
			}
			}	
//=== PARABOLIC ===========================================================================================================	
//=== PARABOLIC ===========================================================================================================							 
if ( torg12 ) { 
			// Parabolic выше цены
			if(Bars[Bars.Range.To-1].High<pr1 && posGuidSell==Guid.Empty) {	
			if(posGuidBuy==Guid.Empty){ var result1 = Trade.OpenPendingPosition(Instrument.Id, 
					ExecutionRule.BuyStop, 0.1,  pr1, 0, Stops.InPips(SL1,null), null, null, null);
					if (result1.IsSuccessful)   posGuidBuy=result1.Position.Id; }
			if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
				      { SL2 = Math.Round(pr1-SL1*Instrument.Point,Instrument.PriceScale);
						  var result = Trade.UpdatePendingPosition(posGuidBuy, 0.1, pr1,SL2, null); 
						if (result.IsSuccessful)  posGuidBuy = result.Position.Id; }
			}
			// Parabolic Ниже цены
			if(Bars[Bars.Range.To-1].Low>pr1 && posGuidBuy==Guid.Empty) {	
			if(posGuidSell==Guid.Empty) { var result1 = Trade.OpenPendingPosition(Instrument.Id, 
					ExecutionRule.SellStop, 0.1,  pr1, 0, Stops.InPips(SL1,null), null, null, null);
					if (result1.IsSuccessful)   posGuidSell=result1.Position.Id; }
			if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			         {  SL2 = Math.Round(pr1+SL1*Instrument.Point,Instrument.PriceScale);
						 var result = Trade.UpdatePendingPosition(posGuidSell, 0.1, pr1, SL2, null); 
						if (result.IsSuccessful)  posGuidSell = result.Position.Id; }
			}
			}	
		if(posGuidBuy!=Guid.Empty &&  Trade.GetPosition(posGuidBuy).Pips>50) tr=true;
		if(posGuidSell!=Guid.Empty &&  Trade.GetPosition(posGuidSell).Pips>50) tr=true;
			if(tr)  TrailActiveOrders();			
			
        }
			  //===== Функции =================================================================================================================   
		protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { 
			  var tr = Trade.UpdateMarketPosition(posGuidBuy,	  getSL(1),null,DTime.ToString()); 
		  
		  }
		  if(posGuidSell!=Guid.Empty) { 
			  var tr = Trade.UpdateMarketPosition(posGuidSell,  getSL(0),null,DTime.ToString()); 
		  }
		} 		  
			
	    protected double getSLfr(int type)
	    {
			switch(type)
			{   case 0: { return Math.Round(frUp+dlt+Instrument.Spread, Instrument.PriceScale); 
						    Print("{0} frUp={1}-{2}",DTime,frUp);
				var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-frac-2].Time; vl1.Color=Color.Red;
			            }
				case 1: { return Math.Round(frDown-dlt-Instrument.Spread, Instrument.PriceScale); 
										    Print("{0} frUp={1}-{2}",DTime,frDown);
				var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-frac-2].Time; vl1.Color=Color.Blue;  }
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
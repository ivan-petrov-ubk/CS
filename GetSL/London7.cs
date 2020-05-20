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
    [TradeSystem("London7")]   
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("StopLoss:", DefaultValue = 250)]
        public int SL1 { get; set; }		
		

		[Parameter("Buy: 7:00", DefaultValue = false)]
        public bool  Buy7 { get; set; }		
		[Parameter("Sell: 7:00", DefaultValue = false)]
        public bool  Sell7 { get; set; }		
	
		[Parameter("Buy: 12:00", DefaultValue = false)]
        public bool  Buy12 { get; set; }		
		[Parameter("Sell: 12:00", DefaultValue = false)]
        public bool  Sell12 { get; set; }			
		
		[Parameter("Fractal :", DefaultValue = 7, MinValue = 2, MaxValue = 200)]
		public int frac { get;set; }	
		
		private Guid posGuidBuy7=Guid.Empty;
		private Guid posGuidSell7=Guid.Empty;
		private Guid posGuidBuy12=Guid.Empty;
		private Guid posGuidSell12=Guid.Empty;		
		public DateTime DTime,zzt1,zzt2,Upt; // Время
		private bool Down1,u1,torg,frTop,torg7,torg12;
		private int k7,k12;
		private double frUp,frDown,frU1,frU2,frD1,frD2,sr1;

		private double zz1=2,zz2=2,zz3=2,sl1,sl2;
		
	
		public static int ci = 0;
		
        protected override void Init()
        {

			k7=0;k12=0;
			torg=false;
        }        
//===============================================================================================================================
        protected override void NewBar()
        {

			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
			if ( DTime.Hour<12 ) { torg12=true; torg12=true;  } 

//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy7!=Guid.Empty && Trade.GetPosition(posGuidBuy7).State==PositionState.Closed) { posGuidBuy7=Guid.Empty; k7=0; }  
		    if (posGuidSell7!=Guid.Empty && Trade.GetPosition(posGuidSell7).State==PositionState.Closed) { posGuidSell7=Guid.Empty; k7=0; }  
			if (posGuidBuy12!=Guid.Empty && Trade.GetPosition(posGuidBuy12).State==PositionState.Closed) { posGuidBuy12=Guid.Empty;   k12=0; } 
		    if (posGuidSell12!=Guid.Empty && Trade.GetPosition(posGuidSell12).State==PositionState.Closed) { posGuidSell12=Guid.Empty;  k12=0; }  			
//=== Трелинг  ===========================================================================================================	
			if(k12>=7) TrailActiveOrders();
	 
//=====  Рисуем вертикальную линию в 7:00 =========================================================================================
			if ( DTime.Hour==12 && DTime.Minute==15 ) 
			{  k12=0;  var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Aqua; vl1.Width=3;	}
//====================
			if ( DTime.Hour>=7 )  k7++;
			if ( DTime.Hour>=12 )  k12++;
			
			// Определение тренда по свечкам
			if ( DTime.Hour==12 && DTime.Minute==00 ) 
			{  var vl2 = Tools.Create<VerticalLine>(); vl2.Time=Bars[Bars.Range.To-1].Time; vl2.Color=Color.DarkGray;	
			 //sr1=(Bars[Bars.Range.To-2].Close+Bars[Bars.Range.To-3].Close+Bars[Bars.Range.To-4].Close)/3;
				if(Bars[Bars.Range.To-2].Open>Bars[Bars.Range.To].Close) { Buy12=true; Sell12=false; 
					var toolArrowUp = Tools.Create<ArrowUp>();
						toolArrowUp.Color=Color.Red;
          				toolArrowUp.Point=new ChartPoint(Bars[Bars.Range.To-3].Time, Bars[Bars.Range.To-1].High+0.0006);
}
				
				if(Bars[Bars.Range.To-2].Open<Bars[Bars.Range.To].Close) { Buy12=false; Sell12=true; 
					var toolArrowDown = Tools.Create<ArrowDown>();
						toolArrowDown.Color=Color.Blue;
          				toolArrowDown.Point=new ChartPoint(Bars[Bars.Range.To-3].Time, Bars[Bars.Range.To-1].Low-0.0006);
}
			}
			
//====================================================================================================================================
			if ( DTime.Hour==12 ) 
               {   
					if (posGuidBuy12==Guid.Empty && Buy12 && torg12) { torg12=false;						
						var vl2 = Tools.Create<VerticalLine>(); vl2.Time=Upt; vl2.Color=Color.Red;						
							var result101 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
												Stops.InPips(SL1,null), null, null);
							if (result101.IsSuccessful)  posGuidBuy12=result101.Position.Id;
						} 
			   			
					if (posGuidSell12==Guid.Empty && Sell12 && torg12) { torg12=false;						
						var vl2 = Tools.Create<VerticalLine>(); vl2.Time=Upt; vl2.Color=Color.Blue;
							 var result201 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                   Stops.InPips(SL1,null),null,  null); 
						if (result201.IsSuccessful)  posGuidSell12=result201.Position.Id;
						}
			   }
/*			   if ( DTime.Hour==12 ) 
			   {
				if (posGuidBuy!=Guid.Empty) 
				   {var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) { posGuidBuy = Guid.Empty;}}
				if (posGuidSell!=Guid.Empty) 
  				   { var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) {posGuidSell = Guid.Empty;} }
			   }
			*/   
        }
//===============================================================================================================================   
		protected void TrailActiveOrders()
		{		
		  if(posGuidBuy12!=Guid.Empty && k12>=7)  { var tr = Trade.UpdateMarketPosition(posGuidBuy12,	getSL(1),null," - update TP,SL"); }
		  if(posGuidSell12!=Guid.Empty && k12>=7) { var tr = Trade.UpdateMarketPosition(posGuidSell12,getSL(0),null," - update TP,SL"); } 
		  
		  if(posGuidBuy7!=Guid.Empty && k7>=7)  { var tr = Trade.UpdateMarketPosition(posGuidBuy7,	getSL(1),null," - update TP,SL"); }
		  if(posGuidSell7!=Guid.Empty && k7>=7) { var tr = Trade.UpdateMarketPosition(posGuidSell7,  getSL(0),null," - update TP,SL"); } 		  
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
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
    [TradeSystem("London6")]      //copy of "London5"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("ExtDepth:", DefaultValue = 5)]
        public int ED { get; set; }		
		
		[Parameter("Fractal :", DefaultValue = 7, MinValue = 2, MaxValue = 200)]
		public int frac { get;set; }		
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public DateTime DTime,zzt1,zzt2,Upt; // Время
		private bool Up1,Down1,u1,torg,frTop,torgD,torgU,StartTR;
		private int k;
		private double frUp,frDown,frU1,frU2,frD1,frD2;

		private double zz1=2,zz2=2,zz3=2,sl1,sl2;
		

		public Fractals _frInd;		
		private ZigZag _wprInd;
		
		public static int ci = 0;
		
        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);

			torg=false;
        }        
//===============================================================================================================================
        protected override void NewBar()
        {
			_wprInd.ReInit();
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
if ( DTime.Hour<7 ) { torgD=true; torgU=true; torg=true; StartTR=false; } 

	if ( DTime.Hour>=6 && DTime.Hour<8)  Print("ДО     - {0}  Up1={1} torgU={2} torgD={3} torg={4}",DTime,Up1,torgU,torgD,torg);	
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  

			
//====== BUY  UPDATE STOP  при появлении фрактала =======================================================================================================
			 // if(posGuidBuy!=Guid.Empty  && _frInd.BottomSeries[Bars.Range.To-5]>0) StartTR=true;
//===SELL UPDATE STOP   при появлении фрактала ===========================================================================================================							 
  			// if(posGuidSell!=Guid.Empty && _frInd.TopSeries[Bars.Range.To-5]>0) StartTR=true;
			if( posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).Pips>30 ) StartTR=true;
			if( posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).Pips>30 ) StartTR=true;
			if(StartTR)TrailActiveOrders();
//=== События ===================================================================================================================
			
			//=====  ZigZag  ======================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0 && DTime.Hour<7) 
			{   zz3=zz2; zz2=zz1; zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				zzt2=zzt1; zzt1=Bars[Bars.Range.To-1].Time;
				if(zz3<zz2 && zz2>zz1)  { Up1=true; Upt=zzt2;  } 
				if(zz3>zz2 && zz2<zz1)  { Up1=false; Upt=zzt2; } 	
			}
			 
			// Рисуем вертикальную линию в 7:00
			if ( DTime.Hour==7 && DTime.Minute==00 ) 
			{   var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Aqua; vl1.Width=3;	}
			
//====================================================================================================================================
			if ( DTime.Hour>=7 ) 
               {    
					if (posGuidBuy==Guid.Empty && Up1 && torg) { torg=false;
						Print("Buy - Upt={0}",Upt);
						var vl2 = Tools.Create<VerticalLine>(); vl2.Time=Upt; vl2.Color=Color.Red;						
							var result10 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
												Stops.InPips(200,null), null, null);
							if (result10.IsSuccessful)  posGuidBuy=result10.Position.Id;
						} 
			   			
					if (posGuidSell==Guid.Empty && !Up1 && torg) { torg=false;
						Print("SELL - Upt={0}",Upt);
						var vl2 = Tools.Create<VerticalLine>(); vl2.Time=Upt; vl2.Color=Color.Blue;
							 var result20 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                   Stops.InPips(200,null),null,  null); 
						if (result20.IsSuccessful)  posGuidSell=result20.Position.Id;
						}
			   }
        }
//===============================================================================================================================   
		protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { var tr = Trade.UpdateMarketPosition(posGuidBuy,	getSL(1),null," - update TP,SL"); }
		  if(posGuidSell!=Guid.Empty) { var tr = Trade.UpdateMarketPosition(posGuidSell,getSL(0),null," - update TP,SL"); } 
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
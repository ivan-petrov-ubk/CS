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
    [TradeSystem("London3")]   //copy of "London2"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("ExtDepth:", DefaultValue = 15)]
        public int ED { get; set; }		
		
		[Parameter("SL :", DefaultValue = 200)]
        public double SL { get; set; }	
		
		[Parameter("Fractal :", DefaultValue = 7, MinValue = 2, MaxValue = 200)]
		public int frac { get;set; }		
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public DateTime DTime,zzt1,zzt2,Upt; // Время
		private bool Up1,Down1,u1,torg,frTop;
		private int k;
		private double frUp,frDown,frU1,frU2,frD1,frD2;

		private double zz1=2,zz2=2,zz3=2,sl1,sl2;
		
		public FisherTransformOscillator _ftoInd;
		public Fractals _frInd;		
		private ZigZag _wprInd;
		
		public static int ci = 0;
		
        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			_ftoInd = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			
        }        
//===============================================================================================================================
        protected override void NewBar()
        {
			_wprInd.ReInit();
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
			
			if(_frInd.BottomSeries[Bars.Range.To-5]>0) { frTop=false; frD2=frD1; frD1=_frInd.BottomSeries[Bars.Range.To-5]; }
			if(_frInd.TopSeries[Bars.Range.To-5]>0) { frTop=true; frU2=frU1; frU1=_frInd.TopSeries[Bars.Range.To-5]; }			
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  

//====== BUY  UPDATE STOP  при появлении фрактала =======================================================================================================
			  if(posGuidBuy!=Guid.Empty  && _frInd.BottomSeries[Bars.Range.To-5]>0 && frD1>frD2) { 
				  sl1 = Math.Round(_frInd.BottomSeries[Bars.Range.To-5]-Instrument.Spread-0.00020, Instrument.PriceScale);
					if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active)  { 
				         var res3=Trade.UpdateMarketPosition(posGuidBuy, sl1, null, null); 
				         if (res3.IsSuccessful) { posGuidBuy=res3.Position.Id; }
				       else  { var res2 = Trade.CloseMarketPosition(posGuidBuy); if (res2.IsSuccessful) posGuidBuy = Guid.Empty; }}
					 }
//===SELL UPDATE STOP   при появлении фрактала ===========================================================================================================							 
			if(posGuidSell!=Guid.Empty && _frInd.TopSeries[Bars.Range.To-5]>0 && frU1<frU2) { 
				sl2 = Math.Round(_frInd.TopSeries[Bars.Range.To-5]+Instrument.Spread+0.00020, Instrument.PriceScale);
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) { 
				 var result2=Trade.UpdateMarketPosition(posGuidSell, sl2, null, null); 
				if (result2.IsSuccessful) { posGuidSell=result2.Position.Id; }  
			  else { var res0 = Trade.CloseMarketPosition(posGuidSell); if (res0.IsSuccessful) posGuidSell = Guid.Empty;}}
			}	


//====================================================================================================================================
//====== BUY  UPDATE STOP  при профит стал больше 100п  ====================================================================================
/*			    
			  if(posGuidBuy!=Guid.Empty  && Trade.GetPosition(posGuidBuy).Pips>100)  { 
				sl1=Math.Round(Trade.GetPosition(posGuidBuy).OpenPrice+Instrument.Spread+0.00020, Instrument.PriceScale);  
					if (posGuidBuy!=Guid.Empty && 
						Trade.GetPosition(posGuidBuy).State==PositionState.Active &&
						Trade.GetPosition(posGuidBuy).StopLoss<sl1)  { 
				         var res3=Trade.UpdateMarketPosition(posGuidBuy, sl1, null, null); 
				         if (res3.IsSuccessful) { posGuidBuy=res3.Position.Id; }
				       else  { var res2 = Trade.CloseMarketPosition(posGuidBuy); if (res2.IsSuccessful) posGuidBuy = Guid.Empty; }}
					 }
*/
			//===SELL UPDATE STOP    при профит стал больше 100п  =============================================================================
/*			  
			if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).Pips>100) { 
			sl2=Math.Round(Trade.GetPosition(posGuidBuy).OpenPrice-Instrument.Spread-0.00020, Instrument.PriceScale);
				if (posGuidSell!=Guid.Empty && 
					Trade.GetPosition(posGuidSell).State==PositionState.Active && 
					Trade.GetPosition(posGuidSell).StopLoss>sl2) { 
				 var result2=Trade.UpdateMarketPosition(posGuidSell, sl2, null, null); 
				if (result2.IsSuccessful) { posGuidSell=result2.Position.Id; }  
			  else { var res0 = Trade.CloseMarketPosition(posGuidSell); if (res0.IsSuccessful) posGuidSell = Guid.Empty;}}
			}		
*/			
//====================================================================================================================================
			if ( DTime.Hour<7)  torg=true; 
			if ( DTime.Hour==7 && DTime.Minute==00 ) 
			{
			    var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Aqua; vl1.Width=3;		
			}
			
			//=====  ZigZag  =================================================================================================================================
//Print("{0} - {1}",Bars[Bars.Range.To-1].Time,_wprInd.MainIndicatorSeries[Bars.Range.To-1]);
			
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{   zz3=zz2; zz2=zz1; zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				zzt2=zzt1; zzt1=Bars[Bars.Range.To-1].Time;
				if(zz3<zz2 && zz2>zz1)  { Up1=true; Upt=zzt2;  } 
				if(zz3>zz2 && zz2<zz1)  { Up1=false; Upt=zzt2; } 	
			}
				
			if ( DTime.Hour==7) 
               {    
					if (posGuidBuy==Guid.Empty && Up1 && torg) { torg=false; 
						frDown= Math.Round(_frInd.BottomSeries[Bars.Range.To-5], Instrument.PriceScale);
						Print("{0} Upt={1}",DTime,Upt);
						var vl2 = Tools.Create<VerticalLine>(); vl2.Time=Upt; vl2.Color=Color.Red;
						//var vl4 = Tools.Create<VerticalLine>(); vl4.Time=Bars[Bars.Range.To-1].Time; vl4.Color=Color.White;	
						 sl1=Instrument.Ask-(SL*Instrument.Point);
						if(sl1<frDown) sl1=frDown;
						
						 var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
												Stops.InPrice(sl1,null), null, null); 
						if (result1.IsSuccessful)  posGuidBuy=result1.Position.Id; else 
						{
							var result10 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
												Stops.InPips(200,null), null, null);
							if (result10.IsSuccessful)  posGuidBuy=result10.Position.Id;
						}  }
			   			
					if (posGuidSell==Guid.Empty && !Up1 && torg) { torg=false;
						Print("{0} Upt={1}",DTime,Upt);
						var vl3 = Tools.Create<VerticalLine>(); vl3.Time=Upt; vl3.Color=Color.Blue;	
						//var vl4 = Tools.Create<VerticalLine>(); vl4.Time=Bars[Bars.Range.To-1].Time; vl4.Color=Color.White;
						frUp= Math.Round(_frInd.TopSeries[Bars.Range.To-5], Instrument.PriceScale);
						sl2=Instrument.Ask+(SL*Instrument.Point);
						if(sl2>frUp) sl2=frUp;
						      var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                   Stops.InPrice(sl2,null),null,  null); 
						if (result2.IsSuccessful)  posGuidSell=result2.Position.Id; else 
						{
							 var result20 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                   Stops.InPips(200,null),null,  null); 
						if (result20.IsSuccessful)  posGuidSell=result20.Position.Id;
						}}
			   }
				 				if( Trade.GetPosition(posGuidBuy).Pips>100 ) TrailActiveOrders1();		
					if( Trade.GetPosition(posGuidSell).Pips>100 ) TrailActiveOrders1();	
			  
        }
		
		protected void TrailActiveOrders1()
		{		
		  if(posGuidBuy!=Guid.Empty)  { var tr = Trade.UpdateMarketPosition(posGuidBuy,	getSL1(1),null," - update TP,SL"); }
		  if(posGuidSell!=Guid.Empty) { var tr = Trade.UpdateMarketPosition(posGuidSell,getSL1(0),null," - update TP,SL"); } 
		}		  
		
//===============================================================================================================================   
		protected double getSL(int type)
		{ 	switch(type)
			{
				case 0:  return Math.Round(Trade.GetPosition(posGuidBuy).OpenPrice+Instrument.Spread, Instrument.PriceScale);
				case 1:	 return Math.Round(Trade.GetPosition(posGuidSell).OpenPrice-Instrument.Spread, Instrument.PriceScale);
				default: break;
			}
			return 0.0;
		}
		
		protected double getSL1(int type)
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
		//===============================================================================================================================   
    }
}
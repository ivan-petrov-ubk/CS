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
    [TradeSystem("London2")]  //copy of "NKZ_ZZ"
    public class ZZ_Ex1 : TradeSystem
    {
//		[Parameter("ExtDepth:", DefaultValue = 15)]
//        public int ED { get; set; }		
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public DateTime DTime; // Время
		private bool Up1,Down1,u1;
		private int k;

		private double zz1=2,zz2=2,zz3=2;
		
		//public FisherTransformOscillator _ftoInd;
		public Fractals _frInd;		
		// private ZigZag _wprInd;
		
        protected override void Init()
        {
//			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
//			_wprInd.ExtDepth=ED;
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
//			_ftoInd = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			
        }        
//===============================================================================================================================
        protected override void NewBar()
        {
// 			_wprInd.ReInit(); k++;
			DTime = Bars[Bars.Range.To-1].Time;
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
//=====  ZigZag  =================================================================================================================================
/*			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{   zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				if(zz3<zz2 && zz2>zz1)  Up1=true; 
				if(zz3>zz2 && zz2<zz1)  Up1=false;  }
*/			
//=====  Fractal =================================================================================================================================
//				if (_frInd.TopSeries[Bars.Range.To-5]>0)  Up1=true; 
//				if (_frInd.BottomSeries[Bars.Range.To-5]>0)  Up1=false;
//=====  FFisher  =================================================================================================================================
//                if (_ftoInd.FisherSeries[Bars.Range.To-2]<_ftoInd.FisherSeries[Bars.Range.To-1])  Up1=true; 
//				if (_ftoInd.FisherSeries[Bars.Range.To-2]>_ftoInd.FisherSeries[Bars.Range.To-1])  Up1=false;
///======================================================================================================================================			
			
			//if ( DTime.Hour==6 && DTime.Minute==30 )  u1=Up1; 
				
			if ( DTime.Hour==7 && DTime.Minute==00 ) 
               {    
				   var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Aqua; vl1.Width=2;	
				   
				  // if(!Up1) {
				    if (posGuidBuy==Guid.Empty && _frInd.BottomSeries[Bars.Range.To-5]>0) { var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
												null, null, null);
												if (result1.IsSuccessful)  {posGuidBuy=result1.Position.Id; k=0; }}
			   			}  else  {
					if (posGuidSell==Guid.Empty && _frInd.BottomSeries[Bars.Range.To-5]>0) {var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 							null,null,  null); 
												if (result2.IsSuccessful)  { posGuidSell=result2.Position.Id; k=0; }}
						//}
			   }
			   
			   //if (posGuidBuy!=Guid.Empty || posGuidSell!=Guid.Empty) Print("Profit-{0} Pips={1} ",Trade.GetPosition(posGuidBuy).Profit,Trade.GetPosition(posGuidBuy).Pips);
			   
//			  if ( DTime.Hour==7 && DTime.Minute==00 ) 
//			   {
//				if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).Pips>50) 
//				   {var tr = Trade.UpdateMarketPosition(posGuidBuy,	getSL(0),null," - update TP,SL");if (tr.IsSuccessful)  posGuidBuy=tr.Position.Id;}
//				if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).Pips>50) 
//  				   { var tr = Trade.UpdateMarketPosition(posGuidSell,getSL(1),null," - update TP,SL"); if (tr.IsSuccessful)  posGuidSell=tr.Position.Id;}
//   }
			   
			   if ( DTime.Hour==8 && DTime.Minute==15 ) 
			   {
				if (posGuidBuy!=Guid.Empty) 
				   {var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) { posGuidBuy = Guid.Empty;}}
				if (posGuidSell!=Guid.Empty) 
  				   { var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) {posGuidSell = Guid.Empty;} }
			   }
			
//			  if (posGuidBuy!=Guid.Empty) Print("{0} - Up1={1} Pips={2}",Bars[Bars.Range.To-1].Time,Up1,Trade.GetPosition(posGuidBuy).Pips);
//			  if (posGuidSell!=Guid.Empty) Print("{0} - Up1={1} Pips={2}",Bars[Bars.Range.To-1].Time,Up1,Trade.GetPosition(posGuidSell).Pips);
			  
			   
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
		
    }
}
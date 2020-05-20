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
    [TradeSystem("ZigZag5")]   //copy of "ZigZag4"
    public class ZigZag2 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "TC ZigZag")]
        public string CommentText { get; set; }
		[Parameter("TakeProfit=", DefaultValue = 100)]
        public int TP { get; set; }
		[Parameter("StopLoss=", DefaultValue = 200)]
        public int SL { get; set; }
		[Parameter("Spred=", DefaultValue = 0)]
        public int SP { get; set; }	
		[Parameter("ExtDepth=", DefaultValue = 12)]
        public int ED { get; set; }		
		
		private ZigZag _wprInd;
		private double zz1,zz2,zz3,zzU,zzD;
		private int zzd1,zzd2,zzd3;
		private int zzi1,zzi2,zzi3;
		private bool up;
		        private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double kor;
		public Fractals _frInd;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
			if(Instrument.Name.EndsWith("JPY")) kor=0.001*SP; else kor=0.00001*SP;		
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
			//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
            // Event occurs on every new bar
			//Print("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}| - {13}", _wprInd.MainIndicatorSeries[Bars.Range.To-1],_wprInd.MainIndicatorSeries[Bars.Range.To-2],_wprInd.MainIndicatorSeries[Bars.Range.To-3],_wprInd.MainIndicatorSeries[Bars.Range.To-4],_wprInd.MainIndicatorSeries[Bars.Range.To-5],_wprInd.MainIndicatorSeries[Bars.Range.To-6],_wprInd.MainIndicatorSeries[Bars.Range.To-7],_wprInd.MainIndicatorSeries[Bars.Range.To-8],_wprInd.MainIndicatorSeries[Bars.Range.To-9],_wprInd.MainIndicatorSeries[Bars.Range.To-10],_wprInd.MainIndicatorSeries[Bars.Range.To-11],_wprInd.MainIndicatorSeries[Bars.Range.To-12],_wprInd.MainIndicatorSeries[Bars.Range.To-13], Bars[Bars.Range.To-1].Time);
			//Print("ZigZag : {0} - {1} -  {2}", _wprInd.MainIndicatorSeries[Bars.Range.To-2],Bars[Bars.Range.To-2].Time,Bars.Range.To-2);
			_wprInd.ReInit();
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-2]>0) 
			{ // Всі точки перегину зиззага
				
				 zz3=zz2;	 zz2=zz1;    zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-2];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1=Bars.Range.To-2;
				Print("{0} {1} {2} -- {3}",zz3,zz2,zz1, Bars[Bars.Range.To-2].Time);
				/*	
				   var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-2].Time;
					toolVerticalLine.Color=Color.Yellow;
				*/
				
         		if(Bars[zzd2].Low>Bars[zzi1].Low &&  Bars[zzd2].High<Bars[zzd1].High && Bars[zzi1].High<Bars[zzd1].High) 
				{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[zzd1].Time;
					toolVerticalLine.Color=Color.Red;   
								if(posGuidSell==Guid.Empty) {
			   var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  Bars[zzd1].High-Instrument.Spread-kor, 0, Stops.InPips(SL,TP), null, null, null);
						 if (result1.IsSuccessful)  posGuidSell=result1.Position.Id; 				
				} 
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending)
				{
					var res = Trade.CancelPendingPosition(posGuidSell);
			     	   							if (res.IsSuccessful) { posGuidSell = Guid.Empty; 
					var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  Bars[zzd1].High-Instrument.Spread-kor, 0, Stops.InPips(SL,TP), null, null, null);
						 if (result1.IsSuccessful)  posGuidSell=result1.Position.Id; 												
												}
													
				}
					}
			
				if(Bars[zzd2].High<Bars[zzi1].High &&  Bars[zzd2].Low>Bars[zzd1].Low && Bars[zzi1].Low>Bars[zzd1].Low) 
					{
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[zzd2].Time;
					toolVerticalLine.Color=Color.Blue;
									    if(posGuidBuy==Guid.Empty) {
				var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, Bars[zzd1].Low+Instrument.Spread+kor, 0, Stops.InPips(SL,TP), null, null, null);
						 if (result.IsSuccessful)  posGuidBuy=result.Position.Id; 
				}
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending)
				{
					var res = Trade.CancelPendingPosition(posGuidBuy);
			     	   							if (res.IsSuccessful) { posGuidBuy = Guid.Empty; 
					var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1,  Bars[zzd1].Low+Instrument.Spread+kor, 0, Stops.InPips(SL,TP), null, null, null);
						 if (result1.IsSuccessful)  posGuidBuy=result1.Position.Id; 												
												}
													
				}
					} 
					
//if(Bars[zzd1].Close>_wprInd.MainIndicatorSeries[zzd1] && zz3>zz2 && zz2<zz1 && up)  { 
if(zz3>zz2 && zz2<zz1 && up)  { 				
	up=false; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; zzU=_wprInd.MainIndicatorSeries[zzd1];
/*				    var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[zzi2].Time;
					toolVerticalLine.Color=Color.Red;  
*/	
//Print("zzU={0} Bars[zzd1]={1} -- {2} -- {3}",zzU,Bars[zzd1].Close,Bars[zzd1].Time,Bars[zzd1].Close<zzU);
}
			
//if(Bars[zzd1].Close<_wprInd.MainIndicatorSeries[zzd1] && zz3<zz2 && zz2>zz1 && !up) { 
if(zz3<zz2 && zz2>zz1 && !up) { 			
	up=true;  zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; zzD=_wprInd.MainIndicatorSeries[zzd1];
/*					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[zzi2].Time;
					toolVerticalLine.Color=Color.Blue;  
*/
//Print("zzD={0} Bars[zzd1]={1} -- {2} -- {3}",zzD,Bars[zzd1].Close,Bars[zzd1].Time,Bars[zzd1].Close>zzD);
	
}	
			
			}
			//Print("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}| - {13}", _wprInd.MainIndicatorSeries[Bars.Range.To-1],_wprInd.MainIndicatorSeries[Bars.Range.To-2],_wprInd.MainIndicatorSeries[Bars.Range.To-3],_wprInd.MainIndicatorSeries[Bars.Range.To-4],_wprInd.MainIndicatorSeries[Bars.Range.To-5],_wprInd.MainIndicatorSeries[Bars.Range.To-6],_wprInd.MainIndicatorSeries[Bars.Range.To-7],_wprInd.MainIndicatorSeries[Bars.Range.To-8],_wprInd.MainIndicatorSeries[Bars.Range.To-9],_wprInd.MainIndicatorSeries[Bars.Range.To-10],_wprInd.MainIndicatorSeries[Bars.Range.To-11],_wprInd.MainIndicatorSeries[Bars.Range.To-12],_wprInd.MainIndicatorSeries[Bars.Range.To-13], Bars[Bars.Range.To-1].Time);
        }
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
    }
}
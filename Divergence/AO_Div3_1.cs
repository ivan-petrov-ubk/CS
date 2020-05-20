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
    [TradeSystem("AO_Div3")]
    public class AO_Div3 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		
		public AwesomeOscillator _awoInd;
		private RelativeStrenghtIndex _rsiInd;
		private double aoUp1,aoDown1,aoUp2,aoDown2,aoUp3,aoDown3,aoUp4,aoDown4,aoUp5,aoDown5;
		private double zz1=2,zz2=2,rsi1,rsh1,rsh2,rsl1,rsl2;
		public int dv=0;
		public bool div=false,aoh0=false,aol0=false;
		private double aol1=2,aoh1=2,aol2=2,aoh2=2,bh1=2,bh2=2,bl1=2,bl2=2;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;

		
        protected override void Init()
        {
					
						_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);
						_rsiInd = GetIndicator<RelativeStrenghtIndex>(Instrument.Id, Timeframe);
        }        

        
        protected override void NewBar()
        {
            //=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed)   posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			// Event occurs on every new bar
			_rsiInd.ReInit();
			_awoInd.ReInit();
			rsi1=_rsiInd.SeriesRsi[Bars.Range.To-1];
			aoUp1    = _awoInd.SeriesUp[Bars.Range.To-1];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown1  = _awoInd.SeriesDown[Bars.Range.To-1]; // Червоні лінії
			aoUp2    = _awoInd.SeriesUp[Bars.Range.To-2];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown2  = _awoInd.SeriesDown[Bars.Range.To-2];
			aoUp3    = _awoInd.SeriesUp[Bars.Range.To-3];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown3  = _awoInd.SeriesDown[Bars.Range.To-3];
			aoUp4    = _awoInd.SeriesUp[Bars.Range.To-4];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown4  = _awoInd.SeriesDown[Bars.Range.To-4];
			aoUp5    = _awoInd.SeriesUp[Bars.Range.To-5];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown5  = _awoInd.SeriesDown[Bars.Range.To-5];
			
			// Пересечение 0 Снизу ВВЕРХ - зеленые столбики	 
				 if (aoUp2<=0 && aoUp1>=0 && div)     { dv=0; aoh0=true;  div=false; }				 
				 
			
 			// Пересечение 0 Сверху вниз красные столбики	
			     if (aoDown2>=0 && aoDown1<=0 && div)  { dv=0; aol0=true; div=false; } 
//=====================================			
			 			// Пересечение 0 Сверху вниз красные столбики	
			     if (aoDown2>=-0.0002 && aoDown1<=-0.0002)  { 
				 if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}						 
				 if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
				 } 	 
			// Пересечение 0 Снизу ВВЕРХ - зеленые столбики	 
				 if (aoUp2<=0.0002 && aoUp1>=0.0002)     { 
				 if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
				 if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}				 
				 }				 
				 
			// Вверху ГОРБ Красний на Зеленый
			 	 if(aoUp5>0.0002 && aoUp4>0.0002 && aoUp3>0.0002 && aoDown2>0.0002 && aoDown1>0.0002) 
     				{  	dv++;div=false;
						bh2=bh1;  bh1= Bars[Bars.Range.To-3].High;
						aoh2=aoh1;  aoh1 = 	rsi1;
						rsh2=rsh1;  rsh1=_rsiInd.SeriesRsi[Bars.Range.To-3];
						Print("ВВЕРХУ  {0}   {1} {2} {3} {4} -- {5} {6} - {7}",Bars[Bars.Range.To-3].Time,bh1,bh2,Math.Round(aoh1,5),Math.Round(aoh2,5),bh1>bh2,aoh2>aoh1,dv);
						if((bh1>bh2 || Bars[Bars.Range.To-2].High>bh2 || Bars[Bars.Range.To-1].High>bh2 ) && 
							rsh2>=rsh1 && aoh2>=aoh1 && dv>1) div=true;
					}
					
			// // Вверху ВПАДИНА Зеленый на Красний
			 	 if(aoDown5>0.0002 && aoDown4>0.0002 && aoDown3>0.0002 && aoUp2>0.0002 && aoUp1>0.0002) 
     				{  if(aoh0 && posGuidBuy==Guid.Empty) {	
						var result1 = Trade.OpenMarketPosition(Instrument.Id, 
							    						       ExecutionRule.Buy, 
							 									0.1,Instrument.Bid, -1,
															null, null, null);
																 //Stops.InPips(200,100), null, null);					
						if (result1.IsSuccessful) { posGuidBuy=result1.Position.Id;   } 
					
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
					toolVerticalLine.Color=Color.Blue; toolVerticalLine.Width=4; }
					aoh0=false;
					}
					
			// ВНИЗУ ГОРБ Красний на Зеленый
			 	if(aoDown5<-0.0002 &&  aoDown4<-0.0002 && aoDown3<-0.0002 && aoUp2<-0.0002 && aoUp1<-0.0002 ) 
     				{   div=false;
						dv++;
						bl2=bl1;  bl1= Bars[Bars.Range.To-3].Low;
					    aol2=aol1;  aol1=aoDown3;
						rsl2=rsl1;  rsl1=_rsiInd.SeriesRsi[Bars.Range.To-3];
						Print("ВНИЗУ  {0}   {1} {2} {3} {4} -- {5} {6} - {7}",Bars[Bars.Range.To-3].Time,bl1,bl2,Math.Round(aol1,5),Math.Round(aol2,5),bl1<bl2,aol2<aol1,dv);
						if((bl1<bl2 || Bars[Bars.Range.To-2].Low<bl2 || Bars[Bars.Range.To-1].Low<bl2 ) && 
							rsl2<=rsl1  && aol2<=aol1 && dv>1) div=true; 	
					}
					
			// // ВНИЗУ ВПАДИНА Зеленый на Красний
			 	if(aoUp4<-0.0002 && aoUp3<-0.0002 && aoDown2<-0.0002 && aoDown1<-0.0002) 
     				{  if(aol0 && posGuidSell==Guid.Empty) {	
						var result1 = Trade.OpenMarketPosition(Instrument.Id, 
							    						       ExecutionRule.Sell, 
							 									0.1,Instrument.Ask, -1,
							null, null, null);
							 				//					 Stops.InPips(200,100), null, null);					
						if (result1.IsSuccessful) { posGuidSell=result1.Position.Id;   } 
							var toolVerticalLine=Tools.Create<VerticalLine>();
     						toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
							toolVerticalLine.Color=Color.Red; toolVerticalLine.Width=4; }
					aol0=false;
					}

			/*	var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				toolVerticalLine.Color=Color.Red;  */
					
			
        }
        
    }
}
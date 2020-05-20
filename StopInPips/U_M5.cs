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
    [TradeSystem("U_M5")]  //copy of "ТС Пробой M5"
    public class ТС_Пробой_Н1 : TradeSystem
    {   public double H1,L1,O1,C1;
		public double U1,UH,UL,UP;
		public HorizontalLine Line1,Line2,Line3;
		public DateTime DTime; // Время
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double[] UR = new double[20];
		private bool isU,isD,First,TU;
		
		
        protected override void Init()
        {	Line1 = Tools.Create<HorizontalLine>();
			Line2 = Tools.Create<HorizontalLine>();
			UH=1.19088; UL=1.11405;
			U1=(UH-UL)/20;
			First=true;
			UR[0]=UL;
			for (int i=1; i<20; i++)  { 
				UR[i]=UL+(U1*i);
				var vB = Tools.Create<HorizontalLine>();
			        vB.Price=UR[i];
			}
				
			}        

        protected override void NewBar()
        {   
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty; 
			
		    H1 = Bars[Bars.Range.To-1].High;
			L1 = Bars[Bars.Range.To-1].Low;
		    C1 = Bars[Bars.Range.To-1].Close;
			O1 = Bars[Bars.Range.To-1].Open;
			
			DTime = Bars[Bars.Range.To-1].Time;
			
			
			foreach (double urov in UR)
			{  if((H1+0.0002)>urov && (L1-0.0002)<urov) {
			    
				if(C1>O1 && UP!=urov) TU=true; else TU=false; 
					 
				// ВВЕРХ 
				if(TU  && UP!=urov)
				{

				if(posGuidBuy==Guid.Empty && posGuidSell==Guid.Empty) {
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Red;toolVerticalLine.Width=3;
				var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1, Stops.InPips(200,300), null, null);
				if (result3.IsSuccessful)  posGuidSell=result3.Position.Id;}
								
					
				}
			    // ВНИЗ 
				if(!TU  && UP!=urov)
				{   

				if(posGuidBuy==Guid.Empty && posGuidSell==Guid.Empty) {
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Blue;toolVerticalLine.Width=3;
				var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1, Stops.InPips(200,300), null, null);
				if (result3.IsSuccessful)  posGuidBuy=result3.Position.Id;}

				}
				UP=urov;
				}
					
			}
        }
    }
}
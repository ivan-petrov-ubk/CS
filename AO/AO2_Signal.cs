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
    [TradeSystem("AO2_1")]     //copy of "AO2_DivirgeceH1"
    public class AO : TradeSystem
    {
	double aoUp1,aoDown1,aoUp2,aoDown2;
	public int W1,W2,W3,R1,R2,R3,B1,B2,B3,Y1,Y2,Y3;	
	public AwesomeOscillator _awoInd;
	protected override void Init() {_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);}  

        protected override void NewBar()
        {
			aoUp1    = _awoInd.SeriesUp[Bars.Range.To-2];   // Зелені лінії - Вверху>0  Внизу<0
			aoDown1  = _awoInd.SeriesDown[Bars.Range.To-2]; // Червоні лінії
			aoUp2    = _awoInd.SeriesUp[Bars.Range.To-3];   // Зелені лінії - Вверху>0  Внизу<0
			aoDown2  = _awoInd.SeriesDown[Bars.Range.To-3];
			
if (aoUp2<0 && aoUp1>0)     { 
	B1=0; B2=0; B3=0; R2=0; R1=0; R3=0; W1=0; W2=0; W3=0; Y1=0; Y2=0; Y3=0; 

}

if (aoDown2>0 && aoDown1<0) { B1=0; B2=0; B3=0; R2=0; R1=0; R3=0; W1=0; W2=0; W3=0; Y1=0; Y2=0; Y3=0; 

}
			
			 if(aoUp1>0 && aoDown2>0)  {  //  Седло вверху
				B3=B2; B2=B1; B1=Bars.Range.To-1; 
			    var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-2].Time;
				toolVerticalLine.Color=Color.Blue;
				}
		
			 
			 if(aoDown1<0 && aoUp2<0)  {  //  Седло внизу
				W3=W2; W2=W1; W1=Bars.Range.To-1;  
			    var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-2].Time;
				toolVerticalLine.Color=Color.White;
			} 
			 
			if(aoUp1<0 && aoDown2<0)  { //  Горка внизу
				Y3=Y2; Y2=Y1; Y1=Bars.Range.To-1; 	
 //               if(W2-Y3>2 && Y2-W2>2 && W1-Y2>2 && Y1-W1>2) 
			if(_awoInd.SeriesUp[Y1]>_awoInd.SeriesUp[Y2] && Bars[Y2].Low>Bars[Y1].Low)	
			{
				var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-2].Time;
				toolVerticalLine.Color=Color.Yellow;}
			}  
			
			if(aoDown1>0 && aoUp2>0)  {  // Горка вверху
				R3=R2; R2=R1; R1=Bars.Range.To-1;
//				if(B2-R3>2 && R2-B2>2 && B1-R2>2 && R1-B1>2) 
		if(_awoInd.SeriesUp[R2]>_awoInd.SeriesUp[R1] && Bars[R1].High>Bars[R2].High)		
			{
			    var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-2].Time;
				toolVerticalLine.Color=Color.Red;}
			}  

			//Print("Up>{0} Down>{1} Up<{2} Down<{3} = {4}  {5}  --  {6}",aoUp>0,aoDown>0,aoUp<0,aoDown<0,Math.Round(aoUp,5),Math.Round(aoDown,5),Bars[Bars.Range.To-1].Time);
		}
 
    }
}
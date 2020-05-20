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
    [TradeSystem("AO2_DivirgeceH1")]    //copy of "AO2_Divirgece"
    public class AO : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		public Fractals _frInd;
		private Guid _positionGuidB=Guid.Empty;
		private Guid _positionGuidS=Guid.Empty;
		public AwesomeOscillator _awoInd;
		//public FisherTransformOscillator _ftoInd;
		double sF,sF1;
		double TP=0.0001;
		double TP1=-0.0001;
		double aoUp1,aoDown1,aoUp2,aoDown2,aoUp3,aoDown3,aoUp4,aoDown4,aoUp5,aoDown5;
		double DV,DN,UV,UN;
		double MaxU,MaxD,MaxU1,MaxD1,MaxU2,MaxD2,BarU1,BarU2,BarD1,BarD2;
		public double frUpH,frDownL,frU,frD,D1,D2,U1,U2;
		bool U0,D0,DivU=false,DivD=false;
		public bool frUp=false;
		public bool frDown=true;	
		private VerticalLine vlR,vlB;
		public bool Dp1,Dm1,Up1,Um1;
		public bool Dp2,Dm2,Up2,Um2;
		public int R1,B1,R2,W1,W2,Y1,indS,indB;


        protected override void Init() {_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe); }        

        protected override void NewBar()
        {
            // Event occurs on every new bar
			aoUp1    = _awoInd.SeriesUp[Bars.Range.To-1];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown1  = _awoInd.SeriesDown[Bars.Range.To-1]; // Червоні лінії
			aoUp2    = _awoInd.SeriesUp[Bars.Range.To-2];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown2  = _awoInd.SeriesDown[Bars.Range.To-2];
			aoUp3    = _awoInd.SeriesUp[Bars.Range.To-3];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown3  = _awoInd.SeriesDown[Bars.Range.To-3];
			aoUp4    = _awoInd.SeriesUp[Bars.Range.To-4];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown4  = _awoInd.SeriesDown[Bars.Range.To-4];
			
			Dp1=aoDown1>0;  Dm1=aoDown1<0; Up1=aoUp1>0; Um1=aoUp1<0;  
			Dp2=aoDown2>0;  Dm2=aoDown2<0; Up2=aoUp2>0; Um2=aoUp2<0;
// Print(" ---------------------------------------- UD|4 {0} {1} |3 {2} {3} |2 {4} {5} |1 {6} {7} | -- {8}",Math.Round(aoUp4,5),Math.Round(aoDown4,5),Math.Round(aoUp3,5),Math.Round(aoDown3,5),Math.Round(aoUp2,5),Math.Round(aoDown2,5),Math.Round(aoUp1,5),Math.Round(aoDown1,5),Bars[Bars.Range.To-1].Time);

// Print(" | {0} {1} | -- {2} -- | {3} {4} | {5} {6} | ",Math.Round(aoUp1,5),Math.Round(aoDown1,5),Bars[Bars.Range.To-1].Time,aoUp1>0,aoDown1>0,aoUp1<0,aoDown1<0);			
//=== КОРЕКЦИЯ ===========================================================================================================							 
//			if (_positionGuidB!=Guid.Empty && Trade.GetPosition(_positionGuidB).State==PositionState.Closed) 
//			     {	_positionGuidB=Guid.Empty;  }
//		    if (_positionGuidS!=Guid.Empty && Trade.GetPosition(_positionGuidS).State==PositionState.Closed) 
//				{   _positionGuidS=Guid.Empty;  }			
// =======================================================================================================================	
// if (aoUp2<0 && aoUp1>0)     { B1=0; R2=0; R1=0; W1=0; W2=0; Y1=0; }
 //if (aoDown2>0 && aoDown1<0) { B1=0; R2=0; R1=0; W1=0; W2=0; Y1=0; }
			
// 1   Горка вверху
//	 if(aoUp4>TP && aoUp3>TP && aoDown2>TP && aoDown1>TP)
 if(Up2 && Dp1)
     {  
//		        R2=R1; R1=Bars.Range.To-1; U2=U1; U1=aoUp3;	
		 //Print("--------------------------------------  1-(V(DU)) - D4={0} D3={1} U2={2} U1={3} -- {4}",Math.Round(aoDown4,5),Math.Round(aoDown3,5),Math.Round(aoUp2,5),Math.Round(aoUp1,5),Bars[Bars.Range.To-1].Time);

			 	var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				toolVerticalLine.Color=Color.Red;

/*		 if(B1-R2>2 && R1-B1>2 && U1<U2  && Bars[R2].High<Bars[R1].High) {
		 		//Print("1-(V(UD)) - U4={0} U3={1} D2={2} D1={3} -- {4}",Math.Round(aoUp4,5),Math.Round(aoUp3,5),Math.Round(aoDown2,5),Math.Round(aoDown1,5),Bars[Bars.Range.To-1].Time);
		        //Print("R2={0} B1={1} R1={2}  - {3}",R2,B1,R1,Bars[Bars.Range.To-1].Time);   
			 	if (_positionGuidS==Guid.Empty) {
		          var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell,  1.0, Instrument.Ask, -1, Stops.InPips(300,null), null, null);	
				  if (result.IsSuccessful)  _positionGuidS = result.Position.Id;   } 
		 }*/				
	 }
// 2   Сідло вверху 
//	 if(aoDown4>TP && aoDown3>TP && aoUp2>TP && aoUp1>TP) // Сідло вверху
 if(Dp2 && Up1) 
     {            
	//	 		B1=Bars.Range.To-1;
				//Print("--------------------------------------  2-(V(DU)) - D4={0} D3={1} U2={2} U1={3} -- {4}",Math.Round(aoDown4,5),Math.Round(aoDown3,5),Math.Round(aoUp2,5),Math.Round(aoUp1,5),Bars[Bars.Range.To-1].Time);
		 		var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				toolVerticalLine.Color=Color.Blue;
	 }
// 3    Гірка внизу
//	 if(aoDown4<0 && aoDown3<0 && aoUp2<0 && aoUp1<0)
 if(Dm2 && Um1)
     {
 	//	        W2=W1; W1=Bars.Range.To-1; D2=D1; D1=aoDown3;
		 		//Print("----------------------------------------  3-(N(DU)) - D4={0} D3={1} U2={2} U1={3} -- {4}",Math.Round(aoDown4,5),Math.Round(aoDown3,5),Math.Round(aoUp2,5),Math.Round(aoUp1,5),Bars[Bars.Range.To-1].Time);
		        var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				toolVerticalLine.Color=Color.White; 
/*
		 if(Y1-W2>2 && W1-Y1>2 && D2<D1 && Bars[W2].Low>Bars[W1].Low) {
				//Print("W2={0} Y1={1} W1={2}  - {3}",W2,Y1,W1,Bars[Bars.Range.To-1].Time);   	
					
			if (_positionGuidB==Guid.Empty) {
				var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 1.0, Instrument.Bid, -1, Stops.InPips(300,null), null, null);	
			    if (result1.IsSuccessful)  _positionGuidB = result1.Position.Id; }	
				}
*/
	  }
// 4  Сідло внизу
//	 if(aoUp4<0 && aoUp3<0 && aoDown2<0 && aoDown1<0)
if(Um2 && Dm1) 
     {
	//			Y1=Bars.Range.To-1;
		 		//Print("------------------------------------------  2-(N(UD)) - U4={0} U3={1} D2={2} D1={3} -- {4}",Math.Round(aoUp4,5),Math.Round(aoUp3,5),Math.Round(aoDown2,5),Math.Round(aoDown1,5),Bars[Bars.Range.To-1].Time);
		 		var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				toolVerticalLine.Color=Color.Yellow;// }
	 }

	 Print(" | {0} {1} | -- {2} -- | {3} {4} | {5} {6} | ",Math.Round(aoUp1,5),Math.Round(aoDown1,5),Bars[Bars.Range.To-1].Time,Up1,Dp1,Um1,Dm1);			
//=================================================================================================================================		
//	 		if(_positionGuidB!=Guid.Empty && aoDown1<0) {      
//				var res = Trade.CloseMarketPosition( _positionGuidB);
//        		if (res.IsSuccessful)  _positionGuidB = Guid.Empty;}
//			
//			if ( _positionGuidB!=Guid.Empty) indB++; else indB=0;
				
//			if(_positionGuidS!=Guid.Empty && aoUp1>0) {      
//				var res = Trade.CloseMarketPosition( _positionGuidS);
//      		if (res.IsSuccessful)  _positionGuidS = Guid.Empty;}
			
//			if ( _positionGuidS!=Guid.Empty) indS++; else indS=0;
	 
	 
}

    }
}
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
    [TradeSystem("AO2_Eliote3Wave")]  //copy of "AO2"
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
		double TP=0.00030;
		double aoUp1,aoDown1,aoUp2,aoDown2,aoUp3,aoDown3,aoUp4,aoDown4;
		double DV,DN,UV,UN;
		double MaxU,MaxD;
		double frUpH,frDownL,frU,frD;
		bool U0,D0;
		public bool frUp=false;
		public bool frDown=true;	
		private VerticalLine vlR,vlB;



        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);
			//_ftoInd= GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			vlR = Tools.Create<VerticalLine>();
			vlR.Color=Color.Red;
			vlB = Tools.Create<VerticalLine>();
			vlB.Color=Color.Blue;
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
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
			
			//  frUp frDown - Истина если появился НОВЫЙ фрактал Вверх/Вниз
			if(_frInd.TopSeries[Bars.Range.To-5]>0) { frUp=true; frUpH=Bars[Bars.Range.To-5].High;} else { frUp=false; }
			if(_frInd.BottomSeries[Bars.Range.To-5]>0) { frDown=true;frDownL=Bars[Bars.Range.To-5].Low; } else { frDown=false; }
			
			//	sF  = _ftoInd.FisherSeries[Bars.Range.To-1];  // Фішеер - Плюс-зверху Мінус - знизу
	 		//	sF1 = _ftoInd.FisherSeries[Bars.Range.To-2];
			
			//  Пересечение Фишер		
            //  if (sF1<0 && sF>0)    vlB.Time=Bars[Bars.Range.To-1].Time; // ВВЕРХ
			//	if (sF1>0 && sF<0)    vlR.Time=Bars[Bars.Range.To-1].Time; // ВНИЗ
			
			// AO - пересечение 0
			
		    if (aoUp2<=0 && aoUp1>=0) { DV=0; UV=0; DN=0; UN=0; U0=true; D0=false;}
            //Print("aoUp2({0})<=0 && aoUp1({1})>=0  {2} - {3}",aoUp2,aoUp1,U0,Bars[Bars.Range.To-1].Time);
			
			if (aoDown2>=0 && aoDown1<=0) { DV=0; UV=0; DN=0; UN=0; U0=false; D0=true;}
			//Print("aoDown2({0})<=0 && aoDown1({1})>=0 {2} - {3}",aoDown2,aoDown1,D0,Bars[Bars.Range.To-1].Time);
			// 1  UV
			if(aoUp4>0 && aoUp3>0 && aoDown2>0 && aoDown1>0)  { UV++; if(UV==1) MaxU=aoUp3; }
			// 2  DV
			if(aoDown4<0 && aoDown3<0 && aoUp2<0 && aoUp1<0)  { DV++; if(DV==1) MaxD=aoDown3;}
			// 3 UN
			if(aoDown4>0 && aoDown3>0 && aoUp2>0 && aoUp1>0)  { UN++;}
			// 4 DN
			if(aoUp4<0 && aoUp3<0 && aoDown2<0 && aoDown1<0)  { DN++;}
			
	//=== КОРЕКЦИЯ ===========================================================================================================							 
			
			if (_positionGuidB!=Guid.Empty && Trade.GetPosition(_positionGuidB).State==PositionState.Closed) 
			     {	_positionGuidB=Guid.Empty;  }
		    if (_positionGuidS!=Guid.Empty && Trade.GetPosition(_positionGuidS).State==PositionState.Closed) 
				{   _positionGuidS=Guid.Empty;  }			
	// =======================================================================================================================		
		//if (U0) Print("U0={0} UV={1} UN={2} MaxU={3} -- {4}",U0,UV,UN,MaxU,Bars[Bars.Range.To-1].Time); 	
		if ( U0 &&  UV==1 && UN==1 && MaxU>0.00005 )  {
			       Print("U0={0} UV={1} UN={2} MaxU={3} -- {4}",U0,UV,UN,MaxU,Bars[Bars.Range.To-1].Time); 

			 if (_positionGuidB==Guid.Empty)  { 
					    var	vlR1 = Tools.Create<VerticalLine>();
			vlR1.Color=Color.Red;
		    vlR1.Time=Bars[Bars.Range.To-1].Time;	 
			var result2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Bid, -1, Stops.InPrice(frDownL-TP,null), null, null);
				if (result2.IsSuccessful) _positionGuidB = result2.Position.Id;
		      } }
		//if (D0)  Print("D0={0} DV={1} DN={2} MaxD={3} -- {4}",D0,DV,DN,MaxD,Bars[Bars.Range.To-1].Time);
		if ( D0 &&  DV==1 && DN==1 && MaxD<-0.00005 )  { 
			       Print("D0={0} DV={1} DN={2} MaxD={3} -- {4}",D0,DV,DN,MaxD,Bars[Bars.Range.To-1].Time);

			 if (_positionGuidS==Guid.Empty)  { 
		    var	vlR2 = Tools.Create<VerticalLine>();
			vlR2.Color=Color.Blue;
		    vlR2.Time=Bars[Bars.Range.To-1].Time;				 
				 
			var result2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1, Stops.InPrice(frUpH+TP,null), null, null);
				if (result2.IsSuccessful) _positionGuidS = result2.Position.Id;	
			} }
			
			if (_positionGuidB!=Guid.Empty && frDown && frD!=frDownL) 
			{ 	frD=frDownL;
				var result2=Trade.UpdateMarketPosition(_positionGuidB,frDownL,null, null); 
				if (result2.IsSuccessful) _positionGuidB = result2.Position.Id; }
			
			if (_positionGuidS!=Guid.Empty  && frUp) 
			{  	frU=frUpH;
				var result2=Trade.UpdateMarketPosition(_positionGuidS,frUpH,null, null); 
				if (result2.IsSuccessful) _positionGuidS = result2.Position.Id;	}

			// Закриття --------------------------------------------------
			/*if ( U0 && UV==2 && UN==1)  { 
				if (_positionGuidB!=Guid.Empty) 
				   {  var result =Trade.CloseMarketPosition(_positionGuidB);
				      if (result.IsSuccessful) _positionGuidB = Guid.Empty;}
			 }*/

			if ( D0 || (aoDown4>0 && aoDown1<0) || (aoDown3>0 && aoDown1<0))  { 
				if (_positionGuidB!=Guid.Empty) 
				   {  var result =Trade.CloseMarketPosition(_positionGuidB);
				      if (result.IsSuccessful) _positionGuidB = Guid.Empty;}
			 }
		  /*
			
			if ( D0 &&  DV==2 && DN==1)  { 
				if (_positionGuidS!=Guid.Empty) 
				   {  var result =Trade.CloseMarketPosition(_positionGuidS);
				      if (result.IsSuccessful) _positionGuidS = Guid.Empty;}
				
            } */
   	 	    if ( U0 || (aoUp4<0 && aoUp1>0) || (aoUp4<0 && aoUp1>0))  { 
				if (_positionGuidS!=Guid.Empty) 
				   {  var result =Trade.CloseMarketPosition(_positionGuidS);
				      if (result.IsSuccessful) _positionGuidS = Guid.Empty;}
				
            }
           
        }
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
				private void Buy()
		{
			if (_positionGuidB!=Guid.Empty) return; 
			
			var result=Trade.Buy(Instrument.Id, 0.1); 		
			if (result.IsSuccessful) 
			{
				_positionGuidB=result.Position.Id;
				
			}
    	}
		
		private void Sell()
		{
			if (_positionGuidS!=Guid.Empty) return; 
			
			var result=Trade.Sell(Instrument.Id, 0.1); 
			if (result.IsSuccessful) 
			{
				_positionGuidS=result.Position.Id;
				
			}
    	}
    }
}
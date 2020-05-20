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
    [TradeSystem("AO2_Divirgece")]   //copy of "AO2_Eliote3Wave"
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
		double MaxU,MaxD,MaxU1,MaxD1,MaxU2,MaxD2,BarU1,BarU2,BarD1,BarD2;
		double frUpH,frDownL,frU,frD;
		bool U0,D0,DivU=false,DivD=false;
		public bool frUp=false;
		public bool frDown=true;	
		private VerticalLine vlR,vlB;



        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);

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

	//=== КОРЕКЦИЯ ===========================================================================================================							 
			
			if (_positionGuidB!=Guid.Empty && Trade.GetPosition(_positionGuidB).State==PositionState.Closed) 
			     {	_positionGuidB=Guid.Empty;  }
		    if (_positionGuidS!=Guid.Empty && Trade.GetPosition(_positionGuidS).State==PositionState.Closed) 
				{   _positionGuidS=Guid.Empty;  }			
	// =======================================================================================================================	
			
			if (aoUp2<=0 && aoUp1>0 && DivD ) 
			{  DivD=false;
					if (_positionGuidB==Guid.Empty)  { 	 
						var result2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Ask, -1, Stops.InPrice(frDownL-TP,null), null, null);
						if (result2.IsSuccessful) _positionGuidB = result2.Position.Id;	}
			}
			
			if (aoDown2>=0 && aoDown1<0 && DivU )
			{  DivU=false;
				    if (_positionGuidS==Guid.Empty)  { 	 
			            var result2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Bid, -1, Stops.InPrice(frUpH+TP,null), null, null);
				        if (result2.IsSuccessful) _positionGuidS = result2.Position.Id;	}	
				
			}
				
				
			// AO - пересечение 0
		    if (aoUp2<=0 && aoUp1>=0) { DivD=false; DivU=false;
				DV=0; UV=0; DN=0; UN=0; U0=true; D0=false; Print("Проход 0 Вверх - {0}",Bars[Bars.Range.To-1].Time);
			    				if (_positionGuidS!=Guid.Empty) 
				   				{  var result2=Trade.CloseMarketPosition(_positionGuidS); 
				   				   if (result2.IsSuccessful) {  _positionGuidS = result2.Position.Id; }  
			       				}
			}
			
			if (aoDown2>=0 && aoDown1<=0) { DivD=false; DivU=false; 
				DV=0; UV=0; DN=0; UN=0; U0=false; D0=true; Print("Проход 0 Вниз - {0}",Bars[Bars.Range.To-1].Time);
			 					if (_positionGuidB!=Guid.Empty) 
				   				{  var result2=Trade.CloseMarketPosition(_positionGuidB); 
				   				   if (result2.IsSuccessful) {  _positionGuidB = result2.Position.Id; }  
			       				}
			}

			// 1  UV
			if(aoUp4>0 && aoUp3>0 && aoDown2>0 && aoDown1>0)  { UV++; Print("ПИК {0} Вверх - {1}",UV,Bars[Bars.Range.To-3].Time);
				 if(UV==1) { MaxU2=aoUp3; BarU2=Bars[Bars.Range.To-3].High;
//				 			 					if (_positionGuidB!=Guid.Empty) 
//				   				{  var result2=Trade.CloseMarketPosition(_positionGuidB); 
//				   				   if (result2.IsSuccessful) {  _positionGuidB = result2.Position.Id; }  
//			       				}
				 } 
				 if(UV>1) { MaxU1=MaxU2; MaxU2=aoUp3; BarU1=BarU2; BarU2=Bars[Bars.Range.To-3].High;
				 // Рабочий
				 if(MaxU1>MaxU2 && BarU1<BarU2) 
				 {		DivU=true;			 
//				    if (_positionGuidS==Guid.Empty)  { 	 
//			           var result2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Bid, -1, Stops.InPrice(frUpH+TP,null), null, null);
//				       if (result2.IsSuccessful) _positionGuidS = result2.Position.Id;	}				 
				 } else DivU=false;
				 
				 }}
			// 2  DV
			if(aoDown4<0 && aoDown3<0 && aoUp2<0 && aoUp1<0)  
			{ 
				 DV++; Print("ПИК {0} Вниз - {1}",DV,Bars[Bars.Range.To-3].Time);
				if(DV==1) { MaxD2=aoDown3; BarD2=Bars[Bars.Range.To-3].Low;
//							    				if (_positionGuidS!=Guid.Empty) 
//				   				{  var result2=Trade.CloseMarketPosition(_positionGuidS); 
//				   				   if (result2.IsSuccessful) {  _positionGuidS = result2.Position.Id; }  
//			       				}	
				}
				if(DV>1) 
				{ MaxD1=MaxD2; MaxD2=aoDown3; BarD1=BarD2; BarD2=Bars[Bars.Range.To-3].Low;
				// Рабочий
				 if(MaxD1<MaxD2 && BarD1>BarD2) { DivD=true;
//				if (_positionGuidB==Guid.Empty)  { 	 
//				var result2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Ask, -1, Stops.InPrice(frDownL-TP,null), null, null);
//				if (result2.IsSuccessful) _positionGuidB = result2.Position.Id;	}
				 } else DivD=false; 
				}
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
// http://tradelikeapro.ru/sistema-dibs-method/

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


namespace IPro.TradeSystems
{
    [TradeSystem("TC_DIBS_Method")]
    public class TC_DIBS_Method : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		public DateTime DTime; // Время 
		bool Time_Start = false;
		double BarH;
		double BarL;
		double cTP;
					//  Время работы АТС

		private Guid _positionGuidA=Guid.Empty;
		private Guid _positionGuidB=Guid.Empty;
		private Guid _positionGuidC=Guid.Empty;
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			


        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
			
        }
        
        protected override void NewBar()
        {
			
			
			if (Bars[Bars.Range.To-1].Time.Hour.ToString()=="6") { Time_Start=true; BarH=Bars[Bars.Range.To].Open; 
			Print("Bars-{0} Open Time-{1}",Bars[Bars.Range.To].Time,BarH=Bars[Bars.Range.To].Open);
			}
			if (Bars[Bars.Range.To-1].Time.Hour.ToString()=="13") { Time_Start=false; 
			Print("Bars-{0} Close Time-{1}",Bars[Bars.Range.To].Time,Bars[Bars.Range.To-1].Time);
			}

			//if (D.ToString()=="06:00:00") { Time_Start=true; Blue(); }
			//if (DTime.TimeOfDay.ToString()=="12:00:00") { Time_Start=false;  Blue(); }
			
			// Event occurs on every new bar
			if (Time_Start) {
			if (Bars[Bars.Range.To-1].High<Bars[Bars.Range.To-2].High && Bars[Bars.Range.To-1].Low>Bars[Bars.Range.To-2].Low) { 
				cTP=Bars[Bars.Range.To-1].High-Bars[Bars.Range.To-1].Low;
				if (Bars[Bars.Range.To-1].Close>BarH) { 
					var result=Trade.BuyStop(Instrument.Id, 0.1,( Bars[Bars.Range.To-1].High+0.0002),Stops.InPrice(Bars[Bars.Range.To-1].Low+0.0002,Bars[Bars.Range.To-1].High+cTP)); 
				   _positionGuidA=result.Position.Id;}
				if (Bars[Bars.Range.To-1].Close<BarH) { 
					var result2=Trade.SellStop(Instrument.Id, 0.1,( Bars[Bars.Range.To-1].Low-0.0002),Stops.InPrice(Bars[Bars.Range.To-1].High+0.0002,Bars[Bars.Range.To-1].Low-cTP));
				_positionGuidA=result2.Position.Id;	
				}		
			                  }
   		                } 
			if (!Time_Start) Trade.CancelPendingPosition(_positionGuidA);
		}
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
		

			private void BuyA()
		{ 	if (_positionGuidA!=Guid.Empty) return; 			
			var result=Trade.BuyStop(Instrument.Id, 0.1,( Bars[Bars.Range.To-1].High+0.0002),Stops.InPrice(Bars[Bars.Range.To-1].Low+0.0002,Bars[Bars.Range.To-1].High+cTP)); 		
			if (result.IsSuccessful) _positionGuidB=result.Position.Id; }
		
		private void SellA()
		{ if (_positionGuidA!=Guid.Empty) return; 			
			var result=Trade.SellStop(Instrument.Id, 0.1,( Bars[Bars.Range.To-1].Low-0.0002),Stops.InPrice(Bars[Bars.Range.To-1].High+0.0002,Bars[Bars.Range.To-1].Low-cTP)); 
			if (result.IsSuccessful) _positionGuidA=result.Position.Id;	}
		
			private void ClosePositionA()
		{ var result =Trade.CloseMarketPosition(_positionGuidA);
			if (result.IsSuccessful) _positionGuidA = Guid.Empty;}
		// B =================================================

			private void BuyB()
		{ 	if (_positionGuidB!=Guid.Empty) return; 			
			var result=Trade.BuyStop(Instrument.Id, 0.1,( Bars[Bars.Range.To-1].High+0.0002),Stops.InPrice(Bars[Bars.Range.To-1].Low+0.0002,Bars[Bars.Range.To-1].High+0.0002+cTP)); 		
			if (result.IsSuccessful) _positionGuidB=result.Position.Id; }
		
		private void SellB()
		{ if (_positionGuidB!=Guid.Empty) return; 			
			var result=Trade.SellStop(Instrument.Id, 0.1,( Bars[Bars.Range.To-1].Low-0.0002),Stops.InPrice(Bars[Bars.Range.To-1].Low+0.0002,Bars[Bars.Range.To-1].High+0.0002+cTP)); 
			if (result.IsSuccessful) _positionGuidB=result.Position.Id;	}
		
			private void ClosePositionB()
		{ var result =Trade.CloseMarketPosition(_positionGuidB);
			if (result.IsSuccessful) _positionGuidB = Guid.Empty;}
		// C ================================================================
				private void BuyC()
		{ 	if (_positionGuidC!=Guid.Empty) return; 			
			var result=Trade.BuyStop(Instrument.Id, 0.1,( Bars[Bars.Range.To-1].High+0.0002),Stops.InPrice(Bars[Bars.Range.To-1].Low+0.0002)); 		
			if (result.IsSuccessful) _positionGuidC=result.Position.Id; }
		
		private void SellC()
		{ if (_positionGuidC!=Guid.Empty) return; 			
			var result=Trade.SellStop(Instrument.Id, 0.1,( Bars[Bars.Range.To-1].Low-0.0002),Stops.InPrice(Bars[Bars.Range.To-1].Low+0.0002)); 
			if (result.IsSuccessful) _positionGuidC=result.Position.Id;	}
		
			private void ClosePositionC()
		{ var result =Trade.CloseMarketPosition(_positionGuidC);
			if (result.IsSuccessful) _positionGuidC = Guid.Empty;}
		
	
	
	
	}
}
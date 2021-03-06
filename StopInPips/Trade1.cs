using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;

namespace IPro.TradeSystems
{
    [TradeSystem("Trade1")]
    public class Trade1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		bool first=true;
		int Count=1;
		private Guid _positionGuid=Guid.Empty;
		

		
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
            // Event occurs on every new bar
			int LotVolume,StopLoss, TakeProfit;
			// var result=Trade.Sell(Instrument.Id, LotVolume, Stops.InPips(StopLoss, TakeProfit));
			Count++;
			if(Count==2) {Trade.Buy(Instrument.Id, 0.1); 	}
			if(Count==4) {Trade.Buy(Instrument.Id, 0.1); 	}
			if(Count==6) {Trade.Sell(Instrument.Id, 0.1); 	}
			
			if(first) { Buy(); first=false;  
					
			//	_positionGuid=result1.Position.InstrumentId;
			//	Print("===================================================================================");
			//	Print("Id={0}",result.Position.Id);
			//	Print("ClosePrice={0}",result.Position.ClosePrice);
			//	Print("CloseTime={0}",result.Position.CloseTime);
			//	Print("Comission={0}",result.Position.Comission);
			//	Print("ExpireTime={0}",result.Position.ExpireTime);
			//	Print("Lots={0}",result.Position.Lots);
			//	Print("MagicNumber={0}",result.Position.MagicNumber);
			//	Print("Number={0}",result.Position.Number);
			//	Print("OpenPrice={0}",result.Position.OpenPrice);
			//	Print("OpenTime={0}",result.Position.OpenTime);
			//	Print("Pips={0}",result.Position.Pips);
			//	Print("PipValue={0}",result.Position.PipValue);
			//	Print("Profit={0}",result.Position.Profit);
			//	Print("State={0}",result.Position.State);
			//	Print("StopLoss={0}",result.Position.StopLoss);
			//	Print("Swap={0}",result.Position.Swap);
			//	Print("TakeProfit={0}",result.Position.TakeProfit);
			//	Print("Type={0}",result.Position.Type);
			//	Print("InstrumentId={0}",result.Position.InstrumentId);
				//_positionOpendSignalType=signal;
			}
						
			var posActiveAll = Trade.GetActivePositions();
            var posActiveMine = Trade.GetActivePositions(null, true);
                Print("Активных позиций: {0}, Позиций АТС: {1} - Count={2}", posActiveAll.Length, posActiveMine.Length,Count);
			
			//var result3 =Trade.GetPosition(_positionGuid);
			//Print("Pips = {0}",result3.Pips);
			//if(Count>9) {  var result3=Trade.GetActivePositions(_positionGuid.Position.MagicNumber);}
			
			if(Count>13 && posActiveMine.Length>1) { ClosePosition();
				 
            //    Print("LR_Position_closed_by_inverse_signal", result2.Position.Number, result2.Position.ClosePrice);	
			//	Print("================================================================================================");
            //  Print("Id={0}",result2.Position.Id);
			//	Print("ClosePrice={0}",result2.Position.ClosePrice);
			//	Print("CloseTime={0}",result2.Position.CloseTime);
			//	Print("Comission={0}",result2.Position.Comission);
			//	Print("ExpireTime={0}",result2.Position.ExpireTime);
			//	Print("Lots={0}",result2.Position.Lots);
			//	Print("MagicNumber={0}",result2.Position.MagicNumber);
			//	Print("Number={0}",result2.Position.Number);
			//	Print("OpenPrice={0}",result2.Position.OpenPrice);
			//	Print("OpenTime={0}",result2.Position.OpenTime);
			//	Print("Pips={0}",result2.Position.Pips);
			//	Print("PipValue={0}",result2.Position.PipValue);
			//	Print("Profit={0}",result2.Position.Profit);
			//	Print("State={0}",result2.Position.State);
			//	Print("StopLoss={0}",result2.Position.StopLoss);
			//	Print("Swap={0}",result2.Position.Swap);
			//	Print("TakeProfit={0}",result2.Position.TakeProfit);
			//	Print("Type={0}",result2.Position.Type);
			//	Print("InstrumentId={0}",result2.Position.InstrumentId);					
				
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
			if (_positionGuid!=Guid.Empty) return; 
			
			var result=Trade.Buy(Instrument.Id, 0.1); 		
			if (result.IsSuccessful) 
			{
				_positionGuid=result.Position.Id;
				
			}
    	}
		
		private void Sell()
		{
			if (_positionGuid!=Guid.Empty) return; 
			
			var result=Trade.Sell(Instrument.Id, 0.1); 
			if (result.IsSuccessful) 
			{
				_positionGuid=result.Position.Id;
				
			}
    	}
			private void ClosePosition()
		{
			var result =Trade.CloseMarketPosition(_positionGuid);
			if (result.IsSuccessful) 
			{
                //Print("LR_Position_closed_by_inverse_signal", result.Position.Number, result.Position.ClosePrice);	
				_positionGuid = Guid.Empty;			
			}
		}
		
    }
}
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
    [TradeSystem("Trade_Ex1")]
    public class Trade_Ex1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
				private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
        }        

   
        protected override void NewBar()
        {
            // Event occurs on every new bar
			var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1, Bars[Bars.Range.To-1].High+0.00100 , 0, Stops.InPips(100,100), null, null, null);
				if (result.IsSuccessful) posGuidSell=result.Position.Id;
								
									if(result.Position.Pips>10) Trade.CloseMarketPosition(posGuidSell);
									
			var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, Bars[Bars.Range.To-1].Low-0.00100 , 0, Stops.InPips(100,100), null, null, null);
				if (result1.IsSuccessful) posGuidSell=result1.Position.Id;
								
									if(result1.Position.Pips>10) Trade.CloseMarketPosition(posGuidBuy);						
								
        }

//  получить все позиции, посчитать по ним прибыль и вывести 
//  значение прибыли перед деинициализацией (Нажатием кнопки Stop) торговой системы.		
		
protected override void Deinit()
{
    var closedPos = Trade.GetClosedPositions(null, true);
    decimal profit = 0;
    foreach (var cpos in closedPos) profit = profit + cpos.Profit;
    Print("Profit = {0}", profit);
} 

    }
}
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
    [TradeSystem("New2")]
    public class New2 : TradeSystem
    {   DateTime BaseTime;
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }

        protected override void Init()
        {
			// Сработало!!!  - var baseTime = BaseTime.Date;  Print("Базовое время = {0}", baseTime.ToString());
            // Event occurs once at the start of the strategy
            //Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			var Account_Type = Account.Type; Print("Account.Type = {0}", Account_Type.ToString()); // Print: Demo
			var Account_Balance = Account.Balance; Print("Account.Balance = {0}", Account_Balance.ToString()); // 10000
			var Account_Currency = Account.Currency; Print("Account.Currency = {0}", Account_Currency.ToString()); //USD
			var Account_Equity = Account.Equity; Print("Account.Equity = {0}", Account_Equity.ToString()); //10000
			var Account_FreeMargin = Account.FreeMargin; Print("Account.FreeMargin = {0}", Account_FreeMargin.ToString()); //10000
			var Account_Leverage = Account.Leverage; Print("Account.Leverage = {0}", Account_Leverage.ToString()); //100
			var Account_LotStep = Account.LotStep; Print("Account.LotStep = {0}", Account_LotStep.ToString());	// 0.01
			var Account_Margin = Account.Margin; Print("Account.Margin = {0}", Account_Margin.ToString()); // 
			var Account_MarginCallLevel = Account.MarginCallLevel; Print("Account.MarginCallLevel = {0}", Account_MarginCallLevel.ToString());
			var Account_MarginLevel = Account.MarginLevel; Print("Account.MarginLevel = {0}", Account_MarginLevel.ToString());
			var Account_MaxLot = Account.MaxLot; Print("Account.MaxLot = {0}", Account_MaxLot.ToString());
            var Account_MaxPendingPositions = Account.MaxPendingPositions; Print("Account.MaxPendingPositions = {0}", Account_MaxPendingPositions.ToString()); 
		    var Account_MaxPositions = Account.MaxPositions; Print("Account.MaxPositions = {0}", Account_MaxPositions.ToString());
		    var Account_MinLot = Account.MinLot; Print("Account.MinLot = {0}", Account_MinLot.ToString());
			var Account_Number = Account.Number; Print("Account.Number = {0}", Account_Number.ToString());
			var Account_StopoutLevel = Account.StopoutLevel; Print("Account.StopoutLevel = {0}", Account_StopoutLevel.ToString());
			
		}   
		

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
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
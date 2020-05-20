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
    [TradeSystem("Print1")]
    public class Print1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }

        protected override void Init()
        {
            // Event occurs once at the start of the strategy
        //   Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
						// вывести номер Вашего текущего торгового счета:
		//	Print("Номер Вашего текущего торгового счета: {0}", Account.Number);
			
		//	Print("тип Вашего текущего торгового счета: {0}", Account.Type);
		//	Print("Started for iPamm account. Lot settings will be ignored");
			
		//	Print("Текущий размер кредитного плеча: {0}", Account.Leverage);
		//	Print("Валюта текущего торгового счета: {0}", Account.Currency);
		//	Print("Состояние баланса с указанием валюты на Вашем текущем торговом счете: {0} {1}", Account.Balance, Account.Currency);
		//	Print("Текущее значение эквити для Вашего текущего торгового счета: {0}", Account.Equity);
		//	Print("Текущая сумма залоговых средств для Вашего текущего торгового счета: {0}", Account.Margin);
		//	Print("Свободная сумма залоговых средств для Вашего текущего торгового счета: {0}", Account.FreeMargin);
		//	Print("Уровень маржи для Вашего текущего торгового счета: {0}", Account.MarginLevel);
		//	Print("Уровень, при котором срабатывает сигнал Margin call на Вашем текущем торговом счете: {0}", Account.MarginCallLevel);
		//	Print("Уровень, при котором срабатывает сигнал Stop out на  Вашем текущем торговом счете: {0}", Account.StopoutLevel);
			//позволит вывести максимальное количество рыночных позиций для Вашего текущего торгового счета:
	//		Print("Current trade account  maximum of active positions is {0}", Account.MaxPositions);
			
			// позволит вывести максимальное количество отложенных ордеров для Вашего текущего торгового счета:
	//		    Print("Количество отложенных ордеров для Вашего текущего торгового счета: {0}", Account.MaxPendingPositions);
			//вывести минимально допустимый лот для Вашего текущего торгового счета:
	//		    Print("Минимально допустимый лот для Вашего текущего торгового счета: {0}", Account.MinLot);
			// вывести максимально допустимый лот для Вашего текущего торгового счета:
	//		    Print("Максимально допустимый лот для Вашего текущего торгового счета: {0}", Account.MaxLot);
			// позволит вывести минимально допустимый шаг лота для Вашего текущего торгового счета:
	//		 Print("Минимально допустимый шаг лота для Вашего текущего торгового счета: {0}", Account.LotStep);
			
			 Print("Значением серверного времени : {0}", ServerTime);
			Print("Значением локального времени пользователя : {0}", Time);
			
	//		Print("Значение начального Time Frame", );
			
			Print( "Уникальный идентификатор торгового инструмента  - Instrument  id: {0}", Instrument.Id);
			Print("Имя торгового инструмента:EURUSD {0}", Instrument.Name);
			Print("Вывести цену Ask {0}", Instrument.Ask);
			Print("Вывести цену Bid {0}", Instrument .Bid);
			Print("вывести значение Spread {0}", Instrument.Spread);
			Print("вывести значение торгового периода  {0} timeframe", Timeframe); 
			Print("Базовое время = {0}", Period.BaseTime.ToString());
			

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
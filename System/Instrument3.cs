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
    [TradeSystem("ww")]
    public class ww : TradeSystem
    {
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			Print("PriceScale = {0}",Instrument.PriceScale);
			Print("LongPositionSwap = {0}",Instrument.LongPositionSwap);
			Print("LotSize = {0}",Instrument.LotSize);
			Print("Point = {0}",Instrument.Point);
			Print("Name = {0}",Instrument.Name);
			Print("PointValues = {0}",Instrument.PointValues);
			Print("Prices = {0}",Instrument.Prices);
			Print("PriceScale = {0}",Instrument.PriceScale);
			Print("ShortPositionSwap = {0}",Instrument.ShortPositionSwap);
			Print("Spread = {0}",Instrument.Spread);
			
        }
    }
}
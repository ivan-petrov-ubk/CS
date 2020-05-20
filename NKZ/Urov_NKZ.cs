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
    [TradeSystem("Urov_NKZ")]
    public class Urov_NKZ : TradeSystem
    {
		[Parameter("NKZ 1/2:", DefaultValue = 0)]
        public int NKZ1 { get; set; }
		public double nkz;
		
        protected override void Init()
        {
			nkz = NKZ1*Instrument.Point;
			nkz = Math.Round(nkz,Instrument.PriceScale); 
			Print("{0}",nkz);
        }        

    }
}
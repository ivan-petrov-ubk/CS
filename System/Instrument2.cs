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
    [TradeSystem("Instrument")]
    public class Instrument : TradeSystem
    {
     
        private bool first=true;
		
        protected override void NewBar()
        {  if(first) { first=false;
			double r = Math.Pow(10.0,Instrument.PriceScale);
			Print("=================================================================================================");
			Print("Pow={0}",r);
			Print("Instrument.Ask = {0}",Instrument.Ask);//Instrument.Ask = 1,24531 - разница=0.00008
			Print("Instrument.Bid = {0}",Instrument.Bid); //Instrument.Bid = 1,24523			
			Print("Instrument.Comission = {0}",Instrument.Comission);//Instrument.Comission = 16,0
			Print("Instrument.Id = {0}",Instrument.Id); //Instrument.Id = 75a40641-944a-4a69-b160-ef84650b3ef5
			Print("Instrument.LongPositionSwap = {0}",Instrument.LongPositionSwap); //Instrument.LongPositionSwap = 8,0
			Print("Instrument.LotSize = {0}",Instrument.LotSize); //Instrument.LotSize = 100000
			Print("Instrument.Name = {0}",Instrument.Name); //Instrument.Name = EURUSD
			Print("Instrument.Point = {0}",Instrument.Point); //Instrument.Point = 1E-05
			Print("Instrument.PointValues = {0}",Instrument.PointValues.Ask);
			Print("Instrument.Prices = {0}",Instrument.Prices.Ask);  //Instrument.Prices = IPro.Model.Client.MarketData.DelegatePriceValues
			Print("Instrument.PriceScale = {0}",Instrument.PriceScale); //Instrument.PriceScale = 5
			Print("Instrument.ShortPositionSwap = {0}",Instrument.ShortPositionSwap); //Instrument.ShortPositionSwap = 8,0
			Print("Instrument.Spread = {0}",Instrument.Spread);         //Instrument.Spread = 7,9999999999858E-05 = 8
			Print("Instrument.ToString() = {0}",Instrument.ToString()); //Instrument.ToString() = Model.Client.DeviceScopeStreamType
			Print("Instrument.GetType() = {0}",Instrument.GetType());   //Instrument.GetType() = Model.Client.DeviceScopeStreamType
			
			Print("=================================================================================================");
        }  }
 
    }
}
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
    [TradeSystem("Pause")]
    public class Pause : TradeSystem
    {
		[Parameter("NKZ :")]
        public int dt1 { get; set; }

		[Parameter("K1 :")]
        public int k1 { get; set; }
		
		public int k;
		
		protected override void Init()
        {	
			Print("Init {0} - {1}",Bars[Bars.Range.To-1].Time,dt1);
			k=0;
			
        }        
        
        protected override void NewBar()
        {    k++; k1++;
             Print("NewBar {0} - {1} - k={2} k1={3}",Bars[Bars.Range.To-1].Time,dt1,k,k1);
        }
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0}  price {1} Number {2} Type={3}", position.Number, position.ClosePrice, position.Number, position.Type );
            }
        }
    }
}
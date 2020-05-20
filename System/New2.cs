using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators.Standard;
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
        private Guid toolGuid = new Guid("D480B4A21DA0");
        protected override void Init()
        {
			var Bars_Range = Bars.Range; Print("Bars.Range = {0}", Bars_Range.ToString());
			
			//var Bars_GetType = Bars.GetType(); Print("Bars.GetType() = {0}", Bars_GetType);
			//var Bars_GetType = Bars.GetType;  Print("Bars.GetType() = {0}", Bars_GetType);
			//var AppContext_BaseDirectory = AppContext.BaseDirectory; Print("AppContext.BaseDirectory = {0}", AppContext_BaseDirectory);
		    // var ActivationContext_ContextForm = .HorizontalLine.; Print("ActivationContext.ContextForm = {0}", ActivationContext_ContextForm); 
			
		}   
		

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
		
		//private Guid toolGuid = new Guid("D480B4A21DA0");
        protected override void NewBar()
        {
			// Event occurs on every new bar
		  for(int i = 0; i< 7; i++)
          {
                if (i % 2 == 0)
                {
                     var bullTool = Tools.Create(toolGuid);
                     bullTool.Label = "Bull";
                }
                else
                {                      
                     var bearTool = Tools.Create(toolGuid);
                     bearTool.Label = "Bear";
                }                         
          }               
          var bulls = Tools.GetByLabel("Bull");
          Print("Bulls tools count are {0}", bulls.Length);

			
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
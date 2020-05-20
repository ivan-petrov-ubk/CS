using System;
using System.IO;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;

namespace IPro.TradeSystems
{
    [TradeSystem("File1")]
    public class File1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }

        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
		string path = @"d:\My\Test.txt";
			
		//using (StreamWriter writer = new StreamWriter(path, true))
          //  {
            //    writer.Write("Hello World");
            //}
			
       if (!File.Exists(path)) 
        { Print("1 ----- ");
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path)) ;
          //  {  Print("2 ----- ");
            //    sw.WriteLine("Hello");
              //  sw.WriteLine("And");
              //  sw.WriteLine("Welcome");
            }	
			Print("3 ----- ");
        //}

        // Open the file to read from.
        //using (StreamReader sr = File.OpenText(path)) 
        //{
          //  string s = "";
           // while ((s = sr.ReadLine()) != null) 
           // {
           //     Console.WriteLine(s);
           // }
       // }
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
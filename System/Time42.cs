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
    [TradeSystem("DTime")]
    public class DTime : TradeSystem
    {

		public DateTime t1;
        public DateTime t2;
		bool Time_Start;
		private DateTime dt1; 
		
        string dateString, format;  
      	DateTime result;
      	IFormatProvider provider;
		
        protected override void NewBar()
        {
            // Event occurs on every new bar
		      t1 = Bars[Bars.Range.To-1].Time;
			//Print("Date = {0}",t1.Date);             // Date = 07.11.2016 0:00:00
			//Print("Day = {0}",t1.Day);               // Day = 4
			//Print("DayOfWeek = {0}",t1.DayOfWeek);   // DayOfWeek = Friday
			//Print("DayOfYear = {0}",t1.DayOfYear);   // DayOfYear = 309
			//Print("Hour = {0}",t1.Hour);             // Hour = 22
			//Print("Kind = {0}",t1.Kind);             // Minute = 0
			//Print("Millisecond = {0}",t1.Millisecond);   // Millisecond = 0
			//Print("Minute = {0}",t1.Minute);        //  Minute = 0
			//Print("Month = {0}",t1.Month);          //  Month = 11
			//Print("Second = {0}",t1.Second);        //  Second = 0
			//Print("Ticks = {0}",t1.Ticks);          //  Ticks = 636138936000000000
			//Print("TimeOfDay = {0}",t1.TimeOfDay);  //  TimeOfDay = 22:00:00
			//Print("Year = {0}",t1.Year);            //  Year = 2016
			//Print("DateTime.Now = {0}",DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"));      // 20-05-2018 10:11:48
			//Print("DateTime.Today = {0}",DateTime.Today.ToString("dd-MM-yyyy hh:mm:ss"));  // DateTime.Today = 20-05-2018 12:00:00
			//Print("{0}",DateTime.UtcNow.AddHours(-10).ToString("dd-MM-yyyy hh:mm:ss"));
						
			//Print("===============================================================");
						
			//Time_Start = (t1.TimeOfDay.ToString()=="22:00:00");
			//Print("{0} = {1}",Time_Start,t1.TimeOfDay);
			//Print("{0} = {1}",t1,t1.CompareTo(t1.AddHours(2)));
			// Print("{0} - {1}",DateTime.UtcNow.ToString("dd-MM-yyyy hh:mm:ss"),DateTime.UtcNow.AddHours(-10).ToString("dd-MM-yyyy hh:mm:ss"));
			// Print("{0}",DateTime.Now.AddHours(-2).ToString("dd-MM-yyyy hh:mm:ss"));
						
			//dateString = "01/01/2018";
      		format = "dd.MM.yyyy hh:mm:ss";
			//result = DateTime.ParseExact(dateString, format, null);
			dateString = Bars[Bars.Range.To-1].Time.Date.AddDays(-1).ToString("dd.MM.yyyy")+" 21:00:00";
			Print("------ {0} - {1}",dateString,format);			
			result = DateTime.ParseExact(dateString);
			Print("{0}",result.Date);
		}
        
   }
}
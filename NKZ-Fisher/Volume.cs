using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using IPro.Model.Programming.Indicators.Standard;
using IPro.Model.Client.MarketData;
using System.Collections.Generic;
using System.IO;

namespace IPro.TradeSystems
{
    [TradeSystem("Volume")]
    public class Volume : TradeSystem
    {      
		public Volumes _vInd;
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		public double High;
		public  DateTime NDate;
		public string st;
		public static StreamReader LogSW = null;
		private string trueInitPath = "";	
		private int k2;
		
	    protected override void Init()
        {
			 _vInd= GetIndicator<Volumes>(Instrument.Id, Timeframe);
		}	
		
		
        protected override void NewBar()
        {  
           //Print("Volumes up={0} down={1}", _vInd.SeriesUp[Bars.Range.To-1], _vInd.SeriesDown[Bars.Range.To-1]);
		   //XPrint("{0};{1};",Bars[Bars.Range.To-1].Time,Bars[Bars.Range.To-1].High);
			InitFile();
		}
        
		protected void XPrint(string xxformat, params object[] parameters)
		{		var trueBuyPath = PathToLogFile+"\\"+Instrument.Name.ToString()+".DAT";
				var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueBuyPath, logString);
		}  
		//===============  Read conf   ===============================================================================		
 		protected void InitFile()
		{ 
			trueInitPath=PathToLogFile+"\\EURUSD.DAT";
			LogSW = new StreamReader(File.Open(trueInitPath, FileMode.Open,FileAccess.Read,FileShare.Read));
		    if(LogSW!=null)
		    {   st="";
 		       string line;
  			      while ((line = LogSW.ReadLine()) != null)
    				    { 
       				     k2=0; 
						for (int j = 0; j < line.Length; j++)
						 	{    
							if(line[j]==';') 
								{ 	k2++; 
									if(k2==1)   { NDate=Convert.ToDateTime(st); }//Print("NDate={0}",st); }   
									if(k2==2)   { High=Convert.ToDouble(st); }//Print("High={0}",st); }
									st="";
								} else { st = st+line[j]; }	
						  	}
        				}		
						Print("{0} - {1}",NDate,High);
						LogSW.Close(); 
				}
		}
    }
}
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
    [TradeSystem("File1")]
    public class File1 : TradeSystem
    {
		
		[Parameter("Log_FileName", DefaultValue = @"nkz")]
		public string LogFileName { get;set; }
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		public string trueLogPath = "",logString;
		public static StreamReader LogSW = null;
		
		
		public int k,kol, NKZ,nkz4,TP2;
		public string st,name;
		public bool tu,td,nu,nd,nkz2;


        protected override void Init()
        {  
			InitFile();
			
        }        
//========================================================================================		
 		protected void InitFile()
		{
			trueLogPath=PathToLogFile+"\\"+LogFileName+".csv";
			LogSW = new StreamReader(File.Open(trueLogPath, FileMode.Open,FileAccess.Read,FileShare.Read)); 
		    if(LogSW!=null)
		    {   st="";
 		       string line;
  			      while ((line = LogSW.ReadLine()) != null)
    				    {
       				     k=0; name="";
						for (int j = 0; j < line.Length; j++)
						 {    
							if(line[j]==';') 
							{ 	k++; 
								if(k==2) name=st; 
								if(name==Instrument.Name) 
								{	
									if(k==3) kol=Convert.ToInt32(st);
									if(k==4) { if(st=="1") tu=true; if(st=="2") td=true; }
									if(k==5) { if(st=="1") nu=true; if(st=="2") nd=true; }
									if(k==6) { if(st=="1") nkz2=true; }
									if(k==7) NKZ=Convert.ToInt32(st);
									if(k==8) TP2=Convert.ToInt32(st);
								}
								st="";
							} else { st = st+line[j]; }	
						  }
        				}
		         Print("INIT name={0} - kol={1} tu={2} td={3} nu={4} nd={5} nkz2={6} NKZ={7} TP2={8}  ",name,kol,tu,td,nu,nd,nkz2,NKZ,TP2);
			 }  			
		}  
// END InitFile()  ===============================================================================

    }
}
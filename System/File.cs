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
		
		[Parameter("Log_FileName", DefaultValue = @"hta")]
		public string LogFileName { get;set; }
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		public string trueLogPath = "",logString;
		
		public static StreamReader LogSW = null;
		public int i,k;
		public string st,name;
		public bool tu,td,nu,nd,nkz2,zp;
		public int NKZ,frU1,frU2,frD1,frD2,nkz4;
		// public string[] line1 = new string[5];
		public IEnumerable<String> line1;

        protected override void Init()
        {   i=0;
			InitFile();
			
        }        
        
        protected override void NewBar()
        {  	
			//logString=string.Format("{0}",Instrument.Name)+Environment.NewLine;
			//File.AppendAllText(trueLogPath, logString);
			
		}
 		protected void InitFile()
		{
			trueLogPath=PathToLogFile+"\\"+LogFileName+".txt";
			LogSW = new StreamReader(File.Open(trueLogPath, FileMode.Open,FileAccess.Read,FileShare.Read)); 

			
    if(LogSW!=null)
    {   st="";
        string line;
        while ((line = LogSW.ReadLine()) != null)
        {
            k=0; zp=false;
			for (int j = 0; j < line.Length; j++)
				{    
					if(line[j]==';') { k++; 
					if(st==Instrument.Name) zp=true;
					
					if(zp) {	
						if(k==2) name=st; 
						if(k==3) nkz4=Convert.ToInt32(st);
						if(k==4) { if(st=="1") tu=true; if(st=="2") td=true; }
						if(k==5) { if(st=="1") nu=true; if(st=="2") nd=true; }
						if(k==6) { if(st=="1") nkz2=true; }
						if(k==7) NKZ=Convert.ToInt32(st);
						if(k==8) { if(tu) frU1=Convert.ToInt32(st);  if(td) frD1=Convert.ToInt32(st);}
						if(k==9) { if(tu) frU2=Convert.ToInt32(st);  if(tu) frD2=Convert.ToInt32(st);}
							}
						st="";
					} else
						{ st = st+line[j]; }
									
				}
				
					
        }
		 Print("INIT name={0} - nkz4={1} NKZ={2} st={3}",name,nkz4,NKZ,st);
		
    }  			
		}

    }
}
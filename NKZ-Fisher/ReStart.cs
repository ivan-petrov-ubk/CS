using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using System.IO;

namespace IPro.TradeSystems
{
    [TradeSystem("ReStart")]
    public class ReStart : TradeSystem
    {
		[Parameter("Номер версии", DefaultValue = 1)]
        public int ver { get; set; }
		
		[Parameter("Log_Path", DefaultValue = @"1")]
		public string L1 { get;set; }
		
		[Parameter("Init_FileName", DefaultValue = @"m")]
		public string BuyFileName { get;set; }
		
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();			
		public static StreamReader LogSW = null;
		private string trueInitPath = "";	
		private string st,st2,name,name2;		
		private int k2=0,NKZ,ini=12;
		private bool nu,nd,n2,torg,tu,td;
		private double kl,tp2;
		public TrendLine toolTrendLine4;		
				
			
        protected override void Init()
        {
			Print("InitFile() - START");
				InitFile();
			Print("InitFile() - STOP");
			
			Print("Start toolTrendLine4.Id = {0}",toolTrendLine4);
			if(toolTrendLine4==null) { Print("ПУСТО!!!");
			toolTrendLine4 = Tools.Create<TrendLine>(); toolTrendLine4.Color=Color.HotPink; toolTrendLine4.Width=3;
			} else {  Tools.Remove(toolTrendLine4); 
			toolTrendLine4 = Tools.Create<TrendLine>(); toolTrendLine4.Color=Color.Red; toolTrendLine4.Width=3;
			}
	
        }        


        protected override void NewBar()
        {
			Print("1 - {0} - ini={1}",Bars[Bars.Range.To-1].Time,ini);
            if(ini==12) ini=1001;
			
toolTrendLine4.Point1= new ChartPoint(Bars[Bars.Range.To-1].Time, 1.1212); toolTrendLine4.Point2= new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(1), 1.1212);				

        }
//===============  Read conf   ===============================================================================		
 		protected void InitFile()
		{ 
			trueInitPath=PathToLogFile+"\\"+BuyFileName+".csv";
			LogSW = new StreamReader(File.Open(trueInitPath, FileMode.Open,FileAccess.Read,FileShare.Read));
		    if(LogSW!=null)
		    {   st="";
 		       string line;
  			      while ((line = LogSW.ReadLine()) != null)
    				    { 
       				     k2=0; name2="";
						for (int j = 0; j < line.Length; j++)
						 {    
							if(line[j]==';') 
							{ 	k2++; 
								if(k2==2) { name2=st; }//Print("{0} - {1}",Instrument.Name,st); }
								if(name2==Instrument.Name) 
								{	
									if(k2==2) { name=st;}   //Print("{0} - {1}",k2,st ); }
									if(k2==3) {  kl=Convert.ToDouble(st); } //Print("----------------- {0} - {1}",k2,st ); }
									if(k2==4) { if(st=="1") { tu=true; td=false; }  if(st=="2") { td=true; tu=false;}} //Print("----------------- {0} - {1}",k2,st ); }
									if(k2==5) { if(st=="1") nu=true;  if(st=="2") nd=true; } //Print("----------------- {0} - {1}",k2,st ); }
									if(k2==6) { if(st=="0") n2=false; if(st=="1") n2=true; } //Print("----------------- {0} - {1}",k2,st ); }
									if(k2==7) { NKZ=Convert.ToInt32(st); }  //Print("----------------- {0} - {1} - {2}",k2,st,NKZ ); }
									if(k2==8) { if(st=="1") torg=true;  if(st=="0") torg=false; }
									if(k2==9) {  tp2=Convert.ToDouble(st); }
								}
								st="";
							} else { st = st+line[j]; }	
						  }
        				}
					
//Print("{14} - File INIT name={0} - kl={1} tu={2} td={3} nu={4} nd={5} n2={6} NKZ={7} TP2={8} lot={9} SL1={10} dl={11} frac={12} kof={13}",name,kl,tu,td,nu,nd,n2,NKZ,TP2,lot,SL1,dl,frac,kof,Instrument.Name);
Print("{8} - File INIT name={0} - kl={1} tu={2} td={3} nu={4} nd={5} n2={6} NKZ={7}",name,kl,tu,td,nu,nd,n2,NKZ,Instrument.Name);			
						LogSW.Close(); 
			}  			
		}  
// END InitFile()  ===============================================================================	        

    }
}
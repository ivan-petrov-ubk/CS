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
    [TradeSystem("ZZ_Test50")]     //copy of "ZZ"
    public class ZZ_Ex1 : TradeSystem
    {

		[Parameter("ExtDepth50:", DefaultValue = 60)]
        public int ED50 { get; set; }
		
		private ZigZag _wprInd50;
		private double zz1=2,zz2=2,zz3=2;
		private double zztb2,zzts2;
		private double z50_1,z50_2,z50_3;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5;
		private int zzi1,zzi2,zzi3,zzi4,V=0;
		private bool torg=true;
		private double sv=0,av1=0,av2=0,av3=0,av4=0,av5=0;
		private int ri1,ri2,ri3,ri4,ri5;
		private int max,min,av;
		
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		private string trueLogPath = "";
		public string LogFileName = @"L_5";
		
        protected override void Init()
        {
			trueLogPath=PathToLogFile+"\\"+LogFileName+".LOG";
			_wprInd50= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd50.ExtDepth=ED50;			
			max=0;min=10000;
			
			
        }        
//===============================================================================================================================
        protected override void NewBar()
        {
			 _wprInd50.ReInit();
//======================================================================================================================================
			if( _wprInd50.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    if(zzd5!=0 && zzd4!=0 && zzd3!=0 && zzd2!=0 && zzd1!=0) torg=true; else torg=false;
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd50.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
			
//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
					ri5=ri4; ri4=ri3; ri3=ri2; ri2=ri1; ri1=(zzd1-zzd2);
					if(torg) {
					if(max<ri1) max=ri1; if(min>ri1) min=ri1;
					av=(ri1+ri2+ri3+ri4+ri5)/5;
					XXPrint("ВЕРХ {0} {1} {2} {3} {4} {5} --- {6}",this.Instrument.Name,ED50,max,min,max-min,av,ri1);
					Print("{0} = {1}-{2}",zzd1-zzd2,max,min);
					}
				}				
//====== ВНИЗУ ПИК ======================================================================================================================				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
					ri5=ri4; ri4=ri3; ri3=ri2; ri2=ri1; ri1=zzd1-zzd2;
					if(torg) {
					if(max<ri1) max=ri1; if(min>ri1) min=ri1;
					av=(ri1+ri2+ri3+ri4+ri5)/5;
					XXPrint("НИЗ {0} {1} {2} {3} {4} {5} --- {6}",this.Instrument.Name,ED50,max,min,max-min,av,ri1);
					Print("{0} = {1}-{2}",zzd1-zzd2,max,min);}
				}
//===============================================================================================================================  	
			}

        }
//===============================================================================================================================   
		protected void XXPrint(string xxformat, params object[] parameters)
		{var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);}
//==================================================================================================================================
		
    }
}
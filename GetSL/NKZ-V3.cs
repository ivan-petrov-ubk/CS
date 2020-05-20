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
    [TradeSystem("NKZ-V3")]           //copy of "NKZ-123_V2"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Время :")]
        public DateTime dt1 { get; set; }
		
		[Parameter("Buy:", DefaultValue = false)]
        public bool tu { get; set; }	
		[Parameter("Sell:", DefaultValue = false)]
        public bool td { get; set; }	
		[Parameter("StopLoss:", DefaultValue = 250)]
        public int SL1 { get; set; }	
		[Parameter("Отступ Stop :", DefaultValue = 30)]
		public int dl { get;set; }	
		[Parameter("Fractal", DefaultValue = 7)]
		public int frac { get;set; }	
		
		private int NKZ,i,mgS,mgB;
		private bool first,t1,t2,per;
		public int iFT=0,k=0,ks=0;
		private double nkz2,nkz4,nkz2v,nkz4v,kf,zmax,zmin;

		// private PolyLine toolPolyLine;
		private Rectangle toolRectangle;
		private Rectangle toolRectangle1;	
		private Rectangle toolRectangle2;
		
		private DateTime dt0; 
	
		public DateTime DTime; // Время
		private int ci = 0;
		private bool FsU,FsD,nu,nd;		
		private double dlt,frUp,frDown,frUp0,frDown0;
		public Fractals _frInd;		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private ZigZag _wprInd;
		private double zz1=2,zz2=2,zz3=2;
		private int    zzt1,zzt2,zzt3,zzt4,zzt5,zzt6;
		private double zzd1,zzd2,zzd3,zzd4,zzd5,zzd6;
		private int    zzi1,zzi2,zzi3,zzi4,zzi5,zzi6;
		private VerticalLine vy,vb;		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		private string trueLogPath = "";	
		
        protected override void Init()
        {	
			k=0; // Для першого входження -
			kf=0.090909;
			dlt=dl*Instrument.Point; // Отступ от стопа
			dt1=dt1.AddHours(-3); // переход на время UTC
			iFT = TimeToIndex(dt1, Timeframe); // Индекс свечипоследнего
			InitLogFile();  // Запись логов
			// Прямоугольники 
			toolRectangle = Tools.Create<Rectangle>(); toolRectangle.BorderColor=Color.Aqua; toolRectangle.Color=Color.DarkSeaGreen;toolRectangle.BorderColor=Color.Blue;
			toolRectangle1 = Tools.Create<Rectangle>(); toolRectangle1.BorderColor=Color.Aqua; toolRectangle1.Color=Color.DarkSeaGreen;

			//_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			//first=true; 
			//_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			//_wprInd.ExtDepth=5;				
			// 15/05/2018
			if (Instrument.Name == "EURUSD") { NKZ=462;  mgB=101; mgS=201; }
			if (Instrument.Name == "GBPUSD") { NKZ=792;  mgB=102; mgS=202; }
			if (Instrument.Name == "AUDUSD") { NKZ=343;  mgB=103; mgS=203; }
			if (Instrument.Name == "NZDUSD") { NKZ=357;  mgB=104; mgS=204; }
			if (Instrument.Name == "USDJPY") { NKZ=519;  mgB=105; mgS=205; }
			if (Instrument.Name == "USDCAD") { NKZ=504;  mgB=106; mgS=206; }
			if (Instrument.Name == "USDCHF") { NKZ=593;  mgB=107; mgS=207; }
			if (Instrument.Name == "AUDJPY") { NKZ=550;  mgB=108; mgS=208; }
			if (Instrument.Name == "AUDNZD") { NKZ=412;  mgB=109; mgS=209; }
			if (Instrument.Name == "CHFJPY") { NKZ=1430; mgB=110; mgS=210; }
			if (Instrument.Name == "EURAUD") { NKZ=682;  mgB=111; mgS=211; }
			if (Instrument.Name == "AUDCAD") { NKZ=357;  mgB=112; mgS=212; }
			if (Instrument.Name == "EURCAD") { NKZ=762;  mgB=113; mgS=213; }
			if (Instrument.Name == "EURCHF") { NKZ=539;  mgB=114; mgS=214; }
			if (Instrument.Name == "EURGBP") { NKZ=484;  mgB=115; mgS=215; }
			if (Instrument.Name == "EURJPY") { NKZ=715;  mgB=116; mgS=216; }
			if (Instrument.Name == "GBPCHF") { NKZ=924;  mgB=117; mgS=217; }
			if (Instrument.Name == "GBPJPY") { NKZ=1045; mgB=118; mgS=218; }
			
			var posActiveMineB = Trade.GetActivePositions(mgB, true);
			if(posActiveMineB!=null && posActiveMineB.Length>0) posGuidBuy=posActiveMineB[0].Id; 
			var posActiveMineS = Trade.GetActivePositions(mgS, true);
			if(posActiveMineS!=null && posActiveMineS.Length>0) posGuidSell=posActiveMineS[0].Id; 			
        }        
//===========================================================================
        protected override void NewBar()
        {   
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
	
//=== КОРЕКЦИЯ =====================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed)   { posGuidBuy=Guid.Empty; ks=0;  }   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) { posGuidSell=Guid.Empty; ks=0; } 
//=== Трелинг  =======================================================================================================================	
			// if(ks>frac) TrailActiveOrders();
			//TrailActiveOrders();
			//if (posGuidSell!=Guid.Empty && frUp0>frUp )  TrailActiveOrders();
			//if (posGuidBuy!=Guid.Empty && frDown0<frDown) TrailActiveOrders();
//===========================================================================			
          if(k==1) 
		  { 
			nu=false; nd=false;

			if(tu) { zmax = Bars[iFT].High;  
				     nkz4 = zmax-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				     nkz2 = zmax-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			         nkz4v= zmax-((NKZ-5)*Instrument.Point); 
				     nkz2v= zmax-((NKZ*2)*Instrument.Point);
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 				
                   }
			
			if(td) { zmin = Bars[iFT].Low;
				     nkz4 = zmin+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				     nkz2 = zmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					 nkz4v= zmin+((NKZ-5)*Instrument.Point);  
				     nkz2v= zmin+(((NKZ*2))*Instrument.Point);
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale);  
  				   }
	  
			toolRectangle.Point1=new ChartPoint(Bars[iFT].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz4v);

		    toolRectangle1.Point1=new ChartPoint(Bars[iFT].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz2v);  
//===========================================================================      		
		  } 
		  if(k>1) {
		  if (tu && _frInd.TopSeries[Bars.Range.To-5]>0 && Bars[Bars.Range.To-5].High>zmax) { 
			  		 zmax=Bars[Bars.Range.To-5].High;
		  			 nkz4 = zmax-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				     nkz2 = zmax-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			         nkz4v= zmax-((NKZ-5)*Instrument.Point); 
				     nkz2v= zmax-((NKZ*2)*Instrument.Point);
			         nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			         nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			         nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			         nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 

			toolRectangle.Point1=new ChartPoint(Bars[Bars.Range.To-5].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz4v);
			 nd=false; nu=false; 			  
		    toolRectangle1.Point1=new ChartPoint(Bars[Bars.Range.To-5].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz2v);   
		  }
		  
		  if (td && _frInd.BottomSeries[Bars.Range.To-5]>0 && Bars[Bars.Range.To-5].Low<zmin ) { 
			  		zmin=Bars[Bars.Range.To-5].Low;
		  			nkz4 = zmin+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				    nkz2 = zmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					nkz4v= zmin+((NKZ-5)*Instrument.Point);  
				    nkz2v= zmin+(((NKZ*2))*Instrument.Point);
			        nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			        nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			        nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			        nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 

			toolRectangle.Point1=new ChartPoint(Bars[Bars.Range.To-5].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz4v);
			 nd=false; nu=false; 			  
		    toolRectangle1.Point1=new ChartPoint(Bars[Bars.Range.To-5].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz2v); 	  
		 				 }
 					 }		
			//== Касание зоны/1\4 ===============================
			if (tu && Bars[ci].Low<nkz4)  nu=true;
			if (td && Bars[ci].High>nkz4) nd=true;

			if (posGuidBuy!=Guid.Empty || posGuidSell!=Guid.Empty) ks++;
		    k++;
			XXPrint("{0} {1} {2} {3} {4} {5} {6}",Bars[ci].Time,nkz4,nkz4v,nkz2,nkz2v,zmax,zmin);
        }
//===============================================================================================================================   
		protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { var tr = Trade.UpdateMarketPosition(posGuidBuy,	  getSLfr(1),null," - update TP,SL"); }
		  if(posGuidSell!=Guid.Empty) { var tr = Trade.UpdateMarketPosition(posGuidSell,  getSLfr(0),null," - update TP,SL");  }
		} 		  
	    protected double getSLfr(int type)
	    {
			switch(type)
			{   case 0: { return Math.Round(frUp+dlt+Instrument.Spread, Instrument.PriceScale); }
				case 1: { return Math.Round(frDown+dlt+Instrument.Spread, Instrument.PriceScale); }
			default:  break;
						}
			return 0.0;		
		}		
		
		protected double getSL(int type)
		{
			switch(type)
			{   case 0: {   double MAX = double.MinValue;
							for(int i = 0; i < frac; i++)
							{ if(Bars[ci - i].High > MAX)
									MAX = Bars[ci - i].High; 
							}	
							return Math.Round(MAX+dlt+Instrument.Spread, Instrument.PriceScale);
						}
				case 1: {   double MIN = double.MaxValue;
							for(int i = 0; i < frac; i++)
							{  if(Bars[ci - i].Low < MIN)
									MIN = Bars[ci - i].Low; 
							}	
							return Math.Round(MIN-dlt-Instrument.Spread, Instrument.PriceScale);
						}
				default:  break;
			}
			return 0.0;
		}
		protected void InitLogFile()
		{trueLogPath=PathToLogFile+"\\"+Instrument.Name.ToString()+".log";}
		
		protected void XXPrint(string xxformat, params object[] parameters)
		{
				var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);
		}
    }
}
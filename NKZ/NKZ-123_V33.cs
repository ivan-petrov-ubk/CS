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
    [TradeSystem("NKZ-123_V3")]           //copy of "NKZ-123_V2"
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
		[Parameter("Zona 1-4", DefaultValue = true)]
		public bool n4 { get;set; }	
		
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
		
		public double frU1,frU2,frU3,frU4,frU5;   // Значение текущего верхнего Fractal
		public double frD1,frD2,frD3,frD4,frD5;    // Значение Low - свечи с верхним фрактклом
		public double fsU1,fsU2,fsU3,fsU4,fsU5;    // Значение Low - свечи с верхним фрактклом
		public double fsD1,fsD2,fsD3,fsD4,fsD5;    // Значение Low - свечи с верхним фрактклом
		public double TPD,TPU,SLU,SLD;
		public double PRD,PRU,PCU,PCD,UBU,UBD;		
		public int PRDi,PRUi,UBDi,UBUi;
		public DateTime tmU1,tmU2,tmU3,tmD1,tmD2,tmD3;
		public bool frU,frD;
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

			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			first=true; 
		
			// 25/08/2018
			if (Instrument.Name == "EURUSD") { NKZ=506;  mgB=101; mgS=201; }
			if (Instrument.Name == "GBPUSD") { NKZ=660;  mgB=102; mgS=202; }
			if (Instrument.Name == "AUDUSD") { NKZ=343;  mgB=103; mgS=203; }
			if (Instrument.Name == "NZDUSD") { NKZ=330;  mgB=104; mgS=204; }
			if (Instrument.Name == "USDJPY") { NKZ=544;  mgB=105; mgS=205; }
			if (Instrument.Name == "USDCAD") { NKZ=525;  mgB=106; mgS=206; }
			if (Instrument.Name == "USDCHF") { NKZ=553;  mgB=107; mgS=207; }
			if (Instrument.Name == "AUDJPY") { NKZ=550;  mgB=108; mgS=208; }
			if (Instrument.Name == "AUDNZD") { NKZ=412;  mgB=109; mgS=209; }
			if (Instrument.Name == "CHFJPY") { NKZ=1430; mgB=110; mgS=210; }
			if (Instrument.Name == "EURAUD") { NKZ=682;  mgB=111; mgS=211; }
			if (Instrument.Name == "AUDCAD") { NKZ=357;  mgB=112; mgS=212; }
			if (Instrument.Name == "EURCAD") { NKZ=762;  mgB=113; mgS=213; }
			if (Instrument.Name == "EURCHF") { NKZ=627;  mgB=114; mgS=214; }
			if (Instrument.Name == "EURGBP") { NKZ=484;  mgB=115; mgS=215; }
			if (Instrument.Name == "EURJPY") { NKZ=781;  mgB=116; mgS=216; }
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
//====  Fractal ====================================================================================
			  if (_frInd.TopSeries[Bars.Range.To-5]>0) 		{				
				  frU5=frU4; frU4=frU3; frU3=frU2; frU2=frU1; frU1=_frInd.TopSeries[Bars.Range.To-5];
				  tmU3=tmU2; tmU2=tmU1; tmU1=Bars[Bars.Range.To-5].Time; frUp=Bars[Bars.Range.To-5].High; }
			  if (_frInd.BottomSeries[Bars.Range.To-5]>0)   {			    
				     frD5=frD4; frD4=frD3; frD3=frD2; frD2=frD1; frD1=_frInd.BottomSeries[Bars.Range.To-5];  
				     tmD3=tmD2; tmD2=tmD1; tmD1=Bars[Bars.Range.To-5].Time; frDown=Bars[Bars.Range.To-5].Low; } 			
//=== КОРЕКЦИЯ =====================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed)   { posGuidBuy=Guid.Empty; ks=0;  }   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) { posGuidSell=Guid.Empty; ks=0; } 
		
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
				 Print("{0} - nkz2={1} nkz4={2} - 1U",Bars[Bars.Range.To-1].Time,nkz2,nkz4);
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
				 Print("{0} - nkz2={1} nkz4={2} - 1D",Bars[Bars.Range.To-1].Time,nkz2,nkz4);
  				   }
	  
			toolRectangle.Point1=new ChartPoint(Bars[iFT].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz4v);

		    toolRectangle1.Point1=new ChartPoint(Bars[iFT].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz2v);  
//===========================================================================      		
		  } 
		  if(k>1) {
		  if (tu && Bars[Bars.Range.To-1].High>zmax) { 
			  		 zmax=Bars[Bars.Range.To-1].High;
		  			 nkz4 = zmax-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				     nkz2 = zmax-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			         nkz4v= zmax-((NKZ-5)*Instrument.Point); 
				     nkz2v= zmax-((NKZ*2)*Instrument.Point);
			         nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			         nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			         nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			         nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
 Print("{0} - nkz2={1} nkz4={2} - 2U",Bars[Bars.Range.To-1].Time,nkz2,nkz4);
			toolRectangle.Point1=new ChartPoint(Bars[Bars.Range.To-5].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz4v);
			 nd=false; nu=false; 			  
		    toolRectangle1.Point1=new ChartPoint(Bars[Bars.Range.To-5].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz2v);   
		  }
		  
		  if (td && Bars[Bars.Range.To-1].Low<zmin ) { 
			  		zmin=Bars[Bars.Range.To-1].Low;
		  			nkz4 = zmin+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				    nkz2 = zmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					nkz4v= zmin+((NKZ-5)*Instrument.Point);  
				    nkz2v= zmin+(((NKZ*2))*Instrument.Point);
			        nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			        nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			        nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			        nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
 Print("{0} - nkz2={1} nkz4={2} - 2D",Bars[Bars.Range.To-1].Time,nkz2,nkz4);
			toolRectangle.Point1=new ChartPoint(Bars[Bars.Range.To-1].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(48), nkz4v);
			 nd=false; nu=false; 			  
		    toolRectangle1.Point1=new ChartPoint(Bars[Bars.Range.To-1].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(48), nkz2v); 	  
		 				 }
 					 }		
			//== Касание зоны/1\4 ===============================
		  if(n4) {
			if (tu && Bars[ci].Low<nkz4 && !nu)  { nu=true; Print("{0} -Перетин 1/4 ВНИЗ",Bars[Bars.Range.To-1].Time); }
			if (td && Bars[ci].High>nkz4 && !nd) { nd=true; Print("{0} -Перетин 1/4 ВВЕРХ",Bars[Bars.Range.To-1].Time); }
                  } 
			if (tu && Bars[ci].Low<nkz2 && !nu)  { nu=true;  Print("{0} -Перетин 1/2 ВНИЗ",Bars[Bars.Range.To-1].Time);}
			if (td && Bars[ci].High>nkz2 && !nd) { nd=true;  Print("{0} -Перетин 1/2 ВВЕРХ",Bars[Bars.Range.To-1].Time);}
		  
		  
			if (posGuidBuy!=Guid.Empty || posGuidSell!=Guid.Empty) ks++;
		    k++;
			
			if(nu && tu) 
			{
// Определение патерна				
		   // ПИК ВНИЗУ - BUY
		   if (_frInd.BottomSeries[Bars.Range.To-5]>0)    
		   { 
           		if( frU3>frU2 && frU1>frU2 ) {
					TPU=zmax;
					SLU=frD2-Instrument.Spread; SLU=Math.Round(SLU, Instrument.PriceScale);
					PRU=Math.Round((TPU-Bars[Bars.Range.To-1].Close)*Instrument.LotSize,0);
					UBU=Math.Round((Bars[Bars.Range.To-1].Close-SLU)*Instrument.LotSize,0);
					PCU=Math.Round(PRU/UBU, 2);
					Print("BUY -- {0} -- PRU={1} UBU={2} PCU={3}",Bars[Bars.Range.To-1].Time,PRU,UBU,PCU);
					PRUi=(int)PRU;
					UBUi=(int)UBU;
					if(UBU>50 && UBU<200 && PCU>1) {
                    Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								Stops.InPips(UBUi,PRUi), null, null);
														
			    	frD=false; var vr2=Tools.Create<VerticalLine>(); vr2.Color=Color.Red; vr2.Time=Bars[Bars.Range.To-1].Time;vr2.Width=4;}
				else 	{frD=false; var vr2=Tools.Create<VerticalLine>(); vr2.Color=Color.Red; vr2.Time=Bars[Bars.Range.To-1].Time; }
				}  
		   }
				
			}
			
			if(nd && td) 
			{
			//ПИК ВВЕРХУ - SELL
			if (_frInd.TopSeries[Bars.Range.To-5]>0)	   { 

				
           		if( frD2>frD3 && frD2>frD1  ) 
				{ 
					TPD=zmin; TPD=Math.Round(TPD, Instrument.PriceScale);
					SLD=frU2+Instrument.Spread; SLD=Math.Round(SLD, Instrument.PriceScale);
					PRD=Math.Round((Bars[Bars.Range.To-1].Close-TPD)*Instrument.LotSize,0);
					PRDi=(int)PRD;
					UBDi=(int)UBD;
					UBD=Math.Round((SLD-Bars[Bars.Range.To-1].Close)*Instrument.LotSize,0);
					PCD=Math.Round(PRD/UBD, 2);
					Print("SELL -- {0} -- PRD={1} UBD={2} PCD={3}",Bars[Bars.Range.To-1].Time,PRD,UBD,PCD);
					if(UBD>50 && UBD<200 && PCD>1) {
					Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,	Stops.InPips(UBDi,PRDi), null, null);    						
									
				    frU=false; var vr1=Tools.Create<VerticalLine>(); vr1.Color=Color.Blue; vr1.Time=Bars[Bars.Range.To-1].Time;vr1.Width=4;}
					else { var vr1=Tools.Create<VerticalLine>(); vr1.Color=Color.Blue; vr1.Time=Bars[Bars.Range.To-1].Time;}} 
			}
				
			}
			XXPrint("{0} {1} {2} {3} {4} {5} {6}",Bars[ci].Time,nkz4,nkz4v,nkz2,nkz2v,zmax,zmin);
        }
//===============================================================================================================================   

		protected void InitLogFile()
		{trueLogPath=PathToLogFile+"\\"+Instrument.Name.ToString()+".log";}
		
		protected void XXPrint(string xxformat, params object[] parameters)
		{
				var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);
		}
    }
}
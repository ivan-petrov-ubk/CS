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
    [TradeSystem("Lo73")]         //copy of "N10-ZZ"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Время :")]
        public DateTime dt1 { get; set; }
		[Parameter("StopLoss:", DefaultValue = 200)]
        public int SL1 { get; set; }		
		[Parameter("Buy:", DefaultValue = false)]
        public bool tu { get; set; }	
		[Parameter("Sell:", DefaultValue = false)]
        public bool td { get; set; }			
		[Parameter("Отступ Stop :", DefaultValue = 20)]
		public int dl { get;set; }	
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private Guid posGuidBuy2=Guid.Empty;
		private Guid posGuidSell2=Guid.Empty;		
		private double  zzu1,zzu2;
		private int zzi1,zzi2,zzi3,zzu,zzd;
		private int NKZ,mgS,mgB,i;
		private bool first,t1,t2,per;
		private int iFT;
		private double nkz2,nkz4,nkz2v,nkz4v,kf=0.090909,zmax,zmin;
		private double slB,slS,cBS,cSS;
		private PolyLine toolPolyLine;
		private Rectangle toolRectangle;
		private Rectangle toolRectangle1;		
		private DateTime dt0; 
		private double zz1=2,zz2=2,zz3=2;
		private double zzd1,zzd2,zzd3,zzd4,zzd5;
		
		private ZigZag _wprInd;		
		public FisherTransformOscillator _ftoInd;


		public DateTime DTime; // Время
		private int ci = 0,frac;
		private bool FsU,FsD,nu,nd;		
		private double dlt,frUp,frDown;
		
        protected override void Init()
        {	dlt=dl*Instrument.Point;
			nu=false; nd=false; frac=7;
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);

			toolPolyLine = Tools.Create<PolyLine>(); toolPolyLine.Color=Color.Aqua; 
			toolRectangle = Tools.Create<Rectangle>(); toolRectangle.BorderColor=Color.Aqua; toolRectangle.Color=Color.DarkSeaGreen;
			toolRectangle1 = Tools.Create<Rectangle>(); toolRectangle1.BorderColor=Color.Aqua; toolRectangle1.Color=Color.DarkSeaGreen;
			
			zmax=0.0; zmin=1000.0;
			per=false;
		    first=true; t1=true; t2=true;
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=5;			
			// 15/05/2018
			if (Instrument.Name == "EURUSD") { NKZ=462;  mgB=101; mgS=201; }
			if (Instrument.Name == "GBPUSD") { NKZ=792;  mgB=102; mgS=202; }
			if (Instrument.Name == "AUDUSD") { NKZ=343;  mgB=103; mgS=203; }
			if (Instrument.Name == "NZDUSD") { NKZ=357;  mgB=104; mgS=204; }
			if (Instrument.Name == "USDJPY") { NKZ=527;  mgB=105; mgS=205; }
			if (Instrument.Name == "USDCAD") { NKZ=491;  mgB=106; mgS=206; }
			if (Instrument.Name == "USDCHF") { NKZ=608;  mgB=107; mgS=207; }
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
        {   //_wprInd.ReInit();
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
//===========================================================================			
          if(first) 
		  { 
			first=false;
			  
			iFT = TimeToIndex(dt1, Timeframe);
			  
			if(tu) { nkz4 =Bars[iFT].High-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				     nkz2 =Bars[iFT].High-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			         nkz4v=Bars[iFT].High-((NKZ-5)*Instrument.Point); 
				     nkz2v=Bars[iFT].High-((NKZ*2)*Instrument.Point);
                   }
			if(td) { nkz4 =Bars[iFT].Low+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				     nkz2 =Bars[iFT].Low+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					 nkz4v=Bars[iFT].Low+((NKZ-5)*Instrument.Point);  
				     nkz2v=Bars[iFT].Low+(((NKZ*2))*Instrument.Point);
			       }
 
			nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			nkz2v=Math.Round(nkz2v,Instrument.PriceScale);  
			
		     zmax = Bars[iFT].High; 
			 zmin = Bars[iFT].Low;
			
			toolPolyLine = Tools.Create<PolyLine>(); toolPolyLine.Color=Color.Aqua; 
			toolRectangle = Tools.Create<Rectangle>(); toolRectangle.BorderColor=Color.Aqua; toolRectangle.Color=Color.DarkSeaGreen;
			toolRectangle1 = Tools.Create<Rectangle>(); toolRectangle1.BorderColor=Color.Aqua; toolRectangle1.Color=Color.DarkSeaGreen;
			  
			toolPolyLine.AddPoint(new ChartPoint(Bars[iFT].Time, Bars[iFT].High));
			toolPolyLine.AddPoint(new ChartPoint(Bars[iFT].Time, nkz2v));
			  
			toolRectangle.Point1=new ChartPoint(Bars[iFT].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz4v);
			  
			toolRectangle1.Point1=new ChartPoint(Bars[iFT].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz2v);  
			Print("First - dt1={0} iFT={1}",dt1,iFT);
      		
		  }
//=== КОРЕКЦИЯ ====================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty; 
//=== Трелинг  =======================================================================================================================	
			TrailActiveOrders();			
//=== Сигналы =====================================================================
			if(_ftoInd.FisherSeries[Bars.Range.To-2]<0 && _ftoInd.FisherSeries[Bars.Range.To-1]>0) FsU=true; else FsU=false;
			if(_ftoInd.FisherSeries[Bars.Range.To-2]>0 && _ftoInd.FisherSeries[Bars.Range.To-1]<0) FsD=true; else FsD=false;	
			//== Касание зоны 1\4 ===============================
			if (tu && Bars[ci].Low<nkz4)  nu=true;
			if (td && Bars[ci].High>nkz4) nd=true;
			//===== Зигзаг  =====================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				    if(zz3<zz2 && zz2>zz1) { zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2; } // ВВЕРХУ
				    if(zz3>zz2 && zz2<zz1) { zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2; } // ВНИЗУ
			}		
//=== Торговля ====================================================================		
	 
					if (posGuidBuy==Guid.Empty && tu && FsU && nu && Bars[ci].Low>nkz2) { 		
						     var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								             Stops.InPips(SL1,null),null,mgB);
						     if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id;
						} 
			   			
					if (posGuidSell==Guid.Empty && td && FsD && nd && Bars[ci].High<nkz2) {		
							 var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                 Stops.InPips(SL1,null),null,mgS); 
						     if (result207.IsSuccessful)  posGuidSell=result207.Position.Id;
						}
					
        }
//===============================================================================================================================   
		protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { var tr = Trade.UpdateMarketPosition(posGuidBuy,	  getSL(1),null," - update TP,SL"); }
		  if(posGuidSell!=Guid.Empty) { var tr = Trade.UpdateMarketPosition(posGuidSell,  getSL(0),null," - update TP,SL");  }
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

    }
}
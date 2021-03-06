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
    [TradeSystem("N6")]     //copy of "N5"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Время :")]
        public DateTime dt1 { get; set; }
	
		[Parameter("Buy:", DefaultValue = false)]
        public bool tu { get; set; }	
		[Parameter("Sell:", DefaultValue = false)]
        public bool td { get; set; }			

		
		private ZigZag _wprInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private Guid posGuidBuy2=Guid.Empty;
		private Guid posGuidSell2=Guid.Empty;		
		private double  zzu1,zzu2,zzd1,zzd2;
		private double   zz1=2,zz2=2,zz3=2;
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
		
        protected override void Init()
        {
			
			// vl = Tools.Create<VerticalLine>(); vl.Color=Color.Aqua; vl.Width=3;	
			toolPolyLine = Tools.Create<PolyLine>(); toolPolyLine.Color=Color.Aqua; 
			toolRectangle = Tools.Create<Rectangle>(); toolRectangle.BorderColor=Color.Aqua; toolRectangle.Color=Color.DarkSeaGreen;
			toolRectangle1 = Tools.Create<Rectangle>(); toolRectangle1.BorderColor=Color.Aqua; toolRectangle1.Color=Color.DarkSeaGreen;
			
			zmax=0.0; zmin=1000.0;
			per=false;
		    first=true; t1=true; t2=true;
			
			if (Instrument.Name == "EURUSD") { NKZ=462;  mgB=101; mgS=201; }
			if (Instrument.Name == "GBPUSD") { NKZ=792;  mgB=102; mgS=202; }
			if (Instrument.Name == "AUDUSD") { NKZ=343;  mgB=103; mgS=203; }
			if (Instrument.Name == "NZDUSD") { NKZ=357;  mgB=104; mgS=204; }
			if (Instrument.Name == "USDJPY") { NKZ=501;  mgB=105; mgS=205; }
			if (Instrument.Name == "USDCAD") { NKZ=500;  mgB=106; mgS=206; }
			if (Instrument.Name == "USDCHF") { NKZ=557;  mgB=107; mgS=207; }
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

			var posPendingMineB = Trade.GetPendingPositions(mgB, true);
			if(posPendingMineB!=null && posPendingMineB.Length>0) posGuidBuy=posPendingMineB[0].Id; 
			var posPendingMineS = Trade.GetPendingPositions(mgS, true);
			if(posPendingMineS!=null && posPendingMineS.Length>0) posGuidSell=posPendingMineS[0].Id;

        }        
//===============================================================================================================================
        protected override void NewBar()
        {   
			
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
		     zmax = Bars[iFT].High; 
			 zmin = Bars[iFT].Low;
      		
		  }

		  if(dt0!=dt1) {
		  Tools.Remove(toolPolyLine);
		  Tools.Remove(toolRectangle);
		  Tools.Remove(toolRectangle1);
			  
			nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			nkz2v=Math.Round(nkz2v,Instrument.PriceScale);  

//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
		
				    if(posGuidBuy==Guid.Empty && tu){
						  var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, 
							           nkz4, 0, 
							           Stops.InPrice(200,null), null, null, mgB);	
						     if (result.IsSuccessful)  posGuidBuy=result.Position.Id; }
					
					 if(posGuidSell==Guid.Empty && td){
							var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  
							               nkz4, 0, 
							               Stops.InPrice(200,null), null, null, mgS);
						        if (result1.IsSuccessful)  posGuidSell=result1.Position.Id; }
					
					
			toolPolyLine = Tools.Create<PolyLine>(); toolPolyLine.Color=Color.Aqua; 
			toolRectangle = Tools.Create<Rectangle>(); toolRectangle.BorderColor=Color.Aqua; toolRectangle.Color=Color.DarkSeaGreen;
			toolRectangle1 = Tools.Create<Rectangle>(); toolRectangle1.BorderColor=Color.Aqua; toolRectangle1.Color=Color.DarkSeaGreen;
			  
			toolPolyLine.AddPoint(new ChartPoint(Bars[iFT].Time, Bars[iFT].High));
			toolPolyLine.AddPoint(new ChartPoint(Bars[iFT].Time, nkz2v));
			  
			toolRectangle.Point1=new ChartPoint(Bars[iFT].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz4v);
			  
			toolRectangle1.Point1=new ChartPoint(Bars[iFT].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz2v);  
			  
			  dt0=dt1;}


        }
       
    }
}
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
    [TradeSystem("N5")]    //copy of "N1"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Время :")]
        public DateTime dt1 { get; set; }
	
		[Parameter("Buy:", DefaultValue = false)]
        public bool tu { get; set; }	
		[Parameter("Sell:", DefaultValue = false)]
        public bool td { get; set; }			
		
		[Parameter("ExtDepth:", DefaultValue = 15)]
        public int ED { get; set; }	
		
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
		private double nkz2,nkz4,kf=0.090909,zmax,zmin;
		private double slB,slS,cBS,cSS;
		
        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
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
			
			//iFT = TimeToIndex(dt2, Timeframe);
			//Print("{0} - {1} - {2} - {3}",Bars[iFT].Time,Bars[iFT].High,dt2,iFT);
				//Print("Start - {0} - NKZ={1} mgB={2} mgS={3}",Instrument.Name,NKZ,mgB,mgS);
        }        
//===============================================================================================================================
        protected override void NewBar()
        {   
			_wprInd.ReInit();
          if(first) {
			iFT = TimeToIndex(dt1, Timeframe);
			if(tu) { nkz4=Bars[iFT].High-((NKZ-5-(NKZ*kf))*Instrument.Point); nkz2=Bars[iFT].High-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);}
			if(td) { nkz4=Bars[iFT].Low+((NKZ-5-(NKZ*kf))*Instrument.Point);  nkz2=Bars[iFT].Low+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);}
		     zmax = Bars[iFT].High; zmin=Bars[iFT].Low;
			// if(td) { nkz4=Bars[iFT].Low+((NKZ)*Instrument.Point); nkz2=Bars[iFT].Low+(((NKZ*2))*Instrument.Point);}
      		// Print("2 -- {0} - {1} - {2} - {3} - {4}",Bars[iFT].Time,nkz4,nkz2,NKZ,Instrument.Point);
		  }
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
		
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{  
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				
//============================== ZIGZAG ВЕРШИНА ВВЕРХУ ==========================================			 
				if(zz3<zz2 && zz2>zz1)  
				{
					zzu2=zzu1; zzu1=zz2; 
					// Тренд ВНИЗ вершина ВВЕРХУ і нижче попередньої і активний - міняємо СТОПЛОС
					if(td && zzu1<zzu2) 
					{   slS=zzu1;
					    if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active){
			 			   var result2=Trade.UpdateMarketPosition(posGuidSell, slS, null); 
					       if (result2.IsSuccessful) posGuidSell = result2.Position.Id;  }
					}
					// Тренд ВНИЗ вершина ВВЕРХУ і вище попередньої - міняємо СТОПЛОС
					if(td && zzu1>zzu2) 
					{   slS=zzu1;
					    if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending){
			 			   var result2=Trade.UpdatePendingPosition(posGuidSell, 0.1, cSS, slS, null); 
					       if (result2.IsSuccessful) posGuidSell = result2.Position.Id;  }
					}
					// Тренд ВВЕРХ вершина ВВЕРХУ і НИЖЧЕ попередньої - переносимо BuyStop
					if(tu && zzu1<zzu2)
					{   cBS=zzu1; 
						if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending){
					     var result3 = Trade.UpdatePendingPosition(posGuidBuy,0.1,cBS,slB,null); 
				       	     if (result3.IsSuccessful) posGuidBuy = result3.Position.Id;}						
					}
					// Тренд ВВЕРХ і новий МАКС від попереднього НКЗ - вираховуємо нові рівні
					if(tu && zz2>zmax) { 
						zmax=zz2; 
						nkz4=zmax-((NKZ-5-(NKZ*kf))*Instrument.Point); 
						nkz2=zmax-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					                    }
				}
				
//======================== ZIGZAG ВЕРШИНА ВНИЗУ ==========================================
				if(zz3>zz2 && zz2<zz1)  
				{  
					zzd2=zzd1; zzd1=zz2;
					// Тренд ВВЕРХ вершина ВНИЗУ і вище попередньої і активний - міняємо СТОПЛОС
					if(tu && zzd1>zzd2) 
					{   slB=zzd1;
					    if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active){
			 			   var result2=Trade.UpdateMarketPosition(posGuidBuy, slB, null); 
					       if (result2.IsSuccessful) posGuidSell = result2.Position.Id;  }
					}
					// тренд ВВЕРХ а вершина ВНИЗУ и ниже предыдущей - переносим стоп от BuyStop
					if(tu && zzd1<zzd2) 
					{	slB=zzd1;
						if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending){
					     var result3 = Trade.UpdatePendingPosition(posGuidBuy,0.1,cBS,slB,null); 
				       	     if (result3.IsSuccessful) posGuidBuy = result3.Position.Id;}
					}  
					// Тренд ВНИЗ вершина ВНИЗУ і ВИЩЕ попередньої - переносимо SellStop
					if(td && zzd1>zzd2) 
					{  cSS=zzd1;
					    if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending){
			 			   var result2=Trade.UpdatePendingPosition(posGuidSell, 0.1, cSS, slS, null); 
					       if (result2.IsSuccessful) posGuidSell = result2.Position.Id;  }						
					}
					// тренд ВНИЗ и новая вершина ВНИЗУ и ниже предыдущего минимума - вычисляем НКЗ от него
					if(td && zz2<zmin) { 
						zmin=zz2;
						nkz4=zmin+((NKZ-5-(NKZ*kf))*Instrument.Point); 
						nkz2=zmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					                     }
				}
				
			}				

// =============  Пересечнние линии НКЗ вниз когда тренд ВВЕРХ =================================================================
			Print("{0} - Low={1} nkz2={2} tu={3}  -- {4}",Bars[Bars.Range.To-1].Time,Bars[Bars.Range.To-1].Low,nkz4, tu,Bars[Bars.Range.To-1].Low<nkz4);
				if( Bars[Bars.Range.To-1].Low<nkz4 && tu) 
				{    cBS=zzu1;
				    if(posGuidBuy==Guid.Empty){
						  var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyStop, 0.1, 
							           cBS, 0, 
							           Stops.InPrice(zzd1,null), null, null, mgB);	
						     if (result.IsSuccessful)  posGuidBuy=result.Position.Id; }
				}
// =============  Пересечнние линии НКЗ вверх когда тренд ВНИЗ =================================================================
				if(Bars[Bars.Range.To-1].High>nkz4 && td) 
				{    cSS=zzd1;
					 if(posGuidSell==Guid.Empty && td){
							var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  
							               cSS, 0, 
							               Stops.InPrice(zzu1,null), null, null, mgS);
						        if (result1.IsSuccessful)  posGuidSell=result1.Position.Id; }
				}
//------------------------------------------------------------------------------------------------------------------------------				
        }
       
    }
}
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
    [TradeSystem("N3")]    //copy of "N1"
    public class ZZ_Ex1 : TradeSystem
    {
		
		
		[Parameter("SL:", DefaultValue = 200)]
        public int SL { get; set; }	
		[Parameter("TP:", DefaultValue = 100)]
        public int TP { get; set; }		
		
		[Parameter("Buy:", DefaultValue = true)]
        public bool tu { get; set; }	
		[Parameter("Sell:", DefaultValue = true)]
        public bool td { get; set; }			
		
		private ZigZag _wprInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private Guid posGuidBuy2=Guid.Empty;
		private Guid posGuidSell2=Guid.Empty;		
		private double  zzu1,zzu2,zzd1,zzd2;
		private double   zz1=2,zz2=2,zz3=2;
		private int zzi1,zzi2,zzi3,zzu,zzd;
		private int NKZ,mgS,mgB,i,kd;
		private bool first,t1,t2;
		private int i1=0;
		private double maxd,mind,maxd1,mind1;
		public DateTime DTime,tmaxd,tmind,tmaxd1,tmind1;
		public double H1,L1;
		private double dy=0,cnU=0,cnD=0;
		
        protected override void Init()
        {

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
			
			dy=DTime.DayOfYear;
			
				Print("Start - {0} - NKZ={1} mgB={2} mgS={3}",Instrument.Name,NKZ,mgB,mgS);
        }        
//===============================================================================================================================
        protected override void NewBar()
        {      DTime = Bars[Bars.Range.To-1].Time;
			if(DTime.DayOfWeek==DayOfWeek.Monday) kd=3; else kd=1;
			  
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
	if(first)		
	{  

	   dy=DTime.DayOfYear-kd-1;
		i1=2;
					maxd=0.0;
					mind=10000.0;
						do { i1++; 
					 //Print("{0} - {1} - {2}",Bars[Bars.Range.To-i1].Time.DayOfYear,dy,kd);
  			         if( Bars[Bars.Range.To-i1].Time.Hour<24 &&  
					 Bars[Bars.Range.To-i1].Time.Hour>0 && 
					 Bars[Bars.Range.To-i1].Time.DayOfYear==dy ) 
					     {	//Print("-2день  {0} - maxd={1} mind={2}",Bars[Bars.Range.To-i1].Time,maxd,mind);			
						     if(maxd<Bars[Bars.Range.To-i1].High) { 
							              	maxd=Bars[Bars.Range.To-i1].High; 
							 			  	tmaxd1=Bars[Bars.Range.To-i1].Time;  }
						 
						     if(mind>Bars[Bars.Range.To-i1].Low)  { 
							 				mind=Bars[Bars.Range.To-i1].Low; 
                			  				tmind1=Bars[Bars.Range.To-i1].Time;  }
					     }
				    } while(Bars[Bars.Range.To-i1].Time.DayOfYear!=(dy-1) && i1<1000);
					// Print("-2день {0} - maxd={1} | {3} - mind={2}",tmaxd,maxd,mind,tmind);
	}			
	
	if(first || ( DTime.Hour==01 && DTime.Minute==00 ) || tmaxd1.DayOfYear-kd-1!=dy) 
        {    i=1; first=false;
// Визначення максимума та мінімума попереднього дня ================================================			
			if(dy!=DTime.DayOfYear) 
			{   
				maxd1=maxd; 
				mind1=mind; 
				dy=DTime.DayOfYear;
				    i1=2;
					maxd=0.0;
					mind=10000.0;
// Визначення максимума ==============================================================================				
				do { i1++; 
					 //Print("{0} - {1} - {2}",Bars[Bars.Range.To-i1].Time.DayOfYear,dy,kd);
  			         if( Bars[Bars.Range.To-i1].Time.Hour<24 &&  
					 Bars[Bars.Range.To-i1].Time.Hour>0 && 
					 Bars[Bars.Range.To-i1].Time.DayOfYear==(dy-kd) ) 
					     {	/// Print("-1день {0} - maxd={1} mind={2}",Bars[Bars.Range.To-i1].Time,maxd,mind);			
						     if(maxd<Bars[Bars.Range.To-i1].High) { 
							              	maxd=Bars[Bars.Range.To-i1].High; 
							 			  	tmaxd=Bars[Bars.Range.To-i1].Time;  }
						 
						     if(mind>Bars[Bars.Range.To-i1].Low)  { 
							 				mind=Bars[Bars.Range.To-i1].Low; 
                			  				tmind=Bars[Bars.Range.To-i1].Time;  }
					     }
				    } while(Bars[Bars.Range.To-i1].Time.DayOfYear!=(dy-kd-1) && i1<1000);
					//Print("{4} {0} - maxd={1} | {3} - mind={2}",tmaxd,maxd,mind,tmind,Instrument.Name);
			}
// ====================================================================================================			
			//Print("{0}>{1} {2}<{3}",maxd,maxd1,mind,mind1);
			Print("{4}({7}) {0} - maxd={1} | {8} - maxd1={5} | {3} - mind={2} | {9} - mind1={6}",tmaxd,maxd,mind,tmind,Instrument.Name,maxd1,mind1,Bars[Bars.Range.To-1].Time,tmaxd1,tmind1);
			
			if(maxd>maxd1) {
					     if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending && maxd!=cnU){
					     var result3 = Trade.UpdatePendingPosition(posGuidBuy, 0.1, 
							      maxd-(NKZ*Instrument.Point)-Instrument.Spread,
							      maxd-((NKZ+SL)*Instrument.Point)-Instrument.Spread,
							      maxd-((NKZ-TP)*Instrument.Point)-Instrument.Spread); 
				       	         if (result3.IsSuccessful) { posGuidBuy =  result3.Position.Id; cnU=maxd; } }

						 if(posGuidBuy==Guid.Empty && tu){ 
						 var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, 
							           maxd-(NKZ*Instrument.Point)-Instrument.Spread, 0, 
							           Stops.InPips(SL,TP), null, null, mgB);	
						               if (result.IsSuccessful)  { posGuidBuy=result.Position.Id; cnU=maxd; } }
            }
									
			if(mind<mind1) {	
					 if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending && mind!=cnD){
			 			 var result2=Trade.UpdatePendingPosition(posGuidSell,  0.1,  
							 mind+(NKZ*Instrument.Point)-Instrument.Spread, 
							 mind+((NKZ+SL)*Instrument.Point)-Instrument.Spread,
							 mind+((NKZ-TP)*Instrument.Point)-Instrument.Spread); 
							          if (result2.IsSuccessful) { posGuidSell = result2.Position.Id; cnD=mind; }  }
				
								if(posGuidSell==Guid.Empty && td){ 							
				             var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  
							               mind+(NKZ*Instrument.Point)+Instrument.Spread, 0, 
							               Stops.InPips(SL,TP), null, null, mgS);
						        if (result1.IsSuccessful) { posGuidSell=result1.Position.Id; cnD=mind; } } 
			}
	
		}

        }
    }
}
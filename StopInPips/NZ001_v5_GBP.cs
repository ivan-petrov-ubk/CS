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
    [TradeSystem("NZ001_v5")]               //copy of "NZ001_v4"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("NKZ :")]
        public int dt1 { get; set; }
		
		[Parameter("fr1 :")]
        public int fr1 { get; set; }

		[Parameter("fr2 :")]
        public int fr2 { get; set; }
	
		
		[Parameter("Only NKZ2:", DefaultValue = false)]
        public bool n2 { get; set; }
		[Parameter("Lot:", DefaultValue = 0.1)]
        public double lot { get; set; }
		
		
		[Parameter("TP2:", DefaultValue = 0)]
        public int TP2 { get; set; }		
		[Parameter("StopLoss:", DefaultValue = 250)]
        public int SL1 { get; set; }	
		[Parameter("Отступ Stop :", DefaultValue = 50)]
		public int dl { get;set; }	
		[Parameter("Fractal", DefaultValue = 5)]
		public int frac { get;set; }
		[Parameter("TP/SL:", DefaultValue = 3)]
        public int kof { get; set; }	

	
		private int NKZ,i;

		public int k=0;
		public Fractals _frInd;	
		public DateTime DTime; // Время
		private int ci = 0;
		private string st;
		private double nkz2,nkz4,nkz2v,nkz4v,kf,zmax,zmin;

	    private double dlt,frUp,frDown;
	
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private Guid posGuidBuy2=Guid.Empty;
		private Guid posGuidSell2=Guid.Empty;		
		
		public double frU1,frU2,frU3;    // Значение текущего верхнего Fractal
		public double frD1,frD2,frD3;    // Значение Low - свечи с верхним фрактклом

		public double TPD,TPU,SLU,SLD;
		public double PRD,PRU,PRD2,PRU2,PCU,PCD,PCU2,PCD2,UBU,UBD,Ot;		
		public int PRDi,PRUi,PRDi2,PRUi2,UBDi,UBUi;
		public DateTime tmU1,tmU2,tmU3,tmD1,tmD2,tmD3,tmax,tmin;
		public bool frU,frD,td,tu,nd,nu;

		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		private string trueLogPath = "";	
		
        protected override void Init()
        {	_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			_frInd.Range=frac-2;


			k=0; // Для першого входження -
			kf=0.090909;
			dlt=dl*Instrument.Point; // Отступ от стопа
			frU1=0.0;frU2=0.0;frU3=0.0;frD1=0.0;frD2=0.0;frD3=0.0;
			
				
			if(tu) {  zmax = dt1*Instrument.Point;  
				frU1=fr1*Instrument.Point; tmU1=Bars[Bars.Range.To-1].Time;
				frU2=fr2*Instrument.Point; tmU2=Bars[Bars.Range.To-1].Time;
				frU3=fr2*Instrument.Point; tmU3=Bars[Bars.Range.To-1].Time;
				     nkz4 = zmax-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				     nkz2 = zmax-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			         nkz4v= zmax-((NKZ-5)*Instrument.Point); 
				     nkz2v= zmax-((NKZ*2)*Instrument.Point);
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 					
			}
			if(td) { zmin = dt1*Instrument.Point;
				frD1=fr1*Instrument.Point; tmD1=Bars[Bars.Range.To-1].Time;
				frD2=fr2*Instrument.Point; tmD2=Bars[Bars.Range.To-1].Time;
				frD3=fr2*Instrument.Point; tmD3=Bars[Bars.Range.To-1].Time;
				     nkz4 = zmin+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				     nkz2 = zmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					 nkz4v= zmin+((NKZ-5)*Instrument.Point);  
				     nkz2v= zmin+(((NKZ*2))*Instrument.Point);
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale);  				
			}		
			nu=false; nd=false;
			InitLogFile();  // Запись логов			
        }        
//===========================================================================
        protected override void NewBar()
        {   
			DTime = Bars[ci].Time;
			ci = Bars.Range.To - 1;
			// Print("{0}/{1} {2}:{3}",DTime.Day,DTime.Month,DTime.Hour,DTime.Minute);
			if ( DTime.Day==15 && DTime.Month==10 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.31456; tu=false; td=true; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=660; Print("{0} DW",DTime); }
			if ( DTime.Day==5 && DTime.Month==10 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.30398; tu=true; td=false; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=660; Print("{0} UP",DTime); }
			if ( DTime.Day==24 && DTime.Month==9 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.30540; tu=false; td=true; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=660; Print("{0} DW",DTime); }
			if ( DTime.Day==6 && DTime.Month==9 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.29822; tu=true; td=false; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=660; Print("{0} UP",DTime); }
			if ( DTime.Day==4 && DTime.Month==9 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.28545; tu=false; td=true; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=660; Print("{0} DW",DTime); }			
			if ( DTime.Day==21 && DTime.Month==8 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.27988; tu=true; td=false; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=660; Print("{0} UP",DTime); }			
			if ( DTime.Day==03 && DTime.Month==8 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.30138; tu=false; td=true; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=660; Print("{0} DW",DTime); }			
			if ( DTime.Day==23 && DTime.Month==7 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.31398; tu=true; td=false; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=660; Print("{0} UP",DTime); }
			if ( DTime.Day==10 && DTime.Month==7 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.30540; tu=false; td=true; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=660; Print("{0} DW",DTime); }
			if ( DTime.Day==2 && DTime.Month==7 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.32132; tu=true; td=false; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=704; Print("{0} UP",DTime); }
			if ( DTime.Day==28 && DTime.Month==6 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.31058; tu=false; td=true; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=704; Print("{0} DW",DTime); }
			if ( DTime.Day==22 && DTime.Month==6 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.32691; tu=true; td=false; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=704; Print("{0} UP",DTime); }
			if ( DTime.Day==15 && DTime.Month==6 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.32694; tu=false; td=true; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=704; Print("{0} DW",DTime); }
			if ( DTime.Day==6 && DTime.Month==6 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.34082; tu=true; td=false; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=704; Print("{0} UP",DTime); }
			if ( DTime.Day==18 && DTime.Month==4 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.42819; tu=false; td=true; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=704; Print("{0} DW",DTime); }	
			if ( DTime.Day==10 && DTime.Month==4 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.41635; tu=true; td=false; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=704; Print("{0} UP",DTime); }
			if ( DTime.Day==28 && DTime.Month==3 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.40898; tu=false; td=true; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=704; Print("{0} DW",DTime); }
			if ( DTime.Day==7 && DTime.Month==3 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.39289; tu=true; td=false; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=704; Print("{0} UP",DTime); }
			if ( DTime.Day==10 && DTime.Month==2 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.40097; tu=false; td=true; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=704; Print("{0} DW",DTime); }
			if ( DTime.Day==15 && DTime.Month==2 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.40164; tu=true; td=false; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=704; Print("{0} UP",DTime); }	
			if ( DTime.Day==26 && DTime.Month==01 && DTime.Hour==0 && DTime.Minute==00 )
					{ zmax=1.40825; tu=false; td=true; nu=false; nd=false; frU1=0;frU2=0;frU3=0; NKZ=704; Print("{0} DW",DTime); }				
				
				
//=== КОРЕКЦИЯ =======================================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed)     { posGuidBuy=Guid.Empty;   }  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed)   { posGuidSell=Guid.Empty;  } 
			if (posGuidBuy2!=Guid.Empty && Trade.GetPosition(posGuidBuy2).State==PositionState.Closed)   { posGuidBuy2=Guid.Empty;  }  
		    if (posGuidSell2!=Guid.Empty && Trade.GetPosition(posGuidSell2).State==PositionState.Closed) { posGuidSell2=Guid.Empty; } 
//====  Fractal вверх ================================================================================================================
	  if (_frInd.TopSeries[Bars.Range.To-frac]>0) 		
		{
				  if(frU) { frU=false;		
				  			frU3=frU2; frU2=frU1; frU1=_frInd.TopSeries[Bars.Range.To-frac];
				  			tmU3=tmU2; tmU2=tmU1; tmU1=Bars[Bars.Range.To-frac].Time; 
				  		   } else 
				   		   {
					  		if(_frInd.TopSeries[Bars.Range.To-frac]>frU1) {
						  			frU1=_frInd.TopSeries[Bars.Range.To-frac];
					  	  			tmU1=Bars[Bars.Range.To-frac].Time;  }
				           }
		 }
//====  Fractal вниз ====================================================================================			  
		if (_frInd.BottomSeries[Bars.Range.To-frac]>0)   
		  { 
				  if(!frU) { frU=true;
				     frD3=frD2; frD2=frD1; frD1=_frInd.BottomSeries[Bars.Range.To-frac];
				     tmD3=tmD2; tmD2=tmD1; tmD1=Bars[Bars.Range.To-frac].Time;
				  } else 
				  {
					 if (_frInd.BottomSeries[Bars.Range.To-frac]<frD1) {
						 frD1=_frInd.BottomSeries[Bars.Range.To-frac];
					     tmD1=Bars[Bars.Range.To-frac].Time;  } 
				  }
		   } 			
//====================================================================================================================      		

		  if (tu && Bars[ci].High>zmax) 
		  		{ 
			  		zmax=Bars[ci].High;
					tmax=Bars[ci].Time;
		  			nkz4 = zmax-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				    nkz2 = zmax-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			        nkz4v= zmax-((NKZ-5)*Instrument.Point); 
				    nkz2v= zmax-((NKZ*2)*Instrument.Point);
			        nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			        nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			        nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			        nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
			 		nd=false; nu=false; 			
		  		}
		  
		  if (td && Bars[ci].Low<zmin ) 
		  		{ 
			  		zmin=Bars[ci].Low;
					tmin=Bars[ci].Time;
		  			nkz4 = zmin+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				    nkz2 = zmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					nkz4v= zmin+((NKZ-5)*Instrument.Point);  
				    nkz2v= zmin+(((NKZ*2))*Instrument.Point);
			        nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			        nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			        nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			        nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
					nd=false; nu=false; 			 
		 		}
//== Касание зоны/1\2 ======================================================================================================
			if(n2) {
				if (tu && Bars[ci].Low<nkz2 && !nu)  { nu=true; }
				if (td && Bars[ci].High>nkz2 && !nd) { nd=true; }
			}
			else  {
				if (tu && Bars[ci].Low<nkz4 && !nu)  { nu=true; }
				if (td && Bars[ci].High>nkz4 && !nd) { nd=true; }
			}				
// Если пересечение зоны было (nu) и торгуем вверх (tu) ===========================================================		
	if(nu && tu) 
		{
// Определение патерна   ПИК ПОСЛЕДНЕГО ФРАКТАЛА ПАТЕРНА ВНИЗУ 
		   if (_frInd.TopSeries[Bars.Range.To-frac]>0)    
		   {
           	  if( frU3>frU2 && frU1>frU2  && frU1>0 && frU2>0 && frU3>0) 
				{
					TPU=zmax;
					SLU=frD2-Instrument.Spread-dlt; SLU=Math.Round(SLU, Instrument.PriceScale);
					PRU=Math.Round((TPU-Bars[ci].Close)*Math.Pow(10,Instrument.PriceScale),0);
					PRU2=Math.Round((TP2-Bars[ci].Close)*Math.Pow(10,Instrument.PriceScale),0);
					UBU=Math.Round((Bars[ci].Close-SLU)*Math.Pow(10,Instrument.PriceScale),0);
					PCU=Math.Round(PRU/UBU, 2);
					PCU2=Math.Round(PRU2/UBU, 2);
					Ot=Math.Round((frU3-frU2)/(frU1-frU2), 2);
					PRUi=(int)PRU;
					UBUi=(int)UBU;
					//UBUi=(int)SL1;
					if(UBU>40 && UBU<SL1 && Bars[ci].Close>nkz2 && PCU>3) 
					{
						if (posGuidBuy!=Guid.Empty && posGuidBuy2==Guid.Empty) { 
							if(TP2!=0) PRUi=PRUi2;
							var res4 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,lot,Instrument.Bid, -1,Stops.InPips(UBUi,PRUi), null, null);
							if (res4.IsSuccessful)  posGuidBuy2=res4.Position.Id;
													}
						if (posGuidBuy==Guid.Empty) { 
							var res1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,lot,Instrument.Bid, -1,Stops.InPips(UBUi,PRUi), null, null);
							if (res1.IsSuccessful)  posGuidBuy=res1.Position.Id;
													}
						frD=false;
					}
				}
		   }
		}
// Если пересечение зоны было (nd) и торгуем вниз (td) ===========================================================				
	if(nd && td) 
		{
// Определение патерна   ПИК ПОСЛЕДНЕГО ФРАКТАЛА ПАТЕРНА ВВЕРХУ 
 			if (_frInd.BottomSeries[Bars.Range.To-frac]>0)	   
			{            
				if( frD2>frD3 && frD2>frD1  && frD1>0 && frD2>0 && frD3>0) 
				{   TPD=Math.Round(zmin, Instrument.PriceScale);
					SLD=frU2+Instrument.Spread+dlt; SLD=Math.Round(SLD, Instrument.PriceScale);
					PRD=Math.Round((Bars[ci].Close-TPD)*Math.Pow(10,Instrument.PriceScale),0);
					PRD2=Math.Round((Bars[ci].Close-TP2)*Math.Pow(10,Instrument.PriceScale),0);
					Ot=Math.Round((frD2-frD3)/(frD2-frD1), 2);
					UBD=Math.Round((SLD-Bars[ci].Close)*Math.Pow(10,Instrument.PriceScale),0);
					
					PRDi2=(int)PRD2;
					PRDi=(int)PRD;
					UBDi=(int)UBD;
					//UBDi=(int)SL1;
					PCD=Math.Round(PRD/UBD, 2);
					PCD2=Math.Round(PRD2/UBD, 2);
					if(UBD>40 && UBD<SL1 && Bars[ci].Close<nkz2 && PCD>kof) 
					{	
						if (posGuidSell!=Guid.Empty && posGuidSell2==Guid.Empty) 
							{   if(TP2!=0) PRDi=PRDi2;
							var res3=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, lot,Instrument.Ask, -1,	Stops.InPips(UBDi,PRDi), null, null);
							if (res3.IsSuccessful)  posGuidSell2=res3.Position.Id;
							}
						if (posGuidSell==Guid.Empty) 
							{
							var res2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, lot,Instrument.Ask, -1,	Stops.InPips(UBDi,PRDi), null, null);
							if (res2.IsSuccessful)  posGuidSell=res2.Position.Id;
							}
							frU=false; 
				 	}
			} // if( frD2>frD3 && frD2>frD1 ) 
         } // if(_frInd.BottomSeries[Bars.Range.To-frac]>0)	 
	  } //if(nd && td) 
//===============================================================================================================================   
	st="";
	if (_frInd.TopSeries[Bars.Range.To-frac]>0) 
	{
			if(tu) //Торгуем в Buy
			{	st=st+" tu --";
				if(nu) // Есть пересечение 1/4
				{	st=st+"++";
					if( frU3>frU2 && frU1>frU2 ) //Патерн
					{
						if(UBU>20 && UBU<SL1 && Bars[ci].Close>nkz2 && PCU>kof) {
						     st=st+"@@ ";
							if (posGuidBuy!=Guid.Empty)  st=st+"Buy1 ";
							if (posGuidBuy2!=Guid.Empty) st=st+"Buy2 "; } else st=st+"!! ";
						
						st=st+"| SL="+UBU.ToString()+" TP="+PRU.ToString()+" %="+PCU.ToString();
					}
				}
			XXPrint("{1} | {0} | frU= {2}:{3}({4}) {5}:{6}({7}) {8}:{9}({10}) -- max={11}({13}) nkz4={12}",Bars[Bars.Range.To-1].Time,st,
				tmU3.Hour,tmU3.Minute,frU3,tmU2.Hour,tmU2.Minute,frU2,tmU1.Hour,tmU1.Minute,frU1,zmax,nkz4,tmax);
			}
	}
	if (_frInd.BottomSeries[Bars.Range.To-frac]>0) 
	{
			if(td) //Торгуем в Buy
			{	st=st+" td --";
				if(nd) // Есть пересечение 1/4
				{   st=st+"++";
					if( frD2>frD3 && frD2>frD1 ) //Патерн
					{
						if(UBD>20 && UBD<SL1 && Bars[ci].Close<nkz2 && PCD>2) {
							 st=st+"@@ ";
							if (posGuidSell!=Guid.Empty)  st=st+"Sell1 ";
							if (posGuidSell2!=Guid.Empty) st=st+"Sell2 "; } else  st=st+"!! ";
						st=st+"| SL="+UBD.ToString()+" TP="+PRD.ToString()+" %="+PCD.ToString();
					}	
				}
			 XXPrint("{1} | {0} | frD= {2}:{3}({4}) {5}:{6}({7}) {8}:{9}({10}) | min={11}({13}) nkz4={12}",Bars[Bars.Range.To-1].Time,st,
				tmD3.Hour,tmD3.Minute,frD3,tmD2.Hour,tmD2.Minute,frD2,tmD1.Hour,tmD1.Minute,frD1,zmin,nkz4,tmin);
			}
	}
  } // NewBar
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

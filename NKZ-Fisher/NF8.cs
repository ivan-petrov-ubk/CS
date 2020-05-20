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
    [TradeSystem("NF8")] //copy of "NF71"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Номер версии NF7", DefaultValue = 1)]
        public int ver { get; set; }

		[Parameter("Fisher MIN:", DefaultValue = 0.9)]
        public double kf1 { get; set; }	
		
		[Parameter("Отступ Stop :", DefaultValue = 30)]
		public int dl { get;set; }
		
		[Parameter("Коф. прибыль :", DefaultValue = 1.5)]
		public double kof { get;set; }						
				
		[Parameter("Log_Path", DefaultValue = @"1")]
		public string L1 { get;set; }
		
		private bool nu,nd,n2,torg,tu,td,trg;
		private int NKZ,i;
		private int k=0,k2=0;
		private double dlt,kl;
		public FisherTransformOscillator _ftoInd;
		public DateTime DTime,MTime; // Время
		public int ci = 0,cu=0;
		private string st,st2,name,name2;
		private double nkz2,nkz4,nkz2v,nkz4v,zmax,zmin,tp2;
		private double kf=0.909090909090909;
	
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private Guid posGuidBuy2=Guid.Empty;
		private Guid posGuidSell2=Guid.Empty;		
		
		public double PRD,PRU,PRD2,PRU2,PCU,PCD,PCU2,PCD2,UBU,UBD,Ot;		
		public int PRDi,PRUi,PRDi2,PRUi2,UBDi,UBUi;
		public DateTime tmU1,tmU2,tmU3,tmD1,tmD2,tmD3,tmax,tmin;
		public bool first=true,kmax=true;
		public double bu1,bu2,sl1,sl2;

		public TrendLine toolTrendLine2,toolTrendLine4,toolTrendLineD;
		public TrendLine toolTrendLineU1,toolTrendLineU2,toolTrendLineU3,toolTrendLineU4;
		public TrendLine toolTrendLineD1,toolTrendLineD2,toolTrendLineD3,toolTrendLineD4;
		
		public static StreamReader LogSW = null;
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		

		private string trueInitPath = "";	
		public int kl7,k7,kt;
		
		public double mind, maxu, mindf, maxuf;
		public double mu1,mu2,mu3,mu4,mu;
		public double md1,md2,md3,md4,md;
		public int mdi1,mdi2,mdi3,mdi4,mindi;
		public int mui1,mui2,mui3,mui4,maxui;

		public DateTime mdt1,mdt2,mdt3,mdt4;
		public DateTime mut1,mut2,mut3,mut4;
		public DateTime vut1,vut2,vut3,vut4,maxvt;
		public DateTime vdt1,vdt2,vdt3,vdt4;
		public Text tU1,tU2,tU3,tU4,tD1,tD2,tD3,tD4,toolText,toolText1;
		
		public DateTime mindt, maxut;		
		public double fbU,fbD;
		
		public double nkz;		
		
        protected override void Init()
        {	nu=false; nd=false; md=0; mu=0;
			trg=true;
			InitFile();
			dlt=dl*Instrument.Point; 
		    if(_ftoInd==null) _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
			// Рисуем зоны
			if(toolTrendLine4==null) { toolTrendLine4 = Tools.Create<TrendLine>(); toolTrendLine4.Color=Color.HotPink;toolTrendLine4.Width=3; }
			if(toolTrendLine2==null) { toolTrendLine2 = Tools.Create<TrendLine>(); toolTrendLine2.Color=Color.Crimson;toolTrendLine2.Width=6; }
			if(toolTrendLineD==null) { toolTrendLineD = Tools.Create<TrendLine>(); toolTrendLineD.Color=Color.ForestGreen;toolTrendLineD.Width=6; }
			// Рисуем линии волн
			if(toolTrendLineU1==null) { toolTrendLineU1 = Tools.Create<TrendLine>(); toolTrendLineU1.Color=Color.Aqua;toolTrendLineU1.Width=3; }
			if(toolTrendLineU2==null) { toolTrendLineU2 = Tools.Create<TrendLine>(); toolTrendLineU2.Color=Color.Aqua;toolTrendLineU2.Width=3; }
			if(toolTrendLineU3==null) { toolTrendLineU3 = Tools.Create<TrendLine>(); toolTrendLineU3.Color=Color.Aqua;toolTrendLineU3.Width=3; }
			if(toolTrendLineU4==null) { toolTrendLineU4 = Tools.Create<TrendLine>(); toolTrendLineU4.Color=Color.Aqua;toolTrendLineU4.Width=3; }			
			if(toolTrendLineD1==null) { toolTrendLineD1 = Tools.Create<TrendLine>(); toolTrendLineD1.Color=Color.Aqua;toolTrendLineD1.Width=3; }
			if(toolTrendLineD2==null) { toolTrendLineD2 = Tools.Create<TrendLine>(); toolTrendLineD2.Color=Color.Aqua;toolTrendLineD2.Width=3; }
			if(toolTrendLineD3==null) { toolTrendLineD3 = Tools.Create<TrendLine>(); toolTrendLineD3.Color=Color.Aqua;toolTrendLineD3.Width=3; }	
			if(toolTrendLineD4==null) { toolTrendLineD4 = Tools.Create<TrendLine>(); toolTrendLineD4.Color=Color.Aqua;toolTrendLineD4.Width=3; }	
			
			if(toolText==null) { toolText  = Tools.Create<Text>();toolText.Color=Color.Aqua; }
			if(toolText1==null) { toolText1 = Tools.Create<Text>();toolText1.Color=Color.Aqua; }
			
			if(tU1==null) { tU1 = Tools.Create<Text>(); tU1.Color=Color.Aqua; }
			if(tU2==null) { tU2 = Tools.Create<Text>(); tU2.Color=Color.Aqua; }
			if(tU3==null) { tU3 = Tools.Create<Text>(); tU3.Color=Color.Aqua; }
			if(tU4==null) { tU4 = Tools.Create<Text>(); tU4.Color=Color.Aqua; }
			
			if(tD1==null) { tD1 = Tools.Create<Text>(); tD1.Color=Color.Aqua; }
			if(tD2==null) { tD2 = Tools.Create<Text>(); tD2.Color=Color.Aqua; }
			if(tD3==null) { tD3 = Tools.Create<Text>(); tD3.Color=Color.Aqua; }
			if(tD4==null) { tD4 = Tools.Create<Text>(); tD4.Color=Color.Aqua; }
        }        
//===========================================================================
        protected override void NewBar()
        {   
			ci = Bars.Range.To - 1;
			DTime = Bars[ci].Time;
//====== START  FIRST  ==============================================================================================				
			if(first) 
		    {   first=false; 
				
				InitFr();
				if(kmax) 
				{ //MAX
if(mu1>0  && md1>0) { toolTrendLineU1.Point1= new ChartPoint(mut1, mu1); toolTrendLineU1.Point2= new ChartPoint(mdt1, md1); }				  
if(mu2>0  && md1>0) { toolTrendLineD1.Point1= new ChartPoint(mdt1, md1); toolTrendLineD1.Point2= new ChartPoint(mut2, mu2); } 
if(mu2>0  && md2>0) { toolTrendLineU2.Point1= new ChartPoint(mut2, mu2); toolTrendLineU2.Point2= new ChartPoint(mdt2, md2); }
if(mu3>0  && md2>0) { toolTrendLineD2.Point1= new ChartPoint(mdt2, md2); toolTrendLineD2.Point2= new ChartPoint(mut3, mu3); }
if(mu3>0  && md3>0) { toolTrendLineU3.Point1= new ChartPoint(mut3, mu3); toolTrendLineU3.Point2= new ChartPoint(mdt3, md3); }
if(mu4>0  && md3>0) { toolTrendLineD3.Point1= new ChartPoint(mdt3, md3); toolTrendLineD3.Point2= new ChartPoint(mut4, mu4); }
if(mu4>0  && md4>0) { toolTrendLineU4.Point1= new ChartPoint(mut4, mu4); toolTrendLineU4.Point2= new ChartPoint(mdt4, md4); }	
if(mu1>0)  { tU1.Point=new ChartPoint(mut1, mu1+Instrument.Spread*3); tU1.Caption=string.Format("U1-{0}",fbD); }
if(md1>0)  { tD1.Point=new ChartPoint(mdt1, md1); tD1.Caption=string.Format("D1"); }
				} else 
				{  //MIN
if(mu1>0  && md1>0) { toolTrendLineD1.Point1= new ChartPoint(mdt1, md1); toolTrendLineD1.Point2= new ChartPoint(mut1, mu1); }
if(mu1>0  && md2>0) { toolTrendLineU1.Point1= new ChartPoint(mut1, mu1); toolTrendLineU1.Point2= new ChartPoint(mdt2, md2); } 
if(mu2>0  && md2>0) { toolTrendLineD2.Point1= new ChartPoint(mdt2, md2); toolTrendLineD2.Point2= new ChartPoint(mut2, mu2); }
if(mu2>0  && md3>0) { toolTrendLineU2.Point1= new ChartPoint(mut2, mu2); toolTrendLineU2.Point2= new ChartPoint(mdt3, md3); }
if(mu3>0  && md3>0) { toolTrendLineD3.Point1= new ChartPoint(mdt3, md3); toolTrendLineD3.Point2= new ChartPoint(mut3, mu3); }
if(mu3>0  && md4>0) { toolTrendLineU3.Point1= new ChartPoint(mut3, mu3); toolTrendLineU3.Point2= new ChartPoint(mdt4, md4); }	
if(mu4>0  && md4>0) { toolTrendLineD4.Point1= new ChartPoint(mdt4, md4); toolTrendLineD4.Point2= new ChartPoint(mut4, mu4); }
if(mu1>0)  { tU1.Point=new ChartPoint(mut1, mu1+Instrument.Spread*3); tU1.Caption=string.Format("U1"); }
if(md1>0)  { tD1.Point=new ChartPoint(mdt1, md1); tD1.Caption=string.Format("D1-{0}",fbU); }
				}
						if(mu2>0)  { tU2.Point=new ChartPoint(mut2, mu2+Instrument.Spread*3); tU2.Caption=string.Format("U2"); }
						if(mu3>0)  { tU3.Point=new ChartPoint(mut3, mu3+Instrument.Spread*3); tU3.Caption=string.Format("U3"); }
						if(mu4>0)  { tU4.Point=new ChartPoint(mut4, mu4+Instrument.Spread*3); tU4.Caption=string.Format("U4"); }
						if(md2>0)  { tD2.Point=new ChartPoint(mdt2, md2); tD2.Caption=string.Format("D2"); }
						if(md3>0)  { tD3.Point=new ChartPoint(mdt3, md3); tD3.Caption=string.Format("D3"); }
						if(md4>0)  { tD4.Point=new ChartPoint(mdt4, md4); tD4.Caption=string.Format("D4"); }	
				if(tu) { 
					zmax = Math.Round(kl,Instrument.PriceScale);
				// ======================================  Статистика =================================
				XXPrint("{0} U START Усл={1} Pater={10} torg={2} nu={3} tu={4} fbU={5} mu1={6} mu2={7} Fish={8} zmax={9}",DTime,
					(torg && nu && tu && fbU>0.5 && fbU<0.8 && mu1>mu2 && _ftoInd.FisherSeries[Bars.Range.To-1]>kf1),
					torg,nu,tu,fbU,mu1,mu2,_ftoInd.FisherSeries[Bars.Range.To-1],zmax,(fbU>0.5 && fbU<0.8 && mu1>mu2));
					 
					 nkz4 = zmax-(NKZ*Instrument.Point);
				     nkz2 = zmax-((NKZ*2)*Instrument.Point);
			         nkz4v= zmax-((NKZ*kf)*Instrument.Point); 
				     nkz2v= zmax-(((NKZ*2)*kf)*Instrument.Point);
					
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
					

toolText.Point=new ChartPoint(DTime, nkz4v+Instrument.Spread); toolText.Caption=string.Format("{0}",nkz4v);				
toolTrendLine4.Point1= new ChartPoint(DTime, nkz4v); toolTrendLine4.Point2= new ChartPoint(DTime.AddDays(1), nkz4v);	
toolText1.Point=new ChartPoint(DTime, nkz2v+Instrument.Spread);toolText1.Caption=string.Format("{0}",nkz2v);					
toolTrendLine2.Point1= new ChartPoint(DTime, nkz2v); toolTrendLine2.Point2= new ChartPoint(DTime.AddDays(1), nkz2v);
						}
				
				if(td) {
					zmin = Math.Round(kl,Instrument.PriceScale);
				// ======================================  Статистика =================================	
				XXPrint("{0} D START Усл={1} Pater={10} torg={2} nd={3} td={4} fbD={5} md1={6} md2={7} Fish={8} zmin={9}",DTime,
					(torg && nd && td && fbD>0.4 && fbD<0.8 && md1<md2 && _ftoInd.FisherSeries[Bars.Range.To-1]<-kf1),
					torg,nd,td,fbD,md1,md2,_ftoInd.FisherSeries[Bars.Range.To-1],zmin,(fbD>0.4 && fbD<0.8 && md1<md2));					
					 
					
					 nkz4 = zmin+(NKZ*Instrument.Point);
				     nkz2 = zmin+((NKZ*2)*Instrument.Point);
			         nkz4v= zmin+((NKZ*kf)*Instrument.Point); 
				     nkz2v= zmin+(((NKZ*2)*kf)*Instrument.Point);
					
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale);
					
	
toolText.Point=new ChartPoint(DTime, nkz4v+Instrument.Spread);toolText.Caption=string.Format("{0}",nkz4v);						
toolTrendLine4.Point1= new ChartPoint(DTime, nkz4v); toolTrendLine4.Point2= new ChartPoint(DTime.AddDays(3), nkz4v);
toolText1.Point=new ChartPoint(DTime, nkz2v+Instrument.Spread);toolText1.Caption=string.Format("{0}",nkz2v);	
toolTrendLine2.Point1= new ChartPoint(DTime, nkz2v); toolTrendLine2.Point2= new ChartPoint(DTime.AddDays(3), nkz2v);					
			}
 		    }
//========  END FIRST ============================================================================================================      		
		  if (tu && Bars[ci].High>zmax) 
		  		{ 
					
			  		zmax=Bars[ci].High;
					tmax=Bars[ci].Time;
					
					 nkz4 = zmax-(NKZ*Instrument.Point);
				     nkz2 = zmax-((NKZ*2)*Instrument.Point);
					
			         nkz4v= zmax-((NKZ*kf)*Instrument.Point); 
				     nkz2v= zmax-(((NKZ*2)*kf)*Instrument.Point);
					
			        nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			        nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			        nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			        nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
			 		nd=false; nu=false;
				
toolTrendLine4.Point1= new ChartPoint(DTime, nkz4v); toolTrendLine4.Point2= new ChartPoint(DTime.AddDays(3), nkz4v);					
toolText.Point=new ChartPoint(DTime, nkz4v+Instrument.Spread);toolText.Caption=string.Format("{0}",nkz4v);				
toolTrendLine2.Point1= new ChartPoint(DTime, nkz2v); toolTrendLine2.Point2= new ChartPoint(DTime.AddDays(3), nkz2v);
toolText1.Point=new ChartPoint(DTime, nkz2v+Instrument.Spread);toolText1.Caption=string.Format("{0}",nkz2v);				
					
		  		}
		  if (td && Bars[ci].Low<zmin ) 
		  		{ 
			  		zmin=Bars[ci].Low;
					tmin=Bars[ci].Time;
					
					 nkz4 = zmin+(NKZ*Instrument.Point);
				     nkz2 = zmin+((NKZ*2)*Instrument.Point);
					
			         nkz4v= zmin+((NKZ*kf)*Instrument.Point); 
				     nkz2v= zmin+(((NKZ*2)*kf)*Instrument.Point);

			        nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			        nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			        nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			        nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
					
					nd=false; nu=false; 

toolTrendLineD.Point1= new ChartPoint(DTime, zmin); toolTrendLineD.Point2= new ChartPoint(DTime, nkz2v);	
toolTrendLine4.Point1= new ChartPoint(DTime, nkz4v); toolTrendLine4.Point2= new ChartPoint(DTime.AddDays(3), nkz4v);	
toolText.Point=new ChartPoint(DTime, nkz4v);toolText.Caption=string.Format("{0}",nkz4v);						
toolTrendLine2.Point1= new ChartPoint(DTime, nkz2v); toolTrendLine2.Point2= new ChartPoint(DTime.AddDays(3), nkz2v);						
toolText1.Point=new ChartPoint(DTime, nkz2v);toolText1.Caption=string.Format("{0}",nkz2v);						

		 		}
//== Касание зоны/1\2 ======================================================================================================
			if(n2) {
				if (tu && Bars[ci].Low<nkz2v && !nu) nu=true; 
				if (td && Bars[ci].High>nkz2v && !nd) nd=true; 
			}
			else  {
				if (tu && Bars[ci].Low<nkz4v && !nu)   nu=true;
				if (td && Bars[ci].High>nkz4v && !nd)  nd=true; 
			}				

//===================================================================================================================================			
			if(_ftoInd.FisherSeries[Bars.Range.To-2]<0 && _ftoInd.FisherSeries[Bars.Range.To-1]>0 ) // MIN - Fisher -/+ снизу/вверх
			{ kl7=0;  mind=double.MaxValue; mindf=double.MaxValue; trg=true;
				do 
				{   kl7++;            
					if(Bars[Bars.Range.To-kl7].Low<mind) {mind=Bars[Bars.Range.To-kl7].Low; 
						mindt=Bars[Bars.Range.To-kl7].Time; mindi=Bars.Range.To-kl7; }
							if(_ftoInd.FisherSeries[Bars.Range.To-kl7]<mindf) mindf=_ftoInd.FisherSeries[Bars.Range.To-kl7];
				}
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl7]<0  &&  _ftoInd.FisherSeries[Bars.Range.To-kl7-1]>0) && kl7<1000);
					if(mindf<-kf1) 
					{ 	if(mui1>mdi1) 
						 {  md4=md3;   md3=md2;   md2=md1;   md1=mind;  
						    mdt4=mdt3; mdt3=mdt2; mdt2=mdt1; mdt1=mindt;
				            mdi4=mdi3; mdi3=mdi2; mdi2=mdi1; mdi1=mindi;
						 } else { if (mind<md1) { md1=mind; mdt1=mindt; mdi1=mindi; } }	
						 
					 if(mu1>0 && md1>0 && md2>0) fbU=Math.Round((mu1-md1)/(mu1-md2),2); else fbU=-1;
					 
if(mu1>0  && md1>0) { toolTrendLineD1.Point1= new ChartPoint(mdt1, md1); toolTrendLineD1.Point2= new ChartPoint(mut1, mu1); }
if(mu1>0  && md2>0) { toolTrendLineU1.Point1= new ChartPoint(mut1, mu1); toolTrendLineU1.Point2= new ChartPoint(mdt2, md2); } 
if(mu2>0  && md2>0) { toolTrendLineD2.Point1= new ChartPoint(mdt2, md2); toolTrendLineD2.Point2= new ChartPoint(mut2, mu2); }
if(mu2>0  && md3>0) { toolTrendLineU2.Point1= new ChartPoint(mut2, mu2); toolTrendLineU2.Point2= new ChartPoint(mdt3, md3); }
if(mu3>0  && md3>0) { toolTrendLineD3.Point1= new ChartPoint(mdt3, md3); toolTrendLineD3.Point2= new ChartPoint(mut3, mu3); }
if(mu3>0  && md4>0) { toolTrendLineU3.Point1= new ChartPoint(mut3, mu3); toolTrendLineU3.Point2= new ChartPoint(mdt4, md4); }	
if(mu4>0  && md4>0) { toolTrendLineD4.Point1= new ChartPoint(mdt4, md4); toolTrendLineD4.Point2= new ChartPoint(mut4, mu4); }	

						if(mu1>0)  { tU1.Point=new ChartPoint(mut1, mu1+Instrument.Spread*2); tU1.Caption=string.Format("U1"); }
						if(mu2>0)  { tU2.Point=new ChartPoint(mut2, mu2+Instrument.Spread*2); tU2.Caption=string.Format("U2"); }
						if(mu3>0)  { tU3.Point=new ChartPoint(mut3, mu3+Instrument.Spread*2); tU3.Caption=string.Format("U3"); }
						if(mu4>0)  { tU4.Point=new ChartPoint(mut4, mu4+Instrument.Spread*2); tU4.Caption=string.Format("U4"); }
						if(md1>0)  { tD1.Point=new ChartPoint(mdt1, md1); tD1.Caption=string.Format("D1-{0}",fbU); }
						if(md2>0)  { tD2.Point=new ChartPoint(mdt2, md2); tD2.Caption=string.Format("D2"); }
						if(md3>0)  { tD3.Point=new ChartPoint(mdt3, md3); tD3.Caption=string.Format("D3"); }
						if(md4>0)  { tD4.Point=new ChartPoint(mdt4, md4); tD4.Caption=string.Format("D4"); }
					}	
				// ======================================  Статистика =================================
				XXPrint("{0} U Усл={1} Patern={10} torg={2} nu={3} tu={4} fbU={5} mu1={6} mu2={7} Fish={8} trg={9}",DTime,
					(torg && nu && tu && fbU>0.5 && fbU<0.8 && mu1>mu2),
					torg,nu,tu,fbU,mu1,mu2,_ftoInd.FisherSeries[Bars.Range.To-1],trg,(fbU>0.5 && fbU<0.8 && mu1>mu2));					
			} 
			
			if( _ftoInd.FisherSeries[Bars.Range.To-2]>0 && _ftoInd.FisherSeries[Bars.Range.To-1]<0 ) // MAX - Fisher +/- зверху/вниз
				{ kl7=0;  maxu=double.MinValue; maxuf=double.MinValue; trg=true;
				do
				{ kl7++;      
					if(Bars[Bars.Range.To-kl7].High>maxu) {maxu=Bars[Bars.Range.To-kl7].High; maxut=Bars[Bars.Range.To-kl7].Time; maxui=Bars.Range.To-kl7;}  
					if(_ftoInd.FisherSeries[Bars.Range.To-kl7]>maxuf) maxuf=_ftoInd.FisherSeries[Bars.Range.To-kl7];
				}				
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl7]>0  &&  _ftoInd.FisherSeries[Bars.Range.To-kl7-1]<0) && kl7<1000);
			    if(maxuf>kf1) 
				  {   if(mui1<mdi1)  {
				   			mu4=mu3;   mu3=mu2;   mu2=mu1;   mu1=maxu;  
				   			mut4=mut3; mut3=mut2; mut2=mut1; mut1=maxut;
							mui4=mui3; mui3=mui2; mui2=mui1; mui1=maxui;
					  } else { if (maxu>mu1) { mu1=maxu; mut1=maxut; mui1=maxui; } }	
				    if(mu1>0 && md1>0 && mu2>0) fbD=Math.Round((mu1-md1)/(mu2-md1),2); else fbD=-1;
					
if(mu1>0  && md1>0) { toolTrendLineU1.Point1= new ChartPoint(mut1, mu1); toolTrendLineU1.Point2= new ChartPoint(mdt1, md1); }				  
if(mu2>0  && md1>0) { toolTrendLineD1.Point1= new ChartPoint(mdt1, md1); toolTrendLineD1.Point2= new ChartPoint(mut2, mu2); } 
if(mu2>0  && md2>0) { toolTrendLineU2.Point1= new ChartPoint(mut2, mu2); toolTrendLineU2.Point2= new ChartPoint(mdt2, md2); }
if(mu3>0  && md2>0) { toolTrendLineD2.Point1= new ChartPoint(mdt2, md2); toolTrendLineD2.Point2= new ChartPoint(mut3, mu3); }
if(mu3>0  && md3>0) { toolTrendLineU3.Point1= new ChartPoint(mut3, mu3); toolTrendLineU3.Point2= new ChartPoint(mdt3, md3); }
if(mu4>0  && md3>0) { toolTrendLineD3.Point1= new ChartPoint(mdt3, md3); toolTrendLineD3.Point2= new ChartPoint(mut4, mu4); }
if(mu4>0  && md4>0) { toolTrendLineU4.Point1= new ChartPoint(mut4, mu4); toolTrendLineU4.Point2= new ChartPoint(mdt4, md4); }	

						if(mu1>0)  { tU1.Point=new ChartPoint(mut1, mu1+Instrument.Spread*2); tU1.Caption=string.Format("U1-{0}",fbD); }
						if(mu2>0)  { tU2.Point=new ChartPoint(mut2, mu2+Instrument.Spread*2); tU2.Caption=string.Format("U2"); }
						if(mu3>0)  { tU3.Point=new ChartPoint(mut3, mu3+Instrument.Spread*2); tU3.Caption=string.Format("U3"); }
						if(mu4>0)  { tU4.Point=new ChartPoint(mut4, mu4+Instrument.Spread*2); tU4.Caption=string.Format("U4"); }
						if(md1>0)  { tD1.Point=new ChartPoint(mdt1, md1); tD1.Caption=string.Format("D1"); }
						if(md2>0)  { tD2.Point=new ChartPoint(mdt2, md2); tD2.Caption=string.Format("D2"); }
						if(md3>0)  { tD3.Point=new ChartPoint(mdt3, md3); tD3.Caption=string.Format("D3"); }
						if(md4>0)  { tD4.Point=new ChartPoint(mdt4, md4); tD4.Caption=string.Format("D4"); }
				  }  
				// ======================================  Статистика =================================	
				XXPrint("{0} D Усл={1} Patern={10} torg={2} nd={3} td={4} fbD={5} md1={6} md2={7} Fish={8} trg={9}",DTime,
					(torg && nd && td && fbD>0.4 && fbD<0.8 && md1<md2),
					torg,nd,td,fbD,md1,md2,_ftoInd.FisherSeries[Bars.Range.To-1],trg,(fbD>0.4 && fbD<0.8 && md1<md2));					  
       		 	} 
//=================================================================================================================================== 
if(torg && trg && nd && td && fbD>0.4 && fbD<0.8 && md1<md2 && _ftoInd.FisherSeries[Bars.Range.To-1]<-kf1)
		{           trg=false;
					PRD=Math.Round((Bars[ci].Close-zmin)*Math.Pow(10,Instrument.PriceScale),0);
					UBD=Math.Round((mu2-Bars[ci].Close+dlt)*Math.Pow(10,Instrument.PriceScale),0);
					PRDi=(int)PRD;
					UBDi=(int)UBD;
					PCD=Math.Round(PRD/UBD, 2);
					XXPrint("{0} SELL  ------------------  SL={1} TP={2} Kf={3}",DTime,UBDi,PRDi,PCD);
						if (posGuidSell==Guid.Empty && PCD>kof) 
							{
							var res2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,	
									Stops.InPips(UBDi,PRDi),
									null, null);
							if (res2.IsSuccessful)  posGuidSell=res2.Position.Id;
							}		
		}
if(torg && trg && nu && tu && fbU>0.5 && fbU<0.8 && mu1>mu2 && _ftoInd.FisherSeries[Bars.Range.To-1]>kf1) 
	    {   trg=false;
			PRU=Math.Round((zmax-Bars[ci].Close)*Math.Pow(10,Instrument.PriceScale),0);
			UBU=Math.Round((Bars[ci].Close-md2+dlt)*Math.Pow(10,Instrument.PriceScale),0);
			PRUi=(int)PRU;
			UBUi=(int)UBU;
			PCU=Math.Round(PRU/UBU, 2);
			XXPrint("{0} BUY  ------------------  SL={1} TP={2} Kf={3}",DTime,UBUi,PRUi,PCU);
						if (posGuidBuy==Guid.Empty && PCU>kof) { 
							var res1 = Trade.OpenMarketPosition(Instrument.Id, 
								ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								Stops.InPips(UBUi,PRUi), 
								null, null);
							if (res1.IsSuccessful)  posGuidBuy=res1.Position.Id;
													}
		}
// if(tu) XXPrint("{0} TU --- max={1} nu={2} n2={3} NKZ={4} TP2={5} fbU={6} FS={7}",DTime,kl,nu,n2,NKZ,tp2,fbU,Math.Round(_ftoInd.FisherSeries[Bars.Range.To-1],2)); 
// if(td) XXPrint("{0} TD --- min={1} nd={2} n2={3} NKZ={4} TP2={5} fbD={6} FS={7}",DTime,kl,nd,n2,NKZ,tp2,fbD,Math.Round(_ftoInd.FisherSeries[Bars.Range.To-1],2));

//=================================================== UPDATE  STOP ==========================================
   if (posGuidSell!=Guid.Empty && sl1!=mu2 ) 	
	{sl1=mu2; var res5 = Trade.UpdateMarketPosition(posGuidSell,mu2,Trade.GetPosition(posGuidSell).TakeProfit, null); 
		    if (res5.IsSuccessful) posGuidSell=res5.Position.Id;}
	
   if (posGuidBuy!=Guid.Empty && bu1!=md2) 	
	{bu1=md2; var res6 = Trade.UpdateMarketPosition(posGuidBuy,md2,Trade.GetPosition(posGuidBuy).TakeProfit, null); 
		    if (res6.IsSuccessful) posGuidBuy=res6.Position.Id;}		
  } // NewBar
		
//============================================================================================================	

        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            if (type==ModificationType.Closed)
            {
               if (posGuidBuy==position.Id)  posGuidBuy=Guid.Empty;  
		       if (posGuidSell==position.Id) posGuidSell=Guid.Empty;
            }
        }			
//===============  Read conf   ===============================================================================		
 		protected void InitFile()
		{ 
			trueInitPath=PathToLogFile+"\\m.csv";
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
								
XXPrint("{0} INIT --------- kl={1} tu={2} td={3} nu={4} nd={5} n2={6} NKZ={7}",DTime,kl,tu,td,nu,nd,n2,NKZ);			
						LogSW.Close(); 
			}  			
		}  
// END InitFile()  ===============================================================================		  
//====== History =================================================================================================		
 		protected void InitFr()
		{
			zmin=double.MaxValue;
			zmax=0;

			for(int i = 500; i > 0; i--)
			{  cu=Bars.Range.To-i;
				MTime=Bars[cu].Time;
			if(_ftoInd.FisherSeries[cu-1]<0 && _ftoInd.FisherSeries[cu]>0 ) // MIN
			{ kl7=0;  mind=double.MaxValue; mindf=double.MaxValue; kmax=false;
				do 
				{   kl7++;     
					if(Bars[cu-kl7].Low<mind)    {mind=Bars[cu-kl7].Low; mindt=Bars[cu-kl7].Time; mindi=cu-kl7; }
					if(_ftoInd.FisherSeries[cu-kl7]<mindf) mindf=_ftoInd.FisherSeries[cu-kl7];
				}
				while(!(_ftoInd.FisherSeries[cu-kl7]<0  &&  _ftoInd.FisherSeries[cu-kl7-1]>0) && kl7<1000);
					if(mindf<-kf1) 
					{ 	if(mui1>mdi1) 
						 {  md4=md3;   md3=md2;   md2=md1;   md1=mind;  
						    mdt4=mdt3; mdt3=mdt2; mdt2=mdt1; mdt1=mindt;
				            mdi4=mdi3; mdi3=mdi2; mdi2=mdi1; mdi1=mindi;
						 } else { if (mind<md1) { md1=mind; mdt1=mindt; mdi1=mindi;  } }	
						 
					if(mu1>0 && md1>0 && md2>0) fbU=Math.Round((mu1-md1)/(mu1-md2),2); else fbU=-1;	 
					}		 
			}
			if( _ftoInd.FisherSeries[cu-1]>0 && _ftoInd.FisherSeries[cu]<0 ) // MAX
				{ kl7=0;  maxu=double.MinValue; maxuf=double.MinValue; kmax=true;
				do
				{ kl7++; 
					if(Bars[cu-kl7].High>maxu) {maxu=Bars[cu-kl7].High; maxut=Bars[cu-kl7].Time; maxui=cu-kl7;}  
					if(_ftoInd.FisherSeries[cu-kl7]>maxuf) maxuf=_ftoInd.FisherSeries[cu-kl7];
				}				
				while(!(_ftoInd.FisherSeries[cu-kl7]>0  &&  _ftoInd.FisherSeries[cu-kl7-1]<0) && kl7<1000);
			    if(maxuf>kf1) 
				  {   if(mui1<mdi1)  {
				   			mu4=mu3;   mu3=mu2;   mu2=mu1;   mu1=maxu;  
				   			mut4=mut3; mut3=mut2; mut2=mut1; mut1=maxut;
							mui4=mui3; mui3=mui2; mui2=mui1; mui1=maxui;
					  } else { if (maxu>mu1) { mu1=maxu; mut1=maxut; mui1=maxui; } }	
					  if(mu1>0 && md1>0 && mu2>0) fbD=Math.Round((mu1-md1)/(mu2-md1),2); else fbD=-1;
					  
	
				}
				} 
 	 
        	}			
		} 
//===============  END History ===============================================================================	
//== Log  File ===================================================================================================   
	
		protected void XXPrint(string xxformat, params object[] parameters)
		{       var trueLogPath = PathToLogFile+"\\"+L1+"_"+Instrument.Name.ToString()+".LOG";
				var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);
		}
/*		protected void XPrint(string xxformat, params object[] parameters)
		{		var trueBuyPath = PathToLogFile+"\\"+Instrument.Name.ToString()+".DAT";
				var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueBuyPath, logString);
		} 
*/

    }
}

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
    [TradeSystem("NF4")]                 //copy of "NF3"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Номер версии", DefaultValue = 1)]
        public int ver { get; set; }

		[Parameter("Fisher MIN:", DefaultValue = 0.9)]
        public double kf1 { get; set; }	
		
		[Parameter("Коф. прибыль :", DefaultValue = 1.5)]
		public double kof { get;set; }						
		
		[Parameter("Log_Path", DefaultValue = @"1")]
		public string L1 { get;set; }
		
		[Parameter("Init_FileName", DefaultValue = @"m")]
		public string BuyFileName { get;set; }
				
		private bool nu,nd,n2,torg,tu,td;
		private int NKZ,i;
		private VerticalLine vl; 
		private HorizontalLine hl;
		private int k=0,k2=0;

		public FisherTransformOscillator _ftoInd;
		public DateTime DTime; // Время
		public int ci = 0;
		private string st,st2,name,name2;
		private double nkz2,nkz4,nkz2v,nkz4v,kf,zmax,zmin,tp2;

	    private double dlt,frUp,frDown;
	
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private Guid posGuidBuy2=Guid.Empty;
		private Guid posGuidSell2=Guid.Empty;		

		public double TPD,TPU,SLU,SLD;
		public double PRD,PRU,PRD2,PRU2,PCU,PCD,PCU2,PCD2,UBU,UBD,Ot;		
		public int PRDi,PRUi,PRDi2,PRUi2,UBDi,UBUi;
		public DateTime tmU1,tmU2,tmU3,tmD1,tmD2,tmD3,tmax,tmin;
		public bool frU,frD,first;
		public double bu1,bu2,sl1,sl2;

		public TrendLine toolTrendLine2,toolTrendLine4,toolTrendLineD,tv;
		public TrendLine toolTrendLineU1,toolTrendLineU2,toolTrendLineU3,toolTrendLineU4,toolTrendLineD1,toolTrendLineD2,toolTrendLineD3,toolTrendLineD4;
		
		public static StreamReader LogSW = null;
		public static StreamReader LogRD = null;
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		private string trueLogPath = "";	
		private string trueBuyPath = "";
		private string trueInitPath = "";	
		public int kl7;
		public double mind, maxu, mindf, maxuf;
		public double mu1,mu2,mu3,mu4,mu;
		public double md1,md2,md3,md4,md;
		public int mdi1,mdi2,mdi3,mdi4,mindi;
		public int mui1,mui2,mui3,mui4,maxui;
		
		public DateTime mdt1,mdt2,mdt3,mdt4;
		public DateTime mut1,mut2,mut3,mut4;
		public Text tU1,tU2,tU3,tU4,tD1,tD2,tD3,tD4,toolText,toolText1;
		
		public DateTime mindt, maxut;		
		public double kl,fbU,fbD;
		
		public double nkz;		
		
        protected override void Init()
        {	nu=false; nd=false; md=0; mu=0;
			first=true;
			InitFile();
		    _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
			// Рисуем зоны
			toolTrendLine4 = Tools.Create<TrendLine>(); toolTrendLine4.Color=Color.HotPink;toolTrendLine4.Width=3;
			toolTrendLine2 = Tools.Create<TrendLine>(); toolTrendLine2.Color=Color.Crimson;toolTrendLine2.Width=6;
			toolTrendLineD = Tools.Create<TrendLine>(); toolTrendLineD.Color=Color.ForestGreen;toolTrendLineD.Width=6;
			// Рисуем линии волн
			toolTrendLineU1 = Tools.Create<TrendLine>(); toolTrendLineU1.Color=Color.Aqua;toolTrendLineU1.Width=3;
			toolTrendLineU2 = Tools.Create<TrendLine>(); toolTrendLineU2.Color=Color.Aqua;toolTrendLineU2.Width=3;
			toolTrendLineU3 = Tools.Create<TrendLine>(); toolTrendLineU3.Color=Color.Aqua;toolTrendLineU3.Width=3;
			toolTrendLineU4 = Tools.Create<TrendLine>(); toolTrendLineU4.Color=Color.Aqua;toolTrendLineU4.Width=3;			
			toolTrendLineD1 = Tools.Create<TrendLine>(); toolTrendLineD1.Color=Color.Aqua;toolTrendLineD1.Width=3;
			toolTrendLineD2 = Tools.Create<TrendLine>(); toolTrendLineD2.Color=Color.Aqua;toolTrendLineD2.Width=3;
			toolTrendLineD3 = Tools.Create<TrendLine>(); toolTrendLineD3.Color=Color.Aqua;toolTrendLineD3.Width=3;	
			toolTrendLineD4 = Tools.Create<TrendLine>(); toolTrendLineD4.Color=Color.Aqua;toolTrendLineD4.Width=3;	
			
			toolText = Tools.Create<Text>();toolText.Color=Color.Aqua;
			toolText1 = Tools.Create<Text>();toolText1.Color=Color.Aqua;
			
			kf=0.909090909090909;
			
			tU1 = Tools.Create<Text>(); tU1.Color=Color.Aqua;
			tU2 = Tools.Create<Text>(); tU2.Color=Color.Aqua;
			tU3 = Tools.Create<Text>(); tU3.Color=Color.Aqua;
			tU4 = Tools.Create<Text>(); tU4.Color=Color.Aqua;
			
			tD1 = Tools.Create<Text>(); tD1.Color=Color.Aqua;
			tD2 = Tools.Create<Text>(); tD2.Color=Color.Aqua;
			tD3 = Tools.Create<Text>(); tD3.Color=Color.Aqua;
			tD4 = Tools.Create<Text>(); tD4.Color=Color.Aqua;
			
			trueLogPath=PathToLogFile+"\\"+L1+"_"+Instrument.Name.ToString()+".LOG"; 
			trueBuyPath=PathToLogFile+"\\"+Instrument.Name.ToString()+".DAT"; 
			//trueBuyPath=PathToLogFile+"\\01_TORG.LOG";			
			
        }        
//===========================================================================
        protected override void NewBar()
        {   
			
			ci = Bars.Range.To - 1;
			DTime = Bars[ci].Time;
			

//====== START  FIRST  ==============================================================================================				
			if(first) 
		    {   first=false;
//========== История ================================================================================================

				
//===================================================================================================================
				
				if(tu) { 
					 zmax = Math.Round(kl,Instrument.PriceScale);
					 nkz4 = zmax-(NKZ*Instrument.Point);
				     nkz2 = zmax-((NKZ*2)*Instrument.Point);
			         nkz4v= zmax-((NKZ*kf)*Instrument.Point); 
				     nkz2v= zmax-(((NKZ*2)*kf)*Instrument.Point);
					
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
					
toolTrendLineD.Point1= new ChartPoint(DTime, zmax); toolTrendLineD.Point2= new ChartPoint(DTime, nkz2v);	
toolText.Point=new ChartPoint(DTime, nkz4v+Instrument.Spread); toolText.Caption=string.Format("{0}",nkz4v);				
toolTrendLine4.Point1= new ChartPoint(DTime, nkz4v); toolTrendLine4.Point2= new ChartPoint(DTime.AddDays(3), nkz4v);	
toolText1.Point=new ChartPoint(DTime, nkz2v+Instrument.Spread);toolText1.Caption=string.Format("{0}",nkz2v);					
toolTrendLine2.Point1= new ChartPoint(DTime, nkz2v); toolTrendLine2.Point2= new ChartPoint(DTime.AddDays(3), nkz2v);
						}
				
				if(td) { 		
					 zmin = Math.Round(kl,Instrument.PriceScale);
					Print("ZMin={0}",zmin);
					 nkz4 = zmin+(NKZ*Instrument.Point);
				     nkz2 = zmin+((NKZ*2)*Instrument.Point);
			         nkz4v= zmin+((NKZ*kf)*Instrument.Point); 
				     nkz2v= zmin+(((NKZ*2)*kf)*Instrument.Point);
					
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale);
					
toolTrendLineD.Point1= new ChartPoint(DTime, zmin); toolTrendLineD.Point2= new ChartPoint(DTime, nkz2v);		
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
toolTrendLineD.Point1= new ChartPoint(tmax, zmax); toolTrendLineD.Point2= new ChartPoint(tmax, nkz2v);					
toolTrendLine4.Point1= new ChartPoint(tmax, nkz4v); toolTrendLine4.Point2= new ChartPoint(tmax.AddDays(3), nkz4v);					
toolText.Point=new ChartPoint(tmax, nkz4v+Instrument.Spread);toolText.Caption=string.Format("{0}",nkz4v);				
toolTrendLine2.Point1= new ChartPoint(tmax, nkz2v); toolTrendLine2.Point2= new ChartPoint(tmax.AddDays(3), nkz2v);
toolText1.Point=new ChartPoint(tmax, nkz2v+Instrument.Spread);toolText1.Caption=string.Format("{0}",nkz2v);				
					
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

toolTrendLineD.Point1= new ChartPoint(tmin, zmin); toolTrendLineD.Point2= new ChartPoint(tmin, nkz2v);	
toolTrendLine4.Point1= new ChartPoint(tmin, nkz4v); toolTrendLine4.Point2= new ChartPoint(tmin.AddDays(3), nkz4v);	
toolText.Point=new ChartPoint(tmin, nkz4v);toolText.Caption=string.Format("{0}",nkz4v);						
toolTrendLine2.Point1= new ChartPoint(tmin, nkz2v); toolTrendLine2.Point2= new ChartPoint(tmin.AddDays(3), nkz2v);						
toolText1.Point=new ChartPoint(tmin, nkz2v);toolText1.Caption=string.Format("{0}",nkz2v);						

		 		}
//== Касание зоны/1\2 ======================================================================================================
			if(n2) {
				if (tu && Bars[ci].Low<nkz2v && !nu) nu=true; //Print("{1} - {0} Пересечение Ввниз 1/2",Bars[Bars.Range.To-1].Time,Instrument.Name);}
				if (td && Bars[ci].High>nkz2v && !nd) nd=true; //Print("{1} - {0} Пересечение Верх 1/2",Bars[Bars.Range.To-1].Time,Instrument.Name);}
			}
			else  {
				if (tu && Bars[ci].Low<nkz4v && !nu)   nu=true;// Print("{1} - {0} Пересечение Вниз 1/4",Bars[Bars.Range.To-1].Time,Instrument.Name);}
				if (td && Bars[ci].High>nkz4v && !nd)  nd=true; //Print("{1} - {0} Пересечение Вверх 1/4",Bars[Bars.Range.To-1].Time,Instrument.Name);}
			}				

//===================================================================================================================================			
			if(_ftoInd.FisherSeries[Bars.Range.To-2]<0 && _ftoInd.FisherSeries[Bars.Range.To-1]>0 ) // MIN
			{ kl7=0;  mind=double.MaxValue; mindf=double.MaxValue; 
				do 
				{   kl7++;         
					if(Bars[Bars.Range.To-kl7].Low<mind) {mind=Bars[Bars.Range.To-kl7].Low; mindt=Bars[Bars.Range.To-kl7].Time; mindi=Bars.Range.To-kl7; }
					if(_ftoInd.FisherSeries[Bars.Range.To-kl7]<mindf) mindf=_ftoInd.FisherSeries[Bars.Range.To-kl7];
				}
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl7]<0  &&  _ftoInd.FisherSeries[Bars.Range.To-kl7-1]>0) && kl7<1000);
					if(mindf<-kf1) 
					{ 	if(mui1>mdi1) 
						 {  md4=md3;   md3=md2;   md2=md1;   md1=mind;  
						    mdt4=mdt3; mdt3=mdt2; mdt2=mdt1; mdt1=mindt;
				            mdi4=mdi3; mdi3=mdi2; mdi2=mdi1; mdi1=mindi;
						 } else { if (mind<md1) { md1=mind; mdt1=mindt; mdi1=mindi; } }	
						 
					 if(mu1>0 && md1>0 && md2>0) fbD=Math.Round((mu1-md1)/(mu1-md2),2); else fbD=-1;
					 
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
						if(md1>0)  { tD1.Point=new ChartPoint(mdt1, md1); tD1.Caption=string.Format("D1-{0}",fbD); }
						if(md2>0)  { tD2.Point=new ChartPoint(mdt2, md2); tD2.Caption=string.Format("D2"); }
						if(md3>0)  { tD3.Point=new ChartPoint(mdt3, md3); tD3.Caption=string.Format("D3"); }
						if(md4>0)  { tD4.Point=new ChartPoint(mdt4, md4); tD4.Caption=string.Format("D4"); }
					}	
			} 
			
			if( _ftoInd.FisherSeries[Bars.Range.To-2]>0 && _ftoInd.FisherSeries[Bars.Range.To-1]<0 ) // MAX
				{ kl7=0;  maxu=double.MinValue; maxuf=double.MinValue;
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
					  } else { if (maxu>mu1) { mu1=maxu; mut1=maxut; mui1=maxui;} }	
				  
				    if(mu1>0 && md1>0 && mu2>0) fbU=Math.Round((mu1-md1)/(mu2-md1),2); else fbU=-1;
					
if(mu1>0  && md1>0) { toolTrendLineU1.Point1= new ChartPoint(mut1, mu1); toolTrendLineU1.Point2= new ChartPoint(mdt1, md1); }				  
if(mu2>0  && md1>0) { toolTrendLineD1.Point1= new ChartPoint(mdt1, md1); toolTrendLineD1.Point2= new ChartPoint(mut2, mu2); } 
if(mu2>0  && md2>0) { toolTrendLineU2.Point1= new ChartPoint(mut2, mu2); toolTrendLineU2.Point2= new ChartPoint(mdt2, md2); }
if(mu3>0  && md2>0) { toolTrendLineD2.Point1= new ChartPoint(mdt2, md2); toolTrendLineD2.Point2= new ChartPoint(mut3, mu3); }
if(mu3>0  && md3>0) { toolTrendLineU3.Point1= new ChartPoint(mut3, mu3); toolTrendLineU3.Point2= new ChartPoint(mdt3, md3); }
if(mu4>0  && md3>0) { toolTrendLineD3.Point1= new ChartPoint(mdt3, md3); toolTrendLineD3.Point2= new ChartPoint(mut4, mu4); }
if(mu4>0  && md4>0) { toolTrendLineU4.Point1= new ChartPoint(mut4, mu4); toolTrendLineU4.Point2= new ChartPoint(mdt4, md4); }	

						if(mu1>0)  { tU1.Point=new ChartPoint(mut1, mu1+Instrument.Spread*2); tU1.Caption=string.Format("U1-{0}",fbU); }
						if(mu2>0)  { tU2.Point=new ChartPoint(mut2, mu2+Instrument.Spread*2); tU2.Caption=string.Format("U2"); }
						if(mu3>0)  { tU3.Point=new ChartPoint(mut3, mu3+Instrument.Spread*2); tU3.Caption=string.Format("U3"); }
						if(mu4>0)  { tU4.Point=new ChartPoint(mut4, mu4+Instrument.Spread*2); tU4.Caption=string.Format("U4"); }
						if(md1>0)  { tD1.Point=new ChartPoint(mdt1, md1); tD1.Caption=string.Format("D1"); }
						if(md2>0)  { tD2.Point=new ChartPoint(mdt2, md2); tD2.Caption=string.Format("D2"); }
						if(md3>0)  { tD3.Point=new ChartPoint(mdt3, md3); tD3.Caption=string.Format("D3"); }
						if(md4>0)  { tD4.Point=new ChartPoint(mdt4, md4); tD4.Caption=string.Format("D4"); }
				  }  
       		 	} 
//=================================================================================================================================== 
// if(td && nd) Print("{6}  ----  nd={0} td={1} fbU={2} md1={3} md2={4} FS={5}",nd,td,fbU,md1,md2,_ftoInd.FisherSeries[Bars.Range.To-1],DTime);
	//if(td && nd) Print("{0} ---- {1}",DTime,(nd && td && fbU>0.4 && fbU<0.8 && md1<md2 && _ftoInd.FisherSeries[Bars.Range.To-1]<-0.900));			
if(nd && td && fbU>0.4 && fbU<0.8 && md1<md2 && _ftoInd.FisherSeries[Bars.Range.To-1]<-0.900)
		{           PRD=Math.Round((Bars[ci].Close-zmin)*Math.Pow(10,Instrument.PriceScale),0);
					UBD=Math.Round((mu2-Bars[ci].Close)*Math.Pow(10,Instrument.PriceScale),0);
					PRDi=(int)PRD;
					UBDi=(int)UBD;
					PCD=Math.Round(PRD/UBD, 2);
//Print("SELL - PRDi={0} UBDi={1} PCD={2}",PRDi,UBDi,PCD);
						if (posGuidSell==Guid.Empty && PCD>kof) 
							{
							var res2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,	
									Stops.InPips(UBDi,PRDi),
									null, null);
							if (res2.IsSuccessful)  posGuidSell=res2.Position.Id;
							}		
		}
// if(tu && nu) Print("{6}  ----  nu={0} tu={1} fbU={2}  mu1={3} mu2={4} FS={5}",nu,tu,fbD,mu1,mu2,_ftoInd.FisherSeries[Bars.Range.To-1],DTime);
//if(tu && nu) Print("{0} -- {1}",DTime,(nu && tu && fbD>0.5 && fbD<0.8 && mu1>mu2 && _ftoInd.FisherSeries[Bars.Range.To-1]>0.900) );
if(nu && tu && fbD>0.5 && fbD<0.8 && mu1>mu2 && _ftoInd.FisherSeries[Bars.Range.To-1]>0.900) 
	    { 
			PRU=Math.Round((zmax-Bars[ci].Close)*Math.Pow(10,Instrument.PriceScale),0);
			UBU=Math.Round((Bars[ci].Close-md2)*Math.Pow(10,Instrument.PriceScale),0);
			PRUi=(int)PRU;
			UBUi=(int)UBU;
			PCU=Math.Round(PRU/UBU, 2);
						if (posGuidBuy==Guid.Empty && PCU>kof) { 
							var res1 = Trade.OpenMarketPosition(Instrument.Id, 
								ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								Stops.InPips(UBUi,PRUi), 
								null, null);
							if (res1.IsSuccessful)  posGuidBuy=res1.Position.Id;
													}
		}
if(tu) XXPrint("{0} TU --- kl={1} nu={2} n2={3} NKZ={4} TP2={5} fbU={6} FS={7}",DTime,kl,nu,n2,NKZ,tp2,fbU,Math.Round(_ftoInd.FisherSeries[Bars.Range.To-1],2)); 
if(td) XXPrint("{0} TD --- kl={1} nd={2} n2={3} NKZ={4} TP2={5} fbD={6} FS={7}",DTime,kl,nd,n2,NKZ,tp2,fbD,Math.Round(_ftoInd.FisherSeries[Bars.Range.To-1],2));

//==========UPDATE  STOP ==========================================
   if (posGuidSell!=Guid.Empty && sl1!=mu2 ) 	
	{sl1=mu2; var res5 = Trade.UpdateMarketPosition(posGuidSell,mu2,Trade.GetPosition(posGuidSell).TakeProfit, null); 
		    if (res5.IsSuccessful) posGuidSell=res5.Position.Id;}
	
   if (posGuidBuy!=Guid.Empty && bu1!=md2) 	
	{bu1=md2; var res6 = Trade.UpdateMarketPosition(posGuidBuy,md2,Trade.GetPosition(posGuidBuy).TakeProfit, null); 
		    if (res6.IsSuccessful) posGuidBuy=res6.Position.Id;}
	
   if (posGuidSell2!=Guid.Empty && sl2!=mu2) 	
	{sl2=mu2; var res7 = Trade.UpdateMarketPosition(posGuidSell2,mu2,Trade.GetPosition(posGuidSell2).TakeProfit, null); 
		    if (res7.IsSuccessful) posGuidSell=res7.Position.Id;}
	
   if (posGuidBuy2!=Guid.Empty && bu2!=md2) 	
	{bu2=md2; var res8 = Trade.UpdateMarketPosition(posGuidBuy2,md2,Trade.GetPosition(posGuidBuy2).TakeProfit, null); 
		    if (res8.IsSuccessful) posGuidBuy2=res8.Position.Id;}			
		
  } // NewBar
		
//============================================================================================================	

        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            if (type==ModificationType.Closed)
            {
               if (posGuidBuy==position.Id)  posGuidBuy=Guid.Empty;  
		       if (posGuidSell==position.Id) posGuidSell=Guid.Empty;
			   if (posGuidBuy2==position.Id)  posGuidBuy2=Guid.Empty;  
		       if (posGuidSell2==position.Id) posGuidSell2=Guid.Empty;
            }
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
	
//== Log  File ===================================================================================================   

		
		protected void XXPrint(string xxformat, params object[] parameters)
		{
				var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);
		}
		protected void XPrint(string xxformat, params object[] parameters)
		{
				var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueBuyPath, logString);
		}

    }
}

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
    [TradeSystem("NF1_v1")]              
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Номер версии", DefaultValue = 1)]
        public int ver { get; set; }

		[Parameter("Lot:", DefaultValue = 0.1)]
        public double lot { get; set; }			
		[Parameter("StopLoss:", DefaultValue = 250)]
        public int SL1 { get; set; }	
		[Parameter("Отступ Stop :", DefaultValue = 50)]
		public int dl { get;set; }	
		[Parameter("Fractal", DefaultValue = 5)]
		public int frac { get;set; }	
		[Parameter("Кофициент :", DefaultValue = 2)]
		public int kof { get;set; }			
		
		[Parameter("Log_Path", DefaultValue = @"1")]
		public string L1 { get;set; }
		[Parameter("Init_FileName", DefaultValue = @"m")]
		public string BuyFileName { get;set; }
		
		private bool tu,td,nu,nd,n2;
		private int NKZ,i;
		private VerticalLine vl; 
		private HorizontalLine hl;
		public int k=0,k2=0;
			public Fractals _frInd;	
			public FisherTransformOscillator _ftoInd;
		public DateTime DTime; // Время
		private int ci = 0,kl;
		private string st,st2,name,name2;
		private double nkz2,nkz4,nkz2v,nkz4v,kf,zmax,zmin,TP2;

	    private double dlt,frUp,frDown;
	
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private Guid posGuidBuy2=Guid.Empty;
		private Guid posGuidSell2=Guid.Empty;		
		
		public double frU1,frU2,frU3;    // Значение текущего верхнего Fractal
		public double frD1,frD2,frD3;    // Значение Low - свечи с верхним фрактклом
		public double fsU1,fsU2,fsU3;    // Значение текущего верхнего Fisher
		public double fsD1,fsD2,fsD3;    	

		public double TPD,TPU,SLU,SLD;
		public double PRD,PRU,PRD2,PRU2,PCU,PCD,PCU2,PCD2,UBU,UBD,Ot;		
		public int PRDi,PRUi,PRDi2,PRUi2,UBDi,UBUi;
		public DateTime tmU1,tmU2,tmU3,tmD1,tmD2,tmD3,tmax,tmin;
		public bool frU,frD,bu1,bu2;
		
		public static StreamReader LogSW = null;
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		private string trueLogPath = "";	
		private string trueBuyPath = "";
		private string trueInitPath = "";		
		
        protected override void Init()
        {	nu=false; nd=false; tu=false; td=false; 
			InitFile();
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			_frInd.Range=frac-2;
			
			vl = Tools.Create<VerticalLine>();
			vl.Width=2;
			hl = Tools.Create<HorizontalLine>();
			
			k=0; // Для першого входження -
			kf=0.090909;
			dlt=dl*Instrument.Point; // Отступ от стопа
			
			frU1=0.0;frU2=0.0;frU3=0.0;
			frD1=0.0;frD2=0.0;frD3=0.0;
			fsU1=0.0;fsU2=0.0;fsU3=0.0;
			fsD1=0.0;fsU2=0.0;fsU3=0.0;

			trueLogPath=PathToLogFile+"\\"+L1+"_"+Instrument.Name.ToString()+".LOG"; 
			trueBuyPath=PathToLogFile+"\\01_TORG.LOG";		
        }        
//===========================================================================
        protected override void NewBar()
        {   
			DTime = Bars[ci].Time;
			ci = Bars.Range.To - 1;
			
//=== КОРЕКЦИЯ =======================================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed)     { posGuidBuy=Guid.Empty;   bu1=true; }  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed)   { posGuidSell=Guid.Empty;  bu1=true;  } 
			if (posGuidBuy2!=Guid.Empty && Trade.GetPosition(posGuidBuy2).State==PositionState.Closed)   { posGuidBuy2=Guid.Empty;  bu2=true;  }  
		    if (posGuidSell2!=Guid.Empty && Trade.GetPosition(posGuidSell2).State==PositionState.Closed) { posGuidSell2=Guid.Empty; bu2=true; } 

//========  начальная инициализация	==================================================================		
			if(k==0) InitFr();
			if(k==1) 
		    { 
				if(tu) {
					vl.Time = tmax;vl.Color=Color.Red;
				     //nkz4 = zmax-((NKZ-5-(NKZ*kf))*Instrument.Point);
					 nkz4 = zmax-(NKZ*Instrument.Point);
				     nkz2 = zmax-((NKZ*2)*Instrument.Point);
					
			         nkz4v= zmax-((NKZ*0.9)*Instrument.Point); 
				     nkz2v= zmax-(((NKZ*2)*0.9)*Instrument.Point);
					
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale);
					
					 XXPrint(" Init - {0} - frU1={1}({4}) frU2={2}({5}) frU3={3}({6}) zmax={7}({8})", 
						Bars[Bars.Range.To-1].Time,frU1,frU2,frU3,tmU1,tmU2,tmU3,zmax,tmax);
					hl.Price=nkz4v;hl.Text=Math.Round((nkz4-zmin)*Math.Pow(10,Instrument.PriceScale),0).ToString();
			}
			if(td) { 
					vl.Time = tmin; vl.Color=Color.Blue;
					 nkz4 = zmin+(NKZ*Instrument.Point);
				     nkz2 = zmin+((NKZ*2)*Instrument.Point);
					
			         nkz4v= zmin+((NKZ*0.9)*Instrument.Point); 
				     nkz2v= zmin+(((NKZ*2)*0.9)*Instrument.Point);
				
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale);
					 XXPrint(" Init - {0} - frD1={4}({1}) frD2={5}({2}) frD3={6}({3})  zmin={7}({8})", 
						Bars[Bars.Range.To-1].Time,tmD1,tmD2,tmD3,frD1,frD2,frD3,zmin,tmin);
				    hl.Price=nkz4v;hl.Text=Math.Round((nkz4-zmin)*Math.Pow(10,Instrument.PriceScale),0).ToString();
			}
 		    }
		    k++;	
		
//====  Fractal вверх ================================================================================================================
	  if (_frInd.TopSeries[Bars.Range.To-frac]>0) 		
		{  
				  if(frU) { frU=false;		
				  			frU3=frU2; frU2=frU1; frU1=_frInd.TopSeries[Bars.Range.To-frac];
				  			tmU3=tmU2; tmU2=tmU1; tmU1=Bars[Bars.Range.To-frac].Time;
					        fsU3=fsU2; fsU2=fsU1; fsU1=_ftoInd.FisherSeries[Bars.Range.To-frac];
				  		   } else 
				   		   {
					  		if(_frInd.TopSeries[Bars.Range.To-frac]>frU1) {
						  			frU1=_frInd.TopSeries[Bars.Range.To-frac];
									fsU1=_ftoInd.FisherSeries[Bars.Range.To-frac];
					  	  			tmU1=Bars[Bars.Range.To-frac].Time;  }
				           }
			 Print("{2} - Top {0} {1}",Bars[Bars.Range.To-frac].Time,_frInd.TopSeries[Bars.Range.To-frac],Instrument.Name);			   
		 }
//====  Fractal вниз ====================================================================================			  
		if (_frInd.BottomSeries[Bars.Range.To-frac]>0)   
		  {  
				  if(!frU) { frU=true;
				     frD3=frD2; frD2=frD1; frD1=_frInd.BottomSeries[Bars.Range.To-frac];
				     tmD3=tmD2; tmD2=tmD1; tmD1=Bars[Bars.Range.To-frac].Time;
					 fsD3=fsD2; fsD2=fsD1; fsD1=_ftoInd.FisherSeries[Bars.Range.To-frac]; 
				  } else 
				  {
					 if (_frInd.BottomSeries[Bars.Range.To-frac]<frD1) {
						 frD1=_frInd.BottomSeries[Bars.Range.To-frac];
						 fsD1=_ftoInd.FisherSeries[Bars.Range.To-frac];
					     tmD1=Bars[Bars.Range.To-frac].Time;  } 
				  }
			Print("{2} - Bootom {0} {1}",Bars[Bars.Range.To-frac].Time,_frInd.BottomSeries[Bars.Range.To-frac],Instrument.Name);
		   } 			
//====================================================================================================================      		

		  if (tu && Bars[ci].High>zmax) 
		  		{ 
			  		zmax=Bars[ci].High;
					tmax=Bars[ci].Time;
					vl.Time = tmax;vl.Color=Color.Red;
					 nkz4 = zmax-(NKZ*Instrument.Point);
				     nkz2 = zmax-((NKZ*2)*Instrument.Point);
					
			         nkz4v= zmax-((NKZ*0.9)*Instrument.Point); 
				     nkz2v= zmax-(((NKZ*2)*0.9)*Instrument.Point);
					
			        nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			        nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			        nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			        nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
			 		nd=false; nu=false; 	
					hl.Price=nkz4v;hl.Text=Math.Round((nkz4-zmin)*Math.Pow(10,Instrument.PriceScale),0).ToString();
		  		}
		  
		  if (td && Bars[ci].Low<zmin ) 
		  		{ 
			  		zmin=Bars[ci].Low;
					tmin=Bars[ci].Time;
					vl.Time = tmin;vl.Color=Color.Blue;
				
					 nkz4 = zmin+(NKZ*Instrument.Point);
				     nkz2 = zmin+((NKZ*2)*Instrument.Point);
					
			         nkz4v= zmin+((NKZ*0.9)*Instrument.Point); 
				     nkz2v= zmin+(((NKZ*2)*0.9)*Instrument.Point);
					
					
			        nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			        nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			        nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			        nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
					nd=false; nu=false; 
					hl.Price=nkz4v;hl.Text=Math.Round((nkz4-zmin)*Math.Pow(10,Instrument.PriceScale),0).ToString();
		 		}
//== Касание зоны/1\2 ======================================================================================================
			if(n2) {
				if (tu && Bars[ci].Low<nkz2v && !nu)  { nu=true; }
				if (td && Bars[ci].High>nkz2v && !nd) { nd=true; }
			}
			else  {
				if (tu && Bars[ci].Low<nkz4v && !nu)  { nu=true; }
				if (td && Bars[ci].High>nkz4v && !nd) { nd=true; }
			}				
// Если пересечение зоны было (nu) и торгуем вверх (tu) ===========================================================		
	if(nu && tu) 
		{
//================================ Определение патерна   ПИК ПОСЛЕДНЕГО ФРАКТАЛА ПАТЕРНА ВНИЗУ =====================
		   if (_frInd.TopSeries[Bars.Range.To-frac]>0)    
		   {   Print("{2} - PATERN Top {0} {1}",Bars[Bars.Range.To-frac].Time,_frInd.TopSeries[Bars.Range.To-frac],Instrument.Name);
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
					PRUi2=(int)PRU2;
					UBUi=(int)UBU;
					//UBUi=(int)SL1;
					if(UBU>40 && UBU<SL1 && Bars[ci].Close>nkz2 && PCU>kof  && fsU1>0 && fsU2<0 && fsU3<0) 
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
//=============== Определение патерна   ПИК ПОСЛЕДНЕГО ФРАКТАЛА ПАТЕРНА ВВЕРХУ =====================================
 			if (_frInd.BottomSeries[Bars.Range.To-frac]>0)	   
			{    Print("{2} - PATERN Bootom {0} {1}",Bars[Bars.Range.To-frac].Time,_frInd.BottomSeries[Bars.Range.To-frac],Instrument.Name);         
				if( frD2>frD3 && frD2>frD1  && frD1>0 && frD2>0 && frD3>0) 
				{   TPD=Math.Round(zmin, Instrument.PriceScale);
					SLD=frU2+Instrument.Spread+dlt; SLD=Math.Round(SLD, Instrument.PriceScale);
					PRD=Math.Round((Bars[ci].Close-TPD)*Math.Pow(10,Instrument.PriceScale),0);
					PRD2=Math.Round((Bars[ci].Close-TP2)*Math.Pow(10,Instrument.PriceScale),0);
					Ot=Math.Round((frD2-frD3)/(frD2-frD1), 2);
					UBD=Math.Round((SLD-Bars[ci].Close)*Math.Pow(10,Instrument.PriceScale),0);
					
					PRDi2=(int)PRD2;
					PRDi=(int)PRD;
					PRDi2=(int)PRD2;
					UBDi=(int)UBD;
					//UBDi=(int)SL1;
					PCD=Math.Round(PRD/UBD, 2);
					PCD2=Math.Round(PRD2/UBD, 2);
					if(UBD>40 && UBD<SL1 && Bars[ci].Close<nkz2 && PCD>kof && fsD1<0 && fsD2>0 && fsD3>0) 
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
//======  Логи  =====================================================================================================================   
	st=""; st2="";
	if (_frInd.TopSeries[Bars.Range.To-frac]>0) 
	{  
		if( frU3>frU2 && frU1>frU2 && frU1>0 && frU2>0 && frU3>0 && tu ) //Патерн
					{             
		 var toolPolyLine = Tools.Create<PolyLine>();
		 toolPolyLine.Color = Color.Red;
		 if(nu) toolPolyLine.Width = 1;  else toolPolyLine.Width = 3;
         toolPolyLine.AddPoint(new ChartPoint(tmU1, frU1));
         toolPolyLine.AddPoint(new ChartPoint(tmU2, frU2));
         toolPolyLine.AddPoint(new ChartPoint(tmU3, frU3)); 
		 
		 var toolText = Tools.Create<Text>(); 
        toolText.Point=new ChartPoint(tmU1, frU1);
		toolText.FontSize=6;
        toolText.Caption=string.Format("{0} SL={1} PR={2}",PCU,UBUi,PRUi);
		 
						}
			if(tu) //Торгуем в Buy
			{	st=st+" tu --";
				if(nu) // Есть пересечение 1/4
				{	st=st+"++";
					if( frU3>frU2 && frU1>frU2 ) //Патерн
					{
						if(UBU>20 && UBU<SL1 && Bars[ci].Close>nkz2 && PCU>kof) {
						     st=st+"@@ ";
							if (posGuidBuy!=Guid.Empty) { st=st+"Buy1 "; 
								          st2=Trade.GetPosition(posGuidBuy).Number.ToString()+" "+Instrument.Name+" Buy1 "; }
							if (posGuidBuy2!=Guid.Empty) { st=st+"Buy2 "; 
	          							  st2=Trade.GetPosition(posGuidBuy2).Number.ToString()+" "+Instrument.Name+" Buy2 "; } 
							if (posGuidBuy==Guid.Empty && posGuidBuy2==Guid.Empty) st=st+"!! ";
						    st=st+"| SL="+UBU.ToString()+" TP="+PRU.ToString()+" %="+PCU.ToString();
							st2=st2+"| SL="+UBU.ToString()+" TP="+PRU.ToString()+" %="+PCU.ToString();
						}
					}
				}
			XXPrint("{1} | {0} | frU1={2}:{3}({4}) frU2={5}:{6}({7}) frU3={8}:{9}({10}) -- zmax={11}({13}) nkz4={12}",Bars[Bars.Range.To-1].Time,st,
				tmU3.Hour,tmU3.Minute,frU3,tmU2.Hour,tmU2.Minute,frU2,tmU1.Hour,tmU1.Minute,frU1,zmax,nkz4,tmax);
			 XPrint("{1} | {0} | frU= {2}:{3}({4}) {5}:{6}({7}) {8}:{9}({10}) | max={11}({13}) nkz4={12}",
				Bars[Bars.Range.To-1].Time,st2,tmU3.Hour,tmU3.Minute,frU3,tmU2.Hour,tmU2.Minute,frU2,tmU1.Hour,tmU1.Minute,frU1,zmax,nkz4,tmax);				
			}
	}
	if (_frInd.BottomSeries[Bars.Range.To-frac]>0) 
	{   
		if( frD2>frD3 && frD2>frD1 && frD1>0 && frD2>0 && frD3>0 && td ) //Патерн
		{
		 var toolPolyLine = Tools.Create<PolyLine>();
		 toolPolyLine.Color = Color.Blue;
		 if(nd) toolPolyLine.Width = 1;  else toolPolyLine.Width = 3;
         toolPolyLine.AddPoint(new ChartPoint(tmD1, frD1));
         toolPolyLine.AddPoint(new ChartPoint(tmD2, frD2));
         toolPolyLine.AddPoint(new ChartPoint(tmD3, frD3)); 
		var toolText = Tools.Create<Text>(); 
        toolText.Point=new ChartPoint(tmD1, frD1);
		toolText.FontSize=6;
        toolText.Caption=string.Format("{0} SL={1} PR={2}",PCD,UBDi,PRDi);
		 
		}
			if(td) //Торгуем в Buy
			{	st=st+" td --";
				if(nd) // Есть пересечение 1/4
				{   st=st+"++";
					if( frD2>frD3 && frD2>frD1 ) //Патерн
					{
						if(UBD>20 && UBD<SL1 && Bars[ci].Close<nkz2 && PCD>2) {
							 st=st+"@@ ";
							if (posGuidSell!=Guid.Empty)  { st=st+"Sell1 ";
								st2=Trade.GetPosition(posGuidSell).Number.ToString()+" "+Instrument.Name+" Sell1 "; }
							if (posGuidSell2!=Guid.Empty) { st=st+"Sell2 "; 
								st2=Trade.GetPosition(posGuidSell2).Number.ToString()+" "+Instrument.Name+" Sell2 "; } 
							if (posGuidSell==Guid.Empty && posGuidSell2==Guid.Empty) st=st+"!! ";
								st=st+"| SL="+UBD.ToString()+" TP="+PRD.ToString()+" %="+PCD.ToString();
							    st2=st2+"| SL="+UBD.ToString()+" TP="+PRD.ToString()+" %="+PCD.ToString();
						}
					}	
				}
			 XXPrint("{1} | {0} | frD1={2}:{3}({4}) frD2={5}:{6}({7}) frD3={8}:{9}({10}) | zmin={11}({13}) nkz4={12}",Bars[Bars.Range.To-1].Time,st,
				tmD3.Hour,tmD3.Minute,frD3,tmD2.Hour,tmD2.Minute,frD2,tmD1.Hour,tmD1.Minute,frD1,zmin,nkz4,tmin);
			 XPrint("{1} | {0} | frD= {2}:{3}({4}) {5}:{6}({7}) {8}:{9}({10}) | min={11}({13}) nkz4={12}",
				Bars[Bars.Range.To-1].Time,st2,tmD3.Hour,tmD3.Minute,frD3,tmD2.Hour,tmD2.Minute,frD2,tmD1.Hour,tmD1.Minute,frD1,zmin,nkz4,tmin);			
			}
	}
	
//=============   LOG2  ========================================================================================================	
	if(tu) XXPrint("--- tu --- {0} | frU3={2}:{3}({4}) frU2={5}:{6}({7}) frU1={8}:{9}({10}) | max={11}({13}) | nkz4={12} nkz2={14}  nkz4v={15} nkz2v={16} nu={17}",Bars[Bars.Range.To-1].Time,st,
				tmU3.Hour,tmU3.Minute,frU3,tmU2.Hour,tmU2.Minute,frU2,tmU1.Hour,tmU1.Minute,frU1,zmax,nkz4,tmax,nkz2,nkz4v,nkz2v,nu);
	if(td) XXPrint("--- td --- {0} | frD3={2}:{3}({4}) frD2={5}:{6}({7}) frD1={8}:{9}({10}) | min={11}({13}) | nkz4={12} nkz2={14}  nkz4v={15} nkz2v={16} nd={17}",Bars[Bars.Range.To-1].Time,st,
				tmD3.Hour,tmD3.Minute,frD3,tmD2.Hour,tmD2.Minute,frD2,tmD1.Hour,tmD1.Minute,frD1,zmin,nkz4,tmin,nkz2,nkz4v,nkz2v,nd);
//========== STOP ==========================================
   if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).Pips>SL1 && bu1) 	
	{bu1=false; var res5 = Trade.UpdateMarketPosition(posGuidSell,Trade.GetPosition(posGuidSell).OpenPrice-Instrument.Spread-dlt,Trade.GetPosition(posGuidSell).TakeProfit, null); 
		    if (res5.IsSuccessful) posGuidSell=res5.Position.Id;}
	
   if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).Pips>SL1 && bu1) 	
	{bu1=false; var res6 = Trade.UpdateMarketPosition(posGuidBuy,Trade.GetPosition(posGuidBuy).OpenPrice+Instrument.Spread+dlt,Trade.GetPosition(posGuidBuy).TakeProfit, null); 
		    if (res6.IsSuccessful) posGuidBuy=res6.Position.Id;}
	
   if (posGuidSell2!=Guid.Empty && Trade.GetPosition(posGuidSell2).Pips>SL1 && bu2) 	
	{bu2=false; var res7 = Trade.UpdateMarketPosition(posGuidSell2,Trade.GetPosition(posGuidSell2).OpenPrice-Instrument.Spread-dlt,Trade.GetPosition(posGuidSell2).TakeProfit, null); 
		    if (res7.IsSuccessful) posGuidSell=res7.Position.Id;}
	
   if (posGuidBuy2!=Guid.Empty && Trade.GetPosition(posGuidBuy2).Pips>SL1 && bu2) 	
	{bu2=false; var res8 = Trade.UpdateMarketPosition(posGuidBuy2,Trade.GetPosition(posGuidBuy2).OpenPrice+Instrument.Spread+dlt,Trade.GetPosition(posGuidBuy2).TakeProfit, null); 
		    if (res8.IsSuccessful) posGuidBuy2=res8.Position.Id;}	
  } // NewBar
		
		
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
//====== History =================================================================================================		
 		protected void InitFr()
		{
				var i=0; 
			    var kfd=0; 
			    var kfu=0;
			zmin=double.MaxValue;
			zmax=0;
			while ( i<kl ) 
			{ 
		Print("{4} - frU={0}-kfu={1} frD={2}-kfd={3}",_frInd.TopSeries[Bars.Range.To-i],kfu,_frInd.BottomSeries[Bars.Range.To-i],kfd,Instrument.Name);
			  if (_frInd.TopSeries[Bars.Range.To-i]>0  && kfu<3)	{  kfu++;
				  frU1=frU2; frU2=frU3; frU3=_frInd.TopSeries[Bars.Range.To-i];
				  fsU3=fsU2; fsU2=fsU1; fsU1=_ftoInd.FisherSeries[Bars.Range.To-i];
				  tmU1=tmU2; tmU2=tmU3; tmU3=Bars[Bars.Range.To-i].Time;  
					  }

			  if (_frInd.BottomSeries[Bars.Range.To-i]>0 && kfd<3)   { kfd++; 
				  frD1=frD2; frD2=frD3; frD3=_frInd.BottomSeries[Bars.Range.To-i];
				  fsU3=fsU2; fsU2=fsU1; fsU1=_ftoInd.FisherSeries[Bars.Range.To-i];
				  tmD1=tmD2; tmD2=tmD3; tmD3=Bars[Bars.Range.To-i].Time;
					  } 
			  
			  if(Bars[Bars.Range.To-i].High>zmax) { zmax=Bars[Bars.Range.To-i].High; tmax=Bars[Bars.Range.To-i].Time; }
			  if(Bars[Bars.Range.To-i].Low<zmin)  { zmin=Bars[Bars.Range.To-i].Low;  tmin=Bars[Bars.Range.To-i].Time;  }
		

			  	  i++;
        	}			
					 Print("{9} - Init OK! ---- {0} frU1={1} frU2={2} frU3={3} frD1={4} frD2={5} frD3={6}  zmax={7} zmin={8}",
			  Bars[Bars.Range.To-i].Time,frU1,frU2,frU3,frD1,frD2,frD3,zmax,zmin,Instrument.Name);
			
		} 
//===============  END History ===============================================================================
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
									if(k2==2) { name=st; } // Print("{0} - {1}",k2,st ); }
									if(k2==3) {  kl=Convert.ToInt32(st);}// Print("----------------- {0} - {1}",k2,st ); }
									if(k2==4) { if(st=="1") tu=true;  if(st=="2") td=true; }// Print("----------------- {0} - {1}",k2,st ); }
									if(k2==5) { if(st=="1") nu=true;  if(st=="2") nd=true; }//Print("----------------- {0} - {1}",k2,st ); }
									if(k2==6) { if(st=="0") n2=false; if(st=="1") n2=true; }//Print("----------------- {0} - {1}",k2,st ); }
									if(k2==7) { NKZ=Convert.ToInt32(st); }//Print("----------------- {0} - {1} - {2}",k2,st,NKZ ); }
								}
								st="";
							} else { st = st+line[j]; }	
						  }
        				}
						if(Timeframe.ToString()=="M30") kl=kl*2;
						if(Timeframe.ToString()=="M15") kl=kl*4;
						if(Timeframe.ToString()=="M5") kl=kl*12;
						
						
		         Print("{14} - File INIT name={0} - kl={1} tu={2} td={3} nu={4} nd={5} n2={6} NKZ={7} TP2={8} lot={9} SL1={10} dl={11} frac={12} kof={13}",name,kl,tu,td,nu,nd,n2,NKZ,TP2,lot,SL1,dl,frac,kof,Instrument.Name);
			LogSW.Close(); 
			}  			
		}  
// END InitFile()  ===============================================================================

    }
}

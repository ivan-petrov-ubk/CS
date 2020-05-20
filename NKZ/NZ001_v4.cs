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
    [TradeSystem("NZ001_v4")]              //copy of "NZ001"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("NKZ :", DefaultValue = 117239)]
        public int dt1 { get; set; }
		[Parameter("fr1 :", DefaultValue = 117439)]
        public int fr1 { get; set; }
		[Parameter("tm1 :")]
        public DateTime tm1 { get; set; }

		[Parameter("fr2 :", DefaultValue = 117448)]
        public int fr2 { get; set; }
		[Parameter("tm2 :")]
        public DateTime tm2 { get; set; }
		
		
		[Parameter("Buy(tu):", DefaultValue = false)]
        public bool tu { get; set; }	
		[Parameter("nu:", DefaultValue = false)]
        public bool nu { get; set; }	

		[Parameter("Sell(td):", DefaultValue = false)]
        public bool td { get; set; }
		[Parameter("nd:", DefaultValue = false)]
        public bool nd { get; set; }	
		
		[Parameter("TP2:", DefaultValue = 0)]
        public int TP2 { get; set; }		
		[Parameter("StopLoss:", DefaultValue = 250)]
        public int SL1 { get; set; }	
		[Parameter("Отступ Stop :", DefaultValue = 50)]
		public int dl { get;set; }	
		[Parameter("Fractal", DefaultValue = 5)]
		public int frac { get;set; }	

	
		private int NKZ,i;

		public int k=0;
		public Fractals _frInd;	
		public DateTime DTime; // Время
		private int ci = 0;
		
		private double nkz2,nkz4,nkz2v,nkz4v,kf,zmax,zmin;

		// private PolyLine toolPolyLine;
		private Rectangle toolRectangle;
		private Rectangle toolRectangle1;	
		private Rectangle toolRectangle2;
		private ArrowUp toolArrowUp;
		private ArrowDown toolArrowDown;
        private Text NKZText;
		private Triangle toolTriangle;
		private HorizontalLine lnkz;
		
	
		private double dlt,frUp,frDown;
	
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private Guid posGuidBuy2=Guid.Empty;
		private Guid posGuidSell2=Guid.Empty;		
		
		public double frU1,frU2,frU3;   // Значение текущего верхнего Fractal
		public double frD1,frD2,frD3;    // Значение Low - свечи с верхним фрактклом

		public double TPD,TPU,SLU,SLD;
		public double PRD,PRU,PRD2,PRU2,PCU,PCD,PCU2,PCD2,UBU,UBD,Ot;		
		public int PRDi,PRUi,PRDi2,PRUi2,UBDi,UBUi;
		public DateTime tmU1,tmU2,tmU3,tmD1,tmD2,tmD3;
		public bool frU,frD;

		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		private string trueLogPath = "";	
		
        protected override void Init()
        {	
			
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			_frInd.Range=frac-2;

			// Прямоугольники 
			toolRectangle = Tools.Create<Rectangle>(); toolRectangle.BorderColor=Color.Aqua; toolRectangle.Color=Color.DarkSeaGreen;toolRectangle.BorderColor=Color.Blue;
			toolRectangle1 = Tools.Create<Rectangle>(); toolRectangle1.BorderColor=Color.Aqua; toolRectangle1.Color=Color.DarkSeaGreen;
			
			toolArrowUp = Tools.Create<ArrowUp>();
			toolArrowDown = Tools.Create<ArrowDown>();
			
			toolTriangle = Tools.Create<Triangle>();
			toolTriangle.Color=Color.DarkSeaGreen;
			toolTriangle.BorderColor=Color.Blue;
			toolTriangle.BorderWidth=2;
			
			NKZText = Tools.Create<Text>();
			NKZText.Color=Color.Blue;	 
			NKZText.FontSize=12;
			
			lnkz = Tools.Create<HorizontalLine>();

			// 10/09/2018
			if (Instrument.Name == "EURUSD")  NKZ=506;
			if (Instrument.Name == "GBPUSD")  NKZ=660;
			if (Instrument.Name == "USDJPY")  NKZ=570;
			if (Instrument.Name == "USDCHF")  NKZ=551;
			if (Instrument.Name == "AUDUSD")  NKZ=343;
			if (Instrument.Name == "USDCAD")  NKZ=508;
			if (Instrument.Name == "NZDUSD")  NKZ=330;
			if (Instrument.Name == "AUDCAD")  NKZ=357; 
			if (Instrument.Name == "AUDJPY")  NKZ=550;
			if (Instrument.Name == "AUDNZD")  NKZ=412;
			if (Instrument.Name == "CHFJPY")  NKZ=1430;
			if (Instrument.Name == "EURAUD")  NKZ=682; 
			if (Instrument.Name == "EURCAD")  NKZ=726; 
			if (Instrument.Name == "EURCHF")  NKZ=682; 
			if (Instrument.Name == "EURGBP")  NKZ=484; 
			if (Instrument.Name == "EURJPY")  NKZ=781; 
			if (Instrument.Name == "GBPCHF")  NKZ=924; 
			if (Instrument.Name == "GBPJPY")  NKZ=1045;

			k=0; // Для першого входження -
			kf=0.090909;
			dlt=dl*Instrument.Point; // Отступ от стопа

			frU1=0.0;frU2=0.0;frU3=0.0;frD1=0.0;frD2=0.0;frD3=0.0;
			
			if(tu) {  zmax = dt1*Instrument.Point;  
				frU1=fr1*Instrument.Point; tmU1=tm1;
				frU2=fr2*Instrument.Point; tmU2=tm2;
				frU3=fr2*Instrument.Point; tmU3=tm2;
				toolTriangle.Point1=new ChartPoint(tmU1,frU1);
        		toolTriangle.Point2=new ChartPoint(tmU2,frU2);
        		toolTriangle.Point3=new ChartPoint(tmU3,frU3);
				Print("INIT - {0} - frU1={1} frU2={2} frU3={3} - zmax={4}", Bars[Bars.Range.To-frac].Time,frU1,frU2,frU3,zmax);

			}
			if(td) { zmin = dt1*Instrument.Point;
				frD1=fr1*Instrument.Point; tmD1=tm1;
				frD2=fr2*Instrument.Point; tmD2=tm2;
				frD3=fr2*Instrument.Point; tmD3=tm2;
				toolTriangle.Point1=new ChartPoint(tmD1,frD1);
        		toolTriangle.Point2=new ChartPoint(tmD2,frD2);
        		toolTriangle.Point3=new ChartPoint(tmD3,frD3); 
				Print("INIT - {0} - frD1={1} frD2={2} frD3={3} - zmin={4}", Bars[Bars.Range.To-frac].Time,frD1,frD2,frD3,zmin);
			}		
			
			InitLogFile();  // Запись логов			
        }        
//===========================================================================
        protected override void NewBar()
        {   
			DTime = Bars[ci].Time;
			ci = Bars.Range.To - 1;
	 
//=== КОРЕКЦИЯ =======================================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) { posGuidBuy=Guid.Empty;  }  
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) { posGuidSell=Guid.Empty;  } 
			if (posGuidBuy2!=Guid.Empty && Trade.GetPosition(posGuidBuy2).State==PositionState.Closed) { posGuidBuy2=Guid.Empty;  }  
		    if (posGuidSell2!=Guid.Empty && Trade.GetPosition(posGuidSell2).State==PositionState.Closed) { posGuidSell2=Guid.Empty;  } 
			
//====  Fractal вверх ====================================================================================
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
				  if(tu) { 	toolTriangle.Point1=new ChartPoint(tmU1,frU1);
        					toolTriangle.Point2=new ChartPoint(tmU2,frU2);
        					toolTriangle.Point3=new ChartPoint(tmU3,frU3); }
				  XXPrint("{0} - frU1={1} frU2={2} frU3={3} frD1={4} frD2={5} frD3={6} nkz4={7} nkz2={8} zmax={9} zmin={10}", Bars[Bars.Range.To-frac].Time,frU1,frU2,frU3,frD1,frD2,frD3,nkz4,nkz2,zmax,zmin);
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
				  if(td) { 	toolTriangle.Point1=new ChartPoint(tmD1,frD1);
        					toolTriangle.Point2=new ChartPoint(tmD2,frD2);
        					toolTriangle.Point3=new ChartPoint(tmD3,frD3); }
				  XXPrint("{0} - frU1={1} frU2={2} frU3={3} frD1={4} frD2={5} frD3={6} nkz4={7} nkz2={8} zmax={9} zmin={10}", Bars[Bars.Range.To-frac].Time,frU1,frU2,frU3,frD1,frD2,frD3,nkz4,zmax,zmin);
		   } 			
// Первое начиртание зон	=====================================================================================		
          if(k==0) 
		  { 			
			nu=false; nd=false;

			if(tu) { zmax = dt1*Instrument.Point;  
				     nkz4 = zmax-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				     nkz2 = zmax-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			         nkz4v= zmax-((NKZ-5)*Instrument.Point); 
				     nkz2v= zmax-((NKZ*2)*Instrument.Point);
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 		
				 	 XXPrint("Первая прорисовка ВВЕРХ - {0} - nkz2={1} nkz4={2} - zmax={3}",Bars[ci].Time,nkz2,nkz4,zmax);
					 NKZText.Point=new ChartPoint(Bars[ci].Time, nkz4);
        			 NKZText.Caption=string.Format("max={0} nkz4={1} nu={2}",zmax,nkz4,nu);
                   }
			
			if(td) { zmin =  dt1*Instrument.Point;
				     nkz4 = zmin+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				     nkz2 = zmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					 nkz4v= zmin+((NKZ-5)*Instrument.Point);  
				     nkz2v= zmin+(((NKZ*2))*Instrument.Point);
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale);  
				 	 XXPrint("Первая прорисовка ВНИЗ - {0} - nkz2={1} nkz4={2} - zmin={3}",Bars[ci].Time,nkz2,nkz4,zmin);
					 NKZText.Point=new ChartPoint(Bars[ci].Time, nkz4v);
					 NKZText.FontSize=10;
        			 NKZText.Caption=string.Format("min={0} nkz4={1} nd={2}",zmin,nkz4,nd);
  				   }
			lnkz.Price = nkz4; 
			toolRectangle.Point1=new ChartPoint(Bars[ci].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[ci].Time.AddHours(24), nkz4v);

		    toolRectangle1.Point1=new ChartPoint(Bars[ci].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[ci].Time.AddHours(24), nkz2v);  
		  } 
//====================================================================================================================      		
	  if(k>0) {
		  if (tu && Bars[ci].High>zmax) 
		  		{ 
			  		zmax=Bars[ci].High;
			  		toolArrowDown.Point = new ChartPoint(Bars[ci].Time, zmax+(50*Instrument.Point));
			  		toolArrowDown.Color=Color.Aqua;
		  			nkz4 = zmax-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				    nkz2 = zmax-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			        nkz4v= zmax-((NKZ-5)*Instrument.Point); 
				    nkz2v= zmax-((NKZ*2)*Instrument.Point);
			        nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			        nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			        nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			        nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
 					XXPrint("ПЕРЕРИСОВКА ВВЕРХ - {0} - nkz2={1} nkz4={2}",Bars[ci].Time,nkz2,nkz4);
					NKZText.Point=new ChartPoint(Bars[ci].Time, nkz4);
					NKZText.FontSize=10;
        			NKZText.Caption=string.Format("max={0} nkz4={1} nkz2={2}",zmax,nkz4,nkz2);
			  			lnkz.Price = nkz4; 
					toolRectangle.Point1=new ChartPoint(Bars[ci].Time, nkz4);
          			toolRectangle.Point2=new ChartPoint(Bars[ci].Time.AddHours(48), nkz4v);
			 		nd=false; nu=false; 				toolTriangle.BorderColor=Color.Blue;		  
		    		toolRectangle1.Point1=new ChartPoint(Bars[ci].Time, nkz2);
          			toolRectangle1.Point2=new ChartPoint(Bars[ci].Time.AddHours(48), nkz2v);   
		  		}
		  
		  if (td && Bars[ci].Low<zmin ) 
		  		{ 
			  		zmin=Bars[ci].Low;
					toolArrowUp.Point = new ChartPoint(Bars[ci].Time, zmin-(10*Instrument.Point));
			  		toolArrowUp.Color=Color.Aqua;
		  			nkz4 = zmin+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				    nkz2 = zmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					nkz4v= zmin+((NKZ-5)*Instrument.Point);  
				    nkz2v= zmin+(((NKZ*2))*Instrument.Point);
			        nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			        nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			        nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			        nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
 					XXPrint("ПЕРЕРИСОВКА ВНИЗ - {0} - nkz2={1} nkz4={2}",Bars[ci].Time,nkz2,nkz4);
					NKZText.Point=new ChartPoint(Bars[ci].Time, nkz4v);
					NKZText.FontSize=10;	
        			NKZText.Caption=string.Format("min={0} nkz4={1} nkz2={2}",zmin,nkz4,nkz2);
			  			lnkz.Price = nkz4; 
					toolRectangle.Point1=new ChartPoint(Bars[ci].Time, nkz4);
          			toolRectangle.Point2=new ChartPoint(Bars[ci].Time.AddHours(48), nkz4v);
			 		nd=false; nu=false; 			toolTriangle.BorderColor=Color.Blue;			  
		    		toolRectangle1.Point1=new ChartPoint(Bars[ci].Time, nkz2);
          			toolRectangle1.Point2=new ChartPoint(Bars[ci].Time.AddHours(48), nkz2v); 	  
		 		}
//== Касание зоны/1\2 ======================================================================================================
			if (tu && Bars[ci].Low<nkz4 && !nu)  { nu=true; XXPrint("ПЕРЕСЕЧЕНИЕ 1/4 ВНИЗ  - {0} - {1}",Bars[ci].Time,nkz4);toolTriangle.BorderColor=Color.Red;}
			if (td && Bars[ci].High>nkz4 && !nd) { nd=true; XXPrint("ПЕРЕСЕЧЕНИЕ 1/4 ВВЕРХ - {0} - {1}",Bars[ci].Time,nkz4);toolTriangle.BorderColor=Color.Red;}
// Если пересечение зоны было (nu) и торгуем вверх (tu) ===========================================================		
	if(nu && tu) 
		{
// Определение патерна   ПИК ПОСЛЕДНЕГО ФРАКТАЛА ПАТЕРНА ВНИЗУ 
		   if (_frInd.TopSeries[Bars.Range.To-frac]>0)    
		   {
			   
           		if( frU3>frU2 && frU1>frU2 ) 
				{
					var toolPolyLine = Tools.Create<PolyLine>(); toolPolyLine.Color=Color.Aqua;toolPolyLine.Width=2;
        			toolPolyLine.AddPoint(new ChartPoint(tmU3, frU3));
        			toolPolyLine.AddPoint(new ChartPoint(tmU2, frU2));
        			toolPolyLine.AddPoint(new ChartPoint(tmU1, frU1));  

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
					XXPrint("PATERN BUY -- {0} -- TP={1} SL={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6} Ot={7}",Bars[ci].Time,PRUi,UBUi,PCU,tmU1,tmU2,tmU3,Ot);

						var toolText = Tools.Create<Text>();
						toolText.Color=Color.Blue;	 
						toolText.FontSize=10;	 
        				toolText.Point=new ChartPoint(tmU1,frU1+(50*Instrument.Point));
        				toolText.Caption=string.Format("TP={0} SL={1} C={2} C2={3}",PRU,UBU,PCU,PCU2);
					
					if(UBU>20 && UBU<SL1 && Bars[ci].Close>nkz2) 
					{
						if (posGuidBuy!=Guid.Empty && posGuidBuy2==Guid.Empty && TP2!=0 && PCU2>2) { 
							XXPrint("BUY1 -- {0} -- TP={1} SL={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6} Ot={7}",Bars[ci].Time,PRU,UBU,PCU,tmU1,tmU2,tmU3,Ot);
							var res4 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.01,Instrument.Bid, -1,Stops.InPips(UBUi,PRUi2), null, null);
							if (res4.IsSuccessful)  posGuidBuy2=res4.Position.Id;
													}
						if (posGuidBuy==Guid.Empty && PCU>2) { 
							XXPrint("BUY2 -- {0} -- TP={1} SL={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6} Ot={7}",Bars[ci].Time,PRU,UBU,PCU,tmU1,tmU2,tmU3,Ot);
							var res1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.01,Instrument.Bid, -1,Stops.InPips(UBUi,PRUi), null, null);
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
				if( frD2>frD3 && frD2>frD1 ) 
				{   
					var toolPolyLine = Tools.Create<PolyLine>();toolPolyLine.Color=Color.Aqua;toolPolyLine.Width=2;
        			toolPolyLine.AddPoint(new ChartPoint(tmD3, frD3));
        			toolPolyLine.AddPoint(new ChartPoint(tmD2, frD2));
        			toolPolyLine.AddPoint(new ChartPoint(tmD1, frD1));  
					
					TPD=zmin; TPD=Math.Round(TPD, Instrument.PriceScale);
					SLD=frU2+Instrument.Spread+dlt; SLD=Math.Round(SLD, Instrument.PriceScale);
					PRD=Math.Round((Bars[ci].Close-TPD)*Math.Pow(10,Instrument.PriceScale),0);
					PRD2=Math.Round((Bars[ci].Close-TP2)*Math.Pow(10,Instrument.PriceScale),0);

					Ot=Math.Round((frD2-frD3)/(frD2-frD1), 2);
					UBD=Math.Round((SLD-Bars[ci].Close)*Math.Pow(10,Instrument.PriceScale),0);
					PRDi2=(int)PRD2;
					PRDi=(int)PRD;
					UBDi=(int)UBD;
					PCD=Math.Round(PRD/UBD, 2);
					PCD2=Math.Round(PRD2/UBD, 2);
					XXPrint("PATERN SELL -- {0} -- TP(п)={1} SL(p)={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6}  Ot={7}",Bars[ci].Time,PRDi,UBDi,PCD,tmD1,tmD2,tmD3,Ot);				
					if(UBD>20 && UBD<SL1 && PCD>2 && Bars[ci].Close<nkz2) 
					{
					
						var toolText1 = Tools.Create<Text>();
						toolText1.Color=Color.Blue;	 
						toolText1.FontSize=10;	 
        				toolText1.Point=new ChartPoint(tmD1,frD1);
        				toolText1.Caption=string.Format("TP={0} SL={1} C={2} C2={3}",PRD,UBD,PCD,PCD2);
						
						if(UBD>20 && UBD<SL1 && Bars[ci].Close>nkz2) 
					 	{

						if (posGuidSell!=Guid.Empty && posGuidSell2==Guid.Empty && TP2!=0 && PCD2>2 ) 
							{
							XXPrint("SELL1 -- {0} -- TP={1} SL={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6} Ot={7}",Bars[ci].Time,PRDi,UBDi,PCD,tmD1,tmD2,tmD3,Ot);
							var res3=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.01,Instrument.Ask, -1,	Stops.InPips(UBDi,PRDi2), null, null);
							if (res3.IsSuccessful)  posGuidSell2=res3.Position.Id;
							}
						if (posGuidSell==Guid.Empty && PCD>2 ) 
							{
							XXPrint("SELL2 -- {0} -- TP={1} SL={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6} Ot={7}",Bars[ci].Time,PRDi,UBDi,PCD,tmD1,tmD2,tmD3,Ot);
							var res2=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.01,Instrument.Ask, -1,	Stops.InPips(UBDi,PRDi), null, null);
							if (res2.IsSuccessful)  posGuidSell=res2.Position.Id;
							}
							frU=false; 
					 	}
				 	  }
			} // if( frD2>frD3 && frD2>frD1 ) 
         } // if(_frInd.BottomSeries[Bars.Range.To-frac]>0)	 
	  } //if(nd && td) 
//===============================================================================================================================   
	  } // k==0
	  k++;
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

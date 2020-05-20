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
		[Parameter("NKZ :")]
        public int dt1 { get; set; }
		
		[Parameter("Buy:", DefaultValue = false)]
        public bool tu { get; set; }	
		[Parameter("Sell:", DefaultValue = false)]
        public bool td { get; set; }	
		[Parameter("StopLoss:", DefaultValue = 250)]
        public int SL1 { get; set; }	
		[Parameter("Отступ Stop :", DefaultValue = 50)]
		public int dl { get;set; }	
		[Parameter("Fractal", DefaultValue = 5)]
		public int frac { get;set; }	
		[Parameter("Zona 1-4", DefaultValue = true)]
		public bool n4 { get;set; }	

		private int NKZ,i;

		public int iFT=0,k=0,ks=0;
		private double nkz2,nkz4,nkz2v,nkz4v,kf,zmax,zmin;

		// private PolyLine toolPolyLine;
		private Rectangle toolRectangle;
		private Rectangle toolRectangle1;	
		private Rectangle toolRectangle2;

		public DateTime DTime; // Время
		private int ci = 0,ki;
		private bool nu,nd;		
		private double dlt,frUp,frDown;
		public Fractals _frInd;		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		public double frU1,frU2,frU3;   // Значение текущего верхнего Fractal
		public double frD1,frD2,frD3;    // Значение Low - свечи с верхним фрактклом

		public double TPD,TPU,SLU,SLD;
		public double PRD,PRU,PCU,PCD,UBU,UBD,Ot;		
		public int PRDi,PRUi,UBDi,UBUi;
		public DateTime tmU1,tmU2,tmU3,tmD1,tmD2,tmD3;
		public bool frU,frD;

		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		private string trueLogPath = "";	
		
        protected override void Init()
        {	
			k=0; // Для першого входження -
			kf=0.090909;

			dlt=dl*Instrument.Point; // Отступ от стопа
			frU1=0.0;frU2=0.0;frU3=0.0;frD1=0.0;frD2=0.0;frD3=0.0;
			
			InitLogFile();  // Запись логов
			// Прямоугольники 
			toolRectangle = Tools.Create<Rectangle>(); toolRectangle.BorderColor=Color.Aqua; toolRectangle.Color=Color.DarkSeaGreen;toolRectangle.BorderColor=Color.Blue;
			toolRectangle1 = Tools.Create<Rectangle>(); toolRectangle1.BorderColor=Color.Aqua; toolRectangle1.Color=Color.DarkSeaGreen;

			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			_frInd.Range=frac-2;
			
			ki=0;
		
			// 10/09/2018
			if (Instrument.Name == "EURUSD")  NKZ=506;
			if (Instrument.Name == "GBPUSD")  NKZ=660;
			if (Instrument.Name == "AUDUSD")  NKZ=343;
			if (Instrument.Name == "NZDUSD")  NKZ=330;
			if (Instrument.Name == "USDJPY")  NKZ=547;
			if (Instrument.Name == "USDCAD")  NKZ=530;
			if (Instrument.Name == "USDCHF")  NKZ=536;
			if (Instrument.Name == "AUDJPY")  NKZ=550;
			if (Instrument.Name == "AUDNZD")  NKZ=412;
			if (Instrument.Name == "CHFJPY")  NKZ=1430;
			if (Instrument.Name == "EURAUD")  NKZ=682; 
			if (Instrument.Name == "AUDCAD")  NKZ=357; 
			if (Instrument.Name == "EURCAD")  NKZ=726; 
			if (Instrument.Name == "EURCHF")  NKZ=682; 
			if (Instrument.Name == "EURGBP")  NKZ=484; 
			if (Instrument.Name == "EURJPY")  NKZ=781; 
			if (Instrument.Name == "GBPCHF")  NKZ=924; 
			if (Instrument.Name == "GBPJPY")  NKZ=1045;
						
        }        
//===========================================================================
        protected override void NewBar()
        {   
			DTime = Bars[ci].Time;
			ci = Bars.Range.To - 1;
			
//====  ИСТОРИЯ Fractal ====================================================================================

				i=0; 
			while ( !(frU1>0 && frU2>0 && frU3>0 && frD1>0 && frD2>0 && frD3>0) && i<200 ) { ki++;
				Print("{0} - {1}  {2}",i,_frInd.TopSeries[Bars.Range.To-frac-i],_frInd.BottomSeries[Bars.Range.To-frac-i]);
		   // XXPrint("{0} - U1={1} U2={2} U3={3} D1={4} D2={5} D3={6}",i ,frU1>0 ,frU2>0 , frU3>0, frD1>0, frD2>0, frD3>0);
			  if (_frInd.TopSeries[Bars.Range.To-frac-i]>0)	{	
				  if(frU) { frU=false;		
				  frU1=frU2; frU2=frU3; frU3=_frInd.TopSeries[Bars.Range.To-frac-i];
				  tmU1=tmU2; tmU2=tmU3; tmU3=Bars[Bars.Range.To-frac-i].Time;  
				  } else {
					  if(_frInd.TopSeries[Bars.Range.To-frac-i]>frU1) {
						  frU3=_frInd.TopSeries[Bars.Range.To-frac-i];
					  	  tmU3=Bars[Bars.Range.To-frac-i].Time; }
				  }
XXPrint("U ИТОРИЯ- {0} - frU1={1} frU2={2} frU3={3} frD1={4} frD2={5} frD3={6} i={7}", Bars[Bars.Range.To-frac-i].Time,frU1,frU2,frU3,frD1,frD2,frD3,i);
				  }
			  if (_frInd.BottomSeries[Bars.Range.To-frac-i]>0)   { 
				  if(!frU) { frU=true;
				  frD1=frD2; frD2=frD3; frD3=_frInd.BottomSeries[Bars.Range.To-frac-i];
				     tmD1=tmD2; tmD2=tmD3; tmD3=Bars[Bars.Range.To-frac-i].Time;
				  } else {
					 if (_frInd.BottomSeries[Bars.Range.To-frac-i]<frD1) {
						 frD3=_frInd.BottomSeries[Bars.Range.To-frac-i];
					     tmD3=Bars[Bars.Range.To-frac-i].Time;  } 
				  }
XXPrint("D ИСТОРИЯ - {0} - frU1={1} frU2={2} frU3={3} frD1={4} frD2={5} frD3={6} i={7}", Bars[Bars.Range.To-frac-i].Time,frU1,frU2,frU3,frD1,frD2,frD3,i);				  
			  	} 			
            i++;
        }		

//====  Fractal ====================================================================================
			  if (_frInd.TopSeries[Bars.Range.To-frac]>0) 		{	
				  if(frU) { frU=false;		
				  frU3=frU2; frU2=frU1; frU1=_frInd.TopSeries[Bars.Range.To-frac];
				  tmU3=tmU2; tmU2=tmU1; tmU1=Bars[Bars.Range.To-frac].Time;  
				  } else {
					  if(_frInd.TopSeries[Bars.Range.To-frac]>frU1) {
						  frU1=_frInd.TopSeries[Bars.Range.To-frac];
					  	  tmU1=Bars[Bars.Range.To-frac].Time; }
				  }
XXPrint("{0} - frU1={1} frU2={2} frU3={3} frD1={4} frD2={5} frD3={6} nkz4={7} nkz2={8} tu={9} td={10} dl={11} frac={12}", Bars[Bars.Range.To-frac].Time,frU1,frU2,frU3,frD1,frD2,frD3,nkz4,nkz2,tu,td,dl,frac);
				  }
			  if (_frInd.BottomSeries[Bars.Range.To-frac]>0)   { 
				  if(!frU) { frU=true;
				  frD3=frD2; frD2=frD1; frD1=_frInd.BottomSeries[Bars.Range.To-frac];
				     tmD3=tmD2; tmD2=tmD1; tmD1=Bars[Bars.Range.To-frac].Time;
				  } else 
				  {
					 if (_frInd.BottomSeries[Bars.Range.To-frac]<frD1) {
						 frD1=_frInd.BottomSeries[Bars.Range.To-frac];
					     tmD1=Bars[Bars.Range.To-frac].Time;  } 
				  }
XXPrint("{0} - frU1={1} frU2={2} frU3={3} frD1={4} frD2={5} frD3={6} nkz4={7} nkz2={8} tu={9} td={10} dl={11} frac={12}", Bars[Bars.Range.To-frac].Time,frU1,frU2,frU3,frD1,frD2,frD3,nkz4,nkz2,tu,td,dl,frac);
				  } 			
// Первое начиртание зон	=====================================================================================		
          if(k==0) 
		  { 			 // Индекс свечипоследнего
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
  				   }
	  
			toolRectangle.Point1=new ChartPoint(Bars[ci].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[ci].Time.AddHours(24), nkz4v);

		    toolRectangle1.Point1=new ChartPoint(Bars[ci].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[ci].Time.AddHours(24), nkz2v);  
		  } 
//====================================================================================================================      		
	  if(k>0) {
		  if (tu && Bars[ci].High>zmax) { 
			  		 zmax=Bars[ci].High;
		  			 nkz4 = zmax-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				     nkz2 = zmax-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			         nkz4v= zmax-((NKZ-5)*Instrument.Point); 
				     nkz2v= zmax-((NKZ*2)*Instrument.Point);
			         nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			         nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			         nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			         nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
 			XXPrint("ПЕРЕРИСОВКА ВВЕРХ - {0} - nkz2={1} nkz4={2} - 1U",Bars[ci].Time,nkz2,nkz4);
			toolRectangle.Point1=new ChartPoint(Bars[ci].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[ci].Time.AddHours(48), nkz4v);
			 nd=false; nu=false; 			  
		    toolRectangle1.Point1=new ChartPoint(Bars[ci].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[ci].Time.AddHours(48), nkz2v);   
		  }
		  
		  if (td && Bars[ci].Low<zmin ) { 
			  		zmin=Bars[ci].Low;
		  			nkz4 = zmin+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				    nkz2 = zmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					nkz4v= zmin+((NKZ-5)*Instrument.Point);  
				    nkz2v= zmin+(((NKZ*2))*Instrument.Point);
			        nkz4=Math.Round(nkz4,Instrument.PriceScale);  
			        nkz2=Math.Round(nkz2,Instrument.PriceScale);  
			        nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
			        nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
 			XXPrint("ПЕРЕРИСОВКА ВНИЗ - {0} - nkz2={1} nkz4={2} - 1U",Bars[ci].Time,nkz2,nkz4);
			toolRectangle.Point1=new ChartPoint(Bars[ci].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[ci].Time.AddHours(48), nkz4v);
			 nd=false; nu=false; 			  
		    toolRectangle1.Point1=new ChartPoint(Bars[ci].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[ci].Time.AddHours(48), nkz2v); 	  
		 				 }
 			    
				   		 		
//== Касание зоны/1\4 ======================================================================================================
		  if(n4)  {
			if (tu && Bars[ci].Low<nkz4 && !nu) { nu=true; XXPrint("ПЕРЕСЕЧЕНИЕ 1/4 ВНИЗ - {0} - {1}",Bars[ci].Time,nkz4);}
			if (td && Bars[ci].High>nkz4 && !nd) { nd=true; XXPrint("ПЕРЕСЕЧЕНИЕ 1/4 ВВЕРХ - {0} - {1}",Bars[ci].Time,nkz4);}
                  } 
			if (tu && Bars[ci].Low<nkz2 && !nu) { nu=true; XXPrint("ПЕРЕСЕЧЕНИЕ 1/2 ВНИЗ - {0} - {1}",Bars[ci].Time,nkz2);}
			if (td && Bars[ci].High>nkz2 && !nd) { nd=true; XXPrint("ПЕРЕСЕЧЕНИЕ 1/2 ВВЕРХ - {0} - {1}",Bars[ci].Time,nkz2);}
// Если пересечение зоны было (nu) и торгуем вверх (tu) ===========================================================		
			if(nu && tu) 
			{
// Определение патерна   ПИК ПОСЛЕДНЕГО ФРАКТАЛА ПАТЕРНА ВНИЗУ 
		   if (_frInd.TopSeries[Bars.Range.To-frac]>0)    
		   { 
           		if( frU3>frU2 && frU1>frU2 ) {
					var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[ci].Time; vl1.Color=Color.Aqua;vl1.Width=2;
					var toolPolyLine = Tools.Create<PolyLine>(); toolPolyLine.Color=Color.Aqua;toolPolyLine.Width=2;
        			toolPolyLine.AddPoint(new ChartPoint(tmU3, frU3));
        			toolPolyLine.AddPoint(new ChartPoint(tmU2, frU2));
        			toolPolyLine.AddPoint(new ChartPoint(tmU1, frU1));  

					TPU=zmax;
					SLU=frD2-Instrument.Spread-dlt; SLU=Math.Round(SLU, Instrument.PriceScale);
					PRU=Math.Round((TPU-Bars[ci].Close)*Math.Pow(10,Instrument.PriceScale),0);
					UBU=Math.Round((Bars[ci].Close-SLU)*Math.Pow(10,Instrument.PriceScale),0);
					PCU=Math.Round(PRU/UBU, 2);
					Ot=Math.Round((frU3-frU2)/(frU1-frU2), 2);
					PRUi=(int)PRU;
					UBUi=(int)UBU;
					Print("{8} PATERN BUY -- {0} -- TP={1} SL={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6} Ot={7}",Bars[ci].Time,PRUi,UBUi,PCU,tmU1,tmU2,tmU3,Ot,Instrument.Name);
					XXPrint("PATERN BUY -- {0} -- TP={1} SL={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6} Ot={7}",Bars[ci].Time,PRUi,UBUi,PCU,tmU1,tmU2,tmU3,Ot);
					if(UBU>20 && UBU<SL1 && PCU>2 && Bars[ci].Close>nkz2) {
				    XXPrint("BUY -- {0} -- TP={1} SL={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6} Ot={7}",Bars[ci].Time,PRU,UBU,PCU,tmU1,tmU2,tmU3,Ot);
                    Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,Stops.InPips(UBUi,PRUi), null, null);
					frD=false;}
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
				{   var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[ci].Time; vl1.Color=Color.Aqua;vl1.Width=2;
					var toolPolyLine = Tools.Create<PolyLine>();toolPolyLine.Color=Color.Aqua;toolPolyLine.Width=2;
        			toolPolyLine.AddPoint(new ChartPoint(tmD3, frD3));
        			toolPolyLine.AddPoint(new ChartPoint(tmD2, frD2));
        			toolPolyLine.AddPoint(new ChartPoint(tmD1, frD1));  
					
					TPD=zmin; TPD=Math.Round(TPD, Instrument.PriceScale);
					SLD=frU2+Instrument.Spread+dlt; SLD=Math.Round(SLD, Instrument.PriceScale);
					PRD=Math.Round((Bars[ci].Close-TPD)*Math.Pow(10,Instrument.PriceScale),0);

					Ot=Math.Round((frD2-frD3)/(frD2-frD1), 2);
					UBD=Math.Round((SLD-Bars[ci].Close)*Math.Pow(10,Instrument.PriceScale),0);
					
					PRDi=(int)PRD;
					UBDi=(int)UBD;
					PCD=Math.Round(PRD/UBD, 2);
					Print("{8}  PATERN SELL -- {0} -- TP(п)={1} SL(p)={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6}  Ot={7}",Bars[ci].Time,PRDi,UBDi,PCD,tmD1,tmD2,tmD3,Ot,Instrument.Name);
					XXPrint("PATERN SELL -- {0} -- TP(п)={1} SL(p)={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6}  Ot={7}",Bars[ci].Time,PRDi,UBDi,PCD,tmD1,tmD2,tmD3,Ot);				
					if(UBD>20 && UBD<SL1 && PCD>2 && Bars[ci].Close<nkz2) {
					XXPrint("SELL -- {0} -- TP={1} SL={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6} Ot={7}",Bars[ci].Time,PRDi,UBDi,PCD,tmD1,tmD2,tmD3,Ot);
					Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,	Stops.InPips(UBDi,PRDi), null, null);
					frU=false; }
				} 
			}
		}
			
			}
			k++;
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

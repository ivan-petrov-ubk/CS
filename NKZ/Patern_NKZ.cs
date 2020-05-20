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
    [TradeSystem("Patern_NKZ")]        //copy of "N10-Active"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Время :")]
        public DateTime dt1 { get; set; }
		
		[Parameter("Buy:", DefaultValue = false)]
        public bool tu { get; set; }	
		[Parameter("Sell:", DefaultValue = false)]
        public bool td { get; set; }	
		
		public int NKZ,i;
		public int iFT=0,k=0;
		public double nkz2,nkz4,nkz2v,nkz4v,kf=0.090909,zmax,zmin;

		// private PolyLine toolPolyLine;
		public Rectangle toolRectangle;
		public Rectangle toolRectangle1;
		public DateTime dt0; 
	
		public DateTime DTime; // Время
		public int ci = 0,frac;
		public bool nu,nd;		
		public double dlt,ot;
		public Fractals _frInd;		

		public ZigZag _wprInd;
		public double zz1=2,zz2=2,zz3=2;
		public int    zzt1,zzt2,zzt3,zzt4,zzt5,zzt6,zzt7,zzt8;
		public double zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,zzd7,zzd8;
		public int    zzi1,zzi2,zzi3,zzi4,zzi5,zzi6,zzi7,zzi8;
	
		
        protected override void Init()
        {	//dt1=Bars[Bars.Range.To-1].Time;
			k=0;
			dt1=dt1.AddHours(-3);
			iFT = TimeToIndex(dt1, Timeframe);
			
			Print("Init - {0} - {1} - {2} - k={3}",dt1,Bars[Bars.Range.To-1].Time,Bars[iFT].Time,k);	
		
			toolRectangle = Tools.Create<Rectangle>(); toolRectangle.BorderColor=Color.Aqua; toolRectangle.Color=Color.DarkSeaGreen;
			toolRectangle1 = Tools.Create<Rectangle>(); toolRectangle1.BorderColor=Color.Aqua; toolRectangle1.Color=Color.DarkSeaGreen;
		
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=5;				
			// 15/05/2018
			if (Instrument.Name == "EURUSD") { NKZ=462; }
			if (Instrument.Name == "GBPUSD") { NKZ=792;  }
			if (Instrument.Name == "AUDUSD") { NKZ=343; }
			if (Instrument.Name == "NZDUSD") { NKZ=357; }
			if (Instrument.Name == "USDJPY") { NKZ=527; }
			if (Instrument.Name == "USDCAD") { NKZ=491; }
			if (Instrument.Name == "USDCHF") { NKZ=608;}
			if (Instrument.Name == "AUDJPY") { NKZ=550; }
			if (Instrument.Name == "AUDNZD") { NKZ=412; }
			if (Instrument.Name == "CHFJPY") { NKZ=1430; }
			if (Instrument.Name == "EURAUD") { NKZ=682; }
			if (Instrument.Name == "AUDCAD") { NKZ=357;  }
			if (Instrument.Name == "EURCAD") { NKZ=762; }
			if (Instrument.Name == "EURCHF") { NKZ=539; }
			if (Instrument.Name == "EURGBP") { NKZ=484;  }
			if (Instrument.Name == "EURJPY") { NKZ=715;  }
			if (Instrument.Name == "GBPCHF") { NKZ=924;  }
			if (Instrument.Name == "GBPJPY") { NKZ=1045; }
        }        
//===========================================================================
        protected override void NewBar()
        {   
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
			Patern123();
			//Print("Bars - {0} - {1} - {2}",dt1,Bars[Bars.Range.To-1].Time,Bars[iFT].Time);		
//===========================================================================			
          if(k==1) 
		  { dt0 = Bars[iFT].Time;
	  		//Print("k=1 {0} - iFT={1} Bars[iFT]={2} tu={3} td={4}",Bars[Bars.Range.To-1].Time,iFT,Bars[iFT].Time,tu,td);
			if(tu) {
				     zmax = Bars[iFT].High;  
					 Rect(0,zmax);                 }
			if(td) { zmin = Bars[iFT].Low;
					 Rect(1,zmin);			       }
		  } 
//=============================================================================================      		
		  if(k>1) {
				dt0 = Bars[Bars.Range.To-5].Time;
	 		  if (tu && _frInd.TopSeries[Bars.Range.To-5]>0 && Bars[Bars.Range.To-5].High>zmax) { 
			  		zmax=Bars[Bars.Range.To-5].High;
			         Rect(0,zmax);
	 		 				}
		  
		  	  if (td && _frInd.BottomSeries[Bars.Range.To-5]>0 && Bars[Bars.Range.To-5].Low<zmin ) { 
			  		zmin=Bars[Bars.Range.To-5].Low;
			  		Rect(1,zmin);
		 					 }
	  			}		
//== Касание зоны/1\4 =========================================================================
			if (tu && Bars[ci].Low<nkz4) nu=true;
			if (td && Bars[ci].High>nkz4) nd=true;
		  		k++;
        }
//=============================================================================================		
		protected void Rect(int updw, double maxmin)
		{    
		      if(updw==0) { // Тренд ВВЕРХ 
				    nkz4 = maxmin-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				    nkz2 = maxmin-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			        nkz4v= maxmin-((NKZ-5)*Instrument.Point); 
				    nkz2v= maxmin-((NKZ*2)*Instrument.Point);
					nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
			  }
			   if(updw==1) {  // Тренд ВНИЗ
				    nkz4 = maxmin+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				    nkz2 = maxmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					nkz4v= maxmin+((NKZ-5)*Instrument.Point);  
				    nkz2v= maxmin+(((NKZ*2))*Instrument.Point);
					nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					nkz2v=Math.Round(nkz2v,Instrument.PriceScale);
		        }
			toolRectangle.Point1=new ChartPoint(dt0, nkz4);
          	toolRectangle.Point2=new ChartPoint(dt0.AddHours(24), nkz4v);
		    toolRectangle1.Point1=new ChartPoint(dt0, nkz2);
          	toolRectangle1.Point2=new ChartPoint(dt0.AddHours(24), nkz2v); 
			   
			   Print("КВАДРАТ - {0} nkz4={1} nkz4v={2} nkz2={3} nkz2v={4}",dt0, nkz4, nkz4v, nkz2, nkz2v);
		}
//===============================================================================================================================
        protected void Patern123()
        {    _wprInd.ReInit();
 			//Print("Patern123 - Function {0} k={1} ZigZag={2}",Bars[Bars.Range.To-1].Time,k,_wprInd.MainIndicatorSeries[Bars.Range.To-1]);
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzt3=zzt2;	 zzt2=zzt1;  zzt1= Bars.Range.To-1;
				//Print("Patern123 - Function {0} z1={1} z2={2} z3={3}",Bars[Bars.Range.To-1].Time,zz1,zz2,zz3);
//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ
					zzi8=zzi7; zzi7=zzi6; zzi6=zzi5; zzi5=zzi4; zzi4=zzi3; zzi3=zzi2; zzi2=zzi1; zzi1=zzt2; 
					zzd8=zzd7; zzd7=zzd6; zzd6=zzd5; zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2;
					//Print("ВВЕРХУ  {0}",Bars[Bars.Range.To].Time);
					ot = zzd2+((zzd3-zzd2)*0.5);
					if( zzd6>zzd8 && zzd4>zzd6 &&  zzd4>zzd2 && zzd3>zzd1) // ВВЕРХУ
						 {   Print("BLUE ВВЕРХУ {0} - 1-{1} 2-{2} 3-{3} 4-{4} 5-{5} 6-{6} 7-{7} 8-{8}",Bars[Bars.Range.To].Time,zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,zzd7,zzd8);
							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Blue;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi8].Time, zzd8));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi7].Time, zzd7));							 
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi6].Time, zzd6));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi5].Time, zzd5));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi4].Time, zzd4));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi3].Time, zzd3));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi2].Time, zzd2));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi1].Time, zzd1));							
						}
				}				
//==== ВНИЗУ ПИК ======================================================================================================================				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
					zzi8=zzi7; zzi7=zzi6; zzi6=zzi5; zzi5=zzi4; zzi4=zzi3; zzi3=zzi2; zzi2=zzi1; zzi1=zzt2; 
					zzd8=zzd7; zzd7=zzd6; zzd6=zzd5; zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2;
				    //Print("ВНИЗУ {0}",Bars[Bars.Range.To].Time);
					ot = zzd2-((zzd2-zzd3)*0.5);
					if( zzd6<zzd8 &&   zzd4<zzd6 &&  zzd4<zzd2 && zzd3<zzd1) // ВНИЗУ
					{
						 	Print("RED ВНИЗУ {0} - 1-{1} 2-{2} 3-{3} 4-{4} 5-{5} 6-{6} 7-{7} 8-{8}",Bars[Bars.Range.To].Time,zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,zzd7,zzd8);
							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Red;
							toolPolyLine.Width=4;		
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi8].Time, zzd8));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi7].Time, zzd7));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi6].Time, zzd6));
		 					toolPolyLine.AddPoint(new ChartPoint(Bars[zzi5].Time, zzd5));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi4].Time, zzd4));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi3].Time, zzd3));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi2].Time, zzd2));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi1].Time, zzd1)); 	
						}
				}
	
			}
        }
//===============================================================================================================================   

    }
}
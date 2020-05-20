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
    [TradeSystem("Patern_NKZ_123")]         //copy of "Patern_NKZ"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Время :")]
        public DateTime dt1 { get; set; }
		
		[Parameter("Buy:", DefaultValue = false)]
        public bool tu { get; set; }	
		[Parameter("Sell:", DefaultValue = false)]
        public bool td { get; set; }	
		
		private int NKZ,i;
		private bool first,t1,t2,per;
		public int iFT=0,k=0;
		private double nkz2,nkz4,nkz2v,nkz4v,kf=0.090909,zmax,zmin;

		// private PolyLine toolPolyLine;
		private Rectangle toolRectangle;
		private Rectangle toolRectangle1;	
		private Rectangle toolRectangle2;
		
		private DateTime dt0; 
	
		public DateTime DTime; // Время
		private int ci = 0,frac;
		private bool FsU,FsD,nu,nd;		
		private double dlt,frUp,frDown;
		public Fractals _frInd;		

		private ZigZag _wprInd;
		private double zz1=2,zz2=2,zz3=2,ot;
		private int    zzt1,zzt2,zzt3,zzt4,zzt5,zzt6,zzt7,zzt8;
		private double zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,zzd7,zzd8;
		private int    zzi1,zzi2,zzi3,zzi4,zzi5,zzi6,zzi7,zzi8;
		private VerticalLine vy,vb;		
		
        protected override void Init()
        {	//dt1=Bars[Bars.Range.To-1].Time;
			k=0;
			_wprInd.ReInit();
			dt1=dt1.AddHours(-3);
			iFT = TimeToIndex(dt1, Timeframe);
			
			//Print("Init - {0} - {1} - {2} - k={3}",dt1,Bars[Bars.Range.To-1].Time,Bars[iFT].Time,k);	
		
			//toolPolyLine = Tools.Create<PolyLine>(); toolPolyLine.Color=Color.Aqua; 
			toolRectangle = Tools.Create<Rectangle>(); toolRectangle.BorderColor=Color.Aqua; toolRectangle.Color=Color.DarkSeaGreen;
			toolRectangle1 = Tools.Create<Rectangle>(); toolRectangle1.BorderColor=Color.Aqua; toolRectangle1.Color=Color.DarkSeaGreen;
			//toolRectangle2 = Tools.Create<Rectangle>(); toolRectangle1.BorderColor=Color.Aqua; toolRectangle1.Color=Color.DarkSeaGreen;
			
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			first=true; 
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
			Print("Bars - {0} - {1} - {2}",dt1,Bars[Bars.Range.To-1].Time,Bars[iFT].Time);		
			
			//if(tu && Bars[Bars.Range.To-1].High>MaxU) MaxU= 
//===========================================================================			
          if(k==1) 
		  { 
			  nu=false; nd=false;
			  Print("First 2 {0}",Bars[iFT].Time);
			  
			if(tu) { zmax = Bars[iFT].High;  
				     nkz4 = zmax-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				     nkz2 = zmax-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			         nkz4v= zmax-((NKZ-5)*Instrument.Point); 
				     nkz2v= zmax-((NKZ*2)*Instrument.Point);
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
				Print("First 3 tu  {0} - {1}",Bars[iFT].Low,nkz2v);	
          }
			if(td) { zmin = Bars[iFT].Low;
				     nkz4 = zmin+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				     nkz2 = zmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					 nkz4v= zmin+((NKZ-5)*Instrument.Point);  
				     nkz2v= zmin+(((NKZ*2))*Instrument.Point);
					nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					nkz2v=Math.Round(nkz2v,Instrument.PriceScale);  
				Print("First 3 td  {0} - {1}",Bars[iFT].High,nkz2v);
			       }
	  
			toolRectangle.Point1=new ChartPoint(Bars[iFT].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz4v);

		    toolRectangle1.Point1=new ChartPoint(Bars[iFT].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz2v);  
//===========================================================================      		
		  } 
		  
		  if(k>1) {
			
			  //====  Fractal =====================================
			  if (_frInd.TopSeries[Bars.Range.To-5]>0) 		frUp=Bars[Bars.Range.To-5].High; 
			  if (_frInd.BottomSeries[Bars.Range.To-5]>0)   frDown=Bars[Bars.Range.To-5].Low; 	
			  
		  if (tu && _frInd.TopSeries[Bars.Range.To-5]>0 && Bars[Bars.Range.To-5].High>zmax) { 
			  		 zmax=Bars[Bars.Range.To-5].High;
		  			 nkz4 = zmax-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				     nkz2 = zmax-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			         nkz4v= zmax-((NKZ-5)*Instrument.Point); 
				     nkz2v= zmax-((NKZ*2)*Instrument.Point);
					nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
			
			toolRectangle.Point1=new ChartPoint(Bars[Bars.Range.To-5].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz4v);
			 nd=false; nu=false; 			  
		    toolRectangle1.Point1=new ChartPoint(Bars[Bars.Range.To-5].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz2v);   
		  }
		  
		  if (td && _frInd.BottomSeries[Bars.Range.To-5]>0 && Bars[Bars.Range.To-5].Low<zmin ) { 
			  		zmin=Bars[Bars.Range.To-5].Low;
		  			nkz4 = zmin+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				    nkz2 = zmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					nkz4v= zmin+((NKZ-5)*Instrument.Point);  
				    nkz2v= zmin+(((NKZ*2))*Instrument.Point);
					nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
			  
			toolRectangle.Point1=new ChartPoint(Bars[Bars.Range.To-5].Time, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz4v);
			 nd=false; nu=false; 			  
		    toolRectangle1.Point1=new ChartPoint(Bars[Bars.Range.To-5].Time, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(24), nkz2v); 	  
		  }
  }		
		  k++;
			//== Касание зоны/1\4 ===============================
			if (tu && Bars[ci].Low<nkz4) nu=true;
			if (td && Bars[ci].High>nkz4) nd=true;
			Patern123();
		  
        }
//===============================================================================================================================
        protected void Patern123()
        {
 			
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzt3=zzt2;	 zzt2=zzt1;  zzt1= Bars.Range.To-1;
//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ
					zzi8=zzi7; zzi7=zzi6; zzi6=zzi5; zzi5=zzi4; zzi4=zzi3; zzi3=zzi2; zzi2=zzi1; zzi1=zzt2; 
					zzd8=zzd7; zzd7=zzd6; zzd6=zzd5; zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2;
				Print("ВВЕРХУ  {0}",Bars[Bars.Range.To].Time);
					ot = zzd2+((zzd3-zzd2)*0.5);
					if( zzd6>zzd8 && zzd4>zzd6 &&  zzd4>zzd2 && zzd3>zzd1 && ot<zzd1) // ВВЕРХУ
						 {   Print("BLUE ВВЕРХУ {0} - 1-{1} 2-{2} 3-{3} 4-{4} 5-{5} 6-{6} 7-{7}",Bars[Bars.Range.To].Time,zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,zzd7);
							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Blue;
							toolPolyLine.Width=4;	
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
				    Print("ВНИЗУ {0}",Bars[Bars.Range.To].Time);
					ot = zzd2-((zzd2-zzd3)*0.5);
					if( zzd6<zzd8 &&   zzd4<zzd6 &&  zzd4<zzd2 && zzd3<zzd1 && ot>zzd1) // ВНИЗУ
					{
						 Print("RED ВНИЗУ {0} - 1-{1} 2-{2} 3-{3} 4-{4} 5-{5} 6-{6} 7-{7}",Bars[Bars.Range.To].Time,zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,zzd7);
							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Red;
							toolPolyLine.Width=4;		
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
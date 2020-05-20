using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;


namespace IPro.TradeSystems
{
    [TradeSystem("Color1")]
    public class Color1 : TradeSystem
    {
		private bool firstBar=true;
		private int kl=0; 
			
        protected override void NewBar()
        {
//Горизонтальная линия	
			if(kl%2==0)
			{			 var toolText = Tools.Create<Text>(); 
        toolText.Point=new ChartPoint(Bars[Bars.Range.To-2].Time, 1.132);
						toolText.FontSize=6;
        toolText.Caption=string.Format("{0}",kl/2);	}
			
			if (kl==0) 	{var vline = Tools.Create<VerticalLine>();vline.Time=Bars[Bars.Range.To-1].Time;vline.Width=4;
            					vline.Color = Color.Aqua;  
		
			
			}
			if (kl==2) 	{var vline1 = Tools.Create<VerticalLine>();vline1.Time=Bars[Bars.Range.To-1].Time;vline1.Width=4;
            					vline1.Color = Color.Blue; }
			if (kl==4) 	{var vline2 = Tools.Create<VerticalLine>();vline2.Time=Bars[Bars.Range.To-1].Time;vline2.Width=4;
            					vline2.Color = Color.BlueViolet; }
			if (kl==6) 	{var vline3 = Tools.Create<VerticalLine>();vline3.Time=Bars[Bars.Range.To-1].Time;vline3.Width=4;
            					vline3.Color = Color.Crimson; }
			if (kl==8) 	{var vline4 = Tools.Create<VerticalLine>();vline4.Time=Bars[Bars.Range.To-1].Time;vline4.Width=4;
            					vline4.Color = Color.DarkBlue; }
			if (kl==10) 	{var vline5 = Tools.Create<VerticalLine>();vline5.Time=Bars[Bars.Range.To-1].Time;vline5.Width=4;
            					vline5.Color = Color.DarkCyan; }
			if (kl==12) 	{var vline6 = Tools.Create<VerticalLine>();vline6.Time=Bars[Bars.Range.To-1].Time;vline6.Width=4;
            					vline6.Color = Color.DarkGoldenrod; }
			if (kl==14) 	{var vline7 = Tools.Create<VerticalLine>();vline7.Time=Bars[Bars.Range.To-1].Time;vline7.Width=4;
            					vline7.Color = Color.DarkGray; }
			if (kl==16) 	{var vline8 = Tools.Create<VerticalLine>();vline8.Time=Bars[Bars.Range.To-1].Time;vline8.Width=4;
            					vline8.Color = Color.DarkGreen; }
			if (kl==18) 	{var vline9 = Tools.Create<VerticalLine>();vline9.Time=Bars[Bars.Range.To-1].Time;vline9.Width=4;
            					vline9.Color = Color.DarkKhaki; }	
			if (kl==20) 	{var vline10 = Tools.Create<VerticalLine>();vline10.Time=Bars[Bars.Range.To-1].Time;vline10.Width=4;
            					vline10.Color = Color.DarkKhaki; }
			
			if (kl==22) 	{var vline11 = Tools.Create<VerticalLine>();vline11.Time=Bars[Bars.Range.To-1].Time;vline11.Width=4;
            					vline11.Color = Color.DarkMagenta; }
			if (kl==24) 	{var vline12 = Tools.Create<VerticalLine>();vline12.Time=Bars[Bars.Range.To-1].Time;vline12.Width=4;
            					vline12.Color = Color.DarkOrange; }
			if (kl==26) 	{var vline13 = Tools.Create<VerticalLine>();vline13.Time=Bars[Bars.Range.To-1].Time;vline13.Width=4;
            					vline13.Color = Color.DarkOrchid; }
			if (kl==28) 	{var vline14 = Tools.Create<VerticalLine>();vline14.Time=Bars[Bars.Range.To-1].Time;vline14.Width=4;
            					vline14.Color = Color.DarkRed; }
			if (kl==30) 	{var vline15 = Tools.Create<VerticalLine>();vline15.Time=Bars[Bars.Range.To-1].Time;vline15.Width=4;
            					vline15.Color = Color.DarkSalmon; }
			if (kl==32) 	{var vline16 = Tools.Create<VerticalLine>();vline16.Time=Bars[Bars.Range.To-1].Time;vline16.Width=4;
            					vline16.Color = Color.DarkSeaGreen; }
			if (kl==34) 	{var vline17 = Tools.Create<VerticalLine>();vline17.Time=Bars[Bars.Range.To-1].Time;vline17.Width=4;
            					vline17.Color = Color.DarkSlateBlue; }
			if (kl==36) 	{var vline18 = Tools.Create<VerticalLine>();vline18.Time=Bars[Bars.Range.To-1].Time;vline18.Width=4;
            					vline18.Color = Color.DarkSlateGray; }
			if (kl==38) 	{var vline19 = Tools.Create<VerticalLine>();vline19.Time=Bars[Bars.Range.To-1].Time;vline19.Width=4;
            					vline19.Color = Color.DarkTurquoise; }		
			
			if (kl==40) 	{var vline20 = Tools.Create<VerticalLine>();vline20.Time=Bars[Bars.Range.To-1].Time;vline20.Width=4;
            					vline20.Color = Color.DeepPink;  }
			if (kl==42) 	{var vline21 = Tools.Create<VerticalLine>();vline21.Time=Bars[Bars.Range.To-1].Time;vline21.Width=4;
            					vline21.Color = Color.DeepSkyBlue; }
			if (kl==44) 	{var vline22 = Tools.Create<VerticalLine>();vline22.Time=Bars[Bars.Range.To-1].Time;vline22.Width=4;
            					vline22.Color = Color.DimGray; }
			if (kl==46) 	{var vline23 = Tools.Create<VerticalLine>();vline23.Time=Bars[Bars.Range.To-1].Time;vline23.Width=4;
            					vline23.Color = Color.Firebrick; }
			if (kl==48) 	{var vline24 = Tools.Create<VerticalLine>();vline24.Time=Bars[Bars.Range.To-1].Time;vline24.Width=4;
            					vline24.Color = Color.ForestGreen; }
			if (kl==50) 	{var vline25 = Tools.Create<VerticalLine>();vline25.Time=Bars[Bars.Range.To-1].Time;vline25.Width=4;
            					vline25.Color = Color.Gold; }
			if (kl==52) 	{var vline26 = Tools.Create<VerticalLine>();vline26.Time=Bars[Bars.Range.To-1].Time;vline26.Width=4;
            					vline26.Color = Color.Goldenrod; }
			if (kl==54) 	{var vline27 = Tools.Create<VerticalLine>();vline27.Time=Bars[Bars.Range.To-1].Time;vline27.Width=4;
            					vline27.Color = Color.Gray; }
			if (kl==56) 	{var vline28 = Tools.Create<VerticalLine>();vline28.Time=Bars[Bars.Range.To-1].Time;vline28.Width=4;
            					vline28.Color = Color.HotPink; }
			if (kl==58) 	{var vline29 = Tools.Create<VerticalLine>();vline29.Time=Bars[Bars.Range.To-1].Time;vline29.Width=4;
            					vline29.Color = Color.IndianRed; }	
			if (kl==60) 	{var vline30 = Tools.Create<VerticalLine>();vline30.Time=Bars[Bars.Range.To-1].Time;vline30.Width=4;
            					vline30.Color = Color.Indigo; }		
			
			if (kl==62) 	{var vline41 = Tools.Create<VerticalLine>();vline41.Time=Bars[Bars.Range.To-1].Time;vline41.Width=4;
            					vline41.Color = Color.Khaki; }
			if (kl==64) 	{var vline42 = Tools.Create<VerticalLine>();vline42.Time=Bars[Bars.Range.To-1].Time;vline42.Width=4;
            					vline42.Color = Color.LightSalmon; }
			if (kl==66) 	{var vline43 = Tools.Create<VerticalLine>();vline43.Time=Bars[Bars.Range.To-1].Time;vline43.Width=4;
            					vline43.Color = Color.LightSeaGreen; }
			if (kl==68) 	{var vline44 = Tools.Create<VerticalLine>();vline44.Time=Bars[Bars.Range.To-1].Time;vline44.Width=4;
            					vline44.Color = Color.Lime; }
			if (kl==70) 	{var vline45 = Tools.Create<VerticalLine>();vline45.Time=Bars[Bars.Range.To-1].Time;vline45.Width=4;
            					vline45.Color = Color.LimeGreen; }
			if (kl==72) 	{var vline46 = Tools.Create<VerticalLine>();vline46.Time=Bars[Bars.Range.To-1].Time;vline46.Width=4;
            					vline46.Color = Color.Magenta; }
			if (kl==74) 	{var vline47 = Tools.Create<VerticalLine>();vline47.Time=Bars[Bars.Range.To-1].Time;vline47.Width=4;
            					vline47.Color = Color.Maroon; }
			if (kl==76) 	{var vline48 = Tools.Create<VerticalLine>();vline48.Time=Bars[Bars.Range.To-1].Time;vline48.Width=4;
            					vline48.Color = Color.MediumBlue; }
			if (kl==78) 	{var vline49 = Tools.Create<VerticalLine>();vline49.Time=Bars[Bars.Range.To-1].Time;vline49.Width=4;
            					vline49.Color = Color.Navy; }	
			if (kl==80) 	{var vline50 = Tools.Create<VerticalLine>();vline50.Time=Bars[Bars.Range.To-1].Time;vline50.Width=4;
            					vline50.Color = Color.Olive; }	
			
			if (kl==82) 	{var vline51 = Tools.Create<VerticalLine>();vline51.Time=Bars[Bars.Range.To-1].Time;vline51.Width=4;
            					vline51.Color = Color.OliveDrab; }
			if (kl==84) 	{var vline52 = Tools.Create<VerticalLine>();vline52.Time=Bars[Bars.Range.To-1].Time;vline52.Width=4;
            					vline52.Color = Color.Purple; }
			if (kl==86) 	{var vline53 = Tools.Create<VerticalLine>();vline53.Time=Bars[Bars.Range.To-1].Time;vline53.Width=4;
            					vline53.Color = Color.Red; }
			if (kl==88) 	{var vline54 = Tools.Create<VerticalLine>();vline54.Time=Bars[Bars.Range.To-1].Time;vline54.Width=4;
            					vline54.Color = Color.SaddleBrown; }
			if (kl==90) 	{var vline55 = Tools.Create<VerticalLine>();vline55.Time=Bars[Bars.Range.To-1].Time;vline55.Width=4;
            					vline55.Color = Color.SeaGreen; }
			if (kl==92) 	{var vline56 = Tools.Create<VerticalLine>();vline56.Time=Bars[Bars.Range.To-1].Time;vline56.Width=4;
            					vline56.Color = Color.Silver; }
			if (kl==94) 	{var vline57 = Tools.Create<VerticalLine>();vline57.Time=Bars[Bars.Range.To-1].Time;vline57.Width=4;
            					vline57.Color = Color.SkyBlue; }
			if (kl==96) 	{var vline58 = Tools.Create<VerticalLine>();vline58.Time=Bars[Bars.Range.To-1].Time;vline58.Width=4;
            					vline58.Color = Color.SpringGreen; }
			if (kl==98) 	{var vline59 = Tools.Create<VerticalLine>();vline59.Time=Bars[Bars.Range.To-1].Time;vline59.Width=4;
            					vline59.Color = Color.Teal; }				
			
			if (kl==100) 	{var vline60 = Tools.Create<VerticalLine>();vline60.Time=Bars[Bars.Range.To-1].Time;vline60.Width=4;
            					vline60.Color = Color.Unknown; }
			if (kl==102) 	{var vline61 = Tools.Create<VerticalLine>();vline61.Time=Bars[Bars.Range.To-1].Time;vline61.Width=4;
            					vline61.Color = Color.Violet; }			
			if (kl==104) 	{var vline62 = Tools.Create<VerticalLine>();vline62.Time=Bars[Bars.Range.To-1].Time;vline62.Width=4;
            					vline62.Color = Color.White; }
			if (kl==106) 	{var vline63 = Tools.Create<VerticalLine>();vline63.Time=Bars[Bars.Range.To-1].Time;vline63.Width=4;
            					vline63.Color = Color.Yellow; }		
				kl++;
        }
        
    }
}
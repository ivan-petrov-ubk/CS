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
    [TradeSystem("Color1")]
    public class Color1 : TradeSystem
    {
		private bool first=true;
        protected override void NewBar()
        {
         if(first) { first=false;
//============================================================================================================================
		var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Aqua;
		var toolText = Tools.Create<Text>();
		toolText.Color=Color.Aqua;	 
		toolText.FontSize=12;	 
        toolText.Point=new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High);
        toolText.Caption=string.Format("Aqua");
//============================================================================================================================
		var vl11 = Tools.Create<VerticalLine>(); vl11.Time=Bars[Bars.Range.To-1].Time.AddMinutes(10); vl11.Color=Color.BlueViolet;
		var toolText1 = Tools.Create<Text>();
		toolText1.Color=Color.BlueViolet;	 
		toolText1.FontSize=12;	 
        toolText1.Point=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(10), Bars[Bars.Range.To-1].High);
        toolText1.Caption=string.Format("BlueViolet");
//============================================================================================================================
		var vl12= Tools.Create<VerticalLine>(); vl12.Time=Bars[Bars.Range.To-1].Time.AddMinutes(20); vl12.Color=Color.Crimson;
		var toolText2 = Tools.Create<Text>();
		toolText2.Color=Color.Crimson;	 
		toolText2.FontSize=12;	 
        toolText2.Point=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(20), Bars[Bars.Range.To-1].High);
        toolText2.Caption=string.Format("Crimson");
//============================================================================================================================
		var vl13 = Tools.Create<VerticalLine>(); vl13.Time=Bars[Bars.Range.To-1].Time.AddMinutes(30); vl13.Color=Color.DarkBlue;
		var toolText3 = Tools.Create<Text>();
		toolText3.Color=Color.DarkBlue;	 
		toolText3.FontSize=12;	 
        toolText3.Point=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(30), Bars[Bars.Range.To-1].High);
        toolText3.Caption=string.Format("DarkBlue");
//============================================================================================================================
		var vl14 = Tools.Create<VerticalLine>(); vl14.Time=Bars[Bars.Range.To-1].Time.AddMinutes(40); vl14.Color=Color.DarkCyan;
		var toolText4 = Tools.Create<Text>();
		toolText4.Color=Color.DarkCyan;	 
		toolText4.FontSize=12;	 
        toolText4.Point=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(40), Bars[Bars.Range.To-1].High);
        toolText4.Caption=string.Format("DarkCyan");
//============================================================================================================================
		var vl15 = Tools.Create<VerticalLine>(); vl15.Time=Bars[Bars.Range.To-1].Time.AddMinutes(50); vl15.Color=Color.DarkGoldenrod;
		var toolText5 = Tools.Create<Text>();
		toolText5.Color=Color.DarkGoldenrod;	 
		toolText5.FontSize=12;	 
        toolText5.Point=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(50), Bars[Bars.Range.To-1].High);
        toolText5.Caption=string.Format("DarkGoldenrod");
//============================================================================================================================
		var vl16 = Tools.Create<VerticalLine>(); vl16.Time=Bars[Bars.Range.To-1].Time.AddMinutes(60); vl16.Color=Color.DarkGray;
		var toolText6 = Tools.Create<Text>();
		toolText6.Color=Color.DarkGray;	 
		toolText6.FontSize=12;	 
        toolText6.Point=new ChartPoint(Bars[Bars.Range.To-1].Time.AddMinutes(60), Bars[Bars.Range.To-1].High);
        toolText6.Caption=string.Format("DarkGray");
//============================================================================================================================
			 
			 
		 }
        }

    }
}
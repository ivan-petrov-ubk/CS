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



namespace IPro.TradeSystems
{
    [TradeSystem("London")]   
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("StopLoss:", DefaultValue = 250)]
        public int SL1 { get; set; }		
		[Parameter("Buy:", DefaultValue = false)]
        public bool  Buy1 { get; set; }		
		[Parameter("Sell:", DefaultValue = false)]
        public bool  Sell1 { get; set; }		
		[Parameter("Fractal :", DefaultValue = 7)]
		public int frac { get;set; }	
		[Parameter("Отступ от Stop :", DefaultValue = 30)]
		public int dl { get;set; }	
		
		public Guid posGuidBuy=Guid.Empty;
		public Guid posGuidSell=Guid.Empty;
		public DateTime DTime; 
		public int k=0;
		public double dlt;
		public static int ci = 0;
		
	protected override void Init()
        {   k=0; dlt=dl*Instrument.Point;   } 		
		
//=============================================================================
	protected override void NewBar()
	{

	DTime = Bars[Bars.Range.To-1].Time;
	ci = Bars.Range.To - 1;
//=== КОРЕКЦИЯ ================================================================							 
	if (posGuidBuy!=Guid.Empty && 
		Trade.GetPosition(posGuidBuy).State==PositionState.Closed) 
				posGuidBuy=Guid.Empty;   
	if (posGuidSell!=Guid.Empty && 
		Trade.GetPosition(posGuidSell).State==PositionState.Closed) 
				posGuidSell=Guid.Empty;  
//=== Трелинг  =================================================================	
	if(k>=7) TrailActiveOrders();
//=====  Рисуем вертикальную линию в 7:00 ======================================
	if ( DTime.Hour==6 && DTime.Minute==45 ) 
	{  k=0; var vl1 = Tools.Create<VerticalLine>(); 
				vl1.Time=Bars[Bars.Range.To-1].Time; 
				vl1.Color=Color.Aqua; vl1.Width=2;	}
//=====  Для определении паузи перед стартом трелинга ==========================
	if ( DTime.Hour>=7 )  k++;
//====== Торги  ================================================================
	if ( DTime.Hour==7 ) 
		{   
			if (posGuidBuy==Guid.Empty && Buy1) 
			{ var result10 = Trade.OpenMarketPosition(Instrument.Id, 
					ExecutionRule.Buy,0.1,Instrument.Bid, -1,
					Stops.InPips(SL1,null), null, null);
				if (result10.IsSuccessful)  posGuidBuy=result10.Position.Id;
			} 
			   			
			if (posGuidSell==Guid.Empty && Sell1) 
			{  var result20 = Trade.OpenMarketPosition(Instrument.Id, 
					ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					Stops.InPips(SL1,null),null,  null); 
				if (result20.IsSuccessful)  posGuidSell=result20.Position.Id;
			}
		}
 
	}
//==============================================================================  
		protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { 
			  var tr = Trade.UpdateMarketPosition(posGuidBuy,getSL(1),null," - update TP,SL"); }
		  if(posGuidSell!=Guid.Empty) { 
			  var tr = Trade.UpdateMarketPosition(posGuidSell,getSL(0),null," - update TP,SL"); } 
		}	
		
		protected double getSL(int type)
		{
			switch(type)
			{
				case 0:
						{
							double MAX = double.MinValue;
							for(int i = 0; i < frac; i++)
							{
								if(Bars[ci - i].High > MAX)
									MAX = Bars[ci - i].High; 
							}	
							return Math.Round(MAX+dlt+Instrument.Spread, Instrument.PriceScale);
						}
				case 1:
						{
							double MIN = double.MaxValue;
							for(int i = 0; i < frac; i++)
							{
								if(Bars[ci - i].Low < MIN)
									MIN = Bars[ci - i].Low; 
							}	
							return Math.Round(MIN-dlt-Instrument.Spread, Instrument.PriceScale);
						}
				default: 
					break;
			}
			return 0.0;
		}
    }
}
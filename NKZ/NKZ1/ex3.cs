using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using IPro.Model.Client.MarketData;
using IPro.Model.Programming.Indicators.Standard;
using System.Collections.Generic;
using System.IO;

namespace IPro.TradeSystems
{
    [TradeSystem("ex2")]
    public class ex2 : TradeSystem
    {

		public static int magicNumber = 7;	
		private int k,NKZ,mgB,mgS;
  		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private int ActiveBuyCount=0,ActiveSellCount=0;
		
		protected override void Init()
        {   k=0;
			var result = Trade.OpenMarketPosition(Instrument.Id, 
						ExecutionRule.Buy,0.1,
						Instrument.Bid, -1,
						Stops.InPips(800,800), "Buy", magicNumber);
			 Print("1 Покупка !!! - {0} - {1}",result.Position.Comment,result.Position.MagicNumber);
			

			
if (Instrument.Name == "EURUSD") { NKZ=462; mgB=101; mgS=201; }
if (Instrument.Name == "GBPUSD") { NKZ=792; mgB=102; mgS=202; }
if (Instrument.Name == "AUDUSD") { NKZ=343; mgB=103; mgS=203; }
if (Instrument.Name == "NZDUSD") { NKZ=357; mgB=104; mgS=204; }
if (Instrument.Name == "USDJPY") { NKZ=501; mgB=105; mgS=205; }
if (Instrument.Name == "USDCAD") { NKZ=500; mgB=106; mgS=206; }
if (Instrument.Name == "USDCHF") { NKZ=557; mgB=107; mgS=207; }
if (Instrument.Name == "AUDJPY") { NKZ=550; mgB=108; mgS=208; }
if (Instrument.Name == "AUDNZD") { NKZ=412; mgB=109; mgS=209; }
if (Instrument.Name == "CHFJPY") { NKZ=1430; mgB=110; mgS=210; }
if (Instrument.Name == "EURAUD") { NKZ=682; mgB=111; mgS=211; }
if (Instrument.Name == "AUDCAD") { NKZ=357; mgB=112; mgS=212; }
if (Instrument.Name == "EURCAD") { NKZ=762; mgB=113; mgS=213; }
if (Instrument.Name == "EURCHF") { NKZ=539; mgB=114; mgS=214; }
if (Instrument.Name == "EURGBP") { NKZ=484; mgB=115; mgS=215; }
if (Instrument.Name == "EURJPY") { NKZ=715; mgB=116; mgS=216; }
if (Instrument.Name == "GBPCHF") { NKZ=924; mgB=117; mgS=217; }
if (Instrument.Name == "GBPJPY") { NKZ=1045; mgB=118; mgS=218; }
			
			
			
			var posActiveMine = Trade.GetActivePositions(7, true);
			if(posActiveMine!=null)
			{
				for(int p = posActiveMine.Length-1; p>=0; p--)
				{ 
					if((int)posActiveMine[p].Type == (int)ExecutionRule.Buy)
					{
						ActiveBuyCount++;
						posGuidBuy=posActiveMine[p].Id;						
					}
					if((int)posActiveMine[p].Type == (int)ExecutionRule.Sell)
					{
						ActiveSellCount++;
						posGuidSell=posActiveMine[p].Id;
					}
				}
			}	
			
			

			//if(posActiveMine.Length>0 && posActiveMine[0].Comment.EndsWith("Buy")) posGuidBuy=posActiveMine[0].Id;
			//if(posActiveMine.Length>0 && posActiveMine[0].Comment.EndsWith("Sell")) posGuidSell=posActiveMine[0].Id;
			//if(posActiveMine.Length>1 && posActiveMine[1].Comment.EndsWith("Buy")) posGuidBuy=posActiveMine[1].Id;
			//if(posActiveMine.Length>1 && posActiveMine[1].Comment.EndsWith("Sell")) posGuidSell=posActiveMine[1].Id;
            // Print("OK!!!! - Buy={0} Sell={1}",ActiveBuyCount,ActiveSellCount);
			// Print("Результат - {0} - {1}",posActiveMine[0].InstrumentId,posActiveMine[0].Id);
			//if(posActiveMine[0].Comment.Equals("Buy")) Print("OK!!!!!");
        }
		
		protected override void NewBar()
        {
			if(k==5) 
			{
				Trade.CloseMarketPosition(posGuidBuy);
			}
			Print("{0} - {1}",Bars[Bars.Range.To-1].Time,k);
			k++;
		}
    }
}
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
    [TradeSystem("Moving Average trading strategy")]
    [ParametersGroup(__ui_cGroupId1, NameOfOptionalParameter = "UseFirstMA", Order = -3, AtLeastOneIsChecked = true)]
    [ParametersGroup(__ui_cGroupId2, NameOfOptionalParameter = "UseSecondMA", Order = -2, AtLeastOneIsChecked = true)]
    [ParametersGroup(__ui_cGroup_Parameters1, "LR_indicators_parameters", ParentId = __ui_cGroupId1)]
    [ParametersGroup(__ui_cGroup_Parameters2, "LR_indicators_parameters", ParentId = __ui_cGroupId2)]
    [ParametersGroup(__ui_cGroup_Signals, "LR_Generated_signals", Order = -1)]
    [ParametersGroup(__ui_cGroup_Props, "LR_Order_properties", Order = 0)]
	public class StrategyMovingAverage : TradeSystem
    {
		#region Parameters
		
     	[Parameter("LR_Use_moving_average_1", DefaultValue = true, GroupId = __ui_cGroupId1)]
        public bool UseFirstMA { get; set; }
        [Parameter("LR_Period", DefaultValue = 34, MinValue = 1, GroupId = __ui_cGroup_Parameters1)]
        public int PeriodMA1 { get; set; }
        [Parameter("LR_Shift", DefaultValue = 0, MinValue = -1000, MaxValue = 1000, GroupId = __ui_cGroup_Parameters1)]
		public int ShiftMA1 { get; set; }
        [Parameter("LR_Method", DefaultValue = MaMethods.Sma, GroupId = __ui_cGroup_Parameters1)]
		public MaMethods MethodMA1 { get; set; }
        [Parameter("LR_Apply_to", DefaultValue = PriceMode.Close, GroupId = __ui_cGroup_Parameters1)]
        public PriceMode ApplyMA1To { get; set; }

        [Parameter("LR_Use_moving_average_2", DefaultValue = true, GroupId = __ui_cGroupId2)]
		public bool UseSecondMA { get; set; }
        [Parameter("LR_Period", DefaultValue = 144, MinValue = 1, GroupId = __ui_cGroup_Parameters2)]
        public int PeriodMA2 { get; set; }
        [Parameter("LR_Shift", DefaultValue = 0, MinValue = -1000, MaxValue = 1000, GroupId = __ui_cGroup_Parameters2)]
		public int ShiftMA2 { get; set; }
        [Parameter("LR_Method", DefaultValue = MaMethods.Sma, GroupId = __ui_cGroup_Parameters2)]
		public MaMethods MethodMA2 { get; set; }
        [Parameter("LR_Apply_to", DefaultValue = PriceMode.Close, GroupId = __ui_cGroup_Parameters2)]
        public PriceMode ApplyMA2To { get; set; }

        [Parameter("LR_Position_type", DefaultValue = AdvisorOrderType.Buy, Header = "LR_Current_quote_higher_than_moving_average", GroupId = __ui_cGroup_Signals)]
        public AdvisorOrderType QuoteAboveMA { get; set; }
        [Parameter("LR_Position_type", DefaultValue = AdvisorOrderType.Sell, Header = "LR_Current_quote_lower_than_moving_average", GroupId = __ui_cGroup_Signals)]
        public AdvisorOrderType QuoteBelowMA { get; set; }
        [Parameter("LR_Position_type", DefaultValue = AdvisorOrderType.Buy, Header = "LR_Intersection_slow_ma_with_fast_ma_down_to_up", GroupId = __ui_cGroup_Signals)]
        public AdvisorOrderType SlowMACrosFastMABottomTop { get; set; }
        [Parameter("LR_Position_type", DefaultValue = AdvisorOrderType.Sell, Header = "LR_Intersection_slow_ma_with_fast_ma_up_to_down", GroupId = __ui_cGroup_Signals)]
        public AdvisorOrderType SlowMACrosFastMATopBottom { get; set; }


        [Parameter("LR_Lot", DefaultValue = 1.00, IsLot = true, GroupId = __ui_cGroup_Props)]
        public decimal LotVolume { get; set; }
        [Parameter("LR_Take_Profit", DefaultValue = 0, GroupId = __ui_cGroup_Props, Postfix = "LR_Points", MaxValue=99999)]
        public int TakeProfit { get; set; }
        [Parameter("LR_Stop_Loss", DefaultValue = 0, GroupId = __ui_cGroup_Props, Postfix = "LR_Points", MaxValue=99999)]
        public int StopLoss { get; set; }
        [Parameter("LR_Position_closing_on_inverse_signal", DefaultValue = true, GroupId = __ui_cGroup_Props)]
        public bool IsDoCloseOnReverseSignal { get; set; }
		
		#endregion
		
		#region UI
		
		private const string __ui_cGroupId1 = "g1";
		private const string __ui_cGroupId2 = "g2";
		private const string __ui_cGroup_Parameters1 = "g-indicator-params-1";
		private const string __ui_cGroup_Parameters2 = "g-indicator-params-2";
		private const string __ui_cGroup_Signals="g-order-signals";
		private const string __ui_cGroup_Props = "g-order-props";
		
		#endregion
		
		private enum SignalType
		{
			QuoteAboveMA,
			QuoteBelowMA,
			SlowMACrosFastMATopBottom,
			SlowMACrosFastMABottomTop,
			None
		}
		
		private MovingAverage _ma1;
		private MovingAverage _ma2;
		private double _ma1Value;
		private double _ma2Value;
		private ISeries<Bar> _barSeries;
		private Period _period = new Period(PeriodType.Minute, 1);
		private bool _firstQuote=true;
		private bool _isMA1BelowMA2=false;
		private bool _isAnyErrors=false;		
		private double _ask;
		private Guid _positionGuid=Guid.Empty;
		private SignalType _positionOpendSignalType=SignalType.None;
		
		protected override void Init()
		{
            Print("LR_Trade_strategy_initalization");
			
			if ((LotVolume>Account.MaxLot)||(LotVolume<Account.MinLot))
			{
                Print("LR_Trade_strategy_initalization");
                Print("LR_Trade_strategy_initalization " + (double)Account.MinLot);
                Print("LR_Lot_maximum_value " + (double)Account.MaxLot);
				_isAnyErrors=true;
				return;
			}
			
			if (!UseFirstMA && !UseSecondMA)
			{
                Print("LR_Indicatorr_for_analysis_not_selected_change_TS_parameters");
				_isAnyErrors=true;
				return;
			} 
			
            _period=Timeframe;
			
			if (!UseFirstMA) 
			{
				_ma1=GetIndicator<MovingAverage>(Instrument.Id, _period, PeriodMA2, ShiftMA2, MethodMA2, ApplyMA2To);
			}
			else
			{
				_ma1 = GetIndicator<MovingAverage>(Instrument.Id, _period, PeriodMA1, ShiftMA1, MethodMA1, ApplyMA1To);
            	_ma2 = GetIndicator<MovingAverage>(Instrument.Id, _period, PeriodMA2, ShiftMA2, MethodMA2, ApplyMA2To);
			}
			
			_barSeries = GetCustomSeries(Instrument.Id, _period);
		}
		
		protected override void NewQuote()
        {
        	_ask=Instrument.Ask;
		
			var mavIndex = TransformIndex(_barSeries.Range.To, _period, _period);
			
			if (!UseFirstMA) 
			{
				_ma1Value = _ma1.SeriesMa[mavIndex+_ma1.Shift];
			}
			else
			{
				_ma1Value = _ma1.SeriesMa[mavIndex+_ma1.Shift];
				_ma2Value = _ma2.SeriesMa[mavIndex+_ma2.Shift];
			}
			
			//Analyze();
        }
		
		protected override void NewBar()
        {
			Analyze();
		}
		
		protected override void PositionChanged(IPosition position, ModificationType type)
		{
			if ((type.Equals(ModificationType.Closed))&&(position.Id==_positionGuid)) 
			{
				_positionGuid=Guid.Empty; 
				_positionOpendSignalType=SignalType.None;
			}
		}
		
		private void Analyze()
		{
			if (_isAnyErrors) return;
			
			if (_firstQuote)
				{
					if (UseFirstMA && UseSecondMA)
					{
						if ((_ma1Value==0) || (_ma2Value==0) || (_ma1Value==double.NaN) || (_ma2Value==double.NaN) || (_ma1Value.Equals(_ma2Value))) return;
						if (_ma1Value<_ma2Value) _isMA1BelowMA2=true; 
					}
					_firstQuote=false; 
				}
			
			if (UseFirstMA && UseSecondMA)
			{
				if (_isMA1BelowMA2 && (_ma1Value>=_ma2Value))
				{
                    Print("LR_Intersection_slow_ma_with_fast_ma_down_to_up"); 
					_isMA1BelowMA2=false; 
					
					if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal)
					{
						CheckForReversSignal(SignalType.SlowMACrosFastMABottomTop);
						return; 
					}
					
					if (SlowMACrosFastMABottomTop.Equals(AdvisorOrderType.Buy)) Buy(SignalType.SlowMACrosFastMABottomTop);
					if (SlowMACrosFastMABottomTop.Equals(AdvisorOrderType.Sell)) Sell(SignalType.SlowMACrosFastMABottomTop);
					
				}
				
				if (!_isMA1BelowMA2 && (_ma1Value<_ma2Value))
				{
                    Print("LR_Intersection_slow_ma_with_fast_ma_up_to_down"); 
					_isMA1BelowMA2=true; 
					
					if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal)
					{
						CheckForReversSignal(SignalType.SlowMACrosFastMATopBottom);
						return; 
					}
					
					if (SlowMACrosFastMATopBottom.Equals(AdvisorOrderType.Buy)) Buy(SignalType.SlowMACrosFastMATopBottom);
					if (SlowMACrosFastMATopBottom.Equals(AdvisorOrderType.Sell)) Sell(SignalType.SlowMACrosFastMATopBottom);
				}
			}

				if (_ask>_ma1Value)
				{
					if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal)
					{
						CheckForReversSignal(SignalType.QuoteAboveMA);
						return; 
					}
					
				if (QuoteAboveMA.Equals(AdvisorOrderType.Buy)) Buy(SignalType.QuoteAboveMA); 
				if (QuoteAboveMA.Equals(AdvisorOrderType.Sell)) Sell(SignalType.QuoteAboveMA); 	
				}
			
				if (_ask<_ma1Value)
				{		
					if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal)
					{
						CheckForReversSignal(SignalType.QuoteBelowMA);
						return; 
					}
					
				if (QuoteBelowMA.Equals(AdvisorOrderType.Buy)) Buy(SignalType.QuoteBelowMA); 
				if (QuoteBelowMA.Equals(AdvisorOrderType.Sell)) Sell(SignalType.QuoteBelowMA); 	
				}
		}
			
		
		private void Buy(SignalType signal)
		{
			if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal) return; 
			
			var result=Trade.Buy(Instrument.Id, LotVolume, Stops.InPips(StopLoss, TakeProfit)); 		
			if (result.IsSuccessful) 
			{
				_positionGuid=result.Position.Id;
				_positionOpendSignalType=signal;
			}
    	}
		
		private void Sell(SignalType signal)
		{
			if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal) return; 
			
			var result=Trade.Sell(Instrument.Id, LotVolume, Stops.InPips(StopLoss, TakeProfit)); 
			if (result.IsSuccessful) 
			{
				_positionGuid=result.Position.Id;
				_positionOpendSignalType=signal;
			}
    	}
		
		private void CheckForReversSignal(SignalType currentSignal)
		{
			switch (currentSignal) 
			{
				case SignalType.SlowMACrosFastMABottomTop:  
					if (_positionOpendSignalType==SignalType.SlowMACrosFastMATopBottom) ClosePosition();
				break;	
				
				case SignalType.SlowMACrosFastMATopBottom:  
					if (_positionOpendSignalType==SignalType.SlowMACrosFastMABottomTop) ClosePosition();
				break;		
				
				case SignalType.QuoteAboveMA:
					if (_positionOpendSignalType==SignalType.QuoteBelowMA) ClosePosition();
				break;
				
				case SignalType.QuoteBelowMA:
					if (_positionOpendSignalType==SignalType.QuoteAboveMA) ClosePosition();
				break;	
			}
		}
		
		private void ClosePosition()
		{
			var result =Trade.CloseMarketPosition(_positionGuid);
			if (result.IsSuccessful) 
			{
                Print("LR_Position_closed_by_inverse_signal", result.Position.Number, result.Position.ClosePrice);	
				_positionGuid=Guid.Empty;
				_positionOpendSignalType=SignalType.None;
			}
		}
    }
}
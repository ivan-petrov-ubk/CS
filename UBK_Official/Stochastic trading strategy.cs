using System;
using IPro.Model.Programming;
using IPro.Model.Programming.TradeSystems;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Indicators.Standard;
using IPro.Model.Client.MarketData;
using System.Collections.Generic;
using IPro.Model.Client.Trade;

namespace IPro.TradeSystems
{
    [TradeSystem("Stochastic trading strategy")]
    [ParametersGroup(__ui_cGroup_Parameters, "LR_Stochastic_indicator_parameters", Order = -2)]
    [ParametersGroup(__ui_cGroup_Signals, "LR_Generated_signals", Order = -1)]
    [ParametersGroup(__ui_cGroup_Props, "LR_Order_properties", Order = 0)]	
	public class StrategyStochastic : TradeSystem
    {
		#region Parameters

        [Parameter("LR_Period_K", DefaultValue = 5, MinValue = 1, GroupId = __ui_cGroup_Parameters)]
        public int PeriodK { get; set; }
        [Parameter("LR_Period_D", DefaultValue = 3, MinValue = 1, GroupId = __ui_cGroup_Parameters)]
        public int PeriodD { get; set; }
        [Parameter("LR_Slowdown", DefaultValue = 3, MinValue = 1, GroupId = __ui_cGroup_Parameters)]
		public int Slowdown { get; set; }
        [Parameter("LR_Method", DefaultValue = MaMethods.Sma, GroupId = __ui_cGroup_Parameters)]
		public MaMethods Method { get; set; }
        [Parameter("LR_Price", DefaultValue = PricePair.LowHigh, GroupId = __ui_cGroup_Parameters)]
        public PricePair Prices { get; set; }

        [Parameter("LR_Position_type", DefaultValue = AdvisorOrderType.Buy, Header = "LR_K_lines_down_to_lower_20_than_up_to_higher_80_percents", GroupId = __ui_cGroup_Signals)]
		public AdvisorOrderType LinesLeave20 { get; set; }
        [Parameter("LR_Position_type", DefaultValue = AdvisorOrderType.Sell, Header = "LR_K_and_D_lines_up_to_higher_80_than_down_to_lower_80_percents", GroupId = __ui_cGroup_Signals)]
		public AdvisorOrderType LinesLeave80 { get; set; }
        [Parameter("LR_Position_type", DefaultValue = AdvisorOrderType.Buy, Header = "LR_K_line_up_to_higher_than_line_D_down_to_up", GroupId = __ui_cGroup_Signals)]
		public AdvisorOrderType LineKcrosDfromBottom { get; set; }
        [Parameter("LR_Position_type", DefaultValue = AdvisorOrderType.Sell, Header = "LR_K_line_down_to_lower_than_D_Line_up_to_down", GroupId = __ui_cGroup_Signals)]
		public AdvisorOrderType LineKcrosDfromTop { get; set; }

        [Parameter("LR_Lot", DefaultValue = 0.01, IsLot = true, GroupId = __ui_cGroup_Props)]
        public decimal LotVolume { get; set; }
        [Parameter("LR_Take_Profit", DefaultValue = 0, GroupId = __ui_cGroup_Props, Postfix = "LR_Points", MaxValue=99999)]
        public int TakeProfit { get; set; }
        [Parameter("LR_Stop_Loss", DefaultValue = 0, GroupId = __ui_cGroup_Props, Postfix = "LR_Points", MaxValue=99999)]
        public int StopLoss { get; set; }
        [Parameter("LR_Position_closing_on_inverse_signal", DefaultValue = true, GroupId = __ui_cGroup_Props)]
        public bool IsDoCloseOnReverseSignal { get; set; }
		
		#endregion
		
		#region UI
		
		private const string __ui_cGroup_Parameters = "g-indicator-params-1";
		private const string __ui_cGroup_Signals="g-order-signals";
		private const string __ui_cGroup_Props = "g-order-props";
		
		#endregion
		
		private enum SignalType
		{
			KDLeave20,
			KDLeave80,
			KcrosDfromTop,
			KcrosDfromBottom,
			None
		}
		
		private StochasticOscillator _sto;
		private Period _period = new Period(PeriodType.Minute, 1);
		private ISeries<Bar> _barSeries;
		private int _stoIndex;
		private double _stoSignalValue;
		private double _stoMainValue;
		private bool _linesInZone20=false;
		private bool _linesInZone80=false;
		private bool _isKBelowD=false;
		private bool _firstStoValue=true;
		private bool _isAnyErrors=false;		
		private Guid _positionGuid=Guid.Empty;
		private SignalType _positionOpendSignalType=SignalType.None;
		
		protected override void Init()
		{
            Print("LR_Trade_strategy_initalization");
			
			if ((LotVolume>Account.MaxLot)||(LotVolume<Account.MinLot))
			{
                Print("LR_Invalid_lot_valueа");
                Print("LR_Lot_minimum_value " + (double)Account.MinLot);
                Print("LR_Lot_maximum_value " + (double)Account.MaxLot);
				_isAnyErrors=true;
				return;
			}
			
			_period=Timeframe;
			_sto =GetIndicator<StochasticOscillator>(Instrument.Id, _period, PeriodK, PeriodD, Slowdown, Method, Prices);	
			_barSeries = GetCustomSeries(Instrument.Id, _period);
		}
		
        protected override void NewQuote()
        {
            _stoIndex = TransformIndex(_barSeries.Range.To, _period, _period);	
			_stoSignalValue = _sto.SignalLine[_stoIndex];
			_stoMainValue = _sto.MainLine[_stoIndex];
        
			Analyze();
		}
		
		protected override void NewBar()
        {
			//Analyze();
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
			
			if (_firstStoValue)
				{
					if ((_stoSignalValue==0) || (_stoMainValue==0) || (_stoSignalValue==double.NaN) || (_stoMainValue==double.NaN)) return;
					if (_stoSignalValue<_stoMainValue) _isKBelowD=true;  
					_firstStoValue=false; 
				}
				
			if ((_isKBelowD)&&(_stoSignalValue>=_stoMainValue)) 
			{
				_isKBelowD=false;
                Print("LR_K_line_up_to_higher_than_line_D_down_to_up");
				KCrosDEvent();
			}	
			
			if ((!_isKBelowD)&&(_stoSignalValue<_stoMainValue)) 
			{
				_isKBelowD=true;
                Print("LR_K_line_down_to_lower_than_D_Line_up_to_down");
				KCrosDEvent();
			}	
		
			if ((_stoSignalValue<20)&&(_stoMainValue<20)) _linesInZone20=true;
			if ((_linesInZone20)&&(_stoSignalValue>20)&&(_stoMainValue>20))	
			{
                Print("LR_K_and_d_lines_up_to_higher_than_20_percents");
				_linesInZone20=false;
				LinesLeaveZone20Event();
			}
			
			if ((_stoSignalValue>80)&&(_stoMainValue>80)) _linesInZone80=true;
			if ((_linesInZone80)&&(_stoSignalValue<80)&&(_stoMainValue<80))	
			{
                Print("LR_K_and_d_lines_down_to_lower_than_80_percents");
				_linesInZone80=false;
				LinesLeaveZone80Event();
			}
		}
		
		private void KCrosDEvent()
		{
			if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal)
					{
						CheckForReversSignal(_isKBelowD ? SignalType.KcrosDfromTop : SignalType.KcrosDfromBottom);
						return; 
					}
			
			if (!_isKBelowD)
			{
				if (LineKcrosDfromBottom.Equals(AdvisorOrderType.Buy)) Buy(SignalType.KcrosDfromBottom); 
			    if (LineKcrosDfromBottom.Equals(AdvisorOrderType.Sell)) Sell(SignalType.KcrosDfromBottom); 	
			}
			else
			{
				if (LineKcrosDfromTop.Equals(AdvisorOrderType.Buy)) Buy(SignalType.KcrosDfromTop); 
			    if (LineKcrosDfromTop.Equals(AdvisorOrderType.Sell)) Sell(SignalType.KcrosDfromTop); 	
			}
		}
		
		private void LinesLeaveZone20Event()
		{
			if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal)
					{
						CheckForReversSignal(SignalType.KDLeave20);
						return; 
					}
			
			if (LinesLeave20.Equals(AdvisorOrderType.Buy)) Buy(SignalType.KDLeave20); 
			if (LinesLeave20.Equals(AdvisorOrderType.Sell)) Sell(SignalType.KDLeave20); 	
		}
		
		private void LinesLeaveZone80Event()
		{
			if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal)
					{
						CheckForReversSignal(SignalType.KDLeave80);
						return; 
					}
			
			if (LinesLeave80.Equals(AdvisorOrderType.Buy)) Buy(SignalType.KDLeave80); 
			if (LinesLeave80.Equals(AdvisorOrderType.Sell)) Sell(SignalType.KDLeave80); 	
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
				case SignalType.KcrosDfromBottom:  
					if (_positionOpendSignalType==SignalType.KcrosDfromTop) ClosePosition();
				break;	
				
				case SignalType.KcrosDfromTop:
					if (_positionOpendSignalType==SignalType.KcrosDfromBottom) ClosePosition();
				break;
				
				case SignalType.KDLeave20:
					if (_positionOpendSignalType==SignalType.KDLeave80) ClosePosition();
				break;	
				
				case SignalType.KDLeave80:
					if (_positionOpendSignalType==SignalType.KDLeave20) ClosePosition();
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
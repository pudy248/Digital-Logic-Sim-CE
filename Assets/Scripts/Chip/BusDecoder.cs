using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLS.Simulation;
using UnityEngine.XR;

namespace Assets.Scripts.Chip
{
	public class BusDecoder : BuiltinChip
	{
		protected override void Awake()
		{
			base.Awake();
		}

		protected override void ProcessOutput()
		{
			var inputSignal = inputPins[0].State;
			foreach(var outputPin in outputPins.Reverse())
			{
				outputPin.ReceiveSignal((PinState)(inputSignal.ToUint() & 1));
				inputSignal = (PinState) (inputSignal.ToUint()>>1);
			}
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLS.Simulation;
using UnityEngine.XR;

namespace Assets.Scripts.Chip
{
	public class BusEncoder : BuiltinChip
	{
		protected override void Awake()
		{
			base.Awake();
		}

		protected override void ProcessOutput()
		{
			uint outputSignal = 0;
			foreach(var inputState in inputPins.Select(x => x.State))
			{
				outputSignal <<= 1;
				outputSignal |= (uint)inputState;
			}
			outputPins[0].ReceiveSignal((PinState)outputSignal);
		}
	}
}

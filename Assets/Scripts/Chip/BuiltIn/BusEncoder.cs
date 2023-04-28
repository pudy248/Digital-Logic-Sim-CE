﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using DLS.Simulation;
using UnityEngine;
using UnityEngine.XR;

namespace Assets.Scripts.Chip
{
	public class BusEncoder : BuiltinChip
	{
		protected override void Awake()
		{
			base.Awake();
		}
		public override void Init()
		{
			base.Init();
			ChipType = ChipType.Miscellaneous;
			PackageGraphicData = new PackageGraphicData()
			{
				PackageColour = new Color(255, 135, 0, 255)
			};
			inputPins = new List<Pin>(4);
			outputPins = new List<Pin>(1);
			chipName = "4 BIT ENCODER";
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

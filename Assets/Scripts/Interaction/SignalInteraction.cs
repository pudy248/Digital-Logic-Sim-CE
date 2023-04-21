using System;
using System.Collections.Generic;
using System.Linq;
using Interaction.Display;
using UnityEngine;
using UnityEngine.Serialization;

namespace Interaction
{
    public enum HandleState
    {
        Default,
        Highlighted,
        Selected,
        SelectedAndFocused
    }

    [RequireComponent(typeof(PinHandlerDisplay))]
    public class SignalInteraction : Interactable
    {
        [FormerlySerializedAs("Display")] [Header("Reference")] [SerializeField]
        private PinHandlerDisplay HandlerDisplay;

        [SerializeField] private ChipSignal signalPrefab;

        public List<ChipSignal> Signals { get; private set; }
        private int GroupSize;
        private int ID;

        private HandleState HandleState = HandleState.Default;


        float BoundsTop;
        float BoundsBottom;
        public event Action<Chip> OnDeleteChip;

        public Vector3 PinContainers { get; set; }
        const float forwardDepth = -0.1f;

        public bool IsPreview;

        public Transform SignalHolder { get; set; }


        private void Awake()
        {
            HandlerDisplay = GetComponent<PinHandlerDisplay>();
        }

        public void SetUpCreation(int _groupSize, float _boundsBottom, float _boundsTop,
            Vector3 _pinContainers, Action<Chip> _onDeleteChip, Transform _signalHolder)
        {
            GroupSize = _groupSize;
            BoundsBottom = _boundsBottom;
            BoundsTop = _boundsTop;
            PinContainers = _pinContainers;
            Signals = new List<ChipSignal>(GroupSize);
            
            for (var i = 0; i < Signals.Capacity; i++)
                Signals.Add(Instantiate(signalPrefab,gameObject.transform) );


            OnDeleteChip = _onDeleteChip;
            SignalHolder = _signalHolder;
        }


        public override void OrderedUpdate()
        {
            // Display.DrawHandle();
        }

        public void UpdateScale()
        {
            foreach (ChipSignal chipSignal in Signals)
            {
                chipSignal.GetComponent<IOScaler>().UpdateScale();
            }

            float yPos = 0;
            foreach (ChipSignal sig in Signals)
            {
                yPos += sig.transform.localPosition.y;
            }

            var handleNewY = yPos / Signals.Count;

            for (var i = 0; i < Signals.Count; i++)
            {
                var y = CalculatePinYPos(handleNewY, i);
                Signals[i].transform.SetYPos(y);
            }
        }

        public void SpawnPreview()
        {
            for (var i = 0; i < GroupSize; i++)
            {
                float posY = CalculatePinYPos(InputHelper.MouseWorldPos.y, i);
                Vector3 spawnPos = new Vector3(PinContainers.x, posY, PinContainers.z + forwardDepth);

                ChipSignal spawnedSignal = Instantiate(signalPrefab, spawnPos, Quaternion.identity, SignalHolder);
                spawnedSignal.SetInteractable(false);
                spawnedSignal.signalName = "Preview";
                spawnedSignal.GetComponent<IOScaler>().UpdateScale();

                Signals.Add(spawnedSignal);

                if (Signals.Count > 1)
                    spawnedSignal.displayGroupDecimalValue = true;

                Signals[i] = spawnedSignal;
            }
        }


        public void SpawnPin()
        {
            foreach (var sig in Signals)
                sig.SetInteractable(true);
        }


        public void SetUpPosition(float handleNewY)
        {
            for (int i = 0; i < Signals.Count; i++)
            {
                float y = CalculatePinYPos(handleNewY, i);
                Signals[i].transform.SetYPos(y);
            }
        }

        public void ChangeWireType(int mode)
        {
            // Change output pin wire mode
            foreach (var sig in Signals)
                sig.wireType = (Pin.WireType)mode;

            foreach (var pin in Signals.SelectMany(x => x.inputPins))
            {
                pin.wireType = (Pin.WireType)mode;
                Manager.ActiveChipEditor.pinAndWireInteraction.DestroyConnectedWires(pin);
            }

            // Change input pin wire mode
            if (Signals[0] is not InputSignal) return;

            foreach (InputSignal signal in Signals)
            {
                var pin = signal.outputPins[0];
                if (pin == null) return;
                pin.wireType = (Pin.WireType)mode;
                Manager.ActiveChipEditor.pinAndWireInteraction.DestroyConnectedWires(pin);
                signal.SetState(0);
            }
        }


        float CalculatePinYPos(float mouseY, int index)
        {
            float centreY = mouseY;
            float halfExtent = ScalingManager.groupSpacing * (GroupSize - 1f);
            float maxY = centreY + halfExtent + ScalingManager.handleSizeY / 2f;
            float minY = centreY - halfExtent - ScalingManager.handleSizeY / 2f;

            if (maxY > BoundsTop)
            {
                centreY -= (maxY - BoundsTop);
            }
            else if (minY < BoundsBottom)
            {
                centreY += (BoundsBottom - minY);
            }

            float t = (GroupSize > 1) ? index / (GroupSize - 1f) : 0.5f;
            t = t * 2 - 1;
            float posY = centreY - t * halfExtent;
            return posY;
        }


        public Vector3 GetGroupCenter()
        {
            return (Signals[0].transform.position + Signals[^1].transform.position) / 2;
        }


        public void DrawSignalsHandle()
        {
            foreach (ChipSignal singnal in Signals)
                HandlerDisplay.DrawHandle(singnal.transform.position.y, HandleState);
        }


        public void UpdateProperty(string NewName, bool twosComplementToggle)
        {
            // Update signal properties
            foreach (ChipSignal Signal in Signals)
            {
                Signal.UpdateSignalName(NewName);
                Signal.useTwosComplement = twosComplementToggle;
            }
        }

        public float SelectSignal()
        {
            var dragMouseStartY = InputHelper.MouseWorldPos.y;
            float dragHandleStartY;

            if (Signals.Count % 2 == 0)
            {
                int indexA = Mathf.Max(0, Signals.Count / 2 - 1);
                int indexB = Signals.Count / 2;
                dragHandleStartY = (Signals[indexA].transform.position.y +
                                    Signals[indexB].transform.position.y) /
                                   2f;
            }
            else
            {
                dragHandleStartY = Signals[Signals.Count / 2].transform.position.y;
            }

            RequestFocus();

            return dragHandleStartY;
        }


        public bool Contains(ChipSignal chip)
        {
            return Signals.Contains(chip);
        }

        public override void DeleteCommand()
        {
            foreach (ChipSignal selectedSignal in Signals)
            {
                OnDeleteChip?.Invoke(selectedSignal);

                Destroy(selectedSignal.gameObject);
            }

            throw new System.NotImplementedException();
        }
    }
}
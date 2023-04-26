using System;
using System.Collections.Generic;
using System.Linq;
using DLS.Simulation;
using Interaction.Display;
using JetBrains.Annotations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.Serialization;

namespace Interaction
{
    public class SignalInteraction : Interactable
    {
        //Editor 
        [SerializeField] private ChipSignal signalPrefab;


        //Work Variable
        public List<ChipSignal> Signals { get; private set; }
        private int GroupSize;
        private int ID;

        float BoundsTop;
        float BoundsBottom;

        public ChipInterfaceEditor.EditorInterfaceType EditorInterfaceType { get; private set; }


        //Event
        public event Action<Chip> OnDeleteChip;
        public event Action<Vector3, ChipInterfaceEditor.EditorInterfaceType> OnDragig;
        [CanBeNull] public event Action OnDeleteInteraction;


        //Property 
        public bool IsGroup => GroupSize > 1;
        public string SignalName => Signals[0].signalName;
        public bool UseTwosComplement => Signals[0].useTwosComplement;
        public Pin.WireType WireType => Signals[0].wireType;

        public Vector3 GroupCenter => (Signals[0].transform.position + Signals[^1].transform.position) / 2;


        public void SetUpCreation(int _groupSize, float _boundsBottom, float _boundsTop,
            Vector3 _pinContainers, Action<Chip> _onDeleteChip,
            ChipInterfaceEditor.EditorInterfaceType _editorInterfaceType)
        {
            EditorInterfaceType = _editorInterfaceType;
            GroupSize = _groupSize;
            BoundsBottom = _boundsBottom;
            BoundsTop = _boundsTop;
            Signals = new List<ChipSignal>(GroupSize);

            for (int i = 0; i < GroupSize; i++)
            {
                var spawnedSignal = Instantiate(signalPrefab, _pinContainers, Quaternion.identity, transform);
                Signals.Add(spawnedSignal);
            }

            UpdateScaleAndPosition();
            SetPinInteractable();
            RegisterEventToAllHandle();

            OnDeleteChip += _onDeleteChip;

            MenuManager.instance.signalPropertiesMenu.RegisterSignalGroup(this);
            OnFocusLost += MenuManager.instance.CloseMenu;
            OnFocusObtained += OpenPropertyMenu;

            RequestFocus();
        }

        private void RegisterEventToAllHandle()
        {
            foreach (var Handle in Signals.Select(x => x.GetComponentInChildren<HandleEvent>()))
            {
                var HandlerDisplais = Signals.Select(x => x.GetComponentInChildren<SignalHandlerDisplay>());
                foreach (var HandlerDisplay in HandlerDisplais)
                {
                    if (HandlerDisplay != null)
                        HandlerDisplay.RegisterToHandleGroup(Handle);
                }

                Handle.OnHandleClick += () =>
                {
                    NotifyMovement();
                    RequestFocus();
                };

                Handle.OnStartDrag += (pos) =>
                {
                    handleStartPosy = pos.y;
                    DragCancelled = false;
                };

                Handle.OnDrag += Drag;
                Handle.OnStopDrag += () => handleStartPosy = 0;
            }
        }

        private void NotifyMovement()
        {
            OnDragig?.Invoke(GroupCenter, EditorInterfaceType);
        }


        #region Positioning

        private float handleStartPosy;
        private bool DragCancelled = false;

        private void Drag()
        {
            if (DragCancelled) return;

            Vector2 mousePos = InputHelper.MouseWorldPos;
            float handleNewY = handleStartPosy - (handleStartPosy - mousePos.y);
            DragCancelled = Input.GetKeyDown(KeyCode.Escape);

            if (DragCancelled) handleNewY = handleStartPosy;

            SetUpPosition(handleNewY);
            NotifyMovement();
            // Cancel drag and deselect
            if (DragCancelled) ReleaseFocus();
        }


        public void UpdateScaleAndPosition()
        {
            foreach (ChipSignal chipSignal in Signals)
                chipSignal.GetComponent<IOScaler>().UpdateScale();

            for (var i = 0; i < Signals.Count; i++)
            {
                var y = AdjustYForGroupMember(transform.position.y, i);
                Signals[i].transform.SetYPos(y);
            }
        }

        float AdjustYForGroupMember(float DesideredCeterY, int index)
        {
            float halfExtent = ScalingManager.groupSpacing * (GroupSize - 1f);
            float maxY = DesideredCeterY + halfExtent + ScalingManager.handleSizeY / 2f;
            float minY = DesideredCeterY - halfExtent - ScalingManager.handleSizeY / 2f;

            if (maxY > BoundsTop)
                DesideredCeterY -= (maxY - BoundsTop);
            else if (minY < BoundsBottom)
                DesideredCeterY += (BoundsBottom - minY);

            float t = (GroupSize > 1) ? index / (GroupSize - 1f) : 0.5f;
            t = t * 2 - 1;
            float posY = DesideredCeterY - t * halfExtent;
            return posY;
        }


        float ClampYBetweenBorder(float y)
        {
            return Mathf.Clamp(y, BoundsBottom + ScalingManager.handleSizeY / 2f,
                BoundsTop - ScalingManager.handleSizeY / 2f);
        }

        #endregion


        private void SetPinInteractable()
        {
            foreach (var sig in Signals)
                sig.SetInteractable(true);
        }


        private void SetUpPosition(float handleNewY)
        {
            for (var i = 0; i < Signals.Count; i++)
            {
                var y = AdjustYForGroupMember(handleNewY, i);
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
                signal.SetState(PinState.LOW);
            }
        }


        #region Property

        void OpenPropertyMenu()
        {
            MenuManager.instance.OpenMenu(MenuType.SignalPropertiesMenu);
            MenuManager.instance.signalPropertiesMenu.SetUpUI(this);
        }


        public void UpdateGroupProperty(string NewName, bool twosComplementToggle)
        {
            // Update signal properties
            foreach (ChipSignal Signal in Signals)
            {
                Signal.UpdateSignalName(NewName);
                Signal.useTwosComplement = twosComplementToggle;
            }
        }

        #endregion


        public bool Contains(ChipSignal chip)
        {
            return Signals.Contains(chip);
        }

        public override void OrderedUpdate()
        {
        }

        public override void DeleteCommand()
        {
            foreach (ChipSignal selectedSignal in Signals)
            {
                OnDeleteChip?.Invoke(selectedSignal);

                Destroy(selectedSignal.gameObject);
            }

            OnDeleteInteraction?.Invoke();
            Destroy(gameObject);
        }
    }
}
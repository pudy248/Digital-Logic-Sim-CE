using System;
using System.Collections.Generic;
using System.Linq;
using DLS.Simulation;
using Interaction.Display;
using JetBrains.Annotations;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.Serialization;
using VitoBarra.System.Interaction;
using VitoBarra.Utils.TextVerifier;

namespace Interaction.Signal
{
    public class SignalInteraction : Interactable
    {
        //Editor 
        [SerializeField] private ChipSignal signalPrefab;

        private DecimalDisplay DecimalDisplay;


        //Work Variable
        public SignalReferenceHolderList Signals { get; private set; }
        public int SignalNumber { get; private set; }
        private int ID;


        float BoundsTop;
        float BoundsBottom;
        private Vector3 PinContainers;

        public EditorInterfaceType EditorInterfaceType { get; private set; }


        //Event
        public event Action<Chip> OnDeleteChip;
        public event Action<Vector3, EditorInterfaceType> OnDragig;
        [CanBeNull] public event Action OnDeleteInteraction;


        //Property 
        public bool IsGroup => SignalNumber > 1;
        public string SignalName => Signals.ChipSignals[0].signalName;
        public bool UseTwosComplement = true;
        public Pin.WireType WireType;
        public bool DisplayEnabled;

        public Vector3 GroupCenter => (Signals.ChipSignals[0].transform.position +
                                       Signals.ChipSignals[^1].transform.position) / 2;


        private void Awake()
        {
            DecimalDisplay = GetComponentInChildren<DecimalDisplay>(true);
        }

        private void Start()
        {
            ScalingManager.i.OnScaleChange += UpdateCenterPosition;
        }

        private void OnDestroy()
        {
            ScalingManager.i.OnScaleChange -= UpdateCenterPosition;
        }

        public void init(Pin.WireType wireType, float _boundsBottom, float _boundsTop,
            EditorInterfaceType _editorInterfaceType, Vector3 _pinContainers, bool displayEnabled = true)
        {
            WireType = wireType;
            EditorInterfaceType = _editorInterfaceType;
            BoundsBottom = _boundsBottom;
            BoundsTop = _boundsTop;
            PinContainers = _pinContainers;
            DisplayEnabled = displayEnabled;
        }


        public void SetUpCreation(Action<Chip> _onDeleteChip, int _groupSize, bool RequireFocus = true)
        {
            if (!DecimalDisplay)
                DecimalDisplay = GetComponentInChildren<DecimalDisplay>(true);

            SignalNumber = WireType != Pin.WireType.Simple ? 1 : _groupSize;



            Signals = new SignalReferenceHolderList(SignalNumber);
            for (var i = 0; i < SignalNumber; i++)
                SignalSpawnLogic(WireType, DisplayEnabled);

            if (IsGroup && DisplayEnabled)
                DecimalDisplay.gameObject.SetActive(true);


            UpdateCenterPosition();
            SetPinInteractable(true);

            OnDeleteChip += _onDeleteChip;

            MenuManager.instance.signalPropertiesMenu.RegisterSignalGroup(this);
            OnFocusLost += MenuManager.instance.CloseMenu;
            OnFocusObtained += OpenPropertyMenu;

            if (RequireFocus)
                RequestFocus();
        }

        private ChipSignal SignalSpawnLogic(Pin.WireType wireType, bool DisplayEnabled)
        {
            var spawnedSignal = CreateSignal();

            var e = Signals.AddSignals(spawnedSignal);
            e.ChipSignal.wireType = wireType;
            RegisterHandler(e.HandleEvent);

            if (!IsGroup || !DisplayEnabled) return spawnedSignal;

            spawnedSignal.OnStateChange += (_, _) =>
                DecimalDisplay.UpdateDecimalDisplay(Signals.ChipSignals, UseTwosComplement);

            return spawnedSignal;
        }

        private ChipSignal CreateSignal()
        {
            return Instantiate(signalPrefab, PinContainers, Quaternion.identity, transform);
        }


        private SignalReferenceHolder AddSignal()
        {
            SignalNumber++;
            return Signals.AddSignals(CreateSignal());
        }

        private void RemoveSignal()
        {
            Signals.RemoveSignals();
            SignalNumber--;
        }


        private void RegisterHandler(HandleEvent HandleEvent)
        {
            HandleEvent.OnHandleClick += () =>
            {
                NotifyMovement();
                RequestFocus();
            };

            HandleEvent.OnStartDrag += (pos) =>
            {
                DragStartY = pos.y;
                centerDragStartDistance = DragStartY - GroupCenter.y;
                DragCancelled = false;
            };

            HandleEvent.OnDrag += Drag;
            HandleEvent.OnStopDrag += () => DragStartY = 0;
        }


        private void NotifyMovement()
        {
            OnDragig?.Invoke(GroupCenter, EditorInterfaceType);
            Manager.ActiveChipEditor.chipInteraction.NotifyMovement();
        }


        #region Positioning

        private float DragStartY;
        private float centerDragStartDistance;
        private bool DragCancelled = false;


        private void Drag()
        {
            if (DragCancelled) return;

            Vector2 mousePos = InputHelper.MouseWorldPos;
            float handleNewY = mousePos.y - centerDragStartDistance;
            DragCancelled = Input.GetKeyDown(KeyCode.Escape);

            if (DragCancelled) handleNewY = DragStartY - centerDragStartDistance;

            MoveCenterYPosition(handleNewY);
            NotifyMovement();
            // Cancel drag and deselect
            if (DragCancelled) ReleaseFocus();
        }


        private void UpdateCenterPosition()
        {
            MoveCenterYPosition(transform.position.y);
        }

        private float AdjustYForGroupMember(float DesideredCeterY, int index)
        {
            var handleSizeY = ScalingManager.HandleSizeY;
            var GroupSpacing = ScalingManager.GroupSpacing;


            float halfExtent = GroupSpacing * (SignalNumber - 1f);
            float maxY = DesideredCeterY + halfExtent + handleSizeY / 2f;
            float minY = DesideredCeterY - halfExtent - handleSizeY / 2f;

            if (maxY > BoundsTop)
                DesideredCeterY -= (maxY - BoundsTop);
            else if (minY < BoundsBottom)
                DesideredCeterY += (BoundsBottom - minY);

            float t = (SignalNumber > 1) ? index / (SignalNumber - 1f) : 0.5f;
            t = t * 2 - 1;
            float posY = DesideredCeterY - t * halfExtent;
            return posY;
        }


        float ClampYBetweenBorder(float y)
        {
            var HandleSizeY = ScalingManager.HandleSizeY;
            return Mathf.Clamp(y, BoundsBottom + HandleSizeY / 2f,
                BoundsTop - HandleSizeY / 2f);
        }

        public void MoveCenterYPosition(float NewYcenter)
        {
            for (var i = 0; i < Signals.Count; i++)
            {
                var y = AdjustYForGroupMember(NewYcenter, i);
                Signals[i].ChipSignal.transform.SetYPos(y);
            }

            if (DecimalDisplay)
                DecimalDisplay.transform.SetYPos(GroupCenter.y);
        }

        #endregion


        public void SetPinInteractable(bool togle = true)
        {
            foreach (var sig in Signals.ChipSignals)
                sig.SetInteractable(togle);
        }


        #region Property

        public void ChangeWireType(Pin.WireType newWireType)
        {
            WireType = newWireType;
            // Change output pin wire mode
            foreach (var sig in Signals.ChipSignals)
                sig.wireType = newWireType;

            foreach (var pin in Signals.ChipSignals.SelectMany(x => x.inputPins))
            {
                pin.wireType = newWireType;
                Manager.PinAndWireInteraction.DestroyConnectedWires(pin);
            }

            // Change input pin wire mode
            if (Signals.ChipSignals[0] is not InputSignal) return;

            foreach (InputSignal signal in Signals.ChipSignals)
            {
                var pin = signal.outputPins[0];
                if (pin == null) return;
                pin.wireType = newWireType;
                Manager.PinAndWireInteraction.DestroyConnectedWires(pin);
                signal.SetState(PinStates.AllLow(newWireType));
            }
        }

        void OpenPropertyMenu()
        {
            MenuManager.instance.signalPropertiesMenu.SetUpUI(this);
        }


        public void UpdateGroupProperty(string NewName, bool twosComplementToggle)
        {
            // Update signal properties
            foreach (var Signal in Signals.ChipSignals)
            {
                Signal.UpdateSignalName(NewName);
                UseTwosComplement = twosComplementToggle;
            }

            if (IsGroup)
                DecimalDisplay.UpdateDecimalDisplay(Signals.ChipSignals, UseTwosComplement);
        }

        public void SetBusValue(int state)
        {
            if (IsGroup) return;
            if (Signals.ChipSignals[0] is InputSignal inputSignal)
                inputSignal.SetBusStatus(state < 0 ? 0 : (uint)state);
        }

        #endregion


        public bool Contains(ChipSignal chip)
        {
            return Signals.ChipSignals.Contains(chip);
        }

        #region Interaction

        public override void OrderedUpdate()
        {
        }

        private bool DeleteAllowed = true;

        public override void DeleteCommand()
        {
            if (!DeleteAllowed) return;
            foreach (var selectedSignal in Signals.ChipSignals)
            {
                OnDeleteChip?.Invoke(selectedSignal);

                Destroy(selectedSignal.gameObject);
            }

            OnDeleteInteraction?.Invoke();
            Destroy(gameObject);
        }

        public void SilenceDeleteCommand()
        {
            DeleteAllowed = false;
        }

        public void EnableDeleteCommand()
        {
            DeleteAllowed = true;
        }

        #endregion

        private void OnDrawGizmos()
        {
            var dragStart = new Vector2(transform.position.x, DragStartY);
            if (DragStartY != 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(dragStart, 0.05f);
            }

            Gizmos.color = Color.magenta;
            var center = new Vector2(transform.position.x, GroupCenter.y);

            Gizmos.DrawSphere(center, 0.05f);
            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(center, dragStart);
        }

        public List<SignalReferenceHolder> SetGroupSize(int desiredGroupSize)
        {
            var list = new List<SignalReferenceHolder>();
            var e = desiredGroupSize - SignalNumber;
            switch (e)
            {
                case < 0:
                    for (var i = 0; i < -e; i++)
                        RemoveSignal();
                    break;
                case > 0:
                    for (var i = 0; i < e; i++)
                    {
                        list.Add(AddSignal());
                    }

                    break;
            }

            return list;
        }
    }
}
﻿using System;
using Interaction.Signal;
using UnityEngine;

namespace Interaction.Signal
{
    public class SignalInteractionBuilder
    {
        SignalInteraction SignalInteractablePref;
        Transform SignalHolder;
        event Action<Chip> OnDeleteChip;
        float BoundsBottom;
        float BoundsTop;
        private float xContainer;
        private float zContainer;
        
        int NextGroupID = 0;
        EditorInterfaceType editorInterfaceType;
        
        public SignalInteractionBuilder(SignalInteraction signalInteractablePref, Transform signalHolder, Action<Chip> onDeleteChip,float boundsBottom,float boundsTop,float _xContainer,float _zContainer, EditorInterfaceType _editorInterfaceType)
        {
            SignalInteractablePref = signalInteractablePref;
            SignalHolder = signalHolder;
            OnDeleteChip = onDeleteChip;

            BoundsBottom = boundsBottom;
            BoundsTop = boundsTop;
            editorInterfaceType = _editorInterfaceType;
            xContainer = _xContainer;
            zContainer = _zContainer;
        }


  

        public (int id,SignalInteraction obj) Build(float  yPos, int desiredGroupSize,bool RequiredFocus= true,bool DisplayEnabled = true)
        {
            var ContaierPosition = new Vector3(xContainer, yPos, zContainer);
            var SignalInteractable = GameObject.Instantiate(SignalInteractablePref,SignalHolder);
            SignalInteractable.transform.SetPositionAndRotation(ContaierPosition, SignalInteractable.transform.rotation);
            SignalInteractable.SetUpCreation(desiredGroupSize, BoundsBottom, BoundsTop, ContaierPosition, OnDeleteChip,editorInterfaceType,RequiredFocus,DisplayEnabled);

            return (NextGroupID++,SignalInteractable);
        }
    }
}
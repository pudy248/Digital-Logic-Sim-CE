using System;
using UnityEngine;

namespace Interaction.Builder
{
    public class SignalInteractionBuilder
    {
        SignalInteraction SignalInteractablePref;
        Transform SignalHolder;
        event Action<Chip> OnDeleteChip;
        float BoundsBottom;
        float BoundsTop;
        int NextGroupID = 0;
        
        public SignalInteractionBuilder(SignalInteraction signalInteractablePref, Transform signalHolder, Action<Chip> onDeleteChip,float boundsBottom,float boundsTop)
        {
            SignalInteractablePref = signalInteractablePref;
            SignalHolder = signalHolder;
            OnDeleteChip = onDeleteChip;

            BoundsBottom = boundsBottom;
            BoundsTop = boundsTop;
        }


  

        public (int id,SignalInteraction obj) Build(Vector3 vec, int desiredGroupSize)
        {
            var PinInteractable = GameObject.Instantiate(SignalInteractablePref);
            PinInteractable.transform.SetPositionAndRotation(vec, PinInteractable.transform.rotation);
            PinInteractable.SetUpCreation(desiredGroupSize, BoundsBottom, BoundsTop, vec, OnDeleteChip, SignalHolder);

            return (NextGroupID++,PinInteractable);
        }
    }
}
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
        ChipInterfaceEditor.EditorInterfaceType editorInterfaceType;
        
        public SignalInteractionBuilder(SignalInteraction signalInteractablePref, Transform signalHolder, Action<Chip> onDeleteChip,float boundsBottom,float boundsTop,ChipInterfaceEditor.EditorInterfaceType _editorInterfaceType)
        {
            SignalInteractablePref = signalInteractablePref;
            SignalHolder = signalHolder;
            OnDeleteChip = onDeleteChip;

            BoundsBottom = boundsBottom;
            BoundsTop = boundsTop;
            editorInterfaceType = _editorInterfaceType;
        }


  

        public (int id,SignalInteraction obj) Build(Vector3 ContaierPosition, int desiredGroupSize)
        {
            var SignalInteractable = GameObject.Instantiate(SignalInteractablePref,SignalHolder);
            SignalInteractable.transform.SetPositionAndRotation(ContaierPosition, SignalInteractable.transform.rotation);
            SignalInteractable.SetUpCreation(desiredGroupSize, BoundsBottom, BoundsTop, ContaierPosition, OnDeleteChip,editorInterfaceType);

            return (NextGroupID++,SignalInteractable);
        }
    }
}
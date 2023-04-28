using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

namespace Interaction.Display
{
    public enum HandleState
    {
        Default,
        Highlighted,
        Focused
    }
    public class SignalHandlerDisplay : MonoBehaviour
    {
        private Renderer HandlerRender;
        private SignalInteraction Interaction;

        private bool ColorCanChange;

        private void Awake()
        {
            Interaction = GetComponentInParent<SignalInteraction>();
            HandlerRender = GetComponent<Renderer>();
            var Lissener = GetComponentInParent<HandleEvent>();
            RegisterToHandleGroup(Lissener);
            
            if(Interaction == null) return;
            
            Interaction.OnFocusObtained += () =>
            {
                ChangeHandleColor(HandleState.Focused);
                ColorCanChange = false;
            };
            Interaction.OnFocusLost += () =>
            {
                ChangeHandleColor(HandleState.Default);
                ColorCanChange = true;
            };

        }

        public void RegisterToHandleGroup(HandleEvent Lissener)
        {
            Lissener.OnHandleEnter += () => CheckedChangeHandleColor(HandleState.Highlighted);
            Lissener.OnHandleExit += () => CheckedChangeHandleColor(HandleState.Default);
        }


        private void CheckedChangeHandleColor(HandleState handleState = HandleState.Default)
        {
            if (ColorCanChange)
                ChangeHandleColor(handleState);
        }

        private void ChangeHandleColor(HandleState handleState = HandleState.Default)
        {
            HadleTheme materialReference = UIThemeManager.HadleTheme;
            Material selectedMat;
            switch (handleState)
            {
                case HandleState.Highlighted:
                    selectedMat = materialReference.HighlightedHandleMat;
                    break;
                case HandleState.Focused:
                    selectedMat = materialReference.FocusedHandle;
                    break;
                default:
                    selectedMat = materialReference.HandleMat;
                    break;
            }

            HandlerRender.material = selectedMat;
        }
    }
}
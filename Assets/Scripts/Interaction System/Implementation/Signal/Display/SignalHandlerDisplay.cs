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
        Selected,
        SelectedAndFocused
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
            var Lissener = GetComponentInParent<HandleSubject>();
            RegisterToHandleGroup(Lissener);

        }

        public void RegisterToHandleGroup(HandleSubject Lissener)
        {
            Lissener.OnHandleEnter += () => CheckedChangeHandleColor(HandleState.Highlighted);
            Lissener.OnHandleExit += () => CheckedChangeHandleColor(HandleState.Default);
            Lissener.OnHandleClick += () => ChangeHandleColor(HandleState.Selected);
            Interaction.OnFocusObtained += () =>
            {
                ChangeHandleColor(HandleState.SelectedAndFocused);
                ColorCanChange = false;
            };
            Interaction.OnFocusLost += () =>
            {
                ChangeHandleColor(HandleState.Default);
                ColorCanChange = true;
            };
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
                case HandleState.Selected:
                    selectedMat = materialReference.SelectedHandleMat;
                    break;
                case HandleState.SelectedAndFocused:
                    selectedMat = materialReference.SelectedAndhighlightedHandle;
                    break;
                default:
                    selectedMat = materialReference.HandleMat;
                    break;
            }

            HandlerRender.material = selectedMat;
        }
    }
}
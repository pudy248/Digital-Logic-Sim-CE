using System;
using UnityEngine;

namespace Interaction.Display
{
    public class PinHandlerDisplay : MonoBehaviour
    {
        public PinInteractionPalette PinPalette;

        //Handler
        Mesh quadMesh;
        Material handleMat;
        Material highlightedHandleMat;
        Material selectedHandleMat;
        Material selectedAndhighlightedHandle;
        [SerializeField] private float xpos;
        const float handleSizeX = 0.15f;



        const float forwardDepth = -0.1f;

        public void Awake()
        {
            PinPalette = UIManager.Palette.PinInteractionPalette;

            MeshShapeCreator.CreateQuadMesh(ref quadMesh);
            handleMat = MaterialUtility.CreateUnlitMaterial(PinPalette.handleCol);
            highlightedHandleMat =
                MaterialUtility.CreateUnlitMaterial(PinPalette.highlightedHandleCol);
            selectedHandleMat =
                MaterialUtility.CreateUnlitMaterial(PinPalette.selectedHandleCol);
            selectedAndhighlightedHandle =
                MaterialUtility.CreateUnlitMaterial(PinPalette.selectedAndFocusedHandleCol);
        }

        public void DrawHandle(float y, HandleState handleState = HandleState.Default)
        {
            float renderZ = forwardDepth;
            Material currentHandleMat;
            switch (handleState)
            {
                case HandleState.Highlighted:
                    currentHandleMat = highlightedHandleMat;
                    break;
                case HandleState.Selected:
                    currentHandleMat = selectedHandleMat;
                    renderZ = forwardDepth * 2;
                    break;
                case HandleState.SelectedAndFocused:
                    currentHandleMat = selectedAndhighlightedHandle;
                    renderZ = forwardDepth * 2;
                    break;
                default:
                    currentHandleMat = handleMat;
                    break;
            }

            Vector3 scale = new Vector3(handleSizeX, ScalingManager.handleSizeY, 1);
            Vector3 pos3D = new Vector3(transform.position.x+xpos, y, transform.position.z + renderZ);
            Matrix4x4 handleMatrix = Matrix4x4.TRS(pos3D, Quaternion.identity, scale);
            Graphics.DrawMesh(quadMesh, handleMatrix, currentHandleMat, 0);
        }

        
     
    }
}
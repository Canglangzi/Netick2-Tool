using UnityEngine;
using Netick;
using Netick.Unity;

public class PlayerCharacterVisual : NetworkBehaviour
{
    [Networked] public Color MeshColor { get; set; }

    public MeshRenderer meshRenderer;


    public struct PlayerCharacterVisualInput: INetworkInput
    {
        public bool RandomizeColor;    
    }
    
    public override void NetworkUpdate()
    {
        if (!IsInputSource) 
            return;
            PlayerCharacterVisualInput input = Sandbox.GetInput<PlayerCharacterVisualInput>();
            input.RandomizeColor = Input.GetKey(KeyCode.Space);

            Sandbox.SetInput(input);
    }
    public override void NetworkFixedUpdate()
    {
        if (FetchInput(out PlayerCharacterVisualInput input))
        {
            if (input.RandomizeColor)
                MeshColor = Random.ColorHSV(0f, 1f);
        }
    }

    [OnChanged(nameof(MeshColor))]
    private void OnColorChanged(OnChangedData onChangedData)
    {
        meshRenderer.material.color = MeshColor;
    }
}

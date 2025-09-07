using UnityEngine;
using Netick;
using Netick.Unity;
using TMPro;

public class PlayerRPC : NetworkBehaviour
{

    [Networked] public NetworkString32 Nickname { get; set; }

    [Rpc(source: RpcPeers.InputSource, target: RpcPeers.Owner, isReliable: true)]
    public void RPC_SetNicknameRandom()
    {
        Nickname = new NetworkString32($"Player_{Random.Range(1000, 9999)}");
    }
    public TMP_Text TextNametag;
  

    [OnChanged(nameof(Nickname))]
    private void OnNicknameChanged(OnChangedData onChangedData)
    {
        TextNametag.SetText(Nickname);
    }

    public override void NetworkUpdate()
    {
        if (IsInputSource && Input.GetKeyDown(KeyCode.Return) && Sandbox.InputEnabled)
        {
            RPC_SetNicknameRandom();
        }
    }
}

using Netick.Unity;
using UnityEngine;

namespace Netick.Examples.SyncDir // ".SyncDirection" would overshadow the enum
{
    public class Player : NetworkBehaviour
    {
        public TextMesh textMesh;
        public Color localColor = Color.white;

        [Networked] public int health
        {
            get;
            set;
        }
     
        [Networked(size: 5)]
        public readonly NetworkArray<int> list = new NetworkArray<int>(5) { 55, 66, 77 };

        // public override void OnStartLocalPlayer()
        // {
        //     textMesh.color = localColor;
        // }

        public override void NetworkStart()
        {
            if (IsInputSource)
            {
                textMesh.color = localColor;
            }
        }

        public override void NetworkUpdate()
        {
            // show health and list for everyone
            textMesh.text = $"{health} / {list.Length}";

            // key presses increase health / list for local player.
            // note that trusting the client is a bad idea, especially with health.
            // SyncDirection is usually used for movement.
            //
            // when using custom OnSerialize, the custom OnDeserialize can still
            // safely validate client data (check position, velocity etc.).
            // this is why it's named SyncDirection, and not ClientAuthority.
            // because the server can still validate the client's data first.
            //
            // try to change SyncDirection to ServerToClient in the editor.
            // then restart the game, clients won't be allowed to change their
            // own health anymore.
            if (IsServer)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                    ++health;

                if (Input.GetKeyDown(KeyCode.L))
                    list.Add(list.Length);
            }
        }
#if !UNITY_SERVER
        // show instructions
        void OnGUI()
        {
            if (!IsInputSource) return;

            int width = 250;
            int height = 50;
            GUI.color = localColor;
            GUI.Label(
                new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - height / 2, width, height),
                "Press Space to increase your own health!\nPress L to add to your SyncList!");
        }
#endif
    }
}

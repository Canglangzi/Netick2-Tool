
using Netick.Unity;
using UnityEngine;

namespace CocKleBurs.GameFrameWork.LagCompensation
{
    public class GameLagCompensation : NetworkBehaviour
    {
        public override void NetworkStart()
        {
            LagCompensationManager.ServerSetup();

        }

        public override void NetworkFixedUpdate()
        {
            if (IsServer)
            {
                LagCompensationManager.ServerUpdate();
            }
        }

        public override void NetworkDestroy()
        {
            LagCompensationManager.ServerShutdown();
        }

    }
}

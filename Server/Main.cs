using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandingZone.Server
{
    public class Main : BaseScript
    {

        [EventHandler("playerDropped")]
        public void KillPlayer([FromSource] Player player, string reason)
        {
            RemoveLandingZone(player);
        }

        [EventHandler("landingzone:add")]
        private void AddLandingZone([FromSource] Player player, Vector3 pos, Vector3 size)
        {
            TriggerClientEvent("landingzone:addZone", player.Handle, pos, size);
        }

        [EventHandler("landingzone:remove")]
        private void RemoveLandingZone([FromSource] Player player)
        {
            TriggerClientEvent("landingzone:removeZone", player.Handle);
        }
    }
}

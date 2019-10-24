using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LandingZone
{
    public class Main : BaseScript
    {
        protected bool isSetting, isSet, drewBlip = false;
        protected Vector3 size = new Vector3(20f, 20f, 0f);
        protected int setTime, blip = 0;
        protected Dictionary<int, LandingZone> landingzones = new Dictionary<int, LandingZone>();

        [Command("setlandingzone")]
        private void SetLandingZone()
        {
            if (!isSet)
            {
                if (!Game.PlayerPed.IsSittingInVehicle())
                {
                    isSetting = !isSetting;
                    size = new Vector3(20f, 20f, 0f);
                }
                else
                {
                    Screen.ShowNotification("~r~You must be on foot to set a landing zone!");
                }
            }
            else
            {
                Screen.ShowNotification("~r~Your old landing zone is still set!");
            }
        }

        [Command("clearlandingzone")]
        private void ClearLandingZone()
        {
            if (isSet)
            {
                isSet = false;
                TriggerServerEvent("landingzone:remove");
            }
        }

        [EventHandler("landingzone:addZone")]
        private void AddZone(int h, Vector3 pos, Vector3 s)
        {
            LandingZone zone = new LandingZone(pos, s, h);
            landingzones.Add(Game.Player.ServerId, zone);
        }

        [EventHandler("landingzone:removeZone")]
        private void RemoveZone(int handle)
        {
            Screen.ShowNotification("Landing zone deleted.");
            if (Game.Player.ServerId == handle)
            {
                isSet = false;
            }

            if (landingzones.ContainsKey(handle))
            {
                landingzones.Remove(handle);
            }

        }

        [Tick]
        private async Task DrawFlares()
        {
            if (isSet && setTime > 0 && (Game.GameTime - setTime) > 300000)
            {
                isSet = false;
                setTime = 0;
                TriggerServerEvent("landingzone:remove");
            }

            if (landingzones.Count > 0)
            {
                foreach (KeyValuePair<int, LandingZone> item in landingzones)
                {
                    Player p = new Player(API.GetPlayerFromServerId(item.Key));
                    if (p != null)
                    {
                        API.DrawMarker((int)MarkerType.HorizontalCircleSkinny, item.Value.origin.X, item.Value.origin.Y, item.Value.origin.Z, 0f, 0f, 0f, 0f, 180, 0f, item.Value.size.X, item.Value.size.Y, item.Value.size.Z, 255, 0, 0, 50, false, false, 2, false, null, null, false);
                        if (Game.PlayerPed.CurrentVehicle != null && Game.PlayerPed.CurrentVehicle.ClassType == VehicleClass.Helicopters && API.GetDistanceBetweenCoords(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, item.Value.origin.X, item.Value.origin.Y, item.Value.origin.Z, true) > 50)
                        {
                            if (!drewBlip)
                            {
                                blip = API.AddBlipForCoord(item.Value.origin.X, item.Value.origin.Y, item.Value.origin.Z);
                                API.SetBlipSprite(blip, 80);
                                API.SetBlipColour(blip, 6);
                                API.BeginTextCommandSetBlipName("STRING");
                                API.AddTextComponentSubstringPlayerName(String.Format("Landing zone by " + p.Name));
                                API.EndTextCommandSetBlipName(blip);
                                drewBlip = true;
                            }
                        }
                        else if (blip != 0)
                        {
                            API.RemoveBlip(ref blip);
                            blip = 0;
                            drewBlip = false;
                        }
                    }
                }
            }
            await Task.FromResult(0);
        }

        [Tick]
        private async Task SetZones()
        {
            if (isSetting)
            {
                if (Game.PlayerPed.IsSittingInVehicle())
                {
                    Screen.ShowNotification("Cancelled the setting of a landing zone due to being in a vehicle.");
                    return;
                }

                Game.DisableControlThisFrame(0, Control.SelectWeapon);
                Game.DisableControlThisFrame(0, Control.Attack);
                Screen.DisplayHelpTextThisFrame("Press ~INPUT_ATTACK~ to set the landing zone.\nUse ~INPUT_WEAPON_WHEEL_NEXT~ to change the size.\nOr type ~y~/setlandingzone ~w~to cancel.");
                Vector3 pos = Game.PlayerPed.Position;
                pos.Z -= 0.8f;
                API.DrawMarker((int)MarkerType.HorizontalSplitArrowCircle, pos.X, pos.Y, pos.Z, 0f, 0f, 0f, 0f, 180, 0f, size.X, size.Y, size.Z, 255, 128, 0, 255, false, false, 2, false, null, null, false);

                if (Game.IsControlJustReleased(0, Control.Attack))
                {
                    isSetting = false;
                    isSet = true;
                    setTime = Game.GameTime;
                    TriggerServerEvent("landingzone:add", pos, size);
                    Screen.ShowNotification("Landing zone has been set! It will automatically clear in 5 minutes, or you can clear it with ~y~/clearlandingzone~w~.");
                }

                if (Game.IsControlPressed(0, Control.WeaponWheelNext))
                {
                    // Keep the shape of the circle by doing both X and Y
                    size.Y += 1f;
                    size.X += 1f;
                }

                if (Game.IsControlPressed(0, Control.WeaponWheelPrev))
                {
                    // For even small helicopters, 20f gives them enough room to hit the landing zone
                    if (size.X > 20f)
                    {
                        size.Y -= 1f;
                        size.X -= 1f;
                    }
                    else
                    {
                        Screen.ShowNotification("A landing zone can't be any smaller!", false);
                    }
                }
            }
            await Task.FromResult(0);
        }
    }

    public class LandingZone
    {
        public Vector3 origin;
        public Vector3 size;
        public int player;

        public LandingZone(Vector3 p, Vector3 s, int h)
        {
            this.origin = p;
            this.size = s;
            this.player = h;
        }
    }
}

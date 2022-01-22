using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Events;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Users.Events;
using SDG.Unturned;
using Steamworks;
using UnityEngine;


[assembly: PluginMetadata("BeltSystem", DisplayName = "BeltSystem", Description = "Add a belt system to the game Unturned.")]

namespace OpenMod_Belt_System
{
    public class BeltSystem : OpenModUnturnedPlugin
    {
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ILogger<BeltSystem> m_Logger;
        public List<CSteamID> BeltPlayerList;
        public List<CSteamID> VehiclePlayerList;

        public BeltSystem(
            IConfiguration configuration,
            IStringLocalizer stringLocalizer,
            ILogger<BeltSystem> logger,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
           this.m_Configuration = configuration;
           this.m_StringLocalizer = stringLocalizer;
           this.m_Logger = logger;
           this.BeltPlayerList = new List<CSteamID>();
           this.VehiclePlayerList = new List<CSteamID>();
        }
        
        protected override async UniTask OnLoadAsync()
        {
            VehicleManager.onEnterVehicleRequested += PlayerEnterToCar;
            VehicleManager.onExitVehicleRequested += PlayerExitToCar;
            PlayerInput.onPluginKeyTick += PluginKeyTick;
            VehicleManager.onDamageVehicleRequested += DamageVehicle;
            m_Logger.LogInformation("Nonantiy belt system loaded");
        }

        private void DamageVehicle(CSteamID instigatorsteamid, InteractableVehicle vehicle, ref ushort pendingtotaldamage, ref bool canrepair, ref bool shouldallow, EDamageOrigin damageorigin)
        {
            Player player = PlayerTool.getPlayer(instigatorsteamid);
            if (damageorigin == EDamageOrigin.Vehicle_Collision_Self_Damage)
            {
                var playerdamage = vehicle.speed / 2;
                var ePlayerKill = EPlayerKill.NONE;
                player.life.askDamage(Convert.ToByte(playerdamage), player.transform.forward, EDeathCause.VEHICLE, ELimb.SKULL, player.channel.owner.lobbyID, out ePlayerKill);
            }
        }


        private void PlayerExitToCar(Player player, InteractableVehicle vehicle, ref bool shouldallow, ref Vector3 pendinglocation, ref float pendingyaw)
        {
            if (!BeltPlayerList.Contains(player.channel.owner.lobbyID))
            {
                shouldallow = true;
                VehiclePlayerList.Add(player.channel.owner.lobbyID);
                EffectManager.askEffectClearByID(3532, player.channel.owner.lobbyID);
            }
            else
            {
                shouldallow = false;
            }
        }

        private void PlayerEnterToCar(Player player, InteractableVehicle vehicle, ref bool shouldallow)
        {
            shouldallow = true;
            VehiclePlayerList.Add(player.channel.owner.lobbyID);
            EffectManager.sendUIEffect(3532, 215, player.channel.owner.lobbyID, true);
            EffectManager.sendUIEffectVisibility(215, player.channel.owner.lobbyID, true, "Belt-On", false);
        }
        
        private void PluginKeyTick(Player player, uint simulation, byte key, bool state)
        {
            if (player.input.keys[9] && VehiclePlayerList.Contains(player.channel.owner.lobbyID))
            {
                if (BeltPlayerList.Contains(player.channel.owner.lobbyID))
                {
                    BeltPlayerList.Remove(player.channel.owner.lobbyID);
                    EffectManager.sendUIEffectVisibility(215, player.channel.owner.lobbyID, true, "Belt-On", false);
                    EffectManager.sendUIEffectVisibility(215, player.channel.owner.lobbyID, true, "Belt-Off", true);
                }
                else
                {
                    BeltPlayerList.Add(player.channel.owner.lobbyID);
                    EffectManager.sendUIEffectVisibility(215, player.channel.owner.lobbyID, true, "Belt-Off", false);
                    EffectManager.sendUIEffectVisibility(215, player.channel.owner.lobbyID, true, "Belt-On", true);
                }
            }
        }
        
        protected override async UniTask OnUnloadAsync()
        {
            VehicleManager.onEnterVehicleRequested -= PlayerEnterToCar;
            VehicleManager.onExitVehicleRequested -= PlayerExitToCar;
            PlayerInput.onPluginKeyTick -= PluginKeyTick;
            VehicleManager.onDamageVehicleRequested -= DamageVehicle;
            
        }
    }
}
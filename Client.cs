using FivePD.API;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Newtonsoft.Json.Linq;

namespace NV_Thermals
{
    internal class Client: Plugin
    {
        private int _googlesUpIndex;
        private int _googlesDownIndex;
        private bool _forceFPV;
        private bool _usingGoggles;

        internal Client() 
        {
            RegisterCommand("ToggleNVG", new Action(() => { ToggleVision(GoggleType.NVG); }), false);
            RegisterCommand("ToggleThermals", new Action(() => { ToggleVision(GoggleType.THERMAL); }), false);
            RegisterCommand("RemoveGoggles", new Action(() => { RemoveGoggles(); }), false);

            //Register Decors
            DecorRegister("Goggles_Equipped", 2);
            DecorRegister("Hat_Index", 3);
            DecorRegister("Hat_Texture", 3);

            //Set Player Decors
            DecorSetBool(Game.PlayerPed.Handle, "Goggles_Equipped", false);
            DecorSetInt(Game.PlayerPed.Handle, "Hat_Index", GetPedPropIndex(Game.PlayerPed.Handle, 0)); //Original index
            DecorSetInt(Game.PlayerPed.Handle, "Hat_Texture", GetPedPropTextureIndex(Game.PlayerPed.Handle, 0)); //orginal texture

            DecorRegisterLock();

            _usingGoggles = false;

            //Load Config Data
            JObject json = JObject.Parse(LoadResourceFile(GetCurrentResourceName(), "plugins/night_vision/config.json"));
            _googlesUpIndex = Int32.Parse(json["goggles-up"].ToString());
            _googlesDownIndex = Int32.Parse(json["goggles-down"].ToString());
            _forceFPV = Boolean.Parse(json["force-first-person"].ToString());

            //Add Chat suggestions to make it easier to figure out what they do
            TriggerEvent("chat:addSuggestion", "/ToggleNVG", "Equip your Night Vision Goggles and Turn them On/Off");
            TriggerEvent("chat:addSuggestion", "/ToggleThermals", "Equip your Thermal Vision Goggles and Turn them On/Off");
            TriggerEvent("chat:addSuggestion", "/RemoveGoggles", "Done with your NVGs/Thermals? Take them off and put your hat back on.");

            //Add optional keybinds
            RegisterKeyMapping("ToggleNVG", "Toggle NVGs On/Off", "KEYBOARD", "");
            RegisterKeyMapping("ToggleThermals", "Toggle Thermals On/Off", "KEYBOARD", "");
            RegisterKeyMapping("RemoveGoggles", "Take off your NVGs/Thermals", "KEYBOARD", "");
        }

        private async void EquipGoggles()
        {
            if(!DecorGetBool(Game.PlayerPed.Handle, "Goggles_Equipped"))
            {
                DecorSetInt(Game.PlayerPed.Handle, "Hat_Index", GetPedPropIndex(Game.PlayerPed.Handle, 0)); //Original index
                DecorSetInt(Game.PlayerPed.Handle, "Hat_Texture", GetPedPropTextureIndex(Game.PlayerPed.Handle, 0)); //orginal texture

                DecorSetBool(Game.PlayerPed.Handle, "Goggles_Equipped", true);

                string animDict = "missheistdockssetup1hardhat@";
                RequestAnimDict(animDict);
                while (!HasAnimDictLoaded(animDict)) { await Delay(0); }
                Game.PlayerPed.Task.PlayAnimation(animDict, "put_on_hat");
                await Delay(1750);
                SetPedPropIndex(Game.PlayerPed.Handle, 0, _googlesUpIndex, 0, false); //Add Goggles in up position

                if (_forceFPV)
                {
                    Tick += MonitorGoggleUse;
                }
            }
        }

        private async void RemoveGoggles()
        {
            if (DecorGetBool(Game.PlayerPed.Handle, "Goggles_Equipped"))
            {
                DecorSetBool(Game.PlayerPed.Handle, "Goggles_Equipped", false);

                string animDict = "anim@mp_helmets@on_foot";
                RequestAnimDict(animDict);
                while (!HasAnimDictLoaded(animDict)) { await Delay(0); }
                await Delay(500);
                Game.PlayerPed.Task.PlayAnimation(animDict, "goggles_up");
                await Delay(500);
                SetPedPropIndex(Game.PlayerPed.Handle, 0, 148, 0, true);
                await Delay(500);
                RemoveAnimDict(animDict);

                SetSeethrough(false);
                SetNightvision(false);

                ClearPedProp(Game.PlayerPed.Handle, 0);
                //Set old hat back
                SetPedPropIndex(Game.PlayerPed.Handle, 0, DecorGetInt(Game.PlayerPed.Handle, "Hat_Index"), DecorGetInt(Game.PlayerPed.Handle, "Hat_Texture"), false);

                if(_forceFPV)
                {
                    Tick -= MonitorGoggleUse;
                }
            }
        }

        private async void ToggleVision(GoggleType goggleType)
        {
            if(!DecorGetBool(Game.PlayerPed.Handle, "Goggles_Equipped")) { EquipGoggles(); await Delay(2000); }

            string animDict = "anim@mp_helmets@on_foot";
            RequestAnimDict(animDict);
            while (!HasAnimDictLoaded(animDict)) { await Delay(0); }
            await Delay(500);
            Game.PlayerPed.Task.PlayAnimation(animDict, "goggles_down");
            await Delay(500);
            SetPedPropIndex(Game.PlayerPed.Handle, 0, _googlesDownIndex, 0, false);

            if (goggleType == GoggleType.NVG)
            {
                if (GetUsingseethrough()) { SetSeethrough(false); }

                //Set NV
                SetNightvision(!GetUsingnightvision());
            }
            else if(goggleType == GoggleType.THERMAL)
            {
                if (GetUsingnightvision()) { SetNightvision(false); }

                //Set Thermals
                SetSeethrough(!GetUsingseethrough());
            }

            _usingGoggles = !_usingGoggles;
            PlaySoundFrontend(-1, "Thermal_On ", "CAR_STEAL_2_SOUNDSET", true);
        }

        private async Task MonitorGoggleUse()
        {
            if(_usingGoggles)
            {
                SetFollowPedCamViewMode(4);
            }

            await Task.FromResult(0);
        }

        public enum GoggleType
        {
            NVG = 0,
            THERMAL = 1
        }
    }
}

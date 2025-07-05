using System;
using Odium.QMPages;
using Odium.Wrappers;
using static Odium.OdiumConsole;
using Console = Il2CppSystem.Console;
using Odium.ButtonAPI.QM;
using static Odium.ButtonAPI.QM.ApiUtils;

using UnityEngine;
using VRC;
using VRC.SDKBase;
using MelonLoader;

//Audio spy by awooochy, this took me 4 hours to adapt.

namespace Odium.Components
{
    public class PlayerExtraMethods
    {
        
        // Listen method
        public static void setInfiniteVoiceRange(VRC.Player player, bool state)
        {

            //get name of currently selected player
            string targetedPlayerName = AppBot.get_selected_player_name();

            //get player by name of currently selected player
            player = GetPlayerByDisplayName(targetedPlayerName);
            
            
            try
            {
                if (state)
                {
                    Log("SetInfiniteVoiceRange: ", "You are no longer listening to  " + targetedPlayerName, LogLevel.Info);
                    Console.WriteLine("SetInfiniteVoiceRange: " + "You are no longer listening to  " + targetedPlayerName);
                    //set audio value to delault
                    player.field_Private_VRCPlayerApi_0.SetVoiceDistanceFar(25f);
                    return;
                }
                
                Log("SetInfiniteVoiceRange: ", "Listening to  " + targetedPlayerName, LogLevel.Info);
                Console.WriteLine("SetInfiniteVoiceRange: " + "Listening to  " + targetedPlayerName);
                //set audio to infinite range to hear them from anywhere
                player.field_Private_VRCPlayerApi_0.SetVoiceDistanceFar(float.PositiveInfinity);
            }
            catch (Exception e)
            {
                Log("PlayerExtraMethods: ", "SetInfiniteVoiceRange shat itself.", LogLevel.Warning);
                LogException(e);
                System.Console.WriteLine("PlayerExtraMethods: " + e);
            }
            
        }
        
        // Focus Voice method
        public static void focusTargetAudio(VRC.Player targetPlayer, bool state)
        {
            float defaultVoiceGain = 0f;
            
            try
            {
                if (!state)
                {
                    defaultVoiceGain = targetPlayer.field_Private_VRCPlayerApi_0.GetVoiceGain();
                    targetPlayer.field_Private_VRCPlayerApi_0.SetVoiceDistanceFar(float.PositiveInfinity);
                    PlayerWrapper.Players.ForEach(player => { player.field_Private_VRCPlayerApi_0.SetVoiceGain(0); });
                }
                else
                {
                    targetPlayer.field_Private_VRCPlayerApi_0.SetVoiceDistanceFar(25f);
                    PlayerWrapper.Players.ForEach(player =>
                    {
                        player.field_Private_VRCPlayerApi_0.SetVoiceGain(defaultVoiceGain);
                    });
                }
            }
            catch(Exception e)
            {
                Log("PlayerExtraMethods: ", "focusTargetAudio shat itself.", LogLevel.Warning);
                LogException(e);
                System.Console.WriteLine("PlayerExtraMethods: " + e);
            }
        }


        // Teleport behind method
        
        public static void teleportBehind(VRC.Player targetPlayer)
        {
            Vector3 targetPosition = targetPlayer.transform.position;
            Vector3 targetForward = targetPlayer.transform.forward;

            float distanceBehind = 2f;
            Vector3 behindPosition = targetPosition - (targetForward * distanceBehind);

            PlayerWrapper.LocalPlayer.transform.position = behindPosition;

            Vector3 directionToTarget = (targetPosition - behindPosition).normalized;
            PlayerWrapper.LocalPlayer.transform.rotation = Quaternion.LookRotation(directionToTarget);
        }

        public static void teleportTo(VRC.Player targetPlayer)
        {
            PlayerWrapper.LocalPlayer.transform.position = targetPlayer.transform.position;
        }
        
        
        
        
        
        
        
        
        // TPose Method
        
        /*public static void TPose(bool state)
        {
            if (!state)
            {
                
            }
            else
            {
                
            }
            Animator field_Internal_Animator_ = Player.prop_Player_0._vrcplayer.field_Internal_Animator_0;
            field_Internal_Animator_.enabled = !field_Internal_Animator_.enabled;
        }*/
        
    }
    
}
using ExitGames.Client.Photon;
using HarmonyLib;
using Odium.ApplicationBot;
using Odium.UX;
using Odium.Wrappers;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;

namespace Odium.Patches
{
    [HarmonyPatch(typeof(LoadBalancingClient))]
    public class PhotonNetworkPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("Method_Public_Virtual_New_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0")]
        static bool PrefixSendEvent(byte __0, Il2CppSystem.Object __1, RaiseEventOptions __2, SendOptions __3)
        {
            if (__0 == 12 && ActionWrapper.serialize || Bot.movementMimic)
            {
                return false;
            }

            if (__0 == 1)
            {
                try
                {
                    var byteArray = __1.TryCast<Il2CppStructArray<byte>>();
                    if (byteArray != null)
                    {
                        // Parse the USpeak packet and log details
                        var parsedPacket = USpeakPacketHandler.ParseUSpeakPacket(byteArray);

                        // Original values
                        float originalGainPercent = (parsedPacket.gain / 255f) * 100f;
                        bool isMuted = (parsedPacket.flags & 0x80) != 0;
                        bool isWhispering = (parsedPacket.flags & 0x40) != 0;
                        bool isTalking = (parsedPacket.flags & 0x01) != 0;

                        // Log original packet details
                        OdiumConsole.LogGradient("Original Packet",
                            $"Timestamp: {parsedPacket.timestamp} | " +
                            $"Gain: {parsedPacket.gain} ({originalGainPercent:F1}%) | " +
                            $"Flags: 0x{parsedPacket.flags:X2} | " +
                            $"Audio Size: {parsedPacket.audioData.Length} bytes");

                        // Log original base64
                        OdiumConsole.LogGradient("Original Base64", Convert.ToBase64String(byteArray));

                        // MODIFY THE GAIN HERE
                        float newGainMultiplier = 45.0f; // 45x gain boost
                        byte newGain = (byte)Math.Min(255, parsedPacket.gain * newGainMultiplier);

                        // Create modified packet
                        var modifiedPacket = parsedPacket;
                        modifiedPacket.gain = newGain;

                        // Serialize back to bytes
                        byte[] newPacketBytes = USpeakPacketHandler.SerializePacket(modifiedPacket);

                        // Convert to Il2Cpp array
                        var newByteArray = new Il2CppStructArray<byte>(newPacketBytes.Length);
                        for (int i = 0; i < newPacketBytes.Length; i++)
                        {
                            newByteArray[i] = newPacketBytes[i];
                        }

                        // Replace the original packet with modified one
                        __1 = newByteArray.Cast<Il2CppSystem.Object>();

                        // Log modified packet details
                        float newGainPercent = (newGain / 255f) * 100f;
                        OdiumConsole.LogGradient("Modified Packet",
                            $"Timestamp: {modifiedPacket.timestamp} | " +
                            $"Gain: {newGain} ({newGainPercent:F1}%) | " +
                            $"Boost: {newGainMultiplier}x | " +
                            $"Audio Size: {modifiedPacket.audioData.Length} bytes");

                        // Log modified base64
                        OdiumConsole.LogGradient("Modified Base64", Convert.ToBase64String(newPacketBytes));

                        // Log the change summary
                        OdiumConsole.LogGradient("Gain Change",
                            $"{parsedPacket.gain} ({originalGainPercent:F1}%) → {newGain} ({newGainPercent:F1}%)");

                        // Optional: Log byte-level differences
                        if (byteArray.Length == newPacketBytes.Length)
                        {
                            List<int> changedPositions = new List<int>();
                            for (int i = 0; i < byteArray.Length; i++)
                            {
                                if (byteArray[i] != newPacketBytes[i])
                                {
                                    changedPositions.Add(i);
                                }
                            }

                            if (changedPositions.Count > 0)
                            {
                                OdiumConsole.LogGradient("Byte Changes",
                                    $"Modified {changedPositions.Count} bytes at positions: {string.Join(", ", changedPositions)}");
                            }
                        }
                    }
                    else
                    {
                        OdiumConsole.LogGradient("PhotonEvent", $"Type: {__1?.GetIl2CppType()?.Name} - Value: {__1?.ToString()}");
                    }
                }
                catch (Exception ex)
                {
                    OdiumConsole.LogGradient("PhotonEvent", $"Error modifying USpeak packet: {ex.Message}");
                }
                return true;
            }

            if (__0 != 43) return true;

            try
            {
                if (__1 != null)
                {
                    if (__1.TryCast<Il2CppSystem.Array>() != null)
                    {
                        var array = __1.TryCast<Il2CppSystem.Array>();
                        if (array.Length > 1)
                        {
                            for (global::System.Int32 i = 0; i < array.Length; i++)
                            {
                                var element = array.GetValue(i);
                                OdiumConsole.LogGradient("PhotonEvent", element?.ToString() ?? "null");
                            }
                        }
                        else
                        {
                            OdiumConsole.LogGradient("PhotonEvent", "Array too short");
                        }
                    }
                    else
                    {
                        OdiumConsole.LogGradient("PhotonEvent", __1.ToString());
                    }
                }
                else
                {
                    OdiumConsole.LogGradient("PhotonEvent", "Event data is null");
                }
            }
            catch (Exception ex)
            {
                OdiumConsole.Log("PhotonEvent", $"Error logging Photon event 43: {ex.Message}", LogLevel.Error);
            }

            return true;
        }
    }
}
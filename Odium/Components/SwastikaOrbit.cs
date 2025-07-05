﻿using System;
using System.Collections;
using System.Linq;
using MelonLoader;
using UnityEngine;
using VRC;
using VRC.SDKBase;
using Odium.Components;
using Odium.QMPages;
using static Odium.ButtonAPI.QM.ApiUtils;

namespace Odium.Components
{
    public class SwasticaOrbit
    {
        public static VRC.Player _target;
        public static bool _blind = false;
        public static bool _instance = false;
        public static bool _itemOrbit;
        public static bool _swastika;
        public static GameObject _targetItem;
        internal static Vector3 _setLocation;
        public static float _swastikaSize = 45f;
        public static float _hasTakenOwner = 1999f;
        public static float _setMultiplier = 160f;
        public static float _rotateState;
        public static Vector3 _originalVelocity;
        public static bool _returnedValue;

        public static void Activated(VRC.Player player, bool state)
        {
            //get name of currently selected player
            string targetedPlayerName = AppBot.get_selected_player_name();

            //get player by name of currently selected player
            player = GetPlayerByDisplayName(targetedPlayerName);
            
            if (state)
            {
                _target = player;
                _swastika = true;
            }
            else 
            {
                _swastika = false;
                _target = null;
            }
        }
        public static void OnUpdate()
        {
            try
            {
                bool swastika = _swastika;
                if (swastika)
                {
                    try
                    {
                        bool flag4 = _target != null;
                        if (flag4)
                        {
                            Vector3 bonePosition = _target.field_Private_VRCPlayerApi_0.GetBonePosition((HumanBodyBones)10);
                            bonePosition.Set(bonePosition.x, bonePosition.y + 2f, bonePosition.z);
                            _setLocation = bonePosition;
                        }
                        bool keyDown = Input.GetKeyDown((KeyCode)273);
                        if (keyDown)
                        {
                            _swastikaSize += 2f;
                        }
                        else
                        {
                            bool keyDown2 = Input.GetKeyDown((KeyCode)274);
                            if (keyDown2)
                            {
                                _swastikaSize -= 2f;
                            }
                        }
                        bool flag5 = _rotateState >= 360f;
                        if (flag5)
                        {
                            _rotateState = Time.deltaTime;
                        }
                        else
                        {
                            _rotateState += Time.deltaTime;
                        }
                        bool flag6 = _hasTakenOwner >= 90f;
                        if (flag6)
                        {
                            _hasTakenOwner = 0f;
                            for (int j = 0; j < OnLoadedSceneManager.sdk3Items.Length; j++)
                            {
                                VRC_Pickup vrc_Pickup2 = OnLoadedSceneManager.sdk3Items[j];
                                Networking.SetOwner(Player.prop_Player_0.field_Private_VRCPlayerApi_0, vrc_Pickup2.gameObject);
                            }
                        }
                        else
                        {
                            _hasTakenOwner += 1f;
                        }
                        float num = (float)Convert.ToInt16(OnLoadedSceneManager.sdk3Items.Length / 8);
                        float num2 = (float)OnLoadedSceneManager.sdk3Items.Length / _swastikaSize;
                        for (int k = 0; k < OnLoadedSceneManager.sdk3Items.Length; k++)
                        {
                            VRC_Pickup vrc_Pickup3 = OnLoadedSceneManager.sdk3Items[k];
                            float num3 = (float)(k % 8);
                            float num4 = (float)(k / 8);
                            float num5 = num3;
                            float num6 = num5;
                            if (num6 != 6f)
                            {
                                if (num6 != 5f)
                                {
                                    if (num6 != 4f)
                                    {
                                        if (num6 != 3f)
                                        {
                                            if (num6 != 2f)
                                            {
                                                if (num6 != 1f)
                                                {
                                                    if (num6 != 0f)
                                                    {
                                                        vrc_Pickup3.transform.position = _setLocation + new Vector3((0f - Mathf.Cos(_rotateState)) * num2 * (num4 / num), num2, Mathf.Sin(_rotateState) * num2 * (num4 / num));
                                                    }
                                                    else
                                                    {
                                                        vrc_Pickup3.transform.position = _setLocation + new Vector3(0f, num2 * (num4 / num), 0f);
                                                    }
                                                }
                                                else
                                                {
                                                    vrc_Pickup3.transform.position = _setLocation + new Vector3(0f, (0f - num2) * (num4 / num), 0f);
                                                }
                                            }
                                            else
                                            {
                                                vrc_Pickup3.transform.position = _setLocation + new Vector3((0f - Mathf.Cos(_rotateState)) * num2 * (num4 / num), 0f, Mathf.Sin(_rotateState) * num2 * (num4 / num));
                                            }
                                        }
                                        else
                                        {
                                            vrc_Pickup3.transform.position = _setLocation + new Vector3((0f - Mathf.Cos(_rotateState + _setMultiplier)) * num2 * (num4 / num), 0f, Mathf.Sin(_rotateState + _setMultiplier) * num2 * (num4 / num));
                                        }
                                    }
                                    else
                                    {
                                        vrc_Pickup3.transform.position = _setLocation + new Vector3((0f - Mathf.Cos(_rotateState + _setMultiplier)) * num2, num2 * (num4 / num), Mathf.Sin(   _rotateState + _setMultiplier) * num2);
                                    }
                                }
                                else
                                {
                                    vrc_Pickup3.transform.position = _setLocation + new Vector3((0f - Mathf.Cos(_rotateState)) * num2, (0f - num2) * (num4 / num), Mathf.Sin(_rotateState) * num2);
                                }
                            }
                            else
                            {
                                vrc_Pickup3.transform.position = _setLocation + new Vector3((0f - Mathf.Cos(_rotateState + _setMultiplier)) * num2 * (num4 / num), 0f - num2, Mathf.Sin(_rotateState + _setMultiplier) * (num2 * (num4 / num)));
                            }
                            Vector3 originalVelocity = _originalVelocity;
                            bool flag7 = false;
                            if (flag7)
                            {
                                _originalVelocity = vrc_Pickup3.GetComponent<Rigidbody>().velocity;
                            }
                            _returnedValue = false;
                            vrc_Pickup3.GetComponent<Rigidbody>().velocity = Vector3.zero;
                            vrc_Pickup3.transform.rotation = Quaternion.Euler(0f, _rotateState * -90f, 0f);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("Module : SwasticaOrbit" + ex.Message);
                    }
                }
                else
                {
                    bool returnedValue = _returnedValue;
                    if (returnedValue)
                    {
                        for (int l = 0; l < OnLoadedSceneManager.sdk3Items.Length; l++)
                        {
                            OnLoadedSceneManager.sdk3Items[l].GetComponent<Rigidbody>().velocity = _originalVelocity;
                        }
                        _returnedValue = true;
                    }
                }
            }
            catch (Exception ex2)
            {
                System.Console.WriteLine("Module : SwasticaOrbit 2" + ex2.Message);
                _itemOrbit = false;
            }
        }
    }
}
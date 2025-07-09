using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Odium.GameCheats
{
    class Murder4Utils
    {
        public static void OpenAllDoors()
        {
            foreach (var door in GameObject.FindObjectsOfType<GameObject>().Where(go => go.name.StartsWith("Door")))
                door.transform.Find("Door Anim/Hinge/Interact open")?.GetComponent<UdonBehaviour>()?.Interact();
        }

        public static void UnlockAndOpenAllDoors()
        {
            foreach (var door in GameObject.FindObjectsOfType<GameObject>().Where(go => go.name.StartsWith("Door")))
            {
                var udon = door.transform.Find("Door Anim/Hinge/Interact shove")?.GetComponent<UdonBehaviour>();
                for (int i = 0; i < 4; i++) udon?.Interact();
            }

            OpenAllDoors();
        }

        public static void CloseAllDoors()
        {
            foreach (var door in GameObject.FindObjectsOfType<GameObject>().Where(go => go.name.StartsWith("Door")))
                door.transform.Find("Door Anim/Hinge/Interact close")?.GetComponent<UdonBehaviour>()?.Interact();
            LockAllDoors();
        }

        public static void LockAllDoors()
        {
            foreach (var door in GameObject.FindObjectsOfType<GameObject>().Where(go => go.name.StartsWith("Door")))
                door.transform.Find("Door Anim/Hinge/Interact lock")?.GetComponent<UdonBehaviour>()?.Interact();
        }

        static void SendGameEvent(string eventName) => GameObject.Find("Game Logic")?.GetComponent<UdonBehaviour>()?.SendCustomNetworkEvent(0, eventName);

        public static void StartMatch() { SendGameEvent("Btn_Start"); SendGameEvent("SyncStartGame"); }
        public static void AbortMatch() => SendGameEvent("SyncAbort");
        public static void ReshowEveryoneRoles() => SendGameEvent("OnLocalPlayerAssignedRole");
        public static void BystandersWin() => SendGameEvent("SyncVictoryB");
        public static void MurderWin() => SendGameEvent("SyncVictoryM");

        static void SendWeaponEvent(string weaponPath, string eventName) => GameObject.Find(weaponPath)?.GetComponent<UdonBehaviour>()?.SendCustomNetworkEvent(0, eventName);

        public static void KillAll() => SendGameEvent("KillLocalPlayer");
        public static void BlindAll() => SendGameEvent("OnLocalPlayerBlinded");
        public static void CameraFlash() => SendGameEvent("OnLocalPlayerFlashbanged");

        static void BringWeapon(string path)
        {
            var weapon = GameObject.Find(path);
            if (!weapon) return;
            Networking.SetOwner(Networking.LocalPlayer, weapon);
            weapon.transform.position = Networking.LocalPlayer.gameObject.transform.position + Vector3.up * 0.1f;
        }

        public static void BringRevolver() => BringWeapon("Game Logic/Weapons/Revolver");
        public static void BringShotgun() => BringWeapon("Game Logic/Weapons/Unlockables/Shotgun (0)");
        public static void BringLuger() => BringWeapon("Game Logic/Weapons/Unlockables/Luger (0)");
        public static void BringSmokeGrenade() => BringWeapon("Game Logic/Weapons/Unlockables/Smoke (0)");
        public static void BringFlashCamera() => BringWeapon("Game Logic/Polaroids Unlock Camera/FlashCamera");

        public static void BringFrag(VRCPlayer player, bool shouldblow)
        {
            var frag = GameObject.Find("Game Logic/Weapons/Unlockables/Frag (0)");
            if (frag)
            {
                Networking.SetOwner(VRCPlayer.field_Internal_Static_VRCPlayer_0.field_Private_VRCPlayerApi_0, frag);
                frag.transform.position = player.transform.position + Vector3.up * 0.1f;
            }
            if (shouldblow)
            {
                Frag0Explode();
            }
        }

        public static void BringTraps()
        {
            for (int i = 0; i < 3; i++)
                BringWeapon($"Game Logic/Weapons/Bear Trap ({i})");
        }

        public static IEnumerator KnifeShieldCoroutine(VRCPlayer player)
        {
            var knives = Enumerable.Range(0, 6).Select(i => GameObject.Find($"Game Logic/Weapons/Knife ({i})")).ToList();
            var shield = new GameObject { transform = { position = player.transform.position + Vector3.up * 0.35f } };

            while (knifeShieldbool)
            {
                shield.transform.SetPositionAndRotation(player.transform.position + Vector3.up * 0.35f,
                    Quaternion.Euler(0, 360f * Time.time, 0));

                for (int i = 0; i < knives.Count; i++)
                {
                    Networking.LocalPlayer.TakeOwnership(knives[i]);
                    knives[i].transform.SetPositionAndRotation(shield.transform.position + shield.transform.forward,
                        Quaternion.LookRotation(player.transform.position - knives[i].transform.position));
                    shield.transform.Rotate(0, 360f / knives.Count, 0);
                }
                yield return null;
            }
            UnityEngine.Object.Destroy(shield);
        }

        public static void fireShotgun() => SendWeaponEvent("Game Logic/Weapons/Unlockables/Shotgun (0)", "Fire");
        public static void firerevolver() => SendWeaponEvent("Game Logic/Weapons/Revolver", "Fire");
        public static void fireLuger() => SendWeaponEvent("Game Logic/Weapons/Unlockables/Luger (0)", "Fire");
        public static void Frag0Explode() => SendWeaponEvent("Game Logic/Weapons/Unlockables/Frag (0)", "Explode");
        public static void RevolverPatronSkin() => SendWeaponEvent("Game Logic/Weapons/Revolver", "PatronSkin");
        public static void ReleaseSnake() => SendWeaponEvent("Game Logic/Snakes/SnakeDispenser", "DispenseSnake");

        public static void FindMurder()
        {
            var murdererName = Resources.FindObjectsOfTypeAll<Transform>()
                .FirstOrDefault(t => t.gameObject.name == "Murderer Name")?.gameObject;
            var text = murdererName?.GetComponent<TextMeshProUGUI>()?.text + ", Is the murder.";
        }

        public static void BeARole(string username, string role)
        {
            for (int i = 0; i < 24; i++)
            {
                if (GameObject.Find($"Game Logic/Game Canvas/Game In Progress/Player List/Player List Group/Player Entry ({i})/Player Name Text")
                    .GetComponent<TextMeshProUGUI>().text == username)
                {
                    GameObject.Find($"Player Node ({i})").GetComponent<UdonBehaviour>().SendCustomNetworkEvent(0, "SyncAssignM");
                    break;
                }
            }
        }

        public static IEnumerator InitTheme()
        {
            while (!GameObject.Find("Game Logic/Game Canvas")) yield return null;

            var setRedText = new Action<string, string>((path, text) => {
                var comp = GameObject.Find(path).GetComponent<TextMeshProUGUI>();
                comp.text = text;
                comp.color = Color.red;
            });

            setRedText("Game Logic/Game Canvas/Pregame/Title Text", "HABIBI 4");
            setRedText("Game Logic/Game Canvas/Pregame/Author Text", "By Osama");
            GameObject.Find("Game Logic/Game Canvas/Background Panel Border").GetComponent<Image>().color = Color.red;

            GameObject.Find("Game Logic/Player HUD/Death HUD Anim").SetActive(false);
            GameObject.Find("Game Logic/Player HUD/Blind HUD Anim").SetActive(false);
        }

        public static bool knifeShieldbool;
    }
}

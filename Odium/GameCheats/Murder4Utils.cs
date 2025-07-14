using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Odium.GameCheats
{
    internal static class Murder4Utils
    {
        private static bool _knifeShieldActive;

        private static void ExecuteDoorAction(string action) =>
            GameObject.FindObjectsOfType<GameObject>()
                .Where(go => go.name.StartsWith("Door"))
                .ToList().ForEach(door => door.transform
                    .Find($"Door Anim/Hinge/Interact {action}")
                    ?.GetComponent<UdonBehaviour>()?.Interact());

        public static void OpenDoors() => ExecuteDoorAction("open");
        public static void CloseDoors() { ExecuteDoorAction("close"); LockDoors(); }
        public static void LockDoors() => ExecuteDoorAction("lock");

        public static void ForceOpenDoors()
        {
            GameObject.FindObjectsOfType<GameObject>()
                .Where(go => go.name.StartsWith("Door"))
                .ToList().ForEach(door => {
                    var udon = door.transform.Find("Door Anim/Hinge/Interact shove")?.GetComponent<UdonBehaviour>();
                    for (int i = 0; i < 4; i++) udon?.Interact();
                });
            OpenDoors();
        }

        private static void SendGameEvent(string eventName) =>
            GameObject.Find("Game Logic")?.GetComponent<UdonBehaviour>()?.SendCustomNetworkEvent(0, eventName);

        private static void SendWeaponEvent(string weaponPath, string eventName) =>
            GameObject.Find(weaponPath)?.GetComponent<UdonBehaviour>()?.SendCustomNetworkEvent(0, eventName);

        private static void SendPatreonEvent(string eventName) =>
            GameObject.Find("Patreon Credits")?.GetComponent<UdonBehaviour>()?.SendCustomNetworkEvent(0, eventName);

        public static void StartGame() { SendGameEvent("Btn_Start"); SendGameEvent("SyncStartGame"); }
        public static void AbortGame() => SendGameEvent("SyncAbort");
        public static void RefreshRoles() => SendGameEvent("OnLocalPlayerAssignedRole");
        public static void TriggerBystanderWin() => SendGameEvent("SyncVictoryB");
        public static void TriggerMurdererWin() => SendGameEvent("SyncVictoryM");
        public static void ExecuteAll() => SendGameEvent("KillLocalPlayer");
        public static void BlindAll() => SendGameEvent("OnLocalPlayerBlinded");
        public static void FlashAll() => SendGameEvent("OnLocalPlayerFlashbanged");

        private static void TeleportWeapon(string path)
        {
            var weapon = GameObject.Find(path);
            if (weapon == null) return;
            Networking.SetOwner(Networking.LocalPlayer, weapon);
            weapon.transform.position = Networking.LocalPlayer.gameObject.transform.position + Vector3.up * 0.1f;
        }

        public static void GetRevolver() => TeleportWeapon("Game Logic/Weapons/Revolver");
        public static void GetShotgun() => TeleportWeapon("Game Logic/Weapons/Unlockables/Shotgun (0)");
        public static void GetLuger() => TeleportWeapon("Game Logic/Weapons/Unlockables/Luger (0)");
        public static void GetSmoke() => TeleportWeapon("Game Logic/Weapons/Unlockables/Smoke (0)");
        public static void GetCamera() => TeleportWeapon("Game Logic/Polaroids Unlock Camera/FlashCamera");
        public static void GetTraps() => Enumerable.Range(0, 3).ToList()
            .ForEach(i => TeleportWeapon($"Game Logic/Weapons/Bear Trap ({i})"));

        public static void DeployFrag(VRCPlayer target, bool detonate = false)
        {
            var frag = GameObject.Find("Game Logic/Weapons/Unlockables/Frag (0)");
            if (frag != null)
            {
                Networking.SetOwner(VRCPlayer.field_Internal_Static_VRCPlayer_0.field_Private_VRCPlayerApi_0, frag);
                frag.transform.position = target.transform.position + Vector3.up * 0.1f;
                if (detonate) DetonateFrag();
            }
        }

        public static IEnumerator CreateKnifeShield(VRCPlayer player)
        {
            var knives = Enumerable.Range(0, 6).Select(i => GameObject.Find($"Game Logic/Weapons/Knife ({i})")).ToList();
            var shield = new GameObject { transform = { position = player.transform.position + Vector3.up * 0.35f } };

            while (_knifeShieldActive)
            {
                shield.transform.SetPositionAndRotation(
                    player.transform.position + Vector3.up * 0.35f,
                    Quaternion.Euler(0, 360f * Time.time, 0));

                for (int i = 0; i < knives.Count; i++)
                {
                    Networking.LocalPlayer.TakeOwnership(knives[i]);
                    knives[i].transform.SetPositionAndRotation(
                        shield.transform.position + shield.transform.forward,
                        Quaternion.LookRotation(player.transform.position - knives[i].transform.position));
                    shield.transform.Rotate(0, 360f / knives.Count, 0);
                }
                yield return null;
            }
            UnityEngine.Object.Destroy(shield);
        }

        public static void FireShotgun() => SendWeaponEvent("Game Logic/Weapons/Unlockables/Shotgun (0)", "Fire");
        public static void FireRevolver() => SendWeaponEvent("Game Logic/Weapons/Revolver", "Fire");
        public static void FireLuger() => SendWeaponEvent("Game Logic/Weapons/Unlockables/Luger (0)", "Fire");
        public static void DetonateFrag() => SendWeaponEvent("Game Logic/Weapons/Unlockables/Frag (0)", "Explode");
        public static void ApplyRevolverSkin() => SendWeaponEvent("Game Logic/Weapons/Revolver", "PatronSkin");
        public static void SpawnSnake() => SendWeaponEvent("Game Logic/Snakes/SnakeDispenser", "DispenseSnake");

        public static void IdentifyMurderer()
        {
            var murdererObj = Resources.FindObjectsOfTypeAll<Transform>()
                .FirstOrDefault(t => t.gameObject.name == "Murderer Name")?.gameObject;
            var murdererText = murdererObj?.GetComponent<TextMeshProUGUI>()?.text + ", Is the murder.";
        }

        public static void ExplodeAtTarget(VRC.Player target)
        {
            var frag = GameObject.Find("Frag (0)");
            Networking.LocalPlayer.TakeOwnership(frag);
            frag.transform.position = target.transform.position;
            frag.GetComponent<UdonBehaviour>().SendCustomNetworkEvent(0, "Explode");
        }

        public static void BlindTarget(VRC.Player target) => SendTargetedEvent(target, "SyncFlashbang");

        public static void AssignRole(string username, string role)
        {
            for (int i = 0; i < 24; i++)
            {
                var playerName = GameObject.Find($"Game Logic/Game Canvas/Game In Progress/Player List/Player List Group/Player Entry ({i})/Player Name Text")
                    .GetComponent<TextMeshProUGUI>().text;

                if (playerName == username)
                {
                    GameObject.Find($"Player Node ({i})")
                        .GetComponent<UdonBehaviour>()
                        .SendCustomNetworkEvent(0, "SyncAssignM");
                    break;
                }
            }
        }

        public static IEnumerator InitializeTheme()
        {
            while (!GameObject.Find("Game Logic/Game Canvas")) yield return null;

            void SetRedText(string path, string text)
            {
                var comp = GameObject.Find(path).GetComponent<TextMeshProUGUI>();
                comp.text = text;
                comp.color = UnityEngine.Color.red;
            }

            SetRedText("Game Logic/Game Canvas/Pregame/Title Text", "HABIBI 4");
            SetRedText("Game Logic/Game Canvas/Pregame/Author Text", "By Osama");
            GameObject.Find("Game Logic/Game Canvas/Background Panel Border")
                .GetComponent<UnityEngine.UI.Image>().color = UnityEngine.Color.red;
            GameObject.Find("Game Logic/Player HUD/Death HUD Anim").SetActive(false);
            GameObject.Find("Game Logic/Player HUD/Blind HUD Anim").SetActive(false);
        }

        public static void SendTargetedPatreonEvent(VRC.Player target, string eventName)
        {
            var credits = GameObject.Find("Patreon Credits");
            credits.GetComponent<UdonBehaviour>().enabled = true;
            UdonExtensions.SendUdon(credits, eventName, target);
            credits.GetComponent<UdonBehaviour>().enabled = false;
        }

        public static void SendTargetedEvent(VRC.Player target, string eventName)
        {
            var gameLogic = GameObject.Find("Game Logic");
            UdonExtensions.SendUdon(gameLogic, eventName, target);
        }

        public static bool KnifeShieldActive
        {
            get => _knifeShieldActive;
            set => _knifeShieldActive = value;
        }
    }
}
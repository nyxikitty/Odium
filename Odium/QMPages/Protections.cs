using Odium.ButtonAPI.QM;
using Odium.Components;
using Odium.Odium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Odium.QMPages
{
    class Protections
    {
        public static void InitializePage(QMNestedMenu gameHacks, Sprite buttonImage)
        {
            Sprite M4Icon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Udon.png");
            Sprite SpeakerIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Speaker.png");
            Sprite PunIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\Pun.png");
            Sprite PeopleIcon = SpriteUtil.LoadFromDisk(Environment.CurrentDirectory + "\\Odium\\People.png");

            QMNestedMenu murder4Protections = new QMNestedMenu(gameHacks, 1, 0, "<color=#8d142b>Udon</color>", "<color=#8d142b>Udon</color>", "Opens Select User menu", false, M4Icon, buttonImage);
            QMNestedMenu uspeakProtection = new QMNestedMenu(gameHacks, 2, 0, "<color=#8d142b>USpeak</color>", "<color=#8d142b>USpeak</color>", "Opens Select User menu", false, SpeakerIcon, buttonImage);
            QMNestedMenu photonProtection = new QMNestedMenu(gameHacks, 3, 0, "<color=#8d142b>Photon</color>", "<color=#8d142b>Photon</color>", "Opens Select User menu", false, PunIcon, buttonImage);
            QMNestedMenu avatarProtection = new QMNestedMenu(gameHacks, 4, 0, "<color=#8d142b>Avatars</color>", "<color=#8d142b>Avatars</color>", "Opens Select User menu", false, PeopleIcon, buttonImage);

            new QMToggleButton(murder4Protections, 1f, 0, "Prevent Patreon Crash", delegate
            {
                AssignedVariables.preventPatreonCrash = true;
            }, delegate
            {
                AssignedVariables.preventPatreonCrash = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(murder4Protections, 2f, 0, "Prevent Event Flooding", delegate
            {
                AssignedVariables.preventM4EventFlooding = true;
            }, delegate
            {
                AssignedVariables.preventM4EventFlooding = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(murder4Protections, 3f, 0, "Prevent Event Spam", delegate
            {
                AssignedVariables.preventM4EventSpam = true;
            }, delegate
            {
                AssignedVariables.preventM4EventSpam = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(murder4Protections, 4f, 0, "Ratelimit Events", delegate
            {
                AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);


            new QMToggleButton(uspeakProtection, 1f, 0, "Gain Check", delegate
            {
                // AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                // AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(uspeakProtection, 2f, 0, "Block Common Packets", delegate
            {
                // AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                // AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(uspeakProtection, 3f, 0, "Spam Prevention", delegate
            {
                // AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                // AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(photonProtection, 1f, 0, "Ratelimit Common Events", delegate
            {
                // AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                // AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(photonProtection, 2f, 0, "Advanced Evt Check", delegate
            {
                AssignedVariables.chatBoxAntis = true;
            }, delegate
            {
                AssignedVariables.chatBoxAntis = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(photonProtection, 3f, 0, "Anti Pen-Trail Crash", delegate
            {
                // AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                // AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(photonProtection, 4f, 0, "Check Interest", delegate
            {
                // AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                // AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(avatarProtection, 1f, 0, "Check Shaders", delegate
            {
                // AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                // AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(avatarProtection, 2f, 0, "Check Materials", delegate
            {
                // AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                // AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(avatarProtection, 3f, 0, "Check Pollygons", delegate
            {
                // AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                // AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(avatarProtection, 4f, 0, "Check Lights", delegate
            {
                // AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                // AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(avatarProtection, 1f, 1, "Check Physbones", delegate
            {
                // AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                // AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(avatarProtection, 2f, 1, "Prevent CABs", delegate
            {
                // AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                // AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);

            new QMToggleButton(avatarProtection, 3f, 1, "Block Known Crashers", delegate
            {
                // AssignedVariables.ratelimitM4Events = true;
            }, delegate
            {
                // AssignedVariables.ratelimitM4Events = false;
            }, "Prevent common crash exploits in Murder 4", true, buttonImage);
        }
    }
}

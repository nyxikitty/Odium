using ExitGames.Client.Photon;
using Odium.Components;
using System;
using System.Collections;
using UnityEngine;

// This honestly may not be correct at all, the only thing that works is the gain
public class USpeakPacketHandler
{
    public float gain = 1.0f;
    public float volume = 1.0f;
    public bool muted = false;
    public bool whispering = false;

    public struct USpeakVoicePacket
    {
        public uint timestamp;
        public byte gain;
        public byte flags;
        public byte reserved1;
        public byte reserved2;
        public byte[] audioData;

        public USpeakVoicePacket(uint timestamp, byte gain, byte flags, byte[] audioData)
        {
            this.timestamp = timestamp;
            this.gain = gain;
            this.flags = flags;
            this.reserved1 = 0x2D;
            this.reserved2 = 0x00;
            this.audioData = audioData ?? new byte[0];
        }
    }

    public static byte[] CreateUSpeakPacket(float gainValue, bool isMuted, bool isWhispering, byte[] audioData)
    {
        uint timestamp = (uint)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() & 0xFFFFFFFF);

        byte gainByte = (byte)Mathf.Clamp(gainValue * 127.5f, 0, 255);

        byte flags = 0x37;
        if (isMuted) flags |= 0x80;
        if (isWhispering) flags |= 0x40;

        var packet = new USpeakVoicePacket(timestamp, gainByte, flags, audioData);
        return SerializePacket(packet);
    }

    public static byte[] SerializePacket(USpeakVoicePacket packet)
    {
        byte[] result = new byte[8 + packet.audioData.Length];

        result[0] = (byte)(packet.timestamp & 0xFF);
        result[1] = (byte)((packet.timestamp >> 8) & 0xFF);
        result[2] = (byte)((packet.timestamp >> 16) & 0xFF);
        result[3] = (byte)((packet.timestamp >> 24) & 0xFF);

        result[4] = packet.gain;
        result[5] = packet.flags;
        result[6] = packet.reserved1;
        result[7] = packet.reserved2;

        if (packet.audioData.Length > 0)
        {
            Array.Copy(packet.audioData, 0, result, 8, packet.audioData.Length);
        }

        return result;
    }

    public static USpeakVoicePacket ParseUSpeakPacket(byte[] packetData)
    {
        if (packetData.Length < 8)
            throw new ArgumentException("USpeak packet too short");

        uint timestamp = (uint)(packetData[0] | (packetData[1] << 8) | (packetData[2] << 16) | (packetData[3] << 24));
        byte gain = packetData[4];
        byte flags = packetData[5];

        byte[] audioData = new byte[packetData.Length - 8];
        if (audioData.Length > 0)
        {
            Array.Copy(packetData, 8, audioData, 0, audioData.Length);
        }

        return new USpeakVoicePacket(timestamp, gain, flags, audioData);
    }

    public void SendCustomVoicePacket(byte[] audioData)
    {
        byte[] packet = CreateUSpeakPacket(gain, muted, whispering, audioData);
        string base64Packet = Convert.ToBase64String(packet);

        RaiseEventOptions options = new RaiseEventOptions
        {
            field_Public_EventCaching_0 = 0,
            field_Public_ReceiverGroup_0 = 0
        };

        PhotonExtensions.OpRaiseEvent(1, packet, options, default(SendOptions));
    }

    public static byte[] ModifyUSpeakPacket(string base64Packet, float newGain, bool newMuted)
    {
        byte[] originalPacket = Convert.FromBase64String(base64Packet);
        var parsed = ParseUSpeakPacket(originalPacket);

        parsed.gain = (byte)Mathf.Clamp(newGain * 127.5f, 0, 255);

        if (newMuted)
            parsed.flags |= 0x80;
        else
            parsed.flags &= 0x7F;

        return SerializePacket(parsed);
    }

    public static byte[] ApplyAudioEffects(byte[] audioData, float gainMultiplier, bool distortion = false)
    {
        byte[] processed = new byte[audioData.Length];

        for (int i = 0; i < audioData.Length; i++)
        {
            int sample = audioData[i] - 128;

            sample = (int)(sample * gainMultiplier);

            if (distortion)
            {
                sample = sample > 0 ? Mathf.Min(sample * 2, 127) : Mathf.Max(sample * 2, -128);
            }

            processed[i] = (byte)(Mathf.Clamp(sample + 128, 0, 255));
        }

        return processed;
    }
}

public class USpeakVoiceManager : MonoBehaviour
{
    [System.Serializable]
    public class VoiceSettings
    {
        public float gain = 1.0f;
        public float pitch = 1.0f;
        public bool muted = false;
        public bool robotVoice = false;
        public bool whisper = false;
    }

    public VoiceSettings voiceSettings = new VoiceSettings();

    public void ProcessIncomingVoicePacket(string base64Packet)
    {
        try
        {
            byte[] packetData = Convert.FromBase64String(base64Packet);
            var packet = USpeakPacketHandler.ParseUSpeakPacket(packetData);

            if (voiceSettings.robotVoice)
            {
                packet.audioData = USpeakPacketHandler.ApplyAudioEffects(packet.audioData, voiceSettings.gain, true);
            }

            byte[] modifiedPacket = USpeakPacketHandler.SerializePacket(packet);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to process voice packet: {e.Message}");
        }
    }
}
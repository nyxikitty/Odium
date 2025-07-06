using ExitGames.Client.Photon;
using Odium.Components;
using System;
using System.Collections;
using UnityEngine;

// USpeak Voice Packet Structure and Manipulation
public class USpeakPacketHandler
{
    public float gain = 1.0f;
    public float volume = 1.0f;
    public bool muted = false;
    public bool whispering = false;

    // USpeak packet structure (based on analysis)
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
            this.reserved1 = 0x2D; // Common value seen in packets
            this.reserved2 = 0x00;
            this.audioData = audioData ?? new byte[0];
        }
    }

    // Create custom USpeak voice packet
    public static byte[] CreateUSpeakPacket(float gainValue, bool isMuted, bool isWhispering, byte[] audioData)
    {
        uint timestamp = (uint)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() & 0xFFFFFFFF);

        // Convert gain (0.0-2.0) to byte (0-255)
        byte gainByte = (byte)Mathf.Clamp(gainValue * 127.5f, 0, 255);

        // Create flags byte
        byte flags = 0x37; // Base flags
        if (isMuted) flags |= 0x80;
        if (isWhispering) flags |= 0x40;

        var packet = new USpeakVoicePacket(timestamp, gainByte, flags, audioData);
        return SerializePacket(packet);
    }

    // Serialize packet to byte array
    public static byte[] SerializePacket(USpeakVoicePacket packet)
    {
        byte[] result = new byte[8 + packet.audioData.Length];

        // Timestamp (little-endian)
        result[0] = (byte)(packet.timestamp & 0xFF);
        result[1] = (byte)((packet.timestamp >> 8) & 0xFF);
        result[2] = (byte)((packet.timestamp >> 16) & 0xFF);
        result[3] = (byte)((packet.timestamp >> 24) & 0xFF);

        // Gain and flags
        result[4] = packet.gain;
        result[5] = packet.flags;
        result[6] = packet.reserved1;
        result[7] = packet.reserved2;

        // Audio data
        if (packet.audioData.Length > 0)
        {
            Array.Copy(packet.audioData, 0, result, 8, packet.audioData.Length);
        }

        return result;
    }

    // Parse USpeak packet
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

    // Send custom USpeak packet via Photon
    public void SendCustomVoicePacket(byte[] audioData)
    {
        byte[] packet = CreateUSpeakPacket(gain, muted, whispering, audioData);
        string base64Packet = Convert.ToBase64String(packet);

        // Send via Photon (modify event ID as needed)
        RaiseEventOptions options = new RaiseEventOptions
        {
            field_Public_EventCaching_0 = 0,
            field_Public_ReceiverGroup_0 = 0
        };

        PhotonExtensions.OpRaiseEvent(1, packet, options, default(SendOptions));
    }

    // Modify existing USpeak packet
    public static byte[] ModifyUSpeakPacket(string base64Packet, float newGain, bool newMuted)
    {
        byte[] originalPacket = Convert.FromBase64String(base64Packet);
        var parsed = ParseUSpeakPacket(originalPacket);

        // Update gain
        parsed.gain = (byte)Mathf.Clamp(newGain * 127.5f, 0, 255);

        // Update mute flag
        if (newMuted)
            parsed.flags |= 0x80;
        else
            parsed.flags &= 0x7F;

        return SerializePacket(parsed);
    }

    // Apply audio effects to USpeak data
    public static byte[] ApplyAudioEffects(byte[] audioData, float gainMultiplier, bool distortion = false)
    {
        byte[] processed = new byte[audioData.Length];

        for (int i = 0; i < audioData.Length; i++)
        {
            // Convert to signed value
            int sample = audioData[i] - 128;

            // Apply gain
            sample = (int)(sample * gainMultiplier);

            // Apply distortion effect
            if (distortion)
            {
                sample = sample > 0 ? Mathf.Min(sample * 2, 127) : Mathf.Max(sample * 2, -128);
            }

            // Clamp and convert back
            processed[i] = (byte)(Mathf.Clamp(sample + 128, 0, 255));
        }

        return processed;
    }

    // Example usage
    public void ExampleUsage()
    {
        // Parse the packet you provided
        string originalPacket = "j1ncdGQ3LQB4hQcEhgWejBkjdAJEqWOkVL3Q4MbgX4cvigg4pw3em0AxAKaVsyct5BabnNg=";
        var parsed = ParseUSpeakPacket(Convert.FromBase64String(originalPacket));

        Debug.Log($"Original - Timestamp: {parsed.timestamp}, Gain: {parsed.gain}, Flags: 0x{parsed.flags:X2}");

        // Create new packet with different settings
        byte[] newPacket = CreateUSpeakPacket(1.5f, false, true, parsed.audioData);
        string newBase64 = Convert.ToBase64String(newPacket);

        Debug.Log($"New packet: {newBase64}");

        // Send the modified packet
        SendCustomVoicePacket(parsed.audioData);
    }
}

// USpeak Voice Manager for handling multiple voice streams
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

    // Process incoming USpeak packets
    public void ProcessIncomingVoicePacket(string base64Packet)
    {
        try
        {
            byte[] packetData = Convert.FromBase64String(base64Packet);
            var packet = USpeakPacketHandler.ParseUSpeakPacket(packetData);

            // Apply voice effects
            if (voiceSettings.robotVoice)
            {
                packet.audioData = USpeakPacketHandler.ApplyAudioEffects(packet.audioData, voiceSettings.gain, true);
            }

            // Re-encode and forward to USpeak
            byte[] modifiedPacket = USpeakPacketHandler.SerializePacket(packet);
            // Forward to USpeak system...
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to process voice packet: {e.Message}");
        }
    }
}
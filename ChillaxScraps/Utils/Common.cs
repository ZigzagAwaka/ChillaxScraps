﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.Utils
{
    public class Scrap
    {
        public string asset;
        public int rarity;
        public int behaviourId;

        public Scrap(string asset, int rarity, int behaviourId = 0)
        {
            this.asset = asset;
            this.rarity = rarity;
            this.behaviourId = behaviourId;
        }
    }

    public class NetworkReference
    {
        public NetworkObjectReference netObjectRef;
        public int value;

        public NetworkReference(NetworkObjectReference netObjectRef, int value)
        {
            this.netObjectRef = netObjectRef;
            this.value = value;
        }
    }

    public class SetupScript
    {
        public static void Network()
        {
            IEnumerable<System.Type> types;
            try
            {
                types = Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null);
            }
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }

        public static void Copy(NoisemakerProp target, Item item)
        {
            var source = item.spawnPrefab.GetComponent<NoisemakerProp>();
            target.useCooldown = source.useCooldown;
            target.noiseAudio = source.noiseAudio;
            target.noiseAudioFar = source.noiseAudioFar;
            target.noiseSFX = new AudioClip[source.noiseSFX.Length];
            for (int i = 0; i < source.noiseSFX.Length; i++)
            {
                target.noiseSFX[i] = source.noiseSFX[i];
            }
            target.noiseSFXFar = new AudioClip[source.noiseSFXFar.Length];
            for (int i = 0; i < source.noiseSFXFar.Length; i++)
            {
                target.noiseSFXFar[i] = source.noiseSFXFar[i];
            }
            target.noiseRange = source.noiseRange;
            target.maxLoudness = source.maxLoudness;
            target.minLoudness = source.minLoudness;
            target.minPitch = source.minPitch;
            target.maxPitch = source.maxPitch;
            target.triggerAnimator = source.triggerAnimator;
            Object.Destroy(source);
        }

        public static void Copy(Shovel target, Item item)
        {
            var source = item.spawnPrefab.GetComponent<Shovel>();
            target.shovelHitForce = source.shovelHitForce;
            target.reelUp = source.reelUp;
            target.swing = source.swing;
            target.hitSFX = new AudioClip[source.hitSFX.Length];
            for (int i = 0; i < source.hitSFX.Length; i++)
            {
                target.hitSFX[i] = source.hitSFX[i];
            }
            target.shovelAudio = source.shovelAudio;
            Object.Destroy(source);
        }

        public static void Copy(SoccerBallProp target, Item item)
        {
            var source = item.spawnPrefab.GetComponent<SoccerBallProp>();
            target.ballHitUpwardAmount = source.ballHitUpwardAmount;
            target.grenadeFallCurve = source.grenadeFallCurve;
            target.grenadeVerticalFallCurve = source.grenadeVerticalFallCurve;
            target.soccerBallVerticalOffset = source.soccerBallVerticalOffset;
            target.grenadeVerticalFallCurveNoBounce = source.grenadeVerticalFallCurveNoBounce;
            target.hitBallSFX = new AudioClip[source.hitBallSFX.Length];
            for (int i = 0; i < source.hitBallSFX.Length; i++)
            {
                target.hitBallSFX[i] = source.hitBallSFX[i];
            }
            target.ballHitFloorSFX = new AudioClip[source.ballHitFloorSFX.Length];
            for (int i = 0; i < source.ballHitFloorSFX.Length; i++)
            {
                target.ballHitFloorSFX[i] = source.ballHitFloorSFX[i];
            }
            target.soccerBallAudio = source.soccerBallAudio;
            target.ballCollider = source.ballCollider;
            var collider = item.spawnPrefab.GetComponentInChildren<GrabbableObjectPhysicsTrigger>();
            collider.itemScript = target;
            Object.Destroy(source);
        }

        public static void Copy(WalkieTalkie target, Item item)
        {
            var source = item.spawnPrefab.GetComponent<WalkieTalkie>();
            target.insertedBattery = source.insertedBattery;
            target.insertedBattery.charge = source.insertedBattery.charge;
            target.mainObjectRenderer = source.mainObjectRenderer;
            target.thisAudio = source.thisAudio;
            target.stopTransmissionSFX = new AudioClip[source.stopTransmissionSFX.Length];
            for (int i = 0; i < source.stopTransmissionSFX.Length; i++)
            {
                target.stopTransmissionSFX[i] = source.stopTransmissionSFX[i];
            }
            target.startTransmissionSFX = new AudioClip[source.startTransmissionSFX.Length];
            for (int i = 0; i < source.startTransmissionSFX.Length; i++)
            {
                target.startTransmissionSFX[i] = source.startTransmissionSFX[i];
            }
            target.switchWalkieTalkiePowerOff = source.switchWalkieTalkiePowerOff;
            target.switchWalkieTalkiePowerOn = source.switchWalkieTalkiePowerOn;
            target.talkingOnWalkieTalkieNotHeldSFX = source.talkingOnWalkieTalkieNotHeldSFX;
            target.playerDieOnWalkieTalkieSFX = source.playerDieOnWalkieTalkieSFX;
            target.onMaterial = source.onMaterial;
            target.offMaterial = source.offMaterial;
            target.walkieTalkieLight = source.walkieTalkieLight;
            target.target = source.target;
            target.recordingRange = source.recordingRange;
            target.maxVolume = source.maxVolume;
            target.collidersInRange = new Collider[source.collidersInRange.Length];
            target.wallAudio = source.wallAudio;
            target.wallAudios = new AudioClip[source.wallAudios.Length];
            for (int i = 0; i < source.wallAudios.Length; i++)
            {
                target.wallAudios[i] = source.wallAudios[i];
            }
            Object.Destroy(source);
        }
    }
}

﻿using GameNetcodeStuff;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ChillaxScraps.Utils
{
    internal class Effects
    {
        public enum DeathAnimation
        {
            Normal,  // classic death
            NoHead1,  // remove head from body
            Spring,  // remove head and replace it with spring
            Haunted,  // body moves a little after classic death
            Mask1,  // comedy mask attached to body
            Mask2,  // tragedy mask attached to body
            Fire,  // burned death
            CutInHalf,  // cut the body in half
            NoHead2  // same as NoHead but without sound
        }

        public static void Damage(PlayerControllerB player, int damageNb, CauseOfDeath cause = 0, int animation = 0, bool criticalBlood = true)
        {
            if (criticalBlood && player.health - damageNb <= 20)
                player.bleedingHeavily = true;
            player.DamagePlayer(damageNb, causeOfDeath: cause, deathAnimation: animation);
        }

        public static IEnumerator DamageHost(PlayerControllerB player, int damageNb, CauseOfDeath cause = 0, int animation = 0, bool criticalBlood = true)
        {
            yield return new WaitForEndOfFrame();
            Damage(player, damageNb, cause, animation, criticalBlood);
        }

        public static void Teleportation(PlayerControllerB player, Vector3 position, bool exterior = false, bool interior = false)
        {
            if (position == StartOfRound.Instance.middleOfShipNode.position)
            {
                player.isInElevator = true;
                player.isInHangarShipRoom = true;
                player.isInsideFactory = false;
            }
            if (exterior)
            {
                player.isInElevator = false;
                player.isInHangarShipRoom = false;
                player.isInsideFactory = false;
            }
            if (interior)
            {
                player.isInElevator = false;
                player.isInHangarShipRoom = false;
                player.isInsideFactory = true;
            }
            player.averageVelocity = 0f;
            player.velocityLastFrame = Vector3.zero;
            player.TeleportPlayer(position, true);
            player.beamOutParticle.Play();
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        }

        public static void Knockback(Vector3 position, float range, int damage = 0, float physicsForce = 30)
        {
            Landmine.SpawnExplosion(position, false, 0, range, damage, physicsForce);
        }

        public static void DropItem(bool destroy = false, Vector3 placingPosition = default)
        {
            GameNetworkManager.Instance.localPlayerController.DiscardHeldObject(true, placePosition: destroy ? RoundManager.Instance.insideAINodes[^1].transform.position - (Vector3.up * 100) : placingPosition);
        }

        public static void Audio(int audioID, float volume)
        {
            RoundManager.PlayRandomClip(HUDManager.Instance.UIAudio, new AudioClip[] { Plugin.audioClips[audioID] }, randomize: false, oneShotVolume: volume);
        }

        public static void Audio(int audioID, Vector3 position, float volume, bool adjust = true)
        {
            var finalPosition = position;
            if (adjust)
                finalPosition += (Vector3.up * 2);
            AudioSource.PlayClipAtPoint(Plugin.audioClips[audioID], finalPosition, volume);
        }

        public static void Audio(int audioID, Vector3 position, float volume, float pitch, bool adjust = true)
        {
            var clip = Plugin.audioClips[audioID];
            var finalPosition = position;
            if (adjust)
                finalPosition += (Vector3.up * 2);
            GameObject gameObject = new GameObject("One shot audio");
            gameObject.transform.position = finalPosition;
            AudioSource audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
            audioSource.clip = clip;
            audioSource.spatialBlend = 1f;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.Play();
            Object.Destroy(gameObject, clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        }

        public static void Audio(int[] audioIDs, Vector3 position, float volume, bool adjust = true)
        {
            var clips = audioIDs.Select(id => Plugin.audioClips[id]).ToArray();
            var finalPosition = position;
            if (adjust)
                finalPosition += (Vector3.up * 2);
            GameObject gameObject = new GameObject("One shot audio");
            gameObject.transform.position = finalPosition;
            AudioSource audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
            audioSource.spatialBlend = 1f;
            audioSource.volume = volume;
            RoundManager.PlayRandomClip(audioSource, clips, randomize: true, oneShotVolume: volume);
            Object.Destroy(gameObject, clips[^1].length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        }

        public static void Message(string title, string bottom, bool warning = false)
        {
            HUDManager.Instance.DisplayTip(title, bottom, warning);
        }
    }
}

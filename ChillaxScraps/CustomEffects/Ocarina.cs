using ChillaxScraps.Utils;
using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class Ocarina : NoisemakerProp
    {
        public int selectedSongID = 0;
        public float selectedSongLength = -1f;
        public float lastUsedTime = 0f;
        public bool isOcarinaUnique = false;
        public bool isOcarinaRestricted = true;
        public bool isPlaying = false;
        public bool isHoldingButton = false;
        public bool resetUsage = false;
        public Vector3 originalPosition = new Vector3(0.03f, 0.22f, -0.09f);
        public Vector3 originalRotation = new Vector3(180, 5, -5);
        public Vector3 animationPosition = new Vector3(-0.12f, 0.15f, 0.01f);
        public Vector3 animationRotation = new Vector3(60, 0, -50);
        private Vector3 position;
        private Vector3 rotation;
        private Coroutine? ocarinaCoroutine;
        private PlayerControllerB? previousPlayerHeldBy;
        private ParticleSystem? particleSystem;
        private Renderer? particleRenderer;
        private readonly GameObject elegyPrefab;
        private List<NetworkObjectReference> tornadoRefs = new List<NetworkObjectReference>();
        public static bool WakeOldBirdFlag = false;
        public static Vector3 WakeOldBirdPosition = Vector3.zero;

        private readonly OcarinaSong[] ocarinaSongs = new OcarinaSong[14] {
            new OcarinaSong("", new Color(0, 0, 0), null, 0, Condition.Invalid),
            new OcarinaSong("Zelda's Lullaby", new Color(0.87f, 0.36f, 1f), OcarinaSong.ZeldaLullaby, 3, Condition.IsPlayerFacingDoor, Condition.IsTimeAfternoon),
            new OcarinaSong("Epona's Song", new Color(0.94f, 0.44f, 0.01f), OcarinaSong.EponaSong, 2, Condition.IsPlayerOutsideFactory),
            new OcarinaSong("Sun's Song", new Color(1f, 0.92f, 0.1f), OcarinaSong.SunSong, 1, Condition.IsOutsideWeatherNotMajora),
            new OcarinaSong("Saria's Song", new Color(0.11f, 0.98f, 0.17f), OcarinaSong.SariaSong, 2, Condition.None),
            new OcarinaSong("Song of Time", new Color(0.18f, 0.76f, 1f), OcarinaSong.SongOfTime, 1, Condition.IsTimeNight),
            new OcarinaSong("Song of Storms", new Color(0.87f, 0.76f, 0.42f), OcarinaSong.SongOfStorms, 2, Condition.IsOutsideWeatherNotStormy, Condition.IsOutsideWeatherStormy, Condition.IsOutsideWeatherBloodMoon),
            new OcarinaSong("Song of Healing", new Color(1f, 0.22f, 0.09f), OcarinaSong.SongOfHealing, 1, Condition.None),
            new OcarinaSong("Song of Soaring", new Color(0.5f, 0.8f, 1f), OcarinaSong.SongOfSoaring, 5, Condition.IsPlayerNotInShip),
            new OcarinaSong("Sonata of Awakening", new Color(0.12f, 1f, 0.45f), OcarinaSong.SonataOfAwakening, 4, Condition.IsPlayerNearOldBirdNest),
            new OcarinaSong("Goron Lullaby", new Color(1f, 0.41f, 0.54f), OcarinaSong.GoronLullaby, 2, Condition.IsOutsideAtLeastOneBaboonSpawned, Condition.IsInsideTimeAfternoon),
            new OcarinaSong("New Wave Bossa Nova", new Color(0.03f, 0.09f, 1f), OcarinaSong.NewWaveBossaNova, 1, Condition.IsPlayerInShipSpeakerNotPlaying),
            new OcarinaSong("Elegy of Emptiness", new Color(0.85f, 0.53f, 0.33f), OcarinaSong.ElegyOfEmptiness, 3, Condition.None),
            new OcarinaSong("Oath to Order", new Color(1f, 1f, 1f), OcarinaSong.OathToOrder, 1, Condition.IsOutsideTimeNight, Condition.IsOutsideFinalHours)
        };

        public Ocarina()
        {
            position = originalPosition;
            rotation = originalRotation;
            ocarinaSongs[11].canBeUsedInOrbit = true;
            isOcarinaUnique = Plugin.config.ocarinaUniqueSongs.Value;
            isOcarinaRestricted = Plugin.config.ocarinaRestrictUsage.Value;
            elegyPrefab = Plugin.gameObjects[2];
        }

        private int GetNoiseID(int audioID)
        {
            int Id = audioID;
            if (audioID == 0)
                Id = Random.Range(0, 5);
            else
                Id += 4;
            return Id;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (isOcarinaUnique)
                selectedSongID = Random.Range(0, ocarinaSongs.Length);
            selectedSongLength = noiseSFX[GetNoiseID(selectedSongID)].length;
            particleSystem = transform.GetChild(3).GetComponent<ParticleSystem>();
            if (particleSystem != null)
                particleRenderer = particleSystem.GetComponent<Renderer>();
        }

        public override void EquipItem()
        {
            SetControlTips();
            EnableItemMeshes(enable: true);
            playerHeldBy.equippedUsableItemQE = true;
            isPocketed = false;
            if (!hasBeenHeld)
            {
                hasBeenHeld = true;
                if (!isInShipRoom && !StartOfRound.Instance.inShipPhase && StartOfRound.Instance.currentLevel.spawnEnemiesAndScrap)
                {
                    RoundManager.Instance.valueOfFoundScrapItems += scrapValue;
                }
            }
        }

        public override void SetControlTipsForItem()
        {
            SetControlTips();
        }

        private void SetControlTips()
        {
            string[] allLines = (isOcarinaUnique ? new string[2] { "Play sound : [RMB]", ocarinaSongs[selectedSongID].title } :
                new string[3] { "Play sound : [RMB]", "Select audio : [Q]", ocarinaSongs[selectedSongID].title });
            if (IsOwner)
            {
                HUDManager.Instance.ClearControlTips();
                HUDManager.Instance.ChangeControlTipMultiple(allLines, holdingItem: true, itemProperties);
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (playerHeldBy == null)
                return;
            isHoldingButton = buttonDown;
            if (!isPlaying && buttonDown)
            {
                isPlaying = true;
                previousPlayerHeldBy = playerHeldBy;
                if (ocarinaCoroutine != null)
                    StopCoroutine(ocarinaCoroutine);
                ocarinaCoroutine = StartCoroutine(PlayOcarina());
            }
        }

        private IEnumerator PlayOcarina()
        {
            playerHeldBy.activatingItem = true;
            playerHeldBy.twoHanded = true;
            UpdatePosRotServerRpc(animationPosition, animationRotation);
            playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", true);  // start playing music animation
            yield return new WaitForSeconds(0.9f);
            if (isHoldingButton && isHeld)
            {
                var isValid = ocarinaSongs[selectedSongID].IsValid(previousPlayerHeldBy, isOcarinaRestricted);
                lastUsedTime = Time.realtimeSinceStartup;
                OcarinaAudioServerRpc(selectedSongID, isValid.Item1);
                StartCoroutine(OcarinaEffect(isValid.Item1, isValid.Item2));
                yield return new WaitForSeconds(0.1f);
                yield return new WaitUntil(() => !isHoldingButton || !isHeld);
            }
            previousPlayerHeldBy.playerBodyAnimator.SetBool("useTZPItem", false);  // stop playing music animation
            UpdatePosRotServerRpc(originalPosition, originalRotation);
            StopOcarinaAudioServerRpc();
            yield return new WaitForSeconds(0.1f);
            previousPlayerHeldBy.activatingItem = false;
            previousPlayerHeldBy.twoHanded = false;
            yield return new WaitForEndOfFrame();
            isPlaying = false;
            ocarinaCoroutine = null;
        }

        private IEnumerator OcarinaEffect(bool isEffectValid, int variationId)
        {
            yield return new WaitForSeconds(0.1f);
            if (isEffectValid)
            {
                yield return new WaitUntil(() => !isHoldingButton || !isHeld || Time.realtimeSinceStartup - lastUsedTime >= selectedSongLength);
                if (isHoldingButton && isHeld && !previousPlayerHeldBy.isPlayerDead)
                {
                    StopOcarinaAudioServerRpc();
                    if (ocarinaSongs[selectedSongID].StartEffect(this, previousPlayerHeldBy, variationId))
                        UpdateUsageServerRpc(selectedSongID);
                }
            }
        }

        public override void ItemInteractLeftRight(bool right)
        {
            base.ItemInteractLeftRight(right);
            if (!right && playerHeldBy != null && !isOcarinaUnique && !isPlaying)
            {
                if (selectedSongID < ocarinaSongs.Length - 1)
                    selectedSongID++;
                else
                    selectedSongID = 0;
                SetControlTips();
                selectedSongLength = noiseSFX[GetNoiseID(selectedSongID)].length;
            }
        }

        public override void PocketItem()
        {
            if (playerHeldBy != null)
                playerHeldBy.equippedUsableItemQE = false;
            base.PocketItem();
        }

        public override void DiscardItem()
        {
            if (playerHeldBy != null && !isPocketed)
                playerHeldBy.equippedUsableItemQE = false;
            base.DiscardItem();
        }

        public override void OnNetworkDespawn()
        {
            if (playerHeldBy != null && !isPocketed)
                playerHeldBy.equippedUsableItemQE = false;
            base.OnNetworkDespawn();
        }

        public override void Update()
        {
            base.Update();
            if (resetUsage && StartOfRound.Instance.inShipPhase)
            {
                resetUsage = false;
                for (int i = 0; i < ocarinaSongs.Length; i++)
                    ocarinaSongs[i].usage = 0;
            }
        }

        public override void LateUpdate()
        {
            if (parentObject != null)
            {
                transform.rotation = parentObject.rotation;
                transform.Rotate(rotation);
                transform.position = parentObject.position;
                Vector3 positionOffset = position;
                positionOffset = parentObject.rotation * positionOffset;
                transform.position += positionOffset;
            }
            if (radarIcon != null)
            {
                radarIcon.position = transform.position;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void OcarinaAudioServerRpc(int audioClientID, bool isEffectValid)
        {
            OcarinaAudioClientRpc(audioClientID, GetNoiseID(audioClientID), isEffectValid);
        }

        [ClientRpc]
        private void OcarinaAudioClientRpc(int audioID, int noiseAudioID, bool isEffectValid)
        {
            if (particleRenderer != null && particleSystem != null && particleSystem.lights.enabled && isEffectValid)
            {
                particleSystem.lights.light.color = ocarinaSongs[audioID].color * OcarinaSong.colorMultiplicator;
                particleRenderer.material.SetColor("_Color", ocarinaSongs[audioID].color * OcarinaSong.colorMultiplicator);
                particleSystem.Play();
            }
            if (noiseAudio != null)
            {
                noiseAudio.PlayOneShot(noiseSFX[noiseAudioID], 1f);
            }
            if (noiseAudioFar != null)
            {
                noiseAudioFar.PlayOneShot(noiseSFXFar[noiseAudioID], 1f);
            }
            WalkieTalkie.TransmitOneShotAudio(noiseAudio, noiseSFX[noiseAudioID], 1f);
            RoundManager.Instance.PlayAudibleNoise(transform.position, noiseRange, 1f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
            if (minLoudness >= 0.6f && playerHeldBy != null)
            {
                playerHeldBy.timeSinceMakingLoudNoise = 0f;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void StopOcarinaAudioServerRpc()
        {
            StopOcarinaAudioClientRpc();
        }

        [ClientRpc]
        private void StopOcarinaAudioClientRpc()
        {
            if (particleSystem != null && particleSystem.isPlaying)
                particleSystem.Stop();
            if (noiseAudio != null && noiseAudio.isPlaying)
                StartCoroutine(Effects.FadeOutAudio(noiseAudio, 0.1f));
            if (noiseAudioFar != null && noiseAudioFar.isPlaying)
                StartCoroutine(Effects.FadeOutAudio(noiseAudioFar, 0.1f));
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateUsageServerRpc(int audioClientID)
        {
            UpdateUsageClientRpc(audioClientID);
        }

        [ClientRpc]
        private void UpdateUsageClientRpc(int audioID)
        {
            ocarinaSongs[audioID].usage++;
            resetUsage = true;
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdatePosRotServerRpc(Vector3 newPos, Vector3 newRot)
        {
            UpdatePosRotClientRpc(newPos, newRot);
        }

        [ClientRpc]
        private void UpdatePosRotClientRpc(Vector3 newPos, Vector3 newRot)
        {
            position = newPos;
            rotation = newRot;
        }

        // OCARINA EFFECTS // COROUTINE & RPCs

        [ServerRpc(RequireOwnership = false)]
        private void EffectsAudioServerRpc(int audioID, float volume, ulong playerID)
        {
            EffectsAudioClientRpc(audioID, volume, true, playerID, false, default);
        }

        [ServerRpc(RequireOwnership = false)]
        private void EffectsAudioServerRpc(int audioID, float volume, Vector3 position)
        {
            EffectsAudioClientRpc(audioID, volume, false, default, true, position);
        }

        [ClientRpc]
        private void EffectsAudioClientRpc(int audioID, float volume, bool onPlayer, ulong playerID, bool onPosition, Vector3 position)
        {
            if (onPlayer)
            {
                var player = StartOfRound.Instance.allPlayerObjects[playerID].GetComponent<PlayerControllerB>();
                player.itemAudio.PlayOneShot(Plugin.audioClips[audioID], volume);
            }
            else if (onPosition)
                Effects.Audio3D(audioID, position, volume, 25);
        }

        [ServerRpc(RequireOwnership = false)]
        private void GlobalAudioServerRpc(int audioID, float volume)
        {
            GlobalAudioClientRpc(audioID, volume);
        }

        [ClientRpc]
        private void GlobalAudioClientRpc(int audioID, float volume)
        {
            Effects.Audio(audioID, volume);
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayMusicInShipServerRpc(int audioID)
        {
            PlayMusicInShipClientRpc(audioID);
        }

        [ClientRpc]
        private void PlayMusicInShipClientRpc(int audioID)
        {
            StartOfRound.Instance.speakerAudioSource.PlayOneShot(Plugin.audioClips[audioID], 0.8f);
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdatePlayerValueServerRpc(ulong playerID)
        {
            UpdatePlayerValueClientRpc(playerID);
        }

        [ClientRpc]
        private void UpdatePlayerValueClientRpc(ulong playerID)
        {
            var player = StartOfRound.Instance.allPlayerObjects[playerID].GetComponent<PlayerControllerB>();
            player.beamUpParticle.Play();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnSpecialEnemyServerRpc(int id, Vector3 position, ulong playerId = default)
        {
            if (id == 0)
            {
                var netRef = Effects.Spawn(GetEnemies.EyelessDog, position);
                SpawnSpecialEnemyClientRpc(id, netRef);
            }
            else if (id == 1)
            {
                if (GetEnemies.RedwoodTitan != null)
                {
                    var netRef = Effects.Spawn(GetEnemies.RedwoodTitan, position);
                    SpawnSpecialEnemyClientRpc(id, netRef);
                }
                else
                    Effects.Spawn(GetEnemies.RedwoodGiant ?? GetEnemies.ForestKeeper, position);
            }
            else if (id == 2)
                Effects.SpawnMaskedOfPlayer(playerId, position);
            else if (id == 3)
            {
                var netRef = Effects.Spawn(GetEnemies.BaboonHawk, position);
                SpawnSpecialEnemyClientRpc(id, netRef);
            }
            else if (id == 98 && GetEnemies.Tornado != null)
            {
                var netRef = Effects.Spawn(GetEnemies.Tornado, position);
                SetTornadoRefClientRpc(netRef);
            }
            else if (id == 99)
                Effects.Spawn(GetEnemies.OldBird, position);
        }

        [ClientRpc]
        private void SpawnSpecialEnemyClientRpc(int id, NetworkObjectReference netRef)
        {
            var enemy = (GameObject)netRef;
            if (id == 0)
            {
                var eponaAI = enemy.GetComponent<MouthDogAI>();
                eponaAI.screamSFX = Plugin.audioClips[15];
                eponaAI.breathingSFX = Plugin.audioClips[16];
                eponaAI.killPlayerSFX = Plugin.audioClips[17];
                eponaAI.enemyBehaviourStates[1].VoiceClip = Plugin.audioClips[21];
                eponaAI.enemyBehaviourStates[2].VoiceClip = eponaAI.breathingSFX;
                eponaAI.enemyBehaviourStates[3].SFXClip = Plugin.audioClips[22];
                enemy.transform.Find("MouthDogModel/VoiceAudio").GetComponent<AudioSource>().clip = eponaAI.breathingSFX;
                var steps = enemy.transform.Find("MouthDogModel/AnimContainer").GetComponent<PlayAudioAnimationEvent>();
                steps.randomClips[0] = Plugin.audioClips[18];
                steps.randomClips[1] = Plugin.audioClips[19];
                steps.randomClips[2] = Plugin.audioClips[20];
                steps.randomizePitch = false;
            }
            else if (id == 1)
            {
                if (GetEnemies.RedwoodTitan != null)  // CR soft dependency
                    Effects.ReplaceRedwoodSfxCR(enemy);
            }
            else if (id == 3)
            {
                var goronAI = enemy.GetComponent<BaboonBirdAI>();
                goronAI.cawScreamSFX[0] = Plugin.audioClips[28];
                goronAI.cawScreamSFX[1] = Plugin.audioClips[29];
                goronAI.cawScreamSFX[2] = Plugin.audioClips[30];
                goronAI.cawScreamSFX[3] = Plugin.audioClips[31];
                goronAI.cawScreamSFX[4] = Plugin.audioClips[32];
            }
        }

        [ClientRpc]
        private void SetTornadoRefClientRpc(NetworkObjectReference netRef)
        {
            tornadoRefs.Add(netRef);
        }

        [ServerRpc(RequireOwnership = false)]
        public void StopTornadoServerRpc()
        {
            if (GetEnemies.Tornado != null && tornadoRefs.Count != 0)
            {
                foreach (var netRef in tornadoRefs)
                    Destroy((GameObject)netRef);
                StopTornadoClientRpc();
            }
        }

        [ClientRpc]
        private void StopTornadoClientRpc()
        {
            if (GetEnemies.Tornado != null)
                tornadoRefs.Clear();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeWeatherServerRpc(string weatherNameResolvable, bool combined = false)
        {
            ChangeWeatherClientRpc(weatherNameResolvable, combined);
        }

        [ClientRpc]
        private void ChangeWeatherClientRpc(string weatherNameResolvable, bool combined)
        {
            if (!Plugin.config.WeatherRegistery)
                return;
            if (!combined)
                Effects.ChangeWeatherWR(weatherNameResolvable);
            else
                Effects.AddCombinedWeatherWR(weatherNameResolvable);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeWeatherServerRpc(LevelWeatherType weather, bool combined = false)
        {
            ChangeWeatherClientRpc(weather, combined);
        }

        [ClientRpc]
        private void ChangeWeatherClientRpc(LevelWeatherType weather, bool combined)
        {
            if (!combined || !Plugin.config.WeatherRegistery)
            {
                Effects.ChangeWeather(weather);
                return;
            }
            Effects.AddCombinedWeatherWR(weather);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeTimeServerRpc(float timeCalculation)
        {
            ChangeTimeClientRpc(timeCalculation);
        }

        [ClientRpc]
        private void ChangeTimeClientRpc(float timeCalculation)
        {
            if (RoundManager.Instance.currentLevel.planetHasTime)
            {
                TimeOfDay.Instance.globalTime += timeCalculation;
                TimeOfDay.Instance.currentDayTime += timeCalculation;
                TimeOfDay.Instance.timeUntilDeadline -= timeCalculation;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void EffectAtPositionServerRpc(int id, Vector3 position)
        {
            EffectAtPositionClientRpc(id, position);
        }

        [ClientRpc]
        private void EffectAtPositionClientRpc(int id, Vector3 position)
        {
            if (id == 0)  // lightning bolt
                Effects.SpawnLightningBolt(position, true);
            else if (id == 1)  // elegy invocation circle
                Instantiate(elegyPrefab, position, Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            else if (id == 2)  // wake old bird flag update
            {
                WakeOldBirdFlag = true;
                WakeOldBirdPosition = position;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void HealPlayersInAreaServerRpc(Vector3 position)
        {
            var players = Effects.GetPlayers();
            foreach (var player in players)
            {
                if (Vector3.Distance(position, player.transform.position) <= 20f)
                {
                    EffectsAudioClientRpc(34, 0.9f, true, player.playerClientId, false, default);
                    HealPlayerClientRpc(player.playerClientId, 100);
                }
            }
        }

        [ClientRpc]
        private void HealPlayerClientRpc(ulong playerID, int health)
        {
            Effects.Heal(playerID, health);
            var player = GameNetworkManager.Instance.localPlayerController;
            if (player.playerClientId == playerID)
                HUDManager.Instance.UpdateHealthUI(player.health, false);
        }

        /*[ServerRpc(RequireOwnership = false)]
        public void ChargeAllItemsServerRpc()
        {
            ChargeAllItemsClientRpc();
        }

        [ClientRpc]
        private void ChargeAllItemsClientRpc()
        {
            bool firstCharge = true;
            var player = GameNetworkManager.Instance.localPlayerController;
            foreach (var item in player.ItemSlots)
            {
                if (item != null && item.insertedBattery != null && item.itemProperties.requiresBattery)
                {
                    item.insertedBattery.charge = 1;
                    if (firstCharge)
                    {
                        EffectsAudioServerRpc(35, 0.9f, player.playerClientId);
                        firstCharge = false;
                    }
                }
            }
        }*/

        [ServerRpc(RequireOwnership = false)]
        private void SetPosFlagsServerRpc(ulong playerID, bool ship, bool exterior, bool interior)
        {
            SetPosFlagsClientRpc(playerID, ship, exterior, interior);
        }

        [ClientRpc]
        private void SetPosFlagsClientRpc(ulong playerID, bool ship, bool exterior, bool interior)
        {
            Effects.SetPosFlags(playerID, ship, exterior, interior);
        }

        [ServerRpc(RequireOwnership = false)]
        public void StopMajoraServerRpc()
        {
            StopMajoraClientRpc();
        }

        [ClientRpc]
        private void StopMajoraClientRpc()
        {
            Effects.StopMajora();
        }

        public IEnumerator OpenDoorZeldaStyle(DoorLock door)
        {
            EffectsAudioServerRpc(12, 1f, door.transform.position);
            yield return new WaitForSeconds(1.45f);
            if (door.isLocked && !door.isPickingLock)
            {
                door.UnlockDoorSyncWithServer();
                yield return new WaitForSeconds(0.5f);
                var animObjTrig = door.gameObject.GetComponent<AnimatedObjectTrigger>();
                if (animObjTrig != null && !door.isDoorOpened)
                {
                    animObjTrig.TriggerAnimationNonPlayer(overrideBool: true);
                    door.OpenDoorAsEnemyServerRpc();
                }
            }
        }

        public IEnumerator TeleportTo(PlayerControllerB player, Vector3 position, bool isFast)
        {
            if (isFast)
            {
                var origin = player.transform.position;
                yield return new WaitForEndOfFrame();
                Effects.Teleportation(player, position);
                SetPosFlagsServerRpc(player.playerClientId, false, false, true);
                EffectsAudioServerRpc(14, 1f, player.playerClientId);
                EffectsAudioServerRpc(14, 1f, origin);
            }
            else
            {
                EffectsAudioServerRpc(10, 0.9f, position);
                EffectsAudioServerRpc(11, 0.9f, player.playerClientId);
                UpdatePlayerValueServerRpc(player.playerClientId);
                yield return new WaitForSeconds(3.2f);
                if (!player.isPlayerDead)
                {
                    Effects.Teleportation(player, position);
                    SetPosFlagsServerRpc(player.playerClientId, true, false, false);
                    EffectsAudioServerRpc(13, 0.9f, player.playerClientId);
                }
            }
        }

        public IEnumerator SoaringEffect(PlayerControllerB player)
        {
            EffectsAudioServerRpc(27, 0.8f, player.playerClientId);
            yield return new WaitForSeconds(5.2f);
            if (!StartOfRound.Instance.inShipPhase && !player.isPlayerDead)
            {
                StartOfRound.Instance.allowLocalPlayerDeath = false;
                Effects.Knockback(player.transform.position - player.transform.up - (player.transform.forward * 0.5f), 7f, physicsForce: 170);
                yield return new WaitForSeconds(0.1f);
                yield return new WaitUntil(() => player.thisController.isGrounded || player.isPlayerDead || StartOfRound.Instance.inShipPhase);
                StartOfRound.Instance.allowLocalPlayerDeath = true;
            }
        }

        public IEnumerator SpawnZeldaEnemy(int id, Vector3 position, ulong playerId = default)
        {
            if (id == 0)  // epona
            {
                var spawnPosition = Effects.GetClosestAINodePosition(RoundManager.Instance.outsideAINodes, position);
                EffectsAudioServerRpc(15, 1f, spawnPosition);
                yield return new WaitForSeconds(1.4f);
                SpawnSpecialEnemyServerRpc(id, spawnPosition);
            }
            else if (id == 1)  // majora giants
            {
                GlobalAudioServerRpc(25, 0.85f);
                yield return new WaitForSeconds(2f);
                for (int i = 0; i < 4; i++)
                {
                    if (StartOfRound.Instance.inShipPhase || StartOfRound.Instance.shipIsLeaving)
                        break;
                    var spawnPosition = RoundManager.Instance.outsideAINodes[Random.Range(0, RoundManager.Instance.outsideAINodes.Length - 1)].transform.position;
                    SpawnSpecialEnemyServerRpc(id, spawnPosition);
                    yield return new WaitForSeconds(5f);
                }
            }
            else if (id == 2)  // elegy statue
            {
                var spawnPosition = RoundManager.Instance.GetNavMeshPosition(position, sampleRadius: 3f);
                if (!RoundManager.Instance.GotNavMeshPositionResult)
                    spawnPosition = Effects.GetClosestAINodePosition(isInFactory ? RoundManager.Instance.insideAINodes : RoundManager.Instance.outsideAINodes, position);
                EffectAtPositionServerRpc(1, spawnPosition);
                yield return new WaitForSeconds(2.5f);
                SpawnSpecialEnemyServerRpc(2, spawnPosition, playerId);
            }
            else if (id == 3)  // gorons
            {
                var spawnPosition = Effects.GetClosestAINodePosition(RoundManager.Instance.insideAINodes, position);
                EffectsAudioServerRpc(28, 1f, spawnPosition);
                yield return new WaitForSeconds(1f);
                for (int i = 0; i < Random.Range(2, 5); i++)
                    SpawnSpecialEnemyServerRpc(id, spawnPosition);
            }
        }

        public IEnumerator WakeTheBird(EnemyAINestSpawnObject birdNest)
        {
            EffectsAudioServerRpc(26, 1f, birdNest.transform.position);
            yield return new WaitForSeconds(2f);
            EffectAtPositionServerRpc(2, birdNest.transform.position);
            SpawnSpecialEnemyServerRpc(99, birdNest.transform.position);
        }

        public IEnumerator MakeAllBaboonSleep()
        {
            int iter = 0;
            yield return new WaitForSeconds(1f);
            var baboons = FindObjectsByType<BaboonBirdAI>(FindObjectsSortMode.None);
            if (baboons.Length == 0)
                yield break;
            while (!StartOfRound.Instance.inShipPhase && !StartOfRound.Instance.shipIsLeaving && iter != 48)
            {
                for (int i = 0; i < baboons.Length; i++)
                {
                    if (baboons[i].eyesClosed)
                        continue;
                    baboons[i].StopFocusingThreat();
                    baboons[i].SwitchToBehaviourState(1);
                    baboons[i].restingDuringScouting = 0f;
                    baboons[i].scoutTimer = 0f;
                    BaboonBirdAI.baboonCampPosition = baboons[i].transform.position;
                    baboons[i].chosenDistanceToCamp = 100f;
                    baboons[i].LeaveCurrentScoutingGroup(sync: true);
                    baboons[i].SetAggressiveMode(0);
                    baboons[i].previousBehaviourState = baboons[i].currentBehaviourStateIndex;
                    if (baboons[i].scoutingSearchRoutine.inProgress)
                        baboons[i].StopSearch(baboons[i].scoutingSearchRoutine);
                    baboons[i].SetDestinationToPosition(BaboonBirdAI.baboonCampPosition);
                    baboons[i].restingAtCamp = true;
                    baboons[i].restAtCampTimer = 240f;
                    if (baboons[i].heldScrap != null)
                        baboons[i].DropHeldItemAndSync();
                    baboons[i].EnemyEnterRestModeServerRpc(true, true);
                }
                yield return new WaitForSeconds(5f);
                iter++;
            }
        }

        public IEnumerator SpawnSuperStormy()
        {
            yield return new WaitForEndOfFrame();
            if (GetEnemies.Tornado != null)
                SpawnSpecialEnemyServerRpc(98, RoundManager.Instance.outsideAINodes[Random.Range(0, RoundManager.Instance.outsideAINodes.Length - 1)].transform.position);
            while (!StartOfRound.Instance.inShipPhase && !StartOfRound.Instance.shipIsLeaving && Effects.IsWeatherEffectPresent(LevelWeatherType.Stormy))
            {
                for (int i = 0; i < Random.Range(1, 5); i++)
                {
                    var boltPosition = RoundManager.Instance.outsideAINodes[Random.Range(0, RoundManager.Instance.outsideAINodes.Length - 1)].transform.position;
                    EffectAtPositionServerRpc(0, boltPosition);
                }
                yield return new WaitForSeconds(Random.Range(0.1f, 2.5f));
            }
            if (GetEnemies.Tornado != null && tornadoRefs.Count != 0)
                StopTornadoServerRpc();
        }
    }
}

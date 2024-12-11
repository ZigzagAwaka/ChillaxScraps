using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class DanceNote : DarkBook
    {
        public readonly GameObject warningPrefab;
        public readonly GameObject glowPrefab;
        private ParticleSystem? particleSystem;

        public DanceNote()
        {
            useCooldown = 2;
            musicToPlayID = 37;
            canKillEnemies = false;
            canvasPrefab = Plugin.gameObjects[3];
            warningPrefab = Plugin.gameObjects[4];
            glowPrefab = Plugin.gameObjects[5];
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            particleSystem = transform.GetChild(0).GetComponent<ParticleSystem>();
        }
    }
}

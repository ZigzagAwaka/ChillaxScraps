using ChillaxScraps.Utils;
using Unity.Netcode;
using UnityEngine;

namespace ChillaxScraps.CustomEffects
{
    internal class Boink : PhysicsProp
    {
        public bool isActive = false;
        public float elapsedTime = 0f;
        public Vector3 force;

        public Boink()
        {
            useCooldown = 2;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (insertedBattery != null)
                insertedBattery.charge = 1;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (playerHeldBy != null && insertedBattery != null && insertedBattery.charge > 0)
            {
                var direction = GetDirection() + Vector3.up;
                BoinkServerRpc(direction.normalized * 500f, 0.5f, 2, playerHeldBy.transform.position, 0.9f, 2.9f);
            }
        }

        public override void Update()
        {
            base.Update();
            if (playerHeldBy != null && isActive)
            {
                elapsedTime -= Time.deltaTime;
                playerHeldBy.externalForces = Vector3.Lerp(playerHeldBy.externalForces, force, Time.deltaTime * 5f);
                if (elapsedTime < 0f)
                    isActive = false;
            }
        }

        private Vector3 GetDirection()
        {
            var r = Random.Range(0, 10);
            if (r <= 7)
                return -transform.forward;
            else if (r == 8)
            {
                if (Random.Range(0, 2) == 0)
                    return -transform.right;
                else
                    return transform.right;
            }
            else  // r == 9
            {
                if (Random.Range(0, 2) == 0)
                    return transform.forward;
                else
                    return -transform.forward;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void BoinkServerRpc(Vector3 force, float duration, int audioID, Vector3 clientPosition, float hostVolume, float clientVolume = default)
        {
            BoinkClientRpc(force, duration, audioID, clientPosition, hostVolume, clientVolume == default ? hostVolume : clientVolume);
        }

        [ClientRpc]
        private void BoinkClientRpc(Vector3 force, float duration, int audioID, Vector3 clientPosition, float hostVolume, float clientVolume)
        {
            if (playerHeldBy != null)
            {
                this.force = force;
                elapsedTime = duration;
                isActive = true;
            }
            Effects.Audio(audioID, clientPosition, hostVolume, clientVolume, playerHeldBy);
        }
    }
}

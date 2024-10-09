using ChillaxScraps.Utils;
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

        private void StartBoink(Vector3 force, float duration)
        {
            if (playerHeldBy != null)
            {
                this.force = force;
                elapsedTime = duration;
                isActive = true;
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (playerHeldBy != null)
            {
                var direction = (-transform.forward) + Vector3.up;
                StartBoink(direction.normalized * 500f, 0.5f);
                Effects.Audio(2, playerHeldBy.transform.position, 3f);
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
    }
}

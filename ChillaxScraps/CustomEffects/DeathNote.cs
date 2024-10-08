namespace ChillaxScraps.CustomEffects
{
    internal class DeathNote : PhysicsProp
    {
        public DeathNote()
        {
            useCooldown = 2;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
        }
    }
}

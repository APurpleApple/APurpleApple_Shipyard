using FMOD;


namespace APurpleApple.Shipyard.EscapePod
{
    internal class ARamAttack : CardAction
    {
        public int hurtAmount;
        public bool targetPlayer;
        
        public override void Begin(G g, State s, Combat c)
        {
            timer = 0;
            
            Ship ship = this.targetPlayer ? c.otherShip : s.ship;
            if (ship == null)
                return;

            Ship target = this.targetPlayer ? s.ship : c.otherShip;

            bool hit = false;
            for (var i = 0; i < ship.parts.Count; i++)
            {
                if (ship.parts[i].type == PType.empty)
                {
                    continue;
                }
                int partX = ship.x + i;
                RaycastResult raycastResult = CombatUtils.RaycastGlobal(c, target, false, partX);
                Part? hitPart = target.GetPartAtWorldX(partX);
                if (hitPart != null && !hitPart.invincible && hitPart.type != PType.empty)
                {
                    hit = true;
                }
                if (raycastResult.hitDrone)
                {
                    if (!c.stuff[partX].Invincible())
                    {
                        c.QueueImmediate(c.stuff[partX].GetActionsOnDestroyed(s, c, !this.targetPlayer, partX));
                        c.stuff[partX].DoDestroyedEffect(s, c);
                        c.stuff.Remove(partX);
                        if (!this.targetPlayer)
                        {
                            foreach (Artifact enumerateAllArtifact in s.EnumerateAllArtifacts())
                                enumerateAllArtifact.OnPlayerDestroyDrone(s, c);
                        }
                    }
                }
            }

            if (!hit) { return; }
            

            target.NormalDamage(s, c, this.hurtAmount, null);

            EffectSpawner.ShipOverheating(g, target.GetShipRect());
            Audio.Play(new GUID?(FSPRO.Event.Hits_HitHurt));
            target.shake++;
        }
        
    }
}
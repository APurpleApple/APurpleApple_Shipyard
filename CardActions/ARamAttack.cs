using FMOD;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.CardActions
{
    internal class ARamAttack : CardAction
    {
        public int hurtAmount;
        public bool targetPlayer;
        
        public override void Begin(G g, State s, Combat c)
        {
            timer = 0;
            
            Ship ship = this.targetPlayer ? s.ship : c.otherShip;
            if (ship == null)
                return;

            Ship target = this.targetPlayer ? c.otherShip : s.ship;

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
            

            ship.NormalDamage(s, c, this.hurtAmount, null);

            EffectSpawner.ShipOverheating(g, ship.GetShipRect());
            Audio.Play(new GUID?(FSPRO.Event.Hits_HitHurt));
            ship.shake++;
        }
        
    }
}
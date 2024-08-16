using FMOD;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using static System.Net.Mime.MediaTypeNames;


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

            if (ApplyAutododge(c, ship, target))
            {
                return;
            }

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
                    DoHit(target, s, c, partX);
                    hit = true;
                }
                if (raycastResult.hitDrone)
                {
                    bool isInvicible = c.stuff[raycastResult.worldX].Invincible();
                    foreach (Artifact item5 in s.EnumerateAllArtifacts())
                    {
                        if (item5.ModifyDroneInvincibility(s, c, c.stuff[raycastResult.worldX]) == true)
                        {
                            isInvicible = true;
                            item5.Pulse();
                        }
                    }

                    if (isInvicible)
                    {
                        c.QueueImmediate(c.stuff[raycastResult.worldX].GetActionsOnShotWhileInvincible(s, c, !targetPlayer, hurtAmount));
                    }
                    else
                    {
                        c.DestroyDroneAt(s, raycastResult.worldX, !targetPlayer);
                    }
                }
            }

            if (!hit) { return; }
            
            EffectSpawner.ShipOverheating(g, target.GetShipRect());
            Audio.Play(new GUID?(FSPRO.Event.Hits_HitHurt));
            target.shake++;
        }
        

        public void DoHit(Ship target, State s, Combat c, int x)
        {
            target.NormalDamage(s, c, this.hurtAmount, x);
            Part? partAtWorldX = target.GetPartAtWorldX(x);
            if (partAtWorldX != null && partAtWorldX.stunModifier == PStunMod.stunnable && !target.isPlayerShip)
            {
                c.QueueImmediate(new AStunPart
                {
                    worldX = x
                });
            }
        }

        public bool ApplyAutododge(Combat c, Ship attacker, Ship target)
        {
            if (attacker.x < target.x + target.parts.Count && attacker.x + attacker.parts.Count > target.x)
            {
                if (target.Get(Status.autododgeRight) > 0)
                {
                    target.Add(Status.autododgeRight, -1);
                    int dir = attacker.x + attacker.parts.Count - target.x;
                    c.QueueImmediate(new List<CardAction>
                {
                    new AMove
                    {
                        targetPlayer = targetPlayer,
                        dir = dir
                    },
                    this
                });
                    timer = 0.0;
                    return true;
                }

                if (target.Get(Status.autododgeLeft) > 0)
                {
                    target.Add(Status.autododgeLeft, -1);
                    int dir2 = attacker.x - target.x - target.parts.Count;
                    c.QueueImmediate(new List<CardAction>
                {
                    new AMove
                    {
                        targetPlayer = targetPlayer,
                        dir = dir2
                    },
                    this
                });
                    timer = 0.0;
                    return true;
                }
            }

            return false;
        }
    }
}
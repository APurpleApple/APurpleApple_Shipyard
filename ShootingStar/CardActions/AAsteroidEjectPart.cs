﻿using APurpleApple.Shipyard.Shared;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.ShootingStar
{
    internal class AAsteroidEjectPart : CardAction
    {
        public string partKey = "";

        public bool far = false;
        public override Icon? GetIcon(State s)
        {
            return new Icon(PMod.sprites[PSpr.Icons_tossPart].Sprite, far ? 2 : 1, Colors.redd);
        }
        public override void Begin(G g, State s, Combat c)
        {
            Part? ejectedPart = null;
            int localX = 0;

            for (int i = 0; i < s.ship.parts.Count; i++)
            {
                if (s.ship.parts[i].key == partKey)
                {
                    ejectedPart = s.ship.parts[i];
                    localX = i;
                    s.ship.parts[i] = new Part()
                    {
                        type = PType.empty,
                        skin = PMod.parts["Asteroid_Scaffolding"].UniqueName,
                        key = "AsteroidScaffolding"
                    };
                    break;
                }
            }


            if (ejectedPart == null) return;
            ArtifactAsteroid? artifact = s.artifacts.Find((x) => x is ArtifactAsteroid) as ArtifactAsteroid;
            if (artifact == null) return;
            if (ejectedPart.key == null) return;

            artifact.ejectedParts.Add(ejectedPart.key, ejectedPart);
            artifact.turnsBeforeComeback.Add(ejectedPart.key, far ? 2 : 1);
            artifact.originalPlace.Add(ejectedPart.key, localX);
            Ship ship = c.otherShip;
            int damage = far ? 2 : 1;
            RaycastResult raycastResult = CombatUtils.RaycastFromShipLocal(s, c, localX, false);

            c.fx.Add(new PartEjection() { part = ejectedPart, worldX = (localX + s.ship.x) * 16, spins = raycastResult.hitDrone || raycastResult.hitShip });
            BgFxPatches.fx.Add(new PartEjectionBG() { part = ejectedPart, worldX = (localX + s.ship.x) * 16 });
            DamageDone dmg = new DamageDone();
            if (raycastResult.hitShip)
            {
                dmg = ship.NormalDamage(s, c, damage, raycastResult.worldX, false);
                Part? partAtWorldX = ship.GetPartAtWorldX(raycastResult.worldX);
                if (partAtWorldX != null && partAtWorldX.stunModifier == PStunMod.stunnable)
                {
                    c.QueueImmediate(new AStunPart
                    {
                        worldX = raycastResult.worldX
                    });
                }

                if ((ship.Get(SStatus.payback) > 0 || ship.Get(SStatus.tempPayback) > 0))
                {
                    c.QueueImmediate(new AAttack
                    {
                        paybackCounter = 1,
                        damage = Card.GetActualDamage(s, ship.Get(SStatus.payback) + ship.Get(SStatus.tempPayback), true),
                        targetPlayer = true,
                        fast = true,
                        storyFromPayback = true
                    });
                }

                if (ship.Get(SStatus.reflexiveCoating) >= 1)
                {
                    c.QueueImmediate(new AArmor
                    {
                        worldX = raycastResult.worldX,
                        targetPlayer = false,
                        justTheActiveOverride = true
                    });
                }
                EffectSpawner.NonCannonHit(g, false, raycastResult, dmg);
            }

            if (raycastResult.hitDrone)
            {
                bool flag2 = c.stuff[raycastResult.worldX].Invincible();
                foreach (Artifact item5 in s.EnumerateAllArtifacts())
                {
                    if (item5.ModifyDroneInvincibility(s, c, c.stuff[raycastResult.worldX]) == true)
                    {
                        flag2 = true;
                        item5.Pulse();
                    }
                }

                if (c.stuff[raycastResult.worldX].bubbleShield)
                {
                    c.stuff[raycastResult.worldX].bubbleShield = false;
                }
                else if (flag2)
                {
                    c.QueueImmediate(c.stuff[raycastResult.worldX].GetActionsOnShotWhileInvincible(s, c, true, damage));
                }
                else
                {
                    c.DestroyDroneAt(s, raycastResult.worldX, true);
                }
                EffectSpawner.NonCannonHit(g, false, raycastResult, dmg);
            }

        }

        public override List<Tooltip> GetTooltips(State s)
        {
            List<Tooltip> list = new List<Tooltip>();

            foreach (Part part in s.ship.parts)
            {
                if (part.key == partKey)
                {
                    part.hilight = true;
                    break;
                }
            }

            list.Add(new CustomTTGlossary(
                CustomTTGlossary.GlossaryType.action,
                () => PMod.sprites[PSpr.Icons_tossPart].Sprite,
                () => PMod.Instance.Localizations.Localize(["action", "TossPart", "name"]),
                () => PMod.Instance.Localizations.Localize(["action", "TossPart", far ? "description2" : "description"]),
                key: typeof(AAsteroidEjectPart).FullName ?? typeof(AAsteroidEjectPart).Name
                ));
            return list;
        }

    }
}

using APurpleApple.Shipyard.Ouranos.Parts;
using Nickel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Ouranos
{
    internal class ArtifactOuranosCannonV2 : Artifact, IModArtifact, IOuranosCannon
    {
        public static void Register(IModHelper helper)
        {
            Type type = MethodBase.GetCurrentMethod()!.DeclaringType!;
            helper.Content.Artifacts.RegisterArtifact(type.Name, new()
            {
                ArtifactType = type,
                Meta = new()
                {
                    owner = Deck.colorless,
                    pools = [ArtifactPool.Boss],
                    unremovable = true,
                },
                Sprite = PMod.sprites[PSpr.Artifacts_OuranosV2].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "Ouranos_V2", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "Ouranos_V2", "description"]).Localize
            });
        }

        public List<AAttack> storedAttacks = new List<AAttack>();

        public override List<Tooltip>? GetExtraTooltips()
        {
            List<Tooltip> tooltips = new List<Tooltip>();

            tooltips.Add(new TTGlossary($"status.{PMod.statuses["ElectricCharge"].Status}", new object[1] { 1 }));

            return tooltips;
        }
        public override void OnReceiveArtifact(State s)
        {
            foreach (Artifact artifact in s.artifacts)
            {
                if (artifact is ArtifactOuranosCannon)
                {
                    artifact.OnRemoveArtifact(s);
                }
            }

            s.artifacts.RemoveAll((Artifact r) => r is ArtifactOuranosCannon);

            ArtifactOuranosCannon.SetCannonSprite(PMod.sprites[PSpr.Parts_ouranos_cannon].Sprite, s);
            PartOuranosCannon? cannon = ArtifactOuranosCannon.GetCannon(s);
            if (cannon != null) cannon.type = PType.special;
        }

        public override void OnCombatStart(State state, Combat combat)
        {
            ArtifactOuranosCannon.SetCannonSprite(PMod.sprites[PSpr.Parts_ouranos_cannon].Sprite, state);
        }

        public void StoreAttack(State state, Combat combat, AAttack attack)
        {
            storedAttacks.Add(attack);
            Pulse();
            combat.QueueImmediate(new AStatus() { targetPlayer = true, status = PMod.statuses["ElectricCharge"].Status, statusAmount = -1, timer = .1 });
            combat.QueueImmediate(new AStatus() { targetPlayer = true, status = PMod.statuses["RailgunCharge"].Status, statusAmount = attack.damage, timer = .1 });
        }

        public override void AfterPlayerStatusAction(State state, Combat combat, Status status, AStatusMode mode, int statusAmount)
        {
            if (status == PMod.statuses["RailgunCharge"].Status)
            {
                ActualizeSprite(state, combat);
            }
        }

        public void ActualizeSprite(State state, Combat combat)
        {
            int charge = state.ship.Get(PMod.statuses["RailgunCharge"].Status);

            Spr sprite = PMod.sprites[PSpr.Parts_ouranos_cannon].Sprite;

            if (charge == 0)
            {
                sprite = PMod.sprites[PSpr.Parts_ouranos_cannon].Sprite;
            }
            if (charge >= 3)
            {
                sprite = PMod.sprites[PSpr.Parts_ouranos_cannon_v2_1].Sprite;
            }
            if (charge >= 6)
            {
                sprite = PMod.sprites[PSpr.Parts_ouranos_cannon_v2_2].Sprite;
            }
            if (charge >= 10)
            {
                sprite = PMod.sprites[PSpr.Parts_ouranos_cannon_v2_3].Sprite;
            }

            RailgunCharge? chargeFx = combat.fx.Find(x => x is RailgunCharge) as RailgunCharge;

            if (chargeFx == null && charge > 0)
            {
                chargeFx = new RailgunCharge();
                combat.fx.Add(chargeFx);
            }
            else if (chargeFx != null && charge == 0)
            {
                combat.fx.Remove(chargeFx);
            }

            if (chargeFx != null)
            {
                chargeFx.intensity = charge;
            }

            ArtifactOuranosCannon.SetCannonSprite(sprite, state);
        }

        public override void OnTurnEnd(State state, Combat combat)
        {
            if (state.ship.Get(PMod.statuses["RailgunCharge"].Status) > 0)
            {/*
                var beam = new AAttack();
                beam.onKillActions = new List<CardAction>();
                foreach (AAttack attack in storedAttacks)
                {
                    beam.weaken |= attack.weaken;
                    beam.stunEnemy |= attack.stunEnemy;
                    if (attack.status.HasValue)
                    {
                        beam.status = attack.status;
                        beam.statusAmount = attack.statusAmount;
                    }
                    beam.armorize |= attack.armorize;
                    beam.brittle |= attack.brittle;
                    beam.moveEnemy = attack.moveEnemy;
                    if (attack.onKillActions != null)
                    {
                        beam.onKillActions.AddRange(attack.onKillActions);
                    }
                    beam.piercing |= attack.piercing;
                }
                storedAttacks.Clear();
                beam.damage = Card.GetActualDamage(state, state.ship.Get(PMod.statuses["RailgunCharge"].Status));
                combat.QueueImmediate(beam);*/

                int cannonX = state.ship.parts.FindIndex((Part p) => p.key == "Ouranos_Cannon");
                combat.Queue(new AAutododgeTrigger() { targetPlayer = false, fromX = cannonX});
                combat.Queue(new ABeamAttack() {damage = state.ship.Get(PMod.statuses["RailgunCharge"].Status), storedattacks = storedAttacks.ToList(), fromX = cannonX });
                storedAttacks.Clear();

                combat.Queue(new AStatus() { targetPlayer = true, mode = AStatusMode.Set, statusAmount = 0, status = PMod.statuses["RailgunCharge"].Status });
            }
        }

        public override void OnTurnStart(State state, Combat combat)
        {
            combat.Queue(new AStatus()
            {
                status = PMod.statuses["ElectricCharge"].Status,
                statusAmount = 1,
                targetPlayer = true,
                artifactPulse = this.Key()
            });
        }
    }
}

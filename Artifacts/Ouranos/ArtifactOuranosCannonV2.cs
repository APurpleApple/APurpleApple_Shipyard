using APurpleApple.Shipyard.VFXs;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Artifacts.Ouranos
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
                Sprite = PMod.sprites["Ouranos_Artifact_Cannon_V2"].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["artifact", "Ouranos_V2", "name"]).Localize,
                Description = PMod.Instance.AnyLocalizations.Bind(["artifact", "Ouranos_V2", "description"]).Localize
            });
        }

        public Spr CannonSprite { get; set; }

        public List<AAttack> storedAttacks = new List<AAttack>();
        public bool allowAttacks = false;

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

            CannonSprite = PMod.sprites["Ouranos_Cannon"].Sprite;
        }

        public override void OnCombatStart(State state, Combat combat)
        {
            CannonSprite = PMod.sprites["Ouranos_Cannon"].Sprite;
        }

        public void StoreAttack(State state, Combat combat, AAttack attack)
        {
            storedAttacks.Add(attack);
            Pulse();
            combat.QueueImmediate(new AStatus() { targetPlayer = true, status = PMod.statuses["RailgunCharge"].Status, statusAmount = attack.damage });
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

            Spr sprite = PMod.sprites["Ouranos_Cannon"].Sprite;

            if (charge == 0)
            {
                sprite = PMod.sprites["Ouranos_Cannon"].Sprite;
            }
            if (charge >= 3)
            {
                sprite = PMod.sprites["Ouranos_Cannon_V2_1"].Sprite;
            }
            if (charge >= 6)
            {
                sprite = PMod.sprites["Ouranos_Cannon_V2_2"].Sprite;
            }
            if (charge >= 10)
            {
                sprite = PMod.sprites["Ouranos_Cannon_V2_3"].Sprite;
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

            CannonSprite = sprite;
        }

        public override void OnTurnEnd(State state, Combat combat)
        {
            allowAttacks = true;
            if (state.ship.Get(PMod.statuses["RailgunCharge"].Status) > 0)
            {
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
                combat.QueueImmediate(beam);
                combat.Queue(new AStatus() { targetPlayer = true, mode = AStatusMode.Set, statusAmount = 0, status = PMod.statuses["RailgunCharge"].Status });
            }
        }

        public override void OnTurnStart(State state, Combat combat)
        {
            allowAttacks = false;
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

using System.Collections.Generic;
namespace APurpleApple.Shipyard.EscapePod
{
    internal class ARamAnim : CardAction
    {
        public int hurtAmount;
        public bool targetPlayer;
        public override void Begin(G g, State s, Combat c)
        {
            c.fx.Add(new ShipRamm());
            c.QueueImmediate(new ARamAttack() { hurtAmount = hurtAmount, targetPlayer = targetPlayer });
        }

        public override Icon? GetIcon(State s)
        {
            return new Icon(PMod.sprites[PSpr.Icons_hurtenemyicon].Sprite, hurtAmount, Colors.hurt);
        }

        public override List<Tooltip> GetTooltips(State s)
        {
            List<Tooltip> list = new List<Tooltip>();
            list.Add(new CustomTTGlossary(
                CustomTTGlossary.GlossaryType.action,
                () => PMod.sprites[PSpr.Icons_hurtenemyicon].Sprite,
                () => PMod.Instance.Localizations.Localize(["action", "Ramm", "name"]),
                () => PMod.Instance.Localizations.Localize(["action", "Ramm", "description"]),
                [() => hurtAmount]
                ));
            return list;
        }
    }
}

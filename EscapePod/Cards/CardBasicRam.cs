using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.EscapePod
{
    internal class CardBasicRam : Card, IModCard
    {
        public static void Register(IModHelper helper)
        {
            Type type = MethodBase.GetCurrentMethod()!.DeclaringType!;
            helper.Content.Cards.RegisterCard(type.Name, new()
            {
                CardType = type,
                Meta = new()
                {
                    deck = Deck.colorless,
                    rarity = Rarity.common,
                    upgradesTo = [Upgrade.A, Upgrade.B],
                    dontOffer = true
                },
                Art = PMod.sprites[PSpr.Cards_card_ramm].Sprite,
                Name = PMod.Instance.AnyLocalizations.Bind(["card", "BasicRam", "name"]).Localize
            });
        }
        public override List<CardAction> GetActions(State s, Combat c)
        {
            var list = new List<CardAction>();

            list.Add(new ARamAnim(){ hurtAmount = upgrade == Upgrade.B ? 2 : 1, targetPlayer = false });
            return list;
        }

        public override CardData GetData(State state)
        {
            CardData data = new CardData();
            switch (this.upgrade)
            {
                case Upgrade.None:
                    data.cost = 1;
                    break;

                case Upgrade.A:
                    data.cost = 0;
                    break;

                case Upgrade.B:
                    data.cost = 1;
                    break;
            }
            return data;
        }
    }
}

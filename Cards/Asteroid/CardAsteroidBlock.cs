using APurpleApple.Shipyard.CardActions;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.Cards
{
    internal class CardAsteroidBlock: Card, IModCard
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
                Name = PMod.Instance.AnyLocalizations.Bind(["card", "AsteroidShield", "name"]).Localize
            });
        }

        public override List<CardAction> GetActions(State s, Combat c)
        {
            List<CardAction> actions = new List<CardAction>();

            actions.Add(new AStatus() { status = SStatus.shield, statusAmount = upgrade == Upgrade.B ? 2 : 1, targetPlayer =true, disabled = flipped});
            actions.Add(new ADummyAction());
            actions.Add(new AAsteroidEjectPart() {disabled = !flipped, partKey = "AsteroidComms", far = upgrade == Upgrade.A });

            return actions;
        }

        public override CardData GetData(State state)
        {
            CardData data = new CardData();
            data.floppable = true;
            data.cost = 1;
            data.art = flipped ? SSpr.cards_Adaptability_Bottom : SSpr.cards_Adaptability_Top;
            return data;
        }
    }
}

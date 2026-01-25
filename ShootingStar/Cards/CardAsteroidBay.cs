using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.ShootingStar
{
    internal class CardAsteroidBay: Card, IModCard
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
                Name = PMod.Instance.AnyLocalizations.Bind(["card", "AsteroidBay", "name"]).Localize
            });
        }

        public override List<CardAction> GetActions(State s, Combat c)
        {
            List<CardAction> actions = new List<CardAction>();

            if (upgrade == Upgrade.B)
            {
                actions.Add(new ASpawn() { thing = new Asteroid(), disabled = flipped , offset = -1});
            }
            actions.Add(new ASpawn() { thing = new Asteroid(), disabled = flipped });
            actions.Add(new ADummyAction());
            actions.Add(new AAsteroidEjectPart() {disabled = !flipped, partKey = "AsteroidMissile"});
            if (upgrade == Upgrade.B)
            {
                actions.Add(new ADrawCard() { disabled = !flipped, count = 1 });
            }

            return actions;
        }

        public override CardData GetData(State state)
        {
            CardData data = new CardData();
            data.floppable = true;
            data.art = flipped ? SSpr.cards_Adaptability_Bottom : SSpr.cards_Adaptability_Top;
            data.cost = 1;

            if (upgrade == Upgrade.A)
            {
                data.cost = 0;
            }
            
            return data;
        }
    }
}

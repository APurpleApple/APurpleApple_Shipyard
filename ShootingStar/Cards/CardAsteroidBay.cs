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
            actions.Add(new AAsteroidEjectPart() {disabled = !flipped, partKey = "AsteroidMissile", far = upgrade == Upgrade.A });

            return actions;
        }

        public override CardData GetData(State state)
        {
            CardData data = new CardData();
            data.floppable = true;

            data.cost = 1;
           
            switch (upgrade)
            {
                case Upgrade.None:
                    data.art = flipped ? SSpr.cards_Adaptability_Bottom : SSpr.cards_Adaptability_Top;
                    break;
                case Upgrade.A:
                    data.art = flipped ? SSpr.cards_Adaptability_Bottom : SSpr.cards_Adaptability_Top;
                    break;
                case Upgrade.B:
                    data.art = flipped ? SSpr.cards_MiningDrill_Bottom : SSpr.cards_MiningDrill_Top;
                    break;
            }
            
            return data;
        }
    }
}

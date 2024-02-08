using Nickel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace APurpleApple.Shipyard.Cards;

internal sealed class CardOuranosSpark : Card, IModCard
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
            Name = PMod.Instance.AnyLocalizations.Bind(["card", "BasicCharge", "name"]).Localize
        });
    }
    public override List<CardAction> GetActions(State s, Combat c)
    {
        var list = new List<CardAction>();
        switch (this.upgrade)
        {
            case Upgrade.None:
                list.Add(new AStatus() { targetPlayer = true, status = PMod.statuses["ElectricCharge"].Status, statusAmount = 1 });
                break;

            case Upgrade.A:
                list.Add(new AStatus() { targetPlayer = true, status = PMod.statuses["ElectricCharge"].Status, statusAmount = 1 });
                break;

            case Upgrade.B:
                list.Add(new AStatus() { targetPlayer = true, status = PMod.statuses["ElectricCharge"].Status, statusAmount = 2 });
                break;
        }

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

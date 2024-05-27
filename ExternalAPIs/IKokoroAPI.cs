using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple.Shipyard.ExternalAPIs
{
    public interface IKokoroApi
    {
        bool IsEvadePossible(State state, Combat combat, int direction, EvadeHookContext context);

        void RegisterEvadeHook(IEvadeHook hook, double priority);
    }

    public enum EvadeHookContext
    {
        Rendering, Action
    }

    public interface IEvadeHook
    {
        bool? IsEvadePossible(State state, Combat combat, int direction, EvadeHookContext context) => IsEvadePossible(state, combat, context);
        bool? IsEvadePossible(State state, Combat combat, EvadeHookContext context) => null;
        void PayForEvade(State state, Combat combat, int direction) { }
        void AfterEvade(State state, Combat combat, int direction, IEvadeHook hook) { }
        List<CardAction>? ProvideEvadeActions(State state, Combat combat, int direction) => null;
    }
}
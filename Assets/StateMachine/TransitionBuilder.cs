using System;

namespace FukaMiya.Utils
{
    public interface ITransitionStarter
    {
        public ITransitionChain When(Func<bool> condition);
        public ITransitionChain When(ICondition condition);
        public Transition Build();
    }

    public interface ITransitionChain
    {
        public ITransitionChain And(Func<bool> condition);
        public ITransitionChain And(ICondition condition);
        public ITransitionChain Or(Func<bool> condition);
        public ITransitionChain Or(ICondition condition);
        public Transition Build();
    }

    public sealed class TransitionBuilder : ITransitionStarter, ITransitionChain
    {
        private readonly State fromState;
        private readonly State toState;
        private ICondition condition;

        public TransitionBuilder(State fromState)
        {
            this.fromState = fromState;
        }
        public TransitionBuilder(State fromState, State toState)
        {
            this.fromState = fromState;
            this.toState = toState;
        }

        public ITransitionChain When(Func<bool> condition) => When(new FuncCondition(condition));
        public ITransitionChain When(ICondition condition)
        {
            this.condition = condition;
            return this;
        }
        
        public ITransitionChain And(Func<bool> condition) => And(new FuncCondition(condition));
        public ITransitionChain And(ICondition condition)
        {
            this.condition = this.condition == null ? condition : new AndCondition(this.condition, condition);
            return this;
        }

        public ITransitionChain Or(Func<bool> condition) => Or(new FuncCondition(condition));
        public ITransitionChain Or(ICondition condition)
        {
            this.condition = this.condition == null ? condition : new OrCondition(this.condition, condition);
            return this;
        }

        public Transition Build()
        {
            var transition = new Transition(fromState, toState);
            transition.AddConditions(condition);
            fromState.AddTransition(transition);
            return transition;
        }
    }

    public interface ICondition
    {
        bool Evaluate();
    }

    public class FuncCondition : ICondition
    {
        private readonly Func<bool> func;
        public FuncCondition(Func<bool> func) => this.func = func;
        public bool Evaluate() => func();
    }

    public class AndCondition : ICondition
    {
        private readonly ICondition a;
        private readonly ICondition b;
        public AndCondition(ICondition a, ICondition b) { this.a = a; this.b = b; }
        public bool Evaluate() => a.Evaluate() && b.Evaluate();
    }

    public class OrCondition : ICondition
    {
        private readonly ICondition a;
        private readonly ICondition b;
        public OrCondition(ICondition a, ICondition b) { this.a = a; this.b = b; }
        public bool Evaluate() => a.Evaluate() || b.Evaluate();
    }

    public sealed class Transition : IEquatable<Transition>
    {
        public State From { get; }
        public State To { get; }
        public float Weight { get; }
        public ICondition Condition { get; private set; }

        public Transition(State from, State to, float weight = 1f)
        {
            From = from;
            To = to;
            Weight = weight;
        }

        public Transition AddConditions(ICondition condition)
        {
            Condition = condition;
            return this;
        }

        public bool Equals(Transition other)
        {
            if (other is null) return false;
            return From == other.From && To == other.To;
        }
    }

    public static class Condition
    {
        public static ICondition Any(Func<bool> a, Func<bool> b)
        {
            return new OrCondition(new FuncCondition(a), new FuncCondition(b));
        }

        public static ICondition Any(params Func<bool>[] conditions)
        {
            return new FuncCondition(() =>
            {
                foreach (var condition in conditions)
                {
                    if (condition()) return true;
                }
                return false;
            });
        }

        public static ICondition All(Func<bool> a, Func<bool> b)
        {
            return new AndCondition(new FuncCondition(a), new FuncCondition(b));
        }

        public static ICondition All(params Func<bool>[] conditions)
        {
            return new FuncCondition(() =>
            {
                foreach (var condition in conditions)
                {
                    if (!condition()) return false;
                }
                return true;
            });
        }

        public static ICondition Not(Func<bool> condition)
        {
            return new FuncCondition(() => !condition());
        }
    }
}
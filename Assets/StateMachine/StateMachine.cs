using System;
using System.Collections.Generic;

namespace FukaMiya.Utils
{
    public sealed class StateMachine
    {
        public State CurrentState { get; private set; }

        private readonly Dictionary<Type, State> states = new();

        public void Update()
        {
            if (CurrentState.CheckTransitionTo(out var nextState))
            {
                CurrentState.OnExit();
                CurrentState = nextState;
                CurrentState.OnEnter();
            }
            else
            {
                CurrentState.OnUpdate();   
            }
        }

        public void SetInitialState<T>() where T : State, new()
        {
            CurrentState = At<T>();
            CurrentState.OnEnter();
        }

        public State At<T>() where T : State, new()
        {
            if (states.TryGetValue(typeof(T), out var state))
            {
                return state;
            }

            state = CreateStateInstance<T>();
            states[typeof(T)] = state;
            return state;
        }

        State CreateStateInstance<T>() where T : State, new()
        {
            T instance = new ();
            instance.Setup(this);
            return instance;
        }
    }

    public static class StateMachineExtensions
    {
        public static TransitionBuilder To<T>(this State from) where T : State, new()
        {
            return new TransitionBuilder(from.StateMachine).From(from).To<T>();
        }
    }

    public abstract class State
    {
        public StateMachine StateMachine { get; private set; }
        public void Setup(StateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        private readonly HashSet<Transition> transitions = new();

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnUpdate() { }

        public bool CheckTransitionTo(out State nextState)
        {
            Transition maxWeightTransition = null;
            foreach (var transition in transitions)
            {
                bool allConditionsMet = true;
                foreach (var condition in transition.Conditions)
                {
                    if (!condition())
                    {
                        allConditionsMet = false;
                        break;
                    }
                }

                if (allConditionsMet)
                {
                    if (maxWeightTransition == null || transition.Weight > maxWeightTransition.Weight)
                    {
                        maxWeightTransition = transition;
                    }
                }
            }

            if (maxWeightTransition != null)
            {
                nextState = maxWeightTransition.To;
                return true;
            }

            nextState = null;
            return false;
        }

        public void AddTransition(Transition transition)
        {
            transitions.Add(transition);
        }

        public override string ToString() => GetType().Name;
    }

    public sealed class Transition : IEquatable<Transition>
    {
        public State From { get; }
        public State To { get; }
        public float Weight { get; }
        public HashSet<Func<bool>> Conditions { get; } = new();

        public Transition(State from, State to, float weight = 1f)
        {
            From = from;
            To = to;
            Weight = weight;
        }

        public Transition AddConditions(HashSet<Func<bool>> condition)
        {
            Conditions.UnionWith(condition);
            return this;
        }

        public bool Equals(Transition other)
        {
            if (other is null) return false;
            return From == other.From && To == other.To;
        }
    }

    public sealed class TransitionBuilder
    {
        private  readonly StateMachine StateMachine;
        private State fromState;
        private State toState;
        private readonly HashSet<Func<bool>> conditions = new();

        public TransitionBuilder(StateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public TransitionBuilder From<T>() where T : State, new()
        {
            fromState = StateMachine.At<T>();
            return this;
        }

        public TransitionBuilder From(State from)
        {
            fromState = from;
            return this;
        }

        public TransitionBuilder To<T>() where T : State, new()
        {
            toState = StateMachine.At<T>();
            return this;
        }

        public TransitionBuilder To(State to)
        {
            toState = to;
            return this;
        }

        public TransitionBuilder When(Func<bool> condition)
        {
            conditions.Add(condition);
            return this;
        }

        public Transition Build()
        {
            var transition = new Transition(fromState, toState);
            transition.AddConditions(conditions);
            fromState.AddTransition(transition);
            return transition;
        }

        public static implicit operator Transition(TransitionBuilder builder)
        {
            return builder.Build();
        }
    }
}

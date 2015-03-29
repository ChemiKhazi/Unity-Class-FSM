using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SubjectNerd.StateMachine
{
	public abstract class StateMachine : MonoBehaviour
	{
		private enum StatePhase
		{
			Started,
			Enter,
			Update,
			Exit
		}

		private StatePhase _statePhase;
		private IFsmState _currentState;
		private IFsmState _nextState;
		private Dictionary<Type, IFsmState> _stateLookup;
		private IEnumerator exitIterator, enterIterator;

		public IFsmState Current_State { get { return _currentState; } }

		public bool IsTransitioning
		{
			get
			{
				return (_statePhase == StatePhase.Enter || _statePhase == StatePhase.Exit);
			}
		}

		public bool HasState
		{
			get { return (_currentState != null && !IsTransitioning); }
		}

		public void Initialize(params Type[] states)
		{
			if (states.Length < 1)
				throw new ArgumentNullException("states", "Initialize must be called with states");

			_statePhase = StatePhase.Started;

			_stateLookup = new Dictionary<Type, IFsmState>();
			for (int i = 0; i < states.Length; i++)
			{
				Type stateType = states[i];
				object instance = Activator.CreateInstance(stateType);
				IFsmState stateInstance = instance as IFsmState;
				if (stateInstance != null)
				{
					SetupState(stateInstance);
					_stateLookup.Add(states[i], stateInstance);
				}
				else
				{
					throw new ArgumentException("State class does not implement IFsmState", stateType.Name);
				}
			}
			StartCoroutine(StateChangeRoutine());
		}

		public void ChangeState(Type state)
		{
			// When coming to change state from the started phase, go immediately to the enter phase
			_statePhase = (_statePhase == StatePhase.Started) ? StatePhase.Enter : StatePhase.Exit;
			_nextState = _stateLookup[state];
			if (_statePhase == StatePhase.Exit)
			{
				exitIterator = _currentState.Exit();
			}
			enterIterator = _nextState.Enter();
		}

		protected abstract void SetupState(IFsmState state);
		protected abstract void InternalStateChange();

		private IEnumerator StateChangeRoutine()
		{
			// Instead of starting a coroutine with every state change
			// and causing allocations, start a single coroutine at the start
			// and use that to manage the state changes. See link for details
			// http://www.gamasutra.com/blogs/WendelinReich/20131109/203841/C_Memory_Management_for_Unity_Developers_part_1_of_3.php

			while (isActiveAndEnabled)
			{
				if (_statePhase == StatePhase.Exit || _statePhase == StatePhase.Enter)
				{
					//Debug.LogFormat("{0} transition", _statePhase);
					bool isExit = _statePhase == StatePhase.Exit;

					// Choose which iterator to use
					var iter = (isExit) ? exitIterator : enterIterator;
					// Loop through the iterator
					while (iter.MoveNext())
					{
						yield return iter.Current;
					}
					//Debug.Log("End transition");
					// Enter/exit coroutined complete, move to next state of transition
					if (isExit)
					{
						_statePhase = StatePhase.Enter;
					}
					else
					{
						_statePhase = StatePhase.Update;
						_currentState = _nextState;
						_nextState = null;
						InternalStateChange();
					}
				}
				yield return null;
			}
		}
	}
}

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

		/// <summary>
		/// Is the StateMachine transitioning between states?
		/// </summary>
		public bool IsTransitioning
		{
			get
			{
				return (_statePhase == StatePhase.Enter || _statePhase == StatePhase.Exit);
			}
		}

		/// <summary>
		/// Whether there is a current state that can be run
		/// </summary>
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

		/// <summary>
		/// Switch the state of the StateMachine
		/// </summary>
		/// <param name="state">The typeof() of the state being switched to</param>
		/// <returns>Success of the state change. Will return false if StateMachine is in the middle of a transition</returns>
		public bool ChangeState(Type state)
		{
			if (_stateLookup.ContainsKey(state) == false)
				throw new ArgumentException("Cannot change to state. Make sure it was added to StateMachine.Initialize", state.Name);

			if (IsTransitioning)
				return false;

			// When coming to change state from the started phase, go immediately to the enter phase
			_statePhase = (_statePhase == StatePhase.Started) ? StatePhase.Enter : StatePhase.Exit;
			_nextState = _stateLookup[state];
			if (_statePhase == StatePhase.Exit)
			{
				exitIterator = _currentState.Exit();
			}
			enterIterator = _nextState.Enter();

			return true;
		}

		protected abstract void InternalStateChange();
		protected abstract void SetupState(IFsmState state);

		private IEnumerator StateChangeRoutine()
		{
			// Instead of starting a coroutine with every state change
			// and causing allocations, start a single coroutine at the start
			// and use that to manage the state changes. See link for details
			// http://www.gamasutra.com/blogs/WendelinReich/20131109/203841/C_Memory_Management_for_Unity_Developers_part_1_of_3.php

			while (isActiveAndEnabled)
			{
				if (IsTransitioning)
				{
					bool isExit = _statePhase == StatePhase.Exit;

					// Choose which iterator to use
					var iter = (isExit) ? exitIterator : enterIterator;
					// Loop through the iterator
					while (iter.MoveNext())
					{
						yield return iter.Current;
					}

					// Exit coroutine complete, move to enter phase
					if (isExit)
					{
						_statePhase = StatePhase.Enter;
					}
					// Enter coroutine complete, move to update phase
					// and clean up
					else
					{
						_statePhase = StatePhase.Update;
						_currentState = _nextState;
						_nextState = null;
						exitIterator = null;
						enterIterator = null;
						InternalStateChange();
					}
				}
				yield return null;
			}
		}
	}
}

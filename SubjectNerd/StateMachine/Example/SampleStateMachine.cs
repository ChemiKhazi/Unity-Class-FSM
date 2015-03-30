using System.Collections;
using UnityEngine;
using SubjectNerd.StateMachine;

namespace MyNamespace.SampleFSM
{
	// This is an interface that defines what events are fired by state machine
	public interface ISampleEvents
	{
		void Update();
		void MouseDown(Vector3 screenPosition);
	}

	// Note: implementing ISampleEvents defined earlier and IFsmState from the state machine
	// This class is not public so it can't be accessed outside of this namespace
	class BaseSampleState : IFsmState, ISampleEvents
	{
		// Lightly coupling the states to the state machine
		// to allow states to call ChangeState()
		// Since this is static, it is shared between all instances of the state class
		protected static SampleStateMachine FSM;

		// This function is called by the state machine and is used
		// to setup the coupling of the states and the state machine
		// In this instance, the state machine is just passing the FSM to the states
		// See the State Machine implementation below for details
		public void Setup(params object[] args)
		{
			FSM = args[0] as SampleStateMachine;
		}

		// The following functions are implementations of the interfaces
		// and are defined as virtual while doing nothing.
		// subclasses simply override them and implement the functionality

		public virtual IEnumerator Enter() { yield break; }
		public virtual IEnumerator Exit() { yield break; }

		public virtual void MouseDown(Vector3 screenPosition) { }
		public virtual void Update() { }
	}

	// This is the actual state machine, which is a MonoBehaviour
	public class SampleStateMachine : StateMachine
	{
		// This function works with the base class to couple state classes to the
		// machine. In this example, the state machine is passed as the first
		// argument to setup. The state base class expects this in its Setup()
		// function. You can pass any other data to state classes here
		protected override void SetupState(IFsmState state)
		{
			state.Setup(this);
		}

		// These cache the state that is currently running
		protected ISampleEvents _current;
		public ISampleEvents Current { get { return _current; } }

		// InternalStateChange is called by the state machine when a state has
		// completed an Enter coroutine. It is strongly recommended that you cache
		// the Current_State (which is an IFsmState) to your event interface
		protected override void InternalStateChange()
		{
			_current = Current_State as ISampleEvents;
		}

		// Standard MonoBehaviour Start()
		void Start()
		{
			// State classes can be defined anywhere, as long as they're accessible by the StateMachine
			// The classes being used to initalize the machine are in TestStates.cs

			// Initialize the states of this machine
			Initialize(typeof(StartTestState),
					  typeof(OtherTestState),
					  typeof(StateInMachineClassFile));

			// Set the initial state
			ChangeState(typeof(StartTestState));
		}

		// Standard MonoBehaviour Update(), use it to drive the events defined in the interface
		void Update()
		{
			// First check with the state machine that a current state exists
			if (!HasState)
				return;

			// We know Current exists, fire off the events  
			Current.Update();

			if (Input.GetMouseButtonDown(0))
				Current.MouseDown(Input.mousePosition);
		}
	} //  End SampleStateMachine

	class StateInMachineClassFile : BaseSampleState
	{
		public override IEnumerator Enter()
		{
			Debug.Log("StateInMachineClassFile.Enter. Press space to go to start, or mouse to go to other state");
			yield break;
		}

		public override void MouseDown(Vector3 screenPosition)
		{
			FSM.ChangeState(typeof (OtherTestState));
		}

		public override void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
				FSM.ChangeState(typeof (StartTestState));
		}
	}
}
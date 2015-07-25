using System.Collections;
using UnityEngine;
using SubjectNerd.StateMachine;

namespace MyNamespace.SampleFSM
{
	/* =====================================================================
	 * 3, 4. Implementing the StateMachine, Initializing the StateMachine
	 * =====================================================================
	 */

	// This is the actual state machine, which is a MonoBehaviour
	public class SampleStateMachine : StateMachine
	{
		// This function works with the base class to couple state classes to the
		// machine. In this example, the state machine is passed as the first
		// argument to setup. The BaseSampleState class expects this in its Setup()
		// function. In your own implementations, you can pass other arguments here
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
			_current = Current as ISampleEvents;
		}

		// Standard MonoBehaviour Start()
		void Start()
		{
			// State classes can be defined anywhere, as long as they're accessible by the StateMachine
			// The first two classes used to initalize the machine are in TestStates.cs,
			// the last is in this same file

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


	/* ======================================================================
	 * A sample state class, check TestStates.cs for the other state classes
	 * ======================================================================
	 */
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
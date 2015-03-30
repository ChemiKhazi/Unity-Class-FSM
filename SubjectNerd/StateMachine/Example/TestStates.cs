using System.Collections;
using SubjectNerd.StateMachine;
using UnityEngine;

namespace MyNamespace.SampleFSM
{
	/* ============================================
	 * 2. State Base Class
	 * ============================================
	 */

	// This base class is not public so it can't be accessed outside of this namespace
	// Note: implementing ISampleEvents defined earlier and IFsmState from the state machine
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

	/* =====================
	 * Test state classes 
	 * -------------------
	 * 
	 * The following classes could be placed in their own files,
	 * but for simple classes keeping multiple states in one file can be useful for organization
	 * =====================
	 */
	class StartTestState : BaseSampleState
	{
		public override IEnumerator Enter()
		{
			Debug.Log("StartTestState.Enter. Mouse down to go to OtherTestState");
			yield break;
		}

		public override void MouseDown(Vector3 screenPosition)
		{
			FSM.ChangeState(typeof (OtherTestState));
		}
	}

	class OtherTestState : BaseSampleState
	{
		public override IEnumerator Enter()
		{
			Debug.Log("OtherTestState.Enter. Press space to go to StateInsideMachine");
			yield break;
		}

		public override IEnumerator Exit()
		{
			Debug.Log("OtherTestState.Exit. Delaying Exit by 2 seconds");
			yield return new WaitForSeconds(2f);
			Debug.Log("OtherTestState has exited");
		}

		public override void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
				FSM.ChangeState(typeof (StateInMachineClassFile));
		}
	}
}

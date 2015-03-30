using System.Collections;
using UnityEngine;

namespace MyNamespace.SampleFSM
{
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

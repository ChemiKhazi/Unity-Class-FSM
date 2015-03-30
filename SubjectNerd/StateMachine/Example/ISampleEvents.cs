using UnityEngine;

namespace MyNamespace.SampleFSM
{
	/* ============================================
	 * 1. State events interface
	 * ============================================
	 */
	public interface ISampleEvents
	{
		void Update();
		void MouseDown(Vector3 screenPosition);
	}
}
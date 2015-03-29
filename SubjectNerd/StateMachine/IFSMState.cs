using System.Collections;

namespace SubjectNerd.StateMachine
{
	public interface IFsmState
	{
		void Setup(params object[] args);
		IEnumerator Enter();
		IEnumerator Exit();
	}
}
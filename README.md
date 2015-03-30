# Class Based FSM for Unity 3D

A class based simple Finite State Machine for Unity3D.

# Design Rationale

This FSM system is designed around having each state being a discrete class, with a configurable level of coupling between the state classes and the MonoBehaviour being controlled.

Instead of using enums to drive state changes, the class types of the states are used as the lookup index allowing for robust refactoring of state names.

# Usage

Using this FSM isn't as automated as other FSM systems, some set up will be required for each FSM that will be created. You will need to write at least one interface and one base class for the state machine to work with.

While not strictly necessary, putting your classes/interfaces in a namespace is recommended as state classes have a tendency to sprawl.

When using this state machine, make sure to include the following directive:

```C#
using SubjectNerd.StateMachine
```

## 1. State events interface

First, you have to decide on what functions your state machine will call and create an interface for it. This allows Mono to call the states in a [performant way](http://stackoverflow.com/a/140018).

```C#
using SubjectNerd.StateMachine

namespace MyNamespace.SampleFSM
{
  public interface ISampleEvents
  {
    void Update();
    void MouseDown(Vector3 screenPosition);
  }
}
```

## 2. State base class

The next step is to create the base class for your states. Your class has to implement the interfaces ```IFsmState``` and the events interface you defined. When creating this class, you can decide how much to couple the state class with the state machine.

```C#
using UnityEngine;
using System.Collections;
using SubjectNerd.StateMachine

namespace MyNamespace.SampleFSM
{
  // Note: implementing IFsmState and the ISampleEvents defined earlier
  class BaseSampleState : IFsmState, ISampleEvents
  {

    // Lightly coupling the states to the state machine
    // to allow the states to call ChangeState() on the machine.
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

    // The following functions are defined as virtual and basically do nothing
    // allowing subclasses to simply override them to implement the functionality
    public virtual IEnumerator Enter() { yield break; }
    public virtual IEnumerator Exit() { yield break; }

    public virtual void MouseDown(Vector3 screenPosition) { }
    public virtual void Update() { }
  }
}
```

## 3. Implementing the StateMachine

The final step is to create a state machine class. This is a subclass of ```StateMachine```, which is a subclass of [```MonoBehaviour```](http://docs.unity3d.com/ScriptReference/MonoBehaviour.html)

When subclassing from ```StateMachine```, there are two functions you must implement: ```SetupState``` and ```InternalStateChange```. See the implementations below for details.

```C#
using UnityEngine;
using SubjectNerd.StateMachine;

namespace MyNamespace.SampleFSM
{
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
  }
}
```

When you implement the StateMachine, you use the StateMachine to call the events defined in the state events interface. Before calling the events in the current state, it is a good idea to check if the state machine ```HasState```.

```C#
public class SampleStateMachine : StateMachine
{
  /* earlier implementations details here */

  // Standard MonoBehaviour update, use it to drive the events defined earlier
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
}
```

## 4. Initializing the State Machine

After the requisite implementation is complete, create your state classes by deriving from your base class.

```C#
namespace MyNamespace.SampleFSM
{
  class TestStart : BaseSampleState
  {
    public override IEnumerator Enter()
    {
      Debug.Log("TestStart.Enter");
    }

    publice override void MouseDown(Vector3 screenPosition)
    {
      // FSM is a reference to the state machine, see the state base class
      FSM.ChangeState(typeof(OtherTestState));
    }
  }

  class OtherTestState : BaseSampleState
  {
    /* Implementation */
  }
}
```

With your state classes, you can now initialize the StateMachine

```C#
public class SampleStateMachine : StateMachine
{
  /* earlier implementations details here */

  // Standard MonoBehaviour start
  void Start()
  {
    // Initialize the states of this machine
    Initialize(typeof(TestStart),
              typeof(OtherTestState));
    // Set the initial state
    ChangeState(typeof(TestStart));
  }
}
```

Calling ```<StateMachine>.ChangeState(typeof(<state class>))``` makes the state machine change states.

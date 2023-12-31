using UnityEngine;
using System.Collections.Generic;

public class FingerEvent
{
    FingerEventDetector detector;
    FingerGestures.Finger finger;
    string name = string.Empty;

    public string Name
    {
        get { return name; }
        internal set { name = value; }
    }

    public FingerEventDetector Detector
    {
        get { return detector; }
        internal set { detector = value; }
    }

    public FingerGestures.Finger Finger
    {
        get { return finger; }
        internal set { finger = value; }
    }

    /// <summary>
    /// Position of the event on the screen
    ///  Usually the same as finger.Position, but it can be different for some events (e.g. move begin)
    /// </summary>
    public virtual Vector2 Position
    {
        get { return finger.Position; }
        internal set { throw new System.NotSupportedException( "Setting position is not supported on " + this.GetType() ); }
    }
    
    #region Object Picking / Raycasting

    GameObject selection;       // object picked at current Position
    RaycastHit hit = new RaycastHit();
    
    /// <summary>
    /// GameObject currently located at this gesture position
    /// </summary>
    public GameObject Selection
    {
        get { return selection; }
        internal set { selection = value; }
    }

    /// <summary>
    /// Last raycast hit result
    /// </summary>
    public RaycastHit Hit
    {
        get { return hit; }
        internal set { hit = value; }
    }
    
    #endregion
}

public abstract class FingerEventDetector<T> : FingerEventDetector where T : FingerEvent, new()
{
    List<T> fingerEventsList;

    public delegate void FingerEventHandler( T eventData );

    protected virtual T CreateFingerEvent()
    {
        return new T();
    }

    public override System.Type GetEventType()
    {
        return typeof( T );
    }

    protected override void Start()
    {
        base.Start();
        InitFingerEventsList( FingerGestures.Instance.MaxFingers );
    }

    protected virtual void InitFingerEventsList( int fingersCount )
    {
        fingerEventsList = new List<T>( fingersCount );

        for( int i = 0; i < fingersCount; ++i )
        {
            T e = CreateFingerEvent();
            e.Detector = this;
            e.Finger = FingerGestures.GetFinger( i );
            fingerEventsList.Add( e );
        }
    }

    protected T GetEvent( FingerGestures.Finger finger )
    {
        return GetEvent( finger.Index );
    }

    protected virtual T GetEvent( int fingerIndex )
    {
        if (fingerEventsList == null) {
            Debug.LogError("FingerEventDetector::GetEvent fingerEventsList is null!");
            return null;
        } else if (fingerEventsList != null && fingerEventsList[fingerIndex] == null) {
            Debug.LogError("FingerEventDetector::GetEvent fingerEventsList["+fingerIndex+"] is null! count:"+fingerEventsList.Count);
            return null;
        }
        return fingerEventsList[fingerIndex];
    }
}

public abstract class FingerEventDetector : MonoBehaviour
{
    public int FingerIndexFilter = -1;    // -1 means any finger
    public ScreenRaycaster Raycaster; 
    public bool UseSendMessage = true;
    public bool SendMessageToSelection = true;
    public GameObject MessageTarget = null;

    FingerGestures.Finger activeFinger;
    RaycastHit lastHit = new RaycastHit();

    protected abstract void ProcessFinger( FingerGestures.Finger finger );

    /// <summary>
    /// Return type description of the internal finger event class used by this detector (editor uses this)
    /// </summary>
    public abstract System.Type GetEventType();
    
    protected virtual void Awake()
    {
        if( !Raycaster )
            Raycaster = GetComponent<ScreenRaycaster>();

        if( !MessageTarget )
            MessageTarget = this.gameObject;
    }

    protected virtual void Start()
    {

    }
        
    protected virtual void Update()
    {
        ProcessFingers();
    }

    protected virtual void ProcessFingers()
    {
        if( FingerIndexFilter >= 0 && FingerIndexFilter < FingerGestures.Instance.MaxFingers )
        {
            ProcessFinger( FingerGestures.GetFinger( FingerIndexFilter ) );
        }
        else
        {
            for( int i = 0; i < FingerGestures.Instance.MaxFingers; ++i )
                ProcessFinger( FingerGestures.GetFinger( i ) );
        }
    }

    /// <summary>
    /// Method used by derived classes to broadcast event message via Unity's SendMessage() API to valid recipients
    /// </summary>
    protected void TrySendMessage( FingerEvent eventData )
    {
        FingerGestures.FireEvent( eventData );

        if( UseSendMessage )
        {
            MessageTarget.SendMessage( eventData.Name, eventData, SendMessageOptions.DontRequireReceiver );

            if( SendMessageToSelection && eventData.Selection && eventData.Selection != MessageTarget )
                eventData.Selection.SendMessage( eventData.Name, eventData, SendMessageOptions.DontRequireReceiver );
        }
    }

    internal RaycastHit LastHit
    {
        get { return lastHit; }
    }

    public GameObject PickObject( Vector2 screenPos )
    {
        if( !Raycaster || !Raycaster.enabled )
            return null;

        if( !Raycaster.Raycast( screenPos, out lastHit ) )
            return null;

        return lastHit.collider.gameObject;
    }

    protected void UpdateSelection( FingerEvent e )
    {
        e.Selection = PickObject( e.Position );
        e.Hit = LastHit;
    }
}

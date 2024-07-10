
using System;
using System.Runtime.CompilerServices;
using Sandbox.Internal;

namespace GeneralGame;



public  class CharacterController2 : Component
{

    private int _stuckTries;

    [SkipHotload]
    private static Attribute[] __Velocity__Attrs2 = new Attribute[2]
    {
        new SyncAttribute2(),
        new SyncAttribute2()
    };

    private Vector3 _repback__Velocity;

    [SkipHotload]
    private static Attribute[] __IsOnGround__Attrs2 = new Attribute[2]
    {
        new SyncAttribute(),
        new SyncAttribute2()
    };




    private bool _repback__IsOnGround;

    [Range( 0f, 200f, 0.01f, true, true )]
    [Property]
    [DefaultValue( 16f )]

    public float Radius { get; set; } = 16f;


    [Range( 0f, 200f, 0.01f, true, true )]
    [Property]
    [DefaultValue( 64f )]

    public float Height { get; set; } = 64f;


    [Range( 0f, 50f, 0.01f, true, true )]
    [Property]
    [DefaultValue( 18f )]

    public float StepHeight { get; set; } = 18f;


    [Range( 0f, 90f, 0.01f, true, true )]
    [Property]
    [DefaultValue( 45f )]

    public float GroundAngle { get; set; } = 45f;


    [Range( 0f, 64f, 0.01f, true, true )]
    [Property]
    [DefaultValue( 10f )]

    public float Acceleration { get; set; } = 10f;


    //
    // Summary:
    //     When jumping into walls, should we bounce off or just stop dead?
    [Range( 0f, 1f, 0.01f, true, true )]
    [Property]
    [DefaultValue( 0.3f )]
    [Description( "When jumping into walls, should we bounce off or just stop dead?" )]

    public float Bounciness { get; set; } = 0.3f;


    //
    // Summary:
    //     If enabled, determine what to collide with using current project's collision
    //     rules for the Sandbox.GameObject.Tags of the containing Sandbox.GameObject.
    [Property]
    [Group( "Collision" )]
    [Title( "Use Project Collision Rules" )]
    [DefaultValue( false )]
    [Description( "If enabled, determine what to collide with using current project's collision rules for the <see cref=\"P:Sandbox.GameObject.Tags\" /> of the containing <see cref=\"T:Sandbox.GameObject\" />." )]

    public bool UseCollisionRules { get; set; } = false;


    [Property]
    [Group( "Collision" )]
    [HideIf( "UseCollisionRules", true )]

    public TagSet IgnoreLayers { get; set; } = new TagSet();



    public BBox BoundingBox => new BBox( new Vector3( 0f - Radius, 0f - Radius, 0f ), new Vector3( Radius, Radius, Height ) );

    [Sync]

    public Vector3 Velocity
    {
        get
        {
            Func<Vector3> func = () => _repback__Velocity;
            return __sync_GetValue( new WrappedPropertyGet<Vector3>
            {
                Value = func(),
                Object = this,
                IsStatic = false,
                TypeName = "Sandbox.CharacterController",
                PropertyName = "Velocity",
                MemberIdent = -906778246,
                Attributes = __Velocity__Attrs
            } );
        }
        set
        {
            __sync_SetValue( new WrappedPropertySet<Vector3>
            {
                Value = value,
                Object = this,
                Setter = delegate
                {
                    _repback__Velocity = value;
                },
                IsStatic = false,
                TypeName = "Sandbox.CharacterController",
                PropertyName = "Velocity",
                MemberIdent = -906778246,
                Attributes = __Velocity__Attrs
            } );
        }
    }

    [Sync]

    public bool IsOnGround
    {
        get
        {
            Func<bool> func = () => _repback__IsOnGround;
            return __sync_GetValue( new WrappedPropertyGet<bool>
            {
                Value = func(),
                Object = this,
                IsStatic = false,
                TypeName = "Sandbox.CharacterController",
                PropertyName = "IsOnGround",
                MemberIdent = 353074387,
                Attributes = __IsOnGround__Attrs
            } );
        }
        set
        {
            __sync_SetValue( new WrappedPropertySet<bool>
            {
                Value = value,
                Object = this,
                Setter = delegate
                {
                    _repback__IsOnGround = value;
                },
                IsStatic = false,
                TypeName = "Sandbox.CharacterController",
                PropertyName = "IsOnGround",
                MemberIdent = 353074387,
                Attributes = __IsOnGround__Attrs
            } );
        }
    }


    public GameObject GroundObject { get; set; }


    public Collider GroundCollider { get; set; }

    protected override void DrawGizmos()
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        Gizmo.GizmoDraw draw = Gizmo.Draw;
        BBox box = BoundingBox;
        draw.LineBBox( in box );
    }

    //
    // Summary:
    //     Add acceleration to the current velocity. No need to scale by time delta - it
    //     will be done inside.
    [Description( "Add acceleration to the current velocity.  No need to scale by time delta - it will be done inside." )]

    public void Accelerate( Vector3 vector )
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        Velocity = Velocity.WithAcceleration( vector, Acceleration * Time.Delta );
    }

    //
    // Summary:
    //     Apply an amount of friction to the current velocity. No need to scale by time
    //     delta - it will be done inside.
    [Description( "Apply an amount of friction to the current velocity. No need to scale by time delta - it will be done inside." )]

    public void ApplyFriction( float frictionAmount, float stopSpeed = 140f )
    {
        float length = Velocity.Length;
        if ( !(length < 0.01f) )
        {
            float num = ((length < stopSpeed) ? stopSpeed : length);
            float num2 = num * Time.Delta * frictionAmount;
            float num3 = length - num2;
            if ( num3 < 0f )
            {
                num3 = 0f;
            }

            if ( num3 != length )
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
                num3 /= length;
                RuntimeHelpers.EnsureSufficientExecutionStack();
                Velocity *= num3;
            }
        }
    }


    private SceneTrace BuildTrace( Vector3 from, Vector3 to )
    {
        return BuildTrace( base.Scene.Trace.Ray( in from, in to ) );
    }

    private SceneTrace BuildTrace( SceneTrace source )
    {
        BBox hull = BoundingBox;
        SceneTrace sceneTrace = source.Size( in hull ).IgnoreGameObjectHierarchy( base.GameObject );
        return UseCollisionRules ? sceneTrace.WithCollisionRules( base.Tags ) : sceneTrace.WithoutTags( IgnoreLayers );
    }

    //
    // Summary:
    //     Trace the controller's current position to the specified delta
    [Description( "Trace the controller's current position to the specified delta" )]

    public SceneTraceResult TraceDirection( Vector3 direction )
    {
        return BuildTrace( base.GameObject.Transform.Position, base.GameObject.Transform.Position + direction ).Run();
    }


    private void Move( bool step )
    {
        if ( step && IsOnGround )
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            Velocity = Velocity.WithZ( 0f );
        }

        if ( Velocity.Length < 0.001f )
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            Velocity = Vector3.Zero;
            return;
        }

        Vector3 position = base.GameObject.Transform.Position;
        CharacterControllerHelper characterControllerHelper = new CharacterControllerHelper( BuildTrace( position, position ), position, Velocity );
        RuntimeHelpers.EnsureSufficientExecutionStack();
        characterControllerHelper.Bounce = Bounciness;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        characterControllerHelper.MaxStandableAngle = GroundAngle;
        if ( step && IsOnGround )
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            characterControllerHelper.TryMoveWithStep( Time.Delta, StepHeight );
        }
        else
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            characterControllerHelper.TryMove( Time.Delta );
        }

        RuntimeHelpers.EnsureSufficientExecutionStack();
        base.Transform.Position = characterControllerHelper.Position;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        Velocity = characterControllerHelper.Velocity;
    }


    private void CategorizePosition()
    {
        Vector3 position = base.Transform.Position;
        Vector3 to = position + Vector3.Down * 2f;
        Vector3 from = position;
        bool isOnGround = IsOnGround;
        if ( !IsOnGround && Velocity.z > 40f )
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            ClearGround();
            return;
        }

        RuntimeHelpers.EnsureSufficientExecutionStack();
        to.z -= (isOnGround ? StepHeight : 0.1f);
        SceneTraceResult sceneTraceResult = BuildTrace( from, to ).Run();
        if ( !sceneTraceResult.Hit || Vector3.GetAngle( in Vector3.Up, in sceneTraceResult.Normal ) > GroundAngle )
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            ClearGround();
            return;
        }

        RuntimeHelpers.EnsureSufficientExecutionStack();
        IsOnGround = true;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        GroundObject = sceneTraceResult.GameObject;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        GroundCollider = sceneTraceResult.Shape?.Collider as Collider;
        if ( isOnGround && !sceneTraceResult.StartedSolid && sceneTraceResult.Fraction > 0f && sceneTraceResult.Fraction < 1f )
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            base.Transform.Position = sceneTraceResult.EndPosition + sceneTraceResult.Normal * 0.01f;
        }
    }

    //
    // Summary:
    //     Disconnect from ground and punch our velocity. This is useful if you want the
    //     player to jump or something.
    [Description( "Disconnect from ground and punch our velocity. This is useful if you want the player to jump or something." )]

    public void Punch( in Vector3 amount )
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        ClearGround();
        RuntimeHelpers.EnsureSufficientExecutionStack();
        Velocity += amount;
    }


    private void ClearGround()
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        IsOnGround = false;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        GroundObject = null;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        GroundCollider = null;
    }

    //
    // Summary:
    //     Move a character, with this velocity
    [Description( "Move a character, with this velocity" )]

    public void Move()
    {
        if ( !TryUnstuck() )
        {
            if ( IsOnGround )
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
                Move( step: true );
            }
            else
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
                Move( step: false );
            }

            RuntimeHelpers.EnsureSufficientExecutionStack();
            CategorizePosition();
        }
    }

    //
    // Summary:
    //     Move from our current position to this target position, but using tracing an
    //     sliding. This is good for different control modes like ladders and stuff.
    [Description( "Move from our current position to this target position, but using tracing an sliding. This is good for different control modes like ladders and stuff." )]

    public void MoveTo( Vector3 targetPosition, bool useStep )
    {
        if ( !TryUnstuck() )
        {
            Vector3 position = base.Transform.Position;
            Vector3 velocity = targetPosition - position;
            CharacterControllerHelper characterControllerHelper = new CharacterControllerHelper( BuildTrace( position, position ), position, velocity );
            RuntimeHelpers.EnsureSufficientExecutionStack();
            characterControllerHelper.MaxStandableAngle = GroundAngle;
            if ( useStep )
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
                characterControllerHelper.TryMoveWithStep( 1f, StepHeight );
            }
            else
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
                characterControllerHelper.TryMove( 1f );
            }

            RuntimeHelpers.EnsureSufficientExecutionStack();
            base.Transform.Position = characterControllerHelper.Position;
        }
    }


    private bool TryUnstuck()
    {
        if ( !BuildTrace( base.Transform.Position, base.Transform.Position ).Run().StartedSolid )
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            _stuckTries = 0;
            return false;
        }

        int num = 20;
        for ( int i = 0; i < num; i++ )
        {
            Vector3 vector = base.Transform.Position + Vector3.Random.Normal * ((float)_stuckTries / 2f);
            if ( i == 0 )
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
                vector = base.Transform.Position + Vector3.Up * 2f;
            }

            RuntimeHelpers.EnsureSufficientExecutionStack();
            if ( !BuildTrace( vector, vector ).Run().StartedSolid )
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
                base.Transform.Position = vector;
                return false;
            }
        }

        RuntimeHelpers.EnsureSufficientExecutionStack();
        _stuckTries++;
        return true;
    }
}

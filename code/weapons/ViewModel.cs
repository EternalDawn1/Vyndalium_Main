using Sandbox;
using System;
using System.Numerics;

namespace GeneralGame;


public sealed class ViewModel : Component
{
	[Property] public SkinnedModelRenderer ModelRenderer { get; set; }
	[Property] public bool UseSprintAnimation { get; set; }

	private Rotation CurRotation { get; set; }
	private Vector3 CurPos { get; set; }

	private float InertiaDamping => 15.0f;

	//private Vector3 SieatOffset => new Vector3( 0f, 0f, -5f );

	private Vector3 swingOffset;
	private float lastPitch;
	private float lastYaw;
	private float bobAnim;
	private float bobSpeed;

	private float SwingInfluence => 0.0125f;
	private float ReturnSpeed => 40.0f;
	private float MaxOffsetLength => 0.125f;
	private float BobCycleTime => 1;

	private static Vector3 BobDirection => new( 0.0f, 0.125f, 0.125f );
	private Rotation CurSmoothRotate { get; set; }
	private Rotation LastCameraCalc { get; set; }

	public float YawInertia { get; private set; }
	public float PitchInertia { get; private set; }


	private Player PlayerController
	{
		get
		{
			if ( Weapon == null || Weapon.Components == null )
			{
				return null;
			}

			return Weapon.Components.GetInAncestors<Player>();

		}
	}
	private CameraComponent Camera { get; set; }
	private WeaponComponent Weapon { get; set; }
	private Rotation targetRotation; // Zielrotation, die erreicht werden soll
	private float rotationDamping = 0.1f;

	public void SetWeaponComponent( WeaponComponent weapon )
	{
		Weapon = weapon;
	}

	public void SetCamera( CameraComponent camera )
	{
		Camera = camera;
	}

	protected override void OnStart()
	{

		ModelRenderer.Set( "b_deploy", true );


		Transform.LocalPosition = Vector3.Zero;
		CurRotation = Rotation.Identity;
		CurSmoothRotate = Rotation.Identity;
		LastCameraCalc = Camera.Transform.Rotation;

		if ( PlayerController.IsValid() )
		{
			PlayerController.OnJump += OnPlayerJumped;

		}
	}
	private void OnPlayerMoved()
	{
		ModelRenderer.Set( "b_run", true );
	}

	

	protected override void OnAwake()
	{
		if ( IsProxy )
		{
			GameObject.Enabled = false;
			ModelRenderer.Enabled = false;
			return;
		}

		base.OnAwake();
	}


	protected override void OnFixedUpdate()
	{

		Vector3 plusPos = Vector3.Zero + Weapon.IdlePos;


		if ( PlayerController.IsAiming )
		{
			CurPos = CurPos.LerpTo( plusPos + Weapon.AimPos, Time.Delta * 10f );
			//Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 20f );

		}
		else
		{
			CurPos = CurPos.LerpTo( plusPos, Time.Delta * 10f );
			//Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
		}
		ModelRenderer.Set( "b_aiming", PlayerController.IsAiming );



		CalcShakeMoves();




		if ( PlayerController.MoveSpeed > 150f )
		{
			CurRotation = Rotation.Lerp( CurRotation, Rotation.Identity * Weapon.RunRotation, Time.Delta * 10f );
		}
		else
		{
			CurRotation = Rotation.Lerp( CurRotation, Rotation.Identity, Time.Delta * 10f );
		}

		CalcRotateSmooth();

		Transform.LocalRotation = CurRotation;
		Transform.LocalPosition = CurPos;

		//base.OnUpdate();
	}

	private void CalcRotateSmooth()
	{
		float CurX;
		float CurY;

		Rotation curCameraCalc = Camera.Transform.Rotation;

		CurX = Angles.NormalizeAngle( LastCameraCalc.Yaw() - curCameraCalc.Yaw() );
		CurY = Angles.NormalizeAngle( LastCameraCalc.Pitch() - curCameraCalc.Pitch() );

		if ( PlayerController.IsAiming )
		{
			targetRotation = Rotation.Identity;
		}
		else
		{
			// Berechnen Sie die Zielrotation basierend auf den aktuellen Kamerabewegungen
			targetRotation = Rotation.From( Math.Clamp( CurY, -1.1f, 1.1f ), Math.Clamp( CurX, -1.1f, 1.5f ), 0 );
		}

		// Anwenden der D�mpfung auf die Rotation, um eine sanfte Bewegung zu erreichen
		CurRotation = Rotation.Slerp( CurRotation, CurRotation * targetRotation, Time.Delta * rotationDamping );

		// Aktualisieren der letzten Kameraberechnung f�r den n�chsten Durchlauf
		LastCameraCalc = Rotation.Lerp( LastCameraCalc, curCameraCalc, Time.Delta * 30f );
	}

	private void CalcShakeMoves()
	{
		var newPitch = CurRotation.Pitch();
		var newYaw = CurRotation.Yaw();

		var pitchDelta = Angles.NormalizeAngle( newPitch - lastPitch );
		var yawDelta = Angles.NormalizeAngle( lastYaw - newYaw );

		PitchInertia += pitchDelta;
		YawInertia += yawDelta;


		var playerVelocity = PlayerController.CharacterController.Velocity;


		var verticalDelta = playerVelocity.z * Time.Delta;
		var viewDown = Rotation.FromPitch( newPitch ).Up * -1.0f;
		verticalDelta *= 1.0f - System.MathF.Abs( viewDown.Cross( Vector3.Down ).y );
		pitchDelta -= verticalDelta * 1.0f;

		var speed = playerVelocity.WithZ( 0 ).Length;
		speed = speed > 10.0 ? speed : 0.0f;


		if ( speed > 0f && PlayerController.IsAiming )
		{
			speed = 10f;
		}


		bobSpeed = bobSpeed.LerpTo( speed, Time.Delta * InertiaDamping );


		var offset = CalcBobbingOffset( bobSpeed );
		offset += CalcSwingOffset( pitchDelta, yawDelta );

		CurPos += offset;


		lastPitch = newPitch;
		lastYaw = newYaw;

		YawInertia = YawInertia.LerpTo( 0, Time.Delta * InertiaDamping );
		PitchInertia = PitchInertia.LerpTo( 0, Time.Delta * InertiaDamping );
	}
	private Vector3 CalcSwingOffset( float pitchDelta, float yawDelta )
	{
		var swingVelocity = new Vector3( 0, yawDelta, pitchDelta );

		// Adjust the swing influence to reduce the impact of small movements on swing offset
		swingVelocity *= 0.3f;

		swingOffset -= swingOffset * ReturnSpeed * Time.Delta;
		swingOffset += (swingVelocity * SwingInfluence);

		if ( swingOffset.Length > MaxOffsetLength )
		{
			swingOffset = swingOffset.Normal * MaxOffsetLength;
		}

		return swingOffset;
	}

	private Vector3 CalcBobbingOffset( float speed )
	{
		bobAnim += Time.Delta * BobCycleTime;

		var twoPI = System.MathF.PI * 2.0f;

		if ( bobAnim > twoPI )
		{
			bobAnim -= twoPI;
		}

		var offset = BobDirection * (speed * 0.005f) * System.MathF.Cos( bobAnim );
		offset = offset.WithZ( -System.MathF.Abs( offset.z ) );

		return offset;
	}


	private void OnPlayerJumped()
	{
		ModelRenderer.Set( "b_jump", true );
	}
	private void ApplyRecoil()
	{
		// Beispielwerte f�r R�cksto�effekte
		float recoilAmount = 5.0f; // St�rke des R�cksto�es
		float recoilRecoverySpeed = 1.5f; // Geschwindigkeit der R�ckkehr

		// Anwendung des R�cksto�es auf die Rotation
		CurRotation *= Rotation.FromPitch( -recoilAmount );

		// Gl�tten der R�ckkehr zur urspr�nglichen Rotation
		CurRotation = Rotation.Slerp( CurRotation, Rotation.Identity, Time.Delta * recoilRecoverySpeed );
	}


}

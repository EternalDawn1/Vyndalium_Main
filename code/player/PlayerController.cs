using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.Citizen;



namespace GeneralGame;

public partial class Player : Component, IHealthComponent
{
	[Property] public Vector3 Gravity { get; set; } = new( 0f, 0f, 800f );

	[Property] public SkinnedModelRenderer ModelRenderer { get; private set; }
	[Property] public RagdollController Ragdoll { get; private set; }
	[Property] public List<CitizenAnimationHelper> Animators { get; private set; } = new();
	[Property] private CitizenAnimationHelper ShadowAnimator { get; set; }
	[Property] public WeaponContainer Weapons { get; set; }
	public WeaponComponent DeployedWeapon { get; set; }
	[Property] public CameraComponent PlyCamera { get; set; }
	[Property] public GameObject ViewModelRoot { get; set; }
	public int DefaultAmmo { get; set; }
	[Property] public AmmoContainer Ammo { get; } = new AmmoContainer();
	[Property] public CharacterController CharacterController { get; set; }
	[Property] public MoveHelper MoveHelper { get; set; }
	[Property] public GameObject Head { get; set; }
	[Property] public GameObject Eye { get; set; }
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] public SoundEvent HurtSound { get; set; }
	[Property] public SoundEvent HurtLowHP { get; set; }
	[Property] public SoundEvent HurtMidHP { get; set; }
	[Property] public bool SicknessMode { get; set; }
	[Property] public float StandHeight { get; set; } = 64f;
	[Property] public float DuckHeight { get; set; } = 28f;
	[Property] public Action OnJump { get; set; }
	[Sync] public LifeState LifeState { get; private set; } = LifeState.Alive;
	[Sync] public Angles EyeAngles { get; set; }
	[Sync] public bool IsAiming { get; set; }
	[Sync] public bool IsRunning { get; set; }
	[Sync] public bool IsCrouching { get; set; }
	[Sync] public int Deaths { get; private set; }
	[Sync] public int Kills { get; private set; }
	public Vector3 WishVelocity { get; private set; }
	private Vector3 SieatOffset => new Vector3( 0f, 0f, -40f );
	private RealTimeSince LastGroundedTime { get; set; }
	private RealTimeSince LastUngroundedTime { get; set; }
	private RealTimeSince TimeSinceDamaged { get; set; }
	private RealTimeSince TimeSinceManaUsed { get; set; }

	private static bool isFirstSpawn = true;


	private bool WantsToCrouch { get; set; }
	private Angles Recoil { get; set; }
	[Property] public float GroundControl { get; private set; } = 4.0f;
	[Property] public float Aircontrol { get; private set; } = 0.1f;
	public static bool DebugCamera { get; set; } = false;


	[Property] public bool ThirdPersonEnabled { get; set; }
	protected BoxCollider Collider;

	HiddenBodyGroup _hideBodygroups;

	public Vector3 Velocity => CharacterController.Velocity;
	[Sync]
	public HiddenBodyGroup HideBodygroups
	{
		get => _hideBodygroups;
		set
		{
			_hideBodygroups = value;

			if ( ModelRenderer == null )
				return;

			ModelRenderer.SetBodyGroup( "head", _hideBodygroups.HasFlag( HiddenBodyGroup.Head ) ? 1 : 0 );
			ModelRenderer.SetBodyGroup( "torso", _hideBodygroups.HasFlag( HiddenBodyGroup.Torso ) ? 1 : 0 );
			ModelRenderer.SetBodyGroup( "hands", _hideBodygroups.HasFlag( HiddenBodyGroup.Hands ) ? 1 : 0 );
			ModelRenderer.SetBodyGroup( "legs", _hideBodygroups.HasFlag( HiddenBodyGroup.Legs ) ? 1 : 0 );
			ModelRenderer.SetBodyGroup( "feet", _hideBodygroups.HasFlag( HiddenBodyGroup.Feet ) ? 1 : 0 );
		}
	}
	bool _blockMovements = false;
	/// <summary>
	/// Block both inputs and mouse aiming
	/// </summary>
	[Sync]
	public bool BlockMovements
	{
		get => DebugCamera ? true : _blockMovements;
		set => _blockMovements = value;
	}

	bool _blockMouseAim = false;

	/// <summary>
	/// Block mouse aiming
	/// </summary>
	[Sync]
	public bool BlockMouseAim
	{
		get => BlockMovements || _blockMouseAim;
		set => _blockMouseAim = value;
	}

	bool _blockInputs = false;

	/// <summary>
	/// Block inputs (Like WASD, Pissing, Left/Right click)
	/// </summary>
	[Sync]
	public bool BlockInputs
	{
		get => BlockMovements || _blockInputs;
		set => _blockInputs = value;
	}
	public void ForceHoldType( HoldType type, float time )
	{
		_targetHoldType = type;
		_resetHoldType = time;
	}
	private HoldType _targetHoldType;
	private TimeUntil _resetHoldType;

	[Sync] public HoldType HoldType { get; set; } = HoldType.Idle;


	public void IncreaseMana( float amount )
	{
		MaxMana += amount;
	}
	public void IncreaseCritHitDamage( float amount )
	{
		CritHitDamage += amount;
	}
	public void DecreaseCritHitDamage( float amount )
	{
		CritHitDamage -= amount;
	}

	public void IncreaseCritHitChance( float amount )
	{
		CritHitChance += amount;
	}
	public void DecreaseCritHitChance( float amount )
	{
		CritHitChance -= amount;
	}

	public static void GiveVyndaliumDebug( int money = 0 )
	{
		Player.Local.GiveVyndalium( money );
		Log.Info( $"Given {money}mk" );
	}

	[ConCmd( "newgame_give_statspoints" ), AdminAttribute]
	public static void GiveStatsPoints()
	{
		Player.Local.StatsPoints += 10;
	}
	public bool TrySpendVyndalium( int amount )
	{
		if ( Vyndalium >= amount )
		{
			Vyndalium -= amount;
			return true;
		}
		return false;
	}


	public bool TakeVyndalium( int amount )
	{
		if ( Vyndalium < amount )
			return false;

		Vyndalium -= amount;
		return true;
	}
	[Broadcast]
	public void GiveVyndalium( int amount )
	{
		Vyndalium += amount;

	}
	[Broadcast]
	public void GiveXp( int amount )
	{
		AddExperience( amount );

	}
	public void ChangeMana( float amount )
	{
		Mana += amount;
	}


	public void ApplyRecoil( Angles recoil )
	{
		if ( IsProxy ) return;

		Recoil += recoil;
	}

	public void ResetViewAngles()
	{
		var rotation = Rotation.Identity;
		EyeAngles = rotation.Angles().WithRoll( 0f );
	}

	public async void RespawnAsync( float seconds )
	{
		if ( IsProxy ) return;

		await Task.DelaySeconds( seconds );
		Respawn();
	}
	public void EquipWeaponsOnSpawn()
	{
		foreach ( var item in Inventory.EquippedItems )
		{
			if ( item is ItemEquipment equipment )
			{
				Inventory.GiveEquipmentItem( equipment );
			}
		}
	}
	public Transform GetAttachment( string attachment, bool world = true )
	=> ModelRenderer.GetAttachment( attachment, world ) ?? global::Transform.Zero;

	public void Respawn()
	{
		if ( IsProxy )
			return;

		Weapons.GiveDefault();
		EquipWeaponsOnSpawn();
		Ragdoll.Unragdoll();
		MoveToSpawnPoint();

		LifeState = LifeState.Alive;


		if ( isFirstSpawn )
		{
			MaxHealth = 50f;
			Health = MaxHealth;
			MaxStamina = 50f;
			MaxMana = 100f;
			PlayerRunSpeed = 220f;
			isFirstSpawn = false; // Markiere den ersten Spawn als abgeschlossen
		}
		Health = MaxHealth;
		// Setze die Gesundheit auf die maximale Gesundheit und die Ausdauer auf die maximale Ausdauer
		MaxHealth = Health;
		Stamina = MaxStamina;
		Mana = MaxMana;

		// Starte die Gesundheitsregeneration
		StartHealthRegen( 500f, 5f );


	}
	[AdminAttribute]
	public async void StartHealthRegen( float regenAmount, float duration )
	{
		float originalHealth = MaxHealth;
		float endTime = Time.Now + duration;

		// Erhöhe die Gesundheit des Spielers über die Dauer hinweg
		while ( Time.Now < endTime )
		{
			Health = Math.Min( MaxHealth, Health + regenAmount * Time.Delta );
			await Task.Delay( 1000 / 60 );
		}


	}

	[Broadcast]
	public void TakeDamage( DamageType type, Single amount, Vector3 hitPosition, Vector3 hitDirection, Guid attackerId, Guid playerId )
	{
		if ( LifeState == LifeState.Dead )
			return;

		if ( type == DamageType.Bullet )
		{
			var p = new SceneParticles( Scene.SceneWorld, "particles/impact.flesh.bloodpuff.vpcf" );
			p.SetControlPoint( 0, hitPosition );
			p.SetControlPoint( 0, Rotation.LookAt( hitDirection.Normal * -1f ) );
			p.PlayUntilFinished( Task );

			if ( HurtSound is not null )
			{
				Sound.Play( HurtSound, Transform.Position );
			}
		}

		if ( IsProxy )
			return;

		TimeSinceDamaged = 0f;
		Health = MathF.Max( Health - amount, 0f );

		if ( Health <= 0f )
		{
			LifeState = LifeState.Dead;
			Ragdoll.Ragdoll( hitPosition, hitDirection );
			SendKilledMessage( attackerId );


		}
	}

	protected virtual bool CanUncrouch()
	{
		if ( !IsCrouching ) return true;
		if ( LastUngroundedTime < 0.2f ) return false;

		var tr = CharacterController.TraceDirection( Vector3.Up * DuckHeight );
		return !tr.Hit;
	}

	protected virtual void OnKilled( GameObject attacker )
	{
		if ( attacker.IsValid() )
		{
			var chat = Scene.GetAllComponents<Chat>().FirstOrDefault();

			if ( chat.IsValid() )

				if ( attacker.Network.OwnerConnection.DisplayName != this.Network.OwnerConnection.DisplayName )
				{
					chat.AddTextLocal( "💀️", $"{this.Network.OwnerConnection.DisplayName} has killed {attacker.Network.OwnerConnection.DisplayName}" );
				}

			if ( !this.IsProxy )
			{
				// We killed this player.
				this.Kills++;
			}


		}



		if ( IsProxy )
			return;

		if ( Weapons.Deployed.IsValid() )
		{
			Weapons.Deployed.Holster();
		}


		RespawnAsync( 3f );

		Deaths++;
	}

	protected override void OnAwake()
	{

		Inventory = Components.Get<Inventory>( FindMode.EverythingInSelfAndDescendants );

		ModelRenderer = Components.GetInDescendantsOrSelf<SkinnedModelRenderer>();
		Collider = Components.Get<BoxCollider>( FindMode.EverythingInSelfAndDescendants );

		CharacterController = Components.GetInDescendantsOrSelf<CharacterController>();
		CharacterController.IgnoreLayers.Add( "player" );

		Ragdoll = Components.GetInDescendantsOrSelf<RagdollController>();

		if ( CharacterController.IsValid() )
		{
			CharacterController.Height = StandHeight;
		}

		if ( IsProxy )
			return;

		ResetViewAngles();


	}

	protected override void OnStart()
	{
		

		if ( !IsProxy )
		{
			Respawn();
			Animators.Clear(); // Entfernt alle vorherigen Einträge
			Animators.Add( ShadowAnimator );
			Animators.Add( AnimationHelper );

		}
		if ( !Game.IsPlaying || Scene == GameObject )
			return;

		if ( !IsProxy ) // Load save.
		{

			Setup( this );
		}


		base.OnStart();
	}





	private void UpdateModelVisibility()
	{
		if ( !ModelRenderer.IsValid() )
			return;

		if ( IsProxy ) PlyCamera.Enabled = false;


		var deployedWeapon = Weapons.Deployed;
		var shadowRenderer = ShadowAnimator.Components.Get<SkinnedModelRenderer>( true );
		var hasViewModel = deployedWeapon.IsValid() && deployedWeapon.HasViewModel;
		var clothing = ModelRenderer.Components.GetAll<ClothingComponent>( FindMode.EverythingInSelfAndDescendants );

		if ( hasViewModel )
		{
			shadowRenderer.Enabled = false;

			ModelRenderer.Enabled = Ragdoll.IsRagdolled;
			ModelRenderer.RenderType = Sandbox.ModelRenderer.ShadowRenderType.On;

			foreach ( var c in clothing )
			{
				c.ModelRenderer.Enabled = Ragdoll.IsRagdolled;
				c.ModelRenderer.RenderType = Sandbox.ModelRenderer.ShadowRenderType.On;
			}

			return;
		}

		ModelRenderer.SetBodyGroup( "head", IsProxy ? 0 : 1 );
		ModelRenderer.Enabled = true;

		if ( Ragdoll.IsRagdolled )
		{
			ModelRenderer.RenderType = Sandbox.ModelRenderer.ShadowRenderType.On;
			shadowRenderer.Enabled = false;
		}
		else
		{
			ModelRenderer.RenderType = IsProxy
				? Sandbox.ModelRenderer.ShadowRenderType.On
				: Sandbox.ModelRenderer.ShadowRenderType.Off;

			shadowRenderer.Enabled = true;
		}

		foreach ( var c in clothing )
		{
			c.ModelRenderer.Enabled = true;

			if ( c.Category is Clothing.ClothingCategory.Hair or Clothing.ClothingCategory.Facial or Clothing.ClothingCategory.Hat )
			{
				c.ModelRenderer.RenderType = IsProxy ? Sandbox.ModelRenderer.ShadowRenderType.On : Sandbox.ModelRenderer.ShadowRenderType.ShadowsOnly;
			}
		}
		if ( !PlyCamera.IsValid() || !Eye.IsValid() )
			return;

		var cameraPosition = PlyCamera.Transform.Position;
		var cameraDirection = PlyCamera.Transform.Rotation.Forward;
		var fieldOfView = PlyCamera.FieldOfView;
		IEnumerable<SceneObject> sceneObjects = GetSceneObjects(); // Annahme: PlyCamera hat eine Eigenschaft FieldOfView

		foreach ( var obj in sceneObjects ) // Pseudocode: Iteriere über alle Objekte in der Szene
		{
			var directionToObject = (obj.Transform.Position - cameraPosition).Normal;
			var angleToObject = Vector3Extensions.AngleBetween( cameraDirection, directionToObject );

			if ( angleToObject <= fieldOfView / 2 )
			{
				// Das Objekt ist im Sichtfeld der Kamera
				obj.SetVisibility( true ); // Pseudocode: Setze die Sichtbarkeit des Objekts
			}
			else
			{
				// Das Objekt ist außerhalb des Sichtfelds der Kamera
				obj.SetVisibility( false ); // Pseudocode: Setze die Sichtbarkeit des Objekts
			}
		}


	}
	public IEnumerable<SceneObject> GetSceneObjects()
	{
		// Implementierung abhängig von der spezifischen Logik Ihrer Anwendung
		return new List<SceneObject>(); // Beispielrückgabe
	}


	protected override void OnPreRender()
	{
		base.OnPreRender();

		if ( !Scene.IsValid() || !PlyCamera.IsValid() )
			return;

		UpdateModelVisibility();

		if ( IsProxy )
			return;

		if ( !Eye.IsValid() )
			return;

		if ( Ragdoll.IsRagdolled )
		{
			PlyCamera.Transform.Position = PlyCamera.Transform.Position.LerpTo( Eye.Transform.Position, Time.Delta * 32f );
			PlyCamera.Transform.Rotation = Rotation.Lerp( PlyCamera.Transform.Rotation, Eye.Transform.Rotation, Time.Delta * 16f );
			return;

		}




		if ( !IsProxy )

		{
			PlyCamera.Transform.LocalPosition = Vector3.Zero;
			var idealEyePos = Eye.Transform.Position;
			var headPosition = Transform.Position + Vector3.Up * CharacterController.Height;
			var headTrace = Scene.Trace.Ray( Transform.Position, headPosition )
				.UsePhysicsWorld()
				.IgnoreGameObjectHierarchy( GameObject )
				.WithAnyTags( "solid" )
				.Run();

			headPosition = headTrace.EndPosition - headTrace.Direction * 2f;

			var trace = Scene.Trace.Ray( headPosition, idealEyePos )
				.UsePhysicsWorld()
				.IgnoreGameObjectHierarchy( GameObject )
				.WithAnyTags( "solid" )
				.Radius( 2f )
				.Run();

			var deployedWeapon = Weapons.Deployed;
			var hasViewModel = deployedWeapon.IsValid() && deployedWeapon.HasViewModel;

			if ( hasViewModel )
				PlyCamera.Transform.Position = Head.Transform.Position;
			else
				PlyCamera.Transform.Position = trace.Hit ? trace.EndPosition : idealEyePos;

			if ( SicknessMode )
				PlyCamera.Transform.Rotation = Rotation.LookAt( Eye.Transform.Rotation.Left ) * Rotation.FromPitch( -10f );
			else
				PlyCamera.Transform.Rotation = EyeAngles.ToRotation() * Rotation.FromPitch( -10f );


			if ( IsCrouching && hasViewModel )
			{
				PlyCamera.Transform.Position = PlyCamera.Transform.Position + SieatOffset;
			}
		}

	}
	bool isLowHealthSoundPlaying = false;

	bool isMidHealthSoundPlaying = false;

	protected override void OnUpdate()
	{


		if ( Ragdoll.IsRagdolled || LifeState == LifeState.Dead )
			return;

		if ( !IsProxy )
		{
			var angles = EyeAngles.Normal;
			angles += Input.AnalogLook * 0.5f;
			angles += Recoil * Time.Delta;
			angles.pitch = angles.pitch.Clamp( -80f, 89.9f );


			EyeAngles = angles.WithRoll( 0f );
			IsRunning = Input.Down( "Run" ) && !IsAiming;
			Recoil = Recoil.LerpTo( Angles.Zero, Time.Delta * 8f );

		}
		// Überprüfen Sie den Gesundheitszustand des Spielers
		// Check the player's health status
		float healthPercentage = Health / MaxHealth * 100;
		int healthRange = healthPercentage > 50 ? 2 : healthPercentage > 25 ? 1 : 0;

		switch ( healthRange )
		{
			case 0: // Gesundheit <= 25%
				if ( !isLowHealthSoundPlaying && HurtLowHP is not null )
				{
					Sound.Play( HurtLowHP, PlyCamera.Transform.Position );
					isLowHealthSoundPlaying = true;
				}
				break;
			case 1: // 25% < Gesundheit <= 50%
				if ( isLowHealthSoundPlaying )
				{
					Sound.StopAll( float.MaxValue );
					isLowHealthSoundPlaying = false;
				}
				if ( !isMidHealthSoundPlaying && HurtMidHP is not null )
				{
					Sound.Play( HurtMidHP, PlyCamera.Transform.Position );
					isMidHealthSoundPlaying = true;
				}
				break;
			case 2: // Gesundheit > 50%
				if ( isMidHealthSoundPlaying )
				{
					Sound.StopAll( float.MaxValue );
					isMidHealthSoundPlaying = false;
				}
				break;
		}
		UpdateModelVisibility();


		var weapon = Weapons.Deployed;

		foreach ( var animator in Animators )
		{
			animator.HoldType = weapon.IsValid() ? weapon.HoldType : CitizenAnimationHelper.HoldTypes.None;
			animator.WithVelocity( CharacterController.Velocity );
			animator.WithWishVelocity( WishVelocity );
			animator.IsGrounded = CharacterController.IsOnGround;
			animator.MoveRotationSpeed = 0f;
			animator.DuckLevel = IsCrouching ? 1f : 0f;
			animator.WithLook( EyeAngles.Forward );
			animator.MoveStyle = (IsRunning && !IsCrouching) ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;
		}
	}

	protected virtual void DoCrouchingInput()
	{
		WantsToCrouch = CharacterController.IsOnGround && Input.Down( "Duck" );

		if ( WantsToCrouch == IsCrouching )
			return;

		if ( WantsToCrouch )
		{
			CharacterController.Height = DuckHeight;
			IsCrouching = true;
			// Setzen Sie die Kameraposition auf die DuckHeight
			PlyCamera.Transform.Position = new Vector3( PlyCamera.Transform.Position.x, PlyCamera.Transform.Position.y, DuckHeight );
		}
		else
		{
			if ( !CanUncrouch() )
				return;

			CharacterController.Height = StandHeight;
			IsCrouching = false;
			// Setzen Sie die Kameraposition auf die StandHeight
			PlyCamera.Transform.Position = new Vector3( PlyCamera.Transform.Position.x, PlyCamera.Transform.Position.y, StandHeight );
		}
	}

	protected virtual void DoMovementInput()
	{
		BuildWishVelocity();

		if ( CharacterController.IsOnGround && Input.Pressed( "Jump" ) )
		{
			CharacterController.Punch( Vector3.Up * 300f );
			SendJumpMessage();
		}

		MoveSpeed = CharacterController.Velocity.WithZ( 0 ).Length;



		if ( CharacterController.IsOnGround )
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0f );
			CharacterController.Accelerate( WishVelocity );
			CharacterController.ApplyFriction( GroundControl );

		}
		else
		{
			CharacterController.Velocity -= Gravity * Time.Delta * 0.5f;
			CharacterController.Accelerate( WishVelocity.ClampLength( 50f ) );
			CharacterController.ApplyFriction( Aircontrol );
		}

		CharacterController.Move();

		if ( !CharacterController.IsOnGround )
		{
			CharacterController.Velocity -= Gravity * Time.Delta * 0.5f;
			LastUngroundedTime = 0f;
		}
		else
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
			LastGroundedTime = 0f;
		}

		Transform.Rotation = Rotation.FromYaw( EyeAngles.ToRotation().Yaw() );
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		if ( Ragdoll.IsRagdolled || LifeState == LifeState.Dead )
			return;

		UpdateInteractions();

		if ( TimeSinceDamaged > 5f )
		{
			Health += HealthRegenPerSecond * Time.Delta;
			Health = MathF.Min( Health, MaxHealth );
		}
		if ( TimeSinceManaUsed > 5f )
		{
			Mana += ManaRegenPerSecond * Time.Delta;
			Mana = MathF.Min( Mana, MaxMana );
		}


		
		RegenerateStamina();
		DoCrouchingInput();
		DoMovementInput();

		if ( Input.MouseWheel.y > 0 )
			Weapons.Next();
		else if ( Input.MouseWheel.y < 0 )
			Weapons.Previous();

		if ( Input.Pressed( "use" ) )
		{
			var startPos = PlyCamera.Transform.Position;
			var direction = PlyCamera.Transform.Rotation.Forward;

			var endPos = startPos + direction * 10000f;
			var trace = Scene.Trace.Ray( startPos, endPos )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.UsePhysicsWorld()
				.UseHitboxes()
				.Run();

			IUse usable = null;

			if ( trace.Component.IsValid() )
				usable = trace.Component.Components.GetInAncestorsOrSelf<IUse>();

			if ( usable is not null )
			{
				usable.OnUse( GameObject.Id );
			}
		}


		var weapon = Weapons.Deployed;
		if ( !weapon.IsValid() ) return;


		if ( Input.Pressed( "Reload" ) )
		{
			weapon.ReloadAction();
		}

		if ( Input.Pressed( "Attack1" ) )
		{
			weapon.PrimaryAction();
		}

		if ( Input.Released( "Attack1" ) )
		{
			weapon.PrimaryActionRelease();
		}

		if ( Input.Pressed( "Attack2" ) )
		{
			weapon.SecondaryAction();
		}

		if ( Input.Released( "Attack2" ) )
		{
			weapon.SeccondaryActionRelease();

		}

		if ( !Game.IsPlaying )
			return;

		if ( IsProxy )
			return;
		
	}

	public void MoveToSpawnPoint()
	{
		if ( IsProxy )
			return;

		var spawnpoints = Scene.GetAllComponents<SpawnPoint>();
		var randomSpawnpoint = Game.Random.FromList( spawnpoints.ToList() );

		Transform.Position = randomSpawnpoint.Transform.Position;
		Transform.Rotation = Rotation.FromYaw( randomSpawnpoint.Transform.Rotation.Yaw() );
		EyeAngles = Transform.Rotation;
	}

	private void BuildWishVelocity()
	{
		var rotation = EyeAngles.ToRotation();

		WishVelocity = rotation * Input.AnalogMove;
		WishVelocity = WishVelocity.WithZ( 0f );

		if ( !WishVelocity.IsNearZeroLength )
			WishVelocity = WishVelocity.Normal;


		if ( IsCrouching )
			WishVelocity *= 64f;
		else if ( IsRunning )

			WishVelocity *= PlayerRunSpeed;
		else
			WishVelocity *= PlayerWalkSpeed;
	}

	[Broadcast]
	private void SendKilledMessage( Guid attackerId )
	{
		var attacker = Scene.Directory.FindByGuid( attackerId );
		OnKilled( attacker );
	}



	[Broadcast]
	private void SendJumpMessage()
	{
		foreach ( var animator in Animators )
		{
			animator.TriggerJump();
		}

		OnJump?.Invoke();
	}



}

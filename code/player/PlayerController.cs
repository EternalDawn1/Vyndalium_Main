using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Security.Permissions;
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
	[Property]public int DefaultAmmo { get; set; }
	private float crouchProgress = 0f;
	private const float crouchSpeed = 5f;
	public ItemEquipment EquippedItem => Inventory?.GetEquippedHandItem();
	private Vector3 targetCameraPosition;
	[Property] public AmmoContainer Ammo { get; set; } 
	public BaseGun CurrentWeapon { get; set; }
	[Property] public CharacterController CharacterController { get; set; }
	[Property] public MoveHelper MoveHelper { get; set; }
	[Property] public GameObject Head { get; set; }
	[Property] public GameObject Eye { get; set; }
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] public SoundEvent HurtSound { get; set; }
	[Property] public SoundEvent HurtLowHP { get; set; }
	[Property] public SoundEvent HurtMidHP { get; set; }
	
	[Property] public float StandHeight { get; set; } = 64f;
	[Property] public float DuckHeight { get; set; } = 29f;
	[Property] public Action OnJump { get; set; }
	[Property] public bool isJumping { get; set; }
	[Sync] public LifeState LifeState { get; private set; } = LifeState.Alive;
	[Sync] public Angles EyeAngles { get; set; }
	[Sync] public bool IsAiming { get; set; }
	[Sync] public bool IsRunning { get; set; }
	[Sync] public bool IsCrouching { get; set; }
	[Sync] public int Deaths { get; private set; }
	[Sync] public int Kills { get; private set; }
	public string DisplayName { get; set; }
	
	public TimeSpan Playtime { get; set; }
	

	public string GuildName { get; set; }
	public string Location { get; set; }
	public List<string> ActiveQuests { get; set; }
	public List<string> Achievements { get; set; }
	public Vector3 WishVelocity { get; private set; }
	private Vector3 SieatOffset => new Vector3( 0f, 0f, -40f );
	private RealTimeSince LastGroundedTime { get; set; }
	private RealTimeSince LastUngroundedTime { get; set; }
	private RealTimeSince TimeSinceDamaged { get; set; }
	private RealTimeSince TimeSinceManaUsed { get; set; }

	private static bool isFirstSpawn = true;
	[Property]public int MAX_BACKPACK_SLOTS = 20;

	private bool WantsToCrouch { get; set; }
	private Angles Recoil { get; set; }
	[Property] public float GroundControl { get; private set; } = 4.0f;
	[Property] public float Aircontrol { get; private set; } = 0.1f;
	public static bool DebugCamera { get; set; } = false;
	[Property] public float MouseSensitivity { get; set; } = 1.0f;
	[Property] public float DefaultFov { get; set; } = 90f;

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
	bool _blockMouseClicks = false;
	/// <summary>
	/// Block mouse clicks
	/// </summary>
	[Sync]
	public bool BlockMouseClicks
	{
		get => _blockMouseClicks;
		set => _blockMouseClicks = value;
	}

	bool _blockInputs = false;

	/// <summary>
	/// Block inputs (Like WASD, Pissing, Left/Right click)
	/// </summary>
	[Sync]
	public bool BlockInputs
	{
		get => BlockMovements || _blockInputs;
		set
		{
			_blockInputs = value;
			if ( _blockInputs )
			{
				StopMovement();
			}
		}
	}
	private void SetPlayerMovement( Vector3 movement )
	{
		BuildWishVelocity();

		if ( BlockInputs )
		{
			// Setze die Geschwindigkeit des Spielers auf null
			if ( CharacterController != null )
			{
				CharacterController.Velocity = Vector3.Zero;
			}
			else
			{
				// Loggen Sie eine Warnung oder werfen Sie eine Ausnahme, um das Problem zu debuggen
			
			}
			return;
		}

		// Normale Bewegungslogik hier...
	}

	private void StopMovement()
	{
		// Setze die Eingaben des Spielers zurück
		Input.ClearActions();

		// Stoppe die Bewegung des Spielers
		SetPlayerMovement( Vector3.Zero );
	}
	public void ForceHoldType( HoldType type, float time )
	{
		_targetHoldType = type;
		_resetHoldType = time;
	}
	private HoldType _targetHoldType;
	private TimeUntil _resetHoldType;

	[Sync] public HoldType HoldType { get; set; } = HoldType.Idle;

	public int GetLevel()
	{
		return Level;
	}
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
	
	public void AddVyndalium(int vyndaliumPointsToAdd)
	{
		Sandbox.Services.Stats.Increment( "vyndalium_count1", vyndaliumPointsToAdd );
		
	}


	
	public void OnZombieKilled()
	{
		Sandbox.Services.Stats.Increment( "npc", 1 );

		Sandbox.Services.Stats.Increment( "npc_killed", 1 );

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

	public void GiveVyndalium( int amount )
	{
		Vyndalium += amount;
		

	}
	
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
		if ( IsProxy )
			return;
		foreach ( var item in Inventory.EquippedItems )
		{
			if ( item is ItemEquipment equipment )
			{
				
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
			MaxStamina = 100f;
			MaxMana = 100f;
			PlayerRunSpeed = 190f;
			PlayerWalkSpeed = 120f;
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
		if ( IsProxy )
			return;
		float originalHealth = MaxHealth;
		float endTime = Time.Now + duration;

		// Erhöhe die Gesundheit des Spielers über die Dauer hinweg
		while ( Time.Now < endTime )
		{
			Health = Math.Min( MaxHealth, Health + regenAmount * Time.Delta );
			await Task.Delay( 1000 / 60 );
		}


	}
	public BaseGun ActiveWeapon { get; set; }

	[Broadcast]
	public void TakeDamage( DamageType type, Single amount, Vector3 hitPosition, Vector3 hitDirection, Guid attackerId, Guid playerId )
	{
		if ( IsProxy )
			return;
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
				Sound.Play( HurtSound, WorldPosition );
			}
		}

		if ( IsProxy )
			return;

		TimeSinceDamaged = 0f;
		Health = MathF.Max( Health - amount, 0f );

		if ( Health <= 0f )
		{
			LifeState = LifeState.Dead;
			BlackScreen( 3f, 1.5f, 1f );
			ActiveWeapon?.StopReloadSound();
			Ragdoll.Ragdoll( hitPosition, hitDirection );
			SendKilledMessage( attackerId );


		}
	}

	protected virtual bool CanUncrouch()
	{
		if ( IsProxy )
			return true;
		if ( !IsCrouching ) return true;
		if ( LastUngroundedTime < 0.2f ) return false;

		var tr = CharacterController.TraceDirection( Vector3.Up * DuckHeight );
		return !tr.Hit;
	}

	protected virtual void OnKilled(GameObject attacker)
	{
		if (IsProxy)
			return;

		// Stop all sounds and reset states
		

		if (Weapons.Deployed != null && Weapons.Deployed.IsValid())
		{
			Weapons.Deployed.Holster();
		}

		RespawnAsync(3f);
		Deaths++;
	}
	

	protected override void OnAwake()
	{
		if ( IsProxy )
			return;

		if (AmmoContainer == null)
		{
			AmmoContainer = Components.GetOrCreate<AmmoContainer>();
		}	
		if(Inventory == null)
		{
			Inventory = Components.GetOrCreate<Inventory>( FindMode.EverythingInSelfAndDescendants );
		}
		

		
		if(ModelRenderer == null)
		{
			ModelRenderer = Components.Get<SkinnedModelRenderer>();
		}
		Collider = Components.Get<BoxCollider>( FindMode.EverythingInSelfAndDescendants );

		if(CharacterController == null)
		{
			CharacterController = Components.Get<CharacterController>();
		}

		CharacterController.IgnoreLayers.Add( "player" );

		Ragdoll = Components.GetInDescendantsOrSelf<RagdollController>();

		if ( CharacterController.IsValid() )
		{
			CharacterController.Height = StandHeight;
		}
		

		

		ResetViewAngles();
		


	}

	protected override void OnStart()
	{
		

		if ( !IsProxy )
		{
			BlackScreen( 0f, 2f, 3f );
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


	[ConCmd("kill_player")]
	public  void KillPlayer()
	{
		int Amount = 100;
		var playerInside = Player.Local;
		playerInside.TakeDamage(DamageType.Bullet, Amount, new Vector3(), new Vector3(), new Guid(), GameObject.Id);
		Log.Info("Player has been killed.");
		
	}



	private void UpdateWeaponModelVisibility()
	{
		if(IsProxy) 
		return;
		var deployedWeapon = Weapons.Deployed;
		foreach ( var weapon in Weapons.All )
		{
			var modelRenderer = weapon.Components.Get<ModelRenderer>();
			var itemComponent = weapon.Components.Get<ItemComponent>();

			if ( modelRenderer != null && itemComponent != null )
			{
				// Überprüfen, ob die Waffe ein Item ist und ob sie die aktuell eingesetzte Waffe ist
				if ( itemComponent.IsItem )
				{
					modelRenderer.Enabled = weapon == deployedWeapon;
					weapon.GameObject.Enabled = false;
				}
				else
				{
					modelRenderer.Enabled = false;
					// Deaktivieren des GameObjects im weaponbone
					weapon.GameObject.Enabled = false;
				}
			}
		}
	}



	private void UpdateModelVisibility()
	{
		if (!ModelRenderer.IsValid())
			return;

		if (IsProxy) PlyCamera.Enabled = false;

		UpdateWeaponModelVisibility(); // Neue Methode aufrufen

		var shadowRenderer = ShadowAnimator.Components.Get<SkinnedModelRenderer>(true);
		var skinnedModelRenderer = ModelRenderer.Components.Get<SkinnedModelRenderer>(true);

		var hasViewModel = Weapons.Deployed.IsValid() && Weapons.Deployed.HasViewModel;
		var clothing = ModelRenderer.Components.GetAll<ClothingComponent>(FindMode.EverythingInSelfAndDescendants);

		if (hasViewModel)
		{
			shadowRenderer.Enabled = false;
			ModelRenderer.Enabled = true;

			ModelRenderer.Enabled = Ragdoll.IsRagdolled;
			ModelRenderer.RenderType = Sandbox.ModelRenderer.ShadowRenderType.On;
			foreach (var c in clothing)
			{
				c.ModelRenderer.Enabled = Ragdoll.IsRagdolled;
				c.ModelRenderer.RenderType = Sandbox.ModelRenderer.ShadowRenderType.On;
			}

			// SkinnedModelRenderer aktivieren
			if (skinnedModelRenderer != null)
			{
				skinnedModelRenderer.Enabled = false;
			}

			return;
		}

		ModelRenderer.SetBodyGroup("head", IsProxy ? 0 : 1);
		ModelRenderer.Enabled = true;

		if (Ragdoll.IsRagdolled)
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

		foreach (var c in clothing)
		{
			c.ModelRenderer.Enabled = false;
			if (c.Category is Clothing.ClothingCategory.Hair or Clothing.ClothingCategory.Facial or Clothing.ClothingCategory.Hat)
			{
				c.ModelRenderer.RenderType = IsProxy ? Sandbox.ModelRenderer.ShadowRenderType.On : Sandbox.ModelRenderer.ShadowRenderType.ShadowsOnly;
			}
		}

		if (!PlyCamera.IsValid() || !Eye.IsValid())
			return;

		var cameraPosition = PlyCamera.WorldPosition;
		var cameraDirection = PlyCamera.WorldRotation.Forward;
		var fieldOfView = DefaultFov;
		IEnumerable<SceneObject> sceneObjects = GetSceneObjects(); // Annahme: PlyCamera hat eine Eigenschaft FieldOfView

		foreach (var obj in sceneObjects) // Pseudocode: Iteriere über alle Objekte in der Szene
		{
			var directionToObject = (obj.Transform.Position - cameraPosition).Normal;
			var angleToObject = Vector3Extensions.AngleBetween(cameraDirection, directionToObject);

			if (angleToObject <= fieldOfView / 2)
			{
				// Das Objekt ist im Sichtfeld der Kamera
				obj.SetVisibility(true); // Pseudocode: Setze die Sichtbarkeit des Objekts
			}
			else
			{
				// Das Objekt ist außerhalb des Sichtfelds der Kamera
				obj.SetVisibility(false); // Pseudocode: Setze die Sichtbarkeit des Objekts
			}
		}
	}
	public IEnumerable<SceneObject> GetSceneObjects()
	{
		// Implementierung abhängig von der spezifischen Logik Ihrer Anwendung
		return new List<SceneObject>(); // Beispielrückgabe
	}
	public void StartSwingAnimation()
	{
		if ( GameObject == null )
		{
			Log.Warning( "GameObject is null." );
			return;
		}

		var animator = GameObject.Components.Get<CitizenAnimationHelper>();
		if ( animator == null )
		{
			// Fügen Sie die CitizenAnimationHelper-Komponente hinzu, falls sie nicht vorhanden ist
			animator = GameObject.Components.Create<CitizenAnimationHelper>();
			Log.Info( "CitizenAnimationHelper-Komponente hinzugefügt." );
		}

		if ( animator != null  )
		{
			animator.Target.Set( "b_attack", true );
			animator.HoldType = CitizenAnimationHelper.HoldTypes.Swing;
			Log.Info( "Swing animation started." );
		}
		else
		{
			Log.Warning( "No animator or animator target found." );
		}
	}





	protected override void OnPreRender()
	{
		base.OnPreRender();

		if (!Scene.IsValid() || !PlyCamera.IsValid())
			return;

		UpdateModelVisibility();

		if (IsProxy)
			return;

		if (!Eye.IsValid())
			return;

		if (Ragdoll.IsRagdolled)
		{
			PlyCamera.WorldPosition = PlyCamera.WorldPosition.LerpTo(Eye.WorldPosition, Time.Delta * 32f);
			PlyCamera.WorldRotation = Rotation.Lerp(PlyCamera.WorldRotation, Eye.WorldRotation, Time.Delta * 16f);
			return;

		}




		if (!IsProxy)

		{
			PlyCamera.LocalPosition = Vector3.Zero;
			var idealEyePos = Eye.WorldPosition;
			var headPosition = WorldPosition + Vector3.Up * CharacterController.Height;
			var headTrace = Scene.Trace.Ray(WorldPosition, headPosition)
				.UsePhysicsWorld()
				.IgnoreGameObjectHierarchy(GameObject)
				.WithAnyTags("solid")
				.Run();

			headPosition = headTrace.EndPosition - headTrace.Direction * 2f;

			var trace = Scene.Trace.Ray(headPosition, idealEyePos)
				.UsePhysicsWorld()
				.IgnoreGameObjectHierarchy(GameObject)
				.WithAnyTags("solid")
				.Radius(2f)
				.Run();

			var deployedWeapon = Weapons.Deployed;
			var hasViewModel = deployedWeapon.IsValid() && deployedWeapon.HasViewModel;

			if (hasViewModel)
				PlyCamera.WorldPosition = Head.WorldPosition;
			else
				PlyCamera.WorldPosition = trace.Hit ? trace.EndPosition : idealEyePos;

			if (SicknessMode)
				PlyCamera.WorldRotation = Rotation.LookAt(Eye.WorldRotation.Left) * Rotation.FromPitch(-10f);
			else
				PlyCamera.WorldRotation = EyeAngles.ToRotation() * Rotation.FromPitch(-10f);


			if (IsCrouching && hasViewModel)
			{
				PlyCamera.WorldPosition = PlyCamera.WorldPosition + SieatOffset;
			}
		}
	}
		bool isLowHealthSoundPlaying = false;
	public bool SicknessMode { get; set; }
	bool isMidHealthSoundPlaying = false;
	public bool IsSwinging { get; set; }
	private float swingCooldown = 1.0f; // Cooldown-Zeit in Sekunden
	private float lastSwingTime = -1.0f;
	private float swingDuration = 0.5f; // Dauer der Swing-Animation in Sekunden
	private float swingStartTime = -1.0f;
	private Vector3 targetCrouchPosition;
	private float crouchDuration = 0.225f; // Dauer des Crouchens in Sekunden
	private float crouchTimer = 0.0f;
	protected override void OnUpdate()
	{
		UpdateModelVisibility();
		if (IsProxy)
		{
			return;
		}

		if ( Ragdoll.IsRagdolled || LifeState == LifeState.Dead )
			return;
		

		if ( !Eye.IsValid() )
			return;

		if ( Ragdoll.IsRagdolled )
		{
			PlyCamera.WorldPosition = PlyCamera.WorldPosition.LerpTo( Eye.WorldPosition, Time.Delta * 32f );
			PlyCamera.WorldRotation = Rotation.Lerp( PlyCamera.WorldRotation, Eye.WorldRotation, Time.Delta * 16f );
			return;

		}
		if ( Input.Down( "attack1" ) && Time.Now >= lastSwingTime + swingCooldown )
		{
			if ( EquippedItem == null )
			{
				
			}
			else if ( EquippedItem.Slot == EquipSlot.Hand && EquippedItem.IsMelee )
			{
				Log.Info( "Attack1 pressed" );
				StartSwingAnimation();
				IsSwinging = true;
				lastSwingTime = Time.Now;
				swingStartTime = Time.Now;
			}
		}

		// Überprüfen, ob die Animationsdauer abgelaufen ist
		if ( IsSwinging && Time.Now >= swingStartTime + swingDuration )
		{
			Log.Info( "Swing animation completed" );
			IsSwinging = false;
		}
		if (Player.Local == this)
		{
			PlyCamera.WorldPosition = Eye.WorldPosition;
			PlyCamera.WorldRotation = Eye.WorldRotation;
		}

		for ( int i = activeStatusEffects.Count - 1; i >= 0; i-- ) 
		{
			var effect = activeStatusEffects[i];

			if ( effect is BurnEffect burnEffect )
			{
				if ( burnEffect.Duration > 0 )
				{
					Health = Math.Max( 0, Health - burnDamagePerSecond * Time.Delta );
					burnEffect.Duration -= Time.Delta;
					
					

					if ( Health <= 0 )
					{
						
						activeStatusEffects.RemoveAt( i );
					}
				}
				else
				{
					
					activeStatusEffects.RemoveAt( i );
				}
			}
		}


		if ( !Scene.IsValid() || !PlyCamera.IsValid() )
			return;

		


		if ( !IsProxy )

		{
			PlyCamera.LocalPosition = Vector3.Zero;
			var idealEyePos = Eye.WorldPosition;
			var headPosition = WorldPosition + Vector3.Up * CharacterController.Height;
			var headTrace = Scene.Trace.Ray( WorldPosition, headPosition )
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
				PlyCamera.WorldPosition = Head.WorldPosition;
			else
				PlyCamera.WorldPosition = trace.Hit ? trace.EndPosition : idealEyePos;


			PlyCamera.WorldRotation = EyeAngles.ToRotation() * Rotation.FromPitch( -10f );



			if ( IsCrouching && hasViewModel )
			{
				targetCrouchPosition = PlyCamera.WorldPosition + SieatOffset;
				crouchTimer += Time.Delta;
				PlyCamera.WorldPosition = Vector3.Lerp( PlyCamera.WorldPosition, targetCrouchPosition, crouchTimer / crouchDuration );
			}
			else
			{
				crouchTimer = 0.0f; // Reset Timer wenn nicht crouching
			}
			
		}



		if ( !IsProxy )
		{
			var angles = EyeAngles.Normal;
			angles += Input.AnalogLook * 2;
			angles += Recoil * Time.Delta;
			angles.pitch = angles.pitch.Clamp( -89f, 89.9f );


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
					Sound.Play( HurtLowHP, Player.Local.Head.WorldPosition );
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
					Sound.Play( HurtMidHP, Player.Local.WorldPosition );
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
			if ( !IsSwinging )
			{
				animator.HoldType = weapon.IsValid() ? weapon.HoldType : CitizenAnimationHelper.HoldTypes.None;
			}
			else if ( IsSwinging )
			{
				animator.HoldType = CitizenAnimationHelper.HoldTypes.Swing;
				
			}
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
		if ( IsProxy )
			return;

		WantsToCrouch = CharacterController.IsOnGround && Input.Down( "Duck" );

		if ( WantsToCrouch == IsCrouching )
			return;

		if ( WantsToCrouch )
		{
			crouchProgress = Lerp( crouchProgress, 1f, Time.Delta * crouchSpeed );
		}
		else
		{
			if ( !CanUncrouch() )
				return;

			crouchProgress = Lerp( crouchProgress, 0f, Time.Delta * crouchSpeed );
		}

		CharacterController.Height = Lerp( StandHeight, DuckHeight, crouchProgress );
		targetCameraPosition = new Vector3( PlyCamera.WorldPosition.x, PlyCamera.WorldPosition.y, Lerp( StandHeight, DuckHeight, crouchProgress ) );
		IsCrouching = crouchProgress > 0.5f;
	}
	public static float Lerp( float a, float b, float t )
	{
		return a + (b - a) * t;
	}

	protected virtual void DoMovementInput()
	{
		if (IsProxy)
			return;
		if (BlockInputs)
		{
			return;
		}
		if (isFrozen)
		{
			return;
		}

		BuildWishVelocity();


		if (CharacterController.IsOnGround && Input.Pressed("Jump") && TryJump())
		{
			CharacterController.Punch(Vector3.Up * 300f);
			SendJumpMessage();
		}

		MoveSpeed = CharacterController.Velocity.WithZ(0).Length;

		if (CharacterController.IsOnGround)
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ(0f);
			CharacterController.Accelerate(WishVelocity);
			CharacterController.ApplyFriction(GroundControl);
		}
		else
		{
			CharacterController.Velocity -= Gravity * Time.Delta * 0.5f;
			CharacterController.Accelerate(WishVelocity.ClampLength(50f));
			CharacterController.ApplyFriction(Aircontrol);
		}

		CharacterController.Move();

		

		if (!CharacterController.IsOnGround)
		{
			CharacterController.Velocity -= Gravity * Time.Delta * 0.5f;
			LastUngroundedTime = 0f;
		}
		else
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ(0);
			LastGroundedTime = 0f;
		}

		WorldRotation = Rotation.FromYaw(EyeAngles.ToRotation().Yaw());
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

		if ( Input.Pressed( "use3" ) )
		{
			var startPos = PlyCamera.WorldPosition;
			var direction = PlyCamera.WorldRotation.Forward;

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

		WorldPosition = randomSpawnpoint.WorldPosition;
		WorldRotation = Rotation.FromYaw( randomSpawnpoint.WorldRotation.Yaw() );
		EyeAngles = WorldRotation;
	}
	public void Move()
	{
		if ( IsProxy )
			return;
		// Aktualisiere die Bewegungslogik des Spielers
		BuildWishVelocity();

		Log.Info( "Player is moving" );
	}

	private void BuildWishVelocity()
	{
		if (IsProxy)
			return;
		if (isFrozen)
		{
			Log.Info("Player cannot build wish velocity while frozen");
			return;
		}

		var moveInput = Input.AnalogMove;

		// Log the input values for debugging
		
		// Set WishVelocity to zero if there is no movement input
		if (moveInput.IsNearlyZero())
		{
			WishVelocity = Vector3.Zero;
		}
		else
		{
			var rotation = EyeAngles.WithRoll(0f).ToRotation();
			WishVelocity = rotation * moveInput;
			WishVelocity = WishVelocity.WithZ(0f);

			if (!WishVelocity.IsNearZeroLength)
				WishVelocity = WishVelocity.Normal;

			if (IsCrouching)
				WishVelocity *= 64f;
			else if (IsRunning)
				WishVelocity *= PlayerRunSpeed;
			else
				WishVelocity *= PlayerWalkSpeed;
		}

		// Log the calculated WishVelocity for debugging
	
	}

	[Broadcast]
	private void SendKilledMessage( Guid attackerId )
	{
		if ( IsProxy )
			return;
		var attacker = Scene.Directory.FindByGuid( attackerId );
		OnKilled( attacker );
	}



	[Broadcast]
	private void SendJumpMessage()
	{
		if ( IsProxy )
			return;
		foreach ( var animator in Animators )
		{
			animator.TriggerJump();
			isJumping = true;
		}

		OnJump?.Invoke();
		isJumping = false;
	}



}

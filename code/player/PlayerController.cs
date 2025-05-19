using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Permissions;
using GeneralGame.HUD;
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
	[Property] public int DefaultAmmo { get; set; }
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
	// Add a property to track respawn attempts
	[Sync, Property]
	public int RespawnAttempts { get; set; } = 3;
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

	private static bool isFirstSpawn { get; set; } = true;

	private Angles originalEyeAngles; // Speichert die ursprünglichen EyeAngles
	private bool WantsToCrouch { get; set; }
	private Angles Recoil { get; set; }
	[Property] public float GroundControl { get; private set; } = 4.0f;
	[Property] public float Aircontrol { get; private set; } = 0.1f;
	public static bool DebugCamera { get; set; } = false;
	[Property] public float MouseSensitivity { get; set; } = 1.0f;
	[Property] public float DefaultFov { get; set; } = 100f;

	[Property] public bool ThirdPersonEnabled { get; set; } = true;


	// In der Player-Klasse, füge diese Eigenschaften hinzu
	[Property] public float MaxCameraDistance { get; set; } = 150f; // Maximale Kameradistanz
	[Property] public float MinCameraDistance { get; set; } = 20f;  // Minimale Kameradistanz
	[Property] public float CameraZoomSpeed { get; set; } = 10f;     // Zoom-Geschwindigkeit
	[Property] public float FirstPersonThreshold { get; set; } = 20f; // Schwellenwert für First-Person
	private float currentCameraDistance = 70f;                      // Aktuelle Kameradistanz
	public bool IsAimingCamera { get; set; } = false;
	public float DefaultCameraDistance { get; set; } = 150f;  // Standardwert für die Kameradistanz
	public float AimingCameraDistance { get; set; } = 50f;

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
			ModelRenderer.SetBodyGroup( "torso", _hideBodygroups.HasFlag( HiddenBodyGroup.Torso ) ? 0 : 0 );
			ModelRenderer.SetBodyGroup( "hands", _hideBodygroups.HasFlag( HiddenBodyGroup.Hands ) ? 0 : 0 );
			ModelRenderer.SetBodyGroup( "legs", _hideBodygroups.HasFlag( HiddenBodyGroup.Legs ) ? 0 : 0 );
			ModelRenderer.SetBodyGroup( "feet", _hideBodygroups.HasFlag( HiddenBodyGroup.Feet ) ? 0 : 0 );
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
	[Rpc.Broadcast]
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

	[Rpc.Broadcast]
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

	public void AddVyndalium( int vyndaliumPointsToAdd )
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
	private List<Angles> recoilPattern = new List<Angles>
	{
		new Angles(-0.02f, 0f, 0f),  // Weniger nach oben
		new Angles(-0.05f, 0.25f, 0f), // Weniger nach oben rechts
		new Angles(-0.05f, -0.25f, 0f), // Weniger nach oben links
		new Angles(-0.1f, 0.15f, 0f),  // Weniger nach oben leicht rechts
		new Angles(-0.1f, -0.15f, 0f)  // Weniger nach oben leicht links
	};


	private async void ApplyCameraShake( float intensity, float duration )
	{
		if ( IsProxy ) return;

		var shakeEndTime = Time.Now + duration;
		var elapsedTime = 0f;

		while ( Time.Now < shakeEndTime )
		{
			// Berechne den Fortschritt der Zeit (0 bis 1)
			elapsedTime += Time.Delta;
			float progress = elapsedTime / duration;

			// Verwende eine Sinuskurve für sanftes Wackeln
			var shakeOffset = new Angles(
				MathF.Sin( progress * MathF.PI * 2 ) * intensity, // Sanftes Wackeln auf der Pitch-Achse
				Game.Random.Float( -intensity * 0.5f, intensity * 0.5f ), // Leichtes Wackeln auf der Yaw-Achse
				0f // Kein Wackeln auf der Roll-Achse
			);

			// Wende die Rotation auf die Kamera an
			PlyCamera.WorldRotation *= Rotation.From( shakeOffset );

			// Warte für die nächste Iteration (~144 FPS)
			await Task.Delay( 4 );
		}

		// Stelle sicher, dass die Kamera am Ende wieder stabil ist
		PlyCamera.WorldRotation = PlyCamera.WorldRotation.Normal;
	}


	private int currentRecoilIndex = 0;
	private float recoilResetSpeed = 15f; // Geschwindigkeit, mit der das Recoil zurückgesetzt wird
	private float recoilRecoveryTime = 0.2f; // Zeit, bis das Recoil zurückgesetzt wird
	private RealTimeSince timeSinceLastShot;

	[Rpc.Broadcast]
	public void ApplyRecoil( Angles recoil )
	{
		if ( IsProxy ) return;

		// Skaliere das Recoil, um die Stärke zu reduzieren
		float recoilScale = 0.3f; // Reduziert das Recoil auf 50%
		recoil *= recoilScale;

		// Speichere die ursprünglichen EyeAngles beim ersten Schuss
		if ( timeSinceLastShot > recoilRecoveryTime )
		{
			originalEyeAngles = EyeAngles;
		}

		// Wende das Recoil-Muster an
		if ( currentRecoilIndex < recoilPattern.Count )
		{
			Recoil += recoilPattern[currentRecoilIndex] * recoilScale;
			currentRecoilIndex++;
			ApplyCameraShake( 0.15f, 0.2f ); // Wende den Kamerawackeleffekt an
		}
		else
		{
			currentRecoilIndex = 0; // Zurücksetzen, wenn das Muster endet
		}

		// Setze die Zeit des letzten Schusses zurück
		timeSinceLastShot = 0;
	}
	public void ResetViewAngles()
	{
		if ( IsProxy ) return;
		var rotation = Rotation.Identity;
		EyeAngles = rotation.Angles().WithRoll( 0f );
	}


	public async void RespawnAsync( float seconds )
	{
		if ( IsProxy ) return;

		await Task.DelaySeconds( seconds );
		Respawn();
	}

	public Transform GetAttachment( string attachment, bool world = true )
	=> ModelRenderer.GetAttachment( attachment, world ) ?? global::Transform.Zero;
	[Rpc.Broadcast]
	public void InitialSpawn()
	{
		if ( IsProxy )
			return;

		// Initiale Spawn-Logik
		Weapons.GiveDefault();
		Ragdoll.Unragdoll();
		MoveToSpawnPoint();

		LifeState = LifeState.Alive;

		MaxHealth = 50f;
		Health = MaxHealth;
		MaxStamina = 100f;
		MaxMana = 100f;
		PlayerRunSpeed = 190f;
		PlayerWalkSpeed = 120f;

		isFirstSpawn = false; // Markiere den ersten Spawn als abgeschlossen

		StartHealthRegen( 500f, 5f );
	}


	[Rpc.Broadcast]
	public void Respawn()
	{
		if ( IsProxy )
			return;

		if ( RespawnAttempts <= 0 )
		{
			// Keine Respawn-Versuche mehr übrig
			InGameHud.Instance.ShowReturnToLobby = true;
			InGameHud.Instance.ShowRespawnOption = true;
			return;
		}



		// Respawn-Logik
		Weapons.GiveDefault();
		Ragdoll.Unragdoll();
		MoveToSpawnPoint();

		LifeState = LifeState.Alive;

		Health = MaxHealth;
		Stamina = MaxStamina;
		Mana = MaxMana;

		StartHealthRegen( 500f, 5f );
	}
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

	[Rpc.Broadcast]
	public void TakeDamage( DamageType type, Single amount, Vector3 hitPosition, Vector3 hitDirection, Guid attackerId, Guid playerId )
	{
		if ( IsProxy )
			return;
		if ( LifeState == LifeState.Dead )

			return;

		if ( type == DamageType.Bullet )
		{


			if ( HurtSound is not null )
			{
				Sound.Play( HurtSound, WorldPosition );
			}
			Local.ModelRenderer.Set( "hit", true );


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
	[ConCmd( "toggle_view" )]
	public static void ToggleView()
	{
		var player = Player.Local;
		if ( player == null ) return;

		// Kamera-Modus wechseln: 0 = First-Person, 1 = Third-Person-Left, 2 = Third-Person-Right
		player.CameraMode = (player.CameraMode + 1) % 3;

		player.PlyCamera.Enabled = true; // Stelle sicher, dass die Kamera aktiviert ist
		player.UpdateWeaponModelVisibility();


		// Versuche PlayerDresser zu bekommen, aber fahre auch fort, wenn es fehlschlägt
		var playerDresser = player.Components.GetInDescendantsOrSelf<PlayerDresser>();
		if ( playerDresser != null )
		{
			// Im First-Person-Modus (0) Kleidung ausblenden, sonst anzeigen
			playerDresser.UpdateClothingVisibility( player.CameraMode != 0 );

		}


		if ( player.CameraMode == 0 )
		{
			// First-Person-Ansicht
			player.ThirdPersonEnabled = false;
			player.PlyCamera.WorldPosition = player.Eye.WorldPosition;
			player.PlyCamera.WorldRotation = player.EyeAngles.ToRotation();

			// Viewmodel aktivieren
			if ( player.Weapons.Deployed != null )
			{
				player.Weapons.Deployed.CreateViewModel();
			}
		}
		else
		{
			// Third-Person-Ansicht
			player.ThirdPersonEnabled = true;

			// Viewmodel deaktivieren
			if ( player.Weapons.Deployed != null )
			{
				player.Weapons.Deployed.DestroyViewModel();
			}

			// Kamera-Position basierend auf dem Modus setzen
			if ( player.CameraMode == 1 )
			{
				// Third-Person-Left
				player.PlyCamera.WorldPosition = player.WorldPosition - player.EyeAngles.Forward * 150f + Vector3.Up * 50f + Vector3.Left * 20f;
			}
			else if ( player.CameraMode == 2 )
			{
				// Third-Person-Right
				player.PlyCamera.WorldPosition = player.WorldPosition - player.EyeAngles.Forward * 150f + Vector3.Up * 50f + Vector3.Right * 20f;
			}

			player.PlyCamera.WorldRotation = player.EyeAngles.ToRotation();
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
	[Rpc.Broadcast]

	protected virtual void OnKilled( GameObject attacker )
	{
		if ( IsProxy )
			return;

		// Stop all sounds and reset states
		if ( Weapons.Deployed != null && Weapons.Deployed.IsValid() )
		{
			Weapons.Deployed.Holster();
		}
		if ( RespawnAttempts > 0 )
		{
			RespawnAttempts--; // Reduziert die Anzahl der Respawn-Versuche
			RespawnAsync( 3f ); // Spieler wird nach 3 Sekunden respawnt
		}
		else
		{
			// Keine Respawn-Versuche mehr übrig, zeige Rückkehr-zur-Lobby-Option
			InGameHud.Instance.ShowReturnToLobby = true;
			InGameHud.Instance.ShowRespawnOption = true;
			return;
		}
		Deaths++;




	}


	protected override void OnAwake()
	{
		if ( IsProxy )
			return;

		if ( AmmoContainer == null )
		{
			AmmoContainer = Components.GetOrCreate<AmmoContainer>();
		}
		if ( Inventory == null )
		{
			Inventory = Components.GetOrCreate<Inventory>( FindMode.EverythingInSelfAndDescendants );
		}



		if ( ModelRenderer == null )
		{
			ModelRenderer = Components.Get<SkinnedModelRenderer>();
		}
		ModelRenderer.OnFootstepEvent += OnFootstep;





		if ( CharacterController == null )
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

	private TimeSince lastStepped;
	private bool isLeftFoot = true;

	[Rpc.Broadcast]
	private void OnFootstep( SceneModel.FootstepEvent e )
	{


		if ( isFrozen )
		{

			return;
		}

		if ( lastStepped < (IsRunning ? 0.2f : 0.5f) )
		{

			return;
		}

		if ( !CharacterController.IsOnGround || CharacterController.Velocity.Length < 0.1f )
		{

			return;
		}

		var pos = WorldPosition + Vector3.Up * 1;
		var tr = Scene.Trace.Sphere( 1, pos + Vector3.Up * 100, pos + Vector3.Down * 100 )
			.WithoutTags( "trigger" )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		lastStepped = 0;
		var path = isLeftFoot ? tr.Surface.Sounds.FootLeft : tr.Surface.Sounds.FootRight;
		isLeftFoot = !isLeftFoot; // Wechseln Sie zwischen linkem und rechtem Fuß

		if ( string.IsNullOrEmpty( path ) )
		{

			return;
		}
		if ( !tr.Hit || tr.Surface == null )
		{

			return;
		}

		var sound = Sound.Play( path, tr.HitPosition + tr.Normal * 5 );
		sound.Volume *= e.Volume;

	}
	protected override void OnStart()
	{
		PlyCamera.Enabled = false;
		base.OnStart();

		if ( IsProxy )
			return;

		healthEffects = Scene.GetAllComponents<HealthEffects>().FirstOrDefault();



		if ( !IsProxy )
		{
			BlackScreen( 0f, 2f, 3f );
			InitialSpawn();
			RespawnAttempts = 3; // Setze die Anzahl der Respawn-Versuche auf 4

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


	}

	[ConCmd( "kill_player" )]
	public void KillPlayer()
	{
		int Amount = 100;
		var playerInside = Player.Local;
		playerInside.TakeDamage( DamageType.Bullet, Amount, new Vector3(), new Vector3(), new Guid(), GameObject.Id );
		Log.Info( "Player has been killed." );

	}

	// Neues Feld zum Speichern des letzten Kameramodus
	private int _lastCameraMode = -1; // -1 als Initialwert, damit beim ersten Aufruf eine Aktualisierung stattfindet

	private void UpdateWeaponModelVisibility()
	{
		if ( IsProxy ) return;

		// Nur aktualisieren, wenn sich der Kameramodus geändert hat
		if ( _lastCameraMode == CameraMode ) return;

		var deployedWeapon = Weapons.Deployed;
		if ( deployedWeapon == null || !deployedWeapon.IsValid() )
			return;

		// Setze die Sichtbarkeit basierend auf dem Kameramodus
		bool shouldBeVisible = CameraMode == 1 || CameraMode == 2;

		// Alternativer Ansatz für eine einzelne Komponente
		var renderer = deployedWeapon.Components.GetInDescendants<SkinnedModelRenderer>( true );
		if ( renderer != null && renderer.Enabled != shouldBeVisible )
			renderer.Enabled = shouldBeVisible;

		// Aktualisiere den gespeicherten Kameramodus
		_lastCameraMode = CameraMode;
	}
	[Rpc.Broadcast]
	private void UpdateModelVisibility()
	{
		if ( !ModelRenderer.IsValid() )
			return;
		if ( !PlyCamera.IsValid() )
			return;

		if ( !IsProxy )
			PlyCamera.Enabled = true;

		//UpdateWeaponModelVisibility(); // Neue Methode aufrufen

		var shadowRenderer = ShadowAnimator.Components.Get<SkinnedModelRenderer>( true );
		var skinnedModelRenderer = ModelRenderer.Components.Get<SkinnedModelRenderer>( true );

		var hasViewModel = Weapons.Deployed.IsValid() && Weapons.Deployed.HasViewModel;

		if ( hasViewModel )
		{
			shadowRenderer.Enabled = false;
			ModelRenderer.Enabled = true;

			ModelRenderer.Enabled = Ragdoll.IsRagdolled;
			ModelRenderer.RenderType = Sandbox.ModelRenderer.ShadowRenderType.On;

			// SkinnedModelRenderer aktivieren
			if ( skinnedModelRenderer != null )
			{
				skinnedModelRenderer.Enabled = false;

			}

			return;
		}

		ModelRenderer.SetBodyGroup( "head", IsProxy ? 1 : 0 );
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

		if ( !PlyCamera.IsValid() || !Eye.IsValid() )
			return;
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
			PlyCamera.WorldPosition = PlyCamera.WorldPosition.LerpTo( Eye.WorldPosition, Time.Delta * 32f );
			PlyCamera.WorldRotation = Rotation.Lerp( PlyCamera.WorldRotation, Eye.WorldRotation, Time.Delta * 16f );
			return;
		}


	}
	bool isLowHealthSoundPlaying = false;
	public bool SicknessMode { get; set; }
	bool isMidHealthSoundPlaying = false;
	public bool IsSwinging { get; set; }

	private Vector3 targetCrouchPosition;
	private float crouchDuration = 5f; // Dauer des Crouchens in Sekunden
	private float crouchTimer = 0.0f;
	[Sync]
	public int CameraMode { get; set; } = 1;
	// Füge diese Methode nach der bestehenden UpdateWeaponModelVisibility() Methode hinzu

	[Rpc.Broadcast]
	private void UpdateHoldTypeAnimation()
	{
		if ( IsProxy ) return;

		// Standard-HoldType (keine Waffe)
		int holdTypeValue = 0; // HoldType.None (0 in der Aufzählung)

		// Prüfen, ob eine Waffe ausgerüstet ist
		var deployedWeapon = Weapons.Deployed;
		if ( deployedWeapon != null && deployedWeapon.IsValid() )
		{
			// Wenn es ein ItemEquipment ist, hole den HoldType von dort
			var itemEquipment = deployedWeapon.Components.Get<ItemEquipment>();
			if ( itemEquipment != null )
			{
				holdTypeValue = (int)itemEquipment.HoldType;

				// Setze den Handedness-Parameter für Pistole
				if ( itemEquipment.HoldType == HoldType.Pistol )
				{
					// Setze für alle Animatoren
					foreach ( var animator in Animators )
					{
						if ( animator.Components.TryGet<SkinnedModelRenderer>( out var renderer ) )
						{
							renderer.Set( "holdtype_handedness", 0 );

						}
					}

					// Setze auch für das Hauptmodell
					ModelRenderer.Set( "holdtype_handedness", 0 );
				}
			}
			else
			{
				// Fallback für Waffen ohne ItemEquipment-Komponente
				if ( deployedWeapon is BaseGun gun )
				{
					if ( gun.IsPistol )
					{
						holdTypeValue = (int)HoldType.Pistol;

						// Setze für alle Animatoren
						foreach ( var animator in Animators )
						{
							if ( animator.Components.TryGet<SkinnedModelRenderer>( out var renderer ) )
							{
								renderer.Set( "holdtype_handedness", 0 );

							}
						}

						// Setze auch für das Hauptmodell
						ModelRenderer.Set( "holdtype_handedness", 0 );
					}
					else if ( gun.IsRifle ) holdTypeValue = (int)HoldType.Rifle;
					else if ( gun.IsShotgun ) holdTypeValue = (int)HoldType.Shotgun;
				}
				else if ( deployedWeapon.IsMelee ) holdTypeValue = (int)HoldType.Melee;
				{
					ModelRenderer.Set( "b_attack", true );
					
				}
			}
		}

		// Animation-Parameter setzen für alle Animatoren
		ModelRenderer.Set( "holdtype", holdTypeValue );
		foreach ( var animator in Animators )
		{
			if ( animator.Components.TryGet<SkinnedModelRenderer>( out var renderer ) )
			{
				renderer.Set( "holdtype", holdTypeValue );
			}
		}
	}
	public HealthEffects healthEffects;
	protected override void OnUpdate()
	{

		if ( !IsProxy )

			if ( Ragdoll.IsRagdolled || LifeState == LifeState.Dead )
				return;

		UpdateCameraZoom();
		UpdateWeaponModelVisibility();
		UpdateHoldTypeAnimation();

	

		if ( !Eye.IsValid() )
			return;

		if ( Ragdoll.IsRagdolled )
		{
			PlyCamera.WorldPosition = PlyCamera.WorldPosition.LerpTo( Eye.WorldPosition, Time.Delta * 32f );
			PlyCamera.WorldRotation = Rotation.Lerp( PlyCamera.WorldRotation, Eye.WorldRotation, Time.Delta * 16f );
			return;

		}

		//UpdateModelVisibility();
		//UpdateWeaponModelVisibility();











		if ( !IsProxy )
		{
			PlyCamera.LocalPosition = Vector3.Zero;

			switch ( CameraMode )
			{
				case 0: // First-Person-Ansicht
					PlyCamera.WorldPosition = Eye.WorldPosition;
					PlyCamera.WorldRotation = EyeAngles.ToRotation();

					// Setze FOV für First-Person auf 100
					PlyCamera.FieldOfView = 100f;

					var deployedWeapon = Weapons.Deployed;
					var hasViewModel = deployedWeapon.IsValid() && deployedWeapon.HasViewModel;

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
					break;

				case 2: // Third-Person-Left
					{
						// Berechne die Zielposition mit fester Distanz
						var offset = EyeAngles.ToRotation().Right * -30f; // Kamera links relativ zur Blickrichtung
						var desiredPosition = WorldPosition - EyeAngles.ToRotation().Forward * currentCameraDistance +
											  Vector3.Up * 50f + offset;

						// Sanfter FOV-Übergang statt Positionsänderung
						if ( IsAimingCamera != wasAimingLastFrame )
						{
							// Beginne den Übergang
							zoomTransitionTime = 0f;
							wasAimingLastFrame = IsAimingCamera;

							// Aktiviere oder deaktiviere den Vignette-Effekt
							if ( healthEffects != null )
							{
								if ( IsAimingCamera )
								{
									healthEffects.ApplyAimEffect();
								}
								else
								{
									healthEffects.RemoveAimEffect();
								}
							}
						}

						// Berechne das aktuelle FOV
						float targetFov = IsAimingCamera ? AimingFov : DefaultFov;

						// Berechne die interpolierte FOV während des Übergangs
						if ( zoomTransitionTime < zoomTransitionDuration )
						{
							zoomTransitionTime += Time.Delta;
							float progress = Math.Min( zoomTransitionTime / zoomTransitionDuration, 1.0f );
							progress = EaseInOutCubic( progress ); // Sanfte Beschleunigung und Verzögerung

							// Interpoliere das FOV
							PlyCamera.FieldOfView = MathX.LerpTo( PlyCamera.FieldOfView, targetFov, progress );
						}
						else
						{
							// Setze direkt das Ziel-FOV, wenn die Übergangszeit abgelaufen ist
							PlyCamera.FieldOfView = targetFov;
						}

						// Raycast von der Spielerposition zur gewünschten Kameraposition
						var leftTrace = Scene.Trace.Ray( WorldPosition + Vector3.Up * 50f, desiredPosition )
							.UsePhysicsWorld()
							.IgnoreGameObjectHierarchy( GameObject )
							.WithAnyTags( "solid" )
							.Run();

						// Wenn ein Hindernis erkannt wird, setze die Kamera auf die Trefferposition
						PlyCamera.WorldPosition = leftTrace.Hit ? leftTrace.EndPosition - leftTrace.Direction * 2f : desiredPosition;
						PlyCamera.WorldRotation = EyeAngles.ToRotation();
						break;
					}

				case 1: // Third-Person-Right
					{
						// Berechne die Zielposition mit fester Distanz
						var offset = EyeAngles.ToRotation().Right * 30f; // Kamera rechts relativ zur Blickrichtung
						var desiredPosition = WorldPosition - EyeAngles.ToRotation().Forward * currentCameraDistance +
											  Vector3.Up * 50f + offset;

						// Sanfter FOV-Übergang statt Positionsänderung
						if ( IsAimingCamera != wasAimingLastFrame )
						{
							// Beginne den Übergang
							zoomTransitionTime = 0f;
							wasAimingLastFrame = IsAimingCamera;

							// Aktiviere oder deaktiviere den Vignette-Effekt
							if ( healthEffects != null )
							{
								if ( IsAimingCamera )
								{
									healthEffects.ApplyAimEffect();
								}
								else
								{
									healthEffects.RemoveAimEffect();
								}
							}
						}

						// Berechne das aktuelle FOV
						float targetFov = IsAimingCamera ? AimingFov : DefaultFov;

						// Berechne die interpolierte FOV während des Übergangs
						if ( zoomTransitionTime < zoomTransitionDuration )
						{
							zoomTransitionTime += Time.Delta;
							float progress = Math.Min( zoomTransitionTime / zoomTransitionDuration, 1.0f );
							progress = EaseInOutCubic( progress ); // Sanfte Beschleunigung und Verzögerung

							// Interpoliere das FOV
							PlyCamera.FieldOfView = MathX.LerpTo( PlyCamera.FieldOfView, targetFov, progress );
						}
						else
						{
							// Setze direkt das Ziel-FOV, wenn die Übergangszeit abgelaufen ist
							PlyCamera.FieldOfView = targetFov;
						}

						// Raycast von der Spielerposition zur gewünschten Kameraposition
						var cameraTrace = Scene.Trace.Ray( WorldPosition + Vector3.Up * 50f, desiredPosition )
							.UsePhysicsWorld()
							.IgnoreGameObjectHierarchy( GameObject )
							.WithAnyTags( "solid" )
							.Run();

						PlyCamera.WorldPosition = cameraTrace.Hit ?
							cameraTrace.EndPosition - cameraTrace.Direction * 2f :
							desiredPosition;

						PlyCamera.WorldRotation = EyeAngles.ToRotation();
						break;
					}
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


			if ( timeSinceLastShot > recoilRecoveryTime )
			{
				Recoil = Recoil.LerpTo( Angles.Zero, Time.Delta * recoilResetSpeed );

				// Setze den Recoil-Index zurück, wenn das Recoil vollständig zurückgesetzt wurde
				if ( Recoil.IsNearlyZero( 0.01f ) )
				{
					currentRecoilIndex = 0;
					Recoil = Angles.Zero;
				}
			}

		}


		// Überprüfen Sie den Gesundheitszustand des Spielers
		// Check the player's health status
		float healthPercentage = Health / MaxHealth * 100;
		int healthRange = healthPercentage > 50 ? 2 : healthPercentage > 25 ? 1 : 0;

		switch ( healthRange )
		{
			case 0: // Gesundheit <= 25%
				if ( !isLowHealthSoundPlaying && HurtLowHP is not null && Player.Local?.Head != null )
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
				if ( !isMidHealthSoundPlaying && HurtMidHP is not null && Player.Local != null )
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
		//UpdateModelVisibility();



		foreach ( var animator in Animators )
		{

			animator.WithVelocity( CharacterController.Velocity );
			animator.WithWishVelocity( WishVelocity );
			animator.IsGrounded = CharacterController.IsOnGround;
			animator.MoveRotationSpeed = 0f;
			animator.DuckLevel = IsCrouching ? 1f : 0f;
			animator.WithLook( EyeAngles.Forward );
			animator.MoveStyle = (IsRunning && !IsCrouching) ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;
		}
	}
	
	public float AimingFov { get; set; } = 70f;  // Zoom-FOV beim Zielen
	private bool wasAimingLastFrame = false;
	private float zoomTransitionTime = 0f;
	private float zoomTransitionDuration = 0.3f;

	// Hilfsfunktion für weichen Übergang
	private float EaseInOutCubic( float t )
	{
		return t < 0.5 ? 4 * t * t * t : 1 - MathF.Pow( -2 * t + 2, 3 ) / 2;
	}
	[Rpc.Broadcast]
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
	[Rpc.Broadcast]
	protected virtual void DoMovementInput()
	{
		if ( IsProxy )
			return;
		if ( BlockInputs )
		{
			return;
		}
		if ( isFrozen )
		{
			return;
		}

		BuildWishVelocity();

		if ( CharacterController.IsOnGround && Input.Pressed( "Jump" ) && TryJump() )
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

			// Fußschritte erzeugen
			OnFootstep( new SceneModel.FootstepEvent { FootId = 0, Volume = 0.2f } );
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
		WorldPosition = CharacterController.LocalPosition;
		WorldRotation = Rotation.FromYaw( EyeAngles.ToRotation().Yaw() );
	}
	protected override void OnFixedUpdate()
	{
		if ( IsProxy || Ragdoll.IsRagdolled || LifeState == LifeState.Dead )
			return;

		if ( Input.Pressed( "Third" ) )
		{

			ToggleView();
		}

		UpdateInteractions();

		if ( TimeSinceDamaged > 5f )
		{
			Health = MathF.Min( Health + HealthRegenPerSecond * Time.Delta, MaxHealth );
		}

		if ( TimeSinceManaUsed > 5f )
		{
			Mana = MathF.Min( Mana + ManaRegenPerSecond * Time.Delta, MaxMana );
		}

		RegenerateStamina();
		DoCrouchingInput();
		DoMovementInput();

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

	}

	[Rpc.Broadcast]
	public void MoveToSpawnPoint()
	{
		if ( IsProxy )
			return;

		var spawnpoints = Scene.GetAllComponents<SpawnPoint>();
		if ( spawnpoints == null || !spawnpoints.Any() )
		{
			// Handle the case where there are no spawn points
			throw new InvalidOperationException( "No spawn points available." );
		}

		var randomSpawnpoint = Game.Random.FromList( spawnpoints.ToList() );

		WorldPosition = randomSpawnpoint.WorldPosition;
		WorldRotation = Rotation.FromYaw( randomSpawnpoint.WorldRotation.Yaw() );
		EyeAngles = WorldRotation;
	}
	[Rpc.Broadcast]
	private void BuildWishVelocity()
	{
		if ( IsProxy )
			return;
		if ( isFrozen )
		{
			Log.Info( "Player cannot build wish velocity while frozen" );
			return;
		}

		var moveInput = Input.AnalogMove;

		// Log the input values for debugging

		// Set WishVelocity to zero if there is no movement input
		if ( moveInput.IsNearlyZero() )
		{
			WishVelocity = Vector3.Zero;
		}
		else
		{
			var rotation = EyeAngles.WithRoll( 0f ).ToRotation();
			WishVelocity = rotation * moveInput;
			WishVelocity = WishVelocity.WithZ( 0f );

			if ( !WishVelocity.IsNearZeroLength )
			{
				WishVelocity = WishVelocity.Normal;

			}

			if ( IsCrouching )
				WishVelocity *= 64f;
			else if ( IsRunning )
				WishVelocity *= PlayerRunSpeed;
			else
				WishVelocity *= PlayerWalkSpeed;
		}

		// Log the calculated WishVelocity for debugging
	}

	[Rpc.Broadcast]
	private void SendKilledMessage( Guid attackerId )
	{
		if ( IsProxy )
			return;
		var attacker = Scene.Directory.FindByGuid( attackerId );
		OnKilled( attacker );
	}



	[Rpc.Broadcast]
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

	// Neue Eigenschaft für das Ziel der Kameradistanz und den Übergang
	private float targetCameraDistance = 70f;
	private float cameraZoomSmoothness = 10f; // Höherer Wert = schnellere Übergänge

	private void UpdateCameraZoom()
	{
		if ( IsProxy )
			return;

		// Hole den Wert des Mausrads
		Vector2 scrollDelta = Input.MouseWheel;

		// Wenn Mausradbewegung vorhanden, Zieldistanz anpassen
		if ( scrollDelta.Length >= 0.01f )
		{
			float wheelDelta = scrollDelta.y;

			// Zieldistanz anpassen
			targetCameraDistance -= wheelDelta * CameraZoomSpeed;

			// Begrenze die Zieldistanz auf Minimum und Maximum
			targetCameraDistance = targetCameraDistance.Clamp( MinCameraDistance, MaxCameraDistance );
		}

		// Sanfte Überblendung zur Zieldistanz
		currentCameraDistance = MathX.Lerp( currentCameraDistance, targetCameraDistance, Time.Delta * cameraZoomSmoothness );

		// Prüfe, ob ein Moduswechsel nötig ist
		bool switchToFirstPerson = currentCameraDistance < FirstPersonThreshold + 2f && CameraMode != 0; // Kleine Toleranz
		bool switchToThirdPerson = currentCameraDistance >= FirstPersonThreshold + 5f && CameraMode == 0; // Größere Toleranz beim Zurückwechseln

		if ( switchToFirstPerson )
		{
			SwitchToFirstPerson();
		}
		else if ( switchToThirdPerson )
		{
			SwitchToThirdPerson();
		}
	}

	// Neue Hilfsmethoden für bessere Lesbarkeit
	private void SwitchToFirstPerson()
	{
		CameraMode = 0;
		ThirdPersonEnabled = false;

		// Viewmodel aktivieren
		if ( Weapons.Deployed != null )
		{
			Weapons.Deployed.CreateViewModel();
		}

		// PlayerDresser aktualisieren
		var playerDresser = Components.GetInDescendantsOrSelf<PlayerDresser>();
		if ( playerDresser != null )
		{
			playerDresser.UpdateClothingVisibility( false );
		}

		
	}

	private void SwitchToThirdPerson()
	{
		CameraMode = 1; // Third Person Right
		ThirdPersonEnabled = true;

		// Viewmodel deaktivieren
		if ( Weapons.Deployed != null )
		{
			Weapons.Deployed.DestroyViewModel();
		}

		// PlayerDresser aktualisieren
		var playerDresser = Components.GetInDescendantsOrSelf<PlayerDresser>();
		if ( playerDresser != null )
		{
			playerDresser.UpdateClothingVisibility( true );
		}

		
	}



}

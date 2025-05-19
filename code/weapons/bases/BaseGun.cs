using Sandbox;
using System;
using System.Numerics;
using Sandbox; // Für GameObject, Transform, etc.
using Sandbox.Physics;
using Sandbox.UI; // Für TextRenderer, FaceThing, etc.
using System.Threading.Tasks; // Für asynchrone Methoden
using System.Collections.Generic; // Für Listen
using System.Linq; // Für LINQ-Abfragen



namespace GeneralGame;

public partial class BaseGun : WeaponComponent, IUse
{
	[Property, Category( "Weapon Type" )] public bool IsShotgun { get; set; } = false;
	[Property, Category( "Weapon Type" )] public bool IsRifle { get; set; } = false;
	[Property, Category( "Weapon Type" )] public bool IsPistol { get; set; } = false;
	[Property, Category( "Weapon Type" )] public new bool IsMelee { get; set; } = false;
	[Property, Category( "Weapon Type" ), Feature( "Weapon Properties" )] public bool IsMagicWeapon { get; set; }
	[Property, Category( "Weapon Type" )] public bool IsAuto { get; set; } = false;

	[Property, Category( "Damage" )] public DamageType DamageType { get; set; } = DamageType.Serious;
	[Property, Category( "Damage" ), Feature( "Weapon Properties" )] public float HitForce { get; set; } = 300;
	public bool isCriticalHit = false;

	[Property, Category( "Ammo" )] public AmmoType AmmoType { get; set; } = AmmoType.Pistol;
	[Property, Category( "Ammo" )] public int DefaultAmmo { get; set; } = 1;
	[Property, Category( "Ammo" ), Feature( "Weapon Properties" )] public int ClipSize { get; set; } = 15;
	[Sync, Property, Category( "Ammo" )] public int AmmoInClip { get; set; }
	[Sync, Category( "Ammo" )] public int MaxAmmo { get; set; }
	private int AmmoCount;

	[Property, Category( "Shooting" ), Feature( "Weapon Properties" )] public float Spread { get; set; } = 0.01f;
	[Property, Category( "Shooting" ), Range( 0, 0.1f, 10 ), Feature( "Weapon Properties" )] public float BulletSpeed { get; set; } = 1f;
	[Property, Category( "Shooting" )] public Angles Recoil { get; set; }

	[Property, Category( "Melee" )] public float MeleeRange { get; set; } = 1.5f;
	[Property, Category( "Melee" )] public float MeleeDamage { get; set; } = 10f;
	[Property, Category( "Melee" )] public float MeleeCooldown { get; set; } = 1f;
	public TimeUntil NextMeleeAttackTime { get; set; }


	[Property, Category( "Reload" )] public float ReloadTime { get; set; } = 2f;
	[Property, Category( "Reload" )] public float EmptyReloadTime { get; set; } = 2f;
	[Sync, Category( "Reload" )] public bool IsReloading { get; set; }
	public TimeUntil ReloadFinishTime { get; set; }

	[Property, Category( "Effects" )] public PrefabFile Trail { get; set; }
	[Property, Category( "Effects" )] public PrefabFile ImpactArea { get; set; }
	[Property, Category( "Effects" )] public PrefabFile MuzzleFlash { get; set; }
	[Property, Category( "Effects" )] public ParticleSystem ImpactEffect { get; set; }
	[Property, Category( "Effects" )] public LineRenderer lineRenderer { get; set; }

	[Property, Category( "Audio" ), Feature( "Weapon Properties" )] public SoundEvent FireSound { get; set; }
	[Property, Category( "Audio" )] public SoundEvent EmptyClipSound { get; set; }
	[Property, Category( "Audio" )] public SoundSequenceData ReloadSoundSequence { get; set; }
	[Property, Category( "Audio" )] public SoundSequenceData EmptyReloadSoundSequence { get; set; }
	public SoundSequence ReloadSound { get; set; }
	private bool IsSoundPlaying { get; set; } = false;
	private float SoundDuration { get; set; } = 0f;
	private const float EmptyClipSoundDuration = 1f;

	public bool IsFiering { get; set; } = false;
	public bool IsHeld { get; private set; }
	public bool IsEquipped { get; set; }
	public ItemComponent item { get; set; }
	// Neue Eigenschaften für das Aufladen hinzufügen
	[Property, Category( "Melee" )] public float ChargeTime { get; set; } = 1.0f; // Zeit zum vollständigen Aufladen
	public bool IsCharging { get; private set; } = false;
	public TimeUntil ChargeComplete { get; private set; }
	private bool FullyCharged => ChargeComplete && IsCharging;
	public void InitializeAmmo( AmmoContainer ammoContainer )
	{
		if ( ammoContainer != null )
		{
			AmmoCount = ammoContainer.GetAmmoCount( AmmoType );

		}
	}

	public void InitializeFireRate( ItemComponent itemComponent )
	{
		FireRate = itemComponent.FireRate;
		BulletSpeed = itemComponent.BulletSpeed;
	}


	public virtual void OnEquip( Player player )
	{

		if ( player == null || !player.IsValid() || player.AmmoContainer == null )
		{
			Log.Info( "Ungültiger Spieler oder AmmoContainer ist null." );
			return;
		}

		Log.Info( $"AmmoCount vor dem Ausrüsten: {player.AmmoContainer.GetAmmoCount( AmmoType.Rifle )}" );

		// Standardmunition abrufen und setzen



		var ammoToTake = Math.Min( ClipSize, player.AmmoContainer.GetAmmoCount( AmmoType.Rifle ) );
		AmmoInClip = ammoToTake;
		player.AmmoContainer.RemoveAmmo( AmmoType.Rifle, ammoToTake );

		Log.Info( $"AmmoCount nach dem Entfernen: {player.AmmoContainer.GetAmmoCount( AmmoType.Rifle )}" );

		IsEquipped = true;
		OnStart();
		OnDeployed();
	}

	public float CalculateDamageWithPlayerStats( Player player )
	{
		Random random = new Random();
		float baseDamage = random.Next( (int)player.MinAttackValue, (int)player.MaxAttackValue + 1 ); // Verwenden Sie die AttackValue des Spielers als Basis-Schaden
		float bonusDamage = baseDamage * (player.AttackPower / 45.0f);
		float magicBonus = baseDamage * (player.MagicPower / 45.0f);

		// Anpassen des Schadens basierend auf dem Schadenstyp der Waffe
		switch ( DamageType )
		{
			case DamageType.fire:
				// Feuerschaden könnte den Basis-Schaden erhöhen
				baseDamage *= 1.2f;
				break;
			case DamageType.ice:
				// Eisschaden könnte den magischen Bonus erhöhen
				magicBonus *= 1.5f;
				break;
				// Fügen Sie hier weitere Schadenstypen hinzu...
		}

		return baseDamage + bonusDamage + magicBonus;
	}
	public void IncreaseAmmo( int amount )
	{
		if ( !IsMelee )
		{
			AmmoCount += amount;
		}
	}
	public void DecreaseAmmo( int amount )
	{
		if ( !IsMelee )
		{
			if ( AmmoCount - amount >= 0 )
			{
				AmmoCount -= amount;
			}
			else
			{
				AmmoCount = 0;
			}
		}
	}
	public int GetAmmoCount()
	{
		return AmmoCount;
	}

	GameObject Hitprefab;


	protected override void OnStart()
	{
		if ( Player.Local != null && Player.Local.LifeState == LifeState.Dead )
		{
			StopAllActions();
		}

		// Standardmunition setzen, wenn sie nicht bereits gesetzt ist
		if ( AmmoCount == 0 )
		{
			if ( !IsMelee )
			{
				AmmoCount = DefaultAmmo;
			}
		}

		var hitPrefabFile = ResourceLibrary.Get<PrefabFile>( "prefabs/hitinfo.prefab" );
		if ( hitPrefabFile != null )
		{
			Hitprefab = SceneUtility.GetPrefabScene( hitPrefabFile );
		}
		else
		{
			// Log or handle the error appropriately
			Log.Info( "Error: Prefab 'prefabs/hitinfo.prefab' not found." );
		}

		if ( IsMelee )
		{

		}
		Components.GetOrCreate<Interactions>();

		Trail = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/firebullet_normal.prefab" );
		ImpactArea = ResourceLibrary.Get<PrefabFile>( "particles/prefabs/impact_area.prefab" );

		base.OnStart();
	}

	[Rpc.Broadcast]
	public virtual void OnUse( Guid pickerId )
	{

		var picker = Scene.Directory.FindByGuid( pickerId );
		if ( !picker.IsValid() ) return;

		var player = Scene.Directory.FindByGuid( pickerId ).Components.GetInDescendantsOrSelf<Player>();
		if ( !player.IsValid() ) return;



		if ( player.IsProxy )
			return;

		// Überprüfen, ob die Waffe bereits gehalten wird
		if ( IsHeld )
		{
			// Wenn ja, die Interaktion verhindern
			return;
		}


		if ( player.Weapons.Has( GameObject ) )
		{

			if ( !IsMelee )
			{
				var ammoToGive = DefaultAmmo - player.Ammo.Get( AmmoType );

				if ( ammoToGive > 0 )
				{
					player.Ammo.Give( AmmoType, ammoToGive );
				}
			}

			GameObject.Destroy();
		}
		else
		{
			player.Weapons.Give( GameObject, false );
			GameObject.Destroy();

			// Die Waffe wird nun gehalten
			IsHeld = true;
		}
	}

	protected override void OnDeployed()
	{
		base.OnDeployed();
		EffectRenderer.Set( "b_empty", AmmoInClip == 0 );
	}
	protected override void OnHolstered()
	{
		base.OnHolstered();

		IsHeld = false;

		ReloadSound?.Stop();

		EffectRenderer.Set( "b_empty", false );
	}
	public void StopReloadSound()
	{
		ReloadSound?.Stop();
		ReloadSound = null;

	}


	public override void PrimaryAction()
	{
		IsFiering = true;

		if ( IsMelee )
		{
			// Wenn aufgeladen, führe den Spezialangriff durch
			if ( FullyCharged )
			{
				PerformMeleeAttack( Player.Local, true );
				StopCharging();
				NextChargeTime = ChargeCooldown * 0.25f;
			}
			else if ( IsCharging )
			{
				// Während des Aufladens keine normale Angriffsaction ausführen
				return;
			}
			else
			{
				// Normalen Angriff nur ausführen, wenn nicht im Aufladezustand
				PerformMeleeAttack( Player.Local );
			}
		}
		else
		{
			// Fernkampfangriff ausführen
			FireBullet( Player.Local );
		}
	}
	[Property, Category( "Melee" )] public float ChargeCooldown { get; set; } = 0.5f;
	public TimeUntil NextChargeTime { get; set; } = 0f;

	public override void PrimaryActionRelease()
	{
		IsFiering = false;
	}
	public override void SeccondaryActionRelease()
	{
		if ( Owner == null )
		{
			return;
		}

		Owner.IsAiming = false;
		if ( Owner.CameraMode != 0 )
		{
			Owner.IsAimingCamera = false;
		}

		// Beende das Aufladen, wenn die rechte Maustaste losgelassen wird
		StopCharging();
	}
	private async Task ResetAnimationStateAfterDelay( float delay )
	{
		await Task.Delay( (int)(delay * 1000) );
		_isAnimationPlaying = false;
	}
	private bool _isAnimationPlaying = false;
	private void StartCharging()
	{
		if ( !NextChargeTime )
		{
			return;
		}

		// Prüfe, ob bereits aufgeladen wird
		if ( IsCharging ) return;

		// Prüfe, ob eine Animation läuft
		if ( _isAnimationPlaying ) return;

		IsCharging = true;
		_isAnimationPlaying = true;
		ChargeComplete = ChargeTime;
		NextChargeTime = ChargeCooldown;

		var boneAnimController = GameObject.Components.GetInDescendantsOrSelf<BoneAnimationController>();
		if ( boneAnimController == null && Player.Local?.GameObject != null )
		{
			boneAnimController = Player.Local.GameObject.Components.GetInDescendantsOrSelf<BoneAnimationController>();
		}



		if ( boneAnimController != null )
		{
			string chargingSequenceName = "Charge";

			if ( boneAnimController.HasSequence( chargingSequenceName ) )
			{
				// Sequenz abspielen
				boneAnimController.PlaySequence( chargingSequenceName );

				// Nach der Dauer der Animation den Zustand zurücksetzen
				_ = ResetAnimationStateAfterDelay( ChargeTime );
			}
			else
			{
				// Erstelle eine neue Charge-Sequenz mit einem speziellen Aufbau
				var newSequence = new AnimationSequence
				{
					Name = chargingSequenceName,
					Steps = new List<AnimationStep>()
				};

				if ( boneAnimController.Animations.Count > 0 )
				{
					// Finde eine Animation, die wir anpassen können
					BoneAnimation chargeAnimation = null;

					// Prüfe, ob wir eine bestehende Animation anpassen können
					foreach ( var anim in boneAnimController.Animations )
					{
						if ( anim.Name.Contains( "Attack" ) || anim.Name.Contains( "Swing" ) )
						{
							// Kopiere die Animation und modifiziere sie
							chargeAnimation = new BoneAnimation
							{
								Name = "ChargeAnim",
								BoneName = anim.BoneName,
								TargetRotation = anim.TargetRotation,
								Duration = 0.5f,  // Schnell in Position gehen
								ReturnDuration = 0.0f,  // Wichtig: Kein automatisches Zurückkehren!
								EaseType = EaseType.EaseOut,
								Loop = false
							};

							// Füge die angepasste Animation zur Liste hinzu
							boneAnimController.Animations.Add( chargeAnimation );
							break;
						}
					}

					// Falls keine passende Animation gefunden wurde, verwende die erste verfügbare
					if ( chargeAnimation == null && boneAnimController.Animations.Count > 0 )
					{
						var defaultAnim = boneAnimController.Animations[0];
						chargeAnimation = new BoneAnimation
						{
							Name = "ChargeAnim",
							BoneName = defaultAnim.BoneName,
							TargetRotation = new Angles( 30, 0, 0 ),  // Eine angehobene Position
							Duration = 0.5f,
							ReturnDuration = 0.0f,  // Wichtig: Kein automatisches Zurückkehren!
							EaseType = EaseType.EaseOut,
							Loop = false
						};

						boneAnimController.Animations.Add( chargeAnimation );
					}

					// Animation zur Sequenz hinzufügen
					if ( chargeAnimation != null )
					{
						newSequence.Steps.Add( new AnimationStep
						{
							AnimationName = chargeAnimation.Name,
							WaitForCompletion = true,  // Warten, bis die Position erreicht ist
							TimeScale = 1.0f
						} );

						boneAnimController.Sequences.Add( newSequence );
						boneAnimController.PlaySequence( chargingSequenceName );
					}
				}
			}
		}

		// Visuellen Effekt hinzufügen
		EffectRenderer?.Set( "b_charging", true );
		if ( Player.Local?.ModelRenderer != null )
		{
			Player.Local.ModelRenderer.Set( "b_charging", true );
		}

		// Sound zum Laden abspielen

	}

	// Methode zum Beenden des Aufladens mit Animation zurück zur Ausgangsposition
	private void StopCharging()
	{
		if ( !IsCharging ) return;

		IsCharging = false;

		// Beende die Auflade-Animation mit Rückkehr zur Originalpose
		var boneAnimController = GameObject.Components.GetInDescendantsOrSelf<BoneAnimationController>();
		if ( boneAnimController == null && Player.Local?.GameObject != null )
		{
			boneAnimController = Player.Local.GameObject.Components.GetInDescendantsOrSelf<BoneAnimationController>();
		}

		if ( boneAnimController != null )
		{
			string returnSequenceName = "ChargeReturn";

			if ( boneAnimController.HasSequence( returnSequenceName ) )
			{
				// Verwende eine existierende Rückkehr-Sequenz
				boneAnimController.PlaySequence( returnSequenceName );
			}
			else
			{
				// Erstelle eine neue Rückkehr-Sequenz
				var animation = boneAnimController.Animations.Find( a => a.Name == "ChargeAnim" );

				if ( animation != null )
				{
					// Erstelle eine temporäre Animation für die Rückkehr
					var returnAnimation = new BoneAnimation
					{
						Name = "ChargeReturnAnim",
						BoneName = animation.BoneName,
						TargetRotation = new Angles( 0, 0, 0 ),  // Zurück zur neutralen Position
						Duration = 0.3f,  // Schnell zurück
						ReturnDuration = 0.0f,
						EaseType = EaseType.EaseIn,
						Loop = false
					};

					boneAnimController.Animations.Add( returnAnimation );

					// Erstelle und spiele die Rückkehr-Sequenz
					var returnSequence = new AnimationSequence
					{
						Name = returnSequenceName,
						Steps = new List<AnimationStep>
					{
						new AnimationStep
						{
							AnimationName = returnAnimation.Name,
							WaitForCompletion = true,
							TimeScale = 1.0f
						}
					}
					};

					boneAnimController.Sequences.Add( returnSequence );
					boneAnimController.PlaySequence( returnSequenceName );
				}
				else
				{
					// Falls keine ChargeAnim gefunden wurde, versuche einen anderen Weg
					// um die Animation zurückzusetzen
					foreach ( var anim in boneAnimController.Animations )
					{
						if ( anim.Name.Contains( "Idle" ) || anim.Name.Contains( "Default" ) )
						{
							boneAnimController.PlayAnimation( anim.Name );
							break;
						}
					}
				}
			}
		}

		

		// Wenn nicht vollständig aufgeladen, beende auch den Sound
		if ( !FullyCharged )
		{
			
		}
	}

	[Property] public GameObject Ragdoll { get; set; }
	private TimeUntil _animationCooldown = 0;

	

	public override void SecondaryAction()
	{
		Owner.IsAiming = true;

		if ( Owner.CameraMode != 0 ) // Wenn wir im Third-Person-Modus sind
		{
			Owner.IsAimingCamera = true;
			if ( Owner.healthEffects != null )
			{
				Owner.healthEffects.ApplyAimEffect();
			}
		}


		if ( IsMelee )
		{
			// Prüfe, ob eine Animation bereits läuft
			var boneAnimController = GameObject.Components.GetInDescendantsOrSelf<BoneAnimationController>();
			if ( boneAnimController == null && Player.Local?.GameObject != null )
			{
				boneAnimController = Player.Local.GameObject.Components.GetInDescendantsOrSelf<BoneAnimationController>();
			}

			// Nur starten, wenn nicht bereits am Aufladen UND keine Animation via _isAnimationPlaying aktiv ist
			if ( !IsCharging && NextChargeTime <= 0 && _animationCooldown <= 0 && !_isAnimationPlaying )
			{
				StartCharging();
				_animationCooldown = 0.5f;
			}
		}
	}
	[Rpc.Broadcast]
	private void PerformMeleeAttack( Player player, bool isSpecialAttack = false )
	{
		
		if ( isSpecialAttack && !FullyCharged )
		{
			isSpecialAttack = false;
		}

		// Bestehender Code für PerformMeleeAttack...
		if ( NextMeleeAttackTime > 0 && !isSpecialAttack ) return;

		if ( player == null ) return;
		
		

		var boneAnimController = GameObject.Components.GetInDescendantsOrSelf<BoneAnimationController>();

		// Falls der Controller nicht an der Waffe ist, schaue beim Spieler nach
		if ( boneAnimController == null && player?.GameObject != null )
		{
			boneAnimController = player.GameObject.Components.GetInDescendantsOrSelf<BoneAnimationController>();
		}

		// Prüfe, ob der Controller gefunden wurde
		if ( boneAnimController == null )
		{
			return;
		}

		// Wähle die Animation basierend auf dem Angriffstyp
		string sequenceName = isSpecialAttack ? "Special" : "Sequenz";

		if ( boneAnimController.HasSequence( sequenceName ) )
		{
			boneAnimController.PlaySequence( sequenceName );
		}
		else
		{
			// Optional: Erstelle die Sequenz dynamisch, falls sie nicht existiert
			var newSequence = new AnimationSequence
			{
				Name = sequenceName,
				Steps = new List<AnimationStep>()
			};

			// Füge einen einfachen Schritt hinzu, wenn Animationen vorhanden sind
			if ( boneAnimController.Animations.Count > 0 )
			{
				newSequence.Steps.Add( new AnimationStep
				{
					AnimationName = boneAnimController.Animations[0].Name,
					WaitForCompletion = true
				} );

				boneAnimController.Sequences.Add( newSequence );
				boneAnimController.PlaySequence( sequenceName );
			}
		}

		// Angriffslogik basierend auf Angriffstyp
		if ( isSpecialAttack )
		{
			// Spezialangriff: Kreisförmiger Angriff um den Spieler herum
			PerformCircularAttack( player );

		}
		else
		{
			// Normaler Nahkampfangriff (bestehende Logik)
			var attachment = EffectRenderer.GetAttachment( "muzzle" );
			var playerPosition = player.PlyCamera.WorldPosition;
			var forwardDirection = player.PlyCamera.WorldRotation.Forward;

			var startPos = playerPosition + forwardDirection * 0;
			var endPos = playerPosition + forwardDirection * 150;

			Owner.ApplyRecoil( new Angles( Random.Shared.Float( -2f, -3f ), Random.Shared.Float( -1f, 1f ), 0 ) );

			float slashRadius = 5.0f;
			var trace = Scene.Trace.Sphere( slashRadius, startPos, endPos )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.WithoutTags( "player" )
				.Size( slashRadius )
				.UseHitboxes()
				.Run();

			DebugOverlay.Sphere( new Sphere( startPos, slashRadius ), Color.Yellow.WithAlpha( 0.3f ), duration: 0.5f, overlay: false );
			DebugOverlay.Sphere( new Sphere( endPos, slashRadius ), Color.Red.WithAlpha( 0.3f ), duration: 0.5f, overlay: false );
			DebugOverlay.Line( startPos, endPos, Color.Green, duration: 0.5f );

			// Restliche bestehende Logik für normalen Angriff...
			// (Der bestehende Code bleibt unverändert)
		}

		NextMeleeAttackTime = isSpecialAttack ? MeleeCooldown * 1.5f : MeleeCooldown;
	}

	// Neue Methode für den kreisförmigen Angriff
	private void PerformCircularAttack( Player player )
	{
		// Kameraposition und -richtung nutzen
		Vector3 cameraPosition = player.PlyCamera.WorldPosition;
		Vector3 cameraForward = player.PlyCamera.WorldRotation.Forward;

		// Mittelpunkt des Angriffs vor dem Spieler platzieren (z.B. 100 Einheiten nach vorne)
		Vector3 attackCenter = cameraPosition + cameraForward * 100.0f;

		// Parameter für den kreisförmigen Angriff
		float attackRadius = 300.0f; // Radius des Angriffs
		int numTraces = 12; // Anzahl der Traces im Kreis
		




		// Konstruiere eine Ebene senkrecht zur Blickrichtung
		Vector3 forward = cameraForward;
		
		Vector3 right = Vector3.Cross( Vector3.Up, forward );

		// Traces in einem Kreis vor dem Spieler durchführen
		for ( int i = 0; i < numTraces; i++ )
		{
			// Berechne Punkte im Kreis in der Ebene vor dem Spieler
			float angle = (360.0f / numTraces) * i;
			float radians = angle * (MathF.PI / 180.0f);

			// Berechne Richtungsvektor in der Ebene vor dem Spieler
			Vector3 direction = (right * MathF.Cos( radians ) + Vector3.Up * MathF.Sin( radians )).Normal;

			// Berechne End-Position für den Trace
			Vector3 endPos = attackCenter + direction * attackRadius;

			// Führe Traces durch
			PerformCircleTrace( attackCenter, endPos, player );

			// Zusätzliche Traces in verschiedenen Höhen
			Vector3 middleDirection = (forward * 0.5f + direction * 0.5f).Normal;
			Vector3 middleEndPos = attackCenter + middleDirection * attackRadius;
			PerformCircleTrace( attackCenter, middleEndPos, player );
		}

		// Rest der Methode bleibt gleich
		Owner.ApplyRecoil( new Angles( Random.Shared.Float( -2f, -3f ), Random.Shared.Float( -1f, 1f ), 0 ) );
		EffectRenderer.Set( "b_attack", true );
		ModelRenderer.Set( "b_attack", true );

		if ( Player.Local?.ModelRenderer != null )
		{
			Player.Local.ModelRenderer.Set( "b_attack", true );
		}

		SendMeleeAttackMessage( cameraPosition, attackCenter, attackRadius );

		var trailrenderer = player.GameObject.Components.GetInDescendantsOrSelf<TrailRenderer>();
		if ( trailrenderer != null )
		{
			trailrenderer.Emitting = true;
			_ = DisableTrailAfterDelay( trailrenderer, 0.5f );
		}

	}

	private void PerformCircleTrace( Vector3 startPos, Vector3 endPos, Player player )
	{
		// Führe den Trace durch
		var trace = Scene.Trace.Ray( startPos, endPos )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "player" )
			.UseHitboxes()
			.Run();

		// Wenn etwas getroffen wurde
		if ( trace.Hit )
		{
			IHealthComponent damageable = null;

			if ( trace.Component.IsValid() )
			{
				damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
			}

			if ( damageable != null )
			{
				// Erhöhter Schaden für Spezialangriff (2x normaler Schaden)
				var damage = Damage * 2;

				Random random = new Random();
				float playerAttackValue = random.Next( (int)player.MinAttackValue, (int)player.MaxAttackValue + 1 );
				var playerAttackPower = player.AttackPower;
				var playerCritChance = player.CritHitChance;
				var playerCritDamage = player.CritHitDamage;

				damage += (int)(damage * (playerAttackValue / 200.0f));
				int calculatedDamage = (int)(damage * (playerAttackPower / 50.0f));
				damage += random.Next( 0, calculatedDamage + 1 );

				// Erhöhte Crit-Chance für Spezialangriff
				int critRoll = random.Next( 0, 101 );
				if ( critRoll <= playerCritChance * 1.5f )
				{
					damage += (int)(damage * 0.5f + playerCritDamage);
					isCriticalHit = true;
				}
				else
				{
					isCriticalHit = false;
				}

				// Füge Schaden zu
				damageable.TakeDamage( DamageType.Bullet, damage, trace.EndPosition, trace.Direction * DamageForce, GameObject.Id, GameObject.Id );

				// Erstelle visuelles Feedback
				GameObject hitinfo = Hitprefab.Clone( trace.EndPosition );
				if ( hitinfo != null )
				{
					FaceThing facething = hitinfo.Components.Get<FaceThing>();
					facething.Thing = player.GameObject;
					TextRenderer textRenderer = hitinfo.Components.Get<TextRenderer>();

					if ( isCriticalHit )
					{
						textRenderer.Color = Color.Red;
					}
					else
					{
						textRenderer.Color = Color.Yellow; // Spezialangriffe in gelb anzeigen
					}
					textRenderer.Text = $"{damage}";
					ScaleTextWithDistance scaleTextWithDistance = hitinfo.Components.Get<ScaleTextWithDistance>();
					scaleTextWithDistance.Thing = player.GameObject;
				}
			}

			// Effekte beim Treffer
			SendImpactMessage( trace.EndPosition, trace.Normal );
		}
	}

	[Rpc.Broadcast]

	private void PerformMeleeAttack( Player player )
	{

		if ( NextMeleeAttackTime > 0 ) return;

		if ( player == null ) return;


		var boneAnimController = GameObject.Components.GetInDescendantsOrSelf<BoneAnimationController>();

		// Falls der Controller nicht an der Waffe ist, schaue beim Spieler nach
		if ( boneAnimController == null && player?.GameObject != null )
		{

			boneAnimController = player.GameObject.Components.GetInDescendantsOrSelf<BoneAnimationController>();
		}

		// Prüfe, ob der Controller gefunden wurde
		if ( boneAnimController == null )
		{

			return;
		}
		var trailrenderer = player.GameObject.Components.GetInDescendantsOrSelf<TrailRenderer>();
		if ( trailrenderer != null )
		{
			trailrenderer.Emitting = true;

			// Schalte den Trail nach einer kurzen Zeit wieder aus
			_ = DisableTrailAfterDelay( trailrenderer, 0.5f );
		}





		string sequenceName = "Sequenz"; // Hier den Namen deiner Animationssequenz eintragen


		if ( boneAnimController.HasSequence( sequenceName ) )
		{

			boneAnimController.PlaySequence( sequenceName );
		}
		else
		{


			// Optional: Erstelle die Sequenz dynamisch, falls sie nicht existiert
			// In der PerformMeleeAttack-Methode
			var newSequence = new AnimationSequence
			{
				Name = sequenceName,
				Steps = new List<AnimationStep>() // Ändere SequenceStep zu AnimationStep
			};

			// Füge einen einfachen Schritt hinzu, wenn Animationen vorhanden sind
			if ( boneAnimController.Animations.Count > 0 )
			{
				newSequence.Steps.Add( new AnimationStep // Ändere SequenceStep zu AnimationStep
				{
					AnimationName = boneAnimController.Animations[0].Name,
					WaitForCompletion = true
				} );


				boneAnimController.Sequences.Add( newSequence );
				boneAnimController.PlaySequence( sequenceName );
			}
		}
		var attachment = EffectRenderer.GetAttachment( "muzzle" );
		var playerPosition = player.PlyCamera.WorldPosition;
		var forwardDirection = player.PlyCamera.WorldRotation.Forward;
			

		var startPos = playerPosition + forwardDirection * 0;

		// Berechnen Sie die Endposition 50 Einheiten vor dem Spieler und 25 Einheiten nach rechts
		var endPos = playerPosition + forwardDirection * 150;

		// Zeigen Sie den Nahkampfangriff an

		Owner.ApplyRecoil( new Angles( Random.Shared.Float( -2f, -3f ), Random.Shared.Float( -1f, 1f ), 0 ) );

		NextMeleeAttackTime = MeleeCooldown;

		EffectRenderer.Set( "b_attack", true );
		ModelRenderer.Set( "b_attack", true );
		// Sicherere Version mit Null-Prüfung und Logging
		if ( Player.Local?.ModelRenderer != null )
		{
			Player.Local.ModelRenderer.Set( "b_attack", true );

		}

		// Führen Sie den Nahkampfangriff aus (Ihre bestehende Logik)
		// ...existing code...
		float slashRadius = 20.0f;
		var trace = Scene.Trace.Sphere( slashRadius, startPos, endPos )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "player" )
			.Size( slashRadius )
			.UseHitboxes()
			.Run();

		

		// Korrigierte DebugOverlay-Aufrufe (Zeilen 885-887)
		

		var damage = Damage;

		var origin = attachment?.Position ?? startPos;

		SendMeleeAttackMessage( origin, trace.EndPosition, trace.Distance );

		IHealthComponent damageable = null;

		
		// ...existing code...

		if ( trace.Component.IsValid() )
			damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();


		if ( damageable is not null )
		{

			// Zuerst prüfen, ob wir einen lebenden NPC/Gegner getroffen haben
			if ( trace.GameObject != null )
			{
				var soundPath = "sounds/impacts/bullets/impact-bullet-generic.sound";
				// Prüfe auf Gesundheitskomponente - das deutet auf NPCs oder andere Lebewesen hin
				if ( trace.GameObject.Components.GetInAncestorsOrSelf<IHealthComponent>() != null )
				{
					
					soundPath = "sounds/impacts/bullets/impact-bullet-flesh.sound";
					Sound.Play( soundPath, trace.EndPosition );
					// Sofort verlassen, da wir wissen, dass es ein Lebewesen ist
				}

				
			}


			Random random = new Random();
			float playerAttackValue = random.Next( (int)player.MinAttackValue, (int)player.MaxAttackValue + 1 );
			var playerAttackPower = player.AttackPower;
			var playerCritChance = player.CritHitChance;
			var playerCritDamage = player.CritHitDamage;

			damage += (int)(damage * (playerAttackValue / 300.0f));

			int calculatedDamage = (int)(damage * (playerAttackPower / 50.0f));
			damage += random.Next( 0, calculatedDamage + 1 );

			int critRoll = random.Next( 0, 101 );
			{
				if ( critRoll <= playerCritChance )
				{
					damage += (int)(damage * 0.5f + playerCritDamage);
					isCriticalHit = true;

				}
				else
				{
					isCriticalHit = false;
				}
			}

			damageable.TakeDamage( DamageType.Bullet, damage, trace.EndPosition, trace.Direction * DamageForce, GameObject.Id, GameObject.Id );

			GameObject hitinfo = Hitprefab.Clone( trace.EndPosition );
			FaceThing facething = hitinfo.Components.Get<FaceThing>();
			facething.Thing = player.GameObject;
			TextRenderer textRenderer = hitinfo.Components.Get<TextRenderer>();

			if ( isCriticalHit )
			{
				textRenderer.Color = Color.Red;
			}
			else
			{
				textRenderer.Color = Color.White;
			}
			textRenderer.Text = $"{damage}";
			ScaleTextWithDistance scaleTextWithDistance = hitinfo.Components.Get<ScaleTextWithDistance>();
			scaleTextWithDistance.Thing = player.GameObject;

		}
		else if ( trace.Hit )
		{
			
			if ( ImpactArea != null )
			{
				
				var impactInstance = ResourceLibrary.Get<PrefabFile>( ImpactArea.ResourcePath );
				if ( impactInstance != null )
				{
					var impactObject = GameObject.Clone( impactInstance );
					if ( impactObject != null )
					{
						impactObject.WorldPosition = trace.EndPosition;
						impactObject.WorldRotation = Rotation.LookAt( trace.Normal );

						// Passe die Partikeleffekte an den Nahkampfstil an
						var impactRenderer = impactObject.Components.Get<ParticleEffect>();
						if ( impactRenderer != null )
						{
							impactRenderer.Scale = 5f; // Kleinere Größe für Nahkampf
							impactRenderer.Tint = Color.Orange.WithAlpha( 0.7f ); // Angepasste Farbe für Nahkampf
						}
						PlayImpactSound( trace );

						// Zerstöre den Effekt nach kurzer Zeit
						_ = DestroyImpactEffectAfterDelay( impactObject, 1.0f );
					}
				}
			}
			SendImpactMessage( trace.EndPosition, trace.Normal );


		}

		if ( trace.GameObject != null && trace.GameObject.IsValid() )
		{
			// Prüfe, ob das Objekt "World Physics" ist, und überspringe die Animation
			if ( trace.GameObject.Name == "World Physics" )
			{

			}
			else
			{
				var boneController = trace.GameObject.Components.GetInAncestorsOrSelf<BoneAnimationController>();
				if ( boneController != null )
				{
					if ( boneController.HasSequence( "Hit" ) )
					{

						boneController.PlaySequence( "Hit" );
					}

				}

			}



			var target = trace.GameObject;
			if ( target != null )
			{
				if ( target.Components.TryGet<Rigidbody>( out var body ) )
					body.ApplyImpulseAt( trace.HitPosition, trace.Direction * HitForce );

				if ( target.Components.TryGet<HealthComponent>( out var health ) )
					health.Damage( Damage, DamageType, player.GameObject, trace.HitPosition, trace.Direction, HitForce );
			}
			else if ( trace.Hit )
			{
				
				SendImpactMessage( trace.EndPosition, trace.Normal );
			}
			
			else
			{
				Log.Warning( "Player.Local oder ModelRenderer ist null - Animation konnte nicht gesetzt werden" );
			}

			
		}
	}
	private void PlayImpactSound( SceneTraceResult trace )
	{


		string soundPath = "sounds/impacts/bullets/impact-bullet-generic.sound";

		



		// Versuche, das Oberflächenmaterial zu bestimmen
		if ( trace.Surface != null )
		{
			// Priorisiere den Surface Tag als Identifikator
			string surfaceTag = trace.Surface.ResourceName.ToLower();

			if ( surfaceTag.Contains( "wood" ) || trace.GameObject?.Name.ToLower().Contains( "wood" ) == true )
			{
				soundPath = "sounds/impacts/bullets/impact-bullet-wood.sound";
			}
			else if ( surfaceTag.Contains( "metal" ) || trace.GameObject?.Name.ToLower().Contains( "metal" ) == true )
			{
				soundPath = "sounds/impacts/bullets/impact-bullet-metal.sound";
			}
			else if ( surfaceTag.Contains( "concrete" ) || trace.GameObject?.Name.ToLower().Contains( "concrete" ) == true )
			{
				soundPath = "sounds/impacts/bullets/impact-bullet-concrete.sound";
			}
			else if ( surfaceTag.Contains( "dirt" ) || trace.GameObject?.Name.ToLower().Contains( "dirt" ) == true )
			{
				soundPath = "sounds/impacts/bullets/impact-bullet-dirt.sound";
			}
			else if ( surfaceTag.Contains( "glass" ) || trace.GameObject?.Name.ToLower().Contains( "glass" ) == true )
			{
				soundPath = "sounds/impacts/bullets/impact-bullet-glass.sound";
			}
			else if ( surfaceTag.Contains( "sand" ) || trace.GameObject?.Name.ToLower().Contains( "sand" ) == true )
			{
				soundPath = "sounds/impacts/bullets/impact-bullet-sand.sound";
			}
			else if ( surfaceTag.Contains( "water" ) || trace.GameObject?.Name.ToLower().Contains( "water" ) == true )
			{
				soundPath = "sounds/impacts/bullets/impact-bullet-water.sound";
			}
			else if ( surfaceTag.Contains( "plastic" ) || trace.GameObject?.Name.ToLower().Contains( "plastic" ) == true )
			{
				soundPath = "sounds/impacts/bullets/impact-bullet-plastic.sound";
			}
			else if ( surfaceTag.Contains( "flesh" ) || trace.GameObject?.Name.ToLower().Contains( "flesh" ) == true
					|| trace.GameObject?.Components.GetInAncestorsOrSelf<IHealthComponent>() != null )
			{
				soundPath = "sounds/impacts/bullets/impact-bullet-flesh.sound";
			}
			else if ( surfaceTag.Contains( "cloth" ) || trace.GameObject?.Name.ToLower().Contains( "cloth" ) == true )
			{
				soundPath = "sounds/impacts/bullets/impact-bullet-cloth.sound";
			}
			else if ( surfaceTag.Contains( "foliage" ) || trace.GameObject?.Name.ToLower().Contains( "foliage" ) == true
					|| trace.GameObject?.Name.ToLower().Contains( "plant" ) == true
					|| trace.GameObject?.Name.ToLower().Contains( "tree" ) == true )
			{
				soundPath = "sounds/impacts/bullets/impact-bullet-foliage.sound";
			}
			else if ( surfaceTag.Contains( "snow" ) || trace.GameObject?.Name.ToLower().Contains( "snow" ) == true )
			{
				soundPath = "sounds/impacts/bullets/impact-bullet-snow.sound";
			}
			else if ( surfaceTag.Contains( "plaster" ) || trace.GameObject?.Name.ToLower().Contains( "plaster" ) == true )
			{
				soundPath = "sounds/impacts/bullets/impact-bullet-plaster.sound";
			}
		}

		// Spiele den Sound ab
		Sound.Play( soundPath, trace.EndPosition );

		// Bei Metallobjekten zusätzlich ein Quietschen abspielen
		if ( soundPath.Contains( "metal" ) )
		{
			// Zufällig eines der Ricochet-Sounds abspielen
			int randomRicochet = Game.Random.Int( 1, 6 );
			Sound.Play( $"sounds/impacts/bullets/bullet-ricochet-{randomRicochet}.vsnd_c", trace.EndPosition );
		}
	}
	private async Task DestroyImpactEffectAfterDelay( GameObject impactEffect, float delay )
	{
		await Task.Delay( (int)(delay * 1000) );
		if ( impactEffect != null && impactEffect.IsValid() )
		{
			impactEffect.Destroy();
		}
	}
	private async Task DisableTrailAfterDelay( TrailRenderer trail, float delay )
	{
		// Warte die angegebene Zeit
		await Task.Delay( (int)(delay * 1000) );

		// Überprüfe ob der Trail noch existiert und schalte ihn aus
		if ( trail != null && trail.IsValid() )
		{
			trail.Emitting = false;
		}
	}


	public override void ReloadAction()
	{
		if ( AmmoInClip >= ClipSize || IsReloading )
		{
			EffectRenderer?.Set( "b_reload", false );

			if ( Owner?.CameraMode != 0 ) // Third-Person-Modus
			{
				Owner?.ModelRenderer.Set( "b_reload", false );
			}

			return;
		}

		var ammoToTake = ClipSize - AmmoInClip;
		if ( ammoToTake <= 0 )
		{
			// Magazin ist bereits voll, Nachladeanimation stoppen
			EffectRenderer?.Set( "b_reload", false );

			if ( Owner?.CameraMode != 0 ) // Third-Person-Modus
			{
				Owner?.ModelRenderer.Set( "b_reload", false );
			}

			Log.Info( "Magazin ist bereits voll, Nachladeanimation gestoppt." );
			return;
		}

		if ( !Owner.IsValid() || IsReloading )
			return;

		if ( !Owner.Ammo.CanTake( AmmoType, ammoToTake, out var taken ) )
			return;

		// Set animations based on camera mode
		EffectRenderer?.Set( "b_reload", true );

		if ( Owner.CameraMode != 0 ) // Third-Person-Modus
		{
			Owner.ModelRenderer.Set( "b_reload", true );
		}

		ReloadFinishTime = AmmoInClip == 0 ? EmptyReloadTime : ReloadTime;
		IsReloading = true;

		SendReloadMessage();

		if ( AmmoInClip >= ClipSize )
		{
			EffectRenderer?.Set( "b_reload", false );

			if ( Owner.CameraMode != 0 ) // Third-Person-Modus
			{
				Owner.ModelRenderer.Set( "b_reload", false );
			}
		}
	}

	private void FireDefaultBullet( Player shooter )
	{
		if ( shooter == null || Owner == null || EffectRenderer == null || Scene == null )
		{
			return;
		}

		// Prüfung auf Munition, etc. bleibt unverändert
		if ( AmmoInClip <= 0 )
		{
			SendEmptyClipMessage();
			ReloadAction();
			NextAttackTime = 1f / FireRate;
			return;
		}

		if ( Input.Pressed( "Run" ) )
		{
			return;
		}

		// Recoil und Animationen bleiben unverändert
		Owner.ApplyRecoil( Recoil );
		EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
		EffectRenderer?.Set( "b_attack", true );
		Owner?.ModelRenderer.Set( "b_attack", true );
		EffectRenderer?.Set( "b_reload", false );

		var gunrenderer = EffectRenderer?.Components.GetAll<SkinnedModelRenderer>();
		if ( gunrenderer != null )
		{
			foreach ( var renderer in gunrenderer )
			{
				renderer.Set( "b_attack", true );
				renderer.Set( "b_empty", AmmoInClip == 0 );
				renderer.Set( "b_reload", false );
			}
		}

		NextAttackTime = 1f / FireRate;
		AmmoInClip--;

		// WICHTIG: Hier beginnt die korrigierte Schusslogik

		// 1. Immer von der Kamera aus den Zielstrahl berechnen
		var cameraPos = Owner.PlyCamera.WorldPosition;
		var cameraDir = Owner.PlyCamera.WorldRotation.Forward;

		// Zielstrahl vom Kamerazentrum durch das Fadenkreuz
		var targetRay = Scene.Trace.Ray( cameraPos, cameraPos + cameraDir * 5000f )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "player" )
			.UseHitboxes( true )
			.Run();

		Vector3 targetPoint = targetRay.Hit ? targetRay.EndPosition : cameraPos + cameraDir * 5000f;

		// 2. Mündungsposition abhängig vom Kameramodus bestimmen
		Vector3 muzzlePos;

		if ( Owner.CameraMode == 0 ) // First-Person
		{
			// In Ego-Perspektive: Mündungsposition aus dem Waffen-Attachment
			var attachment = EffectRenderer.GetAttachment( "muzzle" );
			muzzlePos = attachment?.Position ?? cameraPos;
		}
		else // Third-Person
		{
			// In Third-Person: Mündung aus dem sichtbaren Waffen-Model
			var muzzlePoint = GameObject.Components.GetInDescendantsOrSelf<MuzzlePoint>();
			if ( muzzlePoint != null )
			{
				muzzlePos = muzzlePoint.WorldPosition;
			}
			else
			{
				// Fallback auf die Position der Waffe + Offset in Blickrichtung
				muzzlePos = GameObject.Transform.World.Position + GameObject.Transform.World.Rotation.Forward * 20 + Vector3.Forward * 200;
			}
		}

		// 3. Schussrichtung vom Mündungspunkt zum Zielpunkt
		Vector3 shootDirection = (targetPoint - muzzlePos).Normal;

		// Korrektur für Third-Person: Versatz nach links hinzufügen
		// Korrektur für Third-Person: Versatz nach links und oben hinzufügen
		if ( Owner.CameraMode != 0 ) // Third-Person
		{
			// Berechne den Vektor, der nach links zeigt (relativ zur Blickrichtung)
			
		}

		// 4. Jetzt erst den Spread hinzufügen
		shootDirection += Vector3.Random * Spread;

		// 5. Endpunkt berechnen
		var endPos = muzzlePos + shootDirection * 5000f;

		// 6. Finaler Raycast für Kollisionserkennung
		var trace = Scene.Trace.Ray( muzzlePos, endPos )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "player" )
			.UseHitboxes( true )
			.Run();

		if ( trace.Hit )
		{
			endPos = trace.EndPosition;
		}

		// Trail-Effekt und restliche Logik bleibt unverändert
		if ( Trail != null )
		{
			var trailInstance = ResourceLibrary.Get<PrefabFile>( Trail.ResourcePath );
			if ( trailInstance != null )
			{
				var trailobject = GameObject.Clone( trailInstance );
				if ( trailobject != null )
				{
					trailobject.WorldPosition = muzzlePos;
					trailobject.NetworkSpawn();

					var trailobjectRenderer = trailobject.Components.Get<ParticleEffect>();
					if ( trailobjectRenderer != null )
					{
						trailobjectRenderer.Yaw = Rotation.LookAt( shootDirection ).Yaw();
						trailobjectRenderer.Pitch = Rotation.LookAt( shootDirection ).Pitch();
					}

					var speed = (0.8f + BulletSpeed) * 1000f;
					UpdateTrailObjectPosition( trailobject, shootDirection, speed, endPos, shooter );
				}
			}
		}

		SendAttackMessage( muzzlePos, endPos, trace.Distance, trace );
	}

	public virtual void FireBullet( Player shooter )
	{
		if ( shooter == null || Owner == null || EffectRenderer == null || Scene == null )
		{
			return;
		}
		if ( shooter.LifeState == LifeState.Dead )
		{
			return;
		}

		if ( !NextAttackTime )
		{
			return;
		}
		if ( shooter.IsRunning )
		{
			return;
		}
		if ( IsReloading )
		{
			return;
		}

		if ( IsMagicWeapon && Player.Local.Mana < 10 )
		{
			return;
		}

		if ( IsMagicWeapon )
		{
			// Verbrauche Mana
			Player.Local.ChangeMana( -10 );
		}
		if ( IsShotgun )
		{

			if ( AmmoInClip <= 0 )
			{
				SendEmptyClipMessage();
				ReloadAction();
				NextAttackTime = 1f / FireRate;
				return;
			}

			if ( Owner.MoveSpeed > 150f ) return;

			Owner.ApplyRecoil( Recoil );
			EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
			EffectRenderer?.Set( "b_attack", true );
			EffectRenderer?.Set( "b_reload", false );

			NextAttackTime = 1f / FireRate;
			AmmoInClip--;

			FireShotgun( shooter );

			return;
		}


		if ( AmmoInClip <= 0 )
		{
			SendEmptyClipMessage();
			ReloadAction();
			NextAttackTime = 1f / FireRate;
			return;
		}



		var itemComponent = Components.Get<ItemComponent>();
		if ( itemComponent != null )
		{
			switch ( itemComponent.Aspect )
			{
				case AspectType.Fire:
					// Feueraspekt implementieren
					FireBulletWithFireAspect( shooter );
					return;
				// Water
				case AspectType.Water:
					FireBulletWithWaterAspect( shooter );
					return;
				//ICE
				case AspectType.Ice:
					FireBulletWithIceAspect( shooter );
					return;
				//AIR
				case AspectType.Air:
					FireBulletWithAirAspect( shooter );
					return;
				//EARTH
				case AspectType.Earth:
					FireBulletWithEarthAspect( shooter );
					return;
				//LIGHTNING
				case AspectType.Lightning:
					FireBulletWithLightningAspect( shooter );
					return;
				//SHADOW
				case AspectType.Shadow:
					FireBulletWithShadowAspect( shooter );
					return;
				case AspectType.Holy:
					FireBulletWithHolyAspect( shooter );
					return;
				//BLEED
				case AspectType.Bleed:
					FireBulletWithBleedAspect( shooter );
					return;
				case AspectType.Poison:
					FireBulletWithPoisonAspect( shooter );
					return;
				default:
					FireDefaultBullet( shooter );
					return;
			}
		}



		if ( Owner.MoveSpeed > 150f ) return;
		Owner.ApplyRecoil( Recoil );
		EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
		EffectRenderer?.Set( "b_attack", true );
		EffectRenderer?.Set( "b_reload", false );
		NextAttackTime = 1f / FireRate;
		AmmoInClip--;





		var attachment = EffectRenderer.GetAttachment( "muzzle" );
		var startPos = attachment?.Position ?? Owner.PlyCamera.WorldPosition;
		var direction = Owner.PlyCamera.WorldRotation.Forward;
		direction += Vector3.Random * Spread;
		var endPos = startPos + direction * 5000f;

		var trace = Scene.Trace.Ray( startPos, endPos )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "player" )
			.UseHitboxes( true )
			.Run();

		// Setze endPos auf die Trefferposition, wenn etwas getroffen wird
		if ( trace.Hit )
		{
			endPos = trace.EndPosition;
		}

		if ( Trail != null )
		{
			var trailInstance = ResourceLibrary.Get<PrefabFile>( Trail.ResourcePath ); // Korrigiere die Eigenschaft
			if ( trailInstance != null )
			{
				var trailobject = GameObject.Clone( trailInstance );
				if ( trailobject != null )
				{
					trailobject.WorldPosition = startPos; // Setze die Startposition auf die Mündung

					var trailobjectRenderer = trailobject.Components.Get<ParticleEffect>();
					{
						if ( trailobjectRenderer != null )
						{
							trailobjectRenderer.Yaw = Rotation.LookAt( direction ).Yaw();
							trailobjectRenderer.Pitch = Rotation.LookAt( direction ).Pitch();

						}
					}


					var speed = BulletSpeed * 1000f; // Geschwindigkeit des Schusses basierend auf BulletSpeed
					UpdateTrailObjectPosition( trailobject, direction, speed, endPos, shooter );
				}
			}
		}




		SendAttackMessage( startPos, endPos, trace.Distance, trace );
	}

	private void FireShotgun( Player shooter )
	{




		int pelletCount = 9;
		float spreadAngle = 15f; // Kegelwinkel in Grad

		var attachment = EffectRenderer.GetAttachment( "muzzle" );
		var startPos = attachment?.Position ?? Owner.PlyCamera.WorldPosition;
		var forwardDirection = Owner.PlyCamera.WorldRotation.Forward;

		for ( int i = 0; i < pelletCount; i++ )
		{
			var randomDirection = GetRandomDirectionInCone( forwardDirection, spreadAngle );
			var endPos = startPos + randomDirection * 5000f;

			var trace = Scene.Trace.Ray( startPos, endPos )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.WithoutTags( "player" )
				.UseHitboxes( true )
				.Run();

			if ( trace.Hit )
			{
				endPos = trace.EndPosition;
				var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
				if ( damageable != null )
				{
					int damage = CalculateDamage( shooter );
					damageable.TakeDamage( DamageType.Bullet, damage, trace.EndPosition, trace.Direction * DamageForce, shooter.GameObject.Id, shooter.GameObject.Id );
				}
				SendImpactMessage( trace.EndPosition, trace.Normal );
			}

			if ( Trail != null )
			{
				var trailInstance = ResourceLibrary.Get<PrefabFile>( Trail.ResourcePath );
				if ( trailInstance != null )
				{
					var trailobject = GameObject.Clone( trailInstance );
					if ( trailobject != null )
					{
						trailobject.WorldPosition = startPos;
						var trailobjectRenderer = trailobject.Components.Get<ParticleEffect>();
						if ( trailobjectRenderer != null )
						{
							trailobjectRenderer.Yaw = Rotation.LookAt( randomDirection ).Yaw();
							trailobjectRenderer.Pitch = Rotation.LookAt( randomDirection ).Pitch();
						}
						var speed = BulletSpeed * 1000f;
						UpdateTrailObjectPosition( trailobject, randomDirection, speed, endPos, shooter );
					}
				}
			}

			SendAttackMessage( startPos, endPos, trace.Distance, trace );
		}


	}

	private Vector3 GetRandomDirectionInCone( Vector3 forward, float angle )
	{
		var random = new Random();
		float randomYaw = (float)(random.NextDouble() * angle - angle / 2);
		float randomPitch = (float)(random.NextDouble() * angle - angle / 2);
		var rotation = Rotation.FromYaw( randomYaw ) * Rotation.FromPitch( randomPitch );
		return rotation * forward;
	}

	private bool hasStoppedActions = false;
	protected virtual void OnReloadEnd()
	{
		var ammoToTake = ClipSize - AmmoInClip;
		Owner.Ammo.TryTake( AmmoType, ammoToTake, out var taken );
		AmmoInClip += taken;

		EffectRenderer?.Set( "b_empty", false );
		EffectRenderer?.Set( "b_reload", false ); // Beendet die Nachladeanimation

		// Beende auch die Animation am ModelRenderer in Third-Person
		if ( Owner?.CameraMode != 0 )
		{
			Owner?.ModelRenderer.Set( "b_reload", false );
		}
		var item = GameObject.Components.Get<ItemComponent>();
		if ( item != null )
		{
			item.SaveWeaponAmmoState( this );

		}

		IsReloading = false;
	}
	private void StopAllActions()
	{
		IsFiering = false;
		IsReloading = false;
		StopCharging(); // Beende auch das Aufladen
		ReloadSound?.Stop();
		EffectRenderer?.Set( "b_reload", false );
		EffectRenderer?.Set( "b_attack", false );
		EffectRenderer?.Set( "deage_shoot", false );
	
		
	}
	protected override void OnUpdate()
	{
		// Bestehenden Code beibehalten
		if ( Player.Local != null && Player.Local.LifeState == LifeState.Dead && !hasStoppedActions )
		{
			StopAllActions();
			hasStoppedActions = true;
		}

		if ( Player.Local != null && Player.Local.LifeState != LifeState.Dead )
		{
			hasStoppedActions = false;
		}

		// Überprüfe den Ladezustand
		if ( IsCharging )
		{
			

			// Speichere den vorherigen Ladezustand
			WasFullyCharged = ChargeComplete;
		}

		// Rest des OnUpdate-Codes
		if ( NextAttackTime && IsFiering && IsAuto )
		{
			FireBullet( Player.Local );
		}

		if ( !IsProxy && ReloadFinishTime && IsReloading )
		{
			OnReloadEnd();
		}

		// ... (Rest des bestehenden OnUpdate-Codes)

		base.OnUpdate();
	}
	private bool WasFullyCharged = false;
	[Rpc.Broadcast]
	private void SendReloadMessage()
	{

		if ( Player.Local == null )
		{

			return;
		}

		if ( Player.Local.LifeState == LifeState.Dead )
		{
			// Spieler ist tot, keine Reload-Nachricht senden
			return;
		}

		if ( ReloadSoundSequence == null )
			return;

		// Stoppe den aktuellen ReloadSound, falls er existiert
		ReloadSound?.Stop();


		// Initialisiere den ReloadSound neu
		ReloadSound = new( AmmoInClip == 0 ? EmptyReloadSoundSequence : ReloadSoundSequence );

		ReloadSound.Start( WorldPosition );
	}

	[Rpc.Broadcast]
	private void SendEmptyClipMessage()
	{
		if ( Player.Local == null || Player.Local.LifeState == LifeState.Dead )
		{
			// Spieler ist tot oder Player.Local ist null, keine Reload-Nachricht senden
			return;
		}
		if ( EmptyClipSound != null && !IsSoundPlaying )
		{
			if ( Transform == null )
			{
				Log.Warning( "Transform is null." );
				return;
			}
			Sound.Play( EmptyClipSound, WorldPosition );
			IsSoundPlaying = true;
			SoundDuration = EmptyClipSoundDuration; // Setzen Sie die Dauer des Sounds
		}
	}

	[Rpc.Broadcast]
	private void SendImpactMessage( Vector3 position, Vector3 normal )
	{
		if ( Player.Local == null || Player.Local.LifeState == LifeState.Dead )
		{
			// Spieler ist tot oder Player.Local ist null, keine Nachricht senden
			return;
		}
		if ( Scene.SceneWorld == null )
		{
			throw new InvalidOperationException( "SceneWorld is null." );
		}
		if ( ImpactEffect is null ) return;


		/* 
				var p = new SceneParticles( Scene.SceneWorld, ImpactEffect );
				p.SetControlPoint( 0, position );
				p.SetControlPoint( 0, Rotation.LookAt( normal ) );
				p.PlayUntilFinished( Task ); */
	}

	[Rpc.Broadcast]
	private void SendMeleeAttackMessage( Vector3 startPos, Vector3 endPos, float distance )
	{
		if ( IsMelee ) // Überprüfe, ob der Boolean-Wert wahr ist
		{
			if ( Player.Local == null || Player.Local.LifeState == LifeState.Dead )
			{
				// Spieler ist tot, keine Nachricht senden
				return;
			}
			if ( Scene.SceneWorld == null )
			{
				throw new InvalidOperationException( "SceneWorld is null." );
			}


			if ( FireSound != null )
			{
				Sound.Play( FireSound, startPos );
			}
			else
			{

			}

		}
	}

	private async void UpdateTrailObjectPosition( GameObject trailobject, Vector3 direction, float speed, Vector3 endPos, Player shooter )
	{
		var startTime = Time.Now;
		var duration = 20.0f; // Dauer der Bewegung in Sekunden, anpassen nach Bedarf

		while ( Time.Now - startTime < duration )
		{
			trailobject.WorldPosition += direction * speed * Time.Delta;

			// Logge die aktuelle Position des Trail-Objekts


			// Überprüfen, ob das Objekt die Endposition erreicht hat oder etwas trifft
			var trace = Scene.Trace.Ray( trailobject.WorldPosition, trailobject.WorldPosition + direction * 100f )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.WithoutTags( "player" )
				.UseHitboxes( true )
				.Run();

			if ( trace.Hit )
			{
				// Logge die Trefferinformationen


				// Berechne den Schaden
				var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
				if ( damageable != null )
				{
					int damage = CalculateDamage( shooter );
					damageable.TakeDamage( DamageType.Bullet, damage, trace.EndPosition, trace.Direction * DamageForce, shooter.GameObject.Id, shooter.GameObject.Id );

					// Erzeuge ein Treffer-Feedback
					CreateHitFeedback( trace.EndPosition, damage, shooter );


				}
				if ( ImpactArea != null )
				{
					var impactInstance = ResourceLibrary.Get<PrefabFile>( ImpactArea.ResourcePath );
					if ( impactInstance != null )
					{
						var impactObject = GameObject.Clone( impactInstance );
						if ( impactObject != null )
						{
							impactObject.WorldPosition = trace.EndPosition;
							impactObject.WorldRotation = Rotation.LookAt( trace.Normal );

							var impactRenderer = impactObject.Components.Get<ParticleEffect>();
							if ( impactRenderer != null )
							{
								impactRenderer.Yaw = Rotation.LookAt( direction ).Yaw();
								impactRenderer.Pitch = Rotation.LookAt( direction ).Pitch();

							}
						}

					}

				}


				trailobject.Destroy(); // Zerstöre das Objekt
				return;
			}

			if ( (trailobject.WorldPosition - endPos).Length < 1.0f ) // Überprüfen, ob das Objekt die Endposition erreicht hat
			{
				// Logge das Erreichen der Endposition


				trailobject.Destroy(); // Zerstöre das Objekt
				return;
			}

			await Task.Delay( 10 ); // Aktualisiere die Position alle 10 Millisekunden
		}

		// Logge das Ende der Bewegung


		trailobject.Destroy(); // Zerstöre das Objekt nach Ablauf der Dauer
	}
	private int CalculateDamage( Player shooter )
	{
		Random random = new Random();
		float playerAttackValue = random.Next( (int)shooter.MinAttackValue, (int)shooter.MaxAttackValue + 10 ) / 4;
		var playerAttackPower = shooter.AttackPower;

		int damage = (int)(Damage * (playerAttackValue / 15.0f));
		int calculatedDamage = (int)(damage * (playerAttackPower / 5f) * 0.05);
		int minDamage = 0;
		int maxDamage = calculatedDamage + 1;

		// Ensure minDamage is not greater than maxDamage
		if ( minDamage > maxDamage )
		{
			minDamage = maxDamage;
		}

		damage += random.Next( minDamage, maxDamage );

		int critRoll = random.Next( 0, 101 );
		if ( critRoll <= shooter.CritHitChance )
		{
			damage += (int)(damage * 0.5f + shooter.CritHitDamage);
		}

		return damage;
	}

	private void CreateHitFeedback( Vector3 position, int damage, Player shooter )
	{
		Vector3 randomOffset = new Vector3(
			new Random().Next( -15, -10 ) * (new Random().Next( 0, 2 ) * 2 - 1), // Zufällige Verschiebung auf der X-Achse, links oder rechts
			new Random().Next( -15, -10 ) * (new Random().Next( 0, 2 ) * 2 - 1), // Zufällige Verschiebung auf der Y-Achse, oben oder unten
			new Random().Next( -15, 10 )  // Zufällige Verschiebung auf der Z-Achse
		);

		GameObject hitinfo = Hitprefab.Clone( position + randomOffset );
		FaceThing facething = hitinfo.Components.Get<FaceThing>();
		facething.Thing = shooter.GameObject;
		TextRenderer textRenderer = hitinfo.Components.Get<TextRenderer>();

		textRenderer.Color = Color.White;
		textRenderer.Text = $"{damage}";
		ScaleTextWithDistance scaleTextWithDistance = hitinfo.Components.Get<ScaleTextWithDistance>();
		scaleTextWithDistance.Thing = shooter.GameObject;


	}
	[Rpc.Broadcast]
	private void SendAttackMessage( Vector3 startPos, Vector3 endPos, float distance, SceneTraceResult trace )
	{
		if ( Player.Local == null || Player.Local.LifeState == LifeState.Dead && !IsMelee )
		{
			// Spieler ist tot, keine Nachricht senden
			return;
		}

		if ( Scene.SceneWorld == null )
		{
			throw new InvalidOperationException( "SceneWorld is null." );
		}

		// Sound-Position bestimmen - abhängig vom Kamera-Modus
		Vector3 soundPosition;
		Transform? muzzleTransform = null;

		if ( Owner.CameraMode == 0 ) // First-Person
		{
			// Versuche, das Mündungs-Attachment vom EffectRenderer zu bekommen
			if ( EffectRenderer?.SceneModel != null )
			{
				muzzleTransform = EffectRenderer.SceneModel.GetAttachment( "muzzle" );
				soundPosition = muzzleTransform?.Position ?? startPos;
			}
			else
			{
				soundPosition = startPos;
			}
		}
		else // Third-Person
		{
			// Versuche, das Mündungs-Attachment vom ModelRenderer des Spielers zu bekommen
			var weaponBone = Owner.ModelRenderer.Components.GetAll<SkinnedModelRenderer>();
			soundPosition = startPos; // Fallback

			foreach ( var renderer in weaponBone )
			{
				var attachment = renderer.GetAttachment( "muzzle" );
				if ( attachment != null )
				{
					muzzleTransform = attachment;
					soundPosition = attachment.Value.Position;
					break;
				}
			}
		}
		// Muzzle Flash anzeigen (wenn verfügbar)
		if ( MuzzleFlash != null && muzzleTransform.HasValue )
		{
			var muzzleFlashInstance = ResourceLibrary.Get<PrefabFile>( MuzzleFlash.ResourcePath );
			if ( muzzleFlashInstance != null )
			{
				var muzzleFlash = GameObject.Clone( muzzleFlashInstance );
				if ( muzzleFlash != null )
				{
					muzzleFlash.WorldPosition = muzzleTransform.Value.Position;
					muzzleFlash.WorldRotation = Rotation.LookAt( trace.Direction );
					_ = DestroyMuzzleFlashAfterDelay( muzzleFlash, 1.0f );
				}
			}
		}



		var itemComponent = Components.Get<ItemComponent>();
		if ( itemComponent != null )
		{
			switch ( itemComponent.Aspect )
			{
				case AspectType.Fire:
					// Feueraspekt implementieren
					Sound.Play( "prefabs/hit/fire-sounds/breath.sound", soundPosition );
					Sound.Play( FireSound, soundPosition );
					return;

				case AspectType.Water:
					Sound.Play( "sounds/aspects/water/water.sound", soundPosition );
					return;

				case AspectType.Ice:
					Sound.Play( FireSound, soundPosition );
					_ = PlayDelayedSound( "sounds/aspects/shadow.sound", soundPosition, 0.5f );
					return;

				case AspectType.Air:
					Sound.Play( FireSound, soundPosition );
					return;

				case AspectType.Earth:
					Sound.Play( "sounds/impacts/bullets/impact-bullet-sand.sound", soundPosition );
					return;

				case AspectType.Lightning:
					Sound.Play( "sounds/fireaspect.sound", soundPosition );
					return;

				case AspectType.Shadow:
					Sound.Play( FireSound, soundPosition );
					_ = PlayDelayedSound( "sounds/aspects/shadow.sound", soundPosition, 0.5f );
					return;

				case AspectType.Holy:
					Sound.Play( FireSound, soundPosition );
					_ = PlayDelayedSound( "sounds/aspects/shadow.sound", soundPosition, 0.5f );
					return;

				case AspectType.Bleed:
					Sound.Play( FireSound, soundPosition );
					_ = PlayDelayedSound( "sounds/aspects/shadow.sound", soundPosition, 0.5f );
					return;

				case AspectType.Poison:
					Sound.Play( "prefabs/hit/fire-sounds/breath.sound", soundPosition );
					Sound.Play( FireSound, soundPosition );
					return;

				default:
					if ( FireSound != null )
					{
						Sound.Play( FireSound, soundPosition );
					}
					else
					{
						Log.Warning( "FireSound is null." );
					}
					return;
			}
		}


	}
	private async Task PlayDelayedSound( string soundPath, Vector3 position, float delayInSeconds )
	{
		await Task.Delay( (int)(delayInSeconds * 1000) );
		Sound.Play( soundPath, position );
	}
	private async Task DestroyMuzzleFlashAfterDelay( GameObject muzzleFlash, float delay )
	{
		await Task.Delay( 1000 );
		if ( muzzleFlash != null )
		{
			muzzleFlash.Destroy();
		}

	}
	public class DamageText : Panel
	{
		private Label label;

		public DamageText( Vector3 position, float damage )
		{
			// Erstellen Sie das Text-Label und fügen Sie es dem RootPanel hinzu
			label = Add.Label( $"{damage}", "damage-text" );

			// Fügen Sie eine Ausblendanimation hinzu
			label.AddClass( "fade-out" );

			// Setzen Sie die Position des Panels
			Style.Left = Length.Pixels( position.x );
			Style.Top = Length.Pixels( position.y );
		}

		public static void Create( Vector3 position, float damage, float fadeDuration )
		{
			// Erstellen Sie eine neue Instanz von DamageText
			var damageText = new DamageText( position, damage );

			// Fügen Sie eine Ausblendanimation hinzu
			damageText.label.Style.Set( "animation-duration", $"{fadeDuration}s" );
		}
	}
}
[Title( "Mündungspunkt" ), Category( "Waffen" ), Icon( "adjust" )]
public sealed class MuzzlePoint : Component
{
	[Property]
	public Vector3 LocalOffset { get; set; } = Vector3.Zero;

	[Property]
	public Angles Rotation { get; set; } = Angles.Zero;

	/// <summary>
	/// Gibt die Weltposition des Mündungspunkts zurück
	/// </summary>
	public new Vector3 WorldPosition => Transform.World.PointToWorld( LocalOffset );

	/// <summary>
	/// Gibt die Weltrotation des Mündungspunkts zurück
	/// </summary>
	public new Rotation WorldRotation => Transform.World.RotationToWorld( Rotation );

	/// <summary>
	/// Gibt den Vorwärtsvektor des Mündungspunkts zurück
	/// </summary>
	public Vector3 Forward => WorldRotation.Forward;
}


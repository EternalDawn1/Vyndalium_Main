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

public partial class  BaseGun : WeaponComponent, IUse
{
	[Property]public bool IsMelee { get; set; }

	[Property]
	public PrefabFile Trail { get; set; } 

	[Property] PrefabFile ImpactArea { get; set; }

	[Property] public bool IsShotgun { get; set; } = false;
	[Property, Category( "Parameters" )] public DamageType DamageType { get; set; } = DamageType.Serious;
	[Property, Category( "Parameters" )] public WeaponType Type { get; set; }
	[Property, Category( "Parameters" )] public float ReloadTime { get; set; } = 2f;
	[Property, Category( "Parameters" )] public float EmptyReloadTime { get; set; } = 2f;
	[Property, Category( "Parameters" ), Feature( "Weapon Properties" )] public float Spread { get; set; } = 0.01f;
	[Property, Category( "Parameters" ), Feature( "Weapon Properties" )] public float HitForce { get; set; } = 300;

	[Property, Category( "Parameters" ),Range(0, 0.1f, 10), Feature( "Weapon Properties" )] public float BulletSpeed { get; set; } = 1f;


	[Property, Category( "Parameters_melee" )] public float MeleeRange { get; set; } = 1.5f;
	[Property, Category( "Parameters_melee" )] public float MeleeDamage { get; set; } = 10f;
	[Property, Category( "Parameters_melee" )] public float MeleeCooldown { get; set; } = 1f;



	public TimeUntil NextMeleeAttackTime { get; set; }
	[Property] public Angles Recoil { get; set; }
	[Property, Feature( "Weapon Properties" )] public SoundEvent FireSound { get; set; }
	[Property] public bool IsAuto { get; set; } = false;
	[Property] public SoundEvent EmptyClipSound { get; set; }
	[Property] public SoundSequenceData ReloadSoundSequence { get; set; }
	[Property] public SoundSequenceData EmptyReloadSoundSequence { get; set; }
	[Property] public PrefabFile MuzzleFlash { get; set; }
	[Property] public ParticleSystem ImpactEffect { get; set; }
	[Property] public AmmoType AmmoType { get; set; } = AmmoType.Pistol;
	[Property] public int DefaultAmmo { get; set; } = 1;
	[Property, Feature( "Weapon Properties" )] public int ClipSize { get; set; } = 15;
	[Sync] public bool IsReloading { get; set; }
	[Sync,Property] public int AmmoInClip { get; set; }
	public SoundSequence ReloadSound { get; set; }
	public TimeUntil ReloadFinishTime { get; set; }
	public bool IsFiering { get; set; } = false;
	public bool IsHeld { get; private set; }
	private bool IsSoundPlaying { get; set; } = false;
	private float SoundDuration { get; set; } = 0f;
	private const float EmptyClipSoundDuration = 1f;
	public ItemComponent item { get; set; }
	[Sync] public int MaxAmmo { get; set; }// Add this line

	private int AmmoCount;
	public bool IsEquipped { get; set; }

	public bool isCriticalHit = false;

	public ChargeComponent chargeComponent { get; set; }
	[Property, Feature( "Weapon Properties" )] public bool IsMagicWeapon { get; set; }

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
	[Property] public LineRenderer lineRenderer { get; set; }

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
			chargeComponent = Components.GetOrCreate<ChargeComponent>();
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
			if ( chargeComponent != null && chargeComponent.IsFullyCharged() )
			{
				chargeComponent.FullCharged(); // Aufruf der neuen Methode

				// Katapultiere den Spieler nach vorne
				var ownerPlayer = Owner as Player;
				if ( ownerPlayer != null && ownerPlayer.PlyCamera != null && ownerPlayer.CharacterController != null )
				{
					var forward = ownerPlayer.PlyCamera.WorldRotation.Forward; // Verwenden Sie die Vorwärtsrichtung der Kamera
					float chargePercentage = chargeComponent.Charge; // Ladezustand in Prozent (0-1)
					float speedMultiplier = 2000f * chargePercentage; // Passen Sie die Geschwindigkeit nach Bedarf an

					// Überprüfen Sie, ob die Kamera nicht zu stark nach oben zeigt
					if ( forward.z < 0.2f ) // Der Wert 0.5 kann angepasst werden, um die Empfindlichkeit zu ändern
					{
						ownerPlayer.CharacterController.Velocity += forward * speedMultiplier;
					}

					// Rendern Sie den Effekt hinter dem Spieler
					if ( Ragdoll != null )
					{
						var ragdoll = Ragdoll.Clone( WorldPosition );
						if ( ragdoll != null )
						{
							ragdoll.WorldRotation = WorldRotation;
							ragdoll.WorldPosition = WorldPosition;
							ragdoll.NetworkSpawn();
						}
					}
				}
				chargeComponent.ResetCharge();
			}
			else
			{
				// Führen Sie die normale Primäraktion aus
				if(IsMelee)
				{
					PerformMeleeAttack( Player.Local );
				}
			}
			// Nahkampfangriff ausführen
		}
		else
		{
			// Fernkampfangriff ausführen
			FireBullet( Player.Local );
		}
	}
	public override void PrimaryActionRelease()
	{
		IsFiering = false;
	}
	[Property]public GameObject Ragdoll { get; set; }
	public override void SecondaryAction()
	{
		Owner.IsAiming = true;

		if ( IsMelee )
		{
			if ( chargeComponent.IsCharging )
			{
				chargeComponent.FullCharged(); // Überprüfen, ob die Aufladung vollständig ist und den Sound abspielen

				chargeComponent.StopCharging();
			}
			else
			{
				chargeComponent.StartCharging();
			}
			
			if ( Player.Local.Mana < 25 )
			{
				// Nicht genug Mana, um die magische Waffe abzufeuern
				return;
			}
			

			Player.Local.ChangeMana( -25 );
			

			var player = Player.Local;
			
			

			// Berechne die Flugbahn des Messers
			Vector3 direction = Owner.PlyCamera.WorldRotation.Forward;

			
			
			// Definiere die Start- und Endposition des Traces
			var startPos = Owner.PlyCamera.WorldPosition;
			var endPos = startPos + direction * 5000f;

			// Führe einen Trace aus, um zu überprüfen, ob das Messer etwas trifft
			var trace = Scene.Trace.Ray( startPos, endPos )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.WithoutTags( "player" )
				.UseHitboxes()
				.UsePhysicsWorld()
				.Run();

			

			// Wenn das Messer etwas trifft, füge Schaden hinzu
			if ( trace.Hit )
			{
				IHealthComponent damageable = null;
				var attachment = EffectRenderer.GetAttachment( "muzzle" );
				var damage = Damage;
				var origin = attachment?.Position ?? startPos;

		

				if ( trace.Component.IsValid() )
				{
					damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
				}


				if ( damageable is not null )
				{



					Random random = new Random();
					float playerAttackValue = random.Next( (int)player.MinAttackValue, (int)player.MaxAttackValue + 1 );
					var playerAttackPower = player.AttackPower;
					var playerCritChance = player.CritHitChance;
					var playerCritDamage = player.CritHitDamage;

					damage += (int)(damage * (playerAttackValue / 150.0f));

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
					SendImpactMessage( trace.EndPosition, trace.Normal );
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
				EffectRenderer.Set( "b_attack", true );
				
				NextMeleeAttackTime = MeleeCooldown;
				
				
			}
			
			


		}

	}
	public override void SeccondaryActionRelease()
	{

		if ( Owner == null )
		{
			return;
		}
		Owner.IsAiming = false;
		
		if (chargeComponent != null && chargeComponent.IsCharging && IsMelee )
		{
			chargeComponent.StopCharging();
			chargeComponent.ResetCharge();
		}
		
		
	}
	[Rpc.Broadcast]
	
	private void PerformMeleeAttack( Player player )
	{
		if ( NextMeleeAttackTime > 0 ) return;

		if ( player == null ) return;


		var attachment = EffectRenderer.GetAttachment( "muzzle" );
		var playerPosition = player.PlyCamera.WorldPosition;
		var forwardDirection = player.PlyCamera.WorldRotation.Forward;

		// Berechnen Sie die Startposition 50 Einheiten vor dem Spieler und 25 Einheiten nach links
		var cameraRight = player.PlyCamera.WorldRotation.Right;
		var startPos = playerPosition + forwardDirection * 50 - cameraRight * 25;

		// Berechnen Sie die Endposition 50 Einheiten vor dem Spieler und 25 Einheiten nach rechts
		var endPos = playerPosition + forwardDirection * 50 + cameraRight * 25;

		// Zeigen Sie den Nahkampfangriff an



		// Führen Sie den Nahkampfangriff aus (Ihre bestehende Logik)
		float slashRadius = 1.0f;
		var trace = Scene.Trace.Sphere( slashRadius, startPos, endPos )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "player" )
			.Size( slashRadius )
			.UseHitboxes()
			.Run();

		var damage = Damage;

		var origin = attachment?.Position ?? startPos;

		SendMeleeAttackMessage( origin, trace.EndPosition, trace.Distance );

		IHealthComponent damageable = null;



		if ( trace.Component.IsValid() )
			damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();


		if ( damageable is not null )
		{



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
			SendImpactMessage( trace.EndPosition, trace.Normal );
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
		EffectRenderer.Set( "b_attack", true );
		NextMeleeAttackTime = MeleeCooldown;
	}


	public override void ReloadAction()
	{
		
		if ( AmmoInClip >= ClipSize || IsReloading )
		{
			EffectRenderer?.Set( "b_reload", false );
			
			return;
		}
		var ammoToTake = ClipSize - AmmoInClip;
		if ( ammoToTake <= 0 )
		{
			
			// Magazin ist bereits voll, Nachladeanimation stoppen
			EffectRenderer?.Set( "b_reload", false );
			Log.Info( "Magazin ist bereits voll, Nachladeanimation gestoppt." );
			return;
		}

		if ( !Owner.IsValid() || IsReloading )
			return;

		if ( !Owner.Ammo.CanTake( AmmoType, ammoToTake, out var taken ) )
			return;

		EffectRenderer?.Set( "b_reload", true );
		ReloadFinishTime = AmmoInClip == 0 ? EmptyReloadTime : ReloadTime;
		IsReloading = true;
		
		SendReloadMessage();
		

		if ( AmmoInClip >= ClipSize )
		{
			
			
			EffectRenderer?.Set( "b_reload", false );
		}
	}
	
	private void FireDefaultBullet( Player shooter )
	{
		
		if ( shooter == null || Owner == null || EffectRenderer == null || Scene == null )
		{
			return;
		}
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


					var speed = (0.8f + BulletSpeed) * 1000f;
					UpdateTrailObjectPosition( trailobject, direction, speed, endPos, shooter );
				}
			}
		}

		SendAttackMessage( startPos, endPos, trace.Distance, trace );
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
				case AspectType.Water:
					FireBulletWithWaterAspect( shooter );
					return;
				case AspectType.Ice:
					FireBulletWithIceAspect( shooter );
					return;
				case AspectType.Air:
					FireBulletWithAirAspect( shooter );
					return;
				case AspectType.Earth:
					FireBulletWithEarthAspect( shooter );
					return;
				case AspectType.Lightning:
					FireBulletWithLightningAspect( shooter );
					return;
				case AspectType.Shadow:
					FireBulletWithShadowAspect( shooter );
					return;
				case AspectType.Holy:
					FireBulletWithHolyAspect( shooter );
					return;
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
		IsReloading = false;
		// Animation stoppen
		EffectRenderer?.Set( "b_reload", false );
	}
	private void StopAllActions()
	{
	
		IsFiering = false;
		IsReloading = false;
		ReloadSound?.Stop();
		EffectRenderer?.Set("b_reload", false);
		EffectRenderer?.Set("b_attack", false);
	}
	private bool hasPlayedChargedSound = false;
	protected override void OnUpdate()
	{
		if ( Player.Local != null && Player.Local.LifeState == LifeState.Dead && !hasStoppedActions )
		{
			StopAllActions();
			hasStoppedActions = true;
		}

		if (Player.Local != null && Player.Local.LifeState != LifeState.Dead)
		{
			hasStoppedActions = false;
		}
		if (NextAttackTime && IsFiering && IsAuto)
		{
			FireBullet(Player.Local);
		}

		if ( !IsProxy && ReloadFinishTime && IsReloading )
		{
			
			OnReloadEnd();
		}

		if ( IsSoundPlaying )
		{
			SoundDuration -= Time.Delta; // Reduzieren Sie die verbleibende Dauer des Sounds

			if ( SoundDuration <= 0 )
			{
				IsSoundPlaying = false;
				SoundDuration = 0;
			}
		}

		ReloadSound?.Update( WorldPosition );

		if ( chargeComponent != null && chargeComponent.IsCharging && IsMelee )
		{
			chargeComponent.UpdateCharge( Time.Delta );
			if ( chargeComponent.IsFullyCharged() )
			{
				if ( !hasPlayedChargedSound )
				{
					Player.Local.PlaySuccessSoundFromPath( "sounds/charged.sound", 0.0125f );
					hasPlayedChargedSound = true; // Markiere, dass der Sound abgespielt wurde
				}
			}
			else
			{
				EffectRenderer.Set( "b_charge", false );

				hasPlayedChargedSound = false; // Zurücksetzen, wenn die Aufladung nicht vollständig ist
			}
		}
		
		base.OnUpdate();
	}
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
	private void SendMeleeAttackMessage(Vector3 startPos , Vector3 endPos, float distance)
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
				Log.Warning( "FireSound is null." );
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
		if ( EffectRenderer.SceneModel == null )
		{
			return;
		}
		
		if ( MuzzleFlash != null )
		{
			if ( EffectRenderer.SceneModel != null )
			{
				var transform = EffectRenderer.SceneModel.GetAttachment( "muzzle" );

				if ( transform.HasValue )
				{
					var muzzleFlashInstance = ResourceLibrary.Get<PrefabFile>( MuzzleFlash.ResourcePath );
					if ( muzzleFlashInstance != null )
					{
						var muzzleFlash = GameObject.Clone( muzzleFlashInstance );
						if ( muzzleFlash != null )
						{
							muzzleFlash.WorldPosition = transform.Value.Position;
							muzzleFlash.WorldRotation = Rotation.LookAt( trace.Direction );
						}
						muzzleFlash.Destroy();
						
					}
					
				}
				
			}
			else
			{
				//Log.Warning("EffectRenderer.SceneModel is null.");
			}
		}



		var itemComponent = Components.Get<ItemComponent>();
		if ( itemComponent != null )
		{
			switch ( itemComponent.Aspect )
			{
				case AspectType.Fire:
					// Feueraspekt implementieren
					var transformfire = EffectRenderer.SceneModel.GetAttachment( "muzzle" );
					{
						if ( transformfire.HasValue )
						{
							Sound.Play( "prefabs/hit/fire-sounds/breath.sound", transformfire.Value.Position );
							Sound.Play( FireSound, transformfire.Value.Position );
							


						}

					}
					return;
				case AspectType.Water:
					Sound.Play( "sounds/aspects/water/water.sound", startPos );
					return;
				case AspectType.Ice:
					var transformice = EffectRenderer.SceneModel.GetAttachment( "muzzle" );
					{
						if ( transformice.HasValue )
						{
							Sound.Play( FireSound, transformice.Value.Position );
							Task.Delay( 5000 );
							Sound.Play( "sounds/aspects/shadow.sound", transformice.Value.Position );


						}

					}
					return;
				case AspectType.Air:
					var transformair= EffectRenderer.SceneModel.GetAttachment( "muzzle" );
					{
						if ( transformair.HasValue )
						{
							
							Sound.Play( FireSound, transformair.Value.Position );
						
							


						}

					}
					return;
				case AspectType.Earth:
					Sound.Play( "sounds/impacts/bullets/impact-bullet-sand.sound", startPos );
					return;
				case AspectType.Lightning:
					Sound.Play( "sounds/fireaspect.sound", startPos );
					return;
				case AspectType.Shadow:
					var transformshadow = EffectRenderer.SceneModel.GetAttachment( "muzzle" );
					{
						if ( transformshadow.HasValue )
						{
							Sound.Play( FireSound, transformshadow.Value.Position );
							Task.Delay( 5000 );
							Sound.Play( "sounds/aspects/shadow.sound", transformshadow.Value.Position );
							
							
						}
						
					}
					
					return;
				case AspectType.Holy:
					Sound.Play( "sounds/fireaspect.sound", startPos );
					return;
				case AspectType.Bleed:
					var transformbleed = EffectRenderer.SceneModel.GetAttachment( "muzzle" );
					{
						if ( transformbleed.HasValue )
						{
							Sound.Play( FireSound, transformbleed.Value.Position );
							Task.Delay( 5000 );
							Sound.Play( "sounds/aspects/shadow.sound", transformbleed.Value.Position );


						}

					}
					return;
				case AspectType.Poison:
					Sound.Play( "sounds/fireaspect.sound", startPos );
					return;
				default:
					if ( FireSound != null )
					{
						if ( EffectRenderer.SceneModel != null )
						{
							var transform = EffectRenderer.SceneModel.GetAttachment( "muzzle" );

							if ( transform.HasValue )
							{
								// Spiele den FireSound an der Position der Mündung ab
								Sound.Play( FireSound, transform.Value.Position );
							}
							else
							{
								Log.Warning( "Muzzle attachment not found." );
							}
						}
						else
						{
							Log.Warning( "EffectRenderer.SceneModel is null." );
						}
					}
					else
					{
						Log.Warning( "FireSound is null." );
					}
					return;
			}
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
public class ChargeComponent : Component

	
	{
		public float Charge { get; private set; }
		public bool IsCharging { get; private set; }
		public bool HasPlayedChargedSound { get; set; } = false;

		public void StartCharging()
		{
			IsCharging = true;
			Charge = 0f;
			HasPlayedChargedSound = false; // Zurücksetzen, wenn das Laden beginnt
		}

		public void StopCharging()
		{
			IsCharging = false;
		}

		public void UpdateCharge( float deltaTime )
		{
			if ( IsCharging )
			{
				Charge += deltaTime;
				if ( Charge > 1f )
				{
					Charge = 1f;
				}
			}
		}
		public void FullCharged()
		{
			if ( IsFullyCharged() && !HasPlayedChargedSound )
			{
				
				Player.Local.PlaySuccessSoundFromPath( "/sounds/chargedattack.sound", 0.0125f );
				HasPlayedChargedSound = true; // Markiere, dass der Sound abgespielt wurde
			}
		}

		public bool IsFullyCharged()
		{
		
			return Charge >= 1f;
		}

		public void ResetCharge()
		{
			Charge = 0f;
			HasPlayedChargedSound = false; // Zurücksetzen, wenn die Aufladung zurückgesetzt wird
		}
	}
// Neue Komponente, um den Sound zu verfolgen

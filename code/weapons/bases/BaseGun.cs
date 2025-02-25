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

public class  BaseGun : WeaponComponent, IUse
{
	[Property]public bool IsMelee { get; set; }
	[Property, Category( "Parameters" )] public DamageType DamageType { get; set; } = DamageType.Serious;
	[Property, Category( "Parameters" )] public WeaponType Type { get; set; }
	[Property, Category( "Parameters" )] public float ReloadTime { get; set; } = 2f;
	[Property, Category( "Parameters" )] public float EmptyReloadTime { get; set; } = 2f;
	[Property, Category( "Parameters" )] public float Spread { get; set; } = 0.01f;
	[Property, Category( "Parameters" )] public float HitForce { get; set; } = 300;


	[Property, Category( "Parameters_melee" )] public float MeleeRange { get; set; } = 1.5f;
	[Property, Category( "Parameters_melee" )] public float MeleeDamage { get; set; } = 10f;
	[Property, Category( "Parameters_melee" )] public float MeleeCooldown { get; set; } = 1f;

	public TimeUntil NextMeleeAttackTime { get; set; }
	[Property] public Angles Recoil { get; set; }
	[Property] public SoundEvent FireSound { get; set; }
	[Property] public bool IsAuto { get; set; } = false;
	[Property] public SoundEvent EmptyClipSound { get; set; }
	[Property] public SoundSequenceData ReloadSoundSequence { get; set; }
	[Property] public SoundSequenceData EmptyReloadSoundSequence { get; set; }
	[Property] public ParticleSystem MuzzleFlash { get; set; }
	[Property] public ParticleSystem ImpactEffect { get; set; }
	[Property] public AmmoType AmmoType { get; set; } = AmmoType.Pistol;
	[Property] public int DefaultAmmo { get; set; } = 1;
	[Property] public int ClipSize { get; set; } = 15;
	[Sync] public bool IsReloading { get; set; }
	[Sync] public int AmmoInClip { get; set; }
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
	[Property] public bool IsMagicWeapon { get; set; }

	public void InitializeAmmo( AmmoContainer ammoContainer )
	{
		if ( ammoContainer != null )
		{
			AmmoCount = ammoContainer.GetAmmoCount( AmmoType );
	
		}
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
			chargeComponent = Components.GetOrCreate<ChargeComponent>();
		}
		Components.GetOrCreate<Interactions>();

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

			string attackType = "default";
			var itemComponent = GetComponent<ItemComponent>();
			if ( itemComponent == null )
			{
				switch ( itemComponent.Aspect )
				{
					case AspectType.Fire:
						attackType = "fire";
						break;
					case AspectType.Water:
						attackType = "water";
						break;
					case AspectType.Ice:
						attackType = "ice";
						break;
					case AspectType.Air:
						attackType = "air";
						break;
					case AspectType.Earth:
						attackType = "earth";
						break;
					case AspectType.Shadow:
						attackType = "shadow";
						break;
					case AspectType.Holy:
						attackType = "holy";
						break;
					case AspectType.Bleed:
						attackType = "bleed";
						break;
					case AspectType.Poison:
						attackType = "poison";
						break;
					default:
						attackType = "default";
						break;
				}
			}
			
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

				SendAttackMessage( origin, trace.EndPosition, trace.Distance , attackType );

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


	public override void ReloadAction()
	{
		if ( IsReloading ) return;
		var ammoToTake = ClipSize - AmmoInClip;
		if ( ammoToTake <= 0 )
		{
			// Magazin ist bereits voll, Nachladeanimation stoppen
			EffectRenderer?.Set( "b_reload", false );
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
	}
	[Property]public LineRenderer lineRenderer { get; set; }

	[Rpc.Broadcast]

	public void ShowMeleeAttack( Vector3 origin, Vector3 endPosition )
	{
		if ( lineRenderer == null )
		{
			Log.Error( "LineRenderer is not assigned." );
			return;
		}

		// Alpha-Wert auf den Standardwert zurücksetzen und aktivieren
		SetLineRendererAlpha( lineRenderer, 1.0f );
		lineRenderer.Enabled = true;

		lineRenderer.UseVectorPoints = true;
		lineRenderer.VectorPoints = new List<Vector3> { origin, endPosition };

		// Kollisionsabfrage
		var trace = Scene.Trace.Ray( origin, endPosition )
			.WithoutTags( "player" )
			.Run();

		if ( trace.Hit )
		{
			IHealthComponent damageable = null;

			if ( trace.Component.IsValid() )
				damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
			if ( damageable != null )
			{
				var damage = Damage;
				damageable.TakeDamage( DamageType.Bullet, damage, trace.EndPosition, trace.Direction * DamageForce, GameObject.Id, GameObject.Id );
			}
		}

		// Starten Sie die asynchrone Methode
		_ = FadeLineRenderer( lineRenderer, 0.5f );
	}
	private async Task FadeLineRenderer( LineRenderer lineRenderer, float duration )
	{
		float halfDuration = duration / 2f;
		float elapsedTime = 0f;

		// Einblenden
		while ( elapsedTime < halfDuration )
		{
			elapsedTime += Time.Delta;
			float alpha = elapsedTime / halfDuration;
			SetLineRendererAlpha( lineRenderer, alpha );
			await Task.Delay( 5 ); // Kleinere Verzögerung für glatteres Fading
		}

		// Ausblenden
		elapsedTime = 0f;
		while ( elapsedTime < halfDuration )
		{
			elapsedTime += Time.Delta;
			float alpha = 1f - (elapsedTime / halfDuration);
			SetLineRendererAlpha( lineRenderer, alpha );
			await Task.Delay( 5 ); // Kleinere Verzögerung für glatteres Fading
		}

		// Linie deaktivieren
		lineRenderer.Enabled = false;
	}

	private void SetLineRendererAlpha( LineRenderer lineRenderer, float alpha )
	{
		var color = lineRenderer.Color;
		color.AddAlpha( 0, alpha ); // Setzen Sie den Alpha-Wert der Farbe
		lineRenderer.Color = color; // Setzen Sie die modifizierte Farbe zurück an den LineRenderer
	}
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
		ShowMeleeAttack( startPos, endPos );
		

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

	private void FireBulletWithFireAspect( Player shooter )
	{
		int firedamage = CalculateFireDamage( shooter );

		var trace = Scene.Trace.Ray( shooter.PlyCamera.WorldPosition, shooter.PlyCamera.WorldPosition + shooter.PlyCamera.WorldRotation.Forward * 5000f )
			.IgnoreGameObjectHierarchy( shooter.GameObject.Root )
			.WithoutTags( "player" )
			.UseHitboxes()
			.Run();
		
		if ( trace.Hit )
		{
			var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
			if ( damageable != null )
			{
				damageable.TakeDamage( DamageType.fire, firedamage, trace.EndPosition, trace.Direction * DamageForce, shooter.GameObject.Id, shooter.GameObject.Id );
				

				ApplyFireAspectPassive( damageable );
			}
		}
	}

	private void FireBulletWithWaterAspect( Player shooter )
	{
		

		// Berechne den Wasserschaden
		int waterDamage = CalculateWaterDamage( shooter );

		// Führe einen Trace aus, um zu überprüfen, ob das Geschoss etwas trifft
		var trace = Scene.Trace.Ray( shooter.PlyCamera.WorldPosition, shooter.PlyCamera.WorldPosition + shooter.PlyCamera.WorldRotation.Forward * 5000f )
			.IgnoreGameObjectHierarchy( shooter.GameObject.Root )
			.WithoutTags( "player" )
			.UseHitboxes()
			.Run();

		if ( trace.Hit )
		{
			var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
			if ( damageable != null )
			{
				// Wende den Wasserschaden an
				damageable.TakeDamage( DamageType.water, waterDamage, trace.EndPosition, trace.Direction * DamageForce, shooter.GameObject.Id, shooter.GameObject.Id );

				// Aktiviere die passive Fähigkeit des Wasseraspekts
				ApplyWaterAspectPassive( damageable );
			}
		}
	}
	private void FireBulletWithIceAspect( Player shooter )
	{
		// Implementiere die Logik für das Abfeuern eines Eis-Aspekt-Geschosses
		
		// Beispiel: Erzeuge ein Eisprojektil
	}

	private void FireBulletWithAirAspect( Player shooter )
	{
		int airDamage = 5; // Beispielhafter Luftschaden

		var trace = Scene.Trace.Ray( shooter.PlyCamera.WorldPosition, shooter.PlyCamera.WorldPosition + shooter.PlyCamera.WorldRotation.Forward * 5000f )
			.IgnoreGameObjectHierarchy( shooter.GameObject.Root )
			.WithoutTags( "player" )
			.UseHitboxes()
			.Run();
		
		if ( trace.Hit )
		{
			var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
			if ( damageable != null )
			{
				damageable.TakeDamage( DamageType.air, airDamage, trace.EndPosition, trace.Direction * DamageForce, shooter.GameObject.Id, shooter.GameObject.Id );
				ApplyAirAspectPassive( damageable );
			}
		}
	}

	private void FireBulletWithEarthAspect( Player shooter )
	{
		// Implementiere die Logik für das Abfeuern eines Erd-Aspekt-Geschosses
		
		// Beispiel: Erzeuge ein Erdprojektil
	}

	private void FireBulletWithShadowAspect( Player shooter )
	{
		// Implementiere die Logik für das Abfeuern eines Schatten-Aspekt-Geschosses
	
		// Beispiel: Erzeuge ein Schattenprojektil
	}
	private void FireBulletWithLightningAspect( Player shooter )
	{
		// Implementiere die Logik für das Abfeuern eines Blitz-Aspekt-Geschosses
	
		// Beispiel: Erzeuge ein Blitzprojektil
	}

	private void FireBulletWithHolyAspect( Player shooter )
	{
		// Implementiere die Logik für das Abfeuern eines Heilig-Aspekt-Geschosses

		// Beispiel: Erzeuge ein Heiligprojektil
	}

	private void FireBulletWithBleedAspect( Player shooter )
	{
		int bleedDamage = 5; // Beispielhafter Blutungsschaden

		var trace = Scene.Trace.Ray( shooter.PlyCamera.WorldPosition, shooter.PlyCamera.WorldPosition + shooter.PlyCamera.WorldRotation.Forward * 5000f )
			.IgnoreGameObjectHierarchy( shooter.GameObject.Root )
			.WithoutTags( "player" )
			.UseHitboxes()
			.Run();
		
		if ( trace.Hit )
		{
			var damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
			if ( damageable != null )
			{
				damageable.TakeDamage( DamageType.bleed, bleedDamage, trace.EndPosition, trace.Direction * DamageForce, shooter.GameObject.Id, shooter.GameObject.Id );
			}
			ApplyBleedAspectPassive( damageable , shooter );

		}
	}

	private void FireBulletWithPoisonAspect( Player shooter )
	{
		// Implementiere die Logik für das Abfeuern eines Gift-Aspekt-Geschosses
		
		// Beispiel: Erzeuge ein Giftprojektil
	}

	private void FireDefaultBullet( Player shooter )
	{
		// Implementiere die Standard-Logik für das Abfeuern eines Geschosses
		
		// Beispiel: Erzeuge ein Standardprojektil
	}


	private int CalculateFireDamage( Player shooter )
	{
		// Beispielhafte Berechnung des Feuerschadens
		return (int)(shooter.AttackPower * 0.3f);
	}
	private int CalculateWaterDamage( Player shooter )
	{
		// Beispielhafte Berechnung des Wasserschadens
		return (int)(shooter.AttackPower * 0.2f);
	}
	private void ApplyAirAspectPassive( IHealthComponent damageable )
	{
		// Beispielhafte Implementierung einer passiven Fähigkeit des Luftaspekts
		if ( damageable is Npc npc )
		{
			// Generiere eine Zufallszahl zwischen 0 und 100
			Random random = new Random();
			int chance = random.Next( 0, 100 );

			// Überprüfe, ob die Zufallszahl innerhalb der 10%-Wahrscheinlichkeit liegt
			if ( chance < 10 )
			{
				var knockbackDirection = new Vector3( 0, 0, 50 ); // Beispielhafte Richtung
				var knockbackEffect = new KnockbackEffect( 5, knockbackDirection, 500 ); // Dauer in Sekunden, Richtung und Kraft
				npc.ApplyStatusEffect( knockbackEffect );
			}
		}
	}

	private void ApplyFireAspectPassive( IHealthComponent damageable )
	{
		if ( damageable is Npc npc )
		{
			Random random = new Random();
			int chance = random.Next( 0, 100 );
			if ( chance < 10 )
			{
				var burnEffect = new BurnEffectNpc( 5 ); // Dauer in Sekunden
				npc.ApplyStatusEffect( burnEffect );
			}
		}
	}
	private void ApplyWaterAspectPassive( IHealthComponent damageable )
	{
		// Beispielhafte Implementierung einer passiven Fähigkeit des Wasseraspekts
		if ( damageable is Npc npc )
		{
			// Generiere eine Zufallszahl zwischen 0 und 100
			Random random = new Random();
			int chance = random.Next( 0, 100 );

			// Überprüfe, ob die Zufallszahl innerhalb der 15%-Wahrscheinlichkeit liegt
			if ( chance < 15 )
			{
				var slowEffect = new SlowEffect { Duration = 5 };
				npc.ApplyStatusEffect( slowEffect );
			}
		}
	}
	private void ApplyBleedAspectPassive( IHealthComponent damageable, Player shooter )
	{
		// Beispielhafte Implementierung einer passiven Fähigkeit des Blutungsaspekts
		if ( damageable is Npc npc )
		{
			// Generiere eine Zufallszahl zwischen 0 und 100
			Random random = new Random();
			int chance = random.Next( 0, 100 );

			// Überprüfe, ob die Zufallszahl innerhalb der 20%-Wahrscheinlichkeit liegt
			if ( chance < 20 )
			{
				var bleedEffect = new BleedEffect( 5); // Dauer in Sekunden
				npc.ApplyStatusEffect( bleedEffect );
				// Heile den Angreifer um 1% seines maximalen Lebens
				shooter.Health = Math.Min( shooter.MaxHealth, shooter.Health + shooter.MaxHealth * 0.01f );
			}
		}
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
		if(shooter.IsRunning)
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

		if ( AmmoInClip <= 0 )
		{
		
			SendEmptyClipMessage();
			ReloadAction();
			NextAttackTime = 1f / FireRate;
			return;
		}
		string attackType = "default";
		var itemComponent = Components.Get<ItemComponent>();
		if ( itemComponent != null )

		{
			
			switch ( itemComponent.Aspect )
			{
				case AspectType.Fire:
					FireBulletWithFireAspect( shooter );
					attackType = "fire";
					break;
				case AspectType.Water:
					FireBulletWithWaterAspect( shooter );
					attackType = "water";
					break;
				case AspectType.Ice:
					FireBulletWithIceAspect( shooter );
					attackType = "ice";
					break;
				case AspectType.Air:
					FireBulletWithAirAspect( shooter );
					attackType = "air";
					break;
				case AspectType.Earth:
					FireBulletWithEarthAspect( shooter );
					attackType = "earth";
					break;
				case AspectType.Lightning:
					FireBulletWithLightningAspect( shooter );
					attackType = "lightning";

					break;
				case AspectType.Shadow:
					FireBulletWithShadowAspect( shooter );
					attackType = "shadow";
					break;
				case AspectType.Holy:
					FireBulletWithHolyAspect( shooter );
					attackType = "holy";
					break;
				case AspectType.Bleed:
					FireBulletWithBleedAspect( shooter );
					attackType = "bleed";
					break;
				case AspectType.Poison:
					FireBulletWithPoisonAspect( shooter );
					attackType = "poison";
					break;
				default:
					FireDefaultBullet( shooter );
					break;
			}
		}
		else
		{
			Log.Info( "FireBullet: itemComponent is null" );
			FireDefaultBullet( shooter );
		}



		if ( Owner.MoveSpeed > 150f ) return;
		Owner.ApplyRecoil( Recoil );
		EffectRenderer?.Set( "b_empty", AmmoInClip == 0 );
		EffectRenderer?.Set( "b_attack", true );
		EffectRenderer?.Set( "b_reload", false );
		NextAttackTime = 1f / FireRate;
		AmmoInClip--;

		var attachment = EffectRenderer.GetAttachment( "muzzle" );
		var startPos = Owner.PlyCamera.WorldPosition;
		var direction = Owner.PlyCamera.WorldRotation.Forward;
		direction += Vector3.Random * Spread;

		var endPos = startPos + direction * 5000f;
		var trace = Scene.Trace.Ray( startPos, endPos )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "player" )
			.UseHitboxes()
			.Run();

		var damage = Damage;
		var origin = attachment?.Position ?? startPos;



		SendAttackMessage( origin, trace.EndPosition, trace.Distance , attackType );

		IHealthComponent damageable = null;

		if ( trace.Component.IsValid() )
		{
			damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();
			if ( damageable == null )
			{
				var healthComponent = trace.Component.Components.GetInAncestorsOrSelf<HealthComponent>();
				if ( healthComponent != null )
				{
					
				}
			}

			if ( damageable != null )
			{
				// Fügen Sie hier den Code hinzu, um Schaden zu verursachen
				damageable.TakeDamage( DamageType.Bullet, damage, trace.EndPosition, trace.Direction * DamageForce, shooter.GameObject.Id, shooter.GameObject.Id );
			}
		}



		if ( damageable is not null )
		{
		
			Random random = new Random();

			float playerAttackValue = random.Next( (int)shooter.MinAttackValue, (int)shooter.MaxAttackValue + 10 );
			var playerAttackPower = shooter.AttackPower;

			var playerCritChance = shooter.CritHitChance;
			var playerCritDamage = shooter.CritHitDamage;
			var armorPenetration = shooter.ArmorPenetration;

			damage += (int)(damage * (playerAttackValue / 15.0f));
			

			int calculatedDamage = (int)(damage * (playerAttackPower / 10.0f));
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
			if (damageable is Npc npc)
			{
				var zombieArmor = npc.Armor; // Angenommen, das Ziel hat eine Rüstungseigenschaft
				var effectiveArmor = Math.Max(0, zombieArmor - armorPenetration);
				damage = (int)(damage * (100f / (100f + effectiveArmor)));
			}

			damageable.TakeDamage( DamageType.Bullet, damage, trace.EndPosition, trace.Direction * DamageForce, GameObject.Id, GameObject.Id );


			Vector3 randomOffset = new Vector3(
			random.Next( -15, -10 ) * (random.Next( 0, 2 ) * 2 - 1), // Zufällige Verschiebung auf der X-Achse, links oder rechts
			random.Next( -15, -10 ) * (random.Next( 0, 2 ) * 2 - 1), // Zufällige Verschiebung auf der Y-Achse, oben oder unten
			random.Next( -15, 10 )  // Zufällige Verschiebung auf der Z-Achse
			);


			GameObject hitinfo = Hitprefab.Clone( trace.EndPosition + randomOffset );
			FaceThing facething = hitinfo.Components.Get<FaceThing>();
			facething.Thing = shooter.GameObject;
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
			scaleTextWithDistance.Thing = shooter.GameObject;
			
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
				health.Damage( Damage, DamageType, shooter.GameObject, trace.HitPosition, trace.Direction, HitForce );
		}

		


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
		
		

		var p = new SceneParticles( Scene.SceneWorld, ImpactEffect );
		p.SetControlPoint( 0, position );
		p.SetControlPoint( 0, Rotation.LookAt( normal ) );
		p.PlayUntilFinished( Task );
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
	[Rpc.Broadcast]
	private void SendAttackMessage( Vector3 startPos, Vector3 endPos, float distance, string attackType )
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
			Log.Warning( "EffectRenderer.SceneModel is null." );
			return;
		}

		string particleEffect;
		switch ( attackType )
		{
			case "fire":
				particleEffect = "particles/trail_bullet_fire.vpcf";
				break;
			case "ice":
				particleEffect = "particles/trail_bullet_ice.vpcf";
				break;
			case "water":
				particleEffect = "particles/trail_bullet_water.vpcf";
				break;
			case "shadow":
				particleEffect = "particles/tracer/trail_smoke_shadow.vpcf";
				break;
			case "lightning":
				particleEffect = "particles/trail_bullet_lightning.vpcf";
				break;
			case "air":
				particleEffect = "particles/trail_bullet_air.vpcf";
				break;
			case "earth":
				particleEffect = "particles/trail_bullet_earth.vpcf";
				break;
			case "holy":
				particleEffect = "particles/trail_bullet_holy.vpcf";
				break;
			case "bleed":
				particleEffect = "particles/trail_bullet_bleed.vpcf";
				break;
				
			default:
				particleEffect = "particles/tracer/trail_smoke.vpcf";
				break;
		}

		var trace = Scene.Trace.Ray( startPos, endPos )
		.UseHitboxes()
		.IgnoreGameObjectHierarchy( GameObject.Root )
		.WithoutTags( "player" )// 'this' als gültiges GameObject übergeben
		.Run();

		// Trefferposition ermitteln
		var hitPosition = trace.EndPosition;
		var distanceToHit = (hitPosition - startPos).Length; // Umbenennung von 'distance' zu 'distanceToHit'

		// Partikel erstellen und Kontrollpunkte setzen
		var p = new SceneParticles( Scene.SceneWorld, particleEffect );
		p.SetControlPoint( 0, startPos );
		p.SetControlPoint( 1, hitPosition ); // Endposition des Strahls
		p.SetControlPoint( 2, distanceToHit );

		


		p.PlayUntilFinished( Task );

		if ( MuzzleFlash != null )
		{
			if ( EffectRenderer.SceneModel != null )
			{
				var transform = EffectRenderer.SceneModel.GetAttachment( "muzzle" );

				if ( transform.HasValue )
				{
					p = new SceneParticles( Scene.SceneWorld, MuzzleFlash );
					p.SetControlPoint( 0, transform.Value );
					p.PlayUntilFinished( Task );
				}
			}
			else
			{
				//Log.Warning("EffectRenderer.SceneModel is null.");
			}
		}

		if (FireSound != null)
		{
			if (EffectRenderer.SceneModel != null)
			{
				var transform = EffectRenderer.SceneModel.GetAttachment("muzzle");

				if (transform.HasValue)
				{
					// Spiele den FireSound an der Position der Mündung ab
					Sound.Play(FireSound, transform.Value.Position);
				}
				else
				{
					Log.Warning("Muzzle attachment not found.");
				}
			}
			else
			{
				Log.Warning("EffectRenderer.SceneModel is null.");
			}
		}
		else
		{
			Log.Warning("FireSound is null.");
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


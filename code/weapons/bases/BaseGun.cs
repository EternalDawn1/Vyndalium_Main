using Sandbox;
using System;
using System.Numerics;

namespace GeneralGame;

public class BaseGun : WeaponComponent, IUse
{

	[Property, Category( "Parameters" )] public DamageType DamageType { get; set; } = DamageType.Serious;
	[Property, Category( "Parameters" )] public WeaponType Type { get; set; }
	[Property, Category( "Parameters" )] public float ReloadTime { get; set; } = 2f;
	[Property, Category( "Parameters" )] public float EmptyReloadTime { get; set; } = 2f;
	[Property, Category( "Parameters" )] public float Spread { get; set; } = 0.01f;
	[Property, Category( "Parameters" )] public float HitForce { get; set; } = 300;
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


	[Property] public bool IsMagicWeapon { get; set; }

	public void InitializeAmmo( AmmoContainer ammoContainer )
	{
		if ( ammoContainer != null )
		{
			AmmoCount = ammoContainer.GetAmmoCount( AmmoType );
			DefaultAmmo = ammoContainer.GetDefaultAmmo( AmmoType ); // Stelle sicher, dass DefaultAmmo hier korrekt gesetzt wird
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
		var defaultAmmo = player.AmmoContainer.GetDefaultAmmo( AmmoType.Rifle );
		if ( defaultAmmo > 0 )
		{
			AmmoCount = defaultAmmo;
		}

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
		float baseDamage = player.AttackValue; // Verwenden Sie die AttackValue des Spielers als Basis-Schaden
		float bonusDamage = baseDamage * (player.AttackPower / 100.0f);
		float magicBonus = baseDamage * (player.MagicPower / 100.0f);

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
		AmmoCount += amount;
	}
	public void DecreaseAmmo( int amount )
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
	public int GetAmmoCount()
	{
		return AmmoCount;
	}

	GameObject Hitprefab;

	protected override void OnStart()
	{
		// Standardmunition setzen, wenn sie nicht bereits gesetzt ist
		if ( AmmoCount == 0 )
		{
			AmmoCount = DefaultAmmo;
		}
		Hitprefab = SceneUtility.GetPrefabScene( ResourceLibrary.Get<PrefabFile>( "prefabs/hitinfo.prefab" ) );

		Components.GetOrCreate<Interactions>();

		base.OnStart();
	}

	[Broadcast]
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

			var ammoToGive = DefaultAmmo - player.Ammo.Get( AmmoType );

			if ( ammoToGive > 0 )
			{
				player.Ammo.Give( AmmoType, ammoToGive );
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

		// Die Waffe wird nicht mehr gehalten
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
		FireBullet( Player.Local );
	}

	public override void PrimaryActionRelease()
	{
		IsFiering = false;
	}

	public override void SecondaryAction()
	{
		Owner.IsAiming = true;
	}
	public override void SeccondaryActionRelease()
	{

		if ( Owner == null )
		{
			return;
		}

		Owner.IsAiming = false;
	}

	public override void ReloadAction()
	{


		var ammoToTake = ClipSize - AmmoInClip;
		if ( ammoToTake <= 0 )
			return;


		if ( !Owner.IsValid() || IsReloading )
			return;

		if ( !Owner.Ammo.CanTake( AmmoType, ammoToTake, out var taken ) )
			return;

		EffectRenderer.Set( "b_reload", true );
		ReloadFinishTime = AmmoInClip == 0 ? EmptyReloadTime : ReloadTime;
		IsReloading = true;

		SendReloadMessage();
	}


	public virtual void FireBullet( Player shooter )
	{
		if ( shooter == null || Owner == null || EffectRenderer == null || Scene == null )
		{
			return;
		}
		if ( !NextAttackTime ) return;
		if ( IsReloading ) return;

		if ( AmmoInClip <= 0 )
		{
			SendEmptyClipMessage();
			NextAttackTime = 1f / FireRate;
			return;
		}
		if ( IsMagicWeapon && Player.Local.Mana < 10 )
		{
			// Nicht genug Mana, um die magische Waffe abzufeuern
			return;
		}

		if ( IsMagicWeapon )
		{
			// Verbrauche Mana
			Player.Local.ChangeMana( -10 );
		}


		if ( Owner.MoveSpeed > 150f ) return;
		Owner.ApplyRecoil( Recoil );

		var attachment = EffectRenderer.GetAttachment( "muzzle" );
		var startPos = Owner.PlyCamera.Transform.Position;
		var direction = Owner.PlyCamera.Transform.Rotation.Forward;
		direction += Vector3.Random * Spread;

		var endPos = startPos + direction * 5000f;
		var trace = Scene.Trace.Ray( startPos, endPos )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "player" )
			.UseHitboxes()
			.Run();

		var damage = Damage;
		var origin = attachment?.Position ?? startPos;



		SendAttackMessage( origin, trace.EndPosition, trace.Distance );

		IHealthComponent damageable = null;

		if ( trace.Component.IsValid() )
			damageable = trace.Component.Components.GetInAncestorsOrSelf<IHealthComponent>();


		if ( damageable is not null )
		{
			var playerAttackValue = shooter.AttackValue;
			var playerAttackPower = shooter.AttackPower;
			var playerCritChance = shooter.CritHitChance;
			var playerCritDamage = shooter.CritHitDamage;

			damage += (int)(damage * (playerAttackValue / 300.0f));
			Random random = new Random();
			int calculatedDamage = (int)(damage * (playerAttackPower / 50.0f));
			damage += random.Next( 0, calculatedDamage + 1 );

			int critRoll = random.Next( 0, 101 );
			{
				if ( critRoll <= playerCritChance )
				{
					damage += (int)(damage * 1.5f + playerCritDamage);
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
			/// <summary>
			/// Made from TrollFaceReallife47 thanks <3
			/// </summary>
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

		NextAttackTime = 1f / FireRate;
		AmmoInClip--;


		EffectRenderer.Set( "b_empty", AmmoInClip == 0 );
		EffectRenderer.Set( "b_attack", true );


	}

	




	protected virtual void OnReloadEnd()
	{
		var ammoToTake = ClipSize - AmmoInClip;

		Owner.Ammo.TryTake( AmmoType, ammoToTake, out var taken );
		AmmoInClip += taken;
		EffectRenderer.Set( "b_empty", false );
		IsReloading = false;
	}
	protected override void OnUpdate()
	{
		if ( NextAttackTime && IsFiering && IsAuto ) FireBullet( Player.Local );

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

		ReloadSound?.Update( Transform.Position );


		base.OnUpdate();
	}
	[Broadcast]
	private void SendReloadMessage()
	{
		if ( Player.Local.LifeState == LifeState.Dead )
		{
			// Spieler ist tot, keine Reload-Nachricht senden
			return;
		}
		if ( ReloadSoundSequence is null )
			return;

		ReloadSound?.Stop();

		ReloadSound = new( AmmoInClip == 0 ? EmptyReloadSoundSequence : ReloadSoundSequence );
		ReloadSound.Start( Transform.Position );
	}

	[Broadcast]
	private void SendEmptyClipMessage()
	{
		if ( Player.Local.LifeState == LifeState.Dead )
		{
			// Spieler ist tot, keine Reload-Nachricht senden
			return;
		}
		if ( EmptyClipSound is not null && !IsSoundPlaying )
		{
			Sound.Play( EmptyClipSound, Transform.Position );
			IsSoundPlaying = true;
			SoundDuration = EmptyClipSoundDuration; // Setzen Sie die Dauer des Sounds
		}
	}

	[Broadcast]
	private void SendImpactMessage( Vector3 position, Vector3 normal )
	{
		if ( Player.Local.LifeState == LifeState.Dead )
		{
			// Spieler ist tot, keine Reload-Nachricht senden
			return;
		}
		if ( ImpactEffect is null ) return;

		var p = new SceneParticles( Scene.SceneWorld, ImpactEffect );
		p.SetControlPoint( 0, position );
		p.SetControlPoint( 0, Rotation.LookAt( normal ) );
		p.PlayUntilFinished( Task );
	}

	[Broadcast]
	private void SendAttackMessage(Vector3 startPos, Vector3 endPos, float distance)
	{
		if (Player.Local == null || Player.Local.LifeState == LifeState.Dead)
		{
			// Spieler ist tot, keine Nachricht senden
			return;
		}
		if (Scene.SceneWorld == null)
		{
			throw new InvalidOperationException("SceneWorld is null.");
		}

		var p = new SceneParticles(Scene.SceneWorld, "particles/tracer/trail_smoke.vpcf");
		p.SetControlPoint(0, startPos);
		p.SetControlPoint(1, endPos);
		p.SetControlPoint(2, distance);
		p.PlayUntilFinished(Task);

		if (MuzzleFlash != null)
		{
			if (EffectRenderer.SceneModel != null)
			{
				var transform = EffectRenderer.SceneModel.GetAttachment("muzzle");

				if (transform.HasValue)
				{
					p = new SceneParticles(Scene.SceneWorld, MuzzleFlash);
					p.SetControlPoint(0, transform.Value);
					p.PlayUntilFinished(Task);
				}
			}
			else
			{
				Log.Warning("EffectRenderer.SceneModel is null.");
			}
		}

		if (FireSound != null)
		{
			Sound.Play(FireSound, startPos);
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

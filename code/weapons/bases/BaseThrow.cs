
using Sandbox;
using System;
using System.Diagnostics;
using System.Numerics;
namespace GeneralGame;


public class BaseThrow : WeaponComponent, IUse
{
	[Property] public float PrepareTime { get; set; }
	[Property] public float ReleaseTime { get; set; }
	[Property] public SoundEvent ActivateSound { get; set; }
	[Property] public SoundEvent ThrowSound { get; set; }
	[Property] public GameObject ThrowPrefab { get; set; }
	public TimeUntil CurPrepareTime { get; set; }
	public TimeUntil CurReleaseTime { get; set; }
	public bool IsPreparing { get; set; } = false;
	public bool WaitingThrow { get; set; } = false;

	[Broadcast]
	public virtual void OnUse( Guid pickerId )
	{
		var picker = Scene.Directory.FindByGuid( pickerId );
		if ( !picker.IsValid() ) return;

		var player = picker.Components.GetInDescendantsOrSelf<Player>();
		if ( !player.IsValid() ) return;

		if ( player.IsProxy )
			return;

		if ( !player.Weapons.Has( GameObject ) )
		{
			player.Weapons.Give( GameObject, false );
			GameObject.Destroy();
		}
	}

	public override void PrimaryAction()
	{
		StartPrepare();
	}
	public override void PrimaryActionRelease()
	{
		
		ActivateThrow();
	}
	public override void SecondaryAction()
	{
		
	}


	public virtual void StartPrepare()
	{
		if ( IsPreparing ) return;


		IsPreparing = true;
		CurPrepareTime = PrepareTime;
		CurPrepareTime = ReleaseTime;

		Sound.Play( ActivateSound, WorldPosition );
		EffectRenderer.Set( "b_prepare", true );

	}
	public virtual void ActivateThrow()
	{
		if ( !IsPreparing || CurPrepareTime ) return;
		WaitingThrow = true;
	}

	public virtual void CreateThrow( bool imidiantly )
	{
		var obj = ThrowPrefab.Clone( this.Transform.World );
		obj.NetworkSpawn();
		obj.WorldPosition = Owner.PlyCamera.WorldPosition + Owner.PlyCamera.WorldRotation.Forward * 50;
		obj.WorldRotation = Owner.PlyCamera.WorldRotation;
		obj.Components.Get<Rigidbody>().Velocity = Owner.PlyCamera.WorldRotation.Forward * 1000;
		obj.Components.Get<EntThrow>().explodeTime = imidiantly ? 0f : CurPrepareTime;
	}

	

	protected override void OnUpdate()
	{
		if ( CurPrepareTime && IsPreparing )
		{
			IsPreparing = false;
			CreateThrow( true );
		}
	

		if ( WaitingThrow && CurReleaseTime )
		{
			IsPreparing = false;
			WaitingThrow = false;
			Sound.Play( ThrowSound, WorldPosition );
			EffectRenderer.Set( "b_throw", true );

			CreateThrow( false );
		}
		

		base.OnUpdate();
	}
}

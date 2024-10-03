
using Sandbox;
using System;

namespace GeneralGame;

public partial class DoorEntity : Component, IUse
{
	[Property] public Rotation MoveDir { get; set; }
	[Property] public bool Open { get; set; } = true;
	[Property] public int MoveSpeed { get; set; } = 5;
	public Rotation DefaultRotation { get; set; }

	protected override void OnStart()
	{
		DefaultRotation = WorldRotation;
	}
	protected override void OnUpdate()
	{
		if ( Open )
		{
			LocalRotation = Rotation.Lerp( LocalRotation, DefaultRotation * MoveDir, Time.Delta * 5f );
		}
		else
		{
			LocalRotation = Rotation.Lerp( LocalRotation, DefaultRotation, Time.Delta * 5f );
		}
	}

	[Broadcast]
	public virtual void OnUse( Guid pickerId )
	{
		var picker = Scene.Directory.FindByGuid( pickerId );
		if ( !picker.IsValid() ) return;

		var player = picker.Components.GetInDescendantsOrSelf<Player>();
		if ( !player.IsValid() ) return;

		if ( player.IsProxy )
			return;

		Open = !Open;
	}

}

using System;

public interface IPlayerStatsProvider
{
    PlayerStatsSnapshot Current { get; }
    event Action<PlayerStatsSnapshot> OnStatsChanged;
}

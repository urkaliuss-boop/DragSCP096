namespace DragScp096
{
    using System.Collections.Generic;

    using Exiled.API.Features;

    using MEC;

    /// <summary>
    /// основной класс плагина DragScp096.
    /// </summary>
    public class DragScp096Plugin : Plugin<Config>
    {
        private static DragScp096Plugin singleton = new();

        private EventHandler eventHandler;

        private DragScp096Plugin()
        {
        }

        /// <summary>
        /// Gets singleton экземпляр плагина.
        /// </summary>
        public static DragScp096Plugin Instance => singleton;

        /// <inheritdoc/>
        public override string Name => "DragScp096";

        /// <inheritdoc/>
        public override string Author => "noxiss";

        /// <inheritdoc/>
        public override string Prefix => "drag_scp096";

        /// <summary>
        /// Gets словарь активных захватов: ключ — игрок-захватчик, значение — данные о захвате.
        /// </summary>
        public Dictionary<Player, DragData> ActiveDrags { get; } = new();

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            eventHandler = new EventHandler();

            Exiled.Events.Handlers.Server.RoundEnded += eventHandler.OnRoundEnded;
            Exiled.Events.Handlers.Server.RestartingRound += eventHandler.OnRestartingRound;
            Exiled.Events.Handlers.Player.Died += eventHandler.OnDied;
            Exiled.Events.Handlers.Player.ChangingRole += eventHandler.OnChangingRole;

            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.RoundEnded -= eventHandler.OnRoundEnded;
            Exiled.Events.Handlers.Server.RestartingRound -= eventHandler.OnRestartingRound;
            Exiled.Events.Handlers.Player.Died -= eventHandler.OnDied;
            Exiled.Events.Handlers.Player.ChangingRole -= eventHandler.OnChangingRole;

            StopAllDrags();

            eventHandler = null;

            base.OnDisabled();
        }

        /// <summary>
        /// останавливает захват для конкретного игрока.
        /// </summary>
        /// <param name="dragger">игрок, который тащит SCP-096.</param>
        public void StopDrag(Player dragger)
        {
            if (!ActiveDrags.TryGetValue(dragger, out DragData data))
            {
                return;
            }

            Timing.KillCoroutines(data.Handle);

            if (data.Target != null && !data.Target.IsDead)
            {
                data.Target.DisableEffect(Exiled.API.Enums.EffectType.Ensnared);
            }

            ActiveDrags.Remove(dragger);
        }

        /// <summary>
        /// останавливает все активные захваты.
        /// </summary>
        public void StopAllDrags()
        {
            List<Player> draggers = new(ActiveDrags.Keys);

            foreach (Player dragger in draggers)
            {
                StopDrag(dragger);
            }
        }
    }
}

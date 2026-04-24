namespace DragScp096
{
    using System.Linq;

    using Exiled.API.Features;

    using Exiled.Events.EventArgs.Player;
    using Exiled.Events.EventArgs.Server;

    /// <summary>
    /// Обработчик событий для автоматической остановки захвата.
    /// </summary>
    public class EventHandler
    {
        /// <summary>
        /// Конец раунда.
        /// </summary>
        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            DragScp096Plugin.Instance.StopAllDrags();
        }

        /// <summary>
        /// Рестарт раунда.
        /// </summary>
        public void OnRestartingRound()
        {
            DragScp096Plugin.Instance.StopAllDrags();
        }

        /// <summary>
        /// Смерть игрока.
        /// </summary>
        public void OnDied(DiedEventArgs ev)
        {
            if (ev.Player == null)
            {
                return;
            }

            if (DragScp096Plugin.Instance.ActiveDrags.TryGetValue(ev.Player, out DragData dragData))
            {
                if (dragData.Target != null && !dragData.Target.IsDead)
                {
                    dragData.Target.Broadcast(
                        6,
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                        "<size=30><color=#2ECC40>✓ Вы свободны!</color></size>\n" +
                        "<size=22><color=#E0E0E0>Оперативник <color=#FF4444>погиб</color>. Захват снят.</color></size>\n" +
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");
                }

                DragScp096Plugin.Instance.StopDrag(ev.Player);
            }

            Player dragger = DragScp096Plugin.Instance.ActiveDrags
                .Where(kvp => kvp.Value.Target == ev.Player)
                .Select(kvp => kvp.Key)
                .FirstOrDefault();

            if (dragger != null)
            {
                dragger.Broadcast(
                    5,
                    "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                    "<size=28><color=#FF4444>⚠ SCP-096 мёртв</color></size>\n" +
                    "<size=22><color=#AAAAAA>Захват автоматически снят.</color></size>\n" +
                    "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");

                DragScp096Plugin.Instance.StopDrag(dragger);
            }
        }

        /// <summary>
        /// Смена роли.
        /// </summary>
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null)
            {
                return;
            }

            if (DragScp096Plugin.Instance.ActiveDrags.TryGetValue(ev.Player, out DragData dragData))
            {
                if (dragData.Target != null && !dragData.Target.IsDead)
                {
                    dragData.Target.Broadcast(
                        5,
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                        "<size=30><color=#2ECC40>✓ Вы свободны</color></size>\n" +
                        "<size=22><color=#E0E0E0>Захват снят.</color></size>\n" +
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");
                }

                DragScp096Plugin.Instance.StopDrag(ev.Player);
            }

            Player dragger = DragScp096Plugin.Instance.ActiveDrags
                .Where(kvp => kvp.Value.Target == ev.Player)
                .Select(kvp => kvp.Key)
                .FirstOrDefault();

            if (dragger != null)
            {
                dragger.Broadcast(4, "<size=28><color=#FF4444>⚠ SCP-096 сменил роль — захват снят.</color></size>");
                DragScp096Plugin.Instance.StopDrag(dragger);
            }
        }
    }
}

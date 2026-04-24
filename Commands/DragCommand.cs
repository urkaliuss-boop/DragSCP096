namespace DragScp096.Commands
{
    using System;
    using System.Linq;

    using CommandSystem;

    using Exiled.API.Features;
    using Exiled.API.Features.Roles;

    using MEC;

    using PlayerRoles;

    using UnityEngine;

    /// <summary>
    /// Команда .drag — захватить или отпустить SCP-096
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public class DragCommand : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "drag";

        /// <inheritdoc/>
        public string[] Aliases { get; } = new[] { "drag096" };

        /// <inheritdoc/>
        public string Description { get; } = "Захватить / отпустить SCP-096 (МОГ, Охрана, ПХ).";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (player == null)
            {
                response = "Команду может использовать только игрок.";
                return false;
            }

            if (player.Role.Team != Team.FoundationForces && player.Role.Team != Team.ChaosInsurgency)
            {
                response = "Вы не можете использовать эту команду.";
                return false;
            }

            // тумблер: если уже тащит - отпускаем
            if (DragScp096Plugin.Instance.ActiveDrags.TryGetValue(player, out DragData existingData))
            {
                if (existingData.IsGrabbing)
                {
                    DragScp096Plugin.Instance.StopDrag(player);
                    response = "Захват отменён.";
                    return true;
                }

                existingData.Target?.Broadcast(
                    5,
                    "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                    "<size=30><color=#2ECC40>✓ Вас отпустили</color></size>\n" +
                    "<size=22><color=#E0E0E0>Вы снова свободны.</color></size>\n" +
                    "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");

                DragScp096Plugin.Instance.StopDrag(player);
                response = "Вы отпустили SCP-096.";
                return true;
            }

            Player scp096 = Player.List
                .FirstOrDefault(p => p.Role.Type == RoleTypeId.Scp096 && !p.IsDead);

            if (scp096 == null)
            {
                response = "Не удалось выполнить команду.";
                return false;
            }

            if (scp096.Role is Scp096Role scpRole &&
                scpRole.RageState != PlayerRoles.PlayableScps.Scp096.Scp096RageState.Docile)
            {
                response = "Не удалось выполнить команду.";
                return false;
            }

            bool alreadyDragged = DragScp096Plugin.Instance.ActiveDrags
                .Any(kvp => kvp.Value.Target == scp096);

            if (alreadyDragged)
            {
                response = "Не удалось выполнить команду";
                return false;
            }

            float distance = Vector3.Distance(player.Position, scp096.Position);
            float maxDistance = DragScp096Plugin.Instance.Config.DragDistance;

            if (distance > maxDistance)
            {
                response = distance <= 10f
                    ? $"SCP-096 слишком далеко ({distance:F1}м). Подойдите ближе ({maxDistance}м)."
                    : "Не удалось выполнить команду.";
                return false;
            }

            CoroutineHandle handle = Timing.RunCoroutine(
                DragCoroutine.RunDragLoop(player, scp096));

            DragScp096Plugin.Instance.ActiveDrags[player] = new DragData
            {
                Target = scp096,
                Handle = handle,
                IsGrabbing = true,
            };

            response = "Начинаю захват SCP-096...";
            return true;
        }
    }
}

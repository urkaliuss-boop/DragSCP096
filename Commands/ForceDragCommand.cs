namespace DragScp096.Commands
{
    using System;

    using CommandSystem;

    using Exiled.API.Enums;
    using Exiled.API.Features;

    using MEC;

    /// <summary>
    /// RA-команда forcedrag — принудительный захват для тестирования.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ForceDragCommand : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "forcedrag";

        /// <inheritdoc/>
        public string[] Aliases { get; } = new[] { "fd" };

        /// <inheritdoc/>
        public string Description { get; } = "forcedrag <dragger_id> <target_id> | forcedrag stop <dragger_id> | forcedrag stopall";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Использование:\n" +
                           "forcedrag <dragger_id> <target_id>\n" +
                           "forcedrag stop <dragger_id>\n" +
                           "forcedrag stopall";
                return false;
            }

            string firstArg = arguments.At(0).ToLower();

            if (firstArg == "stopall")
            {
                DragScp096Plugin.Instance.StopAllDrags();
                response = "Все захваты остановлены.";
                return true;
            }

            if (firstArg == "stop")
            {
                if (arguments.Count < 2)
                {
                    response = "Использование: forcedrag stop <dragger_id>";
                    return false;
                }

                Player dragger = Player.Get(arguments.At(1));

                if (dragger == null)
                {
                    response = $"Игрок \"{arguments.At(1)}\" не найден.";
                    return false;
                }

                if (!DragScp096Plugin.Instance.ActiveDrags.ContainsKey(dragger))
                {
                    response = $"{dragger.Nickname} не тащит никого.";
                    return false;
                }

                DragScp096Plugin.Instance.StopDrag(dragger);
                response = $"Захват остановлен для {dragger.Nickname}.";
                return true;
            }

            if (arguments.Count < 2)
            {
                response = "Использование: forcedrag <dragger_id> <target_id>";
                return false;
            }

            Player draggerPlayer = Player.Get(arguments.At(0));
            Player targetPlayer = Player.Get(arguments.At(1));

            if (draggerPlayer == null)
            {
                response = $"Игрок \"{arguments.At(0)}\" не найден.";
                return false;
            }

            if (targetPlayer == null)
            {
                response = $"Игрок \"{arguments.At(1)}\" не найден.";
                return false;
            }

            if (DragScp096Plugin.Instance.ActiveDrags.ContainsKey(draggerPlayer))
            {
                DragScp096Plugin.Instance.StopDrag(draggerPlayer);
            }

            targetPlayer.EnableEffect(EffectType.Ensnared);

            CoroutineHandle handle = Timing.RunCoroutine(
                DragCoroutine.RunForceDragLoop(draggerPlayer, targetPlayer));

            DragScp096Plugin.Instance.ActiveDrags[draggerPlayer] = new DragData
            {
                Target = targetPlayer,
                Handle = handle,
            };

            targetPlayer.Broadcast(
                6,
                "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                "<size=30><color=#FF4444>⚠ Вас схватили!</color></size>\n" +
                "<size=22><color=#E0E0E0>Сотрудник <color=#5B9BD5>" + draggerPlayer.Nickname + "</color> тащит вас за собой.</color></size>\n" +
                "<size=20><color=#AAAAAA>Вы не можете двигаться.</color></size>\n" +
                "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");

            response = $"{draggerPlayer.Nickname} теперь тащит {targetPlayer.Nickname}.";
            return true;
        }
    }
}

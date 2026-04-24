namespace DragScp096
{
    using System.Collections.Generic;

    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;

    using MEC;

    using PlayerRoles;

    using UnityEngine;

    /// <summary>
    /// Корутина перемещения SCP-096 за игроком.
    /// </summary>
    public static class DragCoroutine
    {
        private static readonly string[] GrabProgressBars = new[]
        {
            "<color=#FF4444>||</color><color=#555555>||||</color>",
            "<color=#FF8800>||||</color><color=#555555>||</color>",
            "<color=#2ECC40>||||||</color>",
        };

        /// <summary>
        /// Обратный отсчёт + перетаскивание SCP-096.
        /// </summary>
        public static IEnumerator<float> RunDragLoop(Player dragger, Player target)
        {
            int grabDuration = DragScp096Plugin.Instance.Config.GrabDuration;
            float dragDistance = DragScp096Plugin.Instance.Config.DragDistance;

            // обратный отсчёт
            for (int i = 0; i < grabDuration; i++)
            {
                if (dragger == null || target == null || dragger.IsDead || target.IsDead)
                {
                    DragScp096Plugin.Instance.StopDrag(dragger);
                    yield break;
                }

                if (dragger.Role.Team != Team.FoundationForces && dragger.Role.Team != Team.ChaosInsurgency)
                {
                    DragScp096Plugin.Instance.StopDrag(dragger);
                    yield break;
                }

                if (target.Role.Type != RoleTypeId.Scp096)
                {
                    DragScp096Plugin.Instance.StopDrag(dragger);
                    yield break;
                }

                if (target.Role is Scp096Role scpCheck &&
                    scpCheck.RageState != PlayerRoles.PlayableScps.Scp096.Scp096RageState.Docile)
                {
                    dragger.Broadcast(
                        4,
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                        "<size=28><color=#FF4444>⚠ SCP-096 пришёл в ярость!</color></size>\n" +
                        "<size=22><color=#AAAAAA>Захват невозможен.</color></size>\n" +
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");

                    DragScp096Plugin.Instance.StopDrag(dragger);
                    yield break;
                }

                float distance = Vector3.Distance(dragger.Position, target.Position);

                if (distance > dragDistance)
                {
                    dragger.Broadcast(
                        4,
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                        "<size=28><color=#FF4444>⚠ SCP-096 вырвался!</color></size>\n" +
                        "<size=22><color=#AAAAAA>Объект слишком далеко.</color></size>\n" +
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");

                    target.Broadcast(
                        4,
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                        "<size=28><color=#2ECC40>✓ Вы вырвались!</color></size>\n" +
                        "<size=22><color=#AAAAAA>Захват прерван.</color></size>\n" +
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");

                    DragScp096Plugin.Instance.StopDrag(dragger);
                    yield break;
                }

                int remaining = grabDuration - i;
                string bar = GrabProgressBars[i < GrabProgressBars.Length ? i : GrabProgressBars.Length - 1];

                dragger.Broadcast(
                    2,
                    "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                    "<size=28><color=#FFD700>Захват SCP-096...</color></size>\n" +
                    $"<size=36><b>{bar}</b></size>\n" +
                    $"<size=24><color=#E0E0E0>{remaining}...</color></size>\n" +
                    "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");

                target.Broadcast(
                    2,
                    "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                    "<size=28><color=#FF4444>⚠ Вас пытаются схватить!</color></size>\n" +
                    $"<size=36><b>{bar}</b></size>\n" +
                    $"<size=24><color=#E0E0E0>{remaining}...</color></size>\n" +
                    "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");

                yield return Timing.WaitForSeconds(1f);
            }

            // финальная проверка
            if (dragger == null || target == null || dragger.IsDead || target.IsDead)
            {
                DragScp096Plugin.Instance.StopDrag(dragger);
                yield break;
            }

            if (Vector3.Distance(dragger.Position, target.Position) > dragDistance)
            {
                dragger.Broadcast(4, "<size=28><color=#FF4444>⚠ SCP-096 вырвался в последний момент!</color></size>");
                target.Broadcast(4, "<size=28><color=#2ECC40>✓ Вы вырвались!</color></size>");
                DragScp096Plugin.Instance.StopDrag(dragger);
                yield break;
            }

            if (target.Role is Scp096Role scpFinal &&
                scpFinal.RageState != PlayerRoles.PlayableScps.Scp096.Scp096RageState.Docile)
            {
                dragger.Broadcast(4, "<size=28><color=#FF4444>⚠ SCP-096 пришёл в ярость! Захват невозможен.</color></size>");
                DragScp096Plugin.Instance.StopDrag(dragger);
                yield break;
            }

            // захват успешен
            target.EnableEffect(EffectType.Ensnared);

            if (DragScp096Plugin.Instance.ActiveDrags.TryGetValue(dragger, out DragData data))
            {
                data.IsGrabbing = false;
            }

            dragger.Broadcast(
                5,
                "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                "<size=32><b><color=#2ECC40>✓ SCP-096 захвачен!</color></b></size>\n" +
                "<size=22><color=#E0E0E0>Используйте <color=#FFD700>.drag</color> чтобы отпустить.</color></size>\n" +
                "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");

            target.Broadcast(
                6,
                "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                "<size=30><color=#FF4444>⚠ Вас схватили!</color></size>\n" +
                "<size=22><color=#E0E0E0>Сотрудник <color=#5B9BD5>" + dragger.Nickname + "</color> тащит вас за собой.</color></size>\n" +
                "<size=20><color=#AAAAAA>Вы не можете двигаться.</color></size>\n" +
                "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");

            // перетаскивание
            float followDistance = DragScp096Plugin.Instance.Config.FollowDistance;
            float maxBreakDistance = DragScp096Plugin.Instance.Config.MaxDistanceBreak;
            float updateInterval = DragScp096Plugin.Instance.Config.UpdateInterval;
            const float movementThreshold = 0.15f;
            const float lerpSpeed = 0.25f;

            if (DragScp096Plugin.Instance.ActiveDrags.TryGetValue(dragger, out DragData dragData))
            {
                dragData.LastDraggerPosition = dragger.Position;
                dragData.LastTargetDestination = target.Position;
            }

            Vector3 lastMoveDir = dragger.CameraTransform.forward;
            lastMoveDir.y = 0f;
            lastMoveDir.Normalize();

            while (true)
            {
                yield return Timing.WaitForSeconds(updateInterval);

                if (dragger == null || target == null || dragger.IsDead || target.IsDead)
                {
                    DragScp096Plugin.Instance.StopDrag(dragger);
                    yield break;
                }

                if (dragger.Role.Team != Team.FoundationForces && dragger.Role.Team != Team.ChaosInsurgency)
                {
                    DragScp096Plugin.Instance.StopDrag(dragger);
                    yield break;
                }

                if (target.Role.Type != RoleTypeId.Scp096)
                {
                    DragScp096Plugin.Instance.StopDrag(dragger);
                    yield break;
                }

                if (target.Role is Scp096Role scpRage &&
                    scpRage.RageState != PlayerRoles.PlayableScps.Scp096.Scp096RageState.Docile)
                {
                    dragger.Broadcast(
                        4,
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                        "<size=28><color=#FF4444>⚠ SCP-096 вырвался в ярости!</color></size>\n" +
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");

                    target.Broadcast(
                        4,
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                        "<size=28><color=#FF4444>Ярость! Захват разорван!</color></size>\n" +
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");

                    DragScp096Plugin.Instance.StopDrag(dragger);
                    yield break;
                }

                Vector3 pos = dragger.Position;
                float dist = Vector3.Distance(pos, target.Position);

                if (dist > maxBreakDistance)
                {
                    dragger.Broadcast(4, "<size=28><color=#FF4444>⚠ SCP-096 сорвался с захвата!</color></size>");

                    target.Broadcast(
                        4,
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>\n" +
                        "<size=30><color=#2ECC40>Вы свободны</color></size>\n" +
                        "<size=22><color=#E0E0E0>Захват разорван.</color></size>\n" +
                        "<size=26><color=#B0B0B0>━━━━━━━━━━━━━━━━━━━━━━━━</color></size>");

                    DragScp096Plugin.Instance.StopDrag(dragger);
                    yield break;
                }

                if (!DragScp096Plugin.Instance.ActiveDrags.TryGetValue(dragger, out DragData cd))
                {
                    yield break;
                }

                float moved = Vector3.Distance(pos, cd.LastDraggerPosition);

                if (moved > movementThreshold)
                {
                    Vector3 delta = pos - cd.LastDraggerPosition;
                    delta.y = 0f;

                    if (delta.sqrMagnitude > 0.001f)
                    {
                        lastMoveDir = delta.normalized;
                    }

                    Vector3 desired = pos - (lastMoveDir * followDistance);
                    Vector3 safe = GetSafePosition(pos, desired);

                    cd.LastTargetDestination = Vector3.Lerp(target.Position, safe, lerpSpeed);
                    cd.LastDraggerPosition = pos;
                    target.Position = cd.LastTargetDestination;
                }
                else
                {
                    if (Vector3.Distance(pos, target.Position) > followDistance + 1.0f)
                    {
                        Vector3 pullBack = pos - (lastMoveDir * followDistance);
                        target.Position = Vector3.Lerp(target.Position, pullBack, 0.1f);
                    }

                    cd.LastDraggerPosition = pos;
                }
            }
        }

        /// <summary>
        /// Перетаскивание без проверок ролей (для RA-команды forcedrag).
        /// </summary>
        public static IEnumerator<float> RunForceDragLoop(Player dragger, Player target)
        {
            float followDistance = DragScp096Plugin.Instance.Config.FollowDistance;
            float maxBreakDistance = DragScp096Plugin.Instance.Config.MaxDistanceBreak;
            float updateInterval = DragScp096Plugin.Instance.Config.UpdateInterval;
            const float movementThreshold = 0.15f;
            const float lerpSpeed = 0.25f;

            if (DragScp096Plugin.Instance.ActiveDrags.TryGetValue(dragger, out DragData initData))
            {
                initData.LastDraggerPosition = dragger.Position;
                initData.LastTargetDestination = target.Position;
            }

            Vector3 lastMoveDir = dragger.CameraTransform.forward;
            lastMoveDir.y = 0f;
            lastMoveDir.Normalize();

            while (true)
            {
                yield return Timing.WaitForSeconds(updateInterval);

                if (dragger == null || target == null || dragger.IsDead || target.IsDead)
                {
                    DragScp096Plugin.Instance.StopDrag(dragger);
                    yield break;
                }

                Vector3 pos = dragger.Position;
                float dist = Vector3.Distance(pos, target.Position);

                if (dist > maxBreakDistance)
                {
                    dragger.Broadcast(4, "<size=28><color=#FF4444>⚠ Цель сорвалась с захвата!</color></size>");
                    DragScp096Plugin.Instance.StopDrag(dragger);
                    yield break;
                }

                if (!DragScp096Plugin.Instance.ActiveDrags.TryGetValue(dragger, out DragData cd))
                {
                    yield break;
                }

                float moved = Vector3.Distance(pos, cd.LastDraggerPosition);

                if (moved > movementThreshold)
                {
                    Vector3 delta = pos - cd.LastDraggerPosition;
                    delta.y = 0f;

                    if (delta.sqrMagnitude > 0.001f)
                    {
                        lastMoveDir = delta.normalized;
                    }

                    Vector3 desired = pos - (lastMoveDir * followDistance);
                    Vector3 safe = GetSafePosition(pos, desired);

                    cd.LastTargetDestination = Vector3.Lerp(target.Position, safe, lerpSpeed);
                    cd.LastDraggerPosition = pos;
                    target.Position = cd.LastTargetDestination;
                }
                else
                {
                    if (Vector3.Distance(pos, target.Position) > followDistance + 1.0f)
                    {
                        Vector3 pullBack = pos - (lastMoveDir * followDistance);
                        target.Position = Vector3.Lerp(target.Position, pullBack, 0.1f);
                    }

                    cd.LastDraggerPosition = pos;
                }
            }
        }

        private static Vector3 GetSafePosition(Vector3 from, Vector3 to)
        {
            Vector3 dir = to - from;
            float dist = dir.magnitude;

            if (dist < 0.01f)
            {
                return from;
            }

            if (Physics.Raycast(from, dir.normalized, out RaycastHit hit, dist))
            {
                return hit.point - (dir.normalized * 0.3f);
            }

            return to;
        }
    }
}

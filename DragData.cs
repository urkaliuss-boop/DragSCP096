namespace DragScp096
{
    using Exiled.API.Features;

    using MEC;

    using UnityEngine;

    /// <summary>
    /// Данные об активном захвате.
    /// </summary>
    public class DragData
    {
        /// <summary>
        /// Gets or sets цель захвата.
        /// </summary>
        public Player Target { get; set; }

        /// <summary>
        /// Gets or sets хэндл корутины.
        /// </summary>
        public CoroutineHandle Handle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether идёт фаза обратного отсчёта.
        /// </summary>
        public bool IsGrabbing { get; set; }

        /// <summary>
        /// Gets or sets последнюю позицию захватчика.
        /// </summary>
        public Vector3 LastDraggerPosition { get; set; }

        /// <summary>
        /// Gets or sets последнюю точку назначения цели.
        /// </summary>
        public Vector3 LastTargetDestination { get; set; }
    }
}

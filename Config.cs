namespace DragScp096
{
    using System.ComponentModel;

    using Exiled.API.Interfaces;

    /// <summary>
    /// конфигурация плагина DragScp096.
    /// </summary>
    public sealed class Config : IConfig
    {
        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc/>
        public bool Debug { get; set; } = false;

        /// <summary>
        /// Gets or sets максимальная дистанция для захвата SCP-096 (в метрах).
        /// </summary>
        [Description("Максимальная дистанция для захвата SCP-096 (в метрах).")]
        public float DragDistance { get; set; } = 3.0f;

        /// <summary>
        /// Gets or sets дистанция следования SCP-096 за спиной игрока.
        /// </summary>
        [Description("Дистанция, на которой SCP-096 следует за спиной игрока.")]
        public float FollowDistance { get; set; } = 1.5f;

        /// <summary>
        /// Gets or sets максимальная дистанция, при которой захват разрывается.
        /// </summary>
        [Description("Максимальная дистанция, при которой захват автоматически разрывается.")]
        public float MaxDistanceBreak { get; set; } = 10.0f;

        /// <summary>
        /// Gets or sets интервал обновления позиции (в секундах).
        /// </summary>
        [Description("Интервал обновления позиции SCP-096 (в секундах). Рекомендуется 0.1.")]
        public float UpdateInterval { get; set; } = 0.1f;

        /// <summary>
        /// Gets or sets время захвата в секундах (обратный отсчёт перед началом перетаскивания).
        /// </summary>
        [Description("Время захвата (обратный отсчёт) в секундах. SCP-096 может сбежать за это время.")]
        public int GrabDuration { get; set; } = 3;
    }
}

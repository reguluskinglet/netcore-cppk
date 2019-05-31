namespace TabletLocker.Enums
{
    public enum AppState
    {
        Begin,

        Init,

        InitError,

        /// <summary>
        /// начальное состояние
        /// </summary>
        MainMenu,

        /// <summary>
        /// Введен пользователь
        /// </summary>
        UserSelected,

        /// <summary>
        /// Введено устройство
        /// </summary>
        DeviceSelected,

        /// <summary>
        /// Открыта дверца для возврата девайса
        /// </summary>
        DeviceReturnInProgress,

        DeviceTakeInProgress,
        /// <summary>
        /// Окно ввода проблемы с девайсом
        /// </summary>
        TroubleInput,

        TroubleInputWaitCellClose,

        MainError,

        NoFreeDevices,

        AdminLogin,

        AdminList,

        AdminCellDetail,

        AdminCellOpened,

        AdminCellOpenReasonSelected,
        //new
        AdminClearDbConfirm,
        //new
        AdminClearDbStart,
        //new
        AdminClearDbFinished
    }
}
namespace MESS.Services.DarkMode
{
    /// <summary>
    /// Represents a singleton service used to manage and observe the application's dark mode state.
    /// </summary>
    public class DarkModeInstance
    {
        /// <summary>
        /// Gets a value indicating whether dark mode is currently enabled.
        /// </summary>
        public bool IsDarkMode { get; private set; }

        /// <summary>
        /// Event triggered when the dark mode state changes.
        /// Subscribers can respond to state changes to update their UI accordingly.
        /// </summary>
        public event Action? OnChange;

        /// <summary>
        /// Toggles the current dark mode state and invokes the <see cref="OnChange"/> event.
        /// </summary>
        public void Toggle()
        {
            IsDarkMode = !IsDarkMode;
            OnChange?.Invoke();
        }

        /// <summary>
        /// Sets the dark mode state to a specified value and invokes the <see cref="OnChange"/> event.
        /// </summary>
        /// <param name="value">True to enable dark mode; false to disable it.</param>
        public void Set(bool value)
        {
            IsDarkMode = value;
            OnChange?.Invoke();
        }
    }
}
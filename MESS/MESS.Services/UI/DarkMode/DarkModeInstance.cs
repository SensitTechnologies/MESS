using System.ComponentModel;

namespace MESS.Services.UI.DarkMode
{
    /// <summary>
    /// Represents a singleton service used to manage and observe the application's dark mode state.
    /// Implements <see cref="INotifyPropertyChanged"/> to notify UI components when the state changes.
    /// </summary>
    public class DarkModeInstance : INotifyPropertyChanged
    {
        private bool _isDarkMode;

        /// <summary>
        /// Gets a value indicating whether dark mode is currently enabled.
        /// </summary>
        public bool IsDarkMode
        {
            get => _isDarkMode;
            private set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDarkMode)));
                }
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Toggles the current dark mode state.
        /// </summary>
        public void Toggle()
        {
            IsDarkMode = !IsDarkMode;
        }

        /// <summary>
        /// Sets the dark mode state to a specified value.
        /// </summary>
        /// <param name="value">True to enable dark mode; false to disable it.</param>
        public void Set(bool value)
        {
            IsDarkMode = value;
        }
    }
}
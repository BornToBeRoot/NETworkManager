using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace NETworkManager.Controls
{
    public class GroupExpanderStateStore : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when a single group's expansion state changes via the indexer (not raised by <see cref="LoadState"/>).
        /// </summary>
        public event EventHandler<GroupExpandedChangedArgs> GroupExpandedChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Stores the expansion state of each group by its name.
        /// </summary>
        private readonly Dictionary<string, bool> _states = [];

        /// <summary>
        /// The indexer to get or set the expansion state of a group by its name.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <returns>True if expanded, false if collapsed.</returns>
        public bool this[string groupName]
        {
            get
            {
                // Default to expanded if not set
                if (!_states.TryGetValue(groupName, out var val))
                    _states[groupName] = val = true;

                return val;
            }
            set
            {
                if (_states.TryGetValue(groupName, out var existing) && existing == value)
                    return;

                Debug.WriteLine("GroupExpanderStateStore: Setting state of '{0}' to {1}", groupName, value);

                _states[groupName] = value;
                OnPropertyChanged($"Item[{groupName}]");
                GroupExpandedChanged?.Invoke(this, new GroupExpandedChangedArgs(groupName, value));
            }
        }

        /// <summary>
        /// Replaces all stored states with the given ones (e.g. bulk-loading previously persisted state).
        /// Does not raise <see cref="PropertyChanged"/> or <see cref="GroupExpandedChanged"/> - consumers
        /// (e.g. <see cref="GroupExpander"/>) read the current state fresh whenever they bind, so no
        /// notification is needed, and this avoids a load-triggers-save feedback loop.
        /// </summary>
        public void LoadState(IReadOnlyDictionary<string, bool> state)
        {
            _states.Clear();

            if (state == null)
                return;

            foreach (var kvp in state)
                _states[kvp.Key] = kvp.Value;
        }
    }
}

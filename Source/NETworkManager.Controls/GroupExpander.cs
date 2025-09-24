using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Controls
{
    public class GroupExpander : Expander
    {
        public static readonly DependencyProperty StateStoreProperty =
            DependencyProperty.Register(
                "StateStore",
                typeof(GroupExpanderStateStore),
                typeof(GroupExpander),
                new PropertyMetadata(null, OnStateStoreChanged));

        public GroupExpanderStateStore StateStore
        {
            get => (GroupExpanderStateStore)GetValue(StateStoreProperty);
            set => SetValue(StateStoreProperty, value);
        }

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register(
                nameof(GroupName),
                typeof(string),
                typeof(GroupExpander),
                new PropertyMetadata(null, OnGroupNameChanged));

        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        static GroupExpander()
        {
            IsExpandedProperty.OverrideMetadata(
                   typeof(GroupExpander),
                   new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsExpandedChanged));
        }

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var expander = (GroupExpander)d;

            if (expander.StateStore != null && expander.GroupName != null)
                expander.StateStore[expander.GroupName] = (bool)e.NewValue;
        }

        private static void OnStateStoreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var expander = (GroupExpander)d;

            if (e.OldValue is GroupExpanderStateStore oldStore)
                oldStore.PropertyChanged -= expander.StateStore_PropertyChanged;

            if (e.NewValue is GroupExpanderStateStore newStore)
                newStore.PropertyChanged += expander.StateStore_PropertyChanged;

            expander.UpdateIsExpanded();
        }

        private void StateStore_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == $"Item[{GroupName}]")
                UpdateIsExpanded();
        }

        private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var expander = (GroupExpander)d;

            expander.UpdateIsExpanded();
        }

        private void UpdateIsExpanded()
        {
            if (StateStore == null || GroupName == null)
                return;

            // Prevent recursive updates
            if (IsExpanded == StateStore[GroupName])
                return;

            IsExpanded = StateStore[GroupName];
        }
    }
}

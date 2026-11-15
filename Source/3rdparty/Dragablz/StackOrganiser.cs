using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Dragablz.Core;

namespace Dragablz
{
    public abstract class StackOrganiser : IItemsOrganiser
    {
        private readonly Orientation _orientation;
        private readonly double _itemOffset;
        private readonly Func<DragablzItem, double> _getDesiredSize;
        private readonly Func<DragablzItem, double> _getLocation;
        private readonly DependencyProperty _canvasDependencyProperty;
        private readonly Action<DragablzItem, double> _setLocation;

        private readonly Dictionary<DragablzItem, double> _activeStoryboardTargetLocations =
            new Dictionary<DragablzItem, double>();

        //NETworkManager: the running slide per item, to stop it when the item is placed directly or removed
        private readonly Dictionary<DragablzItem, Storyboard> _activeStoryboards =
            new Dictionary<DragablzItem, Storyboard>();

        protected StackOrganiser(Orientation orientation, double itemOffset = 0)
        {
            _orientation = orientation;
            _itemOffset = itemOffset;

            switch (orientation)
            {
                case Orientation.Horizontal:
                    _getDesiredSize = item => item.DesiredSize.Width;
                    _getLocation = item => item.X;
                    _setLocation = (item, coord) => item.SetCurrentValue(DragablzItem.XProperty, coord);
                    _canvasDependencyProperty = Canvas.LeftProperty;
                    break;
                case Orientation.Vertical:
                    _getDesiredSize = item => item.DesiredSize.Height;
                    _getLocation = item => item.Y;
                    _setLocation = (item, coord) => item.SetCurrentValue(DragablzItem.YProperty, coord);
                    _canvasDependencyProperty = Canvas.TopProperty;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("orientation");
            }
        }

        #region LocationInfo

        private class LocationInfo
        {
            private readonly DragablzItem _item;
            private readonly double _start;
            private readonly double _mid;
            private readonly double _end;

            public LocationInfo(DragablzItem item, double start, double mid, double end)
            {
                _item = item;
                _start = start;
                _mid = mid;
                _end = end;
            }

            public double Start
            {
                get { return _start; }
            }

            public double Mid
            {
                get { return _mid; }
            }

            public double End
            {
                get { return _end; }
            }

            public DragablzItem Item
            {
                get { return _item; }
            }
        }

        #endregion

        public virtual Orientation Orientation
        {
            get { return _orientation; }
        }

        public virtual void Organise(DragablzItemsControl requestor, Size measureBounds, IEnumerable<DragablzItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            OrganiseInternal(
                requestor,
                measureBounds,
                items.Select((di, idx) => new Tuple<int, DragablzItem>(idx, di))
                        .OrderBy(tuple => tuple,
                            //NETworkManager: the target of a running slide, not X / Y (still the old location) - otherwise
                            //an Organise during a slide restores the old order (SetLocation stops the slide)
                            MultiComparer<Tuple<int, DragablzItem>>.Ascending(tuple => GetLocationInfo(tuple.Item2).Start)
                                .ThenAscending(tuple => tuple.Item1))
                        .Select(tuple => tuple.Item2));
        }

        public virtual void Organise(DragablzItemsControl requestor, Size measureBounds, IOrderedEnumerable<DragablzItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            OrganiseInternal(
                requestor,
                measureBounds,
                items);
        }

        private void OrganiseInternal(DragablzItemsControl requestor, Size measureBounds,
            IEnumerable<DragablzItem> items)
        {
            var currentCoord = 0.0;
            var z = int.MaxValue;
            var logicalIndex = 0;
            foreach (var newItem in items)
            {
                Panel.SetZIndex(newItem, newItem.IsSelected ? int.MaxValue : --z);
                SetLocation(newItem, currentCoord);
                newItem.LogicalIndex = logicalIndex++;
                newItem.Measure(measureBounds);
                var desiredSize = _getDesiredSize(newItem);
                if (desiredSize == 0.0) desiredSize = 1.0; //no measure? create something to help sorting
                currentCoord += desiredSize + _itemOffset;
            }
        }


        public virtual void OrganiseOnMouseDownWithin(DragablzItemsControl requestor, Size measureBounds,
            List<DragablzItem> siblingItems, DragablzItem dragablzItem)
        {

        }

        private IDictionary<DragablzItem, LocationInfo> _siblingItemLocationOnDragStart;

        //NETworkManager: an item added during the drag has no start location (KeyNotFoundException) - use its current one
        private double GetStartOnDragStart(LocationInfo location)
        {
            LocationInfo locationOnDragStart;
            return _siblingItemLocationOnDragStart != null &&
                   _siblingItemLocationOnDragStart.TryGetValue(location.Item, out locationOnDragStart)
                ? locationOnDragStart.Start
                : location.Start;
        }

        public virtual void OrganiseOnDragStarted(DragablzItemsControl requestor, Size measureBounds,
            IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            if (siblingItems == null) throw new ArgumentNullException("siblingItems");
            if (dragItem == null) throw new ArgumentNullException("dragItem");

            //NETworkManager: a tab grabbed while it still slides (e.g. right after the previous drag) must follow the
            //cursor - the slide would hold Canvas.Left and set its old target on completion
            StopSlide(dragItem);

            _siblingItemLocationOnDragStart = siblingItems.Select(GetLocationInfo).ToDictionary(loc => loc.Item);
        }

        public virtual void OrganiseOnDrag(DragablzItemsControl requestor, Size measureBounds,
            IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            if (siblingItems == null) throw new ArgumentNullException("siblingItems");
            if (dragItem == null) throw new ArgumentNullException("dragItem");

            var currentLocations = siblingItems
                .Select(GetLocationInfo)
                .Union(new[] { GetLocationInfo(dragItem) })
                .OrderBy(loc => loc.Item == dragItem ? loc.Start : GetStartOnDragStart(loc));

            var currentCoord = 0.0;
            var zIndex = int.MaxValue;
            foreach (var location in currentLocations)
            {
                if (!Equals(location.Item, dragItem))
                {
                    SendToLocation(location.Item, currentCoord);
                    Panel.SetZIndex(location.Item, --zIndex);
                }
                currentCoord += _getDesiredSize(location.Item) + _itemOffset;
            }
            Panel.SetZIndex(dragItem, int.MaxValue);
        }

        public virtual void OrganiseOnDragCompleted(DragablzItemsControl requestor, Size measureBounds,
            IEnumerable<DragablzItem> siblingItems, DragablzItem dragItem)
        {
            if (siblingItems == null) throw new ArgumentNullException("siblingItems");
            var currentLocations = siblingItems
                .Select(GetLocationInfo)
                .Union(new[] { GetLocationInfo(dragItem) })
                .OrderBy(loc => loc.Item == dragItem ? loc.Start : GetStartOnDragStart(loc));

            var currentCoord = 0.0;
            var z = int.MaxValue;
            var logicalIndex = 0;
            foreach (var location in currentLocations)
            {
                SetLocation(location.Item, currentCoord);
                currentCoord += _getDesiredSize(location.Item) + _itemOffset;
                Panel.SetZIndex(location.Item, --z);
                location.Item.LogicalIndex = logicalIndex++;
            }
            Panel.SetZIndex(dragItem, int.MaxValue);

            //NETworkManager: don't keep the items (and their window) of the last drag alive
            _siblingItemLocationOnDragStart = null;
        }

        public virtual Point ConstrainLocation(DragablzItemsControl requestor, Size measureBounds, Point itemCurrentLocation,
            Size itemCurrentSize, Point itemDesiredLocation, Size itemDesiredSize)
        {
            var fixedItems = requestor.FixedItemCount;
            var lowerBound = fixedItems == 0
                ? -1d
                : GetLocationInfo(requestor.DragablzItems()
                    .Take(fixedItems)
                    .Last()).End + _itemOffset - 1;

            return new Point(
                _orientation == Orientation.Vertical
                    ? 0
                    : Math.Min(Math.Max(lowerBound, itemDesiredLocation.X), (measureBounds.Width) + 1),
                _orientation == Orientation.Horizontal
                    ? 0
                    : Math.Min(Math.Max(lowerBound, itemDesiredLocation.Y), (measureBounds.Height) + 1)
                );
        }

        public virtual Size Measure(DragablzItemsControl requestor, Size availableSize, IEnumerable<DragablzItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            var size = new Size(double.PositiveInfinity, double.PositiveInfinity);

            double width = 0, height = 0;
            var isFirst = true;
            foreach (var dragablzItem in items)
            {
                dragablzItem.Measure(size);
                if (_orientation == Orientation.Horizontal)
                {
                    width += !dragablzItem.IsLoaded ? dragablzItem.DesiredSize.Width : dragablzItem.ActualWidth;
                    if (!isFirst)
                        width += _itemOffset;
                    height = Math.Max(height,
                        !dragablzItem.IsLoaded ? dragablzItem.DesiredSize.Height : dragablzItem.ActualHeight);
                }
                else
                {
                    width = Math.Max(width,
                        !dragablzItem.IsLoaded ? dragablzItem.DesiredSize.Width : dragablzItem.ActualWidth);
                    height += !dragablzItem.IsLoaded ? dragablzItem.DesiredSize.Height : dragablzItem.ActualHeight;
                    if (!isFirst)
                        height += _itemOffset;
                }

                isFirst = false;
            }

            return new Size(Math.Max(width, 0), Math.Max(height, 0));
        }

        public virtual IEnumerable<DragablzItem> Sort(IEnumerable<DragablzItem> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            return items.OrderBy(i => GetLocationInfo(i).Start);
        }

        private void SetLocation(DragablzItem dragablzItem, double location)
        {
            //NETworkManager: a slide to another location would end there later (e.g. a gap after a tab was dragged out)
            double activeTarget;
            if (_activeStoryboardTargetLocations.TryGetValue(dragablzItem, out activeTarget) &&
                Math.Abs(activeTarget - location) >= 1.0)
                StopSlide(dragablzItem);
            _setLocation(dragablzItem, location);
        }

        private void StopSlide(DragablzItem dragablzItem)
        {
            Storyboard storyboard;
            if (_activeStoryboards.TryGetValue(dragablzItem, out storyboard))
                storyboard.Remove(dragablzItem);
            _activeStoryboards.Remove(dragablzItem);
            _activeStoryboardTargetLocations.Remove(dragablzItem);
        }

        /// <summary>
        /// NETworkManager: called when the container of an item is removed (tab closed, dragged out or merged) - stops its
        /// slide and forgets the item, so the organiser doesn't keep it (and its window) alive.
        /// </summary>
        public virtual void ForgetItem(DragablzItem dragablzItem)
        {
            StopSlide(dragablzItem);
            if (_siblingItemLocationOnDragStart != null)
                _siblingItemLocationOnDragStart.Remove(dragablzItem);
        }

        /// <summary>
        /// NETworkManager: whether the items slide to their new location while a tab is dragged (checked each time an item
        /// moves). <c>null</c> = always. Set by the application (Appearance > Animations).
        /// </summary>
        public static Func<bool> AnimationsEnabled { get; set; }

        private void SendToLocation(DragablzItem dragablzItem, double location)
        {
            if (AnimationsEnabled != null && !AnimationsEnabled())
            {
                StopSlide(dragablzItem);
                _setLocation(dragablzItem, location);
                return;
            }

            //NETworkManager: compare with the target of a running slide (X / Y only change when it completes), otherwise
            //a drag back to the original location during the slide was ignored
            double activeTarget;
            var hasActiveTarget = _activeStoryboardTargetLocations.TryGetValue(dragablzItem, out activeTarget);
            if (Math.Abs((hasActiveTarget ? activeTarget : _getLocation(dragablzItem)) - location) < 1.0)
                return;

            _activeStoryboardTargetLocations[dragablzItem] = location;

            var storyboard = new Storyboard { FillBehavior = FillBehavior.Stop };
            _activeStoryboards[dragablzItem] = storyboard;
            storyboard.WhenComplete(sb =>
            {
                //NETworkManager: a storyboard replaced by a newer one (or stopped) must not set its location or remove
                //the newer target
                Storyboard activeStoryboard;
                if (!_activeStoryboards.TryGetValue(dragablzItem, out activeStoryboard) ||
                    !ReferenceEquals(activeStoryboard, sb))
                {
                    sb.Remove(dragablzItem);
                    return;
                }
                _setLocation(dragablzItem, location);
                sb.Remove(dragablzItem);
                _activeStoryboards.Remove(dragablzItem);
                _activeStoryboardTargetLocations.Remove(dragablzItem);
            });

            var timeline = new DoubleAnimationUsingKeyFrames();
            timeline.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(_canvasDependencyProperty));
            timeline.KeyFrames.Add(
                new EasingDoubleKeyFrame(location, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)))
                {
                    //NETworkManager: quadratic ease-out like the other animations of the application
                    EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
                });
            storyboard.Children.Add(timeline);
            storyboard.Begin(dragablzItem, true);
        }

        private LocationInfo GetLocationInfo(DragablzItem item)
        {
            var size = _getDesiredSize(item);
            double startLocation;
            if (!_activeStoryboardTargetLocations.TryGetValue(item, out startLocation))
                startLocation = _getLocation(item);
            var midLocation = startLocation + size / 2;
            var endLocation = startLocation + size;

            return new LocationInfo(item, startLocation, midLocation, endLocation);
        }
    }
}
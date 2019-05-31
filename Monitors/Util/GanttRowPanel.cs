using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Rzdmonitors.Util
{
    public class GanttRowPanel : Panel
    {
        public static readonly DependencyProperty StartDateProperty =
           DependencyProperty.RegisterAttached("StartDate", typeof(DateTime), typeof(GanttRowPanel), new FrameworkPropertyMetadata(DateTime.MinValue, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public static readonly DependencyProperty EndDateProperty =
            DependencyProperty.RegisterAttached("EndDate", typeof(DateTime?), typeof(GanttRowPanel), new FrameworkPropertyMetadata(DateTime.MaxValue, FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static readonly DependencyProperty MaxDateProperty =
           DependencyProperty.Register("MaxDate", typeof(DateTime), typeof(GanttRowPanel), new FrameworkPropertyMetadata(DateTime.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty MinDateProperty =
            DependencyProperty.Register("MinDate", typeof(DateTime), typeof(GanttRowPanel), new FrameworkPropertyMetadata(DateTime.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure));


        public static DateTime GetStartDate(DependencyObject obj)
        {
            return (DateTime)obj.GetValue(StartDateProperty);
        }

        public static void SetStartDate(DependencyObject obj, DateTime value)
        {
            obj.SetValue(StartDateProperty, value);
        }

        public static DateTime? GetEndDate(DependencyObject obj)
        {
            return (DateTime?)obj.GetValue(EndDateProperty);
        }

        public static void SetEndDate(DependencyObject obj, DateTime? value)
        {
            obj.SetValue(EndDateProperty, value);
        }

        public DateTime MaxDate
        {
            get => (DateTime)GetValue(MaxDateProperty);
            set => SetValue(MaxDateProperty, value);
        }

        public DateTime MinDate
        {
            get => (DateTime)GetValue(MinDateProperty);
            set => SetValue(MinDateProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
            }

            return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double range = (MaxDate - MinDate).Ticks;
            double pixelsPerTick = finalSize.Width / range;

            foreach (UIElement child in Children)
            {
                ArrangeChild(child, MinDate, MaxDate, pixelsPerTick, finalSize.Height);
            }

            return finalSize;
        }

        private void ArrangeChild(UIElement child, DateTime minDate, DateTime maxDate, double pixelsPerTick, double elementHeight)
        {
            double width;

            var childStartDate = GetStartDate(child);
            var childFixedStartDate = GetStartDate(child);
            if (childFixedStartDate < minDate)
                childFixedStartDate = minDate;

            var childEndDate = GetEndDate(child);

            if (childEndDate == null)
            {
                var maxDuration = maxDate - childFixedStartDate;
                var maxWidth = maxDuration.Ticks * pixelsPerTick;

                width = child.DesiredSize.Width <= maxWidth ? child.DesiredSize.Width : maxWidth;

                if (childStartDate < minDate)
                {
                    width -= (minDate.Ticks - childStartDate.Ticks) * pixelsPerTick;
                }
            }
            else
            {
                if (childEndDate > maxDate)
                    childEndDate = maxDate;

                var childDuration = childEndDate.Value - childFixedStartDate;

                width = childDuration.Ticks * pixelsPerTick;
            }
            if (width < 1)
                width = 1;

            var offset = (childFixedStartDate - minDate).Ticks * pixelsPerTick;

            child.Arrange(new Rect(offset, 0, width, elementHeight));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ruler.Wpf.Models
{
    public class RulerDataTemplateSelector : DataTemplateSelector
    {
        // These properties will be set in the XAML.
        // They hold the references to the two DataTemplates we defined.
        public DataTemplate LineTemplate { get; set; }
        public DataTemplate LabelTemplate { get; set; }

        // This method is called by the ItemsControl for each item in the collection.
        // It's where the logic for selecting the template lives.
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // We check the type of the 'item' object.
            // If it's a LineInfo object, we return the LineTemplate.
            if (item is LineInfo)
            {
                return LineTemplate;
            }
            // If it's a LabelInfo object, we return the LabelTemplate.
            else if (item is LabelInfo)
            {
                return LabelTemplate;
            }
            // If it's neither, we fall back to the base implementation.
            return base.SelectTemplate(item, container);
        }
    }
}

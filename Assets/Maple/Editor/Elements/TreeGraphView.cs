using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Maple.Editor
{
    public class TreeGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<TreeGraphView, UxmlTraits> { }

        public static TreeGraphView Instance { get; private set; }

        public TreeGraphView()
        {
            // Stretch to parent window's size
            this.StretchToParentSize();

            // Add content manipulators for graph interaction
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            // Set up default zooming functionality
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Create and add grid to graphview
            var background = new GridBackground();
            background.StretchToParentSize();
            Insert(0, background);

            // Apply grid styling for grid background
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/GraphView"));
        }
    }
}

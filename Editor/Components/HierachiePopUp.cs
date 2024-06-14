using Editor;
using GeneralGame;
using Sandbox;

namespace GeneralGame;

public class HierarchyEditorPopup : PopupWidget
{
    public SerializedProperty Property { get; private set; }
    private readonly HierarchyEditor _editor;

    public HierarchyEditorPopup(Widget parent, SerializedProperty property) : base(parent)
    {
        Property = property;
        MinimumSize = new Vector2(375, 480);

        _editor = new HierarchyEditor(this);
        _editor.Size = MinimumSize;
        _editor.MinimumSize = _editor.Size;

        Layout = Layout.Column();
        Layout.Margin = 8;
        Layout.Add(_editor);
    }
}
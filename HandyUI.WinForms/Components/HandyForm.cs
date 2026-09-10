using System.ComponentModel;

namespace HandyUI.WinForms.Components;

public class HandyForm : Form
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Size MinimumClientSize
    {
        get => MinimumSize - SizeFromClientSize(Size.Empty);
        set => MinimumSize = SizeFromClientSize(value);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Size MaximumClientSize
    {
        get => MaximumSize - SizeFromClientSize(Size.Empty);
        set => MaximumSize = SizeFromClientSize(value);
    }
}
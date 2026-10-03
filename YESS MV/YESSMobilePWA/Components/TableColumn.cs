using Microsoft.AspNetCore.Components;

namespace YESSMobilePWA.Components
{
    /// <summary>
    /// Define una columna en GenericTable.
    /// Permite header, property name, y template personalizado opcional.
    /// </summary>
    public class TableColumn<TItem>
    {
        public string Header { get; set; } = "";
        public string PropertyName { get; set; } = "";
        public RenderFragment<TItem>? Template { get; set; }

        public TableColumn() { }

        public TableColumn(string header, string propertyName, RenderFragment<TItem>? template = null)
        {
            Header = header;
            PropertyName = propertyName;
            Template = template;
        }
    }
}
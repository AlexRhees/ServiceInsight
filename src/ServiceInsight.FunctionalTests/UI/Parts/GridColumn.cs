namespace Particular.ServiceInsight.FunctionalTests.UI.Parts
{
    public class GridColumn
    {
        public int Index { get; private set; }
        public string Name { get; private set; }

        public GridColumn(int index, string name)
        {
            Index = index;
            Name = name;
        }
    }
}
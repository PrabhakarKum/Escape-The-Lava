namespace FOG.EscapeTheLava
{
    public readonly struct GridCoordinate
    {
        public GridCoordinate(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public int Column { get; }
        public int Row { get; }

        public override string ToString()
        {
            return $"({Column}, {Row})";
        }
    }
}

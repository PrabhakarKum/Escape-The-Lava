using UnityEditor;
using UnityEngine;

namespace FOG.EscapeTheLava.EditorTools
{
    /// <summary>
    /// Lets a level be painted directly in the Inspector instead of authored as a
    /// flat serialized array or hardcoded ASCII-art string in code. Pick a brush
    /// (Island/Lava/Diamond) and click-drag across the grid to paint tiles.
    /// </summary>
    [CustomEditor(typeof(LevelDefinition))]
    public sealed class LevelDefinitionEditor : Editor
    {
        private const float CellSize = 22f;
        private const float CellSpacing = 2f;

        private static readonly Color IslandColor = new(0.42f, 0.78f, 0.32f);
        private static readonly Color LavaColor = new(0.86f, 0.24f, 0.14f);
        private static readonly Color DiamondColor = new(0.32f, 0.62f, 0.92f);
        private static readonly string[] BrushLabels = { "Island", "Lava", "Diamond" };

        private SerializedProperty columnsProperty;
        private SerializedProperty rowsProperty;
        private SerializedProperty tilesProperty;
        private TileType brush = TileType.Lava;

        private void OnEnable()
        {
            columnsProperty = serializedObject.FindProperty("columns");
            rowsProperty = serializedObject.FindProperty("rows");
            tilesProperty = serializedObject.FindProperty("tiles");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(columnsProperty);
            EditorGUILayout.PropertyField(rowsProperty);

            int columns = Mathf.Max(1, columnsProperty.intValue);
            int rows = Mathf.Max(1, rowsProperty.intValue);
            int expectedTileCount = columns * rows;

            if (tilesProperty.arraySize != expectedTileCount)
            {
                EditorGUILayout.HelpBox(
                    $"Tile data has {tilesProperty.arraySize} entries but {columns} x {rows} needs {expectedTileCount}.",
                    MessageType.Warning);

                if (GUILayout.Button($"Resize To {expectedTileCount} Tiles"))
                {
                    tilesProperty.arraySize = expectedTileCount;
                }

                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);
            brush = (TileType)GUILayout.Toolbar((int)brush, BrushLabels);

            EditorGUILayout.Space();
            DrawGrid(columns, rows);
            DrawSummary();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill With Brush"))
            {
                FillAll(brush);
            }
            if (GUILayout.Button("Clear To Islands"))
            {
                FillAll(TileType.Island);
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGrid(int columns, int rows)
        {
            Rect gridRect = GUILayoutUtility.GetRect(
                columns * (CellSize + CellSpacing),
                rows * (CellSize + CellSpacing));

            Event current = Event.current;
            bool isPaintEvent = current.type is EventType.MouseDown or EventType.MouseDrag && current.button == 0;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int index = row * columns + column;
                    Rect cellRect = new(
                        gridRect.x + column * (CellSize + CellSpacing),
                        gridRect.y + row * (CellSize + CellSpacing),
                        CellSize,
                        CellSize);

                    SerializedProperty tileProperty = tilesProperty.GetArrayElementAtIndex(index);
                    TileType currentType = (TileType)tileProperty.enumValueIndex;

                    EditorGUI.DrawRect(cellRect, ColorFor(currentType));

                    if (isPaintEvent && cellRect.Contains(current.mousePosition))
                    {
                        if (currentType != brush)
                        {
                            tileProperty.enumValueIndex = (int)brush;
                        }

                        current.Use();
                        Repaint();
                    }
                }
            }
        }

        private void DrawSummary()
        {
            int diamonds = 0;
            int lava = 0;
            for (int i = 0; i < tilesProperty.arraySize; i++)
            {
                TileType type = (TileType)tilesProperty.GetArrayElementAtIndex(i).enumValueIndex;
                if (type == TileType.Diamond) diamonds++;
                else if (type == TileType.Lava) lava++;
            }

            EditorGUILayout.LabelField($"Diamonds: {diamonds}    Lava: {lava}    Tiles: {tilesProperty.arraySize}");

            if (diamonds == 0)
            {
                EditorGUILayout.HelpBox("No diamonds placed — this level can never be won.", MessageType.Warning);
            }
        }

        private void FillAll(TileType type)
        {
            for (int i = 0; i < tilesProperty.arraySize; i++)
            {
                tilesProperty.GetArrayElementAtIndex(i).enumValueIndex = (int)type;
            }
        }

        private static Color ColorFor(TileType type)
        {
            return type switch
            {
                TileType.Lava => LavaColor,
                TileType.Diamond => DiamondColor,
                _ => IslandColor
            };
        }
    }
}

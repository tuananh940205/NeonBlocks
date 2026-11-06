using UnityEngine;
namespace NeonBlocks.Views
{
    /// <summary>Sprite dependency của view, thứ tự I/O/T/J/L/S/Z dùng chung cho board và preview.</summary>
    public sealed class CellTheme : ScriptableObject
    {
        [SerializeField] private Sprite[] cells = new Sprite[7];
        [SerializeField] private Sprite ghost;
        public Sprite Ghost => ghost;
        /// <summary>Chọn sprite theo mã khối, không Resources hoặc tải asset trong render.</summary>
        public Sprite GetCell(string id)
        {
            int index = id switch { "I" => 0, "O" => 1, "T" => 2, "J" => 3, "L" => 4, "S" => 5, "Z" => 6, _ => -1 };
            return index >= 0 && index < cells.Length ? cells[index] : null;
        }
    }
}
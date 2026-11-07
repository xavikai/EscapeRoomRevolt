namespace EscapeRoomRevolt.Systems.Equipment
{
    /// <summary>Optional callbacks for behaviours hosted by an equippable prefab.</summary>
    public interface IEquipmentLifecycle
    {
        void OnEquipped();
        void OnUnequipped();
    }
}

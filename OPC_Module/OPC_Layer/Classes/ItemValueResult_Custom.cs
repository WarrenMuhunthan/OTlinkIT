using Opc.Da;

namespace WebApiMVC.OPCLayer.Class
{
    internal class ItemValueResult_Custom : ItemValueResult
    {
        public ItemValueResult_Custom(string ItemName)
        {
            
            this.ItemName = ItemName;
            
            
        }

    }
}
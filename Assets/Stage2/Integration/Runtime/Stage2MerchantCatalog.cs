using System;
using UnityEngine;

namespace Not3A.Stage2.Integration
{
    /// <summary>Trial shop prices and finite initial stock; no scroll source or restock timer.</summary>
    [CreateAssetMenu(menuName = "Not 3A/Stage 3/Merchant Catalog")]
    public sealed class Stage2MerchantCatalog : ScriptableObject
    {
        [SerializeField] private int[] baseGold = {20,24,16,32,16,24,20,20};
        [SerializeField] private int[] qualityMultiplier = {1,2,4,8,16,32,64,128};
        [SerializeField] private int levelGrowthPercent = 2;
        [SerializeField] private int salePercent = 50;
        [SerializeField] private float interactionRadius = 3.5f;
        [SerializeField] private int[] potionPrices={100,100,250};
        [SerializeField] private int potionStock=5;
        [SerializeField] private int[] accessoryBaseGold={50,40,60};
        public int PotionStock=>potionStock;
        public float InteractionRadius => interactionRadius;
        public int BuyPrice(Stage2EquipmentItem item) => Price(item.Type,item.Quality,item.Level);
        public int SellPrice(Stage2EquipmentItem item) => (int)((long)BuyPrice(item)*salePercent/100);
        public int Price(Stage2ItemType type,Stage2ItemQuality quality,int level)
        {
            if(!Stage2EquipmentCatalog.ValidType(type)||!Stage2EquipmentCatalog.ValidQuality(quality)||level<1||level>100)
                throw new ArgumentOutOfRangeException(nameof(type));
            if(Stage2EquipmentCatalog.IsConsumable(type))return potionPrices[(int)type-11];
            var basis=(int)type<baseGold.Length?baseGold[(int)type]:accessoryBaseGold[(int)type-8];
            var price=(long)basis*qualityMultiplier[(int)quality]*(100L+(level-1L)*levelGrowthPercent)/100L;
            return (int)Math.Min(int.MaxValue,price);
        }
        public bool TryValidate(out string reason)
        {
            reason="Merchant requires8 positive type prices,8 ordered quality multipliers,50% sale and a finite local radius.";
            if(baseGold==null||baseGold.Length!=8||qualityMultiplier==null||qualityMultiplier.Length!=8||
                levelGrowthPercent<0||levelGrowthPercent>100||salePercent!=50||!float.IsFinite(interactionRadius)||interactionRadius<=0||
                potionPrices==null||potionPrices.Length!=3||potionStock<1||accessoryBaseGold==null||accessoryBaseGold.Length!=3)return false;
            foreach(var value in potionPrices)if(value<2)return false;
            foreach(var value in accessoryBaseGold)if(value<2)return false;
            foreach(var value in baseGold)if(value<2||value>1000000)return false;
            var previous=0;foreach(var value in qualityMultiplier)
            {if(value<1||value<previous||value>10000)return false;previous=value;}
            reason=null;return true;
        }
    }
}

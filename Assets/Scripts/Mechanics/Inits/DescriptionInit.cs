using System.Collections;
using System.Collections.Generic;
using CSV.Parsers;

public static class DescriptionInitExtensions {
    public static string DescriptionInit(this double desc, string md) {
        switch(md) {
            case nameof(CSVMd.Cooldown):

                return desc.ToString() + " s";
            
            case nameof(CSVMd.Health):
            case nameof(CSVMd.HealAmount):
            case nameof(CSVMd.CritChance):
            case nameof(CSVMd.Damage):
            case nameof(CSVMd.SentryDamage):
            case nameof(CSVMd.MaxHP):
                    
                return (desc * 100d).ToString() + " %";

            default:
                return desc.ToString();
        }
    }
}
